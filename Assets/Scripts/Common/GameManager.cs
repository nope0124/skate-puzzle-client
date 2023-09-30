using UnityEngine;

/// <summary>
/// GameStateを扱うクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameObject gameManagerObject;
    private static GameManager singleton;

    private GameState currentGameState = GameState.Ready;

    public GameState CurrentGameState
    {
        get { return currentGameState; }
        set { currentGameState = value; }
    }

    public static GameManager Instance
    {
        get {
            if (singleton == null)
            {
                // GameManager作成
                gameManagerObject = new GameObject("GameManager");
                gameManagerObject.AddComponent<GameManager>();

                // 遷移先シーンでもオブジェクトを破棄しない。
                DontDestroyOnLoad(gameManagerObject);

                singleton = gameManagerObject.GetComponent<GameManager>();
            }
            return singleton;
        }
    }

}
