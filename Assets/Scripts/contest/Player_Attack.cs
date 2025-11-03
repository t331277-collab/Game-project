// 유니티 엔진의 기본 기능을 사용하겠다는 선언입니다.
using UnityEngine;

// 'Player_Attack'라는 이름의 스크립트(컴포넌트)를 선언합니다.
public class Player_Attack : MonoBehaviour
{
    // Rigidbody2D 부품을 담을 변수입니다. (이 코드에선 사용되지 않음)
    Rigidbody2D rgd;
    // Animator(애니메이션) 부품을 담을 변수입니다.
    Animator animator;

    // Start() 함수는 게임 시작 시 첫 프레임 업데이트 직전에 한 번 호출됩니다.
    void Start()
    {
        // 이 오브젝트의 Animator 부품을 찾아서 'animator' 변수에 넣습니다.
        animator = GetComponent<Animator>();
        // 이 오브젝트의 Rigidbody2D 부품을 찾아서 'rgd' 변수에 넣습니다.
        rgd = GetComponent<Rigidbody2D>();
    }

    // 현재 남은 쿨타임을 저장할 비공개 변수입니다.
    private float curTime;
    // [인스펙터 노출] 총 공격 쿨타임을 0.5초로 기본 설정합니다.
    public float coolTime = 0.5f;
    // [인스펙터 노출] 공격이 발동될 위치(중심점)입니다. (보통 플레이어 앞 빈 오브젝트)
    public Transform pos;
    // [인스펙터 노출] 공격 범위(히트박스)의 가로/세로 크기입니다.
    public Vector2 boxSize;

    // 매 프레임마다 호출됩니다.
    void Update()
    {
        //'Z'버튼을 공격 (주석)
        
        // 만약 현재 쿨타임('curTime')이 0 이하라면 (즉, 공격 가능하다면)
        if (curTime <= 0)
        {
            // [권장 수정] Z키가 "눌리는 그 순간"(GetKeyDown)을 감지합니다.
            if (Input.GetKeyDown(KeyCode.Z))
            {
                // 'pos' 위치에 'boxSize' 크기의 네모난 영역을 만들어서,
                // 그 안에 겹치는 "모든" 2D 콜라이더를 찾아 'collider2Ds' 배열에 담습니다.
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);

                // 'collider2Ds' 배열(맞은 모든 대상)을 하나씩 꺼내서 'collider' 변수에 넣고 반복합니다.
                foreach (Collider2D collider in collider2Ds)
                {
                    // 만약 그 콜라이더('collider')의 태그가 "Enemy" 라면,
                    if(collider.tag == "Enemy")
                    {
                        // [권장 수정] 그 콜라이더에서 'Enemy'(대문자 E) 스크립트를 찾아 'TakeDamageZ()' 함수를 실행시킵니다.
                        collider.GetComponent<Enemy>().TakeDamageZ();
                    }
                    // 콘솔에 부딪힌 모든 것의 태그를 출력합니다. (테스트용)
                    Debug.Log(collider.tag);
                
                }

                // 공격을 했으므로, 현재 쿨타임을 다시 0.5초('coolTime')로 채웁니다.
                curTime = coolTime;
            }
            // [권장 수정] X키가 "눌리는 그 순간"(GetKeyDown)을 감지합니다.
            else if (Input.GetKeyDown(KeyCode.X))
            {
                // (위와 동일하게 X키 공격 범위 판정)
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
                foreach (Collider2D collider in collider2Ds)
                {
                    if (collider.tag == "Enemy")
                    {
                        // [권장 수정] 'Enemy'(대문자 E) 스크립트의 'TakeDamageX()' 함수를 실행시킵니다.
                        collider.GetComponent<Enemy>().TakeDamageX();
                    }
                    Debug.Log(collider.tag);
                }
                // 공격을 했으므로, 현재 쿨타임을 다시 채웁니다.
                curTime = coolTime;
            }
            // [권장 수정] C키가 "눌리는 그 순간"(GetKeyDown)을 감지합니다.
            else if (Input.GetKeyDown(KeyCode.C))
            {
                // (위와 동일하게 C키 공격 범위 판정)
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
                foreach (Collider2D collider in collider2Ds)
                {
                    if (collider.tag == "Enemy")
                    {
                        // [권장 수정] 'Enemy'(대문자 E) 스크립트의 'TakeDamageC()' 함수를 실행시킵니다.
                        collider.GetComponent<Enemy>().TakeDamageC();
                    }
                    Debug.Log(collider.tag);
                }
                // 공격을 했으므로, 현재 쿨타임을 다시 채웁니다.
                curTime = coolTime;
            }
        }
        // (if curTime <= 0 의 else) 만약 현재 쿨타임이 0보다 크다면 (즉, 쿨타임이 도는 중이라면)
        else
        {
            // 'curTime'에서 'Time.deltaTime'(한 프레임당 시간)만큼을 뺍니다. (쿨타임 감소)
            curTime -= Time.deltaTime;
        }
    }

    // **(에디터 전용)** 유니티 '씬(Scene)' 화면에서만 보이지 않는 선(기즈모)을 그릴 때 호출됩니다.
    private void OnDrawGizmos()
    {
        // 기즈모(보조선) 색상을 노란색으로 설정합니다.
        Gizmos.color = Color.yellow;
        // 'pos' 위치에 'boxSize' 크기의 노란색 '선으로 된 네모'를 그립니다. 
        // (공격 범위를 눈으로 보면서 편하게 조절할 수 있게 해줍니다)
        Gizmos.DrawWireCube(pos.position, boxSize);
    }
}