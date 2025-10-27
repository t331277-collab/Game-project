using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Move")]
    public float speed = 5f;
    public bool normalizeInput = true;

    [Header("Rotate (옵션)")]
    public bool faceMoveDirection = false;
    public float rotateSpeed = 720f;

    [Header("Backdash")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public float dashDistance = 2.0f;
    public float dashTime = 0.12f;
    public float dashCooldown = 0.35f;
    public AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool lockInputDuringDash = true;

    [Header("Stun")]
    public float stunDuration = 0.5f; // ← 넉백 후 행동불능 시간

    Rigidbody2D rb;
    float fixedY;
    int facing = 1;

    bool isDashing;
    bool isStunned;               // ← 추가: 스턴 중이면 이동 입력 무시
    float dashStartTime, dashEndTime, nextDashReadyTime;
    float dashFromX, dashToX;
    Coroutine knockRoutine;       // ← 중복 넉백 덮어쓰기용

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        fixedY = rb.position.y;
        if (dashCurve == null || dashCurve.length < 2)
            dashCurve = AnimationCurve.Linear(0, 0, 1, 1);
    }

    void Update()
    {
        // 방향 갱신: 대시/스턴 중엔 잠시 멈춤
        if (!isDashing && !isStunned)
        {
            float hRaw = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(hRaw) > 0.0001f)
                facing = hRaw > 0 ? 1 : -1;
        }

        // 수동 백대시
        if (Input.GetKeyDown(dashKey) && !isDashing && !isStunned && Time.time >= nextDashReadyTime)
            StartBackdash();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            float t = Mathf.InverseLerp(dashStartTime, dashEndTime, Time.time);
            float eased = dashCurve.Evaluate(t);
            float x = Mathf.Lerp(dashFromX, dashToX, eased);
            rb.MovePosition(new Vector2(x, fixedY));

            if (Time.time >= dashEndTime)
            {
                isDashing = false;
                nextDashReadyTime = Time.time + dashCooldown;
            }

            if (faceMoveDirection)
            {
                float target = (dashToX - dashFromX) >= 0 ? 0f : 180f;
                float newZ = Mathf.MoveTowardsAngle(rb.rotation, target, rotateSpeed * Time.fixedDeltaTime);
                rb.MoveRotation(newZ);
            }
            return;
        }

        // 스턴 중엔 입력/이동 차단
        if (isStunned) return;

        float h = Input.GetAxisRaw("Horizontal");
        if (normalizeInput) h = Mathf.Clamp(h, -1f, 1f);

        float nextX = rb.position.x + h * speed * Time.fixedDeltaTime;
        rb.MovePosition(new Vector2(nextX, fixedY));

        if (faceMoveDirection && Mathf.Abs(h) > 0.0001f)
        {
            float target = (h > 0f) ? 0f : 180f;
            float newZ = Mathf.MoveTowardsAngle(rb.rotation, target, rotateSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newZ);
        }
    }

    void StartBackdash()
    {
        isDashing = true;
        int backDir = -facing; // 바라보는 반대 방향
        dashFromX = rb.position.x;
        dashToX = dashFromX + backDir * dashDistance;

        dashStartTime = Time.time;
        dashEndTime = dashStartTime + Mathf.Max(0.01f, dashTime);
    }

    public void KnockBack()
    {
        Debug.Log("KnockBack 호출! 충돌!");

        if (knockRoutine != null) StopCoroutine(knockRoutine);
        knockRoutine = StartCoroutine(KnockbackThenStun());
    }

    IEnumerator KnockbackThenStun()
    {
        // 1) 백대시 시작
        if (!isDashing)
            StartBackdash();

        // 2) 백대시 끝날 때까지 대기
        while (isDashing) yield return null;

        // 3) 0.5초 스턴
        isStunned = true;
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;

        knockRoutine = null;
    }
}
