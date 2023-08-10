using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
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

    const int selectLightWidth = 150;
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

    GameObject[] difficultyUI;
    GameObject[] selectLight;

    [SerializeField] GameObject eventSystem;
    static bool fadeOutFlagToMain = false;


    [SerializeField] GameObject Test;
    

    void RequestDefaultBanner()
    {
        #if UNITY_IOS
            string adUnitId = Const.CO.IPHONE_DEFAULT_BANNER;
        #else
            string adUnitId = "unexpected_platform";
        #endif

        defaultBannerView = new BannerView(adUnitId, AdSize.IABBanner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();

        defaultBannerView.LoadAd(request);
    }

    [System.Serializable]
    public class UserProgressData
    {
        public int[] progresses;
    }

    // BGMの設定
    void SetBGM() {
        if(new AudioManager().GetBGMFlag()) {
            audioSource.GetComponent<AudioSource>().mute = true;
            bgmButton.GetComponent<Image>().sprite = muteBGMSprite;
        }else {
            audioSource.GetComponent<AudioSource>().mute = false;
            bgmButton.GetComponent<Image>().sprite = notMuteBGMSprite;
        }
    }

    // SEの設定
    void SetSE() {
        if(new AudioManager().GetSEFlag()) {
            seButton.GetComponent<Image>().sprite = muteSESprite;
        }else {
            seButton.GetComponent<Image>().sprite = notMuteSESprite;
        }
    }


    // ステージボタンの生成
    void GenerateStageSelectButton(int[] progresses) {
        for(int tempDifficulty = 0; tempDifficulty < difficultyNumber; tempDifficulty++) {
            for(int y = 0; y < stageSelectHeightNumber; y++) {
                for(int x = 0; x < stageSelectWidthNumber; x++) {

                    // ステージID、ステージ名、スコアを定義
                    int tempStageId = y*stageSelectWidthNumber+x;
                    string stageName = "StageScore" + tempDifficulty.ToString() + "_" + tempStageId.ToString();
                    // int stageScore = PlayerPrefs.GetInt(stageName, 0);
                    int stageScore = progresses[tempStageId + tempDifficulty * (stageSelectHeightNumber * stageSelectWidthNumber)];

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
                    // if(PlayerPrefs.GetInt(stageName, -1) >= 0) {
                    if(stageScore >= 0) {
                        stageCloneChildStageLockImage.SetActive(false);
                        stageCloneChildStageButton.SetActive(true);
                        // scoreに応じてクリアマークを付ける
                        string scoreName = "Score" + stageScore.ToString();
                        stageCloneChildStageButton.transform.Find(scoreName).gameObject.SetActive(true);
                    }else {
                        stageCloneChildStageLockImage.SetActive(true);
                        stageCloneChildStageButton.SetActive(false);
                    }
                    // 初心者用ステージの調整
                    if(stageName == "StageScore0_0") stageCloneChildStageButton.transform.Find("StageBeginnerImage").gameObject.SetActive(true);
                    else stageCloneChildStageButton.transform.Find("StageBeginnerImage").gameObject.SetActive(false);
                    
                }
            }
            // 最後に大元のレイヤーに付ける
            difficultyUI[tempDifficulty].transform.SetParent(buttonLayer.transform, false);
        }
    }

    
    // セレクトライトの生成
    void GenerateSelectLight() {
        for(int x = 0; x < selectLightWidthNumber; x++) {
            float selectLightClonePositionX = selectLightWidthCenter+selectLightWidth/(selectLightWidthNumber-1)*x-selectLightWidth/2.0f;
            float selectLightClonePositionY = selectLightHeightCenter;
            GameObject selectLightClone = Instantiate(selectLightPrefab, new Vector3(selectLightClonePositionX, selectLightClonePositionY, 0.0f), Quaternion.identity) as GameObject;
            selectLightClone.transform.SetParent(UILayer.transform, false);

            selectLight[x] = selectLightClone;
            if(x == currentDifficulty) selectLight[x].GetComponent<Image>().sprite = selectLightSprite;
            else selectLight[x].GetComponent<Image>().sprite = notSelectLightSprite;
        }
    }

    
    // 選択されている難易度のステージセレクト画面を表示
    void DisplayStageSelectByDifficulty() {
        for(int num = 0; num < difficultyNumber; num++) {
            // 選択された難易度と同じ時Active
            if(num == currentDifficulty) {
                difficultyUI[num].SetActive(true);
            }else {
                difficultyUI[num].SetActive(false);
            }
        }
    }

    void Start()
    {
        MobileAds.Initialize(initStatus => { });
        RequestDefaultBanner();

        // BGMの設定
        SetBGM();

        // SEの設定
        SetSE();

        // 難易度の個数を取得
        difficultyUI = new GameObject[]{easyUI, normalUI, hardUI};
        selectLight = new GameObject[difficultyNumber];

        // セレクトライトの設置
        GenerateSelectLight();

        // 選択されている難易度のステージセレクト画面を表示
        DisplayStageSelectByDifficulty();

        // // ループでステージ生成
        // GenerateStageSelectButton();

        StartCoroutine(GetUserData());
    }

    private string baseURL = "http://localhost:3000";
    IEnumerator GetUserData()
    {
        string url = baseURL + "/api/v1/users/1";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if(request.responseCode == 200)
            {
                // JSONデータをC#オブジェクトにデシリアライズ
                UserProgressData data = JsonUtility.FromJson<UserProgressData>(request.downloadHandler.text);

                // データの表示
                foreach (int progress in data.progresses)
                {
                    Debug.Log(progress);
                }
                int[] progresses = data.progresses;
                // ループでステージ生成
                GenerateStageSelectButton(progresses);

                Test.SetActive(false);
            }
        }
    }


    void Update()
    {
        if(fadeOutFlagToMain == true) {
            fadeOutFlagToMain = false;
            eventSystem.SetActive(false);
            defaultBannerView.Destroy();
            FadeManager.Instance.LoadScene(0.5f, "Main");
        }
    }
    
    /// <summary>
    /// 効果音を鳴らす
    /// </summary>
    public void soundSE(bool argSEFlag) {
        if(!argSEFlag) {
            decisionSoundEffect.GetComponent<AudioSource>().PlayOneShot(decisionSoundEffect.GetComponent<AudioSource>().clip);
        }
    }

    /// <summary>
    /// 難易度ボタン
    /// </summary>
    /// <param name="diff">-1か+1か</param>
    public void OnClickStageLevelButton(int diff) {
        soundSE(new AudioManager().GetSEFlag());
        currentDifficulty = (currentDifficulty+difficultyNumber+diff)%difficultyNumber;
        for(int num = 0; num < difficultyNumber; num++) {
            if(num == currentDifficulty) {
                difficultyUI[currentDifficulty].SetActive(true);
                selectLight[currentDifficulty].GetComponent<Image>().sprite = selectLightSprite;
            }else {
                difficultyUI[num].SetActive(false);
                selectLight[num].GetComponent<Image>().sprite = notSelectLightSprite;
            }
        }
    }

    /// <summary>
    /// スタート画面に戻る
    /// </summary>
    public void OnClickHomeButton() {
        soundSE(new AudioManager().GetSEFlag());
        eventSystem.SetActive(false);
        FadeManager.Instance.LoadScene(0.5f, "Start");
        defaultBannerView.Destroy();
    }


    /// <summary>
    /// BGMのオンオフ
    /// </summary>
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


    /// <summary>
    /// SEのオンオフ
    /// </summary>
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
