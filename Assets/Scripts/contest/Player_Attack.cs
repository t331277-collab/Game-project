// 유니티 엔진의 기본 기능을 사용하겠다는 선언입니다.
using UnityEngine;
// List<> 같은 자료구조를 사용하기 위해 선언합니다. (현재 코드에선 불필요)
using System.Collections.Generic;

// 'Player_Attack'라는 이름의 스크립트(컴포넌트)를 선언합니다.
public class Player_Attack : MonoBehaviour
{
    // Rigidbody2D 부품을 담을 변수입니다. (이 코드에선 사용되지 않음)
    Rigidbody2D rgd;
    // Animator(애니메이션) 부품을 담을 변수입니다.
    Animator animator;

    [Header("Sound")]
    public AudioClip cilp1;
    public AudioClip cilp2;
    public AudioClip cilp3;

    // Start() 함수는 게임 시작 시 첫 프레임 업데이트 직전에 한 번 호출됩니다.
    void Start()
    {
        // 이 오브젝트의 Animator 부품을 찾아서 'animator' 변수에 넣습니다.
        animator = GetComponent<Animator>();
        // 이 오브젝트의 Rigidbody2D 부품을 찾아서 'rgd' 변수에 넣습니다.
        rgd = GetComponent<Rigidbody2D>();

        baseLocal = pos.localPosition;
    }

    // 현재 남은 쿨타임을 저장할 비공개 변수입니다.
    private float curTime;
    // [인스펙터 노출] 총 공격 쿨타임을 0.5초로 기본 설정합니다.
    public float coolTime = 0.5f;
    // [인스펙터 노출] 공격이 발동될 위치(중심점)입니다. (보통 플레이어 앞 빈 오브젝트)
    public Transform pos;
    // [인스펙터 노출] 공격 범위(히트박스)의 가로/세로 크기입니다.
    public Vector2 boxSize;

    float lastDir = 1f;
    Vector2 baseLocal;

    // [수정!] Update 함수가 'S'키(처형)와 'Z/X/C'키(그로기)를 구분합니다.
    void Update()
    {
        //히트박스도 player 기준으로 대칭 이동
        float inputX = Input.GetAxisRaw("Horizontal");

        if (inputX != 0f)
        {
            lastDir = Mathf.Sign(inputX);
        }

        pos.localPosition = new Vector2(Mathf.Abs(baseLocal.x) * lastDir , baseLocal.y);

        // 쿨타임이 0 이하일 때 (공격 가능)
        if (curTime <= 0)
        {
            // [기획 변경] 'A'키(처형) 입력을 먼저 확인합니다.
            if (Input.GetKeyDown(KeyCode.S))
            {
                // 디버그 로그로 A키 입력을 확인합니다.
                Debug.Log("--- A 키 감지! PerformExecution() 호출! ---");
                // '처형' 전용 함수를 호출합니다.
                PerformExecution();
                // 쿨타임 초기화
                curTime = coolTime;
            }
            // 'A'키가 안 눌렸을 때만 Z, X, C키(그로기 공격)를 확인합니다.
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                SoundManager.instance.SFXPlay("Chord_a1", cilp1);
                // 디버그 로그로 Z키 입력을 확인합니다.
                Debug.Log("--- Z 키 감지! PerformAttack(Z) 호출! ---");
                // '일반 공격(그로기)' 함수를 Z키 정보와 함께 호출합니다.
                PerformAttack(KeyCode.Z);
                // 쿨타임 초기화
                curTime = coolTime;
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                SoundManager.instance.SFXPlay("Chord_a1", cilp2);
                // 디버그 로그로 X키 입력을 확인합니다.
                Debug.Log("--- X 키 감지! PerformAttack(X) 호출! ---");
                PerformAttack(KeyCode.X);
                curTime = coolTime;
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                SoundManager.instance.SFXPlay("Chord_a1", cilp3);
                // 디버그 로그로 C키 입력을 확인합니다.
                Debug.Log("--- C 키 감지! PerformAttack(C) 호출! ---");
                PerformAttack(KeyCode.C);
                curTime = coolTime;
            }
            //SoundSkill 구현
            else if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Debug.Log("leftshift");
                SoundSkill();
            }
        }
        // 쿨타임이 0보다 클 때 (쿨타임 도는 중)
        else
        {
            // 쿨타임을 감소시킵니다.
            curTime -= Time.deltaTime;
        }

        
        
    }

    void SoundSkill()
    {
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);

        // 맞은 모든 대상을 하나씩 검사
        foreach (Collider2D collider in collider2Ds)
        {
            // 태그가 "Enemy"인지 확인
            if (collider.CompareTag("Enemy"))
            {
                // Enemy 스크립트를 가져옴
                Enemy enemyScript = collider.GetComponent<Enemy>();

                enemyScript.SoundSkillDamaged();
            }
        }
    }

    // [수정!] Z, X, C 키를 눌렀을 때 호출되는 '일반 공격 (그로기)' 함수입니다.
    void PerformAttack(KeyCode key)
    {
        // (넉백을 유발하는 함수임을 디버그 로그로 확인)
        Debug.Log($"PerformAttack 함수 실행 (키: {key}). 그로기 콤보 및 넉백을 시도합니다.");

        // 공격 범위 판정
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);

        // 맞은 모든 대상을 하나씩 검사
        foreach (Collider2D collider in collider2Ds)
        {
            // 태그가 "Enemy"인지 확인
            if (collider.CompareTag("Enemy"))
            {
                // [버그 수정] 소문자 'enemy'가 아닌 대문자 'Enemy' 스크립트를 찾습니다.
                Enemy enemyScript = collider.GetComponent<Enemy>();

                // [기획 변경] 적이 있고, "그로기 상태가 아닐 때만" 콤보가 들어가도록 합니다.
                if (enemyScript != null && !enemyScript.IsGroggy())
                {
                    // 키에 맞는 데미지 함수(TakeDamageZ/X/C)를 호출합니다. (이 함수들이 넉백을 유발함)
                    if (key == KeyCode.Z) enemyScript.TakeDamageZ();
                    else if (key == KeyCode.X) enemyScript.TakeDamageX();
                    else if (key == KeyCode.C) enemyScript.TakeDamageC();
                }
            }
        }
    }

    // [기획 변경] A 키를 눌렀을 때만 호출되는 '처형' 전용 함수입니다.
    void PerformExecution()
    {
        // (넉백이 없어야 정상임을 디버그 로그로 확인)
        Debug.Log("PerformExecution 함수 실행. 넉백이 없어야 정상. 그로기 상태를 체크합니다.");

        // 공격 범위 판정
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);

        // 맞은 모든 대상을 하나씩 검사
        foreach (Collider2D collider in collider2Ds)
        {
            // 태그가 "Enemy"인지 확인
            if (collider.CompareTag("Enemy"))
            {
                // Enemy 스크립트를 가져옴
                Enemy enemyScript = collider.GetComponent<Enemy>();

                // [기획 변경] "적이 있고, 적이 IsGroggy() (그로기 상태)일 때만"
                if (enemyScript != null && enemyScript.IsGroggy())
                {
                    // Enemy의 'Execute()' (처형) 함수를 호출합니다. (이 함수는 넉백이 없음)
                    Debug.Log("적을 발견했고, 그로기 상태입니다. Execute() 호출! (사망)");
                    enemyScript.Execute();
                    // 한 번에 한 명만 처형
                    break;
                }
                else if (enemyScript != null)
                {
                    // (그로기 상태가 아닐 때의 디버그 로그)
                    Debug.Log("적을 발견했으나, 그로기 상태가 아닙니다. (처형 실패)");
                }
            }
        }
    }

    // 씬(Scene)에서 공격 범위를 노란색 네모로 보여주는 보조 기능입니다. (동일)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos.position, boxSize);
    }
}