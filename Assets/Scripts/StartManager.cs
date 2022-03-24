using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class StartManager : MonoBehaviour
{
    static int difficultyNumber = 3;
    static int stageNumber = 15;

    [SerializeField] GameObject dataResetPanel;
    [SerializeField] GameObject dataResetFinishPanel;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnClickStartButton()
    {
        SceneManager.LoadScene("StageSelect");
    }

    public void OnClickDataResetButton() {
        if (!dataResetPanel.activeSelf) {
            dataResetPanel.SetActive(true);
        }
    }

    public void OnClickDataResetYesButton() {
        for(int tempDifficulty = 0; tempDifficulty < difficultyNumber; tempDifficulty++) {
            for(int tempStageId = 0; tempStageId < stageNumber; tempStageId++) {
                string stageName = "StageScore" + tempDifficulty.ToString() + "_" + tempStageId.ToString();
                PlayerPrefs.SetInt(stageName, 0);
            }
        }
        dataResetPanel.SetActive(false);
        dataResetFinishPanel.SetActive(true);
    }
    
    public void OnClickDataResetNoButton() {
        dataResetPanel.SetActive(false);
    }

    public void OnClickDataResetFinishButton() {
        dataResetFinishPanel.SetActive(false);
    }
}
