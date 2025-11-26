using UnityEngine;

// [핵심] 'Enemy' (부모)를 상속받아 모든 기본 기능(AI, 콤보, 넉백 등)을 물려받습니다.
public class Enemy_Warrior : Enemy
{
    // --- Warrior 전용 변수들 ---
    [Header("Warrior Config")]
    public float attackRange = 2f;      // 이 범위 안에 들어오면 '공격 준비' 시작

    // [추가!] 공격 딜레이 (플레이어가 보고 피할 수 있는 시간)
    // *참고: 이 시간 동안 공격 애니메이션의 앞부분(준비 동작)이 재생됩니다.
    public float attackDelay = 1.0f;    

    // [추가!] 공격 판정이 활성화되는 시간 (0.5초)
    public float attackActiveTime = 0.5f; 

    // [복원!] 딜레이 후 실제 공격 판정이 생길 위치
    public Transform attackPos;         
    // [복원!] 딜레이 후 실제 공격 판정의 크기
    public Vector2 attackBoxSize;       
    
    private float attackTimer;          // 공격 딜레이 / 활성 시간을 잴 타이머
    
    // [추가!] 딜레이가 끝나고 공격을 '이미 실행했는지' 확인하는 스위치
    private bool hasAttackedThisCycle = false;

    // -------------------------------------------------------------------------

    // [수정!] 부모(Enemy.cs)의 'FixedUpdate' 함수를 덮어씁니다.
    protected override void FixedUpdate()
    {
        // 1. 부모(Enemy)의 기본 뇌를 먼저 실행합니다. (순찰, 그로기, 넉백 처리 등)
        base.FixedUpdate();

        // --- [핵심 추가!] 공격 히트박스 위치 동기화 ---
        // (이 부분은 그대로 유지합니다)
        if (spriteRenderer != null && attackPos != null)
        {
            if (spriteRenderer.flipX == true) // 왼쪽 보는 중
            {
                attackPos.localPosition = new Vector3(-Mathf.Abs(attackPos.localPosition.x), attackPos.localPosition.y, attackPos.localPosition.z);
            }
            else // 오른쪽 보는 중
            {
                attackPos.localPosition = new Vector3(Mathf.Abs(attackPos.localPosition.x), attackPos.localPosition.y, attackPos.localPosition.z);
            }
        }
        // -------------------------------------------------------

        
        // 2. Warrior만의 추가 로직: '공격 중' (즉, 딜레이 중)일 때의 행동
        if (currentState == State.Attacking)
        {
            // 공격 중에는 멈춤
            rgd.linearVelocity = Vector2.zero;
            
            // 딜레이/공격 타이머를 감소시킴
            attackTimer -= Time.fixedDeltaTime;

            // [핵심 1] 딜레이가 끝나고, 아직 공격을 실행하지 않았다면
            if (attackTimer <= 0 && !hasAttackedThisCycle)
            {
                // [▼▼▼ 삭제된 부분 ▼▼▼]
                // 원래 여기 있던 애니메이션 트리거 코드를 삭제했습니다.
                // (이제 딜레이 시작할 때 미리 재생하므로 여기서는 필요 없음)
                // [▲▲▲ 삭제 완료 ▲▲▲]

                // [공격!]
                PerformAttack(); // (실제 히트박스 판정 실행)

                // "공격 실행했음" 스위치를 켬
                hasAttackedThisCycle = true;
                
                // 타이머를 '공격 활성화 시간(0.5초)'으로 재설정
                attackTimer = attackActiveTime; 
            }
            // [핵심 2] 공격 활성화 시간(0.5초)마저 다 지났다면
            else if (attackTimer <= 0 && hasAttackedThisCycle)
            {
                // 공격 사이클 완전 종료
                // 다시 '추격' 상태로 돌아가서 플레이어를 쫓음
                currentState = State.Chasing; 
            }
        }
    }

    // [수정!] 'PerformChaseLogic' (추격 뇌)를 덮어씁니다.
    protected override void PerformChaseLogic()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. [수정!] 공격 범위 안이고, "공격 중(딜레이 포함)"이 아닐 때만
        if (distanceToPlayer <= attackRange && currentState == State.Chasing)
        {
            // [공격 준비!]
            currentState = State.Attacking;     // 1. 상태를 '공격 중'으로 변경 (이동 멈춤)
            
            // --- [핵심 위치 이동!] 공격 딜레이 시작과 동시에 애니메이션 재생 ---
            if (animator != null)
            {
                animator.SetTrigger("Attack");
                // *팁: 딜레이 시간(attackDelay)과 애니메이션의 '타격 프레임'이 나오는 시간을
                // 비슷하게 맞추면 더욱 자연스럽습니다!
            }
            // -------------------------------------------------------------------

            // 2. 타이머를 '공격 딜레이(1초)'로 설정
            attackTimer = attackDelay;          
            
            // 3. "아직 공격 안했음" 스위치를 켬
            hasAttackedThisCycle = false;       
        }
        // 2. 공격 범위 밖이거나, 공격/넉백/그로기 중일 때
        else if (currentState == State.Chasing) // (공격 중일 땐 멈춰야 하므로)
        {
            // [추격!]
            base.PerformChaseLogic(); // 부모의 "그냥 추격" 기능 실행
        }
    }

    // [복원!] 딜레이가 끝난 후 호출되는, 실제 공격(히트박스) 함수
    // (이 함수는 그대로 유지합니다)
    private void PerformAttack()
    {
        Debug.Log("Attacker 몬스터 공격! (히트박스 활성화!)");
        
        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(attackPos.position, attackBoxSize, 0);
        foreach (Collider2D playerCollider in hitPlayers)
        {
            if (playerCollider.CompareTag("Player"))
            {
                Player_Health playerHealth = playerCollider.GetComponent<Player_Health>();
                if (playerHealth != null)
                {
                    playerHealth.Player_TakeDamaged(transform.position);
                    break; 
                }
            }
        }
    }

    // [복원!] 씬 화면에서 공격 범위를 빨간 네모로 보여줍니다.
    // (이 함수도 그대로 유지합니다)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        if (attackPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPos.position, attackBoxSize);
        }
    }
}