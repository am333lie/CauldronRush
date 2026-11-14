using UnityEngine;
using UnityEngine.EventSystems;

public class CauldronSlot : MonoBehaviour, IDropHandler
{
    public DraggableIngredient Item => GetComponentInChildren<DraggableIngredient>();

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        var item = eventData.pointerDrag.GetComponent<DraggableIngredient>();
        if (item == null || Item != null) return; // only one item per slot

        item.PlaceIn(transform, true);
    }
}