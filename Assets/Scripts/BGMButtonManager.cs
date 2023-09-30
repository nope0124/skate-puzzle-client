using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BGMボタンを扱うクラス
/// </summary>
public class BGMButtonManager : MonoBehaviour
{

    [SerializeField] Sprite bgmSprite;
    [SerializeField] Sprite pressedBGMSprite;

    [SerializeField] Sprite muteBGMSprite;
    [SerializeField] Sprite pressedMuteBGMSprite;

    private SpriteState state;

    private void Start()
    {
        ChangeBGMSprite();
        gameObject.GetComponent<Button>().onClick.AddListener(OnClickBGMButton);
    }

    private void OnClickBGMButton() {
        AudioManager.Instance.PlaySE("Decision");
        AudioManager.Instance.BGMStatus = !(AudioManager.Instance.BGMStatus);
        ChangeBGMSprite();
    }

    private void ChangeBGMSprite() {
        if(AudioManager.Instance.BGMStatus) {
            gameObject.GetComponent<Image>().sprite = bgmSprite;
            state = gameObject.GetComponent<Button>().spriteState;
            state.pressedSprite = pressedBGMSprite;
        }else {
            gameObject.GetComponent<Image>().sprite = muteBGMSprite;
            state = gameObject.GetComponent<Button>().spriteState;
            state.pressedSprite = pressedMuteBGMSprite;
        }
        gameObject.GetComponent<Button>().spriteState = state;
    }
}
