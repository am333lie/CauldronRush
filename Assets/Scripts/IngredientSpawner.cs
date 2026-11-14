using System.Collections.Generic;
using UnityEngine;


public class IngredientSpawner : MonoBehaviour
{
    [System.Serializable]
    public class IngredientPrefab
    {
        public IngredientType type;
        public GameObject prefab;
    }

    [Tooltip("One entry per ingredient type. Types without a prefab spawn as a coloured sphere.")]
    [SerializeField] private IngredientPrefab[] prefabs;

    [Tooltip("Random ingredients spawned in addition to the ones on the recipe.")]
    [SerializeField] private int extraIngredients = 4;

    [Header("Placement")]
    [SerializeField] private bool snapToGround = true;
    [SerializeField] private float heightAboveGround = 0.5f;
    [SerializeField] private LayerMask groundMask = ~0;

    private void Start()
    {
    
        Spawn();
    }

    private void Spawn()
    {
        // 1. Collect and shuffle the spawn points
        var points = new List<Transform>();
        foreach (Transform child in transform) points.Add(child);
        Shuffle(points);

        // 2. What to spawn: recipe ingredients first, then random extras
        var toSpawn = new List<IngredientType>();

        if (GameManager.Instance != null)
        {
            foreach (var pair in GameManager.Instance.Required)
                for (int i = 0; i < pair.Value; i++)
                    toSpawn.Add(pair.Key);
        }

        var all = (IngredientType[])System.Enum.GetValues(typeof(IngredientType));
        for (int i = 0; i < extraIngredients; i++)
            toSpawn.Add(all[Random.Range(0, all.Length)]);

        if (toSpawn.Count > points.Count)
        {
            Debug.LogWarning($"IngredientSpawner: {toSpawn.Count} ingredients but only " +
                             $"{points.Count} spawn points. Add more child objects.", this);
        }

        // 3. Spawn (recipe ingredients come first, so they always get a point)
        int count = Mathf.Min(toSpawn.Count, points.Count);
        for (int i = 0; i < count; i++)
            SpawnAt(points[i], toSpawn[i]);
    }

    private void SpawnAt(Transform point, IngredientType type)
    {
        Vector3 position = point.position;

        if (snapToGround)
        {
            Vector3 origin = position + Vector3.up * 0.5f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 20f,
                    groundMask, QueryTriggerInteraction.Ignore))
            {
                position = hit.point + Vector3.up * heightAboveGround;
            }
        }

        Quaternion randomTurn = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject prefab = FindPrefab(type);
        GameObject instance = prefab != null
            ? Instantiate(prefab, position, randomTurn * prefab.transform.rotation)
            : CreatePlaceholder(type, position);

        instance.name = "Ingredient_" + type;

        var pickup = instance.GetComponent<IngredientPickup>();
        if (pickup == null) pickup = instance.AddComponent<IngredientPickup>();
        pickup.SetType(type);

        // Move the object so the visible model (not its pivot) sits on the spawn point
        instance.transform.position += position - pickup.WorldCenter;
    }

    private GameObject FindPrefab(IngredientType type)
    {
        if (prefabs == null) return null;
        foreach (var entry in prefabs)
            if (entry.type == type && entry.prefab != null) return entry.prefab;
        return null;
    }

    // Coloured sphere, used until a real prefab is assigned for this type
    private GameObject CreatePlaceholder(IngredientType type, Vector3 position)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * 0.5f;

        var collider = sphere.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 1.5f; // 0.75 m in the world because of the 0.5 scale

        int typeCount = System.Enum.GetValues(typeof(IngredientType)).Length;
        sphere.GetComponent<Renderer>().material.color =
            Color.HSVToRGB((int)type / (float)typeCount, 0.8f, 1f);

        return sphere;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // Shows the spawn points in the Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        foreach (Transform child in transform)
            Gizmos.DrawWireSphere(child.position, 0.4f);
    }
}
