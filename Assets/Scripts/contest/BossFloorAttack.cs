using UnityEngine;

public class BossFloorAttack : MonoBehaviour
{
    [Header("설정")]
    public int damage = 15;           // 데미지
    public float duration = 1.5f;     // 유지 시간
    public float hitBoxHeight = 0.5f; // 히트박스 높이 (점프로 피하게 낮게)

    private BoxCollider2D boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        // 콜라이더를 납작하게 만들고 바닥에 붙임
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(boxCollider.size.x, hitBoxHeight);
            // 스프라이트 중심 기준 바닥으로 내림
            boxCollider.offset = new Vector2(0, -boxCollider.size.y / 2f + hitBoxHeight / 2f);
            boxCollider.isTrigger = true; // 통과 가능하게 트리거로 설정
        }
        
        Destroy(gameObject, duration); // 일정 시간 후 저절로 삭제
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player_Health playerHealth = collision.GetComponent<Player_Health>();
            if (playerHealth != null)
            {
                playerHealth.Player_TakeDamaged(transform.position);
                Debug.Log("플레이어 바닥 패턴 피격!");
            }
        }
    }

    private void OnDrawGizmos() { /* 기즈모 그리기 생략 (필요시 이전 코드 참조) */ }
}