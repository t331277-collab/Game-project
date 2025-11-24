using UnityEngine;
using System.Collections;

public class UIShakeSignal : MonoBehaviour
{
    [Header("Target UI")]
    public RectTransform targetUI; // 흔들 패널 (ShakeContainer)

    [Header("1. 처음 쾅! (Heavy)")]
    public float heavyPower = 20f;
    public float heavyDuration = 0.5f;

    [Header("2. 이후 주기적 흔들림 (Gentle Loop)")]
    public float interval = 1.0f;
    public float gentlePower = 5f;
    public float gentleDuration = 0.2f;

    private Vector2 originalPos;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        if (targetUI == null) targetUI = GetComponent<RectTransform>();
        originalPos = targetUI.anchoredPosition;
    }

    // ★ Timeline Signal Receiver에서 이 함수를 호출합니다 ★
    public void PlayShakeSequence()
    {
        // 혹시 이미 흔들리고 있다면 끄고 다시 시작
        StopShake();
        shakeCoroutine = StartCoroutine(ProcessShake());
    }

    // 흔들림을 강제로 멈추고 싶을 때 호출 (예: 씬 전환 시)
    public void StopShake()
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);

        // 위치 원상복구
        if (targetUI != null) targetUI.anchoredPosition = originalPos;
    }

    IEnumerator ProcessShake()
    {
        // 1단계: 쾅! (Heavy)
        float elapsed = 0.0f;
        while (elapsed < heavyDuration)
        {
            targetUI.anchoredPosition = originalPos + Random.insideUnitCircle * heavyPower;
            elapsed += Time.deltaTime;
            yield return null;
        }
        targetUI.anchoredPosition = originalPos;

        // 2단계: 주기적 반복 (Loop)
        while (true)
        {
            yield return new WaitForSeconds(interval);

            float subElapsed = 0.0f;
            while (subElapsed < gentleDuration)
            {
                targetUI.anchoredPosition = originalPos + Random.insideUnitCircle * gentlePower;
                subElapsed += Time.deltaTime;
                yield return null;
            }
            targetUI.anchoredPosition = originalPos;
        }
    }

    void OnDisable()
    {
        StopShake(); // 비활성화되면 즉시 정지 및 원위치
    }
}