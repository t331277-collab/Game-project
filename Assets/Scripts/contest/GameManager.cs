using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool Hit = false;

    [Header("Judge Window (seconds)")]
    public float Perfect = 0.03f;  // 30ms
    public float Good = 0.07f;  // 70ms
    public float Normal = 0.10f;  // 100ms

    [Header("Beat Settings")]
    public float beatInterval = 1.05f; // 비트 간격 (1.05초마다 노트/박)
    public float firstBeatTime = 1.05f; // 첫 비트가 오는 시간 (SongTime 기준)

    double songStartDspTime;  // 곡이 시작한 dspTime
    double PausedspTime;

    // 3카운트 후 BGM이 시작할 때를 0초로 잡는 노래 시간
    public double SongTime => AudioSettings.dspTime - songStartDspTime;

    [Header("Sound")]
    public AudioClip countThree;    // 3초 카운트 사운드
    public AudioClip Main_BGM;      // Main_BGM start

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

    private void Update()
    {
        // 현재 경과된 시간 출력
        // Debug.Log($"SongTime = {SongTime:F3}");

        if (!Hit) return;  // Hit 아니면 바로 리턴

        Hit = false;       // 한 번만 판정하고 초기화

        JudgeHit(SongTime);
    }

    
    /// SongTime(노래 진행 시간) 기준으로 가장 가까운 비트를 찾고
    /// 그 오차로 Perfect / Good / Normal / Bad 판정
    
    void JudgeHit(double songTime)
    {
        double t = songTime;

        // 첫 비트 기준으로 현재 시간이 어느 정도 떨어져 있는지
        double local = t - firstBeatTime;

        // 아직 첫 비트도 오기 전에 너무 빠르게 쳤으면 Bad 처리
        if (local < -Normal)
        {
            Debug.Log($"Bad (too early), SongTime={t:F3}");
            return;
        }

        // 가장 가까운 비트 인덱스 계산 (0, 1, 2, 3, ...)
        int beatIndex = Mathf.RoundToInt((float)(local / beatInterval));

        // 그 비트가 실제로 발생하는 시간
        double nearestBeatTime = firstBeatTime + beatIndex * beatInterval;

        // 현재 시간과 비트 시간의 차이 (양수: 늦게, 음수: 빠르게)
        double delta = t - nearestBeatTime;
        double absDelta = System.Math.Abs(delta);

        Debug.Log($"HIT!!! SongTime={t:F3}, beatIndex={beatIndex}, " +
                  $"nearestBeat={nearestBeatTime:F3}, delta={delta:F3}");

        if (absDelta <= Perfect)
        {
            Calcul_Score("Perfect");
        }
        else if (absDelta <= Good)
        {
            Calcul_Score("Good");
        }
        else if (absDelta <= Normal)
        {
            Calcul_Score("Normal");
        }
        else
        {
            Debug.Log("Bad");
        }
    }

    public void Calcul_Score(string str)
    {
        if (str == "Perfect")
        {
            Debug.Log("Perfect!");
        }
        else if (str == "Good")
        {
            Debug.Log("Good");
        }
        else if (str == "Normal")
        {
            Debug.Log("Normal");
        }
    }

    // 키/버튼에서 이 함수를 호출해주면 됨
    public void Hit_ZXC()
    {
        Hit = true;
    }

    // 노래를 잠시 멈춤
    public void PauseMain_BGM()
    {
        Debug.Log("Stop");
        SoundManager.instance.PauseBGM();
        //정지한 시간을 저장
        PausedspTime = SongTime;
    }

    // 노래 다시 시작
    public void ResumeMain_BGM()
    {
        Debug.Log("Resume");
        SoundManager.instance.ResumeBGM();

        //다시 재생시 원래 시간대로 흘러감
        songStartDspTime = AudioSettings.dspTime - PausedspTime;
    }

    // Ingame_UI에서 호출할 함수
    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    private System.Collections.IEnumerator StartGameRoutine()
    {
        // 약간의 텀
        yield return new WaitForSecondsRealtime(0.5f);

        // 1) 카운트 사운드 재생
        if (countThree != null)
        {
            SoundManager.instance.SFXPlay("Count3", countThree);
            yield return new WaitForSecondsRealtime(countThree.length);
        }

        // 2) 카운트 끝난 뒤 게임 시작
        Time.timeScale = 1f;
        Debug.Log("Game Start!!");

        SoundManager.instance.PlayBGM(Main_BGM);
       
        songStartDspTime = AudioSettings.dspTime;
    }
}
