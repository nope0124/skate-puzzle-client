using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class StartManager : MonoBehaviour
{
    float EPS = 1e-5f;

    static int difficultyNumber = 2;
    static int stageNumber = 15;

    private BannerView defaultBannerView;

    [SerializeField] GameObject dataResetPanel;
    [SerializeField] GameObject dataResetFinishPanel;
    [SerializeField] GameObject fadeImage;

    [SerializeField] AudioSource decisionSoundEffect;

    bool fadeInFlag = true;
    bool fadeOutFlag = false;
    float fadeTimeCount = 1.0f;

    void requestDefaultBanner()
    {
        #if UNITY_IOS
            string adUnitId = AdmobVariable.GetIPHONE_DEFAULT_BANNER();
        #else
            string adUnitId = "unexpected_platform";
        #endif

        defaultBannerView = new BannerView(adUnitId, AdSize.IABBanner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();

        MobileAds.Initialize(initStatus => { });
        defaultBannerView.LoadAd(request);
    }
    
    void Start()
    {
        // フェードイン
        fadeImage.SetActive(true);
        fadeInFlag = true;
        fadeOutFlag = false;

        string stageNameUnlock = "UnlockStage0_0";
        PlayerPrefs.SetInt(stageNameUnlock, 1);

    }

    public void soundSE(bool argSEFlag) {
        if(!argSEFlag) {
            decisionSoundEffect.GetComponent<AudioSource>().PlayOneShot(decisionSoundEffect.GetComponent<AudioSource>().clip);
        }
    }

    void Update()
    {
        if(fadeInFlag) {
            fadeTimeCount -= Time.deltaTime * 2;
            fadeImage.GetComponent<Image>().color = new Color((float)51.0f/255.0f, (float)51.0f/255.0f, (float)51.0f/255.0f, Mathf.Max(0.0f, fadeTimeCount));
            if(fadeTimeCount < 0.0f-EPS) {
                fadeTimeCount = 0.0f;
                fadeImage.SetActive(false);
                fadeInFlag = false;
                
                // 広告の生成
                requestDefaultBanner();
            }
            return;
        }
        if(fadeOutFlag) {
            defaultBannerView.Destroy();
            fadeTimeCount += Time.deltaTime * 2;
            fadeImage.GetComponent<Image>().color = new Color((float)51.0f/255.0f, (float)51.0f/255.0f, (float)51.0f/255.0f, Mathf.Min(1.0f, fadeTimeCount));
            if (fadeTimeCount > 1.0f+EPS) {
                fadeTimeCount = 1.0f;
                SceneManager.LoadScene("StageSelect");
            }
            return;
        }
    }

    public void OnClickStartButton()
    {
        soundSE(new AudioManager().GetSEFlag());
        fadeOutFlag = true;
        fadeImage.SetActive(true);
    }

    public void OnClickDataResetButton() {
        soundSE(new AudioManager().GetSEFlag());
        if (!dataResetPanel.activeSelf) {
            dataResetPanel.SetActive(true);
        }
    }

    public void OnClickDataResetYesButton() {
        soundSE(new AudioManager().GetSEFlag());
        for(int tempDifficulty = 0; tempDifficulty < difficultyNumber; tempDifficulty++) {
            for(int tempStageId = 0; tempStageId < stageNumber; tempStageId++) {
                string stageName = "StageScore" + tempDifficulty.ToString() + "_" + tempStageId.ToString();
                string stageNameUnlock = "UnlockStage" + tempDifficulty.ToString() + "_" + tempStageId.ToString();
                PlayerPrefs.SetInt(stageName, 0);
                PlayerPrefs.SetInt(stageNameUnlock, 0);
            }
        }
        PlayerPrefs.SetInt("UnlockStage0_0", 1);
        dataResetPanel.SetActive(false);
        dataResetFinishPanel.SetActive(true);
    }
    
    public void OnClickDataResetNoButton() {
        soundSE(new AudioManager().GetSEFlag());
        dataResetPanel.SetActive(false);
    }

    public void OnClickDataResetFinishButton() {
        soundSE(new AudioManager().GetSEFlag());
        dataResetFinishPanel.SetActive(false);
    }
}
