using UnityEngine;

namespace DominantK.Systems
{
    /// <summary>
    /// Visualizes the grid for quarter-view display
    /// </summary>
    public class GridVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridSystem gridSystem;

        [Header("Grid Lines")]
        [SerializeField] private bool showGridLines = true;
        [SerializeField] private Color gridLineColor = new Color(1, 1, 1, 0.2f);
        [SerializeField] private float lineWidth = 0.02f;

        [Header("Hover Highlight")]
        [SerializeField] private bool showHoverHighlight = true;
        [SerializeField] private GameObject highlightPrefab;
        [SerializeField] private Color validHighlightColor = new Color(0, 1, 0, 0.5f);
        [SerializeField] private Color invalidHighlightColor = new Color(1, 0, 0, 0.5f);

        [Header("ZOC Visualization")]
        [SerializeField] private bool showZOC = true;
        [SerializeField] private Material zocMaterial;

        private GameObject gridLinesParent;
        private GameObject highlightObject;
        private MeshRenderer highlightRenderer;

        private void Start()
        {
            if (showGridLines)
            {
                CreateGridLines();
            }

            if (showHoverHighlight)
            {
                CreateHighlight();
            }
        }

        private void CreateGridLines()
        {
            if (gridSystem == null) return;

            gridLinesParent = new GameObject("GridLines");
            gridLinesParent.transform.SetParent(transform);

            int width = gridSystem.Width;
            int height = gridSystem.Height;
            float cellSize = gridSystem.CellSize;

            // Create line material
            var lineMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.color = gridLineColor;

            // Vertical lines
            for (int x = 0; x <= width; x++)
            {
                CreateLine(
                    new Vector3(x * cellSize, 0.01f, 0),
                    new Vector3(x * cellSize, 0.01f, height * cellSize),
                    lineMaterial
                );
            }

            // Horizontal lines
            for (int z = 0; z <= height; z++)
            {
                CreateLine(
                    new Vector3(0, 0.01f, z * cellSize),
                    new Vector3(width * cellSize, 0.01f, z * cellSize),
                    lineMaterial
                );
            }
        }

        private void CreateLine(Vector3 start, Vector3 end, Material material)
        {
            var lineObj = new GameObject("Line");
            lineObj.transform.SetParent(gridLinesParent.transform);

            var lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = material;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.useWorldSpace = true;
        }

        private void CreateHighlight()
        {
            if (highlightPrefab != null)
            {
                highlightObject = Instantiate(highlightPrefab, transform);
            }
            else
            {
                highlightObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                highlightObject.name = "CellHighlight";
                highlightObject.transform.SetParent(transform);

                // Remove collider
                var collider = highlightObject.GetComponent<Collider>();
                if (collider != null) Destroy(collider);

                // Setup material
                highlightRenderer = highlightObject.GetComponent<MeshRenderer>();
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetFloat("_Blend", 0);
                mat.SetFloat("_AlphaClip", 0);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                mat.color = validHighlightColor;
                highlightRenderer.material = mat;
            }

            // Scale to cell size
            float cellSize = gridSystem != null ? gridSystem.CellSize : 1f;
            highlightObject.transform.localScale = new Vector3(cellSize * 0.95f, 0.1f, cellSize * 0.95f);
            highlightObject.SetActive(false);
        }

        public void ShowHighlight(Vector2Int gridPos, bool valid)
        {
            if (!showHoverHighlight || highlightObject == null || gridSystem == null) return;

            Vector3 worldPos = gridSystem.GetWorldPosition(gridPos);
            worldPos.y = 0.05f;

            highlightObject.transform.position = worldPos;
            highlightObject.SetActive(true);

            if (highlightRenderer != null)
            {
                highlightRenderer.material.color = valid ? validHighlightColor : invalidHighlightColor;
            }
        }

        public void HideHighlight()
        {
            if (highlightObject != null)
            {
                highlightObject.SetActive(false);
            }
        }

        public void SetGridLinesVisible(bool visible)
        {
            showGridLines = visible;
            if (gridLinesParent != null)
            {
                gridLinesParent.SetActive(visible);
            }
        }
    }
}
