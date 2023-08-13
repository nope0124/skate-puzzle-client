using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public int argStageId { get; set; }
    public int argDifficulty { get; set; }
    [SerializeField] AudioSource decisionSoundEffect;

    public void OnClickStageSelectButton()
    {
        AudioManager.Instance.PlaySE("Decision");
        new MainManager().SetCurrentStageId(argStageId);
        new MainManager().SetCurrentDifficulty(argDifficulty);
        new StageSelectManager().SetFadeOutFlagToMain();
    }
}
