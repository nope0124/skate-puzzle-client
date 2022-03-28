using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public int argStageId { get; set; }
    public int argDifficulty { get; set; }
    [SerializeField] AudioSource decisionSoundEffect;

    public void soundSE(bool argSEFlag) {
        if(!argSEFlag) {
            decisionSoundEffect.GetComponent<AudioSource>().PlayOneShot(decisionSoundEffect.GetComponent<AudioSource>().clip);
        }
    }

    public void OnClickStageSelectButton()
    {
        soundSE(new AudioManager().GetSEFlag());
        new MainManager().SetCurrentStageId(argStageId);
        new MainManager().SetCurrentDifficulty(argDifficulty);
        SceneManager.LoadScene("Main");
    }
}
