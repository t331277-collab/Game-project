using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScoreManager : MonoBehaviour
{
    public static MainScoreManager Instance;

    public int Count_All_Damaged;

    public int Current_Score;

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

    public void ClearMainScore()
    {
        Current_Score = 0;
    }

    public void CountALLDamage()
    {
        Count_All_Damaged++;
          
    }

    public void CurrentScore()
    {
        Current_Score = Count_All_Damaged;
    }

    public void Ending()
    {
       
        
        if (Count_All_Damaged > 20)
        {
            SceneManager.LoadScene("NormalEnding");
        }
        else if (Count_All_Damaged > 10)
        {
            SceneManager.LoadScene("GoodEnding");
        }
        else
        {
            SceneManager.LoadScene("HiddenEnding");
        }

        Time.timeScale = 1f;
    }

    public void ReTry(int Stagenum)
    {
        Count_All_Damaged = Current_Score;

        if (Stagenum== 0)
        {
            SceneManager.LoadScene("tutorial");
        }
        if(Stagenum == 1)
        {
            SceneManager.LoadScene("Stage1");
        }
        if (Stagenum == 2)
        {
            SceneManager.LoadScene("Stage2");
        }
        if (Stagenum == 3)
        {
            SceneManager.LoadScene("Boss_Stage");
        }
    }
}
