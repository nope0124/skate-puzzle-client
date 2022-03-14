using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    // Left Right Up Down
    int[] dx = {-1, 1, 0, 0};
    int[] dy = {0, 0, 1, -1};
    float EPS = 1e-5f;

    [SerializeField] TileBase iceWall;
    [SerializeField] TileBase iceFloor;
    [SerializeField] TileBase iceBlock;
    [SerializeField] TileBase snowBall;
    // [SerializeField] TileBase soilFloor;
    [SerializeField] TileBase goalFloor;
    [SerializeField] Tilemap stageBoard;
    [SerializeField] GameObject player;
    [SerializeField] GameObject clearUI;
    static int stageNumber;
    static int difficulty;
    int startXOnBoard, startYOnBoard, goalXOnBoard, goalYOnBoard;
    float startX, startY, goalX, goalY;
    int currentPlayerXOnBoard, currentPlayerYOnBoard;
    float currentPlayerX, currentPlayerY;
    float boardScale;
    float screenSize = 5.0f;
    char[][] boardArray;
    char[][] currentBoardArray;

    // 基本的に長さは同じ
    int boardLengthX, boardLengthY;

    bool isFinish = false;
    bool reachedSnowBall = false;

    Vector3 targetPosition;
    Vector3 goalPosition;
    Vector3Int snowBallPosition;
    [SerializeField] float moveSpeed = 1.0f;

    void init() {

        // 盤面の状態をコピー // ディープコピーの方法がわからない
        currentBoardArray = new char[boardLengthY][];
        for(int i = 0; i < boardLengthY; i++) currentBoardArray[i] = new char[boardLengthX];
        for(int i = 0; i < boardLengthY; i++) {
            for(int j = 0; j < boardLengthX; j++) {
                currentBoardArray[i][j] = boardArray[i][j];
            }
        }


        // 盤面の状態を反映
        for(int i = 0; i < boardLengthY; i++) {
            for(int j = 0; j < boardLengthX; j++) {
                Vector3Int grid = new Vector3Int(j, i, 0);
                switch(boardArray[i][j]) {
                    case '#':
                        stageBoard.SetTile(grid, iceWall);
                        break;
                    case '.':
                        stageBoard.SetTile(grid, iceFloor);
                        break;
                    case 'x':
                        stageBoard.SetTile(grid, iceBlock);
                        break;
                    case '@':
                        stageBoard.SetTile(grid, snowBall);
                        break;
                    case 'S':
                        startX = stageBoard.CellToWorld(grid).x;
                        startY = stageBoard.CellToWorld(grid).y;
                        startXOnBoard = j;
                        startYOnBoard = i;
                        break;
                    case 'G':
                        stageBoard.SetTile(grid, goalFloor);
                        goalX = stageBoard.CellToWorld(grid).x;
                        goalY = stageBoard.CellToWorld(grid).y;
                        goalXOnBoard = j;
                        goalYOnBoard = i;
                        break;
                    default:
                        break;
                }
            }
        }


        // プレイヤー位置の初期化
        player.transform.position = new Vector3(startX+boardScale/2.0f, startY+boardScale/2.0f, 0.0f);
        currentPlayerX = startX; currentPlayerXOnBoard = startXOnBoard;
        currentPlayerY = startY; currentPlayerYOnBoard = startYOnBoard;
        player.transform.localScale = new Vector3(boardScale, boardScale, 1.0f);
        targetPosition = new Vector3(currentPlayerX+boardScale/2.0f, currentPlayerY+boardScale/2.0f, 0.0f);
        goalPosition = new Vector3(goalX+boardScale/2.0f, goalY+boardScale/2.0f, 0.0f);

        // その他変数を初期化
        isFinish = false;
        reachedSnowBall = false;
        clearUI.SetActive(false);
    }

    void Start()
    {
        // 盤面のサイズを取得
        boardArray = new Board().GetBoard(difficulty, stageNumber);
        boardLengthX = boardArray[0].Length;
        boardLengthY = boardArray.Length;
        boardScale = screenSize/boardLengthX;

        // 盤面のサイズを調整
        stageBoard.GetComponent<Transform>().position = new Vector3(-boardLengthX*boardScale/2.0f, -boardLengthY*boardScale/2.0f, 0.0f);
        stageBoard.GetComponent<Transform>().localScale = new Vector3(boardScale, boardScale, 0.0f);

        // 盤面の状態をコピー
        // 盤面の状態を取得
        // プレイヤー位置の初期化
        init();
    }


    void Update()
    {


        if(isFinish && (player.transform.position - goalPosition).magnitude <= EPS) {
            clearUI.SetActive(true);
            string stageName = "StageScore" + difficulty.ToString() + "_" + stageNumber.ToString();
            PlayerPrefs.SetInt(stageName, 1);
        }else if(reachedSnowBall && (player.transform.position - targetPosition).magnitude <= EPS) {
            reachedSnowBall = false;
            stageBoard.SetTile(snowBallPosition, iceFloor);
        }else {
            // 目的地に進む
            player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            // Debug.Log((player.transform.position - targetPosition).magnitude.ToString("F15"));
            // Debug.Log((player.transform.position - goalPosition).magnitude.ToString("F15"));
        }
        
    }


    private void movePlayer(int i)  {

        // 次の到着場所を事前に計算
        while(true) {
            int nx = currentPlayerXOnBoard + dx[i];
            int ny = currentPlayerYOnBoard + dy[i];
            if(nx < 0 || ny < 0 || nx >= boardLengthX || ny >= boardLengthY) break;
            if(currentBoardArray[ny][nx] == '#' || currentBoardArray[ny][nx] == 'x') break;
            if(currentBoardArray[ny][nx] == '@') {
                currentBoardArray[ny][nx] = '.';
                snowBallPosition = new Vector3Int(nx, ny, 0);
                reachedSnowBall = true;
                break;
            }
            if(currentBoardArray[ny][nx] == 'S' || currentBoardArray[ny][nx] == 'G' || currentBoardArray[ny][nx] == 'o') {
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
        targetPosition = new Vector3(currentPlayerX+boardScale/2.0f, currentPlayerY+boardScale/2.0f, 0.0f);

        // もし到着場所がゴールならFinishフラグを立てる
        if(goalXOnBoard == currentPlayerXOnBoard && goalYOnBoard == currentPlayerYOnBoard) isFinish = true;
    }

    public void OnClickLeftButton() {
        if((player.transform.position - targetPosition).magnitude > EPS) return;
        if(isFinish) return;
        if(reachedSnowBall) return;
        movePlayer(0);
    }

    public void OnClickRightButton() {
        if((player.transform.position - targetPosition).magnitude > EPS) return;
        if(isFinish) return;
        if(reachedSnowBall) return;
        movePlayer(1);
    }

    public void OnClickUpButton() {
        if((player.transform.position - targetPosition).magnitude > EPS) return;
        if(isFinish) return;
        if(reachedSnowBall) return;
        movePlayer(2);
    }

    public void OnClickDownButton() {
        if((player.transform.position - targetPosition).magnitude > EPS) return;
        if(isFinish) return;
        if(reachedSnowBall) return;
        movePlayer(3);
    }

    public void OnClickResetButton() {
        init();
    }

    public void OnClickBackButton() {
        SceneManager.LoadScene("StageSelect");
    }

    public void SetStageNumber(int argStageNumber) {
        stageNumber = argStageNumber;
    }

    public void SetDifficulty(int argDifficulty) {
        difficulty = argDifficulty;
    }
}
