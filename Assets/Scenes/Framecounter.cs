using UnityEngine;

public class Framecounter : MonoBehaviour
{
    [SerializeField] int targetFps = 60;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFps;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
