using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class IngredientIcon
{
    public IngredientType type;
    public Sprite sprite;
}

public class CauldronUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Inventory and slots")]
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private CauldronSlot[] slots;
    [SerializeField] private DraggableIngredient itemPrefab;
    [SerializeField] private IngredientIcon[] icons;

    [Header("Brewing")]
    [SerializeField] private Button brewButton;
    [SerializeField] private TMP_Text feedbackText;

    public bool IsOpen { get; private set; }

    private CursorLockMode previousLockMode;
    private bool previousCursorVisible;

    private void Start()
    {
        panel.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape)) Close();

        // Brewing is possible once as many slots are filled as the recipe needs
        brewButton.interactable = GetOfferedIngredients().Count == GameManager.Instance.Required.Count;
    }

    public void Open()
    {
        previousLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        panel.SetActive(true);
        IsOpen = true;
        feedbackText.text = "";
        RebuildItems();
    }

    // Also called by the close button
    public void Close()
    {
        panel.SetActive(false);
        IsOpen = false;

        Cursor.lockState = previousLockMode;
        Cursor.visible = previousCursorVisible;
    }

    // Also called by the brew button
    public void Brew()
    {
        bool success = GameManager.Instance.TryBrew(GetOfferedIngredients());
        feedbackText.text = success ? "Potion complete!" : "Wrong ingredients! Time penalty.";
    }

    private List<IngredientType> GetOfferedIngredients()
    {
        var offered = new List<IngredientType>();
        foreach (var slot in slots)
            if (slot.Item != null) offered.Add(slot.Item.Type);
        return offered;
    }

    // Creates one UI item per ingredient the witch carries. Slots start empty.
    private void RebuildItems()
    {
        foreach (var slot in slots)
            if (slot.Item != null) Destroy(slot.Item.gameObject);
        foreach (Transform child in inventoryContainer)
            Destroy(child.gameObject);

        foreach (var type in GameManager.Instance.Inventory)
        {
            var item = Instantiate(itemPrefab, inventoryContainer);
            item.Setup(type, GetIcon(type), panel.transform);
        }
    }

    private Sprite GetIcon(IngredientType type)
    {
        foreach (var entry in icons)
            if (entry.type == type) return entry.sprite;
        return null;
    }
}