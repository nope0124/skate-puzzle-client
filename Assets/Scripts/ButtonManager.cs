using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public int argStageNumber { get; set; }
    public int argDifficulty { get; set; }

    public void OnClickStageSelectButton()
    {
        Debug.Log(argStageNumber);
        new MainManager().SetStageNumber(argStageNumber);
        new MainManager().SetDifficulty(argDifficulty);
        SceneManager.LoadScene("Main");
    }
}
