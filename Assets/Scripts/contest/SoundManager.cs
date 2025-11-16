using UnityEngine;

public class SoundManager : MonoBehaviour
{
    
    public static SoundManager instance;

    
    private AudioSource bgmSource;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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

    public void ResumeBGM()
    {
        if (bgmSource != null) bgmSource.UnPause();
    }

    public void SFXPlay(string sfxName, AudioClip clip)
    {
        GameObject go = new GameObject(sfxName + "Sound");
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();

        Destroy(go, clip.length);
    }
}