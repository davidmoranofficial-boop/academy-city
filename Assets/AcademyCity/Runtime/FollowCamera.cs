using UnityEngine;

namespace AcademyCity
{
    /// <summary>
    /// Orbit camera behind the player. Click the game view to capture the mouse, move the mouse to look around,
    /// scroll to zoom, Esc to free the mouse.
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 4.5f;
        public float minDistance = 1.5f, maxDistance = 10f;
        public float height = 1.5f;          // look-at point above his feet
        public float sensitivity = 0.12f;    // degrees per pixel
        public float minPitch = -20f, maxPitch = 70f;

        float yaw, pitch = 15f;

        void Start()
        {
            if (target != null) yaw = target.eulerAngles.y;
            Capture(true);
        }

        static void Capture(bool on)
        {
            Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !on;
        }

        void LateUpdate()
        {
            if (target == null) return;
            if (GameInput.ReleaseMouse) Capture(false);
            else if (GameInput.ClickedToCapture) Capture(true);

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 look = GameInput.Look;
                yaw += look.x * sensitivity;
                pitch = Mathf.Clamp(pitch - look.y * sensitivity, minPitch, maxPitch);
            }
            distance = Mathf.Clamp(distance - GameInput.Zoom * 0.6f, minDistance, maxDistance);

            Vector3 focus = target.position + Vector3.up * height;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pos = focus - rot * Vector3.forward * distance;
            if (pos.y < 0.2f) pos.y = 0.2f;   // never dip under the floor
            transform.SetPositionAndRotation(pos, Quaternion.LookRotation(focus - pos, Vector3.up));
        }
    }
}
