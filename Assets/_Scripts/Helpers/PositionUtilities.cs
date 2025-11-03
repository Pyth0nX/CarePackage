using System.Collections.Generic;
using UnityEngine;

namespace CarePackage.Utilities
{
    public static class PositionUtilities
    {
        public static void CreateGrid(int count, float offset)
        {
            
        }
        
        public static void SpawnPrefabInRadialGrid(Transform centerTransform, GameObject prefab, int count, float offset)
        {
            Vector3 center = centerTransform.position;
            Vector3 size = prefab.GetComponent<Renderer>().bounds.size;
            float spacing = Mathf.Max(size.x, size.y, size.z) + offset;

            List<Vector3> positions = new List<Vector3>();

            // Always start with center
            if (count > 0) positions.Add(center);

            int placed = 1;

            // Define directional offsets
            Vector3[] directions = new Vector3[]
            {
                Vector3.forward,
                Vector3.back,
                Vector3.right,
                Vector3.left,
                Vector3.forward + Vector3.right,
                Vector3.forward + Vector3.left,
                Vector3.back + Vector3.right,
                Vector3.back + Vector3.left,
                Vector3.up,
                Vector3.down
            };

            int dirIndex = 0;

            while (placed < count)
            {
                Vector3 dir = directions[dirIndex % directions.Length];
                Vector3 pos = center + Vector3.Scale(dir.normalized, new Vector3(spacing, spacing, spacing));

                positions.Add(pos);
                placed++;
                dirIndex++;
            }

            // Instantiate objects
            foreach (var pos in positions)
            {
                GameObject.Instantiate(prefab, pos, Quaternion.identity);
            }
        }

        public static void SpawnPrefabInGridPattern(Transform centerTransform, GameObject prefab, int count, float offset)
        {
            Vector3 center = centerTransform.position;
            Vector3 size = prefab.GetComponent<Renderer>().bounds.size;
            float spacing = Mathf.Max(size.x, size.y, size.z) + offset;

            List<Vector3> positions = new List<Vector3>();

            int placed = 0;
            int zLayer = -1;

            while (placed < count)
            {
                // X positions at current Z layer
                Vector3 baseZ = center + new Vector3(0, 0, spacing * zLayer);

                Vector3 right = baseZ + new Vector3(spacing, 0, 0);
                Vector3 left = baseZ + new Vector3(-spacing, 0, 0);

                if (placed < count)
                {
                    positions.Add(right);
                    placed++;
                }

                if (placed < count)
                {
                    positions.Add(left);
                    placed++;
                }

                zLayer++;

                // Center row (Z = 0)
                if (zLayer == 1)
                {
                    Vector3 centerRowRight = center + new Vector3(spacing, 0, 0);
                    Vector3 centerRowLeft = center + new Vector3(-spacing, 0, 0);

                    if (placed < count)
                    {
                        positions.Add(centerRowRight);
                        placed++;
                    }

                    if (placed < count)
                    {
                        positions.Add(centerRowLeft);
                        placed++;
                    }
                }

                // Z forward layers
                Vector3 forwardZ = center + new Vector3(0, 0, spacing * (zLayer - 1));
                Vector3 forwardRight = forwardZ + new Vector3(spacing, 0, 0);
                Vector3 forwardLeft = forwardZ + new Vector3(-spacing, 0, 0);

                if (placed < count)
                {
                    positions.Add(forwardRight);
                    placed++;
                }

                if (placed < count)
                {
                    positions.Add(forwardLeft);
                    placed++;
                }

                // Y stacking if needed
                if (placed < count)
                {
                    Vector3 yUpRight = right + new Vector3(0, spacing, 0);
                    Vector3 yUpLeft = left + new Vector3(0, spacing, 0);

                    if (placed < count)
                    {
                        positions.Add(yUpRight);
                        placed++;
                    }

                    if (placed < count)
                    {
                        positions.Add(yUpLeft);
                        placed++;
                    }
                }
            }

            // Instantiate objects
            foreach (var pos in positions)
            {
                GameObject.Instantiate(prefab, pos, Quaternion.identity);
            }
        }

        public static List<Vector3> GenerateStrict2x3Grid(Vector3 center, int count, float spacing)
        {
            List<Vector3> positions = new();
            float halfX = spacing * 0.5f;
            float[] zRows = { -spacing, 0f, spacing };
            float[] xCols = { halfX, -halfX };
            int placed = 0;

            if (count <= 3)
            {
                for (int i = 0; i < count; i++)
                {
                    float z = -spacing + i * spacing;
                    positions.Add(center + new Vector3(0, 0, z));
                }
                return positions;
            }
            
            int yLayer = 0;
            while (placed < count)
            {
                float y = yLayer * spacing;

                foreach (float z in zRows)
                {
                    foreach (float x in xCols)
                    {
                        if (placed >= count) break;
                        positions.Add(center + new Vector3(x, y, z));
                        placed++;
                    }
                }
                yLayer++;
            }
            return positions;
        }
        
        private static void SpawnInitalPackages(Transform transitPackages, GameObject prefab, int count, float offset)
        {
            var center = transitPackages.position;
            var size = prefab.transform.localScale;
            float spacing = Mathf.Max(size.x, size.y, size.z) + offset;

            List<Vector3> positions = new();
            var spawned = 1;
            var layer = 1;

            while (spawned < count)
            {
                List<Vector3> layerPositions = new();
                if (layer == 1)
                {
                    layerPositions.Add(center + Vector3.forward * spacing);
                    layerPositions.Add(center + Vector3.back * spacing);
                }
                else if (layer == 2)
                {
                    layerPositions.Add(center + Vector3.right * spacing);
                    layerPositions.Add(center + Vector3.left * spacing);

                    layerPositions.Add(center + Vector3.forward * spacing + Vector3.right * spacing);
                    layerPositions.Add(center + Vector3.forward * spacing + Vector3.left * spacing);
                    layerPositions.Add(center + Vector3.back * spacing + Vector3.right * spacing);
                    layerPositions.Add(center + Vector3.back * spacing + Vector3.left * spacing);
                }
                else if (layer == 3)
                {
                    layerPositions.Add(center + Vector3.up * spacing);
                }
                
                foreach (var position in layerPositions)
                {
                    if (spawned >= count) break;
                    var package = GameObject.Instantiate(prefab, position, Quaternion.identity);
                    spawned++;
                }

                layer++;
            }
        }
    }
}