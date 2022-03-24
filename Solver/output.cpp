#include <bits/stdc++.h>
using namespace std;
typedef pair<int, int> pint;
#define rep(i, n) for(int i = 0; i < (int)n; i++)
int dx[4] = {-1, 1, 0, 0};
int dy[4] = {0, 0, -1, 1};

int main() {
  cout << "new char[][] {" << "\n";
  string S;
  while(cin >> S) {
    int N = S.size();
    cout << "    new char[] {";
    rep(i, N) cout << "'" << S[i] << "', ";
    cout << "}," << "\n";
  }
  cout << "}," << "\n";
}