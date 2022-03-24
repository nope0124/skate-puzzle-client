#include <bits/stdc++.h>
#include <random>
using namespace std;
typedef pair<int, int> pint;
#define rep(i, n) for(int i = 0; i < (int)n; i++)
int dx[4] = {-1, 1, 0, 0};
int dy[4] = {0, 0, -1, 1};

struct RANDOM {
    mt19937 mt;
    RANDOM() {
      random_device rd;
      mt = mt19937(rd());
    }
    long operator()(long a, long b) {
      uniform_int_distribution<long> dist(a, b - 1);
      return dist(mt);
    }
    long operator()(long a) {
      return (*this)((long)0, a);
    }
 
    void set_seed(long seed) {
      mt.seed(seed);
    }
    bool prob(double x) {
      uniform_real_distribution<double> dist(0.0, 1.0);
      return dist(mt) < x;
    }
} rnd;


pair<vector<string>, vector<vector<int> > > solve(int H, int W, int sx, int sy, int gx, int gy, string C) {
  vector<string> board(H);
  string S = "";
  rep(i, W) S += '.';
  rep(i, H) board[i] = S;
  rep(i, H) {
    rep(j, W) {
      board[i][j] = C[rnd(C.size())];
    }
  }
  board[sy][sx] = 'S';
  board[gy][gx] = 'G';
  queue<pair<pint, vector<string> > > que;
  vector<vector<int> > dist(H, vector<int>(W, -1));
  que.push(pair<pint, vector<string> >(pint(sx, sy), board));
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
      que.push(pair<pint, vector<string> >(pint(nx, ny), new_board));
    }
  }
  return pair<vector<string>, vector<vector<int> > >(board, dist);
}

void OUTPUT(vector<string> board) {
  int H = board.size();
  int W = board[0].size();
  cout << "new char[][] {" << "\n";
  rep(i, H) {
    cout << "    new char[] {";
    rep(j, W) cout << "'" << board[i][j] << "', ";
    cout << "}," << "\n";
  }
  cout << "}," << "\n";
}

int main() {
  int r; cin >> r;
  rnd.set_seed(r);
  string C = "........x";
  int H, W;
  int sx, sy, gx, gy;
  vector<string> board;
  vector<vector<int> > dist;
  while(1) {
    H = 15;
    W = H;
    sx = 0;
    sy = H-1;
    while(1) {
      gx = rnd(W);
      gy = rnd(H);
      if(!(sx == gx && sy == gy)) break; 
    }
    pair<vector<string>, vector<vector<int> > > p = solve(H, W, sx, sy, gx, gy, C);
    board = p.first;
    dist = p.second;
    int cnt = 0;
    rep(i, H) rep(j, W) if(board[i][j] == 'x') cnt++;
    if(dist[gy][gx] >= 21 && cnt <= 20) break;
  }
  OUTPUT(board);
  // rep(i, H) {
  //   rep(j, W) {
  //     if(dist[i][j] == -1) cout << "xx" << ' ';
  //     else if(dist[i][j] < 10) cout << ' ' << dist[i][j] << ' ';
  //     else cout << dist[i][j] << ' ';
  //   }
  //   cout << endl;
  // }
}