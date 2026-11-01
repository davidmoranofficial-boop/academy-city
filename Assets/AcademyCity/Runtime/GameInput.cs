using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AcademyCity
{
    /// <summary>Keyboard and mouse reading that works with either Unity input setting (new Input System or the old Input Manager).</summary>
    public static class GameInput
    {
        /// <summary>WASD or arrow keys as x (right) and y (forward), length up to 1.</summary>
        public static Vector2 Move
        {
            get
            {
                Vector2 v = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current;
                if (k != null)
                {
                    if (k.wKey.isPressed || k.upArrowKey.isPressed) v.y += 1;
                    if (k.sKey.isPressed || k.downArrowKey.isPressed) v.y -= 1;
                    if (k.dKey.isPressed || k.rightArrowKey.isPressed) v.x += 1;
                    if (k.aKey.isPressed || k.leftArrowKey.isPressed) v.x -= 1;
                }
#elif ENABLE_LEGACY_INPUT_MANAGER
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v.y += 1;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v.y -= 1;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) v.x += 1;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) v.x -= 1;
#endif
                return Vector2.ClampMagnitude(v, 1f);
            }
        }

        public static bool Sprint
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current; return k != null && (k.leftShiftKey.isPressed || k.rightShiftKey.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#else
                return false;
#endif
            }
        }

        public static bool Walk
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current; return k != null && (k.leftCtrlKey.isPressed || k.rightCtrlKey.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#else
                return false;
#endif
            }
        }

        /// <summary>Mouse movement this frame, in pixels.</summary>
        public static Vector2 Look
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var m = Mouse.current; return m != null ? m.delta.ReadValue() : Vector2.zero;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 10f;
#else
                return Vector2.zero;
#endif
            }
        }

        /// <summary>Scroll wheel this frame: positive is away from you.</summary>
        public static float Zoom
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var m = Mouse.current; return m != null ? m.scroll.ReadValue().y / 120f : 0f;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.mouseScrollDelta.y;
#else
                return 0f;
#endif
            }
        }

        public static bool ClickedToCapture
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var m = Mouse.current; return m != null && m.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetMouseButtonDown(0);
#else
                return false;
#endif
            }
        }

        public static bool ReleaseMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var k = Keyboard.current; return k != null && k.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.GetKeyDown(KeyCode.Escape);
#else
                return false;
#endif
            }
        }
    }
}
