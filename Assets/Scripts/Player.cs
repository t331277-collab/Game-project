using UnityEngine;

public class Player : MonoBehaviour
{

    Rigidbody2D rig; 
    public float moveSpeed;
    public float maxSpeed;
    public float jumpPower;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
   {
      // 왼쪽 화살표를 누르고 있는 경우
      if (Input.GetKey(KeyCode.LeftArrow))
      {         
         // 물체에 왼쪽 방향으로 물리적 힘을 가해줍니다. 즉, 왼쪽으로 이동 시킵니다.
         rig.AddForce(Vector2.left * moveSpeed, ForceMode2D.Impulse);

         // velocity 는 물체의 속도입니다. 일정 속도에 도달하면 더 이상 빨라지지 않게합니다.
         rig.linearVelocity = new Vector2(Mathf.Max(rig.linearVelocity.x, -maxSpeed), rig.linearVelocity.y);

         // scale 값을 이용해 캐릭터가 이동 방향을 바라보게 합니다.
         transform.localScale = new Vector3(-1f, 1f, 1f);
      }
      else if (Input.GetKey(KeyCode.RightArrow)) // 오른쪽 화살표를 누르고 있는 경우
      {         
         rig.AddForce(Vector2.right * moveSpeed, ForceMode2D.Impulse);
         rig.linearVelocity = new Vector2(Mathf.Min(rig.linearVelocity.x, maxSpeed), rig.linearVelocity.y);
         transform.localScale = new Vector3(1f, 1f, 1f);
      }
      else if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow)) // 이동 키를 뗀 경우
      {
         // x 속도를 줄여 이동 방향으로 아주 살짝만 움직이고 거의 바로 멈추게 합니다.
         rig.linearVelocity = new Vector3(rig.linearVelocity.normalized.x, rig.linearVelocity.y);
      }

      // 스페이스바를 누르면 점프합니다.
      if (Input.GetKeyDown(KeyCode.Space))
      {  
         if (IsGrounded())
            rig.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
      }
   }

   // 캐릭터가 땅을 밟고 있는지 체크
   bool IsGrounded()
   {
      // 캐릭터를 중심으로 아래 방향으로 ray 를 쏘아 그 곳에 Layer 타입이 Ground 인 객체가 있는지 검사합니다.
      var ray = Physics2D.Raycast(transform.position, Vector2.down, 1f, 1 << LayerMask.NameToLayer("Ground"));
      return ray.collider != null;
   }
}
