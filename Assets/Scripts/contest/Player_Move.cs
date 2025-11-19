// 유니티 엔진의 기본 기능을 사용하겠다는 선언입니다.
using UnityEngine;

// 'Player_Move'라는 이름의 스크립트(컴포넌트)를 선언합니다.
public class Player_Move : MonoBehaviour


{
    //하강 중력 변수
    public float fallMultiplier = 2.5f;     // 내려갈 때 중력 강화 배율
    

    // [인스펙터 노출] 플레이어의 좌우 이동 속도를 5.0f로 기본 설정합니다.
    public float speed = 5.0f;
    // [인스펙터 노출] 플레이어의 점프 힘을 7.0f로 기본 설정합니다.
    public float jump = 7.0f;

    // [인스펙터 노출] 현재 점프 중인지(공중인지) 상태를 저장합니다.
    public bool isJump = false;

    //KnockBack 의 강도
    public float KBForce;

    //KnockBack 이 지속되는지
    public float KBCounter;

    //KnockBack 의 지속시간
    public float KBToalTime;

    public bool KnockFromRight;

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
        float inputX = Input.GetAxisRaw("Horizontal");

        // 움직이는 방향이 있을 때만 방향 업데이트
        if (inputX > 0f)
        {
            spriteRenderer.flipX = false; // 오른쪽 바라봄
        }
        else if (inputX < 0f)
        {
            spriteRenderer.flipX = true;  // 왼쪽 바라봄
        }
    }

    // 물리 업데이트 주기에 맞춰 (기본 0.02초마다) 고정적으로 호출됩니다.
    // 물리 관련 코드는 FixedUpdate에 넣는 것이 안정적입니다.
    private void FixedUpdate()
    {
        

        if(KBCounter <= 0)
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

            // 매 프레임 'Jump()' 함수를 호출해서 스페이스바 입력을 감시합니다.
            Jump();

            if (rgd.linearVelocity.y < 0) // 떨어지는 중
            {
                rgd.linearVelocity += Vector2.up * Physics2D.gravity.y // 중력배율
                                      * (fallMultiplier - 1f)
                                      * Time.fixedDeltaTime;
            }


        }
        else
        {
            if(KnockFromRight == true)
            {
                rgd.linearVelocity = new Vector2(-KBForce, KBForce * 0.2f); // x, y 벡터 고치면 원하는 방향으로 날아가게 변경 가능
            }
            if (KnockFromRight == false)
            {
                rgd.linearVelocity = new Vector2(KBForce, KBForce * 0.2f);
            }

            KBCounter -= Time.deltaTime;

        }



        
    }



    // 점프 기능을 담당하는 함수입니다.
    void Jump()
    {
        // 스페이스를 막 눌렀고, 땅에 있을 때만 점프
        if (Input.GetKeyDown(KeyCode.Space) && !isJump)
        {
            Debug.Log("jump");
            isJump = true;

            // 현재 x 속도는 유지하고, y 속도만 '점프 속도'로 덮어쓰기
            Vector2 v = rgd.linearVelocity;
            v.y = jump;          // 이 값을 조절해서 점프 높이/시간을 세밀하게 조정
            rgd.linearVelocity = v;
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