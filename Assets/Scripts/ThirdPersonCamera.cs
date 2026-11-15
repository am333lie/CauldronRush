
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Position")]
    public float distance = 9f;
    public float minDistance = 2f;
    public float maxDistance = 14f;
    public float lookHeight = 1.8f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    public float minPitch = -20f;
    public float maxPitch = 65f;

    [Header("Smoothing")]
    public float positionSmoothness = 8f;
    public float rotationSmoothness = 8f;
    public float followSpeed = 5f;

    [Header("Zoom")]
    public float zoomSpeed = 0.025f;

    private float yaw;
    private float pitch = 20f;
    private float currentYaw;
    private float yawVelocity;

    private bool wasMoving;

    void Start()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
            currentYaw = yaw;
        }
    }

    void LateUpdate()
    {
        if (target == null || Mouse.current == null)
            return;

        bool rotatingCamera = Mouse.current.rightButton.isPressed;

        // Check if the witch is moving
        Vector3 movement = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

        bool isMoving = movement.sqrMagnitude > 0.01f;

        // Rotate camera while holding right mouse button
        if (rotatingCamera)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            yaw += mouseDelta.x * mouseSensitivity * 0.1f;
            pitch -= mouseDelta.y * mouseSensitivity * 0.1f;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        // Only return behind the witch while she is moving
        else if (isMoving)
        {
            yaw = target.eulerAngles.y;

            pitch = Mathf.Lerp(
                pitch,
                20f,
                followSpeed * Time.deltaTime
            );
        }

        // Smoothly follow the desired yaw
        currentYaw = Mathf.SmoothDampAngle(
            currentYaw,
            yaw,
            ref yawVelocity,
            rotatingCamera ? 0.05f : 0.2f
        );

        // Zoom
        float scroll = Mouse.current.scroll.ReadValue().y;

        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Calculate camera rotation
        Quaternion cameraRotation = Quaternion.Euler(
            pitch,
            currentYaw,
            0f
        );

        // Calculate camera position
        Vector3 pivot = target.position + Vector3.up * lookHeight;

        Vector3 desiredPosition = pivot +
            cameraRotation * new Vector3(0, 0, -distance);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionSmoothness * Time.deltaTime
        );

        // Look at witch
        Quaternion desiredRotation = Quaternion.LookRotation(
            pivot - transform.position
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSmoothness * Time.deltaTime
        );
    }
}
