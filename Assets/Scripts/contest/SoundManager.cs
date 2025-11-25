using UnityEngine;
using UnityEngine.Audio;
using System.Collections; // [필수!] 코루틴(IEnumerator)을 쓰기 위해 추가

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioMixer mainMixer;
    private AudioSource bgmSource;

    // [추가!] 실행 중인 페이드 아웃 코루틴을 저장할 변수 (중복 실행 방지용)
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(instance);

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;

            if (mainMixer != null)
            {
                bgmSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("BGM")[0];
            }
            else
            {
                Debug.LogError("SoundManager에 MainMixer가 연결되지 않았습니다!");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (mainMixer != null)
        {
            mainMixer.SetFloat("MasterVolume", Mathf.Log10(masterVol > 0 ? masterVol : 0.0001f) * 20);
            mainMixer.SetFloat("BGMVolume", Mathf.Log10(bgmVol > 0 ? bgmVol : 0.0001f) * 20);
            mainMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVol > 0 ? sfxVol : 0.0001f) * 20);
        }
    }

    // =================================================================
    // [수정 및 추가된 BGM 관련 함수들]
    // =================================================================

    public void PlayBGM(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (clip == null) return;

        // [안전장치] 혹시 페이드 아웃 중이었다면 강제로 멈춥니다.
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        bgmSource.clip = clip;
        // [중요] 페이드 아웃 후 볼륨이 0이 되어 있을 수 있으므로, 원래대로 복구하고 재생합니다.
        bgmSource.volume = volume;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    // [기능 추가] BGM 페이드 아웃 함수 (외부에서 호출)
    // duration: 몇 초 동안 줄어들지 설정 (기본 1초)
    public void FadeOutBGM(float duration = 1.0f)
    {
        // 이미 페이드 아웃 중이라면 중복 실행하지 않음
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        // 코루틴 시작
        fadeCoroutine = StartCoroutine(CoFadeOut(duration));
    }

    // [기능 추가] 실제 페이드 아웃 로직 (코루틴)
    private IEnumerator CoFadeOut(float duration)
    {
        float startVolume = bgmSource.volume; // 현재 볼륨에서 시작
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // Lerp를 이용해 현재 볼륨 -> 0으로 부드럽게 줄임
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null; // 한 프레임 대기
        }

        bgmSource.Stop();     // 소리가 다 줄어들면 정지
        bgmSource.volume = startVolume; // [중요] 다음 재생을 위해 볼륨 원상복구
        fadeCoroutine = null; // 코루틴 상태 초기화
    }

    // 즉시 멈추고 싶을 때
    public void StopBGM()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // 페이드 중이었다면 취소
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (bgmSource != null) bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (bgmSource != null) bgmSource.UnPause();
    }

    // =================================================================
    // SFX 부분 (기존과 동일)
    // =================================================================
    public void SFXPlay(string sfxName, AudioClip clip)
    {
        GameObject go = new GameObject(sfxName + "Sound");
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.clip = clip;

        if (mainMixer != null)
        {
            audioSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("SFX")[0];
        }

        audioSource.Play();
        Destroy(go, clip.length);
    }

    public void StopSFX(string sfxName)
    {
        string objName = sfxName + "Sound";
        GameObject sfxObj = GameObject.Find(objName);

        if (sfxObj != null)
        {
            AudioSource src = sfxObj.GetComponent<AudioSource>();
            if (src != null) src.Stop();
            Destroy(sfxObj);
        }
    }
}