#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// ============================================================
//  AssetCaptureCamera.cs  –  HexaForge 3D to 2D Asset Capture
//
//  Orbit/Pan/Zoom controller for the preview window.
//  Mutates ACCameraSettings in-place; caller calls Repaint()
//  when HandleInput returns true (view changed).
// ============================================================

namespace HexaForge.AssetCapture
{
    public class AssetCaptureCameraController
    {
        // ── State ─────────────────────────────────────────────────────

        public ACCameraSettings Settings { get; private set; }

        // Orbit state (separate from Settings to allow smooth syncing)
        private float _orbitX;   // elevation  (pitch)
        private float _orbitY;   // azimuth    (yaw)
        private Vector3 _pivot   = Vector3.zero;

        // Mouse interaction
        private Vector2 _prevMouse;
        private bool    _mouseDown;
        private int     _mouseButton;

        // ── Constructor ───────────────────────────────────────────────

        public AssetCaptureCameraController(ACCameraSettings settings)
        {
            Settings = settings;
            SyncOrbitFromSettings();
        }

        public void UpdateSettings(ACCameraSettings settings)
        {
            Settings = settings;
            SyncOrbitFromSettings();
        }

        // ── Apply to Camera ───────────────────────────────────────────

        /// <summary>
        /// Pushes the current settings onto a Unity Camera component.
        /// </summary>
        public void ApplyToCamera(Camera cam)
        {
            if (cam == null) return;
            cam.orthographic     = Settings.projection == ACProjection.Orthographic;
            cam.fieldOfView      = Settings.fieldOfView;
            cam.orthographicSize = Settings.orthographicSize;
            cam.nearClipPlane    = 0.01f;
            cam.farClipPlane     = 2000f;
            cam.transform.position    = Settings.position;
            cam.transform.eulerAngles = Settings.eulerRotation;
        }

        // ── Mouse Interaction ─────────────────────────────────────────

        /// <summary>
        /// Handles mouse events inside <paramref name="rect"/>.
        /// Returns true if the view was modified and a repaint is needed.
        /// </summary>
        public bool HandleInput(Rect rect, Event evt)
        {
            bool dirty = false;

            switch (evt.type)
            {
                // ── Scroll to zoom ──────────────────────────────────
                case EventType.ScrollWheel:
                    if (rect.Contains(evt.mousePosition))
                    {
                        float delta = evt.delta.y;
                        if (Settings.projection == ACProjection.Perspective)
                        {
                            Settings.distance = Mathf.Max(0.1f,
                                Settings.distance * (1f + delta * 0.05f));
                            RebuildPositionFromOrbit();
                        }
                        else
                        {
                            Settings.orthographicSize = Mathf.Max(0.05f,
                                Settings.orthographicSize * (1f + delta * 0.05f));
                        }
                        evt.Use();
                        dirty = true;
                    }
                    break;

                // ── Mouse down – begin drag ─────────────────────────
                case EventType.MouseDown:
                    if (rect.Contains(evt.mousePosition) && !_mouseDown)
                    {
                        _mouseDown   = true;
                        _mouseButton = evt.button;
                        _prevMouse   = evt.mousePosition;
                        evt.Use();
                    }
                    break;

                // ── Mouse drag ──────────────────────────────────────
                case EventType.MouseDrag:
                    if (_mouseDown)
                    {
                        Vector2 delta = evt.mousePosition - _prevMouse;
                        _prevMouse = evt.mousePosition;

                        if (_mouseButton == 0) // Left drag → Orbit
                        {
                            _orbitX += delta.y * 0.4f;
                            _orbitY += delta.x * 0.4f;
                            _orbitX  = Mathf.Clamp(_orbitX, -89f, 89f);
                            Settings.eulerRotation = new Vector3(_orbitX, _orbitY, 0f);
                            RebuildPositionFromOrbit();
                            dirty = true;
                        }
                        else if (_mouseButton == 2 || (_mouseButton == 1 && evt.alt)) // Middle / Alt+Right → Pan
                        {
                            float panScale = Settings.projection == ACProjection.Orthographic
                                ? Settings.orthographicSize * 0.004f
                                : Settings.distance * 0.002f;

                            Quaternion rot = Quaternion.Euler(Settings.eulerRotation);
                            _pivot -= rot * Vector3.right * delta.x * panScale;
                            _pivot += rot * Vector3.up    * delta.y * panScale;
                            RebuildPositionFromOrbit();
                            dirty = true;
                        }
                        evt.Use();
                    }
                    break;

                // ── Mouse up ────────────────────────────────────────
                case EventType.MouseUp:
                    _mouseDown = false;
                    break;
            }

            return dirty;
        }

        // ── Camera Helpers ────────────────────────────────────────────

        /// <summary>Resets camera to default view direction and distance.</summary>
        public void ResetCamera()
        {
            _orbitX  = 30f;
            _orbitY  = -30f;
            _pivot   = Vector3.zero;
            Settings.eulerRotation    = new Vector3(_orbitX, _orbitY, 0f);
            Settings.distance         = 5f;
            Settings.orthographicSize = 2f;
            RebuildPositionFromOrbit();
        }

        /// <summary>Moves the pivot to the bounds centre without changing orbit angles.</summary>
        public void AutoCenter(Bounds bounds)
        {
            _pivot = bounds.center;
            RebuildPositionFromOrbit();
        }

        /// <summary>
        /// Adjusts orthographic size or camera distance so the bounds fills
        /// the frame at the given <paramref name="padding"/> fraction.
        /// </summary>
        public void FitToFrame(Bounds bounds, float padding, float aspectRatio)
        {
            _pivot = bounds.center;

            if (Settings.projection == ACProjection.Orthographic)
                Settings.orthographicSize = AssetCaptureUtility.FitOrthographicSize(bounds, aspectRatio, padding);
            else
                Settings.distance = AssetCaptureUtility.FitPerspectiveDistance(bounds, Settings.fieldOfView, aspectRatio, padding);

            RebuildPositionFromOrbit();
        }

        // ── Private ───────────────────────────────────────────────────

        private void SyncOrbitFromSettings()
        {
            _orbitX = Settings.eulerRotation.x;
            _orbitY = Settings.eulerRotation.y;
            RebuildPositionFromOrbit();
        }

        private void RebuildPositionFromOrbit()
        {
            Quaternion rotation = Quaternion.Euler(_orbitX, _orbitY, 0f);
            Settings.eulerRotation = new Vector3(_orbitX, _orbitY, 0f);
            // Camera looks in -Z local space, so Back = -forward
            Settings.position = _pivot + rotation * (Vector3.back * Settings.distance);
        }
    }
}
#endif
