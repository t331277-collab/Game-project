using UnityEngine;
using System.Collections;
using System.Collections.Generic; // 리스트 사용을 위해 필수

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    // --- [새로운 기능: 화면에 보이는 적 관리] ---
    // 모든 적 인스턴스가 공유하는 '화면에 보이는 적 목록' (정적 리스트)
    // GameManager가 이 리스트를 보고 적을 선택합니다.
    public static List<Enemy> VisibleEnemies = new List<Enemy>();

    // 이 오브젝트의 스프라이트가 카메라 화면 안에 들어오면 엔진이 자동으로 호출
    private void OnBecameVisible()
    {
        if (!VisibleEnemies.Contains(this))
        {
            VisibleEnemies.Add(this);
            // 만약 스킬 사용 중에 새로 화면에 나타났다면 선택 표시 초기화
            Deselect(); 
        }
    }

    // 이 오브젝트의 스프라이트가 카메라 화면 밖으로 나가면 엔진이 자동으로 호출
    private void OnBecameInvisible()
    {
        if (VisibleEnemies.Contains(this))
        {
            VisibleEnemies.Remove(this);
            // 화면 밖으로 나가면 색상을 원래대로 복구
            Deselect(); 
        }
    }

    // 오브젝트가 파괴될 때 안전하게 리스트에서 제거
    private void OnDestroy()
    {
        if (VisibleEnemies.Contains(this))
        {
            VisibleEnemies.Remove(this);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyDestroyed();
        }
    }
    // ------------------------------------


    // --- AI 상태 변수 ---
    protected enum State { Patrolling, Chasing, KnockedBack, Groggy, Attacking, Executed }
    protected State currentState;

    [Header("Sound")]
    public AudioClip cilp1; //chord_a1
    public AudioClip cilp2; //chord_a2
    public AudioClip cilp3; //chord_a3
    public AudioClip cilp4; //chord_a3
    public AudioClip cilp5; //chord_a3
    public AudioClip cilp6; //chord_a3

    public AudioClip cassete_start_end; //카세트 딸깍~ 소리

    [Header("SoundSkill")]
    public float Sounddelay = 1.0f;
    // public float Delaytime = 0.1f; // (더 이상 사용 안 함 - Time.timeScale로 대체)

    // --- AI 순찰(Patrol) 변수 ---
    [Header("AI - Patrol")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 5f;
    private Vector3 startPosition;
    private bool movingRight = true;

    // --- AI 추격(Chase) 변수 ---
    [Header("AI - Chase")]
    public float chaseSpeed = 4f;
    public float detectionRange = 10f;
    protected Transform playerTransform;

    // --- 전투(Combat) 변수 ---
    [Header("Combat")]
    public List<KeyCode> killSequence = new List<KeyCode>();
    public float knockbackForce = 150f;
    public float knockbackMultiplier = 1.0f;
    public float knockbackDuration = 0.2f;
    [Header("Combat")]
    public float groggyDuration = 3.0f;

    protected int currentSequenceIndex = 0;
    private float knockbackTimer;
    private float groggyTimer;
    
    // --- 컴포넌트 참조 ---
    protected Rigidbody2D rgd;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;

    // [추가!] 선택 시각 효과를 위해 원래 색상을 기억할 변수
    private Color originalColor;

    private Light selectionLight;

    protected virtual void Start()
    {
        rgd = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        currentState = State.Patrolling;
        startPosition = transform.position;
        rgd.gravityScale = 1f;
        rgd.constraints = RigidbodyConstraints2D.FreezeRotation;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        // [추가!] 시작할 때 원래 스프라이트 색상을 저장해둡니다.
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        selectionLight = transform.Find("SelectionLight")?.GetComponent<Light>();
    
        if (selectionLight == null)
        {
            Debug.LogWarning($"[{gameObject.name}] SelectionLight를 찾을 수 없습니다!");
        }
    }

    protected virtual void FixedUpdate()
    {
        if (playerTransform == null) return;

        if (animator != null)
        {
            // 1. 움직이고 있는가? (속도가 0.1보다 큰가?)
            bool isMoving = rgd.linearVelocity.magnitude > 0.1f;
            animator.SetBool("IsMoving", isMoving);

            // 2. 지금 추격 상태인가?
            bool isChasing = (currentState == State.Chasing);
            animator.SetBool("IsChasing", isChasing);

            bool isGroggy = (currentState == State.Groggy);
            animator.SetBool("IsGroggy", isGroggy);

        }

        switch (currentState)
        {
            case State.Patrolling:
                PerformPatrol();
                CheckForPlayer();
                break;
            case State.Chasing:
                PerformChaseLogic();
                break;
            case State.KnockedBack:
                knockbackTimer -= Time.fixedDeltaTime;
                if (knockbackTimer <= 0)
                {
                    currentState = State.Chasing;
                }
                break;
            case State.Groggy:
                rgd.linearVelocity = Vector2.zero;
                groggyTimer -= Time.fixedDeltaTime;
                if (groggyTimer <= 0)
                {
                    Debug.Log("그로기 상태 풀림!");
                    currentState = State.Chasing;
                }
                break;
            case State.Attacking:
                rgd.linearVelocity = Vector2.zero;
                break;
        }
    }

    // --- AI 공통 함수들 ---
    private void PerformPatrol()
    {
        float currentSpeed = movingRight ? patrolSpeed : -patrolSpeed;
        rgd.linearVelocity = new Vector2(currentSpeed, rgd.linearVelocity.y);
        if (movingRight && transform.position.x >= startPosition.x + patrolDistance)
        {
            movingRight = false;
            spriteRenderer.flipX = true;
        }
        else if (!movingRight && transform.position.x <= startPosition.x - patrolDistance)
        {
            movingRight = true;
            spriteRenderer.flipX = false;
        }
    }

    private void CheckForPlayer()
    {
        if (currentState == State.KnockedBack) return;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer < detectionRange)
        {
            currentState = State.Chasing;
        }
    }

    protected virtual void PerformChaseLogic()
    {
        float direction = playerTransform.position.x > transform.position.x ? 1f : -1f;
        rgd.linearVelocity = new Vector2(direction * chaseSpeed, rgd.linearVelocity.y);
        spriteRenderer.flipX = (direction < 0);
    }

    // --- 전투 공통 함수들 ---
    private void PerformKnockback()
    {
        if (playerTransform == null) return;
        Debug.Log("넉백!");
        if (animator != null)
        {
            // 방금 애니메이터에서 만든 Trigger 이름 "Hurt"
            animator.SetTrigger("Hurt");
            // *팁: 만약 공격 중이었다면 공격이 캔슬되는 느낌을 주기 위해
            // "Attack" 트리거를 리셋해주는 것도 좋습니다. (선택 사항)
            animator.ResetTrigger("Attack"); 
        }
        currentState = State.KnockedBack;
        knockbackTimer = knockbackDuration;
        rgd.linearVelocity = Vector2.zero;
        float direction = transform.position.x > playerTransform.position.x ? 1f : -1f;
        rgd.AddForce(new Vector2(direction * knockbackForce * knockbackMultiplier, 2f), ForceMode2D.Impulse);
    }

    public virtual void HandleCombo(KeyCode pressedKey)
    {
        if (currentState == State.Groggy) return;

        if (killSequence.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        if (pressedKey == killSequence[currentSequenceIndex])
        {
            Debug.Log("콤보 성공! (" + (currentSequenceIndex + 1) + "/" + killSequence.Count + ")");
            currentSequenceIndex++;

            if (currentSequenceIndex >= killSequence.Count)
            {
                Debug.Log("그로기 상태 돌입!");
                currentState = State.Groggy;
                groggyTimer = groggyDuration;
                rgd.linearVelocity = Vector2.zero;
                currentSequenceIndex = 0;
            }
            else
            {
                Debug.Log("다음 키 (" + killSequence[currentSequenceIndex] + ") 준비.");
                PerformKnockback();
            }
        }
        else
        {
            Debug.Log("콤보 실패! 순서 초기화.");
            PerformKnockback();
            currentSequenceIndex = 0;

            if (pressedKey == killSequence[currentSequenceIndex])
            {
                Debug.Log("...하지만 콤보의 첫 키와 일치합니다! (1/" + killSequence.Count + ")");
                currentSequenceIndex++;
            }
        }
    }



    public void TakeDamageZ() { Debug.Log("Z키 공격 감지됨!"); HandleCombo(KeyCode.Z); }
    public void TakeDamageX() { Debug.Log("X키 공격 감지됨!"); HandleCombo(KeyCode.X); }
    public void TakeDamageC() { Debug.Log("C키 공격 감지됨!"); HandleCombo(KeyCode.C); }
    public void Execute() { Debug.Log("처형!"); Destroy(gameObject); }
    public bool IsGroggy() { return currentState == State.Groggy; }


    // --- [새로운 기능: 선택 시각 효과 및 소리 트리거] ---

    // GameManager가 이 적을 선택했을 때 호출 (시각 효과)
    public void TurnBlack()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.black;
        }
    }
    public void RestoreOriginalColor()
    {
    if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
    public void Select()
    {
        if (spriteRenderer != null)
        {
            // 예시: 색상을 빨간색으로 변경하여 선택됨을 표시
            spriteRenderer.color = Color.black;
        }
    }

    // GameManager가 다른 적을 선택해서 이 적이 선택 해제될 때 호출 (시각 효과 복구)
    public void Deselect()
    {
        if (spriteRenderer != null)
        {
            // 원래 색상으로 복구
            spriteRenderer.color = originalColor;
        }
    }

    // [핵심] GameManager가 스페이스바를 눌렀을 때 이 함수를 호출하여 소리 재생 시작
    public void TriggerSkillSoundSequence()
    {
        // 그로기 상태가 아닐 때만 재생
        if (!IsGroggy())
        {
            StartCoroutine(SoundSkillDamagedCo());
            // 소리 재생이 시작되면 선택 표시를 해제합니다 (선택 사항)
            Deselect();
        }
        else
        {
            Debug.Log("그로기 상태라 정보를 들을 수 없습니다. 즉시 시간 재개 요청.");
            // 그로기 상태면 정보 제공 없이 바로 시간 재개 요청
            GameManager.Instance.StartCoroutine(GameManager.Instance.ResumeTimeAfterDelay(0f));
        }
    }

    // 기존 Player_Attack에서 호출하던 방식 호환성 유지 (필요 시)
    public void SoundSkillDamaged()
    {
        StartCoroutine(SoundSkillDamagedCo());
    }

    // [수정] 사운드 스킬 재생 코루틴
    private IEnumerator SoundSkillDamagedCo()
    {
        // (Time.timeScale은 이미 GameManager에서 0으로 만들었으므로 별도 설정 불필요)

        // Realtime으로 대기해야 멈춘 시간 속에서도 기다릴 수 있습니다.
        
        SoundManager.instance.SFXPlay("cassete_start", cassete_start_end);
        yield return new WaitForSecondsRealtime(Sounddelay);

        Debug.Log("Skill Info Playing...");

        

        // 현재 enemy의 killSequence에 맞는 음악 재생
        if (killSequence.Count > 0) { Debug.Log(killSequence[0]); SoundManager.instance.SFXPlay("Chord_a1", cilp1); yield return new WaitForSecondsRealtime(Sounddelay); }
        if (killSequence.Count > 1) { Debug.Log(killSequence[1]); SoundManager.instance.SFXPlay("Chord_a1", cilp2); yield return new WaitForSecondsRealtime(Sounddelay); }
        if (killSequence.Count > 2) { Debug.Log(killSequence[2]); SoundManager.instance.SFXPlay("Chord_a1", cilp3); yield return new WaitForSecondsRealtime(Sounddelay); }
        if (killSequence.Count > 3) { Debug.Log(killSequence[3]); SoundManager.instance.SFXPlay("Chord_a1", cilp4); yield return new WaitForSecondsRealtime(Sounddelay); }
        if (killSequence.Count > 4) { Debug.Log(killSequence[4]); SoundManager.instance.SFXPlay("Chord_a1", cilp5); yield return new WaitForSecondsRealtime(Sounddelay); }
        if (killSequence.Count > 5) { Debug.Log(killSequence[5]); SoundManager.instance.SFXPlay("Chord_a1", cilp6); yield return new WaitForSecondsRealtime(Sounddelay); }

        SoundManager.instance.SFXPlay("cassete_end", cassete_start_end);
        yield return new WaitForSecondsRealtime(Sounddelay);

        Debug.Log("소리 재생 완료. 1초 후 시간 재개 요청.");

        // [핵심!] GameManager에게 "1초 뒤에 시간을 다시 흐르게 해줘"라고 요청합니다.
        // (StartCoroutine을 사용하여 GameManager의 코루틴을 실행합니다)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartCoroutine(GameManager.Instance.ResumeTimeAfterDelay(1.0f));
        }
    }
}