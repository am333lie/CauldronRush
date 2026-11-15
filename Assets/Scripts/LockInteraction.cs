using System.Collections.Generic;
using UnityEngine;

public class LockInteraction : MonoBehaviour
{
    [Tooltip("Everything that disappears when the gate opens (bars and the lock itself)")]
    [SerializeField] private GameObject[] objectsToHide;
    [SerializeField] private Color highlightColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private float highlightIntensity = 1.5f;
    [SerializeField] private float interactDistance = 6f;

    private readonly List<Material> materials = new List<Material>();
    private Camera cam;
    private Transform player;
    private CauldronUI cauldronUI;
    private bool highlighted;

    private void Awake()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            materials.AddRange(r.materials);
    }

    private void Start()
    {
        cam = Camera.main;
        var playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        cauldronUI = FindFirstObjectByType<CauldronUI>();
    }

    private void Update()
    {
        bool hovering = GameManager.Instance.State == GameState.Playing
                        && (cauldronUI == null || !cauldronUI.IsOpen)
                        && IsMouseOverLock()
                        && IsPlayerClose();

        if (hovering != highlighted) SetHighlight(hovering);

        if (hovering && Input.GetMouseButtonDown(0))
            Interact();
    }

    private void Interact()
    {
        if (!GameManager.Instance.HasKey)
        {
            if (MessageUI.Instance != null) MessageUI.Instance.Show("I need a key.", 2f);
            return;
        }

        if (MessageUI.Instance != null) MessageUI.Instance.Show("The gate is open!", 2f);
        foreach (var obj in objectsToHide) obj.SetActive(false);
        // This script is on the lock, so it stops here once the lock is hidden
    }

    private bool IsMouseOverLock()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, 200f) && hit.transform.IsChildOf(transform);
    }

    private bool IsPlayerClose()
    {
        return player == null || Vector3.Distance(player.position, transform.position) <= interactDistance;
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