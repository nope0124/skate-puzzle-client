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
  board[sy-1][sx] = '.';
  board[sy][sx+1] = '.';
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
  rep(i, H) cout << board[i] << "\n";
  cout << "\n";
  cout << "new char[][] {" << "\n";
  rep(i, H) {
    cout << "    new char[] {";
    rep(j, W) cout << "'" << board[i][j] << "', ";
    cout << "}," << "\n";
  }
  cout << "}," << "\n";
}

vector<vector<int> > OUTPUT_DIST(int H, int W, int sx, int sy, int gx, int gy, vector<string> board) {
  
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
  return dist;
}

int random_walk(vector<string> board) {
  int H = board.size();
  int W = board[0].size();
  int sx, sy, gx, gy;
  rep(i, H) {
    rep(j, W) {
      if(board[i][j] == 'S') sx = j, sy = i;
      else if(board[i][j] == 'G') gx = j, gy = i;
    }
  }
  for(int itr = 0; ; itr++) {
    int dir = rnd(4);
    int x = sx;
    int y = sy;
    int nx = sx;
    int ny = sy;
    while(1) {
      if(nx+dx[dir] < 0 || ny+dy[dir] < 0 || nx+dx[dir] >= W || ny+dy[dir] >= H) break;
      if(board[ny+dy[dir]][nx+dx[dir]] == 'x') break;
      if(board[ny+dy[dir]][nx+dx[dir]] == 'S' || board[ny+dy[dir]][nx+dx[dir]] == 'G' || board[ny+dy[dir]][nx+dx[dir]] == 'o') {
        nx += dx[dir];
        ny += dy[dir];
        break;
      }
      nx += dx[dir];
      ny += dy[dir];
    }
    sx = nx;
    sy = ny;
    if(sx == gx && sy == gy) return itr;
    if(itr >= 100000) return itr;
  }
}


int ave_random_walk(vector<string> board, int num=100) {
  int sum = 0;
  rep(i, num) {
    int tmp = random_walk(board);
    if(tmp == 100000) return -1;
    else sum += tmp;
  }
  return sum/num;
}

int main() {
  int r = rnd(1000000);
  cout << "Seed=" << r << "\n";
  rnd.set_seed(r);
  string C = "....................xx";
  int H, W;
  int sx, sy, gx, gy;
  vector<string> board;
  vector<vector<int> > dist;
  while(1) {
    H = 13;
    W = H;
    sx = 0;
    sy = H-1;
    while(1) {
      gx = rnd(W);
      gy = rnd(H);
      if(!(sx == gx && sy == gy)) break; 
    }
    pair<vector<string>, vector<vector<int> > > p = solve(H, W, sx, sy, gx, gy, C);
    // solve();
    board = p.first;
    dist = p.second;
    int cnt = 0;
    rep(i, H) rep(j, W) if(board[i][j] == 'x' || board[i][j] == 'o') cnt++;
    // if(dist[gy][gx] >= 10 && cnt <= 20) break;
    if(dist[gy][gx] <= 1 || cnt >= H*2) continue;
    // if(ave_random_walk(board) >= 1000) {
      rep(i, H) {
        rep(j, W) {
          cout << dist[i][j] << ' ';
        }
        cout << "\n";
      }
      cout << "\n";
    //   break;
    // }
    break;
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
  cout << random_walk(board) << endl;
  cin >> H >> W;
  vector<string> a(H);
  rep(i, H) cin >> a[i];
  rep(i, H) {
    rep(j, W) {
      if(a[i][j] == 'S') sx = j, sy = i;
      else if(a[i][j] == 'G') gx = j, gy = i;
    }
  }
  cout << ave_random_walk(a) << endl;
  dist = OUTPUT_DIST(H, W, sx, sy, gx, gy, a);
  cout << "\n";
  rep(i, H) {
    rep(j, W) {
      if(dist[i][j] == -1) cout << "x ";
      else cout << dist[i][j] << ' ';
    }
    cout << "\n";
  }
  cout << "\n";
}