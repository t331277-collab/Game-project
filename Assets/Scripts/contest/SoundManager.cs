using UnityEngine;

public class SoundManager : MonoBehaviour
{
    //호출하고 삭제하기 쉬운 싱글톤 형식 즉 SoundManager가 호출되면 음악을 재생할때 생성되고 음악이 끝나면 파괴됨
    public static SoundManager instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SFXPlay(string sfxName, AudioClip clip)
    {
        GameObject go = new GameObject(sfxName + "Sound");
        AudioSource audioSource= go.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();

        Destroy(go, clip.length);
    }
}
