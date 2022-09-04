using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    private static Canvas canvas;
    private static Image image;

    private static FadeManager singlton;

    public static FadeManager Instance
    {
        get {
            if (singlton == null)
            {
                // Canvas作成
                GameObject canvasObject = new GameObject("CanvasFade");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                // Image作成
                image = new GameObject("ImageFade").AddComponent<Image>();
                image.transform.SetParent(canvas.transform, false);
                image.rectTransform.anchoredPosition = Vector3.zero;
                Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>();
                image.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);

                // 遷移先シーンでもオブジェクトを破棄しない。
                DontDestroyOnLoad(canvas.gameObject);

                canvasObject.AddComponent<FadeManager>();
                singlton = canvasObject.GetComponent<FadeManager>();
            }
            return singlton;
        }
    }



    /// <summary>
    /// フェードイン・アウトを含めた画面遷移を行う
    /// </summary>
    /// <param name="intervalTime">フェードイン・アウトに要する時間</param>
    /// <param name="sceneName">移動先のシーン名</param>
    public void LoadScene(float intervalTime, string sceneName)
    {
        StartCoroutine(Fade(intervalTime, sceneName));
    }

    private IEnumerator Fade(float intervalTime, string sceneName)
    {
        float time = 0f;
        canvas.enabled = true;

        // フェードアウト
        while (time <= intervalTime)
        {
            float fadeAlpha = Mathf.Lerp(0.0f, 1.05f, time/intervalTime);
            image.color = new Color(0.2f, 0.2f, 0.2f, fadeAlpha);
            time += Time.deltaTime;
            yield return null;
        }
        image.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);

        // シーン非同期ロード
        yield return SceneManager.LoadSceneAsync(sceneName);

        // フェードイン
        time = 0f;
        while (time <= intervalTime)
        {
            float fadeAlpha = Mathf.Lerp(1.05f, 0.0f, time / intervalTime);
            image.color = new Color(0.2f, 0.2f, 0.2f, fadeAlpha);
            time += Time.deltaTime;
            yield return null;
        }
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.0f);
        canvas.enabled = false;
    }
}