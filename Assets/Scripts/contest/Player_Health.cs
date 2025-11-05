using UnityEngine;

public class Player_Health : MonoBehaviour
{
    [Header("Health")]
    public float health = 3.0f;

    [Header("Knockback")]
    [SerializeField] float knockbackForce = 2.0f; // 좌우 밀림
    [SerializeField] float knockupForce = 0.5f; // 살짝 위로 튕김(원치 않으면 0)

    Rigidbody2D rgd;

    void Awake()
    {
        rgd = GetComponent<Rigidbody2D>();
    }

    
    /// 때린 쪽 좌표를 모를 때: 캐릭터의 '뒤쪽'(바라보는 반대)으로 살짝 밀림
    
    public void Player_TakeDamaged()
    {
        health -= 1f;

        if (rgd == null) return;

        // 바라보는 반대 방향(사이드뷰 기준). localScale.x 로 좌우판정하는 경우가 흔함.
        // 오른쪽을 보고 있으면 뒤는 -right, 왼쪽을 보고 있으면 뒤는 +right가 됨.
        Vector2 back = -transform.right;

        // 기존 가로 속도는 잠깐 지워 주면 더 깔끔하게 밀려남
        rgd.linearVelocity = new Vector2(0f, rgd.linearVelocity.y);

        // 살짝 뒤+위로 임펄스
        Vector2 impulse = back * knockbackForce + Vector2.up * knockupForce;
        rgd.AddForce(impulse, ForceMode2D.Impulse);
    }