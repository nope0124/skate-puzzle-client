using UnityEngine;
using UnityEngine.UI;

public class SEButtonManager : MonoBehaviour
{

    [SerializeField] Sprite seSprite;
    [SerializeField] Sprite pressedSESprite;

    [SerializeField] Sprite muteSESprite;
    [SerializeField] Sprite pressedMuteSESprite;

    private SpriteState state;

    private void Start()
    {
        ChangeSESprite();
        gameObject.GetComponent<Button>().onClick.AddListener(OnClickSEButton);
    }

    private void OnClickSEButton() {
        AudioManager.Instance.PlaySE("Decision");
        AudioManager.Instance.SEStatus = !(AudioManager.Instance.SEStatus);
        ChangeSESprite();
    }

    private void ChangeSESprite() {
        if(AudioManager.Instance.SEStatus) {
            gameObject.GetComponent<Image>().sprite = seSprite;
            state = gameObject.GetComponent<Button>().spriteState;
            state.pressedSprite = pressedSESprite;
        }else {
            gameObject.GetComponent<Image>().sprite = muteSESprite;
            state = gameObject.GetComponent<Button>().spriteState;
            state.pressedSprite = pressedMuteSESprite;
        }
        gameObject.GetComponent<Button>().spriteState = state;
    }
}
