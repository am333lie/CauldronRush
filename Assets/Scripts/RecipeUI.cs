using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class RecipeUI : MonoBehaviour
{
    [SerializeField] private RectTransform recipePanel;
    [SerializeField] private TMP_Text recipeText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject hintObject;

    [Header("Animation")]
    [SerializeField] private Vector2 shownPosition = new Vector2(-720f, 330f);
    [SerializeField] private Vector2 hiddenPosition = new Vector2(-1300f, 330f);
    [SerializeField] private float revealScale = 1.3f;
    [SerializeField] private float shownScale = 1f;
    [SerializeField] private float showDuration = 3f;
    [SerializeField] private float slideDuration = 0.6f;
    [SerializeField] private KeyCode toggleKey = KeyCode.L;

    private bool introFinished;
    private bool isShown;
    private Coroutine slideRoutine;

    private void Start()
    {
        GameManager.Instance.OnRecipeChanged += RefreshRecipeText;
        RefreshRecipeText();
        if (hintObject != null) hintObject.SetActive(false);
        StartCoroutine(IntroRoutine());
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

        if (introFinished && Input.GetKeyDown(toggleKey))
        {
             Debug.Log("L pressed, isShown will be: " + !isShown);
            isShown = !isShown;
            if (slideRoutine != null) StopCoroutine(slideRoutine);
            slideRoutine = StartCoroutine(SlideTo(
                isShown ? shownPosition : hiddenPosition, shownScale, slideDuration));
        }
    }

    private void RefreshRecipeText()
    {
        var sb = new StringBuilder("Recipe\n");
        foreach (var pair in GameManager.Instance.Required)
        {
            int have = GameManager.Instance.Collected[pair.Key];
            string line = $"{pair.Key} {have}/{pair.Value}";
            sb.AppendLine(have >= pair.Value ? $"<color=#7CFC00>{line}</color>" : line);
        }
        recipeText.text = sb.ToString();
    }

    private IEnumerator IntroRoutine()
    {
        // Start big in the center
        recipePanel.anchoredPosition = Vector2.zero;
        recipePanel.localScale = Vector3.one * revealScale;

        yield return new WaitForSeconds(showDuration);

        // Slide out of the screen
        yield return SlideTo(hiddenPosition, shownScale, 1f);

        introFinished = true;
        if (hintObject != null) hintObject.SetActive(true);
        GameManager.Instance.BeginPlaying();
    }

    private IEnumerator SlideTo(Vector2 targetPos, float targetScale, float duration)
    {
        Vector2 startPos = recipePanel.anchoredPosition;
        float startScale = recipePanel.localScale.x;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float e = Mathf.SmoothStep(0f, 1f, t);
            recipePanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, e);
            recipePanel.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, e);
            yield return null;
        }
    }
}