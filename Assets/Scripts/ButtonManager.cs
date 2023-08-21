using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    private void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClickStageSelectButton);
    }

    private void OnClickStageSelectButton()
    {
        AudioManager.Instance.PlaySE("Decision");
        new MainManager().CurrentStageId = int.Parse(transform.Find("StageIdText").GetComponent<Text>().text);
        AdBannerManager.Instance.DestroyDefaultBanner();
        FadeManager.Instance.LoadScene(0.5f, "Main");
    }
}
