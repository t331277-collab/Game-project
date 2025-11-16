using UnityEngine;

public class Ingame_UI : MonoBehaviour
{

    [Header("Sound")]
    public AudioClip tutorial_bgm;

    private void Awake()
    {
        Time.timeScale = 0f;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickGameStartButton()
    {
        Debug.Log("Game Start!");
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void OnClickListen_Music()
    {
        Debug.Log("Play Music");
        SoundManager.instance.SFXPlay("Chord_a1", tutorial_bgm);
    }
}
