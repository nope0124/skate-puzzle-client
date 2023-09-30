﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;

/// <summary>
/// インゲーム画面を扱うクラス
/// </summary>
public class MainManager : MonoBehaviour
{
    private static MainManager singleton;

    public static MainManager Instance
    {
        get { return singleton; }
    }

    void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private string baseURL = "http://localhost:3000";
    int[] dx = {-1, 1, 0, 0};
    int[] dy = {0, 0, 1, -1};
    string[] ANIMATOR_DIR = {"ToLeft", "ToRight", "ToUp", "ToDown"};
    float EPS = 1e-5f;

    [SerializeField] public TileBase[] iceFloor;
    [SerializeField] TileBase[] iceBlock;
    [SerializeField] TileBase[] snowFloor;
    [SerializeField] TileBase[] snowBall;
    [SerializeField] TileBase[] goalFloor;
    [SerializeField] public TileBase[] snowBallCollapse1;
    [SerializeField] public TileBase[] snowBallCollapse2;
    [SerializeField] public TileBase[] snowBallCollapse3;

    [SerializeField] GameObject stageBoardPrefab;
    [SerializeField] GameObject player;
    
    [SerializeField] GameObject gridLayer;
    [SerializeField] GameObject messageLayer;

    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject hintPanel;
    [SerializeField] GameObject clearPanel;

    [SerializeField] Animator playerAnim;

    [SerializeField] Button[] DPadButton;

    [SerializeField] AudioClip bgmAudioClip;

    [SerializeField] Text stageIdText;
    [SerializeField] Text turnCountText;

    [SerializeField] Button[] clearScore;

    [SerializeField] GameObject loading;

    private bool isTutorial = false;

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
    Vector3 targetPosition;
    Vector3 goalPosition;
    [SerializeField] float moveSpeed = 3.0f;
    bool saveFlag = false;
    bool snowFlag = false;
    int snowX = -1;
    int snowY = -1;
    Queue<int> hintMovesStack = new Queue<int>();
    int turnCount = 0;
    string[] difficultyName;

    public int CurrentStageId
    {
        get { return currentStageId; }
        set { currentStageId = value; }
    }


    private void AnimatorReset() {
        for(int i = 0; i < 4; i++) playerAnim.SetBool(ANIMATOR_DIR[i], false);
    }

    /// <summary>
    /// 盤面にタイルを設置
    /// </summary>
    public void SetTileBoard(int x, int y, TileBase[] tempTile) {
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

    /// <summary>
    /// 雪玉限定の処理
    /// </summary>
    public void SetTileBoardFrom(int x, int y, string tmp) {
        switch(tmp) {
            case "SnowBallCollapse1":
                SetTileBoard(x, y, snowBallCollapse1);
                break;
            case "SnowBallCollapse2":
                SetTileBoard(x, y, snowBallCollapse2);
                break;
            case "SnowBallCollapse3":
                SetTileBoard(x, y, snowBallCollapse3);
                break;
            case "IceFloor":
                SetTileBoard(x, y, iceFloor);
                break;
            default:
                break;
        }

    }


    /// <summary>
    /// 盤面を全探索、状態に応じてタイルを設置する
    /// </summary>
    private void SetTiles() {
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
                    case 'o':
                        SetTileBoard(x, y, snowFloor);
                        break;
                    case '@':
                        SetTileBoard(x, y, snowBall);
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

    /// <summary>
    /// 盤面の大きさを取得して調整
    /// </summary>
    private void GetBoardScale(int width, int height, string[] board) {
        stageBoardGrid = new char[height][];
        for(int i = 0; i < height; i++) stageBoardGrid[i] = new char[width];
        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {
                stageBoardGrid[y][x] = board[y][x];
            }
        }

        // マス目の数
        stageBoardGridWidth = stageBoardGrid[0].Length;
        stageBoardGridHeight = stageBoardGrid.Length;

        // マス目一つの縦横サイズ
        stageBoardWidth = screenScale/stageBoardGridWidth;
        stageBoardHeight = screenScale/stageBoardGridHeight;

        // 盤面のサイズを調整
        stageBoard.GetComponent<Transform>().position = new Vector3(-stageBoardGridWidth*stageBoardWidth/2.0f+stageBoardWidthCenter, -stageBoardGridHeight*stageBoardHeight/2.0f+stageBoardHeightCenter, 0.0f);
        stageBoard.GetComponent<Transform>().localScale = new Vector3(stageBoardWidth, stageBoardHeight, 0.0f);
    }


    /// <summary>
    /// ステージAPIから盤面情報を取得
    /// </summary>
    IEnumerator GetStageData(int stage_id)
    {
        loading.SetActive(true);
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

                // 盤面の状態をコピー
                currentStageBoardGrid = new char[stageBoardGridHeight][];
                for(int i = 0; i < stageBoardGridHeight; i++) currentStageBoardGrid[i] = new char[stageBoardGridWidth];
                for(int i = 0; i < stageBoardGridHeight; i++) {
                    for(int j = 0; j < stageBoardGridWidth; j++) {
                        currentStageBoardGrid[i][j] = stageBoardGrid[i][j];
                    }
                }

                loading.SetActive(false);
            }
        }
    }

    
    private void Init() {
        if (currentStageId == 0) {
            isTutorial = true;
        }else {
            isTutorial = false;
        }
        opeIndex = 0;

        Time.timeScale = 1.0f;
        saveFlag = false;
        hintMovesStack.Clear();

        stageIdText.text = "STAGE: " + (currentStageId).ToString("000");
        turnCount = 0;

        ButtonHintReset();

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
        
        // その他変数を初期化
        pausePanel.SetActive(false);
        clearPanel.SetActive(false);
    }


    /// <summary>
    /// データをPlayerPrefsに保存
    /// </summary>
    private void saveData() {
        string stageName = "StageScore" + currentStageId.ToString();
        string stageNameUnlock = "StageScore" + ((currentStageId + 1) % stageNumber).ToString();
        int tempScore = PlayerPrefs.GetInt(stageName, 0);

        // ソルバーから最短距離を取得
        int distance = new Solver().GetOptMoves(stageBoardGrid);
        if(turnCount <= distance || isTutorial) {
            // 星3
            AudioManager.Instance.PlaySE("Clear");
            clearScore[0].interactable = true;
            clearScore[1].interactable = true;
            PlayerPrefs.SetInt(stageName, Mathf.Max(tempScore, 3));
        }else if(turnCount <= distance*2) {
            // 星2
            clearScore[0].interactable = true;
            clearScore[1].interactable = false;
            PlayerPrefs.SetInt(stageName, Mathf.Max(tempScore, 2));
        }else {
            // 星1
            clearScore[0].interactable = false;
            clearScore[1].interactable = false;
            PlayerPrefs.SetInt(stageName, Mathf.Max(tempScore, 1));
        }
        tempScore = PlayerPrefs.GetInt(stageNameUnlock, 0);
        PlayerPrefs.SetInt(stageNameUnlock, Mathf.Max(tempScore, 0));
    }

    void Start()
    {
        if (currentStageId == 0) {
            isTutorial = true;
        }
        // DatabaseReference scoreReference = FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(userId).Child("scores");
        // AdmobManager.Instance.RequestDefaultBanner("Top");
        // AdmobManager.Instance.RequestDefaultInterstitial();
        // 盤面のサイズを取得
        // 盤面のサイズを調整
        // 盤面の状態をコピー
        // 盤面の状態を取得
        // プレイヤー位置の初期化
        Init();
    }
    

    /// <summary>
    /// 移動方向の制限をつける、okIdsに入っている方向だけ許可
    /// </summary>
    public void LimitDPadAction(int[] okIds) {
        for(int i = 0; i < 4; i++) DPadButton[i].interactable = false;
        if(okIds.Length == 0) return;
        foreach(int okId in okIds) DPadButton[okId].interactable = true;
    }


    void Update()
    {
        turnCountText.text = "TURN: " + turnCount.ToString("000");
        switch (GameManager.Instance.CurrentGameState) {
            // プレイヤーが止まっている状態
            case GameState.Ready:
                if(isTutorial) {
                    Tutorial();
                }else if(hintMovesStack.Count > 0) {
                    LimitDPadAction(new int[]{hintMovesStack.Peek()});
                }else {
                    LimitDPadAction(new int[]{0, 1, 2, 3});
                }
                break;

            // ポーズボタンなどが押されている状態
            case GameState.Pause:
                break;

            // プレイヤーが進んでいる状態
            case GameState.Playing:
                // ボタンを押せないように
                LimitDPadAction(new int[]{});

                // Playerを動かす
                player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, moveSpeed * Time.deltaTime);

                // targetPositionに着いたら状態を遷移させる
                if((player.transform.position - goalPosition).magnitude <= EPS) {
                    GameManager.Instance.CurrentGameState = GameState.Clear;
                    return;
                }else if((player.transform.position - targetPosition).magnitude <= EPS) {
                    GameManager.Instance.CurrentGameState = GameState.Ready;
                    playerAnim.SetFloat("MovingSpeed", 0.0f);
                    playerAnim.Play(playerAnim.GetCurrentAnimatorStateInfo(0).nameHash, 0, 0.0f);
                    // 雪玉限定の処理
                    if(snowFlag) {
                        snowFlag = false;
                        GameObject tileGameObject = new GameObject("TileManager");
                        tileGameObject.AddComponent<TileManager>();
                        TileManager tileManager = tileGameObject.GetComponent<TileManager>();
                        tileManager.X = snowX;
                        tileManager.Y = snowY;
                        tileManager.snowBallCollapse1 = snowBallCollapse1;
                        tileManager.snowBallCollapse2 = snowBallCollapse2;
                        tileManager.snowBallCollapse3 = snowBallCollapse3;
                        tileManager.iceFloor = iceFloor;
                        currentStageBoardGrid[snowY][snowX] = '.';
                    }
                    return;
                }

                // 氷タイルだったらアニメーションを止める
                if(stageBoardGrid[stageBoard.WorldToCell(player.transform.position).y][stageBoard.WorldToCell(player.transform.position).x] == '.') {
                    playerAnim.SetFloat("MovingSpeed", 0.0f);
                }else {
                    playerAnim.SetFloat("MovingSpeed", 1.0f);
                }

                break;

            // クリア状態
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


    /// <summary>
    /// プレイヤーを移動させる
    /// </summary>
    private void MovePlayer(int i)  {

        // 次の到着場所を事前に計算
        while(true) {
            int nx = currentPlayerXOnBoard + dx[i];
            int ny = currentPlayerYOnBoard + dy[i];
            if(nx < 0 || ny < 0 || nx >= stageBoardGridWidth || ny >= stageBoardGridHeight) break;
            if(currentStageBoardGrid[ny][nx] == 'x') break;
            if(currentStageBoardGrid[ny][nx] == '@') {
                snowFlag = true;
                snowX = nx;
                snowY = ny;
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
    }

    /// <summary>
    /// プレイヤー移動ボタン
    /// </summary>
    public void OnClickMoveButton(int i) {
        if(!(GameManager.Instance.CurrentGameState == GameState.Ready)) return;
        if(isTutorial) opeIndex++;
        GameManager.Instance.CurrentGameState = GameState.Playing;

        // ヒント状態の時は移動を制限
        if(hintMovesStack.Count > 0) {
            if(i == hintMovesStack.Peek()) {
                MovePlayer(i);
                AnimatorReset();
                if(turnCount < 999) turnCount++;
                AudioManager.Instance.PlaySE("Decision");
                playerAnim.SetBool(ANIMATOR_DIR[i], true);
                hintMovesStack.Dequeue();
                ButtonHintReset();
                if(hintMovesStack.Count > 0) {
                    LimitDPadAction(new int[]{hintMovesStack.Peek()});
                }
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

    /// <summary>
    /// ポーズボタン
    /// </summary>
    public void OnClickPauseButton() {
        if(!(GameManager.Instance.CurrentGameState == GameState.Ready)) return;
        AudioManager.Instance.PlaySE("Decision");
        GameManager.Instance.CurrentGameState = GameState.Pause;
        pausePanel.SetActive(true);
        Time.timeScale = 0.0f;
    }

    /// <summary>
    /// ステージセレクト画面遷移ボタン
    /// </summary>
    public void OnClickBackButton() {
        if(!(GameManager.Instance.CurrentGameState == GameState.Pause || GameManager.Instance.CurrentGameState == GameState.Clear)) return;
        AudioManager.Instance.PlaySE("Decision");
        Time.timeScale = 1.0f;
        // AdmobManager.Instance.DestroyDefaultBanner();
        FadeManager.Instance.LoadScene(0.5f, "StageSelect");
    }

    /// <summary>
    /// リトライボタン
    /// </summary>
    public void OnClickRetryButton() {
        if(!(GameManager.Instance.CurrentGameState == GameState.Pause || GameManager.Instance.CurrentGameState == GameState.Clear)) return;
        AudioManager.Instance.PlaySE("Decision");
        GameManager.Instance.CurrentGameState = GameState.Ready;
        Init();
        pausePanel.SetActive(false);
        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// 再開ボタン
    /// </summary>
    public void OnClickPlayButton() {
        if(!(GameManager.Instance.CurrentGameState == GameState.Pause)) return;
        AudioManager.Instance.PlaySE("Decision");
        GameManager.Instance.CurrentGameState = GameState.Ready;
        pausePanel.SetActive(false);
        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// ネクストステージ遷移ボタン
    /// </summary>
    public void OnClickNextButton() {
        if(!(GameManager.Instance.CurrentGameState == GameState.Clear)) return;
        AudioManager.Instance.PlaySE("Decision");
        GameManager.Instance.CurrentGameState = GameState.Ready;
        currentStageId = (currentStageId + 1) % stageNumber;
        Init();
    }

    /// <summary>
    /// ヒントボタン
    /// </summary>
    public void OnClickHintButton() {
        if(!(GameManager.Instance.CurrentGameState == GameState.Ready)) return;
        AudioManager.Instance.PlaySE("Decision");
        GameManager.Instance.CurrentGameState = GameState.Pause;
        hintPanel.SetActive(true);
        Time.timeScale = 0.0f;
    }

    private void ButtonHintReset() {
        for(int i = 0; i < 4; i++) {
            DPadButton[i].interactable = true;
        }
    }
    
    public void OnClickHintYesButton() {
        if(!(GameManager.Instance.CurrentGameState == GameState.Pause)) return;
        AudioManager.Instance.PlaySE("Decision");
        GameManager.Instance.CurrentGameState = GameState.Ready;
        hintPanel.SetActive(false);
        Time.timeScale = 1.0f;
        // AdmobManager.Instance.ShowDefaultInterstitial();
        
        Stack<int> HINT = new Solver().Solve(currentStageBoardGrid, currentPlayerXOnBoard, currentPlayerYOnBoard);
        hintMovesStack.Clear();
        for(int i = 0; i < 5 && HINT.Count > 0; i++) {
            hintMovesStack.Enqueue(HINT.Pop());
        }
        ButtonHintReset();
        if(hintMovesStack.Count == 0) return;
        LimitDPadAction(new int[]{hintMovesStack.Peek()});
    }

    public void OnClickHintNoButton() {
        if(!(GameManager.Instance.CurrentGameState == GameState.Pause)) return;
        AudioManager.Instance.PlaySE("Decision");
        GameManager.Instance.CurrentGameState = GameState.Ready;
        hintPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }


    // チュートリアル用

    int opeIndex = 0;
    static string message1 = "チュートリアルを始めます<>まずは雪原(白いマス)を\n移動してみましょう";
    static string message2 = "このように雪原は\n普通に歩くことができます<>次に氷の上(水色のマス)\nを移動してみましょう";
    static string message3 = "氷の上は滑ってしまい\n壁にぶつかるまで止まりません<>上手く操作してペンギンを\nゴール☆まで導いてください！";

    string[] ope = {message1, "R", "D", "U", message2, "R", "D", message3, "R", "U"};
    bool incrementFlag = true;
    public void Tutorial() {
        switch(ope[opeIndex]) {
            case "L":
                LimitDPadAction(new int[]{0});
                break;
            case "R":
                LimitDPadAction(new int[]{1});
                break;
            case "U":
                LimitDPadAction(new int[]{2});
                break;
            case "D":
                LimitDPadAction(new int[]{3});
                break;
            default:
                if(!incrementFlag) return;
                incrementFlag = false;
                messageLayer.GetComponent<Message>().SetMessagePanel(ope[opeIndex]);
                LimitDPadAction(new int[]{});
                break;
        }
    }

    public void IncrementTutorialIndex() {
        opeIndex++;
        incrementFlag = true;
    }

}
