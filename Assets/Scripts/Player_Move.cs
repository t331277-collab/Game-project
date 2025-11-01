using UnityEngine;

public class Player_Move : MonoBehaviour
{

    public float speed = 5.0f;
    public float jump = 7.0f;

    public bool isJump = false;
    
    private Rigidbody2D rgd;
    private Vector2 vector;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rgd = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Jump();
    }

    private void FixedUpdate()
    {
        float inputX = Input.GetAxisRaw("Horizontal");

        Vector2 v = rgd.linearVelocity;

        v.x = inputX * speed;
        rgd.linearVelocity = v;
    }

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

    

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag.Equals("Ground"))
        {
            isJump = false;
        }
    }
}
