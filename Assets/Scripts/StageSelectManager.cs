using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// ステージセレクト画面を扱うクラス
/// </summary>
public class StageSelectManager : MonoBehaviour
{
    float EPS = 1e-5f;

    static int currentDifficulty = 0; // 現在の難易度(easy, normal)
    const int difficultyNumber = 2; // 難易度数
    const int stageNumber = 15; // 一難易度のステージ数

    const int stageSelectWidth = 350;
    const int stageSelectHeight = 300;
    const int stageSelectWidthCenter = 0;
    const int stageSelectHeightCenter = 30;
    const int stageSelectWidthNumber = 5;
    const int stageSelectHeightNumber = 3;

    const int selectLightWidth = 150;
    const int selectLightWidthCenter = 0; // SelectLightの水平方向の中心点
    const int selectLightHeightCenter = -270; // SelectLightの垂直方向の中心点
    const int selectLightWidthNumber = difficultyNumber; // SelectLightの数は難易度数と同じ


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


    [SerializeField] GameObject loading;

    // ステージボタンの生成
    private void GenerateStageSelectButton() {
        int stageId = 0;
        for(int tempDifficulty = 0; tempDifficulty < difficultyNumber; tempDifficulty++) {
            for(int y = 0; y < stageSelectHeightNumber; y++) {
                for(int x = 0; x < stageSelectWidthNumber; x++) {

                    // ステージID、ステージ名、スコアを定義
                    string stageName = "StageScore" + stageId.ToString();
                    int stageScore = PlayerPrefs.GetInt(stageName, -1);

                    // positionを定めてステージを生成、難易度別の親に付ける
                    float stageClonePositionX = stageSelectWidthCenter+stageSelectWidth/(stageSelectWidthNumber-1)*x-stageSelectWidth/2.0f;
                    float stageClonePositionY = stageSelectHeightCenter-stageSelectHeight/(stageSelectHeightNumber-1)*y+stageSelectHeight/2.0f;
                    GameObject stageClone = Instantiate(stagePrefab, new Vector3(stageClonePositionX, stageClonePositionY, 0.0f), Quaternion.identity);
                    stageClone.transform.SetParent(difficultyUI[tempDifficulty].transform, false);

                    // ステージ番号を表示
                    GameObject stageCloneChildStageButton = stageClone.transform.Find("StageButton").gameObject;
                    stageCloneChildStageButton.transform.Find("StageIdText").GetComponent<Text>().text = stageId.ToString();

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

                    // チュートリアルステージにはマークをつける
                    if(stageName == "StageScore0") stageCloneChildStageButton.transform.Find("StageBeginnerImage").gameObject.SetActive(true);
                    else stageCloneChildStageButton.transform.Find("StageBeginnerImage").gameObject.SetActive(false);

                    // stageIdをインクリメント
                    stageId++;
                }
            }
            // 最後に大元のレイヤーに付ける
            difficultyUI[tempDifficulty].transform.SetParent(buttonLayer.transform, false);
        }
        loading.SetActive(false);
    }

    /// <summary>
    /// セレクトライトの生成
    /// </summary>
    private void GenerateSelectLight() {
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

    /// <summary>
    /// 選択されている難易度のステージセレクト画面を表示
    /// </summary>
    private void DisplayStageSelectByDifficulty() {
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
        // バナー広告呼び出し
        // AdmobManager.Instance.RequestDefaultBanner("Bottom");

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
        GenerateStageSelectButton();
    }

    private string baseURL = "http://localhost:3000";

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
        // AdmobManager.Instance.DestroyDefaultBanner();
    }

}
