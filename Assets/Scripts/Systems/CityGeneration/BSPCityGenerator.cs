using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace DominantK.Systems.CityGeneration
{
    /// <summary>
    /// BSP（Binary Space Partitioning）ベースの街生成アルゴリズム
    /// 整然とした区画とL字路・T字路を生成
    /// </summary>
    public class BSPCityGenerator : ICityGenerator
    {
        private System.Random random;
        private CityGenerationSettings settings;

        public CityData Generate(CityGenerationSettings settings)
        {
            this.settings = settings;
            random = new System.Random(settings.Seed);
            var cityData = new CityData(settings.Width, settings.Height);

            // 1. BSP木を構築
            var rootNode = new BSPNode(0, 0, settings.Width, settings.Height);
            SplitNode(rootNode, 0, settings.BspMaxDepth);

            // 2. 葉ノードから地区を生成
            var leaves = new List<BSPNode>();
            CollectLeaves(rootNode, leaves);

            // 3. 各地区に部屋（建物エリア）を生成
            foreach (var leaf in leaves)
            {
                CreateDistrictFromNode(cityData, leaf);
            }

            // 4. BSP木を辿って隣接ノード間に道路を生成
            GenerateRoadsFromBSP(cityData, rootNode);

            // 5. 地区タイプを割り当て
            AssignDistrictTypes(cityData);

            // 6. 建物スロットを生成
            GenerateBuildingSlots(cityData);

            return cityData;
        }

        private void SplitNode(BSPNode node, int depth, int maxDepth)
        {
            if (depth >= maxDepth) return;

            // 分割可能かチェック
            bool canSplitH = node.Height >= settings.BspMinRoomSize * 2;
            bool canSplitV = node.Width >= settings.BspMinRoomSize * 2;

            if (!canSplitH && !canSplitV) return;

            // 分割方向を決定
            bool splitHorizontally;
            if (!canSplitH) splitHorizontally = false;
            else if (!canSplitV) splitHorizontally = true;
            else splitHorizontally = random.NextDouble() < 0.5;

            if (splitHorizontally)
            {
                int minY = node.Y + settings.BspMinRoomSize;
                int maxY = node.Y + node.Height - settings.BspMinRoomSize;
                if (minY >= maxY) return;

                int splitY = random.Next(minY, maxY);

                node.Left = new BSPNode(node.X, node.Y, node.Width, splitY - node.Y);
                node.Right = new BSPNode(node.X, splitY, node.Width, node.Y + node.Height - splitY);
            }
            else
            {
                int minX = node.X + settings.BspMinRoomSize;
                int maxX = node.X + node.Width - settings.BspMinRoomSize;
                if (minX >= maxX) return;

                int splitX = random.Next(minX, maxX);

                node.Left = new BSPNode(node.X, node.Y, splitX - node.X, node.Height);
                node.Right = new BSPNode(splitX, node.Y, node.X + node.Width - splitX, node.Height);
            }

            // 再帰的に分割
            SplitNode(node.Left, depth + 1, maxDepth);
            SplitNode(node.Right, depth + 1, maxDepth);
        }

        private void CollectLeaves(BSPNode node, List<BSPNode> leaves)
        {
            if (node == null) return;

            if (node.Left == null && node.Right == null)
            {
                leaves.Add(node);
            }
            else
            {
                CollectLeaves(node.Left, leaves);
                CollectLeaves(node.Right, leaves);
            }
        }

        private void CreateDistrictFromNode(CityData cityData, BSPNode node)
        {
            // ノードの境界から少し内側に部屋を作成
            int margin = 2;
            int roomX = node.X + margin;
            int roomY = node.Y + margin;
            int roomW = node.Width - margin * 2;
            int roomH = node.Height - margin * 2;

            if (roomW <= 0 || roomH <= 0) return;

            var district = new District(cityData.Districts.Count);
            district.Center = new float2(roomX + roomW / 2f, roomY + roomH / 2f);

            // 部屋の境界を道路でマーク
            for (int x = roomX - 1; x <= roomX + roomW; x++)
            {
                for (int y = roomY - 1; y <= roomY + roomH; y++)
                {
                    if (x < 0 || x >= cityData.Width || y < 0 || y >= cityData.Height)
                        continue;

                    bool isBorder = x == roomX - 1 || x == roomX + roomW ||
                                   y == roomY - 1 || y == roomY + roomH;

                    if (isBorder)
                    {
                        cityData.Cells[x, y] = CellType.Road;
                    }
                    else
                    {
                        district.Cells.Add(new int2(x, y));
                    }
                }
            }

            district.Area = district.Cells.Count;
            cityData.Districts.Add(district);
        }

        private void GenerateRoadsFromBSP(CityData cityData, BSPNode node)
        {
            if (node.Left == null || node.Right == null) return;

            // 左右（または上下）の子ノードを接続する道路を生成
            ConnectNodes(cityData, node.Left, node.Right);

            // 再帰的に処理
            GenerateRoadsFromBSP(cityData, node.Left);
            GenerateRoadsFromBSP(cityData, node.Right);
        }

        private void ConnectNodes(CityData cityData, BSPNode left, BSPNode right)
        {
            // 両ノードの中心を接続
            float2 leftCenter = GetNodeCenter(left);
            float2 rightCenter = GetNodeCenter(right);

            var road = new Road();
            road.Width = 2f;
            road.IsMainRoad = true;

            // L字路またはT字路を形成
            bool horizontalFirst = random.NextDouble() < 0.5;

            if (horizontalFirst)
            {
                float2 corner = new float2(rightCenter.x, leftCenter.y);
                road.Points.Add(leftCenter);
                road.Points.Add(corner);
                road.Points.Add(rightCenter);
            }
            else
            {
                float2 corner = new float2(leftCenter.x, rightCenter.y);
                road.Points.Add(leftCenter);
                road.Points.Add(corner);
                road.Points.Add(rightCenter);
            }

            cityData.Roads.Add(road);
            MarkRoadCells(cityData, road);
        }

        private float2 GetNodeCenter(BSPNode node)
        {
            if (node.Left == null && node.Right == null)
            {
                return new float2(node.X + node.Width / 2f, node.Y + node.Height / 2f);
            }

            // 葉ノードの中心を取得
            var leaves = new List<BSPNode>();
            CollectLeaves(node, leaves);

            float2 center = float2.zero;
            foreach (var leaf in leaves)
            {
                center += new float2(leaf.X + leaf.Width / 2f, leaf.Y + leaf.Height / 2f);
            }
            return center / leaves.Count;
        }

        private void MarkRoadCells(CityData cityData, Road road)
        {
            for (int i = 0; i < road.Points.Count - 1; i++)
            {
                float2 start = road.Points[i];
                float2 end = road.Points[i + 1];
                float length = math.distance(start, end);
                int steps = (int)(length * 2) + 1;

                for (int s = 0; s <= steps; s++)
                {
                    float t = s / (float)steps;
                    float2 point = math.lerp(start, end, t);

                    int halfWidth = (int)(road.Width / 2f);
                    for (int dx = -halfWidth; dx <= halfWidth; dx++)
                    {
                        for (int dy = -halfWidth; dy <= halfWidth; dy++)
                        {
                            int x = (int)point.x + dx;
                            int y = (int)point.y + dy;
                            if (x >= 0 && x < cityData.Width && y >= 0 && y < cityData.Height)
                            {
                                cityData.Cells[x, y] = CellType.Road;
                            }
                        }
                    }
                }
            }
        }

        private void AssignDistrictTypes(CityData cityData)
        {
            float2 cityCenter = new float2(cityData.Width / 2f, cityData.Height / 2f);

            foreach (var district in cityData.Districts)
            {
                float distFromCenter = math.distance(district.Center, cityCenter);
                float maxDist = math.sqrt(cityData.Width * cityData.Width + cityData.Height * cityData.Height) / 2f;
                float normalizedDist = distFromCenter / maxDist;

                // 距離に基づいてゾーニング
                if (normalizedDist < 0.3f)
                {
                    district.Type = CellType.Commercial;
                }
                else if (normalizedDist < 0.6f)
                {
                    district.Type = random.NextDouble() < 0.6 ? CellType.Residential : CellType.Commercial;
                }
                else
                {
                    district.Type = random.NextDouble() < 0.7 ? CellType.Residential : CellType.Industrial;
                }

                // セルにタイプを適用
                foreach (var cell in district.Cells)
                {
                    if (cityData.Cells[cell.x, cell.y] != CellType.Road)
                    {
                        cityData.Cells[cell.x, cell.y] = district.Type;
                    }
                }
            }
        }

        private void GenerateBuildingSlots(CityData cityData)
        {
            foreach (var district in cityData.Districts)
            {
                // 各地区内にグリッド状に建物を配置
                int spacing = 4;
                int2 min = new int2(int.MaxValue, int.MaxValue);
                int2 max = new int2(int.MinValue, int.MinValue);

                foreach (var cell in district.Cells)
                {
                    min = math.min(min, cell);
                    max = math.max(max, cell);
                }

                for (int x = min.x + 1; x < max.x - 1; x += spacing)
                {
                    for (int y = min.y + 1; y < max.y - 1; y += spacing)
                    {
                        if (cityData.Cells[x, y] != CellType.Road)
                        {
                            cityData.BuildingSlots.Add(new int2(x, y));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// BSPノード
        /// </summary>
        private class BSPNode
        {
            public int X, Y, Width, Height;
            public BSPNode Left, Right;

            public BSPNode(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
        }
    }
}
