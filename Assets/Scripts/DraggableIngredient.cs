using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableIngredient : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    public IngredientType Type { get; private set; }

    private CanvasGroup group;
    private RectTransform rect;
    private Transform dragLayer;
    private Transform previousParent;
    private bool previousFill;
    private bool isFilling;

    public void Setup(IngredientType type, Sprite sprite, Transform dragLayer)
    {
        Type = type;
        this.dragLayer = dragLayer;
        group = GetComponent<CanvasGroup>();
        rect = (RectTransform)transform;

        if (icon != null && sprite != null) icon.sprite = sprite;
        if (label != null) label.text = type.ToString();
    }

    // Moves the item into a slot (fill = true) or back into the inventory grid (fill = false)
    public void PlaceIn(Transform parent, bool fill)
    {
        transform.SetParent(parent, false);
        isFilling = fill;
        if (fill)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 8f);
            rect.offsetMax = new Vector2(-8f, -8f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        previousParent = transform.parent;
        previousFill = isFilling;
        Vector2 size = rect.rect.size;

        transform.SetParent(dragLayer, false);
        transform.SetAsLastSibling();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        // Lets the slot underneath receive the drop
        group.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        group.blocksRaycasts = true;

        // Nobody accepted the drop: go back to where we came from
        if (transform.parent == dragLayer)
            PlaceIn(previousParent, previousFill);
    }
}