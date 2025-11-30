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
    public float chargeWaitTime = 3.0f;
    public float chargeDistance = 12.0f;
    public float chargeSpeed = 20f;
    private bool isCharging = false;

    [Header("--- Pattern 3: Normal Attack ---")]
    public Transform normalAttackPos;
    public float normalAttackRange = 2.5f;
    public Vector2 normalAttackBoxSize = new Vector2(2.5f, 2f);
    public float normalAttackDelay = 0.4f;

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
    // FixedUpdate 재정의 (피격 문제 해결 버전)
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
            // [수정 1] 넉백 상태(KnockedBack)일 때는 속도를 0으로 만들지 않습니다! (부모의 넉백 힘 적용을 위해)
            if (currentState != State.KnockedBack)
            {
                rgd.linearVelocity = Vector2.zero;
            }
            
            isCharging = false;
            // 행동 불가 상태에서는 이동 애니메이션 신호 끄기
            if (animator != null) { animator.SetBool("IsMoving", false); animator.SetBool("IsChasing", false); }

            if (currentState == State.Groggy && !isRecoveringFromGroggy)
            {
                Debug.Log($"<color=yellow>[보스] 그로기 상태 감지! {bossGroggyDuration}초 후 회복.</color>");
                if (animator != null) animator.SetBool("IsGroggy", true);
                StartCoroutine(Co_BossRecoverFromGroggy());
                isRecoveringFromGroggy = true;
            }
            return; // 부모 로직 실행 안 함
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
                return; // 공격 시작
            }
        }

        // 5. 기본 추격 AI 실행 (사거리 밖일 때)
        // [수정 2] 핵심! 넉백 후 추격 상태로 돌아왔을 때 이동 애니메이션이 꺼져있다면 강제로 켭니다.
        if (currentState == State.Chasing && animator != null)
        {
            // 애니메이터 파라미터가 꺼져있을 경우를 대비해 강제로 true로 설정
            animator.SetBool("IsMoving", true);
            animator.SetBool("IsChasing", true);
        }
        
        base.FixedUpdate(); // 부모의 실제 이동 로직 실행
    }

    // ... (이하 코루틴 및 유틸리티 함수들은 기존과 완전히 동일합니다) ...
    // (코드가 길어 생략하지만, 이전에 사용하시던 코드의 뒷부분을 그대로 유지해주세요.)
    // Co_BossRecoverFromGroggy, BossAttackLoop, Co_FloorAttack, Co_ChargeAttack, Co_NormalAttack,
    // OnCollisionEnter2D, MoveTowardsPlayer, LookAtPlayer, OnDrawGizmos

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
            // 그로기 신호 끄기
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

            if (currentState == State.Groggy) break;

            // --- [패턴 사이 휴식 시작] ---
            rgd.linearVelocity = Vector2.zero;
            isCharging = false;
            LookAtPlayer();

            // 휴식 중에는 이동 관련 애니메이션 파라미터를 모두 끕니다.
            if(animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetBool("IsChasing", false);
            }

            yield return new WaitForSeconds(delayBetweenPatterns); // 대기
            // --- [휴식 끝] ---
        }

        Debug.Log("보스: 공격 루프 종료");
        isBossAttackingLoop = false;
        isCharging = false;
        
        if (currentState != State.Groggy && currentState != State.Executed)
        {
             // 루프가 정상 종료되면 다시 추격 상태로 복귀
             currentState = State.Chasing;
        }
    }

    // --- 패턴 1: 바닥 공격 ---
    private IEnumerator Co_FloorAttack()
    {
        while (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) > floorAttackRange)
        {
            if (currentState == State.Groggy) yield break;
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

        if (floorAttackPrefab != null && currentState != State.Groggy)
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
        yield return new WaitForSeconds(chargeWaitTime);
        if (currentState == State.Groggy) yield break;

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
            if (currentState == State.Groggy) { isCharging = false; yield break; }
            rgd.linearVelocity = new Vector2(dir * chargeSpeed, rgd.linearVelocity.y);
            distanceCovered = Vector2.Distance(startPos, transform.position);
            timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rgd.linearVelocity = Vector2.zero;
        isCharging = false;
        yield return new WaitForSeconds(0.7f);
    }

    // --- 패턴 3: 일반 공격 ---
    private IEnumerator Co_NormalAttack()
    {
        while (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) > normalAttackRange)
        {
            if (currentState == State.Groggy) yield break;
            MoveTowardsPlayer(patternMoveSpeed);
            yield return new WaitForFixedUpdate();
        }
        rgd.linearVelocity = Vector2.zero;
        if(animator != null) { animator.SetBool("IsMoving", false); animator.SetBool("IsChasing", false); }

        LookAtPlayer();
        animator.SetTrigger("TriggerNormal");
        yield return new WaitForSeconds(normalAttackDelay);
        if (currentState == State.Groggy) yield break;

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

    private void LookAtPlayer()
    {
        if (playerTransform == null) return;
        if (playerTransform.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false;
            if (normalAttackPos != null) normalAttackPos.localPosition = new Vector3(Mathf.Abs(normalAttackPos.localPosition.x), normalAttackPos.localPosition.y, 0);
        }
        else
        {
            spriteRenderer.flipX = true;
            if (normalAttackPos != null) normalAttackPos.localPosition = new Vector3(-Mathf.Abs(normalAttackPos.localPosition.x), normalAttackPos.localPosition.y, 0);
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