using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DominantK.Core
{
    /// <summary>
    /// Sets up quarter-view rendering for URP
    /// Attach to the Main Camera
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class QuarterViewSetup : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float pitchAngle = 45f;
        [SerializeField] private float yawAngle = 45f;
        [SerializeField] private float orthographicSize = 15f;

        [Header("URP Settings")]
        [SerializeField] private bool enableShadows = true;
        [SerializeField] private bool enablePostProcessing = true;
        [SerializeField] private bool enableAntiAliasing = true;

        [Header("Rendering")]
        [SerializeField] private Color backgroundColor = new Color(0.2f, 0.3f, 0.4f);

        private Camera cam;
        private UniversalAdditionalCameraData urpCameraData;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            urpCameraData = GetComponent<UniversalAdditionalCameraData>();

            SetupCamera();
            SetupURP();
        }

        private void SetupCamera()
        {
            // Orthographic setup
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;

            // Rotation for quarter view
            transform.rotation = Quaternion.Euler(pitchAngle, yawAngle, 0);

            // Background
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;

            // Clipping
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;
        }

        private void SetupURP()
        {
            if (urpCameraData == null)
            {
                urpCameraData = gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            urpCameraData.renderShadows = enableShadows;
            urpCameraData.renderPostProcessing = enablePostProcessing;

            if (enableAntiAliasing)
            {
                urpCameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                urpCameraData.antialiasingQuality = AntialiasingQuality.High;
            }
        }

        /// <summary>
        /// Convert screen position to world position on ground plane
        /// </summary>
        public Vector3 ScreenToGroundPosition(Vector3 screenPosition)
        {
            Ray ray = cam.ScreenPointToRay(screenPosition);

            // Intersect with Y=0 plane
            if (ray.direction.y != 0)
            {
                float t = -ray.origin.y / ray.direction.y;
                return ray.origin + ray.direction * t;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Get world bounds visible by camera
        /// </summary>
        public Bounds GetVisibleWorldBounds()
        {
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;

            // Account for rotation
            Vector3 center = ScreenToGroundPosition(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));

            // Rough bounds estimation
            return new Bounds(center, new Vector3(width * 1.5f, 10f, height * 1.5f));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (cam == null)
                cam = GetComponent<Camera>();

            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = orthographicSize;
                transform.rotation = Quaternion.Euler(pitchAngle, yawAngle, 0);
            }
        }
#endif
    }
}
