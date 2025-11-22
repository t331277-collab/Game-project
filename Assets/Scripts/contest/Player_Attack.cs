using UnityEngine;
using System.Collections;

public class Player_Attack : MonoBehaviour
{
    public float coolTime = 0.5f;
    public Transform pos;
    public Vector2 boxSize;

    [Header("Attack Sounds")] // 공격 관련 사운드
    public AudioClip cilp1; // Chord_a1 (기본 공격 Z)
    public AudioClip cilp2; // Chord_a2 (기본 공격 X)
    public AudioClip cilp3; // Chord_a3 (기본 공격 C)

    [Header("Execution Sounds")] // 처형 관련 사운드 (새로 추가!)
    public AudioClip groggySound; // 그로기 상태일 때 재생할 사운드
    public AudioClip executionSound; // 처형 시 재생할 사운드 (예: guitar_performance)

    private float curTime;
    public bool Groggy_State = false;
    public Transform playerTransform;
    public bool isSkillActive = false;

    void Update()
    {
        if (playerTransform.localScale.x > 0) pos.localPosition = new Vector2(Mathf.Abs(pos.localPosition.x), pos.localPosition.y);
        else pos.localPosition = new Vector2(-Mathf.Abs(pos.localPosition.x), pos.localPosition.y);

        if (curTime <= 0)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("S 키 감지! PerformExecution() 호출! ");
                PerformExecution();
                curTime = coolTime;
                GameManager.Instance.Hit_ZXC();
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                SoundManager.instance.SFXPlay("Chord_a1", cilp1);
                Debug.Log("Z 키 감지! PerformAttack(Z) 호출!");
                PerformAttack(KeyCode.Z);
                curTime = coolTime;
                GameManager.Instance.Hit_ZXC();
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                SoundManager.instance.SFXPlay("Chord_a1", cilp2);
                Debug.Log("X 키 감지! PerformAttack(X) 호출!");
                PerformAttack(KeyCode.X);
                curTime = coolTime;
                GameManager.Instance.Hit_ZXC();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                SoundManager.instance.SFXPlay("Chord_a1", cilp3);
                Debug.Log("C 키 감지! PerformAttack(C) 호출!");
                PerformAttack(KeyCode.C);
                curTime = coolTime;
                GameManager.Instance.Hit_ZXC();
            }
            else if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Debug.Log("leftshift");
                GameManager.Instance.ActivateTimeStopSkill();
            }
        }
        else
        {
            curTime -= Time.deltaTime;
        }
    }

    void PerformAttack(KeyCode key)
    {
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
        foreach (Collider2D collider in collider2Ds)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemyScript = collider.GetComponent<Enemy>();
                if (enemyScript != null && !enemyScript.IsGroggy())
                {
                    if (key == KeyCode.Z) enemyScript.TakeDamageZ();
                    else if (key == KeyCode.X) enemyScript.TakeDamageX();
                    else if (key == KeyCode.C) enemyScript.TakeDamageC();
                }
            }
        }
    }

    void PerformExecution()
    {
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
        foreach (Collider2D collider in collider2Ds)
        {
            if (collider.CompareTag("Enemy"))
            {
                Enemy enemyScript = collider.GetComponent<Enemy>();
                if (enemyScript != null && enemyScript.IsGroggy())
                {
                    // [수정!] 그로기 로그와 사운드를 먼저 재생합니다.
                    Debug.Log("적을 발견했고, 그로기 상태입니다. Groggy Sound 재생!");
                    if (groggySound != null)
                    {
                        SoundManager.instance.SFXPlay("Groggy", groggySound);
                    }

                    // [수정!] 처형 로직과 사운드를 실행합니다.
                    Debug.Log("Execute() 호출! (사망)");
                    if (executionSound != null)
                    {
                        // guitar_performance 같은 사운드를 재생합니다.
                        SoundManager.instance.SFXPlay("Execution", executionSound);
                    }
                    enemyScript.Execute();
                    break;
                }
                else if (enemyScript != null)
                {
                    Debug.Log("적을 발견했으나, 그로기 상태가 아닙니다. (처형 실패)");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos.position, boxSize);
    }
}