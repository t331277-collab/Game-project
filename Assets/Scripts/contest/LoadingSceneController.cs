using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 시 필요

public class LoadingSceneController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    // 불러올 씬의 이름을 저장할 정적 변수
    public static string nextSceneName = "MainMenu";

    private void Start()
    {
        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        // 1. 비동기로 씬 로드 시작
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);

        // 2. 로딩이 끝나도 즉시 넘어가지 않도록 설정
        op.allowSceneActivation = false;

        float timer = 0f;

        // 3. 로딩이 완료될 때까지 반복
        while (!op.isDone)
        {
            yield return null; // 한 프레임 대기

            timer += Time.deltaTime;

            // 유니티의 scene 로딩은 0.9에서 멈춤 (나머지 0.1은 activation 단계)
            if (op.progress < 0.9f)
            {
                // 실제 로딩 진행률 표시
                progressBar.value = Mathf.Lerp(progressBar.value, op.progress, timer);
            }
            else
            {
                // 로딩은 끝났지만, 시각적인 완성을 위해 바를 끝까지 채움
                progressBar.value = Mathf.Lerp(progressBar.value, 1f, timer);

                // 바가 꽉 찼다면 씬 전환 (1초 정도의 최소 로딩 시간을 줌)
                if (progressBar.value >= 0.99f)
                {
                    op.allowSceneActivation = true;
                }
            }

            // 텍스트 업데이트 (선택 사항)
            if (progressText != null)
                progressText.text = $"{(progressBar.value * 100):F0}%";
        }
    }
}
