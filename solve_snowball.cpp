#include <bits/stdc++.h>
using namespace std;
typedef pair<int, int> pint;
#define rep(i, n) for(int i = 0; i < (int)n; i++)
int dx[4] = {-1, 1, 0, 0};
int dy[4] = {0, 0, -1, 1};

int main() {
  int H, W; cin >> H >> W;
  vector<string> board(H);
  rep(i, H) cin >> board[i];
  int sx, sy, gx, gy;
  rep(i, H) {
    rep(j, W) {
      if(board[i][j] == 'S') sx = j, sy = i;
      else if(board[i][j] == 'G') gx = j, gy = i;
    }
  }
  queue<pair<pint, vector<string>>> que;
  vector<vector<int>> dist(H, vector<int>(W, -1));
  que.push(pair<pint, vector<string>>(pint(sx, sy), board));
  while(!que.empty()) {
    auto c = que.front(); que.pop();
    vector<string> new_board = c.second;
    rep(i, 4) {
      int x = c.first.first;
      int y = c.first.second;
      int nx = c.first.first;
      int ny = c.first.second;
      while(1) {
        if(nx+dx[i] < 0 || ny+dy[i] < 0 || nx+dx[i] >= W || ny+dy[i] >= H) break;
        if(new_board[ny+dy[i]][nx+dx[i]] == '#' || new_board[ny+dy[i]][nx+dx[i]] == 'x') break;
        if(new_board[ny+dy[i]][nx+dx[i]] == '@') {
          new_board[ny+dy[i]][nx+dx[i]] = '.';
          break;
        }
        if(new_board[ny+dy[i]][nx+dx[i]] == 'S' || new_board[ny+dy[i]][nx+dx[i]] == 'G' || new_board[ny+dy[i]][nx+dx[i]] == 'o') {
          nx += dx[i];
          ny += dy[i];
          break;
        }
        nx += dx[i];
        ny += dy[i];
      }
      if(dist[ny][nx] != -1) continue;
      dist[ny][nx] = dist[y][x]+1;
      que.push(pair<pint, vector<string>>(pint(nx, ny), new_board));
    }
  }
  rep(i, H) {
    rep(j, W) {
      if(dist[i][j] == -1) cout << 'x' << ' ';
      else cout << dist[i][j] << ' ';
    }
    cout << endl;
  }
}