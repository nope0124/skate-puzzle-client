﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// ステージセレクトボタンを扱うクラス
/// </summary>
public class ButtonManager : MonoBehaviour
{
    private void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClickStageSelectButton);
    }

    private void OnClickStageSelectButton()
    {
        AudioManager.Instance.PlaySE("Decision");
        Debug.Log(transform.Find("StageIdText").GetComponent<Text>().text);
        new MainManager().CurrentStageId = int.Parse(transform.Find("StageIdText").GetComponent<Text>().text);
        // AdmobManager.Instance.DestroyDefaultBanner();
        FadeManager.Instance.LoadScene(0.5f, "Main");
    }
}
