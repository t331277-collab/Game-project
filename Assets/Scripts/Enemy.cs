using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 5f;
    public List<KeyCode> killSequence = new List<KeyCode>();

    // [추가!] 넉백 배율을 위한 변수. 기본값은 1 (100%)
    public float knockbackMultiplier = 1.0f;

    private int currentSequenceIndex = 0;
    private Transform playerTransform;
    private KillZone killZoneRef; 

    void Start()
    {
        // ... (Start 함수 내용은 동일)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
    }

    void Update()
    {
        // ... (Update 함수 내용은 동일)
        if (playerTransform != null)
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        }
    }

    // [수정!] PerformKnockback 함수 수정
    private void PerformKnockback()
    {
        if (killZoneRef != null)
        {
            // 1. 킬존으로부터 '기본' 넉백 거리를 받아옵니다.
            float baseKnockbackDistance = killZoneRef.GetKnockbackDistance();
            
            // 2. 기본 거리에 이 몬스터의 '넉백 배율'을 곱합니다.
            float finalKnockbackDistance = baseKnockbackDistance * knockbackMultiplier;

            // 3. 최종 계산된 거리만큼 넉백시킵니다.
            transform.Translate(Vector2.right * finalKnockbackDistance, Space.World);
            Debug.Log("넉백! (거리: " + finalKnockbackDistance + ")");
        }
    }

    // ... (ProcessNextStep, ProcessHit 등 나머지 코드는 모두 동일합니다)
    private void ProcessNextStep(bool success)
    {
        if (success)
        {
            Debug.Log("성공! (" + killSequence[currentSequenceIndex] + ")");
        }
        else
        {
            Debug.Log("실패! 플레이어 공격! (" + killSequence[currentSequenceIndex] + " 실패)");
        }

        currentSequenceIndex++;

        if (currentSequenceIndex >= killSequence.Count)
        {
            if (success) Debug.Log("몬스터 사살!");
            else Debug.Log("모든 공격 기회 소진. 몬스터 소멸.");
            
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("다음 키 (" + killSequence[currentSequenceIndex] + ") 준비.");
            PerformKnockback();
        }
    }

    public void ProcessHit(KeyCode pressedKey)
    {
        if (killSequence.Count == 0 || killZoneRef == null) return;

        if (pressedKey == killSequence[currentSequenceIndex])
        {
            ProcessNextStep(true);
        }
        else if (killZoneRef.validKillKeys.Contains(pressedKey))
        {
            ProcessNextStep(false);
        }
    }

    public void SetKillZoneReference(KillZone zone)
    {
        killZoneRef = zone;
    }

    public void ResetSequence()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (killZoneRef != null)
            {
                ProcessNextStep(false);
            }
            else
            {
                Debug.Log("플레이어 즉시 공격! (킬존 참조 없음)");
                Destroy(gameObject);
            }
        }
    }
}