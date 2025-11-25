using UnityEngine;
using UnityEngine.UI;
using System.Collections; // 코루틴 사용을 위해 필수

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // [추가!] 현재 '시간 정지 스킬'이 활성화된 상태인지 확인하는 변수 (외부에서 읽기만 가능)
    public bool IsTimeStopped { get; private set; } = false;

    public bool IsGamePausedForMenu { get; private set; } = false;

    public bool Hit = false;

    public Slider MusicTimer;
    public float gameTimer;

    private bool stopTimer = true;

    // [추가!] 게임이 종료되었는지 확인하는 스위치 변수
    private bool isGameEnded = false;

    [Header("UI Settings")]
    [SerializeField] private GameObject scoreUIPanel; // 띄울 UI 패널

    private int enemyCount = 0; //enemy 변수

    [Header("Judge Window (seconds)")]
    public float Perfect = 0.03f;  // 30ms
    public float Good = 0.07f;  // 70ms
    public float Normal = 0.10f;  // 100ms

    [Header("Beat Settings")]
    public float beatInterval = 1.05f; // 비트 간격 (1.05초마다 노트/박)
    public float firstBeatTime = 1.05f; // 첫 비트가 오는 시간 (SongTime 기준)

    double songStartDspTime;  // 곡이 시작한 dspTime
    double PausedspTime;      // 일시정지 시점의 dspTime

    // 3카운트 후 BGM이 시작할 때를 0초로 잡는 노래 시간
    public double SongTime => AudioSettings.dspTime - songStartDspTime;

    [Header("Sound")]
    public AudioClip countThree;    // 3초 카운트 사운드
    public AudioClip Main_BGM;      // Main_BGM start

    // 현재 선택된 적의 리스트 인덱스 번호
    private int selectedEnemyIndex = 0;
    // 소리가 재생되는 동안 입력을 막기 위한 변수
    private bool isWaitingForSoundToFinish = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (MusicTimer != null && Main_BGM != null)
        {
            MusicTimer.maxValue = Main_BGM.length;
            MusicTimer.value = 0f;
        }

        // 게임 시작 시 씬에 있는 "Enemy" 태그 오브젝트 수를 셈
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // 시작할 때 UI는 꺼둠
        if (scoreUIPanel != null)
            scoreUIPanel.SetActive(false);

        Debug.Log($"게임 시작! 적의 수: {enemyCount}");
    }

    private void Update()
    {   
        // [수정] 메뉴 때문에 게임이 멈췄다면 Update 로직을 수행하지 않음
        if (IsGamePausedForMenu) return;

        // 시간이 멈췄고, 아직 소리 재생 대기 중이 아니라면 -> 적 선택 입력 처리
        if (IsTimeStopped && !isWaitingForSoundToFinish)
        {
            HandleEnemySelectionInput();
        }

        // 시간이 멈춘 상태(스킬 중)이거나 타이머가 멈췄으면 아래 로직(타이머, 판정)은 실행 안 함
        if (IsTimeStopped || stopTimer) return;

        if (MusicTimer != null)
        {
            // 현재 노래 시간을 슬라이더에 반영
            MusicTimer.value = Mathf.Clamp((float)SongTime, 0f, MusicTimer.maxValue);

            // [수정!] 노래가 끝났고(!isGameEnded), 아직 종료 처리가 안 되었다면
            // (부동소수점 오차를 고려하여 == 대신 >= 사용 권장)
            if (MusicTimer.value >= MusicTimer.maxValue && !isGameEnded)
            {
                Debug.Log("Game End");
                
                // [핵심!] 스위치를 켜서 다음 프레임부터는 이 안으로 들어오지 못하게 막습니다.
                isGameEnded = true; 

                Time.timeScale = 0f;           // 시간을 멈춤
                SoundManager.instance.StopBGM(); // BGM 정지
                
                // (나중에 여기에 게임 오버 UI를 띄우는 코드를 넣으면 됩니다.)
            }
        }

        if (!Hit) return;  // Hit 아니면 바로 리턴

        Hit = false;       // 한 번만 판정하고 초기화

        JudgeHit(SongTime);


    }

    public void OnEnemyDestroyed()
    {
        enemyCount--;

        // 남은 적이 0 이하가 되면 UI 활성화
        if (enemyCount <= 0)
        {
            GameClear();
        }
    }

    private void GameClear()
    {
        Debug.Log("모든 적 처치 완료!");
        if (scoreUIPanel != null)
        {
            stopTimer = true;
            scoreUIPanel.SetActive(true);
            // 필요하다면 여기서 게임 일시정지 등을 추가
            Time.timeScale = 0f;
            SoundManager.instance.StopBGM();
        }
    }


    // --- [새로운 스킬 시스템 함수들] ---

    // [1] 스킬 발동 (Player_Attack에서 Shift 키 입력 시 호출)
    public void ActivateTimeStopSkill()
    {
        if (IsTimeStopped) return; // 이미 스킬 중이면 무시

        // [예외 처리] 화면에 보이는 적이 한 명도 없으면 스킬 발동 실패
        if (Enemy.VisibleEnemies.Count == 0)
        {
            Debug.Log("스킬 발동 실패: 화면에 보이는 적이 없습니다!");
            return;
        }

        Debug.Log("시간 정지! 적 선택 모드 시작.");
        IsTimeStopped = true;    // 상태 변경
        PauseMain_BGM();         // BGM 일시정지 (dspTime 저장)
        Time.timeScale = 0f;     // 시간 완전 정지
        
        // 입력 잠금 해제
        isWaitingForSoundToFinish = false;

        // 첫 번째 적 선택 및 시각 효과 초기화
        selectedEnemyIndex = 0;
        UpdateEnemySelectionVisuals();
    }

    // [NEW!] 적 선택 및 결정 입력 처리 함수 (Update에서 호출)
    private void HandleEnemySelectionInput()
    {
        // 화면에 보이는 적이 없거나 리스트가 비었으면 아무것도 안 함 (안전장치)
        if (Enemy.VisibleEnemies == null || Enemy.VisibleEnemies.Count == 0) return;

        // [오른쪽 방향키] 다음 적으로 이동
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // 현재 적 선택 해제 (색깔 복구)
            if(Enemy.VisibleEnemies[selectedEnemyIndex] != null) 
                Enemy.VisibleEnemies[selectedEnemyIndex].Deselect();
            
            // 인덱스 증가 (리스트 끝에 도달하면 처음으로 돌아감: 나머지 연산 %)
            selectedEnemyIndex = (selectedEnemyIndex + 1) % Enemy.VisibleEnemies.Count;
            
            // 새로운 적 선택 표시 (색깔 변경)
            UpdateEnemySelectionVisuals();
        }
        // [왼쪽 방향키] 이전 적으로 이동
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if(Enemy.VisibleEnemies[selectedEnemyIndex] != null)
                Enemy.VisibleEnemies[selectedEnemyIndex].Deselect();

            // 인덱스 감소 (음수가 되면 리스트 끝으로 보냄)
            selectedEnemyIndex--;
            if (selectedEnemyIndex < 0) selectedEnemyIndex = Enemy.VisibleEnemies.Count - 1;
            
            UpdateEnemySelectionVisuals();
        }
        // [스페이스바] 선택 확정 및 소리 재생
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"적 선택 확정! (인덱스: {selectedEnemyIndex})");
            
            // 선택된 적이 유효한지 확인
            if (Enemy.VisibleEnemies[selectedEnemyIndex] != null)
            {
                // 소리 재생 중 다른 입력 방지
                isWaitingForSoundToFinish = true;
                // 선택된 적에게 소리 재생 명령 (Enemy 스크립트의 함수 호출)
                Enemy.VisibleEnemies[selectedEnemyIndex].TriggerSkillSoundSequence();
            }
        }
    }

    // 현재 선택된 적만 강조 표시하는 보조 함수
    private void UpdateEnemySelectionVisuals()
    {
        // 혹시 모르니 모든 적의 선택 표시를 초기화 (안전장치)
        foreach (var enemy in Enemy.VisibleEnemies)
        {
            if (enemy != null) enemy.Deselect();
        }

        // 현재 인덱스의 적이 유효하면 선택 표시
        if (Enemy.VisibleEnemies.Count > 0 && Enemy.VisibleEnemies[selectedEnemyIndex] != null)
        {
            Enemy.VisibleEnemies[selectedEnemyIndex].Select();
        }
    }


    // [2] 스킬 종료 및 시간 재개 (Enemy에서 코루틴으로 호출)
    public IEnumerator ResumeTimeAfterDelay(float delay)
    {
        Debug.Log($"스킬 종료 대기 중... ({delay}초)");
        
        // [핵심!] 실시간(Realtime)으로 delay만큼 대기합니다.
        // (Time.timeScale이 0이어도 이 시간은 흐릅니다)
        yield return new WaitForSecondsRealtime(delay);

        Debug.Log("시간 재개!");
        Time.timeScale = 1f;     // 시간 정상화
        ResumeMain_BGM();        // BGM 재개 (dspTime 복구)

        IsTimeStopped = false;   // 상태 해제
        // 입력 잠금 해제
        isWaitingForSoundToFinish = false;

        // 모든 적 선택 표시 해제 (깔끔하게 정리)
        foreach (var enemy in Enemy.VisibleEnemies)
        {
             if(enemy != null) enemy.Deselect();
        }
    }


    // --- BGM 일시정지/재개 ---

    public void PauseMain_BGM()
    {
        // Debug.Log("Stop BGM");
        SoundManager.instance.PauseBGM();
        // 현재 노래가 진행된 시간(dspTime 기준)을 저장합니다.
        PausedspTime = SongTime;
        stopTimer = true;
    }

    public void ResumeMain_BGM()
    {
        // Debug.Log("Resume BGM");
        SoundManager.instance.ResumeBGM();
        
        // [핵심!] 저장했던 'PausedspTime'을 기준으로 'songStartDspTime'을 재설정합니다.
        // 이렇게 하면 'SongTime' 계산식 (현재시간 - 시작시간)이 
        // 멈췄던 시점부터 이어서 계산되게 됩니다.
        songStartDspTime = AudioSettings.dspTime - PausedspTime;

        stopTimer = false;
    }


    // --- 리듬 판정 및 게임 시작 ---

    void JudgeHit(double songTime)
    {
        // 시간이 멈춘 상태에서는 판정을 하지 않음 (공격 불가)
        if (IsTimeStopped) return;

        double t = songTime;
        double local = t - firstBeatTime;

        if (local < -Normal)
        {
            Debug.Log($"Bad (too early), SongTime={t:F3}");
            return;
        }

        int beatIndex = Mathf.RoundToInt((float)(local / beatInterval));
        double nearestBeatTime = firstBeatTime + beatIndex * beatInterval;
        double delta = t - nearestBeatTime;
        double absDelta = System.Math.Abs(delta);

        Debug.Log($"HIT!!! SongTime={t:F3}, beatIndex={beatIndex}, " +
                  $"nearestBeat={nearestBeatTime:F3}, delta={delta:F3}");

        if (absDelta <= Perfect) Calcul_Score("Perfect");
        else if (absDelta <= Good) Calcul_Score("Good");
        else if (absDelta <= Normal) Calcul_Score("Normal");
        else Debug.Log("Bad");
    }

    public void Calcul_Score(string str)
    {
        if (str == "Perfect") Debug.Log("Perfect!");
        else if (str == "Good") Debug.Log("Good");
        else if (str == "Normal") Debug.Log("Normal");
    }

    public void Hit_ZXC()
    {
        Hit = true;
    }

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    private System.Collections.IEnumerator StartGameRoutine()
    {
        // [추가!] 게임 시작 시 종료 스위치 초기화
        isGameEnded = false;

        yield return new WaitForSecondsRealtime(0.5f);

        if (countThree != null)
        {
            SoundManager.instance.SFXPlay("Count3", countThree);
            yield return new WaitForSecondsRealtime(countThree.length);
        }

        Time.timeScale = 1f;
        Debug.Log("Game Start!!");

        SoundManager.instance.PlayBGM(Main_BGM);
       
        songStartDspTime = AudioSettings.dspTime;

        stopTimer = false;
    }

    public void PauseGameForMenu()
    {
        if (IsGamePausedForMenu) return; // 이미 멈췄으면 무시

        Debug.Log("메뉴 열림: 게임 일시정지");
        IsGamePausedForMenu = true;
        
        // 1. 시간 멈추기
        Time.timeScale = 0f; 

        // 2. BGM 일시정지 (기존 함수 활용)
        // (만약 스킬 사용 중이라 이미 멈췄다면 다시 멈출 필요 없음)
        if (!stopTimer)
        {
            PauseMain_BGM();
        }

        // 3. 마우스 커서 활성화 (UI 조작을 위해)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGameFromMenu()
    {
        if (!IsGamePausedForMenu) return; // 멈춘 상태가 아니면 무시

        Debug.Log("메뉴 닫힘: 게임 재개");
        IsGamePausedForMenu = false;

        // 1. 시간 다시 흐르게 하기
        // (주의: 만약 스킬 사용 중에 메뉴를 열었다면, 메뉴를 닫아도 스킬 상태가 유지되어야 함)
        if (!IsTimeStopped)
        {
            Time.timeScale = 1f;
        }

        // 2. BGM 재개 (기존 함수 활용)
        // (마찬가지로 스킬 중이 아닐 때만 재개)
        if (!IsTimeStopped && stopTimer)
        {
            ResumeMain_BGM();
        }

        
    }

}