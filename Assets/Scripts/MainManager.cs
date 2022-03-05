using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MainManager : MonoBehaviour
{
    // Left Right Up Down
    int[] dx = {-1, 1, 0, 0};
    int[] dy = {0, 0, 1, -1};
    float EPS = 1e-10f;

    [SerializeField] TileBase iceWall;
    [SerializeField] TileBase iceFloor;
    [SerializeField] TileBase iceBlock;
    [SerializeField] TileBase snowBall;
    // [SerializeField] TileBase soilFloor;
    [SerializeField] Tilemap stageBoard;
    [SerializeField] Board boardScript;
    [SerializeField] GameObject player;
    int number = 0;
    // int difficulty = 0;
    int startXOnBoard, startYOnBoard, goalXOnBoard, goalYOnBoard;
    float startX, startY, goalX, goalY;
    int currentPlayerXOnBoard, currentPlayerYOnBoard;
    float currentPlayerX, currentPlayerY;
    float boardScale;
    char[][] boardArray;
    char[][] currentBoardArray;

    int boardSizeX, boardSizeY;

    Vector3 targetPosition;
    [SerializeField] float moveSpeed = 1.0f;

    void init() {

        // 盤面の状態をコピー // ディープコピーの方法がわからない
        currentBoardArray = new char[boardSizeY][];
        for(int i = 0; i < boardSizeY; i++) currentBoardArray[i] = new char[boardSizeX];
        for(int i = 0; i < boardSizeY; i++) {
            for(int j = 0; j < boardSizeX; j++) {
                currentBoardArray[i][j] = boardArray[i][j];
            }
        }

        // 盤面の状態を反映
        for(int i = 0; i < boardSizeY; i++) {
            for(int j = 0; j < boardSizeX; j++) {
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
    }

    void Start()
    {
        // 盤面のサイズを取得
        boardArray = boardScript.GetBoard(number);
        boardSizeX = boardArray[0].Length;
        boardSizeY = boardArray.Length;
        boardScale = stageBoard.GetComponent<Transform>().localScale.x;

        // 盤面のサイズを調整
        stageBoard.GetComponent<Transform>().position = new Vector3(-boardSizeX*boardScale/2.0f, -boardSizeY*boardScale/2.0f, 0.0f);

        // 盤面の状態を取得
        // プレイヤー位置の初期化
        init();
        
    }

    void Update()
    {
        // 目的地に進む
        player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, moveSpeed * Time.deltaTime);
        // Debug.Log((player.transform.position - targetPosition).magnitude.ToString("F15"));
    }


    private void Move(int i)  {
        while(true) {
            if(currentPlayerXOnBoard + dx[i] < 0 || currentPlayerYOnBoard + dy[i] < 0 || currentPlayerXOnBoard + dx[i] >= boardSizeX || currentPlayerYOnBoard + dy[i] >= boardSizeY) break;
            if(currentBoardArray[currentPlayerYOnBoard+dy[i]][currentPlayerXOnBoard+dx[i]] == '#' || currentBoardArray[currentPlayerYOnBoard+dy[i]][currentPlayerXOnBoard+dx[i]] == 'x') break;
            if(currentBoardArray[currentPlayerYOnBoard+dy[i]][currentPlayerXOnBoard+dx[i]] == '@') {
                currentBoardArray[currentPlayerYOnBoard+dy[i]][currentPlayerXOnBoard+dx[i]] = '.';
                break;
            }
            if(currentBoardArray[currentPlayerYOnBoard+dy[i]][currentPlayerXOnBoard+dx[i]] == 'S' || currentBoardArray[currentPlayerYOnBoard+dy[i]][currentPlayerXOnBoard+dx[i]] == 'G' || currentBoardArray[currentPlayerYOnBoard+dy[i]][currentPlayerXOnBoard+dx[i]] == 'o') {
                currentPlayerXOnBoard += dx[i];
                currentPlayerYOnBoard += dy[i];
                break;
            }
            currentPlayerXOnBoard += dx[i];
            currentPlayerYOnBoard += dy[i];
        }
        Vector3Int grid = new Vector3Int(currentPlayerXOnBoard, currentPlayerYOnBoard, 0);
        currentPlayerX = stageBoard.CellToWorld(grid).x;
        currentPlayerY = stageBoard.CellToWorld(grid).y;
        targetPosition = new Vector3(currentPlayerX+boardScale/2.0f, currentPlayerY+boardScale/2.0f, 0.0f);
    }

    public void OnClickLeftButton() {
        if((player.transform.position - targetPosition).magnitude <= EPS) {
            Move(0);
        }
    }
    public void OnClickRightButton() {
        if((player.transform.position - targetPosition).magnitude <= EPS) {
            Move(1);
        }
    }
    public void OnClickUpButton() {
        if((player.transform.position - targetPosition).magnitude <= EPS) {
            Move(2);
        }
    }
    public void OnClickDownButton() {
        if((player.transform.position - targetPosition).magnitude <= EPS) {
            Move(3);
        }
    }

    public void OnClickResetButton() {
        init();
    }
}
