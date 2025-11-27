using UnityEngine;

public class Player_Move : MonoBehaviour
{
    private Rigidbody2D rgd;
    private SpriteRenderer spriteRenderer;
    // [추가!] 애니메이터 컴포넌트를 담을 변수 선언
    private Animator animator;

    [Header("Movement")]
    public float speed = 5.0f;
    public float jump = 7.0f;
    public bool isJump = false;

    // --- 넉백 관련 변수들 (기존 유지) ---
    [Header("Knockback")]
    public float KBForce;
    public float KBCounter;
    public float KBToalTime;
    public bool KnockFromRight;

    // [임시 변수] 입력을 받아두는 변수
    private float inputX;

    void Awake()
    {
        rgd = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        // [추가!] 내 오브젝트에 붙어있는 Animator 컴포넌트를 찾아 연결
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. 입력은 Update에서 받아서 변수에 저장만 해둡니다.
        inputX = Input.GetAxisRaw("Horizontal");
        
        // (flipX 로직이 여기 있었는데 LateUpdate로 이사 갔습니다!)
    }

    void FixedUpdate()
    {
        if(KBCounter <= 0)
        {
            // 2. 물리 이동 계산 (저장해둔 입력값 사용)
            Vector2 v = rgd.linearVelocity;
            v.x = inputX * speed;
            rgd.linearVelocity = v;

            // (애니메이션 파라미터 전달 로직이 여기 있었는데 LateUpdate로 이사 갔습니다!)

            Jump();
        }
        else
        {
            // 넉백 중일 때 로직 (기존 유지)
            if (KnockFromRight == true) rgd.linearVelocity = new Vector2(-KBForce, KBForce * 0.5f);
            else rgd.linearVelocity = new Vector2(KBForce, KBForce * 0.5f);

            KBCounter -= Time.fixedDeltaTime;

            
        }
    }

    // [새로 추가!] 모든 계산이 끝난 후 비주얼을 처리하는 함수
    void LateUpdate()
    {
        // 넉백 중이 아닐 때만 방향/애니메이션 업데이트
        if (KBCounter <= 0)
        {
            // --- 1. 스프라이트 방향 (flipX) ---
            // 입력(inputX)이 아니라 '실제 물리 속도(rgd.linearVelocity.x)'를 기준으로 합니다.
            // (벽에 막히면 속도가 0이 되어 방향이 안 바뀝니다. 더 자연스러움!)
            if (rgd.linearVelocity.x > 0.1f) spriteRenderer.flipX = false;
            else if (rgd.linearVelocity.x < -0.1f) spriteRenderer.flipX = true;

            // --- 2. 애니메이션 속도 (Speed) ---
            if (animator != null)
            {
                // 역시 '실제 물리 속도'의 절대값을 전달합니다.
                // (미끄러지거나 밀려날 때도 자연스럽게 걷는 모션이 나옵니다.)
                animator.SetFloat("Speed", Mathf.Abs(rgd.linearVelocity.x));
            }
        }
    }

    // Jump 함수 및 OnCollisionEnter2D (기존 유지)
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isJump)
            {
                Debug.Log("jump");
                isJump = true;
                rgd.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Ground"))
        {
            isJump = false;
        }
    }
}