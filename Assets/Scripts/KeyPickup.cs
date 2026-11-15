using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 90f;
    [SerializeField] private float pickupRadius = 1f;
    [SerializeField] private string pickupMessage = "I wonder if I can use this key somewhere...";

    private Vector3 localCenter;

    private Vector3 WorldCenter => transform.TransformPoint(localCenter);

    private void Awake()
    {
        // Middle of the visible model, because imported pivots are often off
        var renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            localCenter = transform.InverseTransformPoint(bounds.center);
        }

        bool hasTrigger = false;
        foreach (var c in GetComponentsInChildren<Collider>())
            if (c.isTrigger) hasTrigger = true;

        if (!hasTrigger)
        {
            float scale = Mathf.Max(0.0001f, transform.lossyScale.x);
            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.center = localCenter;
            sphere.radius = pickupRadius / scale;
        }
    }

    private void Update()
    {
        transform.RotateAround(WorldCenter, Vector3.up, spinSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance.State != GameState.Playing) return;

        GameManager.Instance.CollectKey();
        if (MessageUI.Instance != null) MessageUI.Instance.Show(pickupMessage);
        Destroy(gameObject);
    }
}