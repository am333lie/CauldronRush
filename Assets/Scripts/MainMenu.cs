using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Loads the main game level
    public void PlayGame()
    {
        SceneManager.LoadScene("MainLevel");
    }

    
}