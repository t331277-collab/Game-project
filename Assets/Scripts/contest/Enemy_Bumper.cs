using UnityEngine;

// [핵심!] MonoBehaviour가 아닌 'Enemy' (부모 스크립트)를 상속받습니다.
public class Enemy_Bumper : Enemy
{
    // [추가!] 부모에게는 없었던, Bumper 전용 'player_Move' 변수
    private Player_Move player_Move;

    // [추가!] 부모의 Start() 기능을 실행하고, Bumper 전용 기능도 추가합니다.
    protected override void Start()
    {
        // 'base.Start()'는 부모(Enemy.cs)의 Start() 함수를 먼저 실행하라는 뜻입니다.
        // (이걸 해야 rgd, playerTransform 등이 모두 설정됩니다)
        base.Start();

        // Bumper 전용 기능: Player_Move 스크립트를 찾아 저장합니다.
        // (playerTransform은 부모가 찾아줬으므로 그걸 씁니다)
        if (playerTransform != null)
        {
            player_Move = playerTransform.GetComponent<Player_Move>();
        }
    }

    // [추가!] Bumper 전용 기능: 플레이어와 부딪혔을 때 플레이어를 넉백시킵니다.
    // (이 코드는 원래 Enemy.cs에 있던 것을 그대로 옮겨온 것입니다)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //만약 충돌한게 Player 이다
        if(collision.gameObject.CompareTag("Player") && player_Move != null)
        {
            //시간 카운트 시작
            player_Move.KBCounter = player_Move.KBToalTime;

            //충돌지점 계산
            if(collision.transform.position.x <= transform.position.x)
            {
                //오른쪽에서 충돌
                player_Move.KnockFromRight = true;
            }
            if (collision.transform.position.x > transform.position.x)
            {
                //왼쪽에서 충돌
                player_Move.KnockFromRight = false;
            }

            Debug.Log("Player와 (Bumper)가 충돌하여 넉백시킴");
        }
    }
}