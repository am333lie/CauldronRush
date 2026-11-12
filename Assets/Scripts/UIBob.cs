using UnityEngine;

public class UIBob : MonoBehaviour
{
    [SerializeField] private float amplitude = 15f; // How far it moves up and down (in UI pixels)
    [SerializeField] private float speed = 2f;      // How fast it bobs

    private RectTransform rectTransform;
    private Vector2 startPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        startPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * amplitude;
        rectTransform.anchoredPosition = startPosition + new Vector2(0f, offset);
    }
}