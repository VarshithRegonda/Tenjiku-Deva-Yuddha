using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Camera controller supporting pan, zoom, and rotation.
    /// Works with both mouse/keyboard (desktop) and touch (mobile).
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Pan Settings")]
        public float PanSpeed = 20f;
        public float PanBorderThickness = 10f;
        public bool EdgePanning = true;

        [Header("Zoom Settings")]
        public float ZoomSpeed = 10f;
        public float MinZoom = 5f;
        public float MaxZoom = 50f;

        [Header("Rotation Settings")]
        public float RotationSpeed = 100f;

        [Header("Bounds")]
        public float MinX = -10f;
        public float MaxX = 74f;
        public float MinZ = -10f;
        public float MaxZ = 74f;

        [Header("Mobile Touch")]
        public float TouchPanSensitivity = 0.1f;
        public float PinchZoomSensitivity = 0.02f;

        private Camera _camera;
        private Vector3 _lastMousePosition;
        private bool _isDragging;

        // Touch tracking
        private float _lastPinchDistance;

        private void Start()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
                _camera = Camera.main;

            // Default camera position: above and looking at center of map
            float centerX = GameConstants.DEFAULT_MAP_WIDTH * GameConstants.TILE_SIZE * 0.5f;
            float centerZ = GameConstants.DEFAULT_MAP_HEIGHT * GameConstants.TILE_SIZE * 0.5f;
            transform.position = new Vector3(centerX, 25f, centerZ - 15f);
            transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        }

        private void Update()
        {
            if (Input.touchCount >= 2)
            {
                HandleTouchInput();
            }
            else
            {
                HandleKeyboardPan();
                HandleMousePan();
                HandleMouseZoom();
                HandleRotation();
            }

            ClampPosition();
        }

        // ─────────────────────────────────────────────
        //  Keyboard Pan (WASD / Arrow Keys)
        // ─────────────────────────────────────────────
        private void HandleKeyboardPan()
        {
            Vector3 move = Vector3.zero;
            float speed = PanSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                move += transform.forward;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                move -= transform.forward;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                move -= transform.right;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                move += transform.right;

            // Edge panning (desktop only)
            if (EdgePanning && !Application.isMobilePlatform)
            {
                if (Input.mousePosition.y >= Screen.height - PanBorderThickness)
                    move += transform.forward;
                if (Input.mousePosition.y <= PanBorderThickness)
                    move -= transform.forward;
                if (Input.mousePosition.x >= Screen.width - PanBorderThickness)
                    move += transform.right;
                if (Input.mousePosition.x <= PanBorderThickness)
                    move -= transform.right;
            }

            // Keep movement on the XZ plane
            move.y = 0;
            transform.position += move.normalized * speed;
        }

        // ─────────────────────────────────────────────
        //  Mouse Middle-Button Pan
        // ─────────────────────────────────────────────
        private void HandleMousePan()
        {
            if (Input.GetMouseButtonDown(2))
            {
                _isDragging = true;
                _lastMousePosition = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(2))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                Vector3 delta = Input.mousePosition - _lastMousePosition;
                Vector3 move = (-transform.right * delta.x - transform.forward * delta.y) * 0.05f;
                move.y = 0;
                transform.position += move;
                _lastMousePosition = Input.mousePosition;
            }
        }

        // ─────────────────────────────────────────────
        //  Mouse Scroll Zoom
        // ─────────────────────────────────────────────
        private void HandleMouseZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 pos = transform.position;
                pos.y -= scroll * ZoomSpeed;
                pos.y = Mathf.Clamp(pos.y, MinZoom, MaxZoom);
                transform.position = pos;
            }
        }

        // ─────────────────────────────────────────────
        //  Keyboard Rotation (Q/E)
        // ─────────────────────────────────────────────
        private void HandleRotation()
        {
            if (Input.GetKey(KeyCode.Q))
                transform.RotateAround(GetLookAtPoint(), Vector3.up, -RotationSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.E))
                transform.RotateAround(GetLookAtPoint(), Vector3.up, RotationSpeed * Time.deltaTime);
        }

        // ─────────────────────────────────────────────
        //  Touch Input (Mobile)
        // ─────────────────────────────────────────────
        private void HandleTouchInput()
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // Pinch to zoom
            float currentPinchDist = Vector2.Distance(touch0.position, touch1.position);
            if (touch1.phase == TouchPhase.Began)
            {
                _lastPinchDistance = currentPinchDist;
                return;
            }

            float pinchDelta = currentPinchDist - _lastPinchDistance;
            Vector3 pos = transform.position;
            pos.y -= pinchDelta * PinchZoomSensitivity;
            pos.y = Mathf.Clamp(pos.y, MinZoom, MaxZoom);
            transform.position = pos;
            _lastPinchDistance = currentPinchDist;

            // Two-finger pan
            Vector2 avgDelta = (touch0.deltaPosition + touch1.deltaPosition) * 0.5f;
            Vector3 panMove = (-transform.right * avgDelta.x - transform.forward * avgDelta.y) * TouchPanSensitivity;
            panMove.y = 0;
            transform.position += panMove;
        }

        // ─────────────────────────────────────────────
        //  Utility
        // ─────────────────────────────────────────────
        private void ClampPosition()
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, MinX, MaxX);
            pos.z = Mathf.Clamp(pos.z, MinZ, MaxZ);
            pos.y = Mathf.Clamp(pos.y, MinZoom, MaxZoom);
            transform.position = pos;
        }

        private Vector3 GetLookAtPoint()
        {
            // Raycast from camera center to ground
            Ray ray = new Ray(transform.position, transform.forward);
            Plane ground = new Plane(Vector3.up, Vector3.zero);
            if (ground.Raycast(ray, out float dist))
                return ray.GetPoint(dist);
            return transform.position + transform.forward * 20f;
        }

        /// <summary>
        /// Smoothly move camera to look at a world position.
        /// </summary>
        public void FocusOn(Vector3 worldPosition)
        {
            Vector3 offset = transform.position - GetLookAtPoint();
            transform.position = worldPosition + offset;
            ClampPosition();
        }

        public void FocusOnGridCenter()
        {
            float cx = GameConstants.DEFAULT_MAP_WIDTH * GameConstants.TILE_SIZE * 0.5f;
            float cz = GameConstants.DEFAULT_MAP_HEIGHT * GameConstants.TILE_SIZE * 0.5f;
            FocusOn(new Vector3(cx, 0, cz));
        }
    }
}
