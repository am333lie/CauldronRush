using System.Collections.Generic;
using UnityEngine;

// Put this on the Player. Holds the ingredients the witch is carrying.
public class PlayerInventory : MonoBehaviour
{
    private readonly Dictionary<IngredientType, int> items = new Dictionary<IngredientType, int>();

    public event System.Action OnChanged;

    public IReadOnlyDictionary<IngredientType, int> Items => items;

    public void Add(IngredientType type)
    {
        items[type] = Count(type) + 1;
        OnChanged?.Invoke();
    }

    public int Count(IngredientType type)
    {
        return items.TryGetValue(type, out int amount) ? amount : 0;
    }

    // Returns false if the player does not carry this ingredient
    public bool Remove(IngredientType type)
    {
        if (Count(type) <= 0) return false;

        items[type]--;
        OnChanged?.Invoke();
        return true;
    }
}
