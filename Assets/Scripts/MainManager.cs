using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class MainManager : MonoBehaviour
{
    // Left Right Up Down
    int[] dx = {-1, 1, 0, 0};
    int[] dy = {0, 0, 1, -1};
    float EPS = 1e-5f;

    private BannerView defaultBannerView;
    private BannerView pauseBannerView;
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
    


    Tilemap stageBoard;
    static int currentDifficulty = 0;
    const int difficultyNumber = 2;
    static int currentStageId = 0;
    const int stageNumber = 15;

    int startPointXOnBoard, startPointYOnBoard, goalPointXOnBoard, goalPointYOnBoard;
    float startPointX, startPointY, goalPointX, goalPointY;
    int currentPlayerXOnBoard, currentPlayerYOnBoard;
    float currentPlayerX, currentPlayerY;
    float stageBoardWidthCenter = 0.0f;
    float stageBoardHeightCenter = 0.5f;
    float stageBoardWidth, stageBoardHeight;
    float screenScale = 5.0f;
    char[][] stageBoardGrid, currentStageBoardGrid;

    // 基本的に長さは同じ
    int stageBoardGridWidth, stageBoardGridHeight;

    bool isFinish = false;
    bool reachedSnowBall = false;

    Vector3 targetPosition;
    Vector3 goalPosition;
    Vector3Int snowBallPosition;
    [SerializeField] float moveSpeed = 1.0f;
    bool clearFlag = false;


    bool fadeOutFlag = false;
    bool fadeInFlag = true;
    float fadeTimeCount = 1.0f;

    bool hintFlag = false;

    Stack<int> hintMovesStack = new Stack<int>();


    private void RequestDefaultBanner()
    {
        #if UNITY_IOS
            string adUnitId = AdmobVariable.GetIPHONE_DEFAULT_BANNER();
        #else
            string adUnitId = "unexpected_platform";
        #endif

        defaultBannerView = new BannerView(adUnitId, AdSize.IABBanner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();

        MobileAds.Initialize(initStatus => { });
        defaultBannerView.LoadAd(request);
    }

    private void RequestPauseBanner()
    {
        #if UNITY_IOS
            string adUnitId = AdmobVariable.GetIPHONE_PAUSE_BANNER();
        #else
            string adUnitId = "unexpected_platform";
        #endif

        pauseBannerView = new BannerView(adUnitId, AdSize.MediumRectangle, AdPosition.Center);
        AdRequest request = new AdRequest.Builder().Build();

        MobileAds.Initialize(initStatus => { });
        pauseBannerView.LoadAd(request);
    }

    private void RequestStageTransInterstitial()
    {
        // ★リリース時に自分のIDに変更する
        #if UNITY_IOS
            string adUnitId = AdmobVariableScript.GetIPHONE_STAGE_TRANS_INTERSTITIAL();
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

    // シーン遷移処理
    private void LoadNextScene()
    {
        SceneManager.LoadScene("StageSelect");
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
        LoadNextScene();
    }

    // 広告がデバイスの画面いっぱいに表示されたとき
    public void HandleOnAdOpened(object sender, System.EventArgs args)
    {
    }

    // 広告を閉じたとき
    public void HandleOnAdClosed(object sender, System.EventArgs args)
    {
        // 次のシーンに遷移
        // if (retryFlag == true) {
        //     retryFlag = false;
        //     RequestInterstitial();
        // } else {
            LoadNextScene();
        // }
    }
    
    // 別のアプリ（Google Play ストアなど）を起動した時
    public void HandleOnAdLeavingApplication(object sender, System.EventArgs args)
    {
    }





    void AnimatorReset() {
        playerAnim.SetBool("ToLeft", false);
        playerAnim.SetBool("ToRight", false);
        playerAnim.SetBool("ToUp", false);
        playerAnim.SetBool("ToDown", false);
    }

    void SetTileBoard(int x, int y, TileBase[] tempTile) {
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


    void Init() {

        Time.timeScale = 1.0f;
        clearFlag = false;
        hintFlag = false;

        // フェードイン
        fadeImage.SetActive(true);
        fadeInFlag = true;
        fadeOutFlag = false;


        // プレイヤーのアニメーションをリセット
        AnimatorReset();
        playerAnim.SetFloat("MovingSpeed", 0.0f);
        playerAnim.SetBool("ToDown", true);

        // 盤面のタイルマップをリセット
        Destroy(GameObject.Find("StageBoard(Clone)"));
        stageBoard = Instantiate(stageBoardPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity).GetComponent<Tilemap>();
        stageBoard.transform.SetParent(gridLayer.transform, false);

        // 盤面のサイズを取得
        stageBoardGrid = new Board().GetBoard(currentDifficulty, currentStageId);
        stageBoardGridWidth = stageBoardGrid[0].Length;
        stageBoardGridHeight = stageBoardGrid.Length;
        stageBoardWidth = screenScale/stageBoardGridWidth;
        stageBoardHeight = screenScale/stageBoardGridHeight;

        // 盤面のサイズを調整
        stageBoard.GetComponent<Transform>().position = new Vector3(-stageBoardGridWidth*stageBoardWidth/2.0f+stageBoardWidthCenter, -stageBoardGridHeight*stageBoardHeight/2.0f+stageBoardHeightCenter, 0.0f);
        stageBoard.GetComponent<Transform>().localScale = new Vector3(stageBoardWidth, stageBoardHeight, 0.0f);

        // 盤面の状態をコピー // ディープコピーの方法がわからない
        currentStageBoardGrid = new char[stageBoardGridHeight][];
        for(int i = 0; i < stageBoardGridHeight; i++) currentStageBoardGrid[i] = new char[stageBoardGridWidth];
        for(int i = 0; i < stageBoardGridHeight; i++) {
            for(int j = 0; j < stageBoardGridWidth; j++) {
                currentStageBoardGrid[i][j] = stageBoardGrid[i][j];
            }
        }


        // 盤面の状態を反映
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


        // プレイヤー位置の初期化
        player.transform.position = new Vector3(startPointX+stageBoardWidth/2.0f, startPointY+stageBoardHeight/2.0f, 0.0f);
        currentPlayerX = startPointX; currentPlayerXOnBoard = startPointXOnBoard;
        currentPlayerY = startPointY; currentPlayerYOnBoard = startPointYOnBoard;
        player.transform.localScale = new Vector3(stageBoardWidth, stageBoardHeight, 1.0f);
        targetPosition = new Vector3(currentPlayerX+stageBoardWidth/2.0f, currentPlayerY+stageBoardHeight/2.0f, 0.0f);
        goalPosition = new Vector3(goalPointX+stageBoardWidth/2.0f, goalPointY+stageBoardHeight/2.0f, 0.0f);

        // その他変数を初期化
        isFinish = false;
        reachedSnowBall = false;
        pausePanel.SetActive(false);
        clearPanel.SetActive(false);
    }

    void Start()
    {
        // 盤面のサイズを取得
        // 盤面のサイズを調整
        // 盤面の状態をコピー
        // 盤面の状態を取得
        // プレイヤー位置の初期化
        Init();
    }
    

    void Update()
    {
        if(fadeInFlag) {
            fadeTimeCount -= Time.deltaTime * 2;
            fadeImage.GetComponent<Image>().color = new Color((float)51.0f/255.0f, (float)51.0f/255.0f, (float)51.0f/255.0f, Mathf.Max(0.0f, fadeTimeCount));
            if(fadeTimeCount < 0.0f-EPS) {
                fadeTimeCount = 0.0f;
                fadeImage.SetActive(false);
                fadeInFlag = false;
                
                // 広告の生成
                RequestDefaultBanner();
                RequestStageTransInterstitial();
            }
            return;
        }
        if(fadeOutFlag) {
            Time.timeScale = 1.0f;
            fadeTimeCount += Time.deltaTime * 2;
            fadeImage.GetComponent<Image>().color = new Color((float)51.0f/255.0f, (float)51.0f/255.0f, (float)51.0f/255.0f, Mathf.Min(1.0f, fadeTimeCount));
            if (fadeTimeCount > 1.0f+EPS) {
                fadeTimeCount = 1.0f;
                if (stageTransInterstitialAd.IsLoaded()) {
                    stageTransInterstitialAd.Show();
                } else {
                    SceneManager.LoadScene("StageSelect");
                }
            }
            return;
        }
        if(isFinish && (player.transform.position - goalPosition).magnitude <= EPS) { //ゴールに着いている状態
            if(clearFlag == false) {
                playerAnim.SetFloat("MovingSpeed", 0.0f);
                AnimatorReset();
                playerAnim.SetBool("ToDown", true);
                RequestPauseBanner();
                clearPanel.SetActive(true);
                string stageName = "StageScore" + currentDifficulty.ToString() + "_" + currentStageId.ToString();
                string stageNameUnlock = "UnlockStage" + ((currentDifficulty + (currentStageId+1)/stageNumber) % difficultyNumber).ToString() + "_" + ((currentStageId+1) % stageNumber).ToString();
                PlayerPrefs.SetInt(stageName, 1);
                if((currentDifficulty + (currentStageId+1)/stageNumber) % difficultyNumber < 2) PlayerPrefs.SetInt(stageNameUnlock, 1);
                clearFlag = true;
            }
        }else if(reachedSnowBall && (player.transform.position - targetPosition).magnitude <= EPS) { // 雪玉に当たった状態
            playerAnim.SetFloat("MovingSpeed", 0.0f);
            reachedSnowBall = false;
            SetTileBoard(snowBallPosition.x, snowBallPosition.y, iceFloor);
        }else if((player.transform.position - targetPosition).magnitude <= EPS) { //目的地に着き、止まっている状態
            playerAnim.SetFloat("MovingSpeed", 0.0f);
            playerAnim.Play(playerAnim.GetCurrentAnimatorStateInfo(0).nameHash, 0, 0.0f);
        }else { //目的地に進んでいる状態
            if(stageBoardGrid[stageBoard.WorldToCell(player.transform.position).y][stageBoard.WorldToCell(player.transform.position).x] == '.') {
                playerAnim.SetFloat("MovingSpeed", 0.0f);
            }else {
                playerAnim.SetFloat("MovingSpeed", 1.0f);
            }
            player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            // Debug.Log((player.transform.position - targetPosition).magnitude.ToString("F15"));
            // Debug.Log((player.transform.position - goalPosition).magnitude.ToString("F15"));
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

        // もし到着場所がゴールならFinishフラグを立てる
        if(goalPointXOnBoard == currentPlayerXOnBoard && goalPointYOnBoard == currentPlayerYOnBoard) isFinish = true;
    }

    public void OnClickMoveButton(int i) {
        if((player.transform.position - targetPosition).magnitude > EPS) return;
        if(isFinish) return;
        if(reachedSnowBall) return;
        if(hintFlag) {
            if(i == hintMovesStack.Peek()) {
                MovePlayer(i);
                AnimatorReset();
                if(i == 0) {
                    playerAnim.SetBool("ToLeft", true);
                }else if(i == 1) {
                    playerAnim.SetBool("ToRight", true);
                }else if(i == 2) {
                    playerAnim.SetBool("ToUp", true);
                }else if(i == 3) {
                    playerAnim.SetBool("ToDown", true);
                }
                hintMovesStack.Pop();
                ButtonHintReset();
                if(hintMovesStack.Count > 0) DPadButton[hintMovesStack.Peek()].GetComponent<Image>().sprite = redButton[hintMovesStack.Peek()];
                else hintFlag = false;
            }else {
                return;
            }
        }else {
            MovePlayer(i);
            AnimatorReset();
            if(i == 0) {
                playerAnim.SetBool("ToLeft", true);
            }else if(i == 1) {
                playerAnim.SetBool("ToRight", true);
            }else if(i == 2) {
                playerAnim.SetBool("ToUp", true);
            }else if(i == 3) {
                playerAnim.SetBool("ToDown", true);
            }
        }
        
    }

    public void OnClickPauseButton() {
        if(isFinish) return;
        RequestPauseBanner();
        if (!pausePanel.activeSelf) {
            pausePanel.SetActive(true);
            Time.timeScale = 0.0f;
        }
    }

    public void OnClickBackButton() {
        fadeImage.SetActive(true);
        fadeOutFlag = true;
        defaultBannerView.Destroy();
        pauseBannerView.Destroy();
    }

    public void OnClickRetryButton() {
        Init();
        pausePanel.SetActive(false);
        pauseBannerView.Destroy();
        Time.timeScale = 1.0f;
    }

    public void OnClickPlayButton() {
        pausePanel.SetActive(false);
        pauseBannerView.Destroy();
        Time.timeScale = 1.0f;
    }

    public void OnClickHintButton() {
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
        hintPanel.SetActive(false);
        Time.timeScale = 1.0f;
        Init();
        hintFlag = true;
        int[] HINT = new Board().GetHint(currentDifficulty, currentStageId);
        hintMovesStack.Clear();
        for(int i = HINT.Length-1; i >= 0; i--) {
            hintMovesStack.Push(HINT[i]);
        }
        ButtonHintReset();
        DPadButton[hintMovesStack.Peek()].GetComponent<Image>().sprite = redButton[hintMovesStack.Peek()];
    }

    public void OnClickHintNoButton() {
        hintPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }



    public void OnClickNextButton() {
        currentDifficulty = (currentDifficulty + (currentStageId+1)/stageNumber) % difficultyNumber;
        currentStageId = (currentStageId+1) % stageNumber;
        Init();
        pauseBannerView.Destroy();
    }


    public void SetCurrentStageId(int argStageId) {
        currentStageId = argStageId;
    }

    public void SetCurrentDifficulty(int argDifficulty) {
        currentDifficulty = argDifficulty;
    }
}
