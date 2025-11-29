using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // [추가] TextMeshPro를 사용하기 위해 꼭 필요합니다.

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    // [추가] 에디터에서 TextMeshPro UI를 여기에 드래그해서 넣으세요.
    public TMP_Text scoreText;

    public TMP_Text scoreString;

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

    // [추가] 게임 시작 시 초기 점수(0)를 화면에 표시
    private void Start()
    {
        UpdateScoreUI();
    }

    public int Count_Damaged = 0;

    public void TakeDamaged(int amount)
    {
        Count_Damaged += amount;
        Debug.Log("현재 피격횟수 " + Count_Damaged);

        // [추가] 점수가 바뀌었으니 UI도 갱신
        UpdateScoreUI();
    }

    public void ResetScore()
    {
        Count_Damaged = 0;

        // [추가] 리셋되었으니 UI도 0으로 갱신
        UpdateScoreUI();
    }

    // [추가] 실제 텍스트를 변경하는 함수
    private void UpdateScoreUI()
    {
        // scoreText가 연결되어 있을 때만 실행 (에러 방지)
        if (scoreText != null)
        {
            // 방법 1: 가장 단순한 방법
            // scoreText.text = "Damaged: " + Count_Damaged.ToString();

            // 방법 2: 성능 최적화 (권장)
            scoreText.SetText("{0}", 100 - Count_Damaged);
        }

        if(Count_Damaged > 30)
        {
            scoreString.SetText("코멘트 : 형편없군!!!");
        }
        else if(Count_Damaged > 15)
        {
            scoreString.SetText("코멘트 : 나쁘지 않은데!!!!");
        }
        else if(Count_Damaged > 1)
        {
            scoreString.SetText("코멘트 : 좋아 이대로만!!!!");
        }
        else
        {
            scoreString.SetText("코멘트 : 너 정체가 뭐야!!!!");
        }
    }

    public void Ending()
    {
        if (Count_Damaged > 30)
        {
            SceneManager.LoadScene("BadEnding");
        }
        else if (Count_Damaged > 15)
        {
            SceneManager.LoadScene("NormalEnding");
        }
        else if (Count_Damaged > 0)
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