using UnityEngine;

public class Player_Health : MonoBehaviour
{
    [Header("Health")]
    public float health = 3.0f;

    [Header("Knockback")]
    [SerializeField] float knockbackForce = 2.0f; // 좌우 밀림 세기
    [SerializeField] float knockupForce = 0.5f; // 위로 튕김(원치 않으면 0)

    Rigidbody2D rgd;

    void Awake()
    {
        rgd = GetComponent<Rigidbody2D>();
    }

  
    /// 충돌/가해자 위치 기반: "가해자 → 플레이어" 방향으로 정확히 밀림(= 뒤로)
    public void Player_TakeDamaged(Vector2 fromWorldPos)
    {
        // HP 감소
        health -= 1f;

        // (플레이어 - 가해자) = 가해자 반대(뒤) 방향
        Vector2 dir = ((Vector2)transform.position - fromWorldPos).normalized;

        if (dir.sqrMagnitude < 0.0001f) dir = -transform.right; // 동일 위치 대비 안전장치


        //뒤로 밀려나는 함수 
        rgd.linearVelocity = new Vector2(0f, rgd.linearVelocity.y);
        Vector2 impulse = dir * knockbackForce + Vector2.up * knockupForce;
        rgd.AddForce(impulse, ForceMode2D.Impulse);
    }


    //충돌 트리거
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            // 충돌 접점(또는 가해자 중심) 기준으로 "반대(뒤)" 방향 넉백
            Vector2 hitPoint = collision.GetContact(0).point;
            Player_TakeDamaged(hitPoint);

            // 만약 접점 대신 가해자 중심을 쓰고 싶다면:
            // Player_TakeDamaged(collision.collider.bounds.center);

            Debug.Log("Collider Enemy");
        }
    }
}
