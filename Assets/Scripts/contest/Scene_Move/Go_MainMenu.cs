using UnityEngine;
using UnityEngine.SceneManagement;

public class Go_MainMenu : MonoBehaviour
{
    public void Awake()
    {
        Time.timeScale = 1.0f;
    }
    public void StartGame()
    {
        ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene("MainMenu"); // (æ¿ ¿Ã∏ß »Æ¿Œ!)
    }
}
