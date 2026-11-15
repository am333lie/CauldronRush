using UnityEngine;

public class KeySpawner : MonoBehaviour
{
    [SerializeField] private GameObject keyPrefab;

    private void Start()
    {
        if (transform.childCount == 0 || keyPrefab == null) return;

        // The key appears at one random spawn point
        Transform point = transform.GetChild(Random.Range(0, transform.childCount));
        Instantiate(keyPrefab, point.position, point.rotation);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (Transform child in transform)
            Gizmos.DrawWireSphere(child.position, 0.4f);
    }
}