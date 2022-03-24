using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public int argStageId { get; set; }
    public int argDifficulty { get; set; }

    public void OnClickStageSelectButton()
    {
        new MainManager().SetCurrentStageId(argStageId);
        new MainManager().SetCurrentDifficulty(argDifficulty);
        SceneManager.LoadScene("Main");
    }
}
