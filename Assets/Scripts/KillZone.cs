using UnityEngine;
using System.Collections.Generic;

// [확인 1] 이 이름이 파일 이름 "KillZone.cs"와 정확히 같아야 합니다.
public class KillZone : MonoBehaviour
{
    // [확인 2] 'Enemy'를 찾을 수 없다는 오류는 안 뜨나요?
    private Enemy currentEnemy = null;

    public List<KeyCode> validKillKeys = new List<KeyCode>();
    
    private BoxCollider2D zoneCollider;


    void Start()
    {
        zoneCollider = GetComponent<BoxCollider2D>();

 
        
    }

    public float GetKnockbackDistance()
    {
        return zoneCollider.bounds.size.x * 0.5f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && currentEnemy == null)
        {
            currentEnemy = other.GetComponent<Enemy>();
            
            if (currentEnemy != null) // [추가] Enemy 스크립트가 있는지 확인
            {
                currentEnemy.SetKillZoneReference(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.GetComponent<Enemy>() == currentEnemy)
        {
            currentEnemy = null; 
        }
    }

    void Update()
    {
        if (currentEnemy != null)
        {
            foreach (KeyCode key in validKillKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    currentEnemy.ProcessHit(key);
                    
                    break;
                }
            }
        }
        else
        {
            foreach (KeyCode key in validKillKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    Debug.Log("실패! (너무 빠름)");
                    break;
                }
            }
        }
    }
}