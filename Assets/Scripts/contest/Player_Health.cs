using UnityEngine; // 유니티 엔진 API

public class Player_Health : MonoBehaviour // 플레이어 체력 및 피격 반응 처리
{
    [Header("Health")]
    public float health = 3.0f; // 현재 체력

    [Header("Knockback")]
    [SerializeField] float knockbackForce = 2.0f; // 수평 넉백 세기
    [SerializeField] float knockupForce = 0.5f;   // 위로 살짝 튕김(0이면 비활성)

    Rigidbody2D rgd; // 캐싱된 Rigidbody2D

    void Awake() // 오브젝트가 활성화될 때 1회 호출
    {
        rgd = GetComponent<Rigidbody2D>(); // 동일 오브젝트의 Rigidbody2D 가져오기
    }

    // 가해자(또는 충돌점) 기준 반대 방향으로 넉백
    public void Player_TakeDamaged(Vector2 fromWorldPos) // 가해자/접점 월드 좌표
    {
        health -= 1f; // 체력 1 감소

        // 밀어낼 방향 = (플레이어 - 가해자) 정규화
        Vector2 dir = ((Vector2)transform.position - fromWorldPos).normalized;

        // 거의 같은 위치면 안전하게 '뒤쪽(바라보는 반대)'으로 대체
        if (dir.sqrMagnitude < 0.0001f)
            dir = -transform.right;

        // 기존 X 속도 제거(넉백 깔끔), Y는 유지(중력/점프 보존)
        rgd.linearVelocity = new Vector2(0f, rgd.linearVelocity.y);

        // 임펄스 벡터 = 뒤 방향 + 위 방향(옵션)
        Vector2 impulse = dir * knockbackForce + Vector2.up * knockupForce;

        // 임펄스 적용(한 번에 튕기듯 밀기)
        rgd.AddForce(impulse, ForceMode2D.Impulse);
        //rgd.AddForce(Vector2.left * 100f, ForceMode2D.Impulse);
    }

    // 비-트리거 충돌 콜백
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy")) // Enemy 태그와 충돌 시
        {
            Vector2 hitPoint = collision.GetContact(0).point; // 첫 번째 접촉 지점
            Player_TakeDamaged(hitPoint); // 접점 기준 반대 방향으로 넉백 및 데미지

            // 대안: 가해자 콜라이더의 중심 기준으로 넉백
            // Player_TakeDamaged(collision.collider.bounds.center);

            Debug.Log("Collider Enemy");
        }
    }
}
