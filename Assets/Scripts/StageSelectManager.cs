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
    const int selectLightWidthCenter = 0; // SelectLightの水平方向の中心点
    const int selectLightHeightCenter = -270; // SelectLightの垂直方向の中心点
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

    [SerializeField] AudioClip bgmAudioClip;

    [SerializeField] Sprite selectLightSprite;
    [SerializeField] Sprite notSelectLightSprite;

    GameObject[] difficultyUI;
    GameObject[] selectLight;

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

                    // もし1個前のステージがクリアしてなかったら鍵をかける
                    GameObject stageCloneChildStageLockImage = stageClone.transform.Find("StageLockImage").gameObject;
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
        AudioManager.Instance.SetBGMAudioClip(bgmAudioClip);

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
                GetUserStageProgressResponse data = JsonUtility.FromJson<GetUserStageProgressResponse>(request.downloadHandler.text);

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
            defaultBannerView.Destroy();
            FadeManager.Instance.LoadScene(0.5f, "Main");
        }
    }

    /// <summary>
    /// 難易度ボタン
    /// </summary>
    /// <param name="diff">-1か+1か</param>
    public void OnClickStageLevelButton(int diff) {
        AudioManager.Instance.PlaySE("Decision");
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
        AudioManager.Instance.PlaySE("Decision");
        FadeManager.Instance.LoadScene(0.5f, "Start");
        defaultBannerView.Destroy();
    }

    public void SetFadeOutFlagToMain() {
        fadeOutFlagToMain = true;
    }

}
