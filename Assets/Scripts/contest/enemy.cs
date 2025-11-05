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
    // [기획 변경] 'Groggy' (그로기) 상태를 AI 상태 목록(enum)에 추가합니다.
    private enum State { Patrolling, Chasing, KnockedBack, Groggy }
    // 몬스터의 현재 AI 상태를 저장할 변수입니다.
    private State currentState;

    // --- AI 순찰(Patrol) 변수 ---
    // [기획 변경] 순찰 AI를 위한 변수들입니다.
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
    // [기획 변경] 추격 AI를 위한 변수들입니다.
    [Header("AI - Chase")]
    // [인스펙터 노출] 플레이어 추격 속도입니다.
    public float chaseSpeed = 4f;
    // [인스펙터 노출] 플레이어를 감지할 수 있는 거리입니다.
    public float detectionRange = 10f;
    // 플레이어의 위치(Transform) 정보를 저장할 변수입니다.
    private Transform playerTransform;

    // --- 전투(Combat) 변수 ---
    [Header("Combat")]
    // [인스펙터 노출] 이 몬스터를 그로기 상태로 만들기 위해 눌러야 할 키의 순서 목록입니다.
    public List<KeyCode> killSequence = new List<KeyCode>();
    // [인스펙터 노출] 넉백 시 받을 물리적인 힘의 크기입니다.
    public float knockbackForce = 150f;
    // [인스펙터 노출] 넉백 힘의 추가 배율입니다.
    public float knockbackMultiplier = 1.0f;
    // [인스펙터 노출] 넉백 상태가 지속될 시간(초)입니다.
    public float knockbackDuration = 0.2f;

    // [기획 변경] 그로기 지속 시간을 인스펙터에서 설정할 수 있습니다.
    [Header("Combat")]
    public float groggyDuration = 3.0f; 
    
    // 현재 콤보(killSequence)의 몇 번째 순서를 맞춰야 하는지 기억하는 숫자(인덱스)입니다.
    private int currentSequenceIndex = 0; 
    // 넉백 상태가 얼마나 지속되었는지 시간을 잴 타이머 변수입니다.
    private float knockbackTimer;
    // [기획 변경] 그로기 상태가 얼마나 지속되었는지 시간을 잴 타이머 변수입니다.
    private float groggyTimer; 

    // --- 컴포넌트 참조 ---
    // 이 오브젝트의 Rigidbody2D(물리) 부품을 담을 비공개 변수입니다.
    private Rigidbody2D rgd;
    // 이 오브젝트의 SpriteRenderer(이미지) 부품을 담을 비공개 변수입니다. (좌우 반전용)
    private SpriteRenderer spriteRenderer;
    
    // 게임 시작 시 한 번 호출됩니다.
    void Start()
    {
        // 'rgd' 변수에 이 오브젝트의 Rigidbody2D 부품을 넣습니다.
        rgd = GetComponent<Rigidbody2D>();
        // 'spriteRenderer' 변수에 SpriteRenderer 부품을 넣습니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
        // [기획 변경] 몬스터는 '순찰' 상태로 시작합니다.
        currentState = State.Patrolling;
        // 현재 위치를 순찰의 중심점('startPosition')으로 저장합니다.
        startPosition = transform.position;
        // [기획 변경] 플랫포머 게임이므로 중력을 1로 설정합니다.
        rgd.gravityScale = 1f; 
        // 몬스터가 넘어지지 않게 Z축 회전을 고정(Freeze)시킵니다.
        rgd.constraints = RigidbodyConstraints2D.FreezeRotation; 
        // 씬에서 "Player" 태그를 가진 오브젝트를 찾습니다.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        // 만약 'playerObj'를 성공적으로 찾았다면
        if (playerObj != null)
        {
            // 찾은 플레이어의 위치 정보(transform)를 'playerTransform' 변수에 저장합니다.
            playerTransform = playerObj.transform;
        }
    }

    // 물리 업데이트 주기에 맞춰 실행 (AI의 뇌)
    void FixedUpdate()
    {
        // 플레이어 없으면 정지
        if (playerTransform == null) return; 

        // [기획 변경] AI 상태(State)에 따라 다른 행동을 합니다.
        switch (currentState)
        {
            // 순찰 상태일 때
            case State.Patrolling:
                PerformPatrol();  // 순찰 행동 실행
                CheckForPlayer(); // 플레이어 감지 실행
                break;
            // 추격 상태일 때
            case State.Chasing:
                PerformChase(); // 추격 행동 실행
                break;
            // 넉백 상태일 때 (AI가 넉백을 방해하지 못하게 함)
            case State.KnockedBack:
                knockbackTimer -= Time.fixedDeltaTime; // 넉백 시간 감소
                if (knockbackTimer <= 0) // 넉백 시간이 끝나면
                {
                    currentState = State.Chasing; // 다시 추격 상태로
                }
                // (넉백 중에는 아무런 이동 AI도 실행하지 않음)
                break;
            // [기획 변경] 그로기 상태일 때
            case State.Groggy:
                rgd.linearVelocity = Vector2.zero; // 이동 속도를 0으로 고정 (멈춤)
                groggyTimer -= Time.fixedDeltaTime; // 그로기 시간 감소
                if (groggyTimer <= 0) // 그로기 시간이 끝나면
                {
                    Debug.Log("그로기 상태 풀림!");
                    currentState = State.Chasing; // 다시 추격 상태로
                }
                break;
        }
    }

    // [기획 변경] 순찰 AI 함수
    private void PerformPatrol()
    {
        // 현재 이동 방향에 따라 속도를 설정
        float currentSpeed = movingRight ? patrolSpeed : -patrolSpeed;
        // X축(좌우) 속도만 변경하고 Y축(상하)은 유지
        rgd.linearVelocity = new Vector2(currentSpeed, rgd.linearVelocity.y);
        
        // 오른쪽 순찰 한계에 도달하면
        if (movingRight && transform.position.x >= startPosition.x + patrolDistance)
        {
            movingRight = false; // 왼쪽으로 방향 전환
            spriteRenderer.flipX = true; // 왼쪽 보기
        }
        // 왼쪽 순찰 한계에 도달하면
        else if (!movingRight && transform.position.x <= startPosition.x - patrolDistance)
        {
            movingRight = true; // 오른쪽으로 방향 전환
            spriteRenderer.flipX = false; // 오른쪽 보기
        }
    }

    // [기획 변경] 플레이어 감지 AI 함수
    private void CheckForPlayer()
    {
        // 넉백 중에는 감지 안 함
        if (currentState == State.KnockedBack) return; 
        
        // 플레이어와의 거리를 계산
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        // 거리가 '감지 범위'보다 가까우면
        if (distanceToPlayer < detectionRange)
        {
            // '추격' 상태로 변경
            currentState = State.Chasing;
        }
    }

    // [기획 변경] 추격 AI 함수
    private void PerformChase()
    {
        // 플레이어가 몬스터의 오른쪽에 있는지 왼쪽에 있는지 방향을 (-1 또는 1) 계산
        float direction = playerTransform.position.x > transform.position.x ? 1f : -1f;
        // '추격 속도'로 플레이어 방향으로 X축 속도 변경
        rgd.linearVelocity = new Vector2(direction * chaseSpeed, rgd.linearVelocity.y);
        // 플레이어를 바라보도록 스프라이트 뒤집기
        spriteRenderer.flipX = (direction < 0);
    }

    // [수정] 넉백 함수 (AI 상태를 'KnockedBack'으로 변경)
    private void PerformKnockback()
    {
        if (playerTransform == null) return;
        Debug.Log("넉백!");
        
        // [수정!] AI가 넉백을 방해하지 못하도록 상태를 '넉백'으로 변경
        currentState = State.KnockedBack; 
        // '넉백 지속 시간'만큼 타이머 설정
        knockbackTimer = knockbackDuration; 
        // AI가 부여하던 속도를 0으로 초기화 (넉백이 잘 먹히도록)
        rgd.linearVelocity = Vector2.zero; 
        
        // 플레이어의 반대 방향으로
        float direction = transform.position.x > playerTransform.position.x ? 1f : -1f;
        // '순간적인 힘(Impulse)'을 가하여 몬스터를 튕겨냄 (위로도 2f만큼 살짝 띄움)
        rgd.AddForce(new Vector2(direction * knockbackForce * knockbackMultiplier, 2f), ForceMode2D.Impulse);
    }

    // [수정!] ZXC 콤보 판정 함수
    private void ProcessNextStep(bool success)
    {
        // (성공/실패 로그 출력)
        if (success) { Debug.Log("성공! (" + killSequence[currentSequenceIndex] + ")"); }
        else { Debug.Log("실패! 플레이어 공격! (" + killSequence[currentSequenceIndex] + " 실패)"); }
        
        // 다음 콤보 순서로 넘김
        currentSequenceIndex++; 
        
        // [기획 변경] 콤보의 마지막 순서에 도달했을 때
        if (currentSequenceIndex >= killSequence.Count)
        {
            // [기획 변경] 마지막 콤보를 "성공"했다면
            if (success)
            {
                Debug.Log("그로기 상태 돌입!");
                // (Destroy(gameObject) 대신) AI 상태를 'Groggy'로 변경
                currentState = State.Groggy; 
                // 'groggyDuration'(3초)만큼 타이머 설정
                groggyTimer = groggyDuration; 
                // 즉시 멈춤
                rgd.linearVelocity = Vector2.zero; 
                // 콤보 순서를 0으로 초기화 (그로기 풀리면 다시 Z부터)
                currentSequenceIndex = 0; 
            }
            // 마지막 콤보를 "실패"했다면
            else 
            {
                Debug.Log("마지막 일격 실패. 순서 초기화.");
                PerformKnockback(); // 넉백만 하고
                currentSequenceIndex = 0; // 콤보 초기화
            }
        }
        // (콤보가 아직 남았다면 - 예: ZXC 중 Z만 맞힘)
        else
        {
            Debug.Log("다음 키 (" + killSequence[currentSequenceIndex] + ") 준비.");
            PerformKnockback(); // 넉백만 하고 다음 순서 대기
        }
    }

    // [수정!] ZXC 키 입력을 받는 내부 함수
    private void ProcessHit(KeyCode pressedKey)
    {
        // [기획 변경] 몬스터가 '그로기' 상태일 때는 ZXC 콤보가 안 먹히도록 함
        if (currentState == State.Groggy) return;
        
        // (키 목록 없는 몹은 즉사)
        if (killSequence.Count == 0)
        {
            Destroy(gameObject);
            return;
        }
        
        // '눌린 키'가 '현재 순서의 키'와 같다면 '성공(true)'으로
        if (pressedKey == killSequence[currentSequenceIndex]) { ProcessNextStep(true); }
        // 다르다면 '실패(false)'로 'ProcessNextStep' 함수를 호출
        else { ProcessNextStep(false); }
    }

    // --- 공개 함수들 (Player_Attack.cs가 호출하는 부분) ---
    
    // 'Player_Attack'가 Z키 공격을 성공시켰을 때 이 함수를 호출합니다.
    public void TakeDamageZ() 
    { 
        // 내부적으로 'ProcessHit' 함수에 'Z'키 정보를 넘깁니다.
        ProcessHit(KeyCode.Z); 
    }
    // 'Player_Attack'가 X키 공격을 성공시켰을 때 이 함수를 호출합니다.
    public void TakeDamageX() 
    { 
        // 내부적으로 'ProcessHit' 함수에 'X'키 정보를 넘깁니다.
        ProcessHit(KeyCode.X); 
    }
    // 'Player_Attack'가 C키 공격을 성공시켰을 때 이 함수를 호출합니다.
    public void TakeDamageC() 
    { 
        // 내부적으로 'ProcessHit' 함수에 'C'키 정보를 넘깁니다.
        ProcessHit(KeyCode.C); 
    }

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
}