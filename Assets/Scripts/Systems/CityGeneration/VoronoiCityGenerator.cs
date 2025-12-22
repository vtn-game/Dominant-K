using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace DominantK.Systems.CityGeneration
{
    /// <summary>
    /// ボロノイ図ベースの街生成アルゴリズム
    /// 自然な区画分けと有機的な街並みを生成
    /// </summary>
    public class VoronoiCityGenerator : ICityGenerator
    {
        private System.Random random;

        public CityData Generate(CityGenerationSettings settings)
        {
            random = new System.Random(settings.Seed);
            var cityData = new CityData(settings.Width, settings.Height);

            // 1. サイト（種）を生成
            var sites = GenerateSites(settings);

            // 2. Lloyd緩和でサイトを均等に分布
            for (int i = 0; i < settings.VoronoiRelaxationIterations; i++)
            {
                sites = LloydRelaxation(sites, settings.Width, settings.Height);
            }

            // 3. ボロノイ図を計算してセルを割り当て
            AssignCellsToDistricts(cityData, sites);

            // 4. 隣接する地区間に道路を生成
            GenerateRoadsBetweenDistricts(cityData, sites);

            // 5. 地区タイプを割り当て
            AssignDistrictTypes(cityData);

            // 6. 建物スロットを生成
            GenerateBuildingSlots(cityData);

            return cityData;
        }

        private List<float2> GenerateSites(CityGenerationSettings settings)
        {
            var sites = new List<float2>();
            int count = settings.VoronoiSiteCount;

            // ポアソンディスク風の分布で初期配置
            float minDist = math.sqrt((settings.Width * settings.Height) / (float)count) * 0.7f;

            for (int i = 0; i < count; i++)
            {
                float2 site;
                int attempts = 0;
                do
                {
                    site = new float2(
                        random.Next(settings.Width),
                        random.Next(settings.Height)
                    );
                    attempts++;
                } while (IsTooClose(site, sites, minDist) && attempts < 100);

                sites.Add(site);
            }

            return sites;
        }

        private bool IsTooClose(float2 point, List<float2> existing, float minDist)
        {
            foreach (var p in existing)
            {
                if (math.distance(point, p) < minDist)
                    return true;
            }
            return false;
        }

        private List<float2> LloydRelaxation(List<float2> sites, int width, int height)
        {
            // 各サイトに属するセルを計算
            var cellLists = new List<List<int2>>();
            for (int i = 0; i < sites.Count; i++)
            {
                cellLists.Add(new List<int2>());
            }

            // 全セルを最も近いサイトに割り当て
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int closest = FindClosestSite(new float2(x, y), sites);
                    cellLists[closest].Add(new int2(x, y));
                }
            }

            // 各領域の重心を新しいサイトとする
            var newSites = new List<float2>();
            for (int i = 0; i < sites.Count; i++)
            {
                if (cellLists[i].Count > 0)
                {
                    float2 centroid = float2.zero;
                    foreach (var cell in cellLists[i])
                    {
                        centroid += new float2(cell.x, cell.y);
                    }
                    centroid /= cellLists[i].Count;

                    // 境界内に収める
                    centroid.x = math.clamp(centroid.x, 0, width - 1);
                    centroid.y = math.clamp(centroid.y, 0, height - 1);
                    newSites.Add(centroid);
                }
                else
                {
                    newSites.Add(sites[i]);
                }
            }

            return newSites;
        }

        private int FindClosestSite(float2 point, List<float2> sites)
        {
            int closest = 0;
            float minDist = float.MaxValue;

            for (int i = 0; i < sites.Count; i++)
            {
                float dist = math.distancesq(point, sites[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = i;
                }
            }

            return closest;
        }

        private void AssignCellsToDistricts(CityData cityData, List<float2> sites)
        {
            // 地区を作成
            for (int i = 0; i < sites.Count; i++)
            {
                var district = new District(i);
                district.Center = sites[i];
                cityData.Districts.Add(district);
            }

            // 各セルを最も近いサイトの地区に割り当て
            for (int x = 0; x < cityData.Width; x++)
            {
                for (int y = 0; y < cityData.Height; y++)
                {
                    int closest = FindClosestSite(new float2(x, y), sites);
                    cityData.Districts[closest].Cells.Add(new int2(x, y));
                }
            }

            // 各地区の面積を計算
            foreach (var district in cityData.Districts)
            {
                district.Area = district.Cells.Count;
            }
        }

        private void GenerateRoadsBetweenDistricts(CityData cityData, List<float2> sites)
        {
            // Delaunay三角形分割（簡易版：最近傍を接続）
            var connections = new HashSet<(int, int)>();

            for (int i = 0; i < sites.Count; i++)
            {
                // 各サイトから最も近い3つのサイトへ接続
                var distances = new List<(int index, float dist)>();
                for (int j = 0; j < sites.Count; j++)
                {
                    if (i != j)
                    {
                        distances.Add((j, math.distance(sites[i], sites[j])));
                    }
                }
                distances.Sort((a, b) => a.dist.CompareTo(b.dist));

                for (int k = 0; k < math.min(3, distances.Count); k++)
                {
                    int j = distances[k].index;
                    var conn = i < j ? (i, j) : (j, i);
                    connections.Add(conn);
                }
            }

            // 接続に基づいて道路を生成
            foreach (var (a, b) in connections)
            {
                var road = new Road();
                road.Width = random.NextDouble() < 0.3 ? 3f : 1.5f;
                road.IsMainRoad = road.Width > 2f;

                // 道路の経路を計算（直線 + わずかな曲がり）
                float2 start = sites[a];
                float2 end = sites[b];
                float2 mid = (start + end) / 2f;

                // わずかにオフセット
                float2 perpendicular = math.normalize(new float2(-(end.y - start.y), end.x - start.x));
                mid += perpendicular * (float)(random.NextDouble() - 0.5) * 5f;

                road.Points.Add(start);
                road.Points.Add(mid);
                road.Points.Add(end);

                cityData.Roads.Add(road);

                // 道路セルをマーク
                MarkRoadCells(cityData, road);
            }
        }

        private void MarkRoadCells(CityData cityData, Road road)
        {
            for (int i = 0; i < road.Points.Count - 1; i++)
            {
                float2 start = road.Points[i];
                float2 end = road.Points[i + 1];
                float length = math.distance(start, end);
                int steps = (int)(length * 2);

                for (int s = 0; s <= steps; s++)
                {
                    float t = s / (float)steps;
                    float2 point = math.lerp(start, end, t);

                    int halfWidth = (int)(road.Width / 2f) + 1;
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
            foreach (var district in cityData.Districts)
            {
                // 中心からの距離に基づいてタイプを決定
                float2 cityCenter = new float2(cityData.Width / 2f, cityData.Height / 2f);
                float distFromCenter = math.distance(district.Center, cityCenter);
                float normalizedDist = distFromCenter / math.sqrt(cityData.Width * cityData.Width + cityData.Height * cityData.Height) * 2f;

                if (normalizedDist < 0.2f)
                {
                    district.Type = CellType.Commercial;
                }
                else if (normalizedDist < 0.5f)
                {
                    district.Type = random.NextDouble() < 0.7 ? CellType.Commercial : CellType.Residential;
                }
                else
                {
                    district.Type = random.NextDouble() < 0.8 ? CellType.Residential : CellType.Industrial;
                }

                // 地区のセルにタイプを適用
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
            // 道路に隣接するセルに建物スロットを配置
            for (int x = 1; x < cityData.Width - 1; x++)
            {
                for (int y = 1; y < cityData.Height - 1; y++)
                {
                    if (cityData.Cells[x, y] != CellType.Road && IsAdjacentToRoad(cityData, x, y))
                    {
                        // 間隔を空けて配置
                        if ((x + y) % 3 == 0)
                        {
                            cityData.BuildingSlots.Add(new int2(x, y));
                        }
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
    }
}
