using UnityEngine;

public class Player_Move : MonoBehaviour
{
    private Rigidbody2D rgd;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [Header("Movement")]
    public float speed = 5.0f;
    public float jump = 7.0f;
    public bool isJump = false;

    // [수정] 점프 입력을 저장해둘 변수
    private bool jumpRequest = false;

    [Header("Knockback")]
    public float KBForce;
    public float KBCounter;
    public float KBToalTime;
    public bool KnockFromRight;

    private float inputX;

    void Awake()
    {
        rgd = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. 이동 입력 (기존과 동일)
        inputX = Input.GetAxisRaw("Horizontal");

        // [중요!] 점프 입력은 Update에서 받아서 '요청' 상태로 만들어둡니다.
        // 이미 점프 중이 아닐 때만 입력을 받음
        if (Input.GetKeyDown(KeyCode.Space) && !isJump)
        {
            jumpRequest = true;
        }
    }

    void FixedUpdate()
    {
        if (KBCounter <= 0)
        {
            Vector2 v = rgd.linearVelocity; // Unity 6버전 기준 linearVelocity, 구버전은 velocity
            v.x = inputX * speed;
            rgd.linearVelocity = v;

            // [중요!] 물리 업데이트 타이밍에 요청된 점프가 있다면 실행
            if (jumpRequest)
            {
                rgd.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
                Debug.Log("jump");
                isJump = true;

                // 점프를 수행했으니 요청을 초기화합니다.
                jumpRequest = false;
            }
        }
        else
        {
            // 넉백 로직 (기존 유지)
            if (KnockFromRight == true) rgd.linearVelocity = new Vector2(-KBForce, KBForce * 0.5f);
            else rgd.linearVelocity = new Vector2(KBForce, KBForce * 0.5f);
            KBCounter -= Time.fixedDeltaTime;
        }
    }

    void LateUpdate()
    {
        if (KBCounter <= 0)
        {
            // [수정] 물리 속도(rgd.linearVelocity)가 아닌 입력값(inputX) 기준으로 방향 전환
            if (inputX > 0) spriteRenderer.flipX = false;      // 오른쪽 키 입력 시
            else if (inputX < 0) spriteRenderer.flipX = true;  // 왼쪽 키 입력 시
            // inputX가 0일 때(키를 안 누를 때)는 방향을 유지합니다.

            // [참고] 애니메이션 재생 속도(걷는 모션)는 
            // 여전히 '실제 이동 속도'를 따르는 것이 자연스럽습니다.
            // (벽에 막혔을 때 제자리걸음 하는 것을 방지하기 위함)
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(rgd.linearVelocity.x));
            }
        }
    }

    // [추가 팁] 바닥 체크 개선
    // OnCollisionEnter는 '부딪히는 순간'만 체크하므로, 바닥에 계속 서있을 때 불안정할 수 있습니다.
    // OnCollisionStay2D를 쓰거나, 레이캐스트를 쓰는 것이 훨씬 좋습니다.
    // 일단 간단한 수정을 위해 Enter와 Stay를 같이 사용합니다.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGround(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckGround(collision);
    }

    private void CheckGround(Collision2D collision)
    {
        // 태그 체크
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy"))
        {
            // [중요] 떨어지는 중일 때만 착지 판정을 하는 것이 자연스럽습니다.
            // 점프해서 올라가는 도중에 벽에 닿았다고 착지 처리되면 안되니까요.
            if (rgd.linearVelocity.y <= 0.1f)
            {
                isJump = false;
                // 만약 점프했다가 착지했는데 jumpRequest가 남아있다면 제거 (버그 방지)
                jumpRequest = false;
            }
        }
    }
}