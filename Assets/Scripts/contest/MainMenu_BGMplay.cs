using UnityEngine;

public class MainMenu_BGMplay : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip Main_BGM; //chord_a1
                               // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        SoundManager.instance.PlayBGM(Main_BGM);
    }
}
