
using UnityEngine;

public class WitchBob : MonoBehaviour
{
    [Header("Idle Floating")]
    public float idleAmplitude = 0.015f;
    public float idleFrequency = 0.35f;

    [Header("Transition")]
    public float smoothness = 3f;

    private Vector3 startPosition;
    private CharacterController playerController;
    private float movementAmount;
    private float animationTime;
    private bool initialized;

    void Start()
    {
        playerController = GetComponentInParent<CharacterController>();
    }

    void LateUpdate()
    {
        if (!initialized)
        {
            startPosition = transform.localPosition;
            initialized = true;
        }

        float speed = 0f;

        if (playerController != null)
        {
            Vector3 velocity = playerController.velocity;
            velocity.y = 0f;
            speed = velocity.magnitude;
        }

        // Only float while standing still
        float targetMovement = speed > 0.1f ? 0f : 1f;

        movementAmount = Mathf.Lerp(
            movementAmount,
            targetMovement,
            smoothness * Time.deltaTime
        );

        animationTime += Time.deltaTime * idleFrequency;

        float bob = Mathf.Sin(
            animationTime * Mathf.PI * 2f
        ) * idleAmplitude;

        // Fade floating in and out smoothly
        float finalBob = bob * movementAmount;

        transform.localPosition = startPosition
            + Vector3.up * finalBob;
    }
}
