using UnityEngine;
using System.Collections; // [필수!] 코루틴 사용을 위해 추가

public class Player_Health : MonoBehaviour
{
    [Header("Health")]
    public float health = 3.0f;

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f; // 무적 지속 시간 (초)
    private bool isInvincible = false;         // 현재 무적 상태인지 체크
    public float blinkInterval = 0.1f;         // 깜빡이는 속도

    Rigidbody2D rgd;
    private Player_Move playerMove;
    private SpriteRenderer spriteRenderer; // [추가] 깜빡임 효과를 위해 필요

    void Awake()
    {
        rgd = GetComponent<Rigidbody2D>();
        playerMove = GetComponent<Player_Move>();

        // [추가] 스프라이트 렌더러 컴포넌트 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Player_TakeDamaged(Vector2 fromWorldPos)
    {
        // [추가!] 무적 상태라면 데미지와 넉백을 모두 무시하고 함수 종료
        if (isInvincible) return;

        // -------------------------------------------------------
        // 기존 로직 (데미지 및 넉백 트리거)
        // -------------------------------------------------------

        // 1. 체력 감소 (ScoreManager 등 외부 로직 연동)
        ScoreManager.Instance.TakeDamaged(1);

        // 2. Player_Move 스크립트 확인
        if (playerMove == null)
        {
            Debug.LogError("Player_Health가 Player_Move 스크립트를 찾지 못했습니다!");
            return;
        }

        // 3. Player_Move의 넉백 타이머 활성화
        playerMove.KBCounter = playerMove.KBToalTime;

        // 4. 넉백 방향 계산 및 전달
        if (transform.position.x <= fromWorldPos.x)
        {
            playerMove.KnockFromRight = true;
        }
        else
        {
            playerMove.KnockFromRight = false;
        }

        // -------------------------------------------------------
        // [추가!] 무적 및 깜빡임 코루틴 시작
        // -------------------------------------------------------
        StartCoroutine(InvincibilityCoroutine());
    }

    // [추가!] 무적 시간과 깜빡임 효과를 담당하는 코루틴
    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true; // 무적 ON

        float timer = 0f;

        while (timer < invincibilityDuration)
        {
            // 투명하게 (Alpha값 0.4)
            spriteRenderer.color = new Color(1, 1, 1, 0.4f);
            yield return new WaitForSeconds(blinkInterval);

            // 불투명하게 (Alpha값 1.0)
            spriteRenderer.color = new Color(1, 1, 1, 1f);
            yield return new WaitForSeconds(blinkInterval);

            timer += (blinkInterval * 2); // 대기한 시간만큼 타이머 증가
        }

        // 루프가 끝난 후 확실하게 원상복구
        spriteRenderer.color = new Color(1, 1, 1, 1f);
        isInvincible = false; // 무적 OFF
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Enemy_Bumper.cs 에서 처리하므로 비워둠
    }
}