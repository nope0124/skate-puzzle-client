using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{

    enum TileState {
        break1,
        break2,
        break3,
        break4,
        break5
    }

    [SerializeField] public TileBase[] snowBallCollapse1;
    [SerializeField] public TileBase[] snowBallCollapse2;
    [SerializeField] public TileBase[] snowBallCollapse3;
    [SerializeField] public TileBase[] iceFloor;

    TileState currentTileState = TileState.break1;

    void Start()
    {
        
    }

    public float span = 0.08f;
    private float currentTime = 0f;

    void Update () {
        currentTime += Time.deltaTime;

        if(currentTime > span){
            currentTime = 0f;

            switch(currentTileState) {
                case TileState.break1:
                    currentTileState = TileState.break2;
                    MainManager.Instance.SetTileBoardFrom(x, y, "SnowBallCollapse1");
                    break;
                case TileState.break2:
                    currentTileState = TileState.break3;
                    MainManager.Instance.SetTileBoardFrom(x, y, "SnowBallCollapse2");
                    break;
                case TileState.break3:
                    currentTileState = TileState.break4;
                    MainManager.Instance.SetTileBoardFrom(x, y, "SnowBallCollapse3");
                    break;
                case TileState.break4:
                    currentTileState = TileState.break5;
                    MainManager.Instance.SetTileBoardFrom(x, y, "IceFloor");
                    Destroy(gameObject);
                    break;
            }
        }
    }

    private int x = -1;
    private int y = -1;
    public int X {
        get { return x; }
        set { x = value; }
    }

    public int Y {
        get { return y; }
        set { y = value; }
    }
}
