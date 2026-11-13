using UnityEngine;

public class CauldronUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    public bool IsOpen { get; private set; }

    private CursorLockMode previousLockMode;
    private bool previousCursorVisible;

    private void Start()
    {
        panel.SetActive(false);
    }

    private void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    public void Open()
    {
        previousLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        panel.SetActive(true);
        IsOpen = true;
    }

    // Also called by the close button
    public void Close()
    {
        panel.SetActive(false);
        IsOpen = false;

        Cursor.lockState = previousLockMode;
        Cursor.visible = previousCursorVisible;
    }
}
