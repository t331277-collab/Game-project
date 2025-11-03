using UnityEngine;

public class Player_Attack : MonoBehaviour
{

    Rigidbody2D rgd;
    Animator animator;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        rgd = GetComponent<Rigidbody2D>();
    }

    private float curTime;
    public float coolTime = 0.5f;
    public Transform pos;
    public Vector2 boxSize;

    // Update is called once per frame
    void Update()
    {
        //'Z'버튼을 공격
        
        if (curTime <= 0)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                //animator.SetTrigger("Attack");
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);

                foreach (Collider2D collider in collider2Ds)
                {
                    if(collider.tag == "Enemy")
                    {
                        collider.GetComponent<enemy>().TakeDamageZ();
                    }
                    Debug.Log(collider.tag);
                
                }

                curTime = coolTime;
            }
            else if (Input.GetKey(KeyCode.X))
            {
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);

                foreach (Collider2D collider in collider2Ds)
                {
                    if (collider.tag == "Enemy")
                    {
                        collider.GetComponent<enemy>().TakeDamageX();
                    }
                    Debug.Log(collider.tag);

                }

                curTime = coolTime;
            }
            else if (Input.GetKey(KeyCode.C))
            {
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);

                foreach (Collider2D collider in collider2Ds)
                {
                    if (collider.tag == "Enemy")
                    {
                        collider.GetComponent<enemy>().TakeDamageC();
                    }
                    Debug.Log(collider.tag);

                }

                curTime = coolTime;
            }

        }
        else
        {
            curTime -= Time.deltaTime;
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos.position, boxSize);
    }

}
