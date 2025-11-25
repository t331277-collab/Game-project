using UnityEngine;
using UnityEngine.UI;   // ★ 버튼 제어용
using UnityEngine.SceneManagement;

public class Ingame_UI : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip tutorial_bgm;

    [Header("UI")]
    [SerializeField] private Button listenMusicButton; // 음악 버튼 (인스펙터에 연결)

    [SerializeField] private GameObject startUIPanel; // 띄울 UI 패널

    private bool isMusicPlaying = false; // 지금 음악 재생 중인지?

    private void Awake()
    {
        Time.timeScale = 0f;
    }

    public void OnClickNextStage()
    {
        SceneManager.LoadScene("Stage_CutScene"); // 메인 컷씬씬으로 넘어가는 버튼
    }

    public void OnClickNextStage2()
    {
        SceneManager.LoadScene("Stage2"); // Stage2로 넘어가는 버튼
    }

    public void OnClickNextBoss()
    {
        SceneManager.LoadScene("Boss_Stage"); // Stage2로 넘어가는 버튼
    }

    public void OnClickStopListenMusic()
    {
        SoundManager.instance.StopBGM();

        isMusicPlaying = false;

        if (listenMusicButton != null)
            listenMusicButton.interactable = true;
    }

    public void OnClickGameStartButton()
    {
        if(isMusicPlaying)
        {
            SoundManager.instance.StopBGM();
        }
        

        GameManager.Instance.StartGame();

        startUIPanel.SetActive(false);
    }

    public void OnClickListen_Music()
    {
        // 이미 재생 중이면 무시
        if (isMusicPlaying)
        {
            Debug.Log("이미 음악 재생 중!");
            return;
        }

        Debug.Log("Play Music");
        isMusicPlaying = true;

        // 버튼 비활성화 (클릭 못 하게)
        if (listenMusicButton != null)
            listenMusicButton.interactable = false;

        SoundManager.instance.PlayBGM(tutorial_bgm);

        // 노래 길이만큼 기다렸다가 다시 활성화
        StartCoroutine(EnableListenButtonAfterMusic());
    }

    private System.Collections.IEnumerator EnableListenButtonAfterMusic()
    {

        

        if (tutorial_bgm != null)
        {
            // ★ timeScale의 영향을 안 받는 실시간 대기
            yield return new WaitForSecondsRealtime(tutorial_bgm.length);
        }
        else
        {
            // 혹시 clip이 비어있으면 안전하게 1초 정도만
            yield return new WaitForSecondsRealtime(1f);
        }

        isMusicPlaying = false;

        if (listenMusicButton != null)
            listenMusicButton.interactable = true;
    }
}
