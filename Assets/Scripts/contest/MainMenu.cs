using UnityEngine;
using UnityEngine.SceneManagement;
// [필수!] Audio Mixer를 제어하려면 이 코드를 추가해야 합니다.
using UnityEngine.Audio;
// [필수!] Slider를 제어하려면 이 코드를 추가해야 합니다.
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // [추가!] 옵션 패널 오브젝트를 연결할 변수
    public GameObject optionsPanel;

    // [추가!] 오디오 믹서를 연결할 변수
    public AudioMixer mainMixer;

    // [추가!] 슬라이더 3개를 연결할 변수 (값 로드용)
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    // [추가!] 게임이 시작될 때 저장된 볼륨 값을 불러옵니다.
    void Start()
    {
        // PlayerPrefs: 유니티가 기기에 간단한 데이터를 저장하는 기능
        // "MasterVolume"이라는 이름으로 저장된 값을 불러오고, 없으면 1 (최대 볼륨)로 시작
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // 슬라이더의 '값'을 불러온 값으로 설정
        masterSlider.value = masterVol;
        bgmSlider.value = bgmVol;
        sfxSlider.value = sfxVol;

        // 믹서의 볼륨을 설정 (슬라이더 값(0~1)을 데시벨(-80~0)로 바꿔야 함)
        // Log10(0)은 무한대(-Infinity)가 되므로 0.0001f 같은 아주 작은 값을 보정해줍니다.
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(masterVol) * 20);
        mainMixer.SetFloat("BGMVolume", Mathf.Log10(bgmVol) * 20);
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVol) * 20);
    }

    // "게임 시작" 버튼이 호출할 함수 (기존)
    public void StartGame()
    {
        SceneManager.LoadScene("CutScene"); // (씬 이름 확인!)
    }

    // "게임 종료" 버튼이 호출할 함수 (기존)
    public void QuitGame()
    {
        Application.Quit();
    }

    // [추가!] "옵션" 버튼이 호출할 함수
    public void ShowOptionsPanel()
    {
        optionsPanel.SetActive(true); // 옵션 패널을 켭니다.
    }

    // [추가!] "뒤로가기" 버튼이 호출할 함수
    public void HideOptionsPanel()
    {
        optionsPanel.SetActive(false); // 옵션 패널을 끕니다.
    }

    // --- 슬라이더가 호출할 함수 3개 ---
    // [중요!] 슬라이더 값(0~1)을 데시벨(-80~0)로 변환하는 공식: Mathf.Log10(volume) * 20
    
    // [추가!] MasterSlider가 값이 바뀔 때마다 이 함수를 호출
    public void SetMasterVolume(float volume)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        // 변경된 값을 "MasterVolume"이라는 이름으로 기기에 저장
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    // [추가!] BGMSlider가 값이 바뀔 때마다 이 함수를 호출
    public void SetBGMVolume(float volume)
    {
        mainMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    // [추가!] SFXSlider가 값이 바뀔 때마다 이 함수를 호출
    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}