using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    // --- AI 상태 변수 ---
    protected enum State { Patrolling, Chasing, KnockedBack, Groggy, Attacking }
    protected State currentState;

    [Header("SoundSkill")]
    public float Sounddelay = 1.0f;
    public float Delaytime = 0.1f;

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
    // [핵심!] 이 목록을 [Z, X, C] 또는 [X, C, Z] 등으로 편집하면
    // 몬스터의 콤보 순서가 자유롭게 바뀝니다.
    public List<KeyCode> killSequence = new List<KeyCode>();
    public float knockbackForce = 150f;
    public float knockbackMultiplier = 1.0f;
    public float knockbackDuration = 0.2f;
    [Header("Combat")]
    public float groggyDuration = 3.0f; 
    
    // 현재 콤보(killSequence)의 몇 번째 순서를 맞춰야 하는지 기억하는 숫자(인덱스)입니다.
    private int currentSequenceIndex = 0; 
    
    private float knockbackTimer;
    private float groggyTimer; 

    // --- 컴포넌트 참조 ---
    protected Rigidbody2D rgd;
    protected SpriteRenderer spriteRenderer;
    
    // Start() 함수는 'virtual'로 변경하여 자식들이 이 함수를 확장(override)할 수 있게 합니다.
    protected virtual void Start()
    {
        rgd = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentState = State.Patrolling;
        startPosition = transform.position;
        rgd.gravityScale = 1f; 
        rgd.constraints = RigidbodyConstraints2D.FreezeRotation; 
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    // FixedUpdate()는 'virtual'로 변경하여 자식들이 이 함수를 확장(override)할 수 있게 합니다.
    protected virtual void FixedUpdate()
    {
        if (playerTransform == null) return;

        // AI 상태(State)에 따라 다른 행동을 합니다.
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

    // 순찰 AI (자식도 공통 사용)
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

    // 플레이어 감지 AI (자식도 공통 사용)
    private void CheckForPlayer()
    {
        if (currentState == State.KnockedBack) return; 
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer < detectionRange)
        {
            currentState = State.Chasing;
        }
    }
    
    // 'virtual'(덮어쓰기 가능)로 만든 기본 추격 로직
    protected virtual void PerformChaseLogic()
    {
        float direction = playerTransform.position.x > transform.position.x ? 1f : -1f;
        rgd.linearVelocity = new Vector2(direction * chaseSpeed, rgd.linearVelocity.y);
        spriteRenderer.flipX = (direction < 0);
    }

    // --- 전투 공통 함수들 ---
    
    // 넉백 (자식도 공통 사용)
    private void PerformKnockback()
    {
        if (playerTransform == null) return;
        Debug.Log("넉백!");
        currentState = State.KnockedBack; 
        knockbackTimer = knockbackDuration; 
        rgd.linearVelocity = Vector2.zero; 
        float direction = transform.position.x > playerTransform.position.x ? 1f : -1f;
        rgd.AddForce(new Vector2(direction * knockbackForce * knockbackMultiplier, 2f), ForceMode2D.Impulse);
    }

    // [삭제!] ProcessNextStep 함수 (버그 원인)
    // [삭제!] ProcessHit 함수 (버그 원인)

    // [추가!] 새로운 콤보 판정 함수 (버그 수정)
    private void HandleCombo(KeyCode pressedKey)
    {
        // 1. 그로기 상태일 때는 콤보가 안 먹힘
        if (currentState == State.Groggy) return;

        // 2. 키 목록이 비어있으면 즉시 사망 (이전과 동일)
        if (killSequence.Count == 0)
        {
            Destroy(gameObject);
            return;
        }

        // 3. [핵심 로직] 눌린 키가 현재 콤보 순서와 일치하는가?
        if (pressedKey == killSequence[currentSequenceIndex])
        {
            // [성공]
            Debug.Log("콤보 성공! (" + (currentSequenceIndex + 1) + "/" + killSequence.Count + ")");
            // 다음 콤보 순서로
            currentSequenceIndex++; 

            // 4. [핵심 로직] 콤보를 모두 완료했는가?
            if (currentSequenceIndex >= killSequence.Count)
            {
                // [그로기 발동!]
                Debug.Log("그로기 상태 돌입!");
                currentState = State.Groggy; 
                groggyTimer = groggyDuration; 
                rgd.linearVelocity = Vector2.zero; 
                currentSequenceIndex = 0; // 콤보 초기화
                // (그로기는 넉백 없음)
            }
            else
            {
                // [콤보 진행 중] (예: ZXC 중 Z만 맞힘)
                Debug.Log("다음 키 (" + killSequence[currentSequenceIndex] + ") 준비.");
                PerformKnockback(); // 콤보 중간 넉백
            }
        }
        // 5. [핵심 로직] 콤보 순서가 틀렸을 때 (예: ZXC 차례에 Z를 또 누름)
        else
        {
            // [실패]
            Debug.Log("콤보 실패! 순서 초기화.");
            PerformKnockback(); // 실패 시 넉백
            // [버그 수정!] 콤보를 0으로 즉시 초기화
            currentSequenceIndex = 0; 

            // 6. [재검사!] 방금 누른 '틀린' 키가 콤보의 '첫 번째' 키는 아닌지?
            // (이것이 XC'ZXC'를 가능하게 합니다)
            if (pressedKey == killSequence[currentSequenceIndex]) // (currentSequenceIndex는 0)
            {
                Debug.Log("...하지만 콤보의 첫 키와 일치합니다! (1/" + killSequence.Count + ")");
                // 콤보를 1로 시작
                currentSequenceIndex++; 
                // (이때는 넉백을 두 번 할 필요 없으니 넉백은 생략)
            }
        }
    }

    // [수정!] TakeDamage 함수들이 새 HandleCombo 함수를 호출하도록 변경
    public void TakeDamageZ() 
    { 
        Debug.Log("Z키 공격 감지됨!");
        HandleCombo(KeyCode.Z); 
    }
    public void TakeDamageX() 
    { 
        Debug.Log("X키 공격 감지됨!");
        HandleCombo(KeyCode.X); 
    }
    public void TakeDamageC() 
    { 
        Debug.Log("C키 공격 감지됨!");
        HandleCombo(KeyCode.C); 
    }
    
    // 처형 (자식도 공통 사용)
    public void Execute() 
    { 
        Debug.Log("처형!");
        Destroy(gameObject); 
    }
    
    // 그로기 상태 확인 (자식도 공통 사용)
    public bool IsGroggy() 
    { 
        return currentState == State.Groggy; 
    }

    public void SoundSkillDamaged()
    {
        StartCoroutine(SoundSkillDamagedCo());
    }

    private IEnumerator SoundSkillDamagedCo()
    {
        Time.timeScale = Delaytime;

        Debug.Log("Skill!");

        Debug.Log(killSequence[0]);
        yield return new WaitForSecondsRealtime(Sounddelay);

        Debug.Log(killSequence[1]);
        yield return new WaitForSecondsRealtime(Sounddelay);

        Debug.Log(killSequence[2]);
        yield return new WaitForSecondsRealtime(Sounddelay);

        Time.timeScale = 1f;
    }




}