// 유니티 엔진의 기본 기능을 사용하겠다는 선언입니다.
using UnityEngine;

// 'Player_Move'라는 이름의 스크립트(컴포넌트)를 선언합니다.
public class Player_Move : MonoBehaviour
{
    // [기획 변경] 플레이어가 움직일 수 있게 되었으므로, 이동 속도를 인스펙터에서 설정합니다.
    public float speed = 5.0f;
    // [기획 변경] 플랫포머 게임이 되었으므로, 점프 힘을 설정합니다.
    public float jump = 7.0f;

    // [기획 변경] 점프 상태를 확인해 공중에서 연속 점프를 막습니다.
    public bool isJump = false;
    
    // Rigidbody2D(물리) 부품을 담을 변수입니다.
    private Rigidbody2D rgd;

    
    // Start() 함수보다 먼저, 게임 시작 전 딱 한 번 호출됩니다.
    void Awake()
    {
        // 물리 부품을 'rgd' 변수에 할당합니다.
        rgd = GetComponent<Rigidbody2D>();
    }

    // 매 프레임마다 점프 입력을 확인합니다.
    void Update()
    {
        Jump();
    }

    // 물리 업데이트 주기에 맞춰 실행됩니다.
    private void FixedUpdate()
    {
        // [수정!] 'A'키(처형)와의 충돌을 피하기 위해 방향키 입력을 직접 받도록 코드를 변경했습니다.
        
        // 일단 좌우 입력을 0으로 초기화합니다.
        float inputX = 0f;

        // '왼쪽 방향키'가 눌리면 -1을 줍니다.
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            inputX = -1.0f;
        }
        // '오른쪽 방향키'가 눌리면 1을 줍니다.
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            inputX = 1.0f;
        }
        
        // 현재 속도를 가져옵니다.
        Vector2 v = rgd.linearVelocity;
        // 위에서 계산된 'inputX' (방향키 입력)와 'speed'를 곱해 X축 속도를 설정합니다.
        v.x = inputX * speed;
        // Rigidbody에 최종 속도를 적용해 플레이어를 움직입니다.
        rgd.linearVelocity = v;
    }

    // [기획 변경] 점프 기능을 담당하는 함수입니다.
    void Jump()
    {
        // 스페이스바가 '눌리는 순간'을 감지합니다.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 'isJump'가 false (즉, 땅에 있을 때)일 때만 점프합니다.
            if (!isJump)
            {
                // 콘솔에 "jump"라고 테스트 로그를 찍습니다.
                Debug.Log("jump");
                // 점프 상태로 변경
                isJump = true; 
                // 'jump' 변수의 힘만큼 위쪽으로 '순간적인 힘(Impulse)'을 가해 점프시킵니다.
                rgd.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
            }
        }
    }

    
    // [기획 변경] 바닥 충돌을 감지하는 함수입니다.
    private void OnCollisionEnter2D(Collision2D other)
    {
        // 만약 부딪힌 물체의 태그가 "Ground" (땅)라면
        if (other.gameObject.tag.Equals("Ground"))
        {
            // 'isJump'를 false (땅)로 바꿔 다시 점프할 수 있게 합니다.
            isJump = false;
        }
    }
}