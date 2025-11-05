// 유니티 엔진 기능을 사용합니다.
using UnityEngine;
// List<> (목록) 자료구조를 사용하기 위해 선언합니다.
using System.Collections.Generic;

// 이 스크립트를 붙이면 Rigidbody2D와 SpriteRenderer가 자동으로 추가되도록 합니다.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
// 'Enemy'라는 이름의 스크립트(컴포넌트)를 선언합니다.
public class Enemy : MonoBehaviour
{
    // --- AI 상태 변수 ---
    // 몬스터가 가질 수 있는 상태(순찰, 추격, 넉백)를 정의합니다.
    private enum State { Patrolling, Chasing, KnockedBack }
    // 몬스터의 현재 상태를 저장할 변수입니다.
    private State currentState;

    // --- AI 순찰(Patrol) 변수 ---
    // [인스펙터 노출] 인스펙터 창에 "AI - Patrol" 헤더를 추가합니다.
    [Header("AI - Patrol")]
    // [인스펙터 노출] 순찰 속도입니다.
    public float patrolSpeed = 2f;
    // [인스펙터 노출] 시작 위치에서 좌우로 순찰할 거리입니다.
    public float patrolDistance = 5f;
    // 몬스터가 순찰할 중심점(시작 위치)을 저장할 변수입니다.
    private Vector3 startPosition;
    // 순찰 시 현재 오른쪽으로 가고 있는지(true) 저장할 변수입니다.
    private bool movingRight = true;

    // --- AI 추격(Chase) 변수 ---
    [Header("AI - Chase")]
    // [인스펙터 노출] 플레이어 추격 속도입니다.
    public float chaseSpeed = 4f;
    // [인스펙터 노출] 플레이어를 감지할 수 있는 거리입니다.
    public float detectionRange = 10f;
    // 플레이어의 위치(Transform) 정보를 저장할 변수입니다.
    private Transform playerTransform;

    // --- 전투(Combat) 변수 ---
    [Header("Combat")]
    // [인스펙터 노출] 이 몬스터를 잡기 위해 눌러야 할 키의 순서 목록입니다.
    public List<KeyCode> killSequence = new List<KeyCode>();
    // [인스펙터 노출] 넉백 시 받을 물리적인 힘의 크기입니다.
    public float knockbackForce = 150f;
    // [인스펙터 노출] 넉백 힘의 추가 배율입니다.
    public float knockbackMultiplier = 1.0f;
    // [인스펙터 노출] 넉백 상태가 지속될 시간(초)입니다.
    public float knockbackDuration = 0.2f;
    
    // 현재 몇 번째 키 순서(인덱스)를 처리해야 하는지 저장하는 숫자입니다. (0 = 첫 번째)
    private int currentSequenceIndex = 0; 
    // 넉백 상태가 지속된 시간을 잴 타이머 변수입니다.
    private float knockbackTimer;

    // --- 컴포넌트 참조 ---
    // 이 오브젝트의 Rigidbody2D(물리 부품)를 담을 비공개 변수입니다.
    private Rigidbody2D rgd;
    // 이 오브젝트의 SpriteRenderer(이미지 부품)를 담을 비공개 변수입니다. (좌우 반전용)
    private SpriteRenderer spriteRenderer;
    
    // 게임 시작 시 한 번 호출됩니다.
    void Start()
    {
        // 'rgd' 변수에 이 오브젝트의 Rigidbody2D 부품을 넣습니다.
        rgd = GetComponent<Rigidbody2D>();
        // 'spriteRenderer' 변수에 SpriteRenderer 부품을 넣습니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
        // AI의 첫 상태를 '순찰'로 설정합니다.
        currentState = State.Patrolling;
        // 현재 위치를 순찰의 중심점('startPosition')으로 저장합니다.
        startPosition = transform.position;
        // 중력(gravityScale)을 1로 설정하여 바닥으로 떨어지게 합니다.
        rgd.gravityScale = 1f; 
        // 몬스터가 물리적으로 굴러가지 않도록 Z축 회전을 고정(Freeze)합니다.
        rgd.constraints = RigidbodyConstraints2D.FreezeRotation; 
        // 씬에서 "Player" 태그를 가진 오브젝트를 찾습니다.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        // 만약 'playerObj'를 찾았다면 (null이 아니라면)
        if (playerObj != null)
        {
            // 찾은 플레이어의 위치 정보(transform)를 'playerTransform' 변수에 저장합니다.
            playerTransform = playerObj.transform;
        }
    }

    // 물리 업데이트 주기에 맞춰 고정적으로 호출됩니다.
    void FixedUpdate()
    {
        // 만약 플레이어를 못 찾았다면(null), 아무것도 하지 않고 함수를 종료(return)합니다.
        if (playerTransform == null) return;

        // 'currentState'(현재 AI 상태)에 따라 다른 행동을 하도록 switch 문을 사용합니다.
        switch (currentState)
        {
            // [상태 1] 만약 '순찰' 상태라면
            case State.Patrolling:
                // 'PerformPatrol()' (순찰 행동) 함수를 실행합니다.
                PerformPatrol();
                // 'CheckForPlayer()' (플레이어 감지) 함수를 실행합니다.
                CheckForPlayer();
                // switch 문을 빠져나갑니다.
                break;
            // [상태 2] 만약 '추격' 상태라면
            case State.Chasing:
                // 'PerformChase()' (추격 행동) 함수를 실행합니다.
                PerformChase();
                break;
            // [상태 3] 만약 '넉백' 상태라면
            case State.KnockedBack:
                // 'knockbackTimer'를 물리 프레임 시간(fixedDeltaTime)만큼 감소시킵니다.
                knockbackTimer -= Time.fixedDeltaTime;
                // 만약 넉백 타이머가 0 이하라면 (넉백 시간이 끝났다면)
                if (knockbackTimer <= 0)
                {
                    // 다시 '추격' 상태로 돌아갑니다.
                    currentState = State.Chasing;
                }
                // (중요) 넉백 상태에서는 AI 이동(순찰/추격)을 하지 않습니다.
                break;
        }
    }

    // [AI 기능 1] 순찰 행동을 정의하는 함수입니다.
    private void PerformPatrol()
    {
        // 'movingRight'(오른쪽 이동 중)가 true면 patrolSpeed, 아니면 -patrolSpeed를 'currentSpeed'에 저장합니다.
        float currentSpeed = movingRight ? patrolSpeed : -patrolSpeed;
        // 'rgd'의 좌우 속도('linearVelocity.x')를 'currentSpeed'로, 상하 속도('linearVelocity.y')는 원래대로 설정합니다.
        rgd.linearVelocity = new Vector2(currentSpeed, rgd.linearVelocity.y);
        
        // 만약 오른쪽으로 가다가 순찰 한계('startPosition.x + patrolDistance')에 도달하면
        if (movingRight && transform.position.x >= startPosition.x + patrolDistance)
        {
            // 왼쪽으로 가도록 방향('movingRight')을 false로 바꿉니다.
            movingRight = false;
            // 스프라이트의 x축을 뒤집어서(flipX) 왼쪽을 보게 합니다.
            spriteRenderer.flipX = true;
        }
        // 만약 왼쪽으로 가다가 순찰 한계('startPosition.x - patrolDistance')에 도달하면
        else if (!movingRight && transform.position.x <= startPosition.x - patrolDistance)
        {
            // 오른쪽으로 가도록 방향('movingRight')을 true로 바꿉니다.
            movingRight = true;
            // 스프라이트 뒤집기(flipX)를 false로 돌려서 오른쪽을 보게 합니다.
            spriteRenderer.flipX = false;
        }
    }

    // [AI 기능 2] 플레이어를 감지하는 함수입니다.
    private void CheckForPlayer()
    {
        // 넉백 중에는 이 함수를 실행하지 않고 바로 종료합니다.
        if (currentState == State.KnockedBack) return; 
        
        // 몬스터와 플레이어 사이의 2D 거리를 계산합니다.
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        // 만약 이 거리가 'detectionRange'(감지 범위)보다 짧으면
        if (distanceToPlayer < detectionRange)
        {
            // AI 상태를 '추격'으로 변경합니다.
            currentState = State.Chasing;
        }
    }

    // [AI 기능 3] 플레이어를 추격하는 함수입니다.
    private void PerformChase()
    {
        // 플레이어가 몬스터보다 오른쪽에 있으면 1, 왼쪽에 있으면 -1을 'direction'에 저장합니다.
        float direction = playerTransform.position.x > transform.position.x ? 1f : -1f;
        // 'rgd'의 좌우 속도를 '방향 * 추격 속도'로 설정합니다.
        rgd.linearVelocity = new Vector2(direction * chaseSpeed, rgd.linearVelocity.y);
        // 'direction'이 음수(왼쪽)일 때만 스프라이트를 뒤집습니다. (플레이어를 바라보게 함)
        spriteRenderer.flipX = (direction < 0);
    }

    // [전투 기능 1] 넉백을 실행하는 함수입니다.
    private void PerformKnockback()
    {
        // 플레이어 정보가 없으면 넉백을 실행할 수 없으므로 함수를 종료합니다.
        if (playerTransform == null) return;
        // 콘솔에 "넉백!" 로그를 찍습니다.
        Debug.Log("넉백!");

        // AI 상태를 즉시 'KnockedBack'(넉백 중)으로 변경합니다. (이게 AI 이동을 멈춥니다)
        currentState = State.KnockedBack;
        // 넉백 타이머를 'knockbackDuration'(예: 0.2초)으로 설정합니다.
        knockbackTimer = knockbackDuration;
        
        // 넉백 힘이 AI 이동 속도와 싸우지 않도록, 현재 속도를 즉시 0으로 초기화합니다.
        rgd.linearVelocity = Vector2.zero; 

        // 플레이어의 반대 방향 (몬스터 위치 > 플레이어 위치 = 오른쪽(1))을 계산합니다.
        float direction = transform.position.x > playerTransform.position.x ? 1f : -1f;
        // 'rgd'에 '방향 * 힘 * 배율' 만큼의 '순간적인 힘(Impulse)'을 가합니다. (위로도 2f만큼 살짝 띄움)
        rgd.AddForce(new Vector2(direction * knockbackForce * knockbackMultiplier, 2f), ForceMode2D.Impulse);
    }

    // [전투 기능 2] 성공/실패 판정 및 다음 단계를 처리하는 함수입니다.
    // 'success' 변수로 '성공(true)' 또는 '실패(false)' 결과를 받습니다.
    private void ProcessNextStep(bool success)
    {
        // 만약 'success'가 true (성공)라면
        if (success) { Debug.Log("성공! (" + killSequence[currentSequenceIndex] + ")"); }
        // 만약 'success'가 false (실패)라면
        else { Debug.Log("실패! 플레이어 공격! (" + killSequence[currentSequenceIndex] + " 실패)"); }
        
        // 성공/실패와 관계없이, 다음 키 순서로 넘어가기 위해 인덱스를 1 증가시킵니다.
        currentSequenceIndex++; 
        
        // 만약 현재 인덱스가 'killSequence' 목록의 전체 개수보다 크거나 같아졌다면 (즉, 순서가 끝났다면)
        if (currentSequenceIndex >= killSequence.Count)
        {
            // 마지막 결과에 따라 다른 로그를 남깁니다.
            if (success) Debug.Log("몬스터 사살!");
            else Debug.Log("모든 공격 기회 소진. 몬스터 소멸.");
            // 이 몬스터 오브젝트를 파괴(제거)합니다.
            Destroy(gameObject);
        }
        // 아직 다음 키 순서가 남아있다면
        else
        {
            // 콘솔에 다음 순서의 키를 알려줍니다.
            Debug.Log("다음 키 (" + killSequence[currentSequenceIndex] + ") 준비.");
            // 다음 공격을 위해 몬스터를 넉백시킵니다.
            PerformKnockback();
        }
    }

    // [전투 기능 3] 'Player_Attack'에서 받은 키가 올바른지 판정하는 내부 함수입니다.
    private void ProcessHit(KeyCode pressedKey)
    {
        // 만약 'killSequence' 목록이 비어있다면 (즉, ZXC 설정이 없는 몹이라면)
        if (killSequence.Count == 0)
        {
            // 즉시 파괴하고 함수를 종료합니다.
            Destroy(gameObject);
            return;
        }
        
        // 만약 '눌린 키'가 '현재 순서의 키'와 같다면
        if (pressedKey == killSequence[currentSequenceIndex]) 
        { 
            // '성공(true)'으로 'ProcessNextStep' 함수를 호출합니다.
            ProcessNextStep(true); 
        }
        // 만약 '눌린 키'가 '현재 순서의 키'와 다르다면
        else 
        { 
            // '실패(false)'로 'ProcessNextStep' 함수를 호출합니다.
            ProcessNextStep(false); 
        }
    }

    // --- 공개 함수들 (Player_Attack.cs가 호출하는 부분) ---
    
    // 'Player_Attack'에서 Z키 공격이 맞았을 때 이 함수를 호출합니다.
    public void TakeDamageZ() 
    { 
        Debug.Log("Z키 공격 감지됨!");
        // Z키가 눌렸다고 'ProcessHit' 함수에 전달합니다.
        ProcessHit(KeyCode.Z); 
    }
    // 'Player_Attack'에서 X키 공격이 맞았을 때 이 함수를 호출합니다.
    public void TakeDamageX() 
    { 
        Debug.Log("X키 공격 감지됨!");
        // X키가 눌렸다고 'ProcessHit' 함수에 전달합니다.
        ProcessHit(KeyCode.X); 
    }
    // 'Player_Attack'에서 C키 공격이 맞았을 때 이 함수를 호출합니다.
    public void TakeDamageC() 
    { 
        Debug.Log("C키 공격 감지됨!");
        // C키가 눌렸다고 'ProcessHit' 함수에 전달합니다.
        ProcessHit(KeyCode.C); 
    }
<<<<<<< HEAD

    // [기획 변경] '처형' 당했을 때 Player_Attack이 호출하는 함수
    public void Execute()
    {
        Debug.Log("처형!");
        // 몬스터를 즉시 파괴(제거)합니다.
        Destroy(gameObject);
    }
    
    // [기획 변경] Player_Attack이 "지금 그로기 상태야?"라고 물어볼 때 쓰는 함수
    public bool IsGroggy()
    {
        // '현재 상태(currentState)'가 '그로기(Groggy)'와 같으면 true(참)를, 아니면 false(거짓)를 반환(return)합니다.
        return currentState == State.Groggy;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag("Player"))
        {
            Debug.Log("This is Player");

            if (collision.collider.TryGetComponent<Player_Health>(out var playerHealth))
            {
                // 단순 데미지 + 기본 노크백
                playerHealth.Player_TakeDamaged();

                
            }
        }
    }
=======
>>>>>>> parent of 1415d4e (처형 추가)
}