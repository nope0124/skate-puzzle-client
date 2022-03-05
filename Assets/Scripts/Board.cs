using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // IceWall   -> #
    // IceFloor  -> .
    // IceBlock  -> x
    // SnoqBall  -> @
    // SoilFloor -> o
    // Start     -> S
    // Goal      -> G

    // 15ステージ
    private readonly char[][][] EASY_BOARD = new char[][][] {
        new char[][] { 
            new char[] {'#', '#', '#', '#', '#', '#'},
            new char[] {'#', '.', '.', '.', 'x', '#'},
            new char[] {'#', '@', '.', '.', '.', '#'},
            new char[] {'#', '.', '.', 'G', '.', '#'},
            new char[] {'#', '.', '.', '.', '.', '#'},
            new char[] {'#', 'S', '.', '.', 'x', '#'},
            new char[] {'#', '#', '#', '#', '#', '#'},
        },
    };

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public char[][] GetBoard(int num){
        return EASY_BOARD[num];
    }

    public void display(int num) {
        for(int i = 0; i < EASY_BOARD[num].Length; i++) {
            for(int j = 0; j < EASY_BOARD[num][0].Length; j++) {
                Debug.Log(EASY_BOARD[num][i][j]);
            }
        }
    }
}
