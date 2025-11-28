using UnityEngine;
using UnityEngine.Rendering; // Volume 사용을 위해 필수
using System.Collections;

public class VolumeController : MonoBehaviour
{
    [Header("연결 필요")]
    public Volume targetVolume; // 제어할 볼륨 (인스펙터에서 연결)
    public float transitionDuration = 0.5f; // 전환 시간 (0.5초)

    private Coroutine currentCoroutine;

    // 흑백 모드 켜기 (외부에서 호출)
    public void TurnOnBlackAndWhite()
    {
        // [범인 추적용 로그] 누가 언제 켜는지 확인
        Debug.Log($"[VolumeController] 흑백 ON 호출됨! (현재 시간: {Time.unscaledTime})");

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(FadeVolumeWeight(1f));
    }

    // 흑백 모드 끄기 (외부에서 호출)
    public void TurnOffBlackAndWhite()
    {
        // [범인 추적용 로그] 누가 언제 끄는지 확인
        Debug.Log($"<color=red>[VolumeController] 흑백 OFF 호출됨! (현재 시간: {Time.unscaledTime})</color>");

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(FadeVolumeWeight(0f));
    }

    // [핵심 수정] 가장 정석적인 코루틴 방식 (시간 정지 지원)
    private IEnumerator FadeVolumeWeight(float targetWeight)
    {
        if (targetVolume == null) yield break;

        float startWeight = targetVolume.weight;
        float elapsed = 0f;

        // 코루틴이 시작됨을 알림
        // Debug.Log($"코루틴 시작: {startWeight} -> {targetWeight}");

        while (elapsed < transitionDuration)
        {
            // [중요!] Time.timeScale이 0이어도 흘러가는 실제 시간을 더합니다.
            elapsed += Time.unscaledDeltaTime;
            
            // 현재 진행률 (0 ~ 1 사이 값)
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            
            // 부드럽게 값 변경 (Lerp)
            targetVolume.weight = Mathf.Lerp(startWeight, targetWeight, t);

            // 다음 프레임까지 대기 (시간이 멈춰도 unscaledDeltaTime 덕분에 루프는 계속 돕니다)
            yield return null; 
        }

        // 루프가 끝나면 목표값으로 확실하게 고정
        targetVolume.weight = targetWeight;
        currentCoroutine = null;
        // Debug.Log($"코루틴 종료. 최종 Weight: {targetVolume.weight}");
    }
}