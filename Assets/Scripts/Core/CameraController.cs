using UnityEngine;

namespace DominantK.Core
{
    /// <summary>
    /// Quarter-view (isometric-style) camera controller for 3D gameplay
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Quarter View Settings")]
        [SerializeField] private float cameraAngleX = 45f;  // Pitch (looking down)
        [SerializeField] private float cameraAngleY = 45f;  // Yaw (rotation around Y)
        [SerializeField] private bool useOrthographic = true;

        [Header("Movement")]
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float panBorderThickness = 10f;
        [SerializeField] private bool useEdgePan = true;
        [SerializeField] private bool useKeyboardPan = true;
        [SerializeField] private bool useDragPan = true;
        [SerializeField] private float dragPanSpeed = 0.5f;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minOrthoSize = 5f;
        [SerializeField] private float maxOrthoSize = 30f;
        [SerializeField] private float defaultOrthoSize = 15f;

        [Header("Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-10, -10);
        [SerializeField] private Vector2 maxBounds = new Vector2(60, 60);

        [Header("Focus")]
        [SerializeField] private Vector3 initialFocusPoint = new Vector3(25, 0, 25);
        [SerializeField] private float cameraDistance = 50f;

        private Camera cam;
        private Vector3 focusPoint;
        private Vector3 lastMousePosition;
        private bool isDragging;

        // Direction vectors for quarter view movement (rotated 45 degrees)
        private Vector3 forwardDir;
        private Vector3 rightDir;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }
        }

        private void Start()
        {
            SetupQuarterView();
        }

        private void SetupQuarterView()
        {
            focusPoint = initialFocusPoint;

            // Calculate camera rotation
            Quaternion rotation = Quaternion.Euler(cameraAngleX, cameraAngleY, 0);
            transform.rotation = rotation;

            // Position camera based on focus point and distance
            UpdateCameraPosition();

            // Setup orthographic
            if (useOrthographic && cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = defaultOrthoSize;
            }

            // Calculate movement directions (45 degree rotated)
            float yawRad = cameraAngleY * Mathf.Deg2Rad;
            forwardDir = new Vector3(-Mathf.Sin(yawRad), 0, -Mathf.Cos(yawRad)).normalized;
            rightDir = new Vector3(Mathf.Cos(yawRad), 0, -Mathf.Sin(yawRad)).normalized;
        }

        private void Update()
        {
            HandleKeyboardPan();
            HandleEdgePan();
            HandleDragPan();
            HandleZoom();
            ClampFocusPoint();
            UpdateCameraPosition();
        }

        private void HandleKeyboardPan()
        {
            if (!useKeyboardPan) return;

            Vector3 moveDirection = Vector3.zero;

            // W/S moves along the visual "up/down" direction (inverted)
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                moveDirection -= forwardDir;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                moveDirection += forwardDir;

            // A/D moves along the visual "left/right" direction
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveDirection -= rightDir;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveDirection += rightDir;

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                focusPoint += moveDirection.normalized * panSpeed * Time.deltaTime;
            }
        }

        private void HandleEdgePan()
        {
            if (!useEdgePan) return;

            Vector3 mousePos = Input.mousePosition;
            Vector3 moveDirection = Vector3.zero;

            if (mousePos.y >= Screen.height - panBorderThickness)
                moveDirection -= forwardDir;
            if (mousePos.y <= panBorderThickness)
                moveDirection += forwardDir;
            if (mousePos.x >= Screen.width - panBorderThickness)
                moveDirection += rightDir;
            if (mousePos.x <= panBorderThickness)
                moveDirection -= rightDir;

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                focusPoint += moveDirection.normalized * panSpeed * Time.deltaTime;
            }
        }

        private void HandleDragPan()
        {
            if (!useDragPan) return;

            // Middle mouse button drag
            if (Input.GetMouseButtonDown(2))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(2))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                lastMousePosition = Input.mousePosition;

                // Convert screen delta to world movement (Y inverted)
                Vector3 move = (-rightDir * delta.x + forwardDir * delta.y) * dragPanSpeed * Time.deltaTime;
                focusPoint += move;
            }
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Abs(scroll) > 0.01f && cam != null)
            {
                if (useOrthographic)
                {
                    cam.orthographicSize -= scroll * zoomSpeed * 10f;
                    cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthoSize, maxOrthoSize);
                }
                else
                {
                    cameraDistance -= scroll * zoomSpeed * 10f;
                    cameraDistance = Mathf.Clamp(cameraDistance, 10f, 100f);
                }
            }
        }

        private void ClampFocusPoint()
        {
            if (!useBounds) return;

            focusPoint.x = Mathf.Clamp(focusPoint.x, minBounds.x, maxBounds.x);
            focusPoint.z = Mathf.Clamp(focusPoint.z, minBounds.y, maxBounds.y);
        }

        private void UpdateCameraPosition()
        {
            // Calculate position offset from focus point based on rotation and distance
            Vector3 offset = Quaternion.Euler(cameraAngleX, cameraAngleY, 0) * Vector3.back * cameraDistance;
            transform.position = focusPoint + offset;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }

        public void FocusOn(Vector3 worldPosition)
        {
            focusPoint = new Vector3(worldPosition.x, 0, worldPosition.z);
        }

        public void SetZoom(float normalizedZoom)
        {
            if (cam != null && useOrthographic)
            {
                cam.orthographicSize = Mathf.Lerp(minOrthoSize, maxOrthoSize, 1f - normalizedZoom);
            }
        }

        /// <summary>
        /// Get the ground position under the mouse cursor
        /// </summary>
        public bool GetMouseGroundPosition(out Vector3 position, LayerMask groundLayer)
        {
            position = Vector3.zero;

            if (cam == null) return false;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            {
                position = hit.point;
                return true;
            }

            // Fallback: intersect with Y=0 plane
            if (ray.direction.y != 0)
            {
                float t = -ray.origin.y / ray.direction.y;
                if (t > 0)
                {
                    position = ray.origin + ray.direction * t;
                    return true;
                }
            }

            return false;
        }
    }
}
