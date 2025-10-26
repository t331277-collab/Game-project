using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class KinematicMover2D : MonoBehaviour
{
    [Header("Move")]
    public float speed = 5f;                 // 유닛/초
    public bool normalizeInput = true;       // 대각선 보정(좌우만이라 거의 의미 없음)

    [Header("Rotate (옵션)")]
    public bool faceMoveDirection = false;   // 이동 방향으로 회전할지
    public float rotateSpeed = 720f;         // 도/초

    [Header("Backdash")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public float dashDistance = 2.0f;        // 뒤로 밀릴 거리
    public float dashTime = 0.12f;           // 대시 지속시간(초)
    public float dashCooldown = 0.35f;       // 다음 대시까지 쿨타임
    public AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool lockInputDuringDash = true;  // 대시 중 입력 잠금

    Rigidbody2D rb;
    float fixedY;
    int facing = 1;                          // 1:오른쪽, -1:왼쪽 (마지막 비영(0) 입력 기준)

    // 대시 상태
    bool isDashing;
    float dashStartTime, dashEndTime, nextDashReadyTime;
    float dashFromX, dashToX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // 좌우만 이동: Y 고정 & 회전 고정
        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        fixedY = rb.position.y;

        // 기본 곡선이 없으면 선형으로
        if (dashCurve == null || dashCurve.length < 2)
            dashCurve = AnimationCurve.Linear(0, 0, 1, 1);
    }

    void Update()
    {
        // 입력 기준으로 바라보는 방향 갱신 (대시 중이 아닐 때만)
        if (!isDashing)
        {
            float hRaw = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(hRaw) > 0.0001f)
                facing = hRaw > 0 ? 1 : -1;
        }

        // 백대시 시작 (대시 중/쿨타임 중이면 무시)
        if (Input.GetKeyDown(dashKey) && !isDashing && Time.time >= nextDashReadyTime)
        {
            StartBackdash();
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            // 대시 진행
            float t = Mathf.InverseLerp(dashStartTime, dashEndTime, Time.time);
            float eased = dashCurve.Evaluate(t);
            float x = Mathf.Lerp(dashFromX, dashToX, eased);
            rb.MovePosition(new Vector2(x, fixedY));

            if (Time.time >= dashEndTime)
            {
                isDashing = false;
                nextDashReadyTime = Time.time + dashCooldown;
            }

            // 대시 중 회전(옵션)
            if (faceMoveDirection)
            {
                float target = (dashToX - dashFromX) >= 0 ? 0f : 180f;
                float newZ = Mathf.MoveTowardsAngle(rb.rotation, target, rotateSpeed * Time.fixedDeltaTime);
                rb.MoveRotation(newZ);
            }
            return; // 대시 중엔 일반 이동 스킵
        }

        // ── 일반 좌우 이동 ────────────────────────────────
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

        // 뒤로: 현재 바라보는 방향의 반대쪽
        int backDir = -facing; // 오른쪽 보고 있으면 왼쪽으로, 반대도 동일
        dashFromX = rb.position.x;
        dashToX = dashFromX + backDir * dashDistance;

        dashStartTime = Time.time;
        dashEndTime = dashStartTime + Mathf.Max(0.01f, dashTime);
    }
}
