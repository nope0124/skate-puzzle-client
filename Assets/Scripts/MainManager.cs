using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;
using Firebase;
using Firebase.Database;

public class MainManager : MonoBehaviour
{
    public enum GameState
    {
        Ready,
        Playing,
        Pause,
        Hint,
        Clear
    }
    

    private GameState currentGameState = GameState.Ready;
    // Left Right Up Down
    private string baseURL = "http://localhost:3000";
    int[] dx = {-1, 1, 0, 0};
    int[] dy = {0, 0, 1, -1};
    string[] ANIMATOR_DIR = {"ToLeft", "ToRight", "ToUp", "ToDown"};
    float EPS = 1e-5f;

    private BannerView defaultBannerView;
    private InterstitialAd stageTransInterstitialAd;
    private InterstitialAd hintInterstitialAd;

    [SerializeField] TileBase[] iceFloor;
    [SerializeField] TileBase[] iceBlock;
    [SerializeField] TileBase snowBall;
    [SerializeField] TileBase[] snowFloor;
    [SerializeField] TileBase[] goalFloor;

    [SerializeField] GameObject stageBoardPrefab;
    [SerializeField] GameObject player;
    
    [SerializeField] GameObject gridLayer;

    [SerializeField] GameObject unitLayer;
    [SerializeField] GameObject UILayer;
    [SerializeField] GameObject buttonLayer;
    [SerializeField] GameObject effectLayer;

    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject hintPanel;
    [SerializeField] GameObject clearPanel;

    [SerializeField] Animator playerAnim;

    [SerializeField] GameObject fadeImage;

    [SerializeField] GameObject[] DPadButton;
    [SerializeField] Sprite[] blueButton;
    [SerializeField] Sprite[] redButton;

    [SerializeField] AudioClip bgmAudioClip;

    [SerializeField] Text stageIdText;
    [SerializeField] Text turnCountText;

    [SerializeField] GameObject[] clearScore;


    [SerializeField] GameObject Test;
    


    Tilemap stageBoard;

    static int currentStageId = 0;
    const int stageNumber = 30;

    int startPointXOnBoard, startPointYOnBoard, goalPointXOnBoard, goalPointYOnBoard;
    float startPointX, startPointY, goalPointX, goalPointY;
    int currentPlayerXOnBoard, currentPlayerYOnBoard;
    float currentPlayerX, currentPlayerY;
    float stageBoardWidthCenter = 0.0f;
    float stageBoardHeightCenter = 0.5f;
    float stageBoardWidth, stageBoardHeight;
    float screenScale = 4.5f;
    char[][] stageBoardGrid, currentStageBoardGrid;

    // 基本的に長さは同じ
    int stageBoardGridWidth, stageBoardGridHeight;

    bool isFinish = false;
    bool reachedSnowBall = false;

    Vector3 targetPosition;
    Vector3 goalPosition;
    Vector3Int snowBallPosition;
    [SerializeField] float moveSpeed = 1.0f;
    bool saveFlag = false;


    bool fadeOutFlag = false;
    bool fadeInFlag = true;
    float fadeTimeCount = 1.0f;

    bool hintFlag = false;
    static int gamePlayCount = 0;
    int adPlayBorderCount = 7;
    bool nextFlag = false;
    // bool backFlag = false;

    Queue<int> hintMovesStack = new Queue<int>();

    int turnCount = 0;
    // string userId = "";
    // DatabaseReference scoreReference;
    string[] difficultyName;

    public int CurrentStageId
    {
        get { return currentStageId; }
        set { currentStageId = value; }
    }

    private void RequestDefaultBanner()
    {
        #if UNITY_IOS
            string adUnitId = Const.CO.IPHONE_DEFAULT_BANNER;
        #else
            string adUnitId = "unexpected_platform";
        #endif

        defaultBannerView = new BannerView(adUnitId, AdSize.IABBanner, AdPosition.Top);
        AdRequest request = new AdRequest.Builder().Build();

        defaultBannerView.LoadAd(request);
    }

    private void RequestStageTransInterstitial()
    {
        // ★リリース時に自分のIDに変更する
        #if UNITY_IOS
            string adUnitId = Const.CO.IPHONE_STAGE_TRANS_INTERSTITIAL;
        #else
            string adUnitId = "unexpected_platform";
        #endif

        // Initialize an InterstitialAd.
        stageTransInterstitialAd = new InterstitialAd(adUnitId);

        // Called when an ad request has successfully loaded.
        stageTransInterstitialAd.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        stageTransInterstitialAd.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        stageTransInterstitialAd.OnAdOpening += HandleOnAdOpened;
        // Called when the ad is closed.
        stageTransInterstitialAd.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        stageTransInterstitialAd.OnAdDidRecordImpression += HandleOnAdLeavingApplication;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        stageTransInterstitialAd.LoadAd(request);
    }


    private void OnDestroy()
    {
        // オブジェクトの破棄
        stageTransInterstitialAd.Destroy();
    }

    // ---以下、イベントハンドラー
    
    // 広告の読み込み完了時
    public void HandleOnAdLoaded(object sender, System.EventArgs args)
    {
    }

    // 広告の読み込み失敗時
    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // 次のシーンに遷移
        gamePlayCount = 0;
        RequestStageTransInterstitial();
        hintFlag = false;
        // if(backFlag == true) {
        //     backFlag = false;
        //     SceneManager.LoadScene("StageSelect");
        // }

    }

    // 広告がデバイスの画面いっぱいに表示されたとき
    public void HandleOnAdOpened(object sender, System.EventArgs args)
    {
    }

    // 広告を閉じたとき
    public void HandleOnAdClosed(object sender, System.EventArgs args)
    {
        
        // 次のシーンに遷移
        if(hintFlag == true) {
            gamePlayCount = Mathf.Max(0, gamePlayCount-2);
            hintFlag = false;
            RequestStageTransInterstitial();
        } else if (nextFlag == true) {
            gamePlayCount = 0;
            nextFlag = false;
            RequestStageTransInterstitial();
        } else {
            gamePlayCount = 0;
            SceneManager.LoadScene("StageSelect");
        }
    }
    
    // 別のアプリ（Google Play ストアなど）を起動した時
    public void HandleOnAdLeavingApplication(object sender, System.EventArgs args)
    {
    }


    void AnimatorReset() {
        for(int i = 0; i < 4; i++) playerAnim.SetBool(ANIMATOR_DIR[i], false);
    }

    void SetTileBoard(int x, int y, TileBase[] tempTile) {
        // y = stageBoardGridHeight - y - 1;
        Vector3Int grid = new Vector3Int(x, y, 0);
        if(x == 0 && y == 0) { // 左下
            stageBoard.SetTile(grid, tempTile[6]);
        }else if(x == stageBoardGridWidth-1 && y == 0) { // 右下
            stageBoard.SetTile(grid, tempTile[8]);
        }else if(x == 0 && y == stageBoardGridHeight-1) { // 左上
            stageBoard.SetTile(grid, tempTile[0]);
        }else if(x == stageBoardGridWidth-1 && y == stageBoardGridHeight-1) { // 右上
            stageBoard.SetTile(grid, tempTile[2]);
        }else if(x == 0) { // 右
            stageBoard.SetTile(grid, tempTile[3]);
        }else if(x == stageBoardGridWidth-1) { // 左
            stageBoard.SetTile(grid, tempTile[5]);
        }else if(y == 0) { // 下
            stageBoard.SetTile(grid, tempTile[7]);
        }else if(y == stageBoardGridHeight-1) { // 上
            stageBoard.SetTile(grid, tempTile[1]);
        }else { // 中
            stageBoard.SetTile(grid, tempTile[4]);
        }
    }


    void SetTiles() {
        for(int y = 0; y < stageBoardGridHeight; y++) {
            for(int x = 0; x < stageBoardGridWidth; x++) {
                Vector3Int grid = new Vector3Int(x, y, 0);
                switch(stageBoardGrid[y][x]) {
                    case '.':
                        SetTileBoard(x, y, iceFloor);
                        break;
                    case 'x':
                        SetTileBoard(x, y, iceBlock);
                        break;
                    case '@':
                        stageBoard.SetTile(grid, snowBall);
                        break;
                    case 'o':
                        SetTileBoard(x, y, snowFloor);
                        break;
                    case 'S':
                        SetTileBoard(x, y, snowFloor);
                        startPointX = stageBoard.CellToWorld(grid).x;
                        startPointY = stageBoard.CellToWorld(grid).y;
                        startPointXOnBoard = x;
                        startPointYOnBoard = y;
                        break;
                    case 'G':
                        SetTileBoard(x, y, goalFloor);
                        goalPointX = stageBoard.CellToWorld(grid).x;
                        goalPointY = stageBoard.CellToWorld(grid).y;
                        goalPointXOnBoard = x;
                        goalPointYOnBoard = y;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    void GetBoardScale(int width, int height, string[] board) {
        // stageBoardGrid = new Board().GetBoard(currentDifficulty, currentStageId);
        stageBoardGrid = new char[height][];
        for(int i = 0; i < height; i++) stageBoardGrid[i] = new char[width];
        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {
                stageBoardGrid[y][x] = board[y][x];
            }
        }
        stageBoardGridWidth = stageBoardGrid[0].Length;
        stageBoardGridHeight = stageBoardGrid.Length;
        stageBoardWidth = screenScale/stageBoardGridWidth;
        stageBoardHeight = screenScale/stageBoardGridHeight;

        // 盤面のサイズを調整
        stageBoard.GetComponent<Transform>().position = new Vector3(-stageBoardGridWidth*stageBoardWidth/2.0f+stageBoardWidthCenter, -stageBoardGridHeight*stageBoardHeight/2.0f+stageBoardHeightCenter, 0.0f);
        stageBoard.GetComponent<Transform>().localScale = new Vector3(stageBoardWidth, stageBoardHeight, 0.0f);
    }


    IEnumerator GetStageData(int stage_id)
    {
        string url = baseURL + "/api/v1/stages/" + (stage_id + 1).ToString();
        Debug.Log(url);
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if(request.responseCode == 200)
            {
                // JSONデータをC#オブジェクトにデシリアライズ
                GetStageBoardResponse data = JsonUtility.FromJson<GetStageBoardResponse>(request.downloadHandler.text);

                Debug.Log(data.width);
                Debug.Log(data.height);
                Debug.Log(data.board);
                GetBoardScale(data.width, data.height, data.board);
                // 盤面の状態を反映
                SetTiles();
                // プレイヤー位置の初期化
                player.transform.position = new Vector3(startPointX+stageBoardWidth/2.0f, startPointY+stageBoardHeight/2.0f, 0.0f);
                currentPlayerX = startPointX; currentPlayerXOnBoard = startPointXOnBoard;
                currentPlayerY = startPointY; currentPlayerYOnBoard = startPointYOnBoard;
                player.transform.localScale = new Vector3(stageBoardWidth, stageBoardHeight, 1.0f);
                targetPosition = new Vector3(currentPlayerX+stageBoardWidth/2.0f, currentPlayerY+stageBoardHeight/2.0f, 0.0f);
                goalPosition = new Vector3(goalPointX+stageBoardWidth/2.0f, goalPointY+stageBoardHeight/2.0f, 0.0f);

                // 盤面の状態をコピー // ディープコピーの方法がわからない
                currentStageBoardGrid = new char[stageBoardGridHeight][];
                for(int i = 0; i < stageBoardGridHeight; i++) currentStageBoardGrid[i] = new char[stageBoardGridWidth];
                for(int i = 0; i < stageBoardGridHeight; i++) {
                    for(int j = 0; j < stageBoardGridWidth; j++) {
                        currentStageBoardGrid[i][j] = stageBoardGrid[i][j];
                    }
                }

                Test.SetActive(false);
            }
        }
    }


    void Init() {

        Time.timeScale = 1.0f;
        saveFlag = false;
        hintFlag = false;
        hintMovesStack.Clear();


        stageIdText.text = "STAGE: " + (currentStageId).ToString("000");
        turnCount = 0;

        for(int i = 0; i < 3; i++) {
            clearScore[i].SetActive(false);
        }

        ButtonHintReset();

        // // フェードイン
        // fadeImage.SetActive(true);
        // fadeInFlag = true;
        // fadeOutFlag = false;

        // BGMの設定
        AudioManager.Instance.SetBGMAudioClip(bgmAudioClip);

        // プレイヤーのアニメーションをリセット
        AnimatorReset();
        playerAnim.SetFloat("MovingSpeed", 0.0f);
        playerAnim.SetBool("ToDown", true);

        // 盤面のタイルマップをリセット
        Destroy(GameObject.Find("StageBoard(Clone)"));
        stageBoard = Instantiate(stageBoardPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity).GetComponent<Tilemap>();
        stageBoard.transform.SetParent(gridLayer.transform, false);

        // 盤面のサイズを取得
        StartCoroutine(GetStageData(currentStageId));
        // GetBoardScale();

        // // 盤面の状態をコピー // ディープコピーの方法がわからない
        // currentStageBoardGrid = new char[stageBoardGridHeight][];
        // for(int i = 0; i < stageBoardGridHeight; i++) currentStageBoardGrid[i] = new char[stageBoardGridWidth];
        // for(int i = 0; i < stageBoardGridHeight; i++) {
        //     for(int j = 0; j < stageBoardGridWidth; j++) {
        //         currentStageBoardGrid[i][j] = stageBoardGrid[i][j];
        //     }
        // }

        // // 盤面の状態を反映
        // SetTiles();

        // // プレイヤー位置の初期化
        // player.transform.position = new Vector3(startPointX+stageBoardWidth/2.0f, startPointY+stageBoardHeight/2.0f, 0.0f);
        // currentPlayerX = startPointX; currentPlayerXOnBoard = startPointXOnBoard;
        // currentPlayerY = startPointY; currentPlayerYOnBoard = startPointYOnBoard;
        // player.transform.localScale = new Vector3(stageBoardWidth, stageBoardHeight, 1.0f);
        // targetPosition = new Vector3(currentPlayerX+stageBoardWidth/2.0f, currentPlayerY+stageBoardHeight/2.0f, 0.0f);
        // goalPosition = new Vector3(goalPointX+stageBoardWidth/2.0f, goalPointY+stageBoardHeight/2.0f, 0.0f);

        // その他変数を初期化
        isFinish = false;
        reachedSnowBall = false;
        pausePanel.SetActive(false);
        clearPanel.SetActive(false);
    }


    void saveData() {
        string stageName = "StageScore" + currentStageId.ToString();
        string stageNameUnlock = "StageScore" + ((currentStageId + 1) % stageNumber).ToString();
        int tempScore = PlayerPrefs.GetInt(stageName, 0);
        int distance = new Solver().GetOptMoves(stageBoardGrid);
        string userId = PlayerPrefs.GetString("user_id");
        if(userId == "") {
            if(turnCount <= distance) {
                AudioManager.Instance.PlaySE("Clear");
                clearScore[2].SetActive(true);
                PlayerPrefs.SetInt(stageName, Mathf.Max(tempScore, 3));
            }else if(turnCount <= distance*2) {
                clearScore[1].SetActive(true);
                PlayerPrefs.SetInt(stageName, Mathf.Max(tempScore, 2));
            }else {
                clearScore[0].SetActive(true);
                PlayerPrefs.SetInt(stageName, Mathf.Max(tempScore, 1));
            }
        }else {
            FirebaseDatabase.GetInstance(Const.CO.DATABASE_URL); // データベースのURLを設定
            DatabaseReference databaseRoot = FirebaseDatabase.DefaultInstance.RootReference; // ルートを作成
            DatabaseReference scoreReference = databaseRoot.Child("users").Child(userId).Child("scores");
            Dictionary<string, object> childUpdates = new Dictionary<string, object>();
            difficultyName = new string[]{"easy", "normal", "hard"};
            if(turnCount <= distance) {
                AudioManager.Instance.PlaySE("Clear");
                clearScore[2].SetActive(true);
                PlayerPrefs.SetInt(stageName, Mathf.Max(tempScore, 3));
            }else if(turnCount <= distance*2) {
                clearScore[1].SetActive(true);
                PlayerPrefs.SetInt(stageName, Mathf.Max(tempScore, 2));
            }else {
                clearScore[0].SetActive(true);
                PlayerPrefs.SetInt(stageName, Mathf.Max(tempScore, 1));
            }
            // childUpdates["/"+difficultyName[currentDifficulty]+"/"+currentStageId.ToString()] = PlayerPrefs.GetInt(stageName);
            // if((currentDifficulty + (currentStageId+1)/stageNumber) % difficultyNumber < 2) {
            //     PlayerPrefs.SetInt(stageNameUnlock, Mathf.Max(0, PlayerPrefs.GetInt(stageNameUnlock)));
            //     childUpdates["/"+difficultyName[(currentDifficulty + (currentStageId+1)/stageNumber) % difficultyNumber]+"/"+((currentStageId+1) % stageNumber).ToString()] = PlayerPrefs.GetInt(stageNameUnlock);
            // }
            // scoreReference.UpdateChildrenAsync(childUpdates);
        }
    }

    void Start()
    {
        // userId = PlayerPrefs.GetString("user_id");
        // FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Const.CO.DATABASE_URL); // データベースのURLを設定
        // DatabaseReference scoreReference = FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(userId).Child("scores");
        MobileAds.Initialize(initStatus => { });
        RequestDefaultBanner();
        RequestStageTransInterstitial();
        // 盤面のサイズを取得
        // 盤面のサイズを調整
        // 盤面の状態をコピー
        // 盤面の状態を取得
        // プレイヤー位置の初期化
        Init();
    }
    

    void Update()
    {
        if(gamePlayCount >= 20) gamePlayCount = 20;
        turnCountText.text = "TURN: " + turnCount.ToString("000");
        
        switch (currentGameState) {
            case GameState.Ready:
                break;
            case GameState.Pause:
                break;

            case GameState.Playing:
                // Playerを動かす
                player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, moveSpeed * Time.deltaTime);

                // targetPositionに着いたら状態を遷移させる
                if((player.transform.position - goalPosition).magnitude <= EPS) {
                    currentGameState = GameState.Clear;
                    return;
                }else if((player.transform.position - targetPosition).magnitude <= EPS) {
                    currentGameState = GameState.Ready;
                    playerAnim.SetFloat("MovingSpeed", 0.0f);
                    playerAnim.Play(playerAnim.GetCurrentAnimatorStateInfo(0).nameHash, 0, 0.0f);
                    return;
                }

                // 氷タイルだったらアニメーションを止める
                if(stageBoardGrid[stageBoard.WorldToCell(player.transform.position).y][stageBoard.WorldToCell(player.transform.position).x] == '.') {
                    playerAnim.SetFloat("MovingSpeed", 0.0f);
                }else {
                    playerAnim.SetFloat("MovingSpeed", 1.0f);
                }

                break;

            case GameState.Clear:
                playerAnim.SetFloat("MovingSpeed", 0.0f); // 止める
                AnimatorReset(); // 一回だけ
                playerAnim.SetBool("ToDown", true); // リトライ後も下を向くように
                clearPanel.SetActive(true);

                if(saveFlag == false) { //1回だけセーブ
                    saveData();
                    saveFlag = true;
                }

                break;

            default:
                break;

        }
    }


    private void MovePlayer(int i)  {

        // 次の到着場所を事前に計算
        while(true) {
            int nx = currentPlayerXOnBoard + dx[i];
            int ny = currentPlayerYOnBoard + dy[i];
            if(nx < 0 || ny < 0 || nx >= stageBoardGridWidth || ny >= stageBoardGridHeight) break;
            if(currentStageBoardGrid[ny][nx] == '#' || currentStageBoardGrid[ny][nx] == 'x') break;
            if(currentStageBoardGrid[ny][nx] == '@') {
                currentStageBoardGrid[ny][nx] = '.';
                snowBallPosition = new Vector3Int(nx, ny, 0);
                reachedSnowBall = true;
                break;
            }
            if(currentStageBoardGrid[ny][nx] == 'S' || currentStageBoardGrid[ny][nx] == 'G' || currentStageBoardGrid[ny][nx] == 'o') {
                currentPlayerXOnBoard += dx[i];
                currentPlayerYOnBoard += dy[i];
                break;
            }
            currentPlayerXOnBoard += dx[i];
            currentPlayerYOnBoard += dy[i];
        }

        // targetPositionを更新
        Vector3Int grid = new Vector3Int(currentPlayerXOnBoard, currentPlayerYOnBoard, 0);
        currentPlayerX = stageBoard.CellToWorld(grid).x;
        currentPlayerY = stageBoard.CellToWorld(grid).y;
        targetPosition = new Vector3(currentPlayerX+stageBoardWidth/2.0f, currentPlayerY+stageBoardHeight/2.0f, 0.0f);

        // // もし到着場所がゴールならFinishフラグを立てる
        // if(goalPointXOnBoard == currentPlayerXOnBoard && goalPointYOnBoard == currentPlayerYOnBoard) isFinish = true;
    }

    public void OnClickMoveButton(int i) {
        if(!(currentGameState == GameState.Ready)) return;
        currentGameState = GameState.Playing;
        if(hintMovesStack.Count > 0) {
            if(i == hintMovesStack.Peek()) {
                MovePlayer(i);
                AnimatorReset();
                if(turnCount < 999) turnCount++;
                AudioManager.Instance.PlaySE("Decision");
                playerAnim.SetBool(ANIMATOR_DIR[i], true);
                hintMovesStack.Dequeue();
                ButtonHintReset();
                if(hintMovesStack.Count > 0) DPadButton[hintMovesStack.Peek()].GetComponent<Image>().sprite = redButton[hintMovesStack.Peek()];
            }else {
                return;
            }
        }else {
            MovePlayer(i);
            if((player.transform.position - targetPosition).magnitude <= EPS && playerAnim.GetBool(ANIMATOR_DIR[i])) return;
            AnimatorReset();
            if(turnCount < 999) turnCount++;
            AudioManager.Instance.PlaySE("Decision");
            playerAnim.SetBool(ANIMATOR_DIR[i], true);
        }
        
    }

    public void OnClickPauseButton() {
        if(!(currentGameState == GameState.Ready)) return;
        AudioManager.Instance.PlaySE("Decision");
        pausePanel.SetActive(true);
        Time.timeScale = 0.0f;
    }

    public void OnClickBackButton() {
        if(!(currentGameState == GameState.Ready || currentGameState == GameState.Clear)) return;
        AudioManager.Instance.PlaySE("Decision");
        Time.timeScale = 1.0f;
        defaultBannerView.Destroy();
        FadeManager.Instance.LoadScene(0.5f, "StageSelect");
    }

    public void OnClickRetryButton() {
        if(!(currentGameState == GameState.Ready || currentGameState == GameState.Clear)) return;
        AudioManager.Instance.PlaySE("Decision");
        currentGameState = GameState.Ready;
        gamePlayCount++;
        Init();
        pausePanel.SetActive(false);
        Time.timeScale = 1.0f;
    }

    public void OnClickPlayButton() {
        if(!(currentGameState == GameState.Ready)) return;
        AudioManager.Instance.PlaySE("Decision");
        currentGameState = GameState.Ready;
        pausePanel.SetActive(false);
        Time.timeScale = 1.0f;
    }

    public void OnClickNextButton() {
        if(!(currentGameState == GameState.Clear)) return;
        AudioManager.Instance.PlaySE("Decision");
        currentGameState = GameState.Ready;
        gamePlayCount += 2;
        currentStageId = (currentStageId+1) % stageNumber;
        Init();
        if(gamePlayCount >= adPlayBorderCount) {
            nextFlag = true;
            stageTransInterstitialAd.Show();
        }
    }

    public void OnClickHintButton() {
        AudioManager.Instance.PlaySE("Decision");
        if(isFinish) return;
        if (!hintPanel.activeSelf) {
            hintPanel.SetActive(true);
            Time.timeScale = 0.0f;
        }
    }

    void ButtonHintReset() {
        for(int i = 0; i < 4; i++) {
            DPadButton[i].GetComponent<Image>().sprite = blueButton[i];
        }
    }
    
    public void OnClickHintYesButton() {
        AudioManager.Instance.PlaySE("Decision");
        hintPanel.SetActive(false);
        Time.timeScale = 1.0f;
        // Init();
        if(stageTransInterstitialAd.IsLoaded()) {
            hintFlag = true;
            // audioSource.GetComponent<AudioSource>().mute = true;
            stageTransInterstitialAd.Show();
        }
        
        Stack<int> HINT = new Solver().Solve(currentStageBoardGrid, currentPlayerXOnBoard, currentPlayerYOnBoard);
        hintMovesStack.Clear();
        for(int i = 0; i < 5 && HINT.Count > 0; i++) {
            hintMovesStack.Enqueue(HINT.Pop());
        }
        ButtonHintReset();
        if(hintMovesStack.Count == 0) return;
        DPadButton[hintMovesStack.Peek()].GetComponent<Image>().sprite = redButton[hintMovesStack.Peek()];
    }
    

    public void OnClickHintNoButton() {
        AudioManager.Instance.PlaySE("Decision");
        hintPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }

}
