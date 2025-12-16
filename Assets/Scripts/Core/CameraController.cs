using UnityEngine;

namespace DominantK.Core
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float panBorderThickness = 10f;
        [SerializeField] private bool useEdgePan = true;
        [SerializeField] private bool useKeyboardPan = true;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 50f;

        [Header("Bounds")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-10, -10);
        [SerializeField] private Vector2 maxBounds = new Vector2(60, 60);

        [Header("Initial Position")]
        [SerializeField] private Vector3 startPosition = new Vector3(25, 30, 25);
        [SerializeField] private Vector3 startRotation = new Vector3(60, 0, 0);

        private Camera cam;

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
            transform.position = startPosition;
            transform.eulerAngles = startRotation;
        }

        private void Update()
        {
            HandleMovement();
            HandleZoom();
            ClampPosition();
        }

        private void HandleMovement()
        {
            Vector3 moveDirection = Vector3.zero;

            // Keyboard input
            if (useKeyboardPan)
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                    moveDirection += Vector3.forward;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                    moveDirection += Vector3.back;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                    moveDirection += Vector3.left;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                    moveDirection += Vector3.right;
            }

            // Edge pan
            if (useEdgePan)
            {
                Vector3 mousePos = Input.mousePosition;

                if (mousePos.y >= Screen.height - panBorderThickness)
                    moveDirection += Vector3.forward;
                if (mousePos.y <= panBorderThickness)
                    moveDirection += Vector3.back;
                if (mousePos.x >= Screen.width - panBorderThickness)
                    moveDirection += Vector3.right;
                if (mousePos.x <= panBorderThickness)
                    moveDirection += Vector3.left;
            }

            // Apply movement (ignore Y component from camera rotation)
            Vector3 move = new Vector3(moveDirection.x, 0, moveDirection.z).normalized;
            transform.position += move * panSpeed * Time.deltaTime;
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 pos = transform.position;
                pos.y -= scroll * zoomSpeed * 10f;
                pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom);
                transform.position = pos;
            }
        }

        private void ClampPosition()
        {
            if (!useBounds) return;

            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
            pos.z = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);
            transform.position = pos;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }

        public void FocusOn(Vector3 worldPosition)
        {
            Vector3 pos = transform.position;
            pos.x = worldPosition.x;
            pos.z = worldPosition.z;
            transform.position = pos;
        }
    }
}
