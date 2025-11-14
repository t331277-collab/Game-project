using UnityEngine;

public class Player_Health : MonoBehaviour
{
    [Header("Health")]
    public float health = 3.0f; 

    // [삭제!] 이 스크립트는 넉백 '힘'을 직접 관리하지 않습니다.
    // [SerializeField] float knockbackForce = 2.0f; 
    // [SerializeField] float knockupForce = 0.5f;   

    Rigidbody2D rgd; 

    // [추가!] Player_Move 스크립트(시스템 A)에 접근하기 위한 변수
    private Player_Move playerMove;

    void Awake() 
    {
        rgd = GetComponent<Rigidbody2D>(); 
        
        // [추가!] Player_Move 스크립트를 찾아서 변수에 저장
        playerMove = GetComponent<Player_Move>();
    }

    // [수정!] 이 함수는 이제 '시스템 A' (KBCounter)를 활성화시킵니다.
    public void Player_TakeDamaged(Vector2 fromWorldPos) 
    {
        // 1. 체력 1 감소
        health -= 1f;

        // 2. Player_Move 스크립트가 있는지 확인 (오류 방지)
        if (playerMove == null)
        {
            Debug.LogError("Player_Health가 Player_Move 스크립트를 찾지 못했습니다!");
            return;
        }

        // 3. [핵심!] Player_Move의 넉백 타이머를 활성화
        playerMove.KBCounter = playerMove.KBToalTime;

        // 4. [핵심!] 넉백 방향 계산 및 'Player_Move' 변수에 저장
        // (플레이어 X위치 - 공격자 X위치)
        if (transform.position.x <= fromWorldPos.x)
        {
            // 공격자가 오른쪽에 있음 -> 왼쪽으로 넉백
            playerMove.KnockFromRight = true; 
        }
        else
        {
            // 공격자가 왼쪽에 있음 -> 오른쪽으로 넉백
            playerMove.KnockFromRight = false;
        }
        
        // [삭제!] 시스템 B (AddForce) 코드는 모두 삭제합니다.
    }

    // [수정!] 이 함수는 비워둡니다.
    // 몬스터와의 충돌 처리는 'Enemy_Bumper.cs'가 전담해야 
    // 넉백이 두 번 중복으로 실행되는 것을 막을 수 있습니다.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*
        if (collision.collider.CompareTag("Enemy")) 
        {
            // 이 코드는 Enemy_Bumper.cs가 대신 처리함
        }
        */
    }
}