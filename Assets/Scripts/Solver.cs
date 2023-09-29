using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solver
{
    int[] dx = {-1, 1, 0, 0};
    int[] dy = {0, 0, 1, -1};

    public Stack<int> Solve(char[][] argBoard, int sx, int sy)
    {
        int Height = argBoard.Length;
        int Width = argBoard[0].Length;
        char[,] board = new char[Height, Width];
        int[,] dist = new int[Height, Width];
        int[,] prevPointX = new int[Height, Width];
        int[,] prevPointY = new int[Height, Width];
        int[,] prevMove = new int[Height, Width];
        int gx = -1, gy = -1;
        for(int i = 0; i < Height; i++) {
            for(int j = 0; j < Width; j++) {
                board[i, j] = argBoard[i][j];
                dist[i, j] = -1;
                prevPointX[i, j] = -1;
                prevPointY[i, j] = -1;
                prevMove[i, j] = -1;
                if(board[i, j] == 'G') {
                    gx = j;
                    gy = i;
                }
            }
        }
        Queue<int> queX = new Queue<int>();
        Queue<int> queY = new Queue<int>();
        dist[sy, sx] = 0;
        queX.Enqueue(sx);
        queY.Enqueue(sy);
        while(queX.Count != 0) {
            int tempX = queX.Dequeue();
            int tempY = queY.Dequeue();
            for(int i = 0; i < 4; i++) {
                int x = tempX;
                int y = tempY;
                int nx = tempX;
                int ny = tempY;
                while(true) {
                    if(nx+dx[i] < 0 || ny+dy[i] < 0 || nx+dx[i] >= Width || ny+dy[i] >= Height) break;
                    if(board[ny+dy[i], nx+dx[i]] == '#' || board[ny+dy[i], nx+dx[i]] == 'x') break;
                    if(board[ny+dy[i], nx+dx[i]] == 'S' || board[ny+dy[i], nx+dx[i]] == 'G' || board[ny+dy[i], nx+dx[i]] == 'o') {
                        nx += dx[i];
                        ny += dy[i];
                        break;
                    }
                    nx += dx[i];
                    ny += dy[i];
                }
                if(dist[ny, nx] != -1) continue;
                dist[ny, nx] = dist[y, x]+1;
                prevPointX[ny, nx] = x;
                prevPointY[ny, nx] = y;
                prevMove[ny, nx] = i;
                queX.Enqueue(nx);
                queY.Enqueue(ny);
            }
        }
        Stack<int> movesStack = new Stack<int>();
        while(!(sx == gx && sy == gy)) {
            if(prevPointX[gy, gx] == -1 && prevPointY[gy, gx] == -1) {
                movesStack.Clear();
                break;
            }
            int tempX = gx;
            int tempY = gy;
            movesStack.Push(prevMove[tempY, tempX]);
            gx = prevPointX[tempY, tempX];
            gy = prevPointY[tempY, tempX];
        }
        return movesStack;
    }

    public int GetOptMoves(char[][] argBoard) {
        int Height = argBoard.Length;
        int Width = argBoard[0].Length;
        char[,] board = new char[Height, Width];
        int[,] dist = new int[Height, Width];
        int sx = -1, sy = -1, gx = -1, gy = -1;
        for(int i = 0; i < Height; i++) {
            for(int j = 0; j < Width; j++) {
                board[i, j] = argBoard[i][j];
                dist[i, j] = -1;
                if(board[i, j] == 'S') {
                    sx = j;
                    sy = i;
                }
                if(board[i, j] == 'G') {
                    gx = j;
                    gy = i;
                }
            }
        }
        Queue<int> queX = new Queue<int>();
        Queue<int> queY = new Queue<int>();
        dist[sy, sx] = 0;
        queX.Enqueue(sx);
        queY.Enqueue(sy);
        while(queX.Count != 0) {
            int tempX = queX.Dequeue();
            int tempY = queY.Dequeue();
            for(int i = 0; i < 4; i++) {
                int x = tempX;
                int y = tempY;
                int nx = tempX;
                int ny = tempY;
                while(true) {
                    if(nx+dx[i] < 0 || ny+dy[i] < 0 || nx+dx[i] >= Width || ny+dy[i] >= Height) break;
                    if(board[ny+dy[i], nx+dx[i]] == '#' || board[ny+dy[i], nx+dx[i]] == 'x') break;
                    if(board[ny+dy[i], nx+dx[i]] == 'S' || board[ny+dy[i], nx+dx[i]] == 'G' || board[ny+dy[i], nx+dx[i]] == 'o') {
                        nx += dx[i];
                        ny += dy[i];
                        break;
                    }
                    nx += dx[i];
                    ny += dy[i];
                }
                if(dist[ny, nx] != -1) continue;
                dist[ny, nx] = dist[y, x]+1;
                queX.Enqueue(nx);
                queY.Enqueue(ny);
            }
        }
        return dist[gy, gx];
    }
}
