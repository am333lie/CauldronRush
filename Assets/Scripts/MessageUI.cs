using System.Collections;
using TMPro;
using UnityEngine;

public class MessageUI : MonoBehaviour
{
    public static MessageUI Instance { get; private set; }

    [SerializeField] private TMP_Text messageText;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float fadeDuration = 0.4f;

    private Coroutine routine;

    private void Awake()
    {
        Instance = this;
        group.alpha = 0f;
    }

    public void Show(string text, float duration = 3f)
    {
        messageText.text = text;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(duration));
    }

    private IEnumerator ShowRoutine(float duration)
    {
        group.alpha = 1f;
        yield return new WaitForSeconds(duration);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            group.alpha = 1f - t;
            yield return null;
        }
        group.alpha = 0f;
    }
}