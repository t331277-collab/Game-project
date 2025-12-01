using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; // 오디오 믹서 사용을 위해 필수

public class InGameMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel; // 설정창 패널 (IngameSettingsPanel)

    [Header("Audio Settings")]
    public AudioMixer mainMixer;     // 오디오 믹서
    public Slider masterSlider;      // 마스터 볼륨 슬라이더
    public Slider bgmSlider;         // BGM 볼륨 슬라이더
    public Slider sfxSlider;         // SFX 볼륨 슬라이더

    void Start()
    {
        // 씬 시작 시 패널이 확실히 닫혀있도록 설정
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // [중요!] 게임 시작 시 저장된 볼륨 값을 불러와서 슬라이더에 적용합니다.
        // 이렇게 안 하면 메뉴를 열었을 때 슬라이더가 이상한 위치에 가있게 됩니다.
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    // --- 버튼 기능 ---

    // 오른쪽 상단 설정 버튼 클릭 시 호출
    public void OpenSettingsBtnClicked()
    {
        settingsPanel.SetActive(true);
        // GameManager에게 게임을 멈추라고 요청
        GameManager.Instance.PauseGameForMenu();
    }

    // 패널 안의 닫기 버튼 클릭 시 호출
    public void CloseSettingsBtnClicked()
    {
        settingsPanel.SetActive(false);
        // GameManager에게 게임을 재개하라고 요청
        GameManager.Instance.ResumeGameFromMenu();
    }


    // --- 볼륨 조절 기능 (MainMenu.cs와 동일한 로직) ---

    // 마스터 볼륨 슬라이더 조절 시 호출
    public void SetMasterVolume(float volume)
    {
        if (mainMixer != null)
        {
            // 0이 되면 로그 계산 오류가 나므로 아주 작은 값을 대신 사용
            mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume > 0 ? volume : 0.0001f) * 20);
            PlayerPrefs.SetFloat("MasterVolume", volume); // 값 저장
        }
    }

    // BGM 볼륨 슬라이더 조절 시 호출
    public void SetBGMVolume(float volume)
    {
        if (mainMixer != null)
        {
            mainMixer.SetFloat("BGMVolume", Mathf.Log10(volume > 0 ? volume : 0.0001f) * 20);
            PlayerPrefs.SetFloat("BGMVolume", volume);
        }
    }

    // SFX 볼륨 슬라이더 조절 시 호출
    public void SetSFXVolume(float volume)
    {
        if (mainMixer != null)
        {
            mainMixer.SetFloat("SFXVolume", Mathf.Log10(volume > 0 ? volume : 0.0001f) * 20);
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }
    }

    public void OnClickRetryStage1()
    {
        MainScoreManager.Instance.ReTry(1);
    }
    public void OnClickRetryStage2()
    {
        MainScoreManager.Instance.ReTry(2);
    }
    public void OnClickRetryBoss()
    {
        MainScoreManager.Instance.ReTry(3);
    }
}