using UnityEngine;

// [핵심!] MonoBehaviour가 아닌 'Enemy' (부모 스크립트)를 상속받습니다.
public class Enemy_Attacker : Enemy
{
    // --- 'Attacker' 전용 변수들 ---
    [Header("Attacker Config")]
    public float attackRange = 2f;      // 이 범위 안에 들어오면 멈춰서 공격
    public float attackDuration = 1.0f; // 공격 모션(멈춤)이 지속되는 시간
    public Transform attackPos;         // Player_Attack처럼 공격 히트박스 위치
    public Vector2 attackBoxSize;       // 공격 히트박스 크기
    private float attackTimer;          // 공격 시간(쿨타임) 재는 타이머
    
    // [핵심!] 부모(Enemy.cs)의 'FixedUpdate' 함수를 덮어씁니다.
    protected override void FixedUpdate()
    {
        // 'base.FixedUpdate()'를 호출하여, 부모의 FixedUpdate 로직
        // (Patrol, KnockBack, Groggy 상태 처리)을 그대로 실행합니다.
        base.FixedUpdate();

        // [추가!] 'Attacking' 상태일 때의 로직을 여기에 추가합니다.
        if (currentState == State.Attacking)
        {
            // (부모는 멈추기만 했지만, 여기서는 타이머까지 돌립니다)
            attackTimer -= Time.fixedDeltaTime;
            if (attackTimer <= 0)
            {
                // 공격 시간(1초)이 끝나면 다시 추격
                currentState = State.Chasing; 
            }
        }
    }

    // [핵심!] 부모(Enemy.cs)의 'PerformChaseLogic' 함수를 덮어씁니다.
    // 'Bumper'는 그냥 추격하지만, 'Attacker'는 멈춰서 공격합니다.
    protected override void PerformChaseLogic()
    {
        if (playerTransform == null) return;

        // 플레이어와의 거리를 잽니다.
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. 만약 공격 범위 안이고, 넉백/그로기 중이 아니라면
        if (distanceToPlayer <= attackRange &&
            currentState != State.KnockedBack && currentState != State.Groggy)
        {
            // [공격!]
            currentState = State.Attacking;     // 1. 상태를 '공격 중'으로 변경
            attackTimer = attackDuration;       // 2. 공격 타이머(1초) 시작
            PerformAttack();                    // 3. 실제 공격(히트박스) 실행
        }
        // 2. 공격 범위 밖이라면
        else
        {
            // [추격!]
            // 'base.PerformChaseLogic()'를 호출하여 부모의 "그냥 추격" 기능을 실행합니다.
            base.PerformChaseLogic(); 
        }
    }

    // [추가!] 'Attacker' 전용 공격(히트박스) 함수
    private void PerformAttack()
    {
        Debug.Log("Attacker 몬스터 공격!");
        
        // 1. 몬스터의 공격 히트박스 생성
        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(attackPos.position, attackBoxSize, 0);

        foreach (Collider2D playerCollider in hitPlayers)
        {
            // 2. "Player" 태그를 가진 콜라이더를 찾음
            if (playerCollider.CompareTag("Player"))
            {
                // 3. 플레이어의 Player_Health 스크립트를 찾음
                Player_Health playerHealth = playerCollider.GetComponent<Player_Health>();
                if (playerHealth != null)
                {
                    // 4. 플레이어의 Player_TakeDamaged 함수를 호출 (넉백 방향 계산을 위해 내 위치 전달)
                    playerHealth.Player_TakeDamaged(transform.position);
                    break;
                }
            }
        }
    }

    // [추가!] 공격 범위를 씬에서 노란색 네모로 볼 수 있게 함
    private void OnDrawGizmos()
    {
        if (attackPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPos.position, attackBoxSize);
        }
    }
}