using UnityEngine;
using System.Collections;

// 보스 패턴 종류 열거형
public enum BossPatternType { Floor, Charge, Normal }

public class Enemy_Boss : Enemy
{
    [Header("=== Boss General Settings ===")]
    public float attackTriggerRange = 7.0f; // 이 거리 안에 들어오면 공격 루프 시스템 시작
    public float delayBetweenPatterns = 1.5f; // 패턴이 끝나고 다음 패턴까지의 휴식 시간
    public float patternMoveSpeed = 3.5f; // 패턴 사용을 위해 접근할 때의 이동 속도
    private bool isBossAttackingLoop = false; // 현재 공격 루프 시스템이 돌아가는 중인가?

    [Header("--- Pattern 1: Floor Attack (바닥 장판) ---")]
    public GameObject floorAttackPrefab;
    public float floorAttackRange = 5.0f; // 바닥 공격 시전 사거리
    public float floorAttackDelay = 0.5f; // 애니메이션 선딜레이

    [Header("--- Pattern 2: Charge Attack (돌진 - Bumper 방식) ---")]
    public float chargeWaitTime = 3.0f; // 기 모으는 시간
    public float chargeDistance = 12.0f; // 돌진 거리
    public float chargeSpeed = 20f; // 돌진 속도
    // public int chargeDamage = 25; // [제거] 돌진 데미지 변수 제거 (Player_TakeDamaged는 고정 데미지 사용)
    private bool isCharging = false; // 현재 돌진 중인지 체크하는 플래그

    [Header("--- Pattern 3: Normal Attack (일반 근접 - Warrior 방식) ---")]
    public Transform normalAttackPos; // 공격 히트박스 중심점 (자식 오브젝트)
    public float normalAttackRange = 2.5f; // 일반 공격 시전 사거리
    public Vector2 normalAttackBoxSize = new Vector2(2.5f, 2f);
    public float normalAttackDelay = 0.4f; // 선딜레이
    // public int normalDamage = 20; // [제거] 일반 공격 데미지 변수 제거


    protected override void FixedUpdate()
    {
        // 1. 그로기/넉백 상태면 아무것도 안 함
        if (currentState == State.Groggy || currentState == State.KnockedBack)
        {
            rgd.linearVelocity = Vector2.zero;
            isCharging = false; // 넉백되면 돌진도 취소
            return;
        }

        // 2. 공격 루프 시스템이 작동 중이면 기본 추격 AI는 정지
        if (isBossAttackingLoop)
        {
            // 돌진 중이 아닐 때만 속도 제어
            if (!isCharging)
            {
                 // 접근 중이 아닐 때(대기 시간 등)는 정지
            }
            return;
        }

        // 3. 기본 순찰/추격 AI 실행
        base.FixedUpdate();

        // 4. 공격 루프 시스템 시작 조건 체크
        if (playerTransform != null && currentState == State.Chasing)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackTriggerRange)
            {
                StartCoroutine(BossAttackLoop());
            }
        }
    }

    // ==================================================================================
    // 메인 공격 루프
    // ==================================================================================
    private IEnumerator BossAttackLoop()
    {
        isBossAttackingLoop = true;
        currentState = State.Attacking;
        Debug.Log("보스: 공격 패턴 루프 시스템 가동!");

        while (playerTransform != null && currentState != State.Groggy)
        {
            BossPatternType pattern = (BossPatternType)Random.Range(0, 3);
            Debug.Log($"<color=orange>[보스] 선택된 패턴: {pattern}</color>");
            
            switch (pattern)
            {
                case BossPatternType.Floor:  yield return StartCoroutine(Co_FloorAttack()); break;
                case BossPatternType.Charge: yield return StartCoroutine(Co_ChargeAttack()); break;
                case BossPatternType.Normal: yield return StartCoroutine(Co_NormalAttack()); break;
            }

            if (currentState == State.Groggy) break;

            rgd.linearVelocity = Vector2.zero;
            isCharging = false;
            LookAtPlayer();
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsChasing", false);
            yield return new WaitForSeconds(delayBetweenPatterns);
        }

        Debug.Log("보스: 공격 루프 종료");
        isBossAttackingLoop = false;
        isCharging = false;
        if (currentState != State.Groggy) currentState = State.Chasing;
    }


    // ==================================================================================
    // 각 패턴별 세부 코루틴
    // ==================================================================================

    // --- 패턴 1: 바닥 공격 (유지) ---
    private IEnumerator Co_FloorAttack()
    {
        while (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) > floorAttackRange)
        {
            if (currentState == State.Groggy) yield break;
            MoveTowardsPlayer(patternMoveSpeed);
            yield return new WaitForFixedUpdate();
        }
        rgd.linearVelocity = Vector2.zero;

        if (playerTransform == null) yield break;

        Vector3 targetPos = playerTransform.position;
        Debug.Log($"보스: 바닥 공격 시전! 1초 뒤 타격 예정");

        LookAtPlayer();
        animator.SetTrigger("TriggerFloor");

        yield return new WaitForSeconds(1.0f);

        if (floorAttackPrefab != null)
        {
             Instantiate(floorAttackPrefab, targetPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.5f);
    }

    // --- 패턴 2: 돌진 공격 ---
    private IEnumerator Co_ChargeAttack()
    {
        rgd.linearVelocity = Vector2.zero;
        LookAtPlayer();
        Debug.Log($"보스: 돌진 준비 ({chargeWaitTime}초)");
        animator.SetTrigger("TriggerChargeReady"); 
        yield return new WaitForSeconds(chargeWaitTime);

        Debug.Log("보스: 돌진 시작!");
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
            if (currentState == State.Groggy)
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

    // --- 패턴 3: 일반 공격 ---
    private IEnumerator Co_NormalAttack()
    {
        // [접근]
        while (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) > normalAttackRange)
        {
            if (currentState == State.Groggy) yield break;
            MoveTowardsPlayer(patternMoveSpeed);
            yield return new WaitForFixedUpdate();
        }
        rgd.linearVelocity = Vector2.zero;

        // [공격 시작]
        Debug.Log("보스: 일반 공격 시전! (애니메이션 시작)");
        LookAtPlayer();
        animator.SetTrigger("TriggerNormal");
        
        // 선딜레이 대기
        yield return new WaitForSeconds(normalAttackDelay);

        // [히트박스 판정 & 탐정 로그]
        if (normalAttackPos != null)
        {
            // 1. 판정 시도 정보 출력
            Debug.Log($"[탐정] 히트박스 판정 시도. 위치:{normalAttackPos.position}, 크기:{normalAttackBoxSize}, 찾는 레이어: Player");

            // 핵심: Player 레이어만 감지
            Collider2D[] hits = Physics2D.OverlapBoxAll(normalAttackPos.position, normalAttackBoxSize, 0, LayerMask.GetMask("Player"));
            
            // 2. 감지된 개수 출력
            Debug.Log($"[탐정] 감지된 'Player' 레이어 콜라이더 수: {hits.Length}개");

            foreach (Collider2D hit in hits)
            {
                 // 3. 감지된 녀석의 정체 출력
                 Debug.Log($"[탐정] 감지된 오브젝트 이름: {hit.gameObject.name} -> Player_Health 찾기 시도");
                 
                 hit.GetComponent<Player_Health>()?.Player_TakeDamaged(transform.position);
                 Debug.Log("<color=red>보스 일반 공격 적중! (데미지 적용 완료)</color>");
            }
        }
        else
        {
             Debug.LogError("보스 에러: Normal Attack Pos가 연결되지 않았습니다!");
        }
        yield return new WaitForSeconds(0.5f); // 후딜레이
    }

    // ==================================================================================
    // 물리 충돌 이벤트 (Bumper 방식)
    // ==================================================================================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging && collision.gameObject.CompareTag("Player"))
        {
            Player_Health playerHealth = collision.gameObject.GetComponent<Player_Health>();
            if (playerHealth != null)
            {
                // [수정됨] Player_TakeDamaged 함수 사용
                playerHealth.Player_TakeDamaged(transform.position);
                Debug.Log("<color=red>보스 돌진 몸통 박치기 성공! (넉백 적용)</color>");
            }
        }
    }


    // 유틸리티 함수들 (MoveTowardsPlayer, LookAtPlayer, OnDrawGizmos)은 기존과 동일합니다.
    private void MoveTowardsPlayer(float speed)
    {
        if (playerTransform == null) return;
        LookAtPlayer();
        float dir = playerTransform.position.x > transform.position.x ? 1f : -1f;
        rgd.linearVelocity = new Vector2(dir * speed, rgd.linearVelocity.y);
        animator.SetBool("IsMoving", true);
    }

    private void LookAtPlayer()
    {
        if (playerTransform == null) return;
        if (playerTransform.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false;
            if (normalAttackPos != null)
                normalAttackPos.localPosition = new Vector3(Mathf.Abs(normalAttackPos.localPosition.x), normalAttackPos.localPosition.y, 0);
        }
        else
        {
            spriteRenderer.flipX = true;
            if (normalAttackPos != null)
                normalAttackPos.localPosition = new Vector3(-Mathf.Abs(normalAttackPos.localPosition.x), normalAttackPos.localPosition.y, 0);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackTriggerRange);
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, floorAttackRange);
        Gizmos.color = Color.blue; Gizmos.DrawWireSphere(transform.position, normalAttackRange);
        if (normalAttackPos != null) {
             Gizmos.color = Color.yellow;
             Gizmos.DrawWireCube(normalAttackPos.position, normalAttackBoxSize);
        }
    }
}