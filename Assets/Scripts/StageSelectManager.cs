using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class StageSelectManager : MonoBehaviour
{
    float EPS = 1e-5f;

    private BannerView defaultBannerView;

    static int currentDifficulty = 0;
    const int difficultyNumber = 2;
    static int currentStageId = 0;
    const int stageNumber = 15;

    const int stageSelectWidth = 350;
    const int stageSelectHeight = 300;
    const int stageSelectWidthCenter = 0;
    const int stageSelectHeightCenter = 30;
    const int stageSelectWidthNumber = 5;
    const int stageSelectHeightNumber = 3;

    const int selectLightWidth = 200;
    const int selectLightWidthCenter = 0;
    const int selectLightHeightCenter = -270;
    const int selectLightWidthNumber = difficultyNumber;


    [SerializeField] GameObject easyUI;
    [SerializeField] GameObject normalUI;
    [SerializeField] GameObject hardUI;
    [SerializeField] GameObject stagePrefab;
    [SerializeField] GameObject selectLightPrefab;

    [SerializeField] GameObject backgroundLayer;
    [SerializeField] GameObject unitLayer;
    [SerializeField] GameObject UILayer;
    [SerializeField] GameObject buttonLayer;
    [SerializeField] GameObject effectLayer;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource decisionSoundEffect;

    [SerializeField] Sprite muteBGMSprite;
    [SerializeField] Sprite notMuteBGMSprite;
    [SerializeField] Sprite muteSESprite;
    [SerializeField] Sprite notMuteSESprite;
    [SerializeField] Sprite selectLightSprite;
    [SerializeField] Sprite notSelectLightSprite;

    [SerializeField] GameObject bgmButton;
    [SerializeField] GameObject seButton;

    [SerializeField] GameObject fadeImage;

    GameObject[] difficultyUI;
    GameObject[] selectLight;

    bool fadeOutFlagToStart = false;
    static bool fadeOutFlagToMain = false;
    bool fadeInFlag = true;
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
        fadeOutFlagToMain = false;
        fadeOutFlagToStart = false;


        // BGMの設定
        if(new AudioManager().GetBGMFlag()) {
            audioSource.GetComponent<AudioSource>().mute = true;
            bgmButton.GetComponent<Image>().sprite = muteBGMSprite;
        }else {
            audioSource.GetComponent<AudioSource>().mute = false;
            bgmButton.GetComponent<Image>().sprite = notMuteBGMSprite;
        }

        // SEの設定
        if(new AudioManager().GetSEFlag()) {
            seButton.GetComponent<Image>().sprite = muteSESprite;
        }else {
            seButton.GetComponent<Image>().sprite = notMuteSESprite;
        }

        // 難易度の個数を取得
        difficultyUI = new GameObject[]{easyUI, normalUI, hardUI};
        selectLight = new GameObject[difficultyNumber];

        // ループでステージ生成
        for(int tempDifficulty = 0; tempDifficulty < difficultyNumber; tempDifficulty++) {
            for(int y = 0; y < stageSelectHeightNumber; y++) {
                for(int x = 0; x < stageSelectWidthNumber; x++) {

                    // ステージID、ステージ名、スコアを定義
                    int tempStageId = y*stageSelectWidthNumber+x;
                    string stageName = "StageScore" + tempDifficulty.ToString() + "_" + tempStageId.ToString();
                    int stageScore = PlayerPrefs.GetInt(stageName, 0);

                    // positionを定めてステージを生成、難易度別の親に付ける
                    float stageClonePositionX = stageSelectWidthCenter+stageSelectWidth/(stageSelectWidthNumber-1)*x-stageSelectWidth/2.0f;
                    float stageClonePositionY = stageSelectHeightCenter-stageSelectHeight/(stageSelectHeightNumber-1)*y+stageSelectHeight/2.0f;
                    GameObject stageClone = Instantiate(stagePrefab, new Vector3(stageClonePositionX, stageClonePositionY, 0.0f), Quaternion.identity);
                    stageClone.transform.SetParent(difficultyUI[tempDifficulty].transform, false);

                    // ステージ番号を表示
                    GameObject stageCloneChildStageButton = stageClone.transform.Find("StageButton").gameObject;
                    stageCloneChildStageButton.transform.Find("StageIdText").GetComponent<Text>().text = (tempStageId+tempDifficulty*stageSelectHeightNumber*stageSelectWidthNumber).ToString();

                    // それぞれのステージボタンにステージIDと難易度を記録
                    ButtonManager stageCloneChildStageButtonScript = stageCloneChildStageButton.GetComponent<ButtonManager>();
                    stageCloneChildStageButtonScript.argStageId = tempStageId;
                    stageCloneChildStageButtonScript.argDifficulty = tempDifficulty;

                    // もし1個前のステージがクリアしてなかったら鍵をかける
                    GameObject stageCloneChildStageLockImage = stageClone.transform.Find("StageLockImage").gameObject;
                    string stageNameUnlock = "UnlockStage" + tempDifficulty.ToString() + "_" + tempStageId.ToString();
                    if(PlayerPrefs.GetInt(stageNameUnlock, 0) == 1) {
                        stageCloneChildStageLockImage.SetActive(false);
                        stageCloneChildStageButton.SetActive(true);
                    }else {
                        stageCloneChildStageLockImage.SetActive(true);
                        stageCloneChildStageButton.SetActive(false);
                    }

                    // 初心者用ステージの調整
                    if(stageName == "StageScore0_0") stageCloneChildStageButton.transform.Find("StageBeginnerImage").gameObject.SetActive(true);
                    else stageCloneChildStageButton.transform.Find("StageBeginnerImage").gameObject.SetActive(false);

                    // もしscoreが1以上だったらクリアマークを付ける
                    GameObject stageCloneChildStageButtonClearImage = stageCloneChildStageButton.transform.Find("StageClearImage").gameObject;
                    if(stageScore > 0) stageCloneChildStageButtonClearImage.SetActive(true);
                    else stageCloneChildStageButtonClearImage.SetActive(false);

                }
            }
            // 最後に大元のレイヤーに付ける
            difficultyUI[tempDifficulty].transform.SetParent(buttonLayer.transform, false);
        }


        for(int x = 0; x < selectLightWidthNumber; x++) {

            float selectLightClonePositionX = selectLightWidthCenter+selectLightWidth/(selectLightWidthNumber-1)*x-selectLightWidth/2.0f;
            float selectLightClonePositionY = selectLightHeightCenter;
            GameObject selectLightClone = Instantiate(selectLightPrefab, new Vector3(selectLightClonePositionX, selectLightClonePositionY, 0.0f), Quaternion.identity) as GameObject;
            selectLightClone.transform.SetParent(UILayer.transform, false);

            selectLight[x] = selectLightClone;
            if(x == currentDifficulty) selectLight[x].GetComponent<Image>().sprite = selectLightSprite;
            else selectLight[x].GetComponent<Image>().sprite = notSelectLightSprite;
        }

        for(int num = 0; num < difficultyNumber; num++) {
            if(num == currentDifficulty) difficultyUI[num].SetActive(true);
            else difficultyUI[num].SetActive(false);
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
        if(fadeOutFlagToMain || fadeOutFlagToStart) {
            defaultBannerView.Destroy();
            fadeImage.SetActive(true);
            fadeTimeCount += Time.deltaTime * 2;
            fadeImage.GetComponent<Image>().color = new Color((float)51.0f/255.0f, (float)51.0f/255.0f, (float)51.0f/255.0f, Mathf.Min(1.0f, fadeTimeCount));
            if (fadeTimeCount > 1.0f+EPS) {
                fadeTimeCount = 1.0f;
                if(fadeOutFlagToStart) SceneManager.LoadScene("Start");
                if(fadeOutFlagToMain)  SceneManager.LoadScene("Main");
            }
            return;
        }

        for(int num = 0; num < difficultyNumber; num++) {
            if(num == currentDifficulty) difficultyUI[num].SetActive(true);
            else difficultyUI[num].SetActive(false);
        }
    }

    public void soundSE(bool argSEFlag) {
        if(!argSEFlag) {
            decisionSoundEffect.GetComponent<AudioSource>().PlayOneShot(decisionSoundEffect.GetComponent<AudioSource>().clip);
        }
    }

    public void OnClickStageEasierButton() {
        soundSE(new AudioManager().GetSEFlag());
        selectLight[currentDifficulty].GetComponent<Image>().sprite = notSelectLightSprite;
        currentDifficulty = (currentDifficulty+difficultyNumber-1)%difficultyNumber;
        selectLight[currentDifficulty].GetComponent<Image>().sprite = selectLightSprite;
    }

    public void OnClickStageHarderButton() {
        soundSE(new AudioManager().GetSEFlag());
        selectLight[currentDifficulty].GetComponent<Image>().sprite = notSelectLightSprite;
        currentDifficulty = (currentDifficulty+1)%difficultyNumber;
        selectLight[currentDifficulty].GetComponent<Image>().sprite = selectLightSprite;
    }

    public void OnClickHomeButton() {
        soundSE(new AudioManager().GetSEFlag());
        fadeImage.SetActive(true);
        fadeOutFlagToStart = true;
        defaultBannerView.Destroy();
    }

    public void OnClickBGMButton() {
        soundSE(new AudioManager().GetSEFlag());
        new AudioManager().SetBGMFlag(!(new AudioManager().GetBGMFlag()));
        if(new AudioManager().GetBGMFlag()) {
            bgmButton.GetComponent<Image>().sprite = muteBGMSprite;
        }else {
            bgmButton.GetComponent<Image>().sprite = notMuteBGMSprite;
        }
        audioSource.GetComponent<AudioSource>().mute = new AudioManager().GetBGMFlag();
    }

    public void OnClickSEButton() {
        soundSE(new AudioManager().GetSEFlag());
        new AudioManager().SetSEFlag(!(new AudioManager().GetSEFlag()));
        if(new AudioManager().GetSEFlag()) {
            seButton.GetComponent<Image>().sprite = muteSESprite;
        }else {
            seButton.GetComponent<Image>().sprite = notMuteSESprite;
        }
    }

    public void SetFadeOutFlagToMain() {
        fadeOutFlagToMain = true;
    }

}
