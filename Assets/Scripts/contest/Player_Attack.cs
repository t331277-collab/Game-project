using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;

public class Player_Attack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float coolTime = 0.5f; // 공격 쿨타임
    public Transform pos; // 공격 히트박스의 중심점 (Inspector에서 연결 필요)
    public Vector2 boxSize; // 공격 히트박스의 크기

    [Header("Attack Sounds")] // 기본 공격 Z, X, C 사운드
    public AudioClip cilp1; // Chord_a1 (Z키)
    public AudioClip cilp2; // Chord_a2 (X키)
    public AudioClip cilp3; // Chord_a3 (C키)

    [Header("Execution Sounds")] // 처형 관련 사운드
    public AudioClip groggySound; // 그로기 상태일 때 재생할 사운드 (Inspector에서 연결)
    public AudioClip executionSound; // 처형 시 재생할 사운드 (예: guitar_performance, Inspector에서 연결)
    private Animator animator;
    private float curTime; // 쿨타임 계산용 변수
    
    // 스프라이트 방향 확인을 위한 컴포넌트
    private SpriteRenderer spriteRenderer;

    [Header("Feel VFX (자식 오브젝트 연결)")]
    // [수정!] 3개의 별도 MMF_Player 변수를 만듭니다.
    public MMF_Player vfxZ;
    public MMF_Player vfxX;
    public MMF_Player vfxC;

    void Start()
    {
        // 이 스크립트가 붙은 오브젝트의 SpriteRenderer 컴포넌트를 가져옵니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        animator = GetComponent<Animator>();

        // (혹시 pos가 연결 안 되었을 경우를 대비한 안전장치)
        if (pos == null)
        {
            Debug.LogWarning("Player_Attack: 공격 위치(pos)가 연결되지 않았습니다!");
        }
    }

    void Update()
    {
        // --- [핵심] 스프라이트 방향에 따라 공격 범위 위치 반전 ---
        if (spriteRenderer != null && pos != null)
        {
            // flipX가 true면 왼쪽을 보는 상태 -> pos의 x 좌표를 음수로 설정
            if (spriteRenderer.flipX)
            {
                pos.localPosition = new Vector3(-Mathf.Abs(pos.localPosition.x), pos.localPosition.y, pos.localPosition.z);
            }
            // flipX가 false면 오른쪽을 보는 상태 -> pos의 x 좌표를 양수로 설정
            else
            {
                pos.localPosition = new Vector3(Mathf.Abs(pos.localPosition.x), pos.localPosition.y, pos.localPosition.z);
            }
        }
        // ----------------------------------------------------------


        // 쿨타임이 끝났는지 확인
        if (curTime <= 0)
        {
            // [중요] 시간이 멈춘 스킬 사용 상태가 아닐 때만 공격 입력 가능
            if (GameManager.Instance != null && !GameManager.Instance.IsTimeStopped)
            {
                // S키: 처형 시도
                if (Input.GetKeyDown(KeyCode.S))
                {
                    Debug.Log("S 키 감지! PerformExecution() 호출!");
                    PerformExecution();
                    curTime = coolTime; // 쿨타임 초기화
                    GameManager.Instance.Hit_ZXC(); // 리듬 판정 시도
                }
                // Z키 공격
                else if (Input.GetKeyDown(KeyCode.Z))
                {
                    SoundManager.instance.SFXPlay("Chord_a1", cilp1);
                    Debug.Log("Z 키 감지! PerformAttack(Z) 호출!");
                    PerformAttack(KeyCode.Z);
                    curTime = coolTime;
                    GameManager.Instance.Hit_ZXC();

                    if (vfxZ != null)
                {
                    vfxZ.PlayFeedbacks();
                }

                    if (animator != null)
                    {
                        animator.SetTrigger("Attack"); // "Attack" 하나만 사용!
                    }
                }
                // X키 공격
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    SoundManager.instance.SFXPlay("Chord_a2", cilp2);
                    Debug.Log("X 키 감지! PerformAttack(X) 호출!");
                    PerformAttack(KeyCode.X);
                    curTime = coolTime;
                    GameManager.Instance.Hit_ZXC();

                    if (vfxX != null)
                {
                    vfxX.PlayFeedbacks();
                }

                    if (animator != null)
                    {
                        animator.SetTrigger("Attack"); // "Attack" 하나만 사용!
                    }
                }
                // C키 공격
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    SoundManager.instance.SFXPlay("Chord_a3", cilp3);
                    Debug.Log("C 키 감지! PerformAttack(C) 호출!");
                    PerformAttack(KeyCode.C);
                    curTime = coolTime;
                    GameManager.Instance.Hit_ZXC();

                    if (vfxC != null)
                {
                    vfxC.PlayFeedbacks();
                }
                    
                    if (animator != null)
                    {
                        animator.SetTrigger("Attack"); // "Attack" 하나만 사용!
                    }
                }
                // LeftShift키: 시간 정지 스킬 발동
                else if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    Debug.Log("LeftShift 키 감지! 시간 정지 스킬 발동 요청");
                    GameManager.Instance.ActivateTimeStopSkill();
                }
            }
        }
        else
        {
            // 쿨타임 감소
            curTime -= Time.deltaTime;
        }
    }

    // 기본 공격 함수 (Z, X, C)
    void PerformAttack(KeyCode key)
    {
        // 설정된 위치(pos)와 크기(boxSize)로 사각형 범위를 만들어 충돌하는 모든 콜라이더 감지
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
        foreach (Collider2D collider in collider2Ds)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemyScript = collider.GetComponent<Enemy>();
                // 적이 존재하고, 그로기 상태가 아닐 때만 콤보 공격 적용
                if (enemyScript != null && !enemyScript.IsGroggy())
                {
                    if (key == KeyCode.Z) enemyScript.TakeDamageZ();
                    else if (key == KeyCode.X) enemyScript.TakeDamageX();
                    else if (key == KeyCode.C) enemyScript.TakeDamageC();
                }
            }
        }
    }

    // 처형 함수 (S)
    void PerformExecution()
    {
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
        foreach (Collider2D collider in collider2Ds)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemyScript = collider.GetComponent<Enemy>();
                // 적이 존재하고, '그로기 상태'일 때만 처형 실행
                if (enemyScript != null && enemyScript.IsGroggy())
                {
                    Debug.Log("그로기 상태 적 발견. 사운드 재생 및 처형 실행!");

                    // [수정] 그로기 사운드 재생 (Inspector에 연결된 클립이 있을 경우)
                    if (groggySound != null)
                    {
                        SoundManager.instance.SFXPlay("Groggy", groggySound);
                    }

                    // [수정] 처형 사운드 재생 (Inspector에 연결된 클립이 있을 경우)
                    if (executionSound != null)
                    {
                        SoundManager.instance.SFXPlay("Execution", executionSound);
                    }

                    if (animator != null)
                    {
                        // 아까 애니메이터에서 만든 트리거 이름과 똑같이!
                        animator.SetTrigger("Execution"); 
                    }
                    enemyScript.Execute(); // 적 처형 (파괴)
                    break; // 한 번에 한 명만 처형하고 루프 종료
                }
                else if (enemyScript != null)
                {
                    Debug.Log("적을 발견했으나, 그로기 상태가 아닙니다. (처형 실패)");
                }
            }
        }
    }

    // 에디터에서 공격 범위를 시각적으로 보여주는 함수
    private void OnDrawGizmos()
    {
        // pos가 연결되어 있을 때만 그림
        if (pos != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(pos.position, boxSize);
        }
    }
}