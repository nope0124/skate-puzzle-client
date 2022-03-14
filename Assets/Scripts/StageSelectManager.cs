using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{

    static int difficulty = 0;
    const int width = 600;
    const int height = 300;
    const int widthNumber = 5;
    const int heightNumber = 3;
    const int difficultyNumber = 3;

    [SerializeField] GameObject canvas;
    [SerializeField] GameObject easyUI;
    [SerializeField] GameObject normalUI;
    [SerializeField] GameObject hardUI;
    [SerializeField] GameObject stagePrefab;

    GameObject[] difficultyUI;
    void Start()
    {
        difficultyUI = new GameObject[]{easyUI, normalUI, hardUI};
        for(int num = 0; num < difficultyNumber; num++) {
            for(int y = 0; y < heightNumber; y++) {
                for(int x = 0; x < widthNumber; x++) {

                    int stageId = y*widthNumber+x;
                    string stageName = "StageScore" + num.ToString() + "_" + stageId.ToString();
                    int stageScore = PlayerPrefs.GetInt(stageName, 0);

                    GameObject stageClone = Instantiate(stagePrefab, new Vector3(width/(widthNumber-1)*x-width/2.0f, -height/(heightNumber-1)*y+height/2.0f, 0.0f), Quaternion.identity);
                    stageClone.transform.SetParent(difficultyUI[num].transform, false);

                    GameObject stageCloneChildScoreText = stageClone.transform.Find("ScoreText").gameObject;
                    if(stageScore > 0) stageCloneChildScoreText.SetActive(true);
                    else stageCloneChildScoreText.SetActive(false);

                    GameObject stageCloneChildStageButton = stageClone.transform.Find("StageButton").gameObject;
                    stageCloneChildStageButton.transform.Find("StageIdText").GetComponent<Text>().text = (stageId+num*heightNumber*widthNumber).ToString();

                    ButtonManager stageCloneChildStageButtonScript = stageCloneChildStageButton.GetComponent<ButtonManager>();
                    stageCloneChildStageButtonScript.argStageNumber = stageId;
                    stageCloneChildStageButtonScript.argDifficulty = num;

                }
            }
            difficultyUI[num].transform.SetParent(canvas.transform, false);
        }
    }


    void Update()
    {
        for(int num = 0; num < difficultyNumber; num++) {
            if(num == difficulty) difficultyUI[num].SetActive(true);
            else difficultyUI[num].SetActive(false);
        }
    }

    // public void OnClickStageSelectButton(int argStageNumber)
    // {
    //     new MainManager().SetStageNumber(argStageNumber);
    //     new MainManager().SetDifficulty(difficulty);
    //     SceneManager.LoadScene("Main");
    // }

    public void OnClickStageEasierButton() {
        difficulty = (difficulty+2)%3;
    }

    public void OnClickStageHarderButton() {
        difficulty = (difficulty+1)%3;
    }

}
