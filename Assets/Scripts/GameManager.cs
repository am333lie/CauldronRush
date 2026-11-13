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

    public event System.Action OnRecipeChanged;
    public event System.Action<GameState> OnStateChanged;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        TimeRemaining = timeLimit;
        GenerateRecipe();
    }

    private void Update()
    {
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

    // Returns true if the ingredient was needed
    public bool CollectIngredient(IngredientType type)
    {
        if (State != GameState.Playing) return false;

        bool needed = Required.ContainsKey(type) && Collected[type] < Required[type];
        if (needed)
        {
            Collected[type]++;
            OnRecipeChanged?.Invoke();
            if (IsRecipeComplete()) SetState(GameState.Won);
        }
        else
        {
            TimeRemaining = Mathf.Max(0f, TimeRemaining - wrongIngredientPenalty);
        }
        return needed;
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
}