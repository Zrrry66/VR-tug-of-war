using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour
{
    private bool isPaused = false;

    public void PlayGame()
    {
        Debug.Log("▶ PlayGame() called");
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void PauseGame()
    {
        Debug.Log("⏸ PauseGame() called");
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void RestartGame()
    {
        Debug.Log("🔁 RestartGame() called");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
