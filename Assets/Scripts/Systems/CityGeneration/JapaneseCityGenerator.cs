using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace DominantK.Systems.CityGeneration
{
    /// <summary>
    /// 日本の街並みベースの街生成アルゴリズム
    /// 特徴：
    /// - 碁盤の目状の基本構造（京都・札幌スタイル）
    /// - 主要道路と生活道路の階層
    /// - 駅前商店街
    /// - 住宅地の入り組んだ路地
    /// - 大通り沿いの商業施設
    /// </summary>
    public class JapaneseCityGenerator : ICityGenerator
    {
        private System.Random random;
        private CityGenerationSettings settings;

        public CityData Generate(CityGenerationSettings settings)
        {
            this.settings = settings;
            random = new System.Random(settings.Seed);
            var cityData = new CityData(settings.Width, settings.Height);

            // 1. メイン道路グリッドを生成（大通り）
            GenerateMainRoadGrid(cityData);

            // 2. 駅と商店街を配置
            PlaceStation(cityData);

            // 3. ブロック内に生活道路（細い路地）を生成
            GenerateAlleyways(cityData);

            // 4. ゾーニング（商業・住宅・工業）
            AssignZoning(cityData);

            // 5. 公園・神社を配置
            PlaceParksAndShrines(cityData);

            // 6. 建物スロットを生成
            GenerateBuildingSlots(cityData);

            return cityData;
        }

        private void GenerateMainRoadGrid(CityData cityData)
        {
            int blockWidth = cityData.Width / settings.BlockCountX;
            int blockHeight = cityData.Height / settings.BlockCountY;

            // 縦の大通り
            for (int i = 0; i <= settings.BlockCountX; i++)
            {
                int x = i * blockWidth;
                if (x >= cityData.Width) x = cityData.Width - 1;

                bool isMainStreet = (i == settings.BlockCountX / 2) || (i == 0) || (i == settings.BlockCountX);
                float roadWidth = isMainStreet ? settings.MainRoadWidth : settings.SubRoadWidth;

                var road = new Road();
                road.Width = roadWidth;
                road.IsMainRoad = isMainStreet;
                road.Points.Add(new float2(x, 0));
                road.Points.Add(new float2(x, cityData.Height - 1));
                cityData.Roads.Add(road);

                MarkRoadLine(cityData, x, 0, x, cityData.Height - 1, roadWidth);
            }

            // 横の大通り
            for (int j = 0; j <= settings.BlockCountY; j++)
            {
                int y = j * blockHeight;
                if (y >= cityData.Height) y = cityData.Height - 1;

                bool isMainStreet = (j == settings.BlockCountY / 2) || (j == 0) || (j == settings.BlockCountY);
                float roadWidth = isMainStreet ? settings.MainRoadWidth : settings.SubRoadWidth;

                var road = new Road();
                road.Width = roadWidth;
                road.IsMainRoad = isMainStreet;
                road.Points.Add(new float2(0, y));
                road.Points.Add(new float2(cityData.Width - 1, y));
                cityData.Roads.Add(road);

                MarkRoadLine(cityData, 0, y, cityData.Width - 1, y, roadWidth);
            }
        }

        private void PlaceStation(CityData cityData)
        {
            // 街の中心に駅を配置
            int stationX = cityData.Width / 2;
            int stationY = cityData.Height / 2;

            // 駅周辺を商業地区としてマーク
            int stationSize = 8;
            var stationDistrict = new District(cityData.Districts.Count);
            stationDistrict.Type = CellType.Commercial;
            stationDistrict.Center = new float2(stationX, stationY);

            for (int x = stationX - stationSize; x <= stationX + stationSize; x++)
            {
                for (int y = stationY - stationSize; y <= stationY + stationSize; y++)
                {
                    if (x >= 0 && x < cityData.Width && y >= 0 && y < cityData.Height)
                    {
                        stationDistrict.Cells.Add(new int2(x, y));
                    }
                }
            }

            stationDistrict.Area = stationDistrict.Cells.Count;
            cityData.Districts.Add(stationDistrict);

            // 駅前ロータリー（円形の道路）
            GenerateRotary(cityData, stationX, stationY, 5);

            // 商店街を4方向に伸ばす
            GenerateShoppingStreet(cityData, stationX, stationY, 1, 0, 20);  // 東
            GenerateShoppingStreet(cityData, stationX, stationY, -1, 0, 20); // 西
            GenerateShoppingStreet(cityData, stationX, stationY, 0, 1, 15);  // 北
            GenerateShoppingStreet(cityData, stationX, stationY, 0, -1, 15); // 南
        }

        private void GenerateRotary(CityData cityData, int centerX, int centerY, int radius)
        {
            for (float angle = 0; angle < math.PI * 2; angle += 0.1f)
            {
                int x = centerX + (int)(math.cos(angle) * radius);
                int y = centerY + (int)(math.sin(angle) * radius);

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && nx < cityData.Width && ny >= 0 && ny < cityData.Height)
                        {
                            cityData.Cells[nx, ny] = CellType.Road;
                        }
                    }
                }
            }
        }

        private void GenerateShoppingStreet(CityData cityData, int startX, int startY, int dirX, int dirY, int length)
        {
            var road = new Road();
            road.Width = 2f;
            road.IsMainRoad = true;

            for (int i = 0; i < length; i++)
            {
                int x = startX + dirX * i;
                int y = startY + dirY * i;

                if (x < 0 || x >= cityData.Width || y < 0 || y >= cityData.Height)
                    break;

                road.Points.Add(new float2(x, y));

                // アーケード風に少し広めの道
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && nx < cityData.Width && ny >= 0 && ny < cityData.Height)
                        {
                            cityData.Cells[nx, ny] = CellType.Road;
                        }
                    }
                }
            }

            cityData.Roads.Add(road);
        }

        private void GenerateAlleyways(CityData cityData)
        {
            int blockWidth = cityData.Width / settings.BlockCountX;
            int blockHeight = cityData.Height / settings.BlockCountY;

            // 各ブロック内に路地を生成
            for (int bx = 0; bx < settings.BlockCountX; bx++)
            {
                for (int by = 0; by < settings.BlockCountY; by++)
                {
                    int startX = bx * blockWidth + 3;
                    int startY = by * blockHeight + 3;
                    int endX = (bx + 1) * blockWidth - 3;
                    int endY = (by + 1) * blockHeight - 3;

                    if (endX <= startX || endY <= startY) continue;

                    // ブロック内にランダムに路地を生成
                    int alleyCount = random.Next(1, 4);

                    for (int a = 0; a < alleyCount; a++)
                    {
                        bool horizontal = random.NextDouble() < 0.5;

                        if (horizontal)
                        {
                            int y = random.Next(startY, endY);
                            MarkRoadLine(cityData, startX, y, endX, y, 1f);

                            var road = new Road();
                            road.Width = 1f;
                            road.Points.Add(new float2(startX, y));
                            road.Points.Add(new float2(endX, y));
                            cityData.Roads.Add(road);
                        }
                        else
                        {
                            int x = random.Next(startX, endX);
                            MarkRoadLine(cityData, x, startY, x, endY, 1f);

                            var road = new Road();
                            road.Width = 1f;
                            road.Points.Add(new float2(x, startY));
                            road.Points.Add(new float2(x, endY));
                            cityData.Roads.Add(road);
                        }
                    }

                    // 袋小路を追加（日本の住宅街の特徴）
                    if (random.NextDouble() < 0.3)
                    {
                        int deadEndX = random.Next(startX, endX);
                        int deadEndY = random.Next(startY, endY);
                        int length = random.Next(3, 8);

                        int direction = random.Next(4);
                        int dx = direction == 0 ? 1 : direction == 1 ? -1 : 0;
                        int dy = direction == 2 ? 1 : direction == 3 ? -1 : 0;

                        for (int i = 0; i < length; i++)
                        {
                            int x = deadEndX + dx * i;
                            int y = deadEndY + dy * i;
                            if (x >= startX && x < endX && y >= startY && y < endY)
                            {
                                cityData.Cells[x, y] = CellType.Road;
                            }
                        }
                    }
                }
            }
        }

        private void AssignZoning(CityData cityData)
        {
            int blockWidth = cityData.Width / settings.BlockCountX;
            int blockHeight = cityData.Height / settings.BlockCountY;
            float2 cityCenter = new float2(cityData.Width / 2f, cityData.Height / 2f);

            for (int bx = 0; bx < settings.BlockCountX; bx++)
            {
                for (int by = 0; by < settings.BlockCountY; by++)
                {
                    int startX = bx * blockWidth;
                    int startY = by * blockHeight;
                    int endX = (bx + 1) * blockWidth;
                    int endY = (by + 1) * blockHeight;

                    float2 blockCenter = new float2((startX + endX) / 2f, (startY + endY) / 2f);
                    float distFromCenter = math.distance(blockCenter, cityCenter);
                    float maxDist = math.sqrt(cityData.Width * cityData.Width + cityData.Height * cityData.Height) / 2f;
                    float normalizedDist = distFromCenter / maxDist;

                    // 日本風ゾーニング
                    CellType zoneType;
                    if (normalizedDist < 0.2f)
                    {
                        // 駅前：商業
                        zoneType = CellType.Commercial;
                    }
                    else if (IsFacingMainRoad(bx, by))
                    {
                        // 大通り沿い：商業（ロードサイド店舗）
                        zoneType = random.NextDouble() < 0.7 ? CellType.Commercial : CellType.Residential;
                    }
                    else if (normalizedDist > 0.7f && IsCorner(bx, by))
                    {
                        // 外縁部の角：工業
                        zoneType = CellType.Industrial;
                    }
                    else
                    {
                        // その他：住宅
                        zoneType = CellType.Residential;
                    }

                    var district = new District(cityData.Districts.Count);
                    district.Type = zoneType;
                    district.Center = blockCenter;

                    for (int x = startX; x < endX && x < cityData.Width; x++)
                    {
                        for (int y = startY; y < endY && y < cityData.Height; y++)
                        {
                            if (cityData.Cells[x, y] != CellType.Road)
                            {
                                cityData.Cells[x, y] = zoneType;
                                district.Cells.Add(new int2(x, y));
                            }
                        }
                    }

                    district.Area = district.Cells.Count;
                    cityData.Districts.Add(district);
                }
            }
        }

        private bool IsFacingMainRoad(int bx, int by)
        {
            int centerBlockX = settings.BlockCountX / 2;
            int centerBlockY = settings.BlockCountY / 2;

            return bx == centerBlockX || by == centerBlockY ||
                   bx == 0 || bx == settings.BlockCountX - 1 ||
                   by == 0 || by == settings.BlockCountY - 1;
        }

        private bool IsCorner(int bx, int by)
        {
            bool isXEdge = bx == 0 || bx == settings.BlockCountX - 1;
            bool isYEdge = by == 0 || by == settings.BlockCountY - 1;
            return isXEdge && isYEdge;
        }

        private void PlaceParksAndShrines(CityData cityData)
        {
            int parkCount = random.Next(2, 5);

            for (int i = 0; i < parkCount; i++)
            {
                int parkX = random.Next(10, cityData.Width - 10);
                int parkY = random.Next(10, cityData.Height - 10);
                int parkSize = random.Next(5, 10);

                var parkDistrict = new District(cityData.Districts.Count);
                parkDistrict.Type = CellType.Park;
                parkDistrict.Center = new float2(parkX, parkY);

                for (int x = parkX - parkSize; x <= parkX + parkSize; x++)
                {
                    for (int y = parkY - parkSize; y <= parkY + parkSize; y++)
                    {
                        if (x >= 0 && x < cityData.Width && y >= 0 && y < cityData.Height)
                        {
                            // 円形の公園
                            float dist = math.distance(new float2(x, y), new float2(parkX, parkY));
                            if (dist <= parkSize)
                            {
                                cityData.Cells[x, y] = CellType.Park;
                                parkDistrict.Cells.Add(new int2(x, y));
                            }
                        }
                    }
                }

                parkDistrict.Area = parkDistrict.Cells.Count;
                cityData.Districts.Add(parkDistrict);
            }
        }

        private void GenerateBuildingSlots(CityData cityData)
        {
            // 道路に面した場所に建物を配置
            for (int x = 2; x < cityData.Width - 2; x++)
            {
                for (int y = 2; y < cityData.Height - 2; y++)
                {
                    CellType cellType = cityData.Cells[x, y];

                    if (cellType == CellType.Road || cellType == CellType.Park)
                        continue;

                    // 道路に隣接しているかチェック
                    if (!IsAdjacentToRoad(cityData, x, y))
                        continue;

                    // 建物の間隔を調整
                    int spacing = cellType == CellType.Commercial ? 2 : 3;
                    if ((x % spacing == 0) && (y % spacing == 0))
                    {
                        cityData.BuildingSlots.Add(new int2(x, y));
                    }
                }
            }
        }

        private bool IsAdjacentToRoad(CityData cityData, int x, int y)
        {
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];
                if (nx >= 0 && nx < cityData.Width && ny >= 0 && ny < cityData.Height)
                {
                    if (cityData.Cells[nx, ny] == CellType.Road)
                        return true;
                }
            }
            return false;
        }

        private void MarkRoadLine(CityData cityData, int x1, int y1, int x2, int y2, float width)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int steps = math.max(math.abs(dx), math.abs(dy));

            if (steps == 0)
            {
                cityData.Cells[x1, y1] = CellType.Road;
                return;
            }

            for (int s = 0; s <= steps; s++)
            {
                float t = s / (float)steps;
                int x = x1 + (int)(dx * t);
                int y = y1 + (int)(dy * t);

                int halfWidth = (int)(width / 2f);
                for (int wx = -halfWidth; wx <= halfWidth; wx++)
                {
                    for (int wy = -halfWidth; wy <= halfWidth; wy++)
                    {
                        int nx = x + wx;
                        int ny = y + wy;
                        if (nx >= 0 && nx < cityData.Width && ny >= 0 && ny < cityData.Height)
                        {
                            cityData.Cells[nx, ny] = CellType.Road;
                        }
                    }
                }
            }
        }
    }
}
