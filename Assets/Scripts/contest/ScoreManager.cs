using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    // 1. 다른 스크립트에서 쉽게 접근할 수 있는 정적(static) 변수
    public static ScoreManager Instance;

    // 2. 게임 오브젝트가 생성될 때 가장 먼저 실행되는 함수
    private void Awake()
    {
        // 인스턴스가 아직 없다면 (게임 시작 후 처음 로드된 경우)
        if (Instance == null)
        {
            Instance = this; // 자기 자신을 인스턴스에 할당
            DontDestroyOnLoad(gameObject); // 씬이 변경되어도 파괴되지 않도록 설정
        }
        else
        {
            // 이미 인스턴스가 존재한다면 (씬 이동 후 다시 로드되어 중복된 경우)
            Destroy(gameObject); // 새로 생긴 중복 오브젝트를 파괴하여 하나만 유지
        }
    }

    // --- 아래는 기능 테스트를 위한 예시 코드입니다 ---
    public int Count_Damaged = 0;

    public void TakeDamaged(int amount)
    {
        Count_Damaged += amount;
        Debug.Log("현재 피격횟수 " + Count_Damaged);
    }

    public void ResetScore()
    {
        Count_Damaged = 0;
    }

    public void Ending()
    {
        if(Count_Damaged > 30)
        {
            SceneManager.LoadScene("BadEnding");
        }
        else if(Count_Damaged > 15)
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