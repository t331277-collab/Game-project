using UnityEngine;

public class Enemy_Bumper : Enemy
{
    // [삭제!] Bumper가 Player_Move를 직접 알 필요가 없습니다.
    // private Player_Move player_Move;

    // [삭제!] 부모의 Start()만 쓰면 되므로 Bumper의 Start()는 필요 없습니다.
    // protected override void Start() { ... }

    // [수정!] Bumper 전용 충돌 기능
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 만약 충돌한게 Player 라면
        if(collision.gameObject.CompareTag("Player"))
        {
            // 2. 플레이어의 'Player_Health' 스크립트를 찾습니다.
            Player_Health playerHealth = collision.gameObject.GetComponent<Player_Health>();

            // 3. 스크립트를 찾았다면
            if (playerHealth != null)
            {
                // 4. [핵심!] Player_Health의 TakeDamaged 함수를 호출합니다.
                // (자신의 위치(transform.position)를 넘겨줘서 넉백 방향을 계산하게 함)
                playerHealth.Player_TakeDamaged(transform.position);

                Debug.Log("Player와 (Bumper)가 충돌하여 Player_TakeDamaged 호출");
            }
        }
    }
}