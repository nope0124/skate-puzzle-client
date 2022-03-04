using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // IceWall   -> #
    // IceFloor  -> .
    // IceBlock  -> x
    // IceBall   -> @
    // SoilFloor -> o
    // Start     -> S
    // Goal      -> G

    // 15ステージ
    private char[][][] EASY_BOARD = new char[][][] {
        new char[][] { 
            new char[] {'#', '#', '#', '#', '#', '#'},
            new char[] {'#', 'S', '.', '.', 'x', '#'},
            new char[] {'#', '.', '.', '.', '.', '#'},
            new char[] {'#', '.', '.', 'G', '.', '#'},
            new char[] {'#', '.', '.', '.', '.', '#'},
            new char[] {'#', '.', '.', '.', 'x', '#'},
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
}
