using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MainManager : MonoBehaviour
{
    [SerializeField] TileBase iceWall;
    [SerializeField] TileBase iceFloor;
    [SerializeField] TileBase iceBlock;
    // [SerializeField] TileBase iceBall;
    // [SerializeField] TileBase soilFloor;
    [SerializeField] Tilemap stageBoard;
    [SerializeField] Board boardScript;
    [SerializeField] GameObject player;
    int number = 0;
    int difficulty = 0;
    void Start()
    {
        char[][] board = boardScript.GetBoard(number);
        int boardY = board.Length;
        int boardX = board[0].Length;
        float boardScale = stageBoard.GetComponent<Transform>().localScale.x;
        player.transform.localScale = new Vector3(boardScale, boardScale, 1.0f);
        float startX = 0.0f;
        float startY = 0.0f;
        stageBoard.GetComponent<Transform>().position = new Vector3(-boardX*boardScale/2.0f, -boardY*boardScale/2.0f, 0.0f);
        for(int i = 0; i < boardY; i++) {
            for(int j = 0; j < boardX; j++) {
                Vector3Int grid = new Vector3Int(j, boardY-i-1, 0);
                switch(board[i][j]) {
                    case '#':
                        stageBoard.SetTile(grid, iceWall);
                        break;
                    case '.':
                        stageBoard.SetTile(grid, iceFloor);
                        break;
                    case 'x':
                        stageBoard.SetTile(grid, iceBlock);
                        break;
                    case 'S':
                        startX = stageBoard.CellToWorld(grid).x;
                        startY = stageBoard.CellToWorld(grid).y;
                        break;
                    default:
                        break;
                }
            }
        }
        player.transform.position = new Vector3(startX+boardScale/2.0f, startY+boardScale/2.0f, 0.0f);

    }

    void Update()
    {
        
    }
    
}
