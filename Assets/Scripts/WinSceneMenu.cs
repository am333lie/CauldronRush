using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinSceneMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (timeText != null)
        {
            int total = Mathf.FloorToInt(GameManager.LastElapsedTime);
            timeText.text = $"Time: {total / 60:00}:{total % 60:00}";
        }
    }

    // Called by the button
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}