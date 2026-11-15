using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { ShowingRecipe, Playing, Won, Lost }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float timeLimit = 300f;
    [SerializeField] private int minIngredients = 3;
    [SerializeField] private int maxIngredients = 4;
    [SerializeField] private float wrongIngredientPenalty = 10f;

    public GameState State { get; private set; } = GameState.ShowingRecipe;
    public float TimeRemaining { get; private set; }
    public Dictionary<IngredientType, int> Required { get; } = new Dictionary<IngredientType, int>();
    public Dictionary<IngredientType, int> Collected { get; } = new Dictionary<IngredientType, int>();
    public List<IngredientType> Inventory { get; } = new List<IngredientType>();
    public event System.Action OnInventoryChanged;

    public event System.Action OnRecipeChanged;
    public event System.Action<GameState> OnStateChanged;

    public bool HasKey { get; private set; }

    public float SpeedMultiplier { get; private set; } = 1f;
    private float boostTimeLeft;

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        SpeedMultiplier = multiplier;
        boostTimeLeft = duration;
    }

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        TimeRemaining = timeLimit;
        GenerateRecipe();
    }

    private void Update()
    {
        if (boostTimeLeft > 0f)
{
        boostTimeLeft -= Time.deltaTime;
        if (boostTimeLeft <= 0f) SpeedMultiplier = 1f;
}
        if (State != GameState.Playing) return;

        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            SetState(GameState.Lost);
        }
    }

    // Picks a random set of different ingredients
    private void GenerateRecipe()
    {
        var all = (IngredientType[])System.Enum.GetValues(typeof(IngredientType));

        // Shuffle 
        for (int i = all.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (all[i], all[j]) = (all[j], all[i]);
        }

        int count = Mathf.Min(Random.Range(minIngredients, maxIngredients + 1), all.Length);
        for (int i = 0; i < count; i++)
        {
            Required[all[i]] = 1;
            Collected[all[i]] = 0;
        }
    }

    // Called by the RecipeUI after the recipe slid into the corner
    public void BeginPlaying()
    {
        SetState(GameState.Playing);
    }

    
    // Called by an ingredient pickup. Everything goes into the bag, even wrong ones.
public void AddToInventory(IngredientType type)
{
    if (State != GameState.Playing) return;

    Inventory.Add(type);

    // The recipe list shows what you already carry
    if (Required.ContainsKey(type) && Collected[type] < Required[type])
    {
        Collected[type]++;
        OnRecipeChanged?.Invoke();
    }
    OnInventoryChanged?.Invoke();
}

// Called by the brew button. Wins if exactly the recipe ingredients are offered.
public bool TryBrew(List<IngredientType> offered)
{
    if (State != GameState.Playing) return false;

    bool correct = offered.Count == Required.Count;
    if (correct)
    {
        foreach (var pair in Required)
        {
            if (!offered.Contains(pair.Key)) { correct = false; break; }
        }
    }

    if (correct) SetState(GameState.Won);
    else TimeRemaining = Mathf.Max(0f, TimeRemaining - wrongIngredientPenalty);

    return correct;
}

    public void CaughtByEnemy()
    {
        if (State == GameState.Playing) SetState(GameState.Lost);
    }

    private bool IsRecipeComplete()
    {
        foreach (var pair in Required)
            if (Collected[pair.Key] < pair.Value) return false;
        return true;
    }

    private void SetState(GameState newState)
{
    State = newState;
    OnStateChanged?.Invoke(newState);

    if (newState == GameState.Lost)
        SceneManager.LoadScene("GameOver");
}

public void CollectKey()
{
    HasKey = true;
}
}