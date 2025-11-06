// 유니티 엔진의 기본 기능을 사용하겠다는 선언입니다.
using UnityEngine;

// 'Player_Move'라는 이름의 스크립트(컴포넌트)를 선언합니다.
public class Player_Move : MonoBehaviour
{
    // [인스펙터 노출] 플레이어의 좌우 이동 속도를 5.0f로 기본 설정합니다.
    public float speed = 5.0f;
    // [인스펙터 노출] 플레이어의 점프 힘을 7.0f로 기본 설정합니다.
    public float jump = 7.0f;

    // [인스펙터 노출] 현재 점프 중인지(공중인지) 상태를 저장합니다.
    public bool isJump = false;
    
    // 이 오브젝트의 Rigidbody2D(물리 부품)를 담을 비공개 변수입니다.
    private Rigidbody2D rgd;

    // 이 오브젝트의 SpriteRenderer를 담을 비공개 변수입니다.
    private SpriteRenderer spriteRenderer;

    
    // Start() 함수보다 먼저, 게임 시작 전 딱 한 번 호출됩니다.
    void Awake()
    {
        // 이 오브젝트에 붙어있는 Rigidbody2D 부품을 찾아서 'rgd' 변수에 넣어둡니다.
        rgd = GetComponent<Rigidbody2D>();


        // 이 오브젝트에 붙어있는  SpriteRenderer부품을 찾아서 'spriteRenderer' 변수에 넣어둡니다.
        spriteRenderer = GetComponent<SpriteRenderer>();   
    }


    // 매 프레임마다 호출됩니다.
    void Update()
    {
        // 매 프레임 'Jump()' 함수를 호출해서 스페이스바 입력을 감시합니다.
        Jump();

        //방향 전환
        if(Input.GetButtonDown("Horizontal")) {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        
        }
    }

    // 물리 업데이트 주기에 맞춰 (기본 0.02초마다) 고정적으로 호출됩니다.
    // 물리 관련 코드는 FixedUpdate에 넣는 것이 안정적입니다.
    private void FixedUpdate()
    {
        // 키보드 좌우 방향키 (A, D) 입력을 받습니다. (왼쪽 -1, 없음 0, 오른쪽 1)
        float inputX = Input.GetAxisRaw("Horizontal");

        // 'rgd'의 현재 속도를 'v'라는 임시 변수에 복사합니다.
        Vector2 v = rgd.linearVelocity;

        // 'v'의 x값(좌우 속도)만 '(입력값 * 속도)'로 새로 설정합니다.
        // (y값(점프/낙하 속도)은 그대로 둡니다.)
        v.x = inputX * speed;
        
        // 'rgd'의 실제 속도를 우리가 수정한 'v' 값으로 덮어씌워서 이동시킵니다.
        rgd.linearVelocity = v;
    }

    // 점프 기능을 담당하는 함수입니다.
    void Jump()
    {
        // 만약 스페이스바가 "눌리는 그 순간"이라면,
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
            // 그리고 "isJump가 false가 아니라면" (즉, !isJump = 땅에 있다면!)
            if (!isJump)
            {
                // 유니티 콘솔 창에 "jump"라고 테스트 로그를 찍습니다.
                Debug.Log("jump");
                // "현재 점프 중" 상태(true)로 바꿔서 공중에서 또 점프하는 것을 막습니다.
                isJump = true;
                // 'rgd'에 '위쪽(Vector2.up) * 점프힘' 만큼 '순간적인 힘(Impulse)'을 가합니다. (점프!)
                rgd.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
            }
        }
    }

    
    // 이 오브젝트의 콜라이더가 다른 콜라이더(Collision2D other)와 "부딪히는 순간" 실행됩니다.
    private void OnCollisionEnter2D(Collision2D other)
    {
        // 만약 부딪힌 상대방('other')의 태그가 "Ground" 라면,
        if (other.gameObject.tag.Equals("Ground"))
        {
            // "땅에 닿았음" 상태(false)로 변경해 다시 점프할 수 있게 합니다.
            isJump = false;
        }
    }
}