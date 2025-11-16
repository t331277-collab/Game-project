using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Sound")]
    public AudioClip countThree;    // 3초 카운트 사운드
    public AudioClip Main_BGM;    // Main_BGM start

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 필요하면 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //노래를 잠시 멈춤
    public void PauseMain_BGM()
    {
        Debug.Log("Stop");
        SoundManager.instance.PauseBGM();
    }

    //노래 다시 시작
    public void ResumeMain_BGM()
    {
        Debug.Log("Resume");
        SoundManager.instance.ResumeBGM();
    }

    // Ingame_UI에서 호출할 함수
    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    private System.Collections.IEnumerator StartGameRoutine()
    {
        //약간의 텀

        yield return new WaitForSecondsRealtime(0.5f);

        // 1) 카운트 사운드 재생
        if (countThree != null)
        {
            SoundManager.instance.SFXPlay("Count3", countThree);
            // timeScale = 0이어도 상관 없게 Realtime 사용
            yield return new WaitForSecondsRealtime(countThree.length);
        }

        // 2) 카운트 끝난 뒤 게임 시작
        Time.timeScale = 1f;
        Debug.Log("Game Start!!");
        SoundManager.instance.PlayBGM(Main_BGM);
    }
}
