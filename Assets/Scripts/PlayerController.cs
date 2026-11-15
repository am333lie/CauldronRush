
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 4f;
    public Transform cameraTransform;

    [Header("Jump")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public int maxJumps = 2;

    [Header("Ground Detection")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask = ~0;

    [Header("Character Model")]
    public Transform visualModel;

    private CharacterController controller;
    private float verticalVelocity;

    private bool isGrounded;
    private Vector3 groundNormal = Vector3.up;

    private int jumpsRemaining;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        jumpsRemaining = maxJumps;

        // Align visible model with the bottom
        // of the Character Controller
        if (visualModel != null)
        {
            Renderer[] renderers =
                visualModel.GetComponentsInChildren<Renderer>();

            if (renderers.Length > 0)
            {
                Bounds modelBounds = renderers[0].bounds;

                foreach (Renderer renderer in renderers)
                {
                    modelBounds.Encapsulate(renderer.bounds);
                }

                float offset =
                    controller.bounds.min.y - modelBounds.min.y;

                visualModel.position += Vector3.up * offset;
            }
        }
    }

    void Update()
    {
        if (Keyboard.current == null || cameraTransform == null)
            return;

        // 1. Ground Detection
        CheckGround();

        // 2. Reset jumps when landing
        if (isGrounded && verticalVelocity <= 0f)
        {
            verticalVelocity = -2f;
            jumpsRemaining = maxJumps;
        }

        // 3. WASD Input
        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) input.y += 1;
        if (Keyboard.current.sKey.isPressed) input.y -= 1;
        if (Keyboard.current.aKey.isPressed) input.x -= 1;
        if (Keyboard.current.dKey.isPressed) input.x += 1;

        input = Vector2.ClampMagnitude(input, 1f);

        // 4. Camera-relative movement
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * input.y + right * input.x).normalized;

        // 5. Adapt movement to terrain slope
        if (isGrounded)
        {
            move = Vector3.ProjectOnPlane(
                move, groundNormal
            ).normalized;
        }

       
// 6. Smooth character rotation
if (input.sqrMagnitude > 0.01f)
{
    Quaternion targetRotation = Quaternion.LookRotation(move);

    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        targetRotation,
        rotationSpeed * Time.deltaTime
    );
}


        // 7. Double Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame
            && jumpsRemaining > 0)
        {
            verticalVelocity = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );

            jumpsRemaining--;
        }

        // 8. Gravity
        verticalVelocity += gravity * Time.deltaTime;

        // 9. Apply movement
        Vector3 velocity = move * moveSpeed * GameManager.Instance.SpeedMultiplier;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void CheckGround()
    {
        Bounds bounds = controller.bounds;

        Vector3 origin = bounds.center;
        float rayDistance = bounds.extents.y + groundCheckDistance;

        RaycastHit hit;

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out hit,
            rayDistance,
            groundMask,
            QueryTriggerInteraction.Ignore))
        {
            float slopeAngle = Vector3.Angle(
                hit.normal, Vector3.up
            );

            isGrounded =
                slopeAngle <= controller.slopeLimit + 1f;

            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = controller.isGrounded;
            groundNormal = Vector3.up;
        }
    }
}
