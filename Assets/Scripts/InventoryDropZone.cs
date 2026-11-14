using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        var item = eventData.pointerDrag.GetComponent<DraggableIngredient>();
        if (item != null) item.PlaceIn(transform, false);
    }
}