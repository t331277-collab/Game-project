using UnityEngine;
using UnityEngine.SceneManagement;

public class Go_Stage1 : MonoBehaviour
{

    public void Awake()
    {
        Time.timeScale = 1.0f;
    }
    public void StartGame()
    {
        MainScoreManager.Instance.ClearMainScore();
        MainScoreManager.Instance.ClearScore();

        ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene("Stage1"); // (æ¿ ¿Ã∏ß »Æ¿Œ!)
        


    }
}
