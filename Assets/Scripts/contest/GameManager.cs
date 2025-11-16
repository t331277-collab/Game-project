using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    [Header("Sound")]
    public AudioClip countThree;    // 3초 카운트 사운드
    public AudioClip main_bgm;    // 이 노래에 맞춰 입력해야 함

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
        
    }

    private System.Collections.IEnumerator StartGameRoutine()
    {
        // 1) 카운트 사운드 재생
        if (countThree != null)
        {
            SoundManager.instance.SFXPlay("Count3", countThree);
            // timeScale = 0이어도 상관 없게 Realtime 사용
            yield return new WaitForSecondsRealtime(countThree.length);
        }

        // 2) 카운트 끝난 뒤 게임 시작
        Time.timeScale = 1f;
        Debug.Log("Game Start!!");

        SoundManager.instance.SFXPlay("Play", main_bgm);
    }

}
