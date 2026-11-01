using UnityEngine;

namespace AcademyCity
{
    /// <summary>
    /// Third-person movement: WASD moves relative to the camera, Shift sprints, Ctrl walks.
    /// The character turns smoothly to face where he is going.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public Transform cameraTransform;
        public float walkSpeed = 1.7f;
        public float jogSpeed = 4.0f;
        public float sprintSpeed = 6.5f;
        public float acceleration = 14f;
        public float turnSpeed = 720f; // degrees per second
        public float gravity = -20f;

        CharacterController cc;
        CharacterAnimation anim;
        Vector3 velocity;
        float vertical;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            anim = GetComponent<CharacterAnimation>();
        }

        void Update()
        {
            Vector2 input = GameInput.Move;
            Vector3 fwd = Vector3.forward, right = Vector3.right;
            if (cameraTransform != null)
            {
                fwd = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                right = Vector3.Cross(Vector3.up, fwd);
            }
            Vector3 wish = fwd * input.y + right * input.x;
            float target = input.sqrMagnitude < 0.01f ? 0f : GameInput.Sprint ? sprintSpeed : GameInput.Walk ? walkSpeed : jogSpeed;
            Vector3 targetVel = wish.normalized * target;
            velocity = Vector3.MoveTowards(velocity, targetVel, acceleration * Time.deltaTime);

            if (wish.sqrMagnitude > 0.01f)
            {
                var face = Quaternion.LookRotation(wish, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, face, turnSpeed * Time.deltaTime);
            }

            vertical = cc.isGrounded ? -2f : vertical + gravity * Time.deltaTime;
            cc.Move((velocity + Vector3.up * vertical) * Time.deltaTime);

            // Keep him on the floor.
            float half = Playground.FloorSize * 0.5f - 0.5f;
            var p = transform.position;
            if (Mathf.Abs(p.x) > half || Mathf.Abs(p.z) > half)
            {
                cc.enabled = false;
                transform.position = new Vector3(Mathf.Clamp(p.x, -half, half), p.y, Mathf.Clamp(p.z, -half, half));
                cc.enabled = true;
            }

            if (anim != null) anim.speed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        }
    }
}
