using UnityEngine;
// [필수!] Audio Mixer를 제어하려면 이 코드를 추가해야 합니다.
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    
    public static SoundManager instance;

    // [필수!] 인스펙터에서 "MainMixer" 에셋을 연결할 변수
    public AudioMixer mainMixer;
    
    private AudioSource bgmSource;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // [수정!] MainScene에만 있다면 DontDestroyOnLoad는 필요 없습니다.
            // (만약 MainMenu 씬으로 돌아갈 때 BGM이 끊겨도 된다면)
            // (일단 지금 문제 해결을 위해 이 코드는 그대로 둡니다.)
            DontDestroyOnLoad(instance); 

            // BGM 전용 오디오 소스 생성
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;

            // [핵심 1] BGM 소스의 '출력(Output)'을 "BGM" 그룹으로 설정
            if (mainMixer != null)
            {
                bgmSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("BGM")[0];
            }
            else
            {
                // MainScene에만 있는 SoundManager라면 이 로그는 뜨면 안됩니다!
                Debug.LogError("SoundManager에 MainMixer가 연결되지 않았습니다!");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // [추가!] Start 함수 추가
    private void Start()
    {
        // 씬이 시작될 때 (Awake 다음), PlayerPrefs에 저장된 볼륨 값을 불러옵니다.
        // GetFloat("이름", 기본값): "이름"으로 저장된 값을 가져오고, 없으면 1f(최대볼륨)를 씀
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // [핵심 2] 불러온 값을 '즉시' 오디오 믹서에 적용합니다.
        // (슬라이더 값(0~1)을 데시벨(-80~0)로 바꿔야 함: Log10(volume) * 20)
        // (슬라이더 값이 0이 될 경우를 대비해 0.0001f를 더해 -Infinity 오류 방지)
        if (mainMixer != null)
        {
            mainMixer.SetFloat("MasterVolume", Mathf.Log10(masterVol > 0 ? masterVol : 0.0001f) * 20);
            mainMixer.SetFloat("BGMVolume", Mathf.Log10(bgmVol > 0 ? bgmVol : 0.0001f) * 20);
            mainMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVol > 0 ? sfxVol : 0.0001f) * 20);
            
            Debug.Log("저장된 볼륨 값을 믹서에 적용했습니다!");
        }
    }


    // (BGM 관련 다른 함수들은 수정할 필요 없음)
    public void OnPlayerConnected(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (clip == null) { return; }
        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlayBGM(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (clip == null) return;
        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (bgmSource != null) bgmSource.Pause();
    }

    // [수정!] 실수로 코드가 끊겼던 부분
    public void ResumeBGM()
    {
        if (bgmSource != null) bgmSource.UnPause();
    }

    // (SFX 재생 함수)
    public void SFXPlay(string sfxName, AudioClip clip)
    {
        GameObject go = new GameObject(sfxName + "Sound");
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.clip = clip;

        // [핵심 3] SFX 소리의 '출력(Output)'을 "SFX" 그룹으로 설정
        if (mainMixer != null)
        {
            audioSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("SFX")[0];
        }
        
        audioSource.Play();
        Destroy(go, clip.length);
    }
}