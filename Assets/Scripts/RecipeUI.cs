using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class RecipeUI : MonoBehaviour
{
    [SerializeField] private RectTransform recipePanel;
    [SerializeField] private TMP_Text recipeText;
    [SerializeField] private TMP_Text timerText;

    [Header("Animation")]
    [SerializeField] private Vector2 cornerPosition = new Vector2(-710f, 340f);
    [SerializeField] private float revealScale = 1.8f;
    [SerializeField] private float showDuration = 3f;
    [SerializeField] private float slideDuration = 1f;

    private void Start()
    {
        GameManager.Instance.OnRecipeChanged += RefreshRecipeText;
        RefreshRecipeText();
        StartCoroutine(RevealRoutine());
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRecipeChanged -= RefreshRecipeText;
    }

    private void Update()
    {
        float time = GameManager.Instance.TimeRemaining;
        int total = Mathf.CeilToInt(time);
        timerText.text = $"{total / 60:00}:{total % 60:00}";
        timerText.color = time <= 30f ? Color.red : Color.white;
    }

    private void RefreshRecipeText()
    {
        var sb = new StringBuilder("Recipe\n");
        foreach (var pair in GameManager.Instance.Required)
        {
            int have = GameManager.Instance.Collected[pair.Key];
            string line = $"{pair.Key} {have}/{pair.Value}";
            // Completed ingredients turn green
            sb.AppendLine(have >= pair.Value ? $"<color=#7CFC00>{line}</color>" : line);
        }
        recipeText.text = sb.ToString();
    }

    private IEnumerator RevealRoutine()
    {
        // Start big in the center of the screen
        recipePanel.anchoredPosition = Vector2.zero;
        recipePanel.localScale = Vector3.one * revealScale;

        yield return new WaitForSeconds(showDuration);

        // Slide and shrink into the corner
        Vector2 startPos = recipePanel.anchoredPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slideDuration;
            float e = Mathf.SmoothStep(0f, 1f, t);
            recipePanel.anchoredPosition = Vector2.Lerp(startPos, cornerPosition, e);
            recipePanel.localScale = Vector3.one * Mathf.Lerp(revealScale, 1f, e);
            yield return null;
        }

        GameManager.Instance.BeginPlaying();
    }
}