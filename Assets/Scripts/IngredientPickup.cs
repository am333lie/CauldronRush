using UnityEngine;

// Sits on every ingredient in the world. The spawner adds it automatically
// if the prefab does not have it. Walking into it puts it in the inventory.
public class IngredientPickup : MonoBehaviour
{
    [SerializeField] private IngredientType type;
    [SerializeField] private float spinSpeed = 60f;
    [Tooltip("Size of the automatic pickup trigger, in metres.")]
    [SerializeField] private float pickupRadius = 0.75f;

    // Middle of the visible model, in local space. Imported models often
    // have their pivot far away from the mesh, so we never use the pivot.
    private Vector3 localCenter;

    public IngredientType Type => type;

    // Middle of the visible model in the world
    public Vector3 WorldCenter => transform.TransformPoint(localCenter);

    public void SetType(IngredientType newType)
    {
        type = newType;
    }

    private void Awake()
    {
        // Find the middle of everything that is drawn
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
            localCenter = transform.InverseTransformPoint(bounds.center);
        }

        // Make sure there is a trigger collider to walk into
        bool hasTrigger = false;
        foreach (var c in GetComponentsInChildren<Collider>())
            if (c.isTrigger) hasTrigger = true;

        if (!hasTrigger)
        {
            Vector3 s = transform.lossyScale;
            float scale = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
            if (scale < 0.0001f) scale = 1f;

            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.center = localCenter;
            sphere.radius = pickupRadius / scale; // stays the same size in the world
        }
    }

    private void Update()
    {
        // Spin around the middle of the model, not around its pivot
        transform.RotateAround(WorldCenter, Vector3.up, spinSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        var inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory != null) inventory.Add(type);
        else Debug.LogWarning("Picked up " + type + " but the Player has no PlayerInventory component.");

        Destroy(gameObject);
    }
}
