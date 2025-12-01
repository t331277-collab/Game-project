using UnityEngine;
using System.Collections;

public enum BossPatternType { Floor, Charge, Normal }

public class Enemy_Boss : Enemy
{
    [Header("=== Boss General Settings ===")]
    public float attackTriggerRange = 7.0f;
    public float delayBetweenPatterns = 1.5f;
    public float patternMoveSpeed = 3.5f;
    private bool isBossAttackingLoop = false;

    [Header("=== Boss Groggy Settings ===")]
    public float bossGroggyDuration = 5.0f;
    private bool isRecoveringFromGroggy = false; 

    [Header("=== Boss Hit Feedback ===")]
    public float hitBlinkDuration = 0.5f;
    public float hitBlinkInterval = 0.05f;
    private Coroutine blinkCoroutine;

    [Header("--- Pattern 1: Floor Attack ---")]
    public GameObject floorAttackPrefab;
    public float floorAttackRange = 5.0f;
    public float floorAttackDelay = 0.5f;

    [Header("--- Pattern 2: Charge Attack ---")]
    public GameObject chargeIndicatorBox;
    public float indicatorBlinkInterval = 0.1f;
    private Coroutine indicatorCoroutine;
    // ▼▼▼ [추가] 경고 표시의 스프라이트 렌더러 저장용 변수 ▼▼▼
    private SpriteRenderer indicatorRenderer; 
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    public float chargeWaitTime = 3.0f;
    public float chargeDistance = 12.0f;
    public float chargeSpeed = 20f;
    private bool isCharging = false;

    [Header("--- Pattern 3: Normal Attack ---")]
    public Transform normalAttackPos;
    public float normalAttackRange = 2.5f;
    public Vector2 normalAttackBoxSize = new Vector2(2.5f, 2f);
    public float normalAttackDelay = 0.4f;

    // ==================================================================================
    // [추가] Start 함수 재정의 (초기화 작업)
    // ==================================================================================
    protected override void Start()
    {
        base.Start(); // 부모의 Start 먼저 실행

        // 경고 표시 오브젝트에서 SpriteRenderer 컴포넌트를 미리 찾아둡니다.
        if (chargeIndicatorBox != null)
        {
            indicatorRenderer = chargeIndicatorBox.GetComponent<SpriteRenderer>();
            // 시작 시 경고 표시는 꺼둡니다.
            chargeIndicatorBox.SetActive(false);
        }
    }

    // 콤보 처리 재정의 (깜빡임 효과)
    public override void HandleCombo(KeyCode pressedKey)
    {
        int previousIndex = currentSequenceIndex;
        base.HandleCombo(pressedKey);
        bool isComboSuccess = (currentSequenceIndex > previousIndex) || (currentState == State.Groggy && previousIndex == killSequence.Count - 1);
        
        if (isComboSuccess && currentState != State.Executed)
        {
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(Co_BossHitBlink());
        }
    }

    private IEnumerator Co_BossHitBlink()
    {
        float timer = 0f;
        while (timer < hitBlinkDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(hitBlinkInterval);
            timer += hitBlinkInterval;
        }
        spriteRenderer.enabled = true;
        blinkCoroutine = null;
    }

    // ==================================================================================
    // FixedUpdate 재정의 (슈퍼아머 및 상태 관리)
    // ==================================================================================
    protected override void FixedUpdate()
    {
        // 1. 상태 꼬임 방지
        if (currentState == State.Attacking && !isBossAttackingLoop)
        {
            currentState = State.Chasing;
            isCharging = false;
            if (animator != null) { animator.SetBool("IsMoving", false); animator.SetBool("IsChasing", false); }
        }

        // 2. 행동 불가 상태 처리
        if (currentState == State.Groggy || currentState == State.KnockedBack || currentState == State.Executed)
        {
            if (currentState != State.KnockedBack) rgd.linearVelocity = Vector2.zero;
            
            isCharging = false;
            if (animator != null) { animator.SetBool("IsMoving", false); animator.SetBool("IsChasing", false); }

            if (currentState == State.Groggy && !isRecoveringFromGroggy)
            {
                Debug.Log($"<color=yellow>[보스] 그로기 상태 감지! {bossGroggyDuration}초 후 회복.</color>");
                if (animator != null) animator.SetBool("IsGroggy", true);
                StartCoroutine(Co_BossRecoverFromGroggy());
                isRecoveringFromGroggy = true;
            }
            // [중요] 행동 불가 상태가 되면 경고 표시도 끕니다.
            if (chargeIndicatorBox != null && chargeIndicatorBox.activeSelf)
            {
                 if (indicatorCoroutine != null) StopCoroutine(indicatorCoroutine);
                 chargeIndicatorBox.SetActive(false);
            }
            return;
        }

        // 3. 공격 루프 실행 중이면 리턴
        if (isBossAttackingLoop) return;

        // 4. 추격 및 사거리 체크
        if (playerTransform != null && currentState == State.Chasing)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackTriggerRange)
            {
                rgd.linearVelocity = Vector2.zero;
                if (animator != null) { animator.SetBool("IsMoving", false); animator.SetBool("IsChasing", false); }
                StartCoroutine(BossAttackLoop());
                return;
            }
        }

        // 5. 기본 추격 AI 실행
        if (currentState == State.Chasing && animator != null)
        {
            animator.SetBool("IsMoving", true);
            animator.SetBool("IsChasing", true);
        }
        
        base.FixedUpdate();
    }
    
    // ... (Co_BossRecoverFromGroggy, BossAttackLoop, Co_FloorAttack 코루틴은 기존과 동일합니다) ...
    // (코드가 너무 길어 이 부분은 생략합니다. 이전 코드의 중간 부분을 그대로 유지해주세요.)

    // ==================================================================================
    // 보스 전용 그로기 회복 코루틴
    // ==================================================================================
    private IEnumerator Co_BossRecoverFromGroggy()
    {
        yield return new WaitForSeconds(bossGroggyDuration);

        if (currentState == State.Groggy)
        {
            Debug.Log("<color=cyan>[보스] 그로기 상태에서 자동으로 회복했습니다!</color>");
            currentState = State.Chasing;
            if (animator != null) animator.SetBool("IsGroggy", false);
        }
        isRecoveringFromGroggy = false;
    }

    // ==================================================================================
    // 메인 공격 루프
    // ==================================================================================
    private IEnumerator BossAttackLoop()
    {
        isBossAttackingLoop = true;
        currentState = State.Attacking;
        Debug.Log("보스: 공격 패턴 루프 시스템 가동!");

        while (playerTransform != null && currentState != State.Groggy && currentState != State.Executed)
        {
            BossPatternType pattern = (BossPatternType)Random.Range(0, 3);
            
            switch (pattern)
            {
                case BossPatternType.Floor:  yield return StartCoroutine(Co_FloorAttack()); break;
                case BossPatternType.Charge: yield return StartCoroutine(Co_ChargeAttack()); break;
                case BossPatternType.Normal: yield return StartCoroutine(Co_NormalAttack()); break;
            }

            if (currentState == State.Groggy || currentState == State.KnockedBack || currentState == State.Executed) break;

            // --- [패턴 사이 휴식] ---
            rgd.linearVelocity = Vector2.zero;
            isCharging = false;
            LookAtPlayer();

            if(animator != null) { animator.SetBool("IsMoving", false); animator.SetBool("IsChasing", false); }

            yield return new WaitForSeconds(delayBetweenPatterns);
        }

        Debug.Log("보스: 공격 루프 종료");
        isBossAttackingLoop = false;
        isCharging = false;
        
        if (currentState != State.Groggy && currentState != State.Executed)
        {
             currentState = State.Chasing;
        }
    }

    // --- 패턴 1: 바닥 공격 ---
    private IEnumerator Co_FloorAttack()
    {
        while (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) > floorAttackRange)
        {
            if (currentState == State.Groggy || currentState == State.KnockedBack || currentState == State.Executed) yield break;
            MoveTowardsPlayer(patternMoveSpeed);
            yield return new WaitForFixedUpdate();
        }
        rgd.linearVelocity = Vector2.zero;
        if(animator != null) { animator.SetBool("IsMoving", false); animator.SetBool("IsChasing", false); }

        if (playerTransform == null) yield break;

        Vector3 targetPos = playerTransform.position;
        LookAtPlayer();
        animator.SetTrigger("TriggerFloor");
        yield return new WaitForSeconds(1.0f);

        if (floorAttackPrefab != null && currentState != State.Groggy && currentState != State.KnockedBack && currentState != State.Executed)
             Instantiate(floorAttackPrefab, targetPos, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
    }


    // --- 패턴 2: 돌진 공격 ---
    private IEnumerator Co_ChargeAttack()
    {
        rgd.linearVelocity = Vector2.zero;
        LookAtPlayer();

        if(animator != null) { animator.SetBool("IsMoving", false); animator.SetBool("IsChasing", false); }
        animator.SetTrigger("TriggerChargeReady"); 

        // 경고 표시 시작
        if (chargeIndicatorBox != null)
        {
            chargeIndicatorBox.SetActive(true);
            if (indicatorCoroutine != null) StopCoroutine(indicatorCoroutine);
            indicatorCoroutine = StartCoroutine(Co_FlashIndicator());
        }

        yield return new WaitForSeconds(chargeWaitTime);

        // 경고 표시 종료
        if (indicatorCoroutine != null) StopCoroutine(indicatorCoroutine);
        if (chargeIndicatorBox != null) chargeIndicatorBox.SetActive(false);

        if (currentState == State.Groggy || currentState == State.KnockedBack || currentState == State.Executed) yield break;

        animator.SetTrigger("TriggerChargeRun");
        LookAtPlayer();
        isCharging = true;

        float dir = spriteRenderer.flipX ? -1f : 1f;
        Vector3 startPos = transform.position;
        float distanceCovered = 0f;
        float timeElapsed = 0f;
        float maxDuration = (chargeDistance / chargeSpeed) + 1.0f;

        while (distanceCovered < chargeDistance && timeElapsed < maxDuration)
        {
            if (currentState == State.Groggy || currentState == State.KnockedBack || currentState == State.Executed)
            { 
                isCharging = false;
                yield break; 
            }
            rgd.linearVelocity = new Vector2(dir * chargeSpeed, rgd.linearVelocity.y);
            distanceCovered = Vector2.Distance(startPos, transform.position);
            timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rgd.linearVelocity = Vector2.zero;
        isCharging = false;
        yield return new WaitForSeconds(0.7f);
    }

    // 경고 표시 깜빡임 코루틴
    private IEnumerator Co_FlashIndicator()
    {
        if (indicatorRenderer == null) yield break;

        while (true)
        {
            indicatorRenderer.enabled = !indicatorRenderer.enabled;
            yield return new WaitForSeconds(indicatorBlinkInterval);
        }
    }

    // --- 패턴 3: Normal Attack, OnCollision, MoveTowards... 등은 기존과 동일하므로 생략합니다 ---
    // (이전 코드의 뒷부분을 그대로 사용해주세요)
    
    private IEnumerator Co_NormalAttack()
    {
        while (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) > normalAttackRange)
        {
            if (currentState == State.Groggy || currentState == State.KnockedBack || currentState == State.Executed) yield break;
            MoveTowardsPlayer(patternMoveSpeed);
            yield return new WaitForFixedUpdate();
        }
        rgd.linearVelocity = Vector2.zero;
        if(animator != null) { animator.SetBool("IsMoving", false); animator.SetBool("IsChasing", false); }

        LookAtPlayer();
        animator.SetTrigger("TriggerNormal");
        yield return new WaitForSeconds(normalAttackDelay);

        if (currentState == State.Groggy || currentState == State.KnockedBack || currentState == State.Executed) yield break;

        if (normalAttackPos != null)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(normalAttackPos.position, normalAttackBoxSize, 0, LayerMask.GetMask("Player"));
            foreach (Collider2D hit in hits)
                 hit.GetComponent<Player_Health>()?.Player_TakeDamaged(transform.position);
        }
        yield return new WaitForSeconds(0.5f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging && collision.gameObject.CompareTag("Player"))
        {
            Player_Health playerHealth = collision.gameObject.GetComponent<Player_Health>();
            if (playerHealth != null) playerHealth.Player_TakeDamaged(transform.position);
        }
    }

    private void MoveTowardsPlayer(float speed)
    {
        if (playerTransform == null) return;
        LookAtPlayer();
        float dir = playerTransform.position.x > transform.position.x ? 1f : -1f;
        rgd.linearVelocity = new Vector2(dir * speed, rgd.linearVelocity.y);
        if (animator != null) { animator.SetBool("IsMoving", true); animator.SetBool("IsChasing", true); }
    }

    // ==================================================================================
    // [수정] 플레이어 방향 보기 (경고 표시 스프라이트 반전 추가)
    // ==================================================================================
    private void LookAtPlayer()
    {
        if (playerTransform == null) return;

        if (playerTransform.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false; // 보스 정방향
            if (normalAttackPos != null) normalAttackPos.localPosition = new Vector3(Mathf.Abs(normalAttackPos.localPosition.x), normalAttackPos.localPosition.y, 0);
            if (chargeIndicatorBox != null) chargeIndicatorBox.transform.localPosition = new Vector3(Mathf.Abs(chargeIndicatorBox.transform.localPosition.x), chargeIndicatorBox.transform.localPosition.y, chargeIndicatorBox.transform.localPosition.z);

            // ▼▼▼ [추가] 경고 표시 스프라이트 정방향 ▼▼▼
            if (indicatorRenderer != null) indicatorRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true; // 보스 반전
            if (normalAttackPos != null) normalAttackPos.localPosition = new Vector3(-Mathf.Abs(normalAttackPos.localPosition.x), normalAttackPos.localPosition.y, 0);
            if (chargeIndicatorBox != null) chargeIndicatorBox.transform.localPosition = new Vector3(-Mathf.Abs(chargeIndicatorBox.transform.localPosition.x), chargeIndicatorBox.transform.localPosition.y, chargeIndicatorBox.transform.localPosition.z);

            // ▼▼▼ [추가] 경고 표시 스프라이트 반전 ▼▼▼
            if (indicatorRenderer != null) indicatorRenderer.flipX = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackTriggerRange);
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, floorAttackRange);
        Gizmos.color = Color.blue; Gizmos.DrawWireSphere(transform.position, normalAttackRange);
        if (normalAttackPos != null) {
             Gizmos.color = Color.yellow; Gizmos.DrawWireCube(normalAttackPos.position, normalAttackBoxSize);
        }
    }
}