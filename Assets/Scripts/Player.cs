using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KinematicMover2D : MonoBehaviour
{
    [Header("Move")]
    public float speed = 5f;                 // 유닛/초
    public bool normalizeInput = true;       // 대각선 속도 보정

    [Header("Rotate (옵션)")]
    public bool faceMoveDirection = false;   // 이동 방향으로 회전할지
    public float rotateSpeed = 720f;         // 도/초

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;      // Static 금지, Kinematic으로!
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void FixedUpdate()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        if (normalizeInput) input = input.normalized;

        Vector2 next = rb.position + input * speed * Time.fixedDeltaTime;
        rb.MovePosition(next);

        if (faceMoveDirection && input.sqrMagnitude > 0.0001f)
        {
            float target = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
            float newZ = Mathf.MoveTowardsAngle(rb.rotation, target, rotateSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newZ);
        }
    }
}
