using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScoreManager : MonoBehaviour
{
    public static MainScoreManager Instance;

    public int Count_All_Damaged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void ClearScore()
    {
        Count_All_Damaged = 0;
    }

    public void CountALLDamage()
    {
        Count_All_Damaged++;
          
    }

    public void Ending()
    {
        if (Count_All_Damaged > 30)
        {
            SceneManager.LoadScene("BadEnding");
        }
        else if (Count_All_Damaged > 15)
        {
            SceneManager.LoadScene("NormalEnding");
        }
        else if (Count_All_Damaged > 0)
        {
            SceneManager.LoadScene("GoodEnding");
        }
        else
        {
            SceneManager.LoadScene("HiddenEnding");
        }

        Time.timeScale = 1f;
    }
}
