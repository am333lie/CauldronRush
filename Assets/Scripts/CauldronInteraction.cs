using System.Collections.Generic;
using UnityEngine;

public class CauldronInteraction : MonoBehaviour
{
    [SerializeField] private CauldronUI cauldronUI;
    [SerializeField] private Color highlightColor = new Color(1f, 0.6f, 0.1f);
    [SerializeField] private float highlightIntensity = 1.5f;
    [SerializeField] private float interactDistance = 6f;

    private readonly List<Material> materials = new List<Material>();
    private Camera cam;
    private Transform player;
    private bool highlighted;

    private void Awake()
    {
        // Material instances of the cauldron (and all children)
        foreach (var r in GetComponentsInChildren<Renderer>())
            materials.AddRange(r.materials);
    }

    private void Start()
    {
        cam = Camera.main;
        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    private void Update()
    {
        bool hovering = IsMouseOverCauldron() && IsPlayerClose()
                        && !cauldronUI.IsOpen
                        && GameManager.Instance.State == GameState.Playing;

        if (hovering != highlighted) SetHighlight(hovering);

        if (hovering && Input.GetMouseButtonDown(0))
            cauldronUI.Open();
    }

    private bool IsMouseOverCauldron()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, 200f) && hit.transform.IsChildOf(transform);
    }

    private bool IsPlayerClose()
    {
        if (player == null) return true;
        return Vector3.Distance(player.position, transform.position) <= interactDistance;
    }

    private void SetHighlight(bool on)
    {
        highlighted = on;
        Color color = on ? highlightColor * highlightIntensity : Color.black;
        foreach (var m in materials)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color);
        }
    }
}