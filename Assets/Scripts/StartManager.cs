using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Database;


/// <summary>
/// スタート画面を扱うクラス
/// </summary>

public class StartManager : MonoBehaviour
{
    float EPS = 1e-5f;

    static int difficultyNumber = 2;
    static int stageNumber = 15;

    [SerializeField] Button idButton;
    [SerializeField] Text idButtonText;
    [SerializeField] GameObject dataResetPanel;
    [SerializeField] GameObject dataResetFinishPanel;
    [SerializeField] GameObject Test;

    [Serializable]
    class UserSaveData : ISerializationCallbackReceiver
    {
        public List<int> easy;
        public List<int> normal;
        public List<int> hard;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
        }
    }



    FirebaseAuth _auth;
    FirebaseUser _user;
    public FirebaseUser UserData { get { return _user; } }
    public delegate void CreateUser(bool result);

    private bool userLoginFlag = false;
    private string userId;
    

    void Awake()
    {
        // 初期化
        _auth = FirebaseAuth.DefaultInstance;
        // すでにユーザーが作られているのか確認
        if (_auth.CurrentUser == null)
        {
            // まだユーザーができていないためユーザー作成
            Create((result) =>
            {
                if (result)
                {
                    userLoginFlag = true;
                    userId = _user.UserId;
                    Debug.Log($"成功: #{userId}");
                }
                else
                {
                    Debug.Log("失敗");
                }
            });
        }
        else
        {
            _user = _auth.CurrentUser;
            userId = _user.UserId;
            userLoginFlag = true;
            Debug.Log($"ログイン中: #{_user.UserId}");
        }

        StartCoroutine(GetData());
    }

    private string baseURL = "http://localhost:3000";

    IEnumerator GetData()
    {
        string url = baseURL + "/stages/1";
        UnityWebRequest request = UnityWebRequest.Get(url);
        Debug.Log("呼び出し中");
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if(request.responseCode == 200)
            {
                Debug.Log("OK");
                Test.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 匿名でユーザー作成
    /// </summary>
    public void Create(CreateUser callback)
    {
        _auth.SignInAnonymouslyAsync().ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                callback(false);
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                callback(false);
                return;
            }

            _user = task.Result;
            Debug.Log($"User signed in successfully: {_user.DisplayName} ({_user.UserId})");
            callback(true);
        });
    }



    void Start()
    {
        // バナー広告呼び出し
        AdBannerManager.Instance.RequestDefaultBanner();

        // BGMの設定
        AudioManager.Instance.SetBGMAudioClip(null);
        if(PlayerPrefs.GetInt("Version1_1", 0) == 0) ChangePlayerPrefs();
        // if(PlayerPrefs.GetInt("v1_2", 0) == 0) ChangePlayerPrefs1_2();
    }

    void Update() {
        if(userLoginFlag == true && PlayerPrefs.GetString("user_id") == "") {
            userLoginFlag = false;
            var saveData = new UserSaveData()
            {
                easy   = new List<int>(){ 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                normal = new List<int>(){-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                hard   = new List<int>(){-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            };
            
            for(int tempDifficulty = 0; tempDifficulty < difficultyNumber; tempDifficulty++) {
                for(int tempStageId = 0; tempStageId < stageNumber; tempStageId++) {
                    string stageName = "StageScore"+tempDifficulty.ToString()+"_"+tempStageId.ToString();
                    if(tempDifficulty == 0) saveData.easy[tempStageId] = Mathf.Max(saveData.easy[tempStageId], PlayerPrefs.GetInt(stageName, -1));
                    else if(tempDifficulty == 1) saveData.normal[tempStageId] = Mathf.Max(saveData.normal[tempStageId], PlayerPrefs.GetInt(stageName, -1));
                    else if(tempDifficulty == 2) saveData.hard[tempStageId] = Mathf.Max(saveData.hard[tempStageId], PlayerPrefs.GetInt(stageName, -1));
                }
            }

            FirebaseDatabase.GetInstance(Const.CO.DATABASE_URL); // データベースのURLを設定
            DatabaseReference databaseRoot = FirebaseDatabase.DefaultInstance.RootReference; // ルートを作成
            // 新規ユーザーIDを作成し、それをPlayerPrefsに保存
            // string userId = databaseRoot.Child("users").Push().Key;
            PlayerPrefs.SetString("user_id", userId);
            DatabaseReference scoreReference = databaseRoot.Child("users").Child(userId).Child("scores");

            string data = JsonUtility.ToJson(saveData);
            scoreReference.SetRawJsonValueAsync(data);
        }
        // _auth.SignOut();
        // PlayerPrefs.DeleteAll();

    }

    /// <summary>
    /// スタートボタン
    /// </summary>
    public void OnClickStartButton()
    {
        AudioManager.Instance.PlaySE("Decision");
        AdBannerManager.Instance.DestroyDefaultBanner();
        FadeManager.Instance.LoadScene(0.5f, "StageSelect");
    }

    
    /// <summary>
    /// データリセットボタン
    /// </summary>
    public void OnClickDataResetButton() {
        AudioManager.Instance.PlaySE("Decision");
        if (!dataResetPanel.activeSelf) {
            dataResetPanel.SetActive(true);
        }
    }

    /// <summary>
    /// データリセットボタンでYes
    /// </summary>
    public void OnClickDataResetYesButton() {
        AudioManager.Instance.PlaySE("Decision");
        for(int tempDifficulty = 0; tempDifficulty < difficultyNumber; tempDifficulty++) {
            for(int tempStageId = 0; tempStageId < stageNumber; tempStageId++) {
                string stageName = "StageScore" + tempDifficulty.ToString() + "_" + tempStageId.ToString();
                PlayerPrefs.SetInt(stageName, -1);
            }
        }
        PlayerPrefs.SetInt("StageScore0_0", 0);
        dataResetPanel.SetActive(false);
        dataResetFinishPanel.SetActive(true);

        string userId = PlayerPrefs.GetString("user_id");
        if(userId != "") {
            FirebaseDatabase.GetInstance(Const.CO.DATABASE_URL); // データベースのURLを設定
            // FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Const.CO.DATABASE_URL); // データベースのURLを設定
            DatabaseReference databaseRoot = FirebaseDatabase.DefaultInstance.RootReference; // ルートを作成
            
            DatabaseReference scoreReference = databaseRoot.Child("users").Child(userId).Child("scores");

            var saveData = new UserSaveData()
            {
                easy   = new List<int>(){ 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                normal = new List<int>(){-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                hard   = new List<int>(){-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            };

            string data = JsonUtility.ToJson(saveData);
            scoreReference.SetRawJsonValueAsync(data);
        }
    }
    
    /// <summary>
    /// データリセットボタンでNo
    /// </summary>
    public void OnClickDataResetNoButton() {
        AudioManager.Instance.PlaySE("Decision");
        dataResetPanel.SetActive(false);
    }

    /// <summary>
    /// データリセット完了
    /// </summary>
    public void OnClickDataResetFinishButton() {
        AudioManager.Instance.PlaySE("Decision");
        dataResetFinishPanel.SetActive(false);
    }

    /// <summary>
    /// ユーザIDを表示
    /// </summary>
    public void IDDisplayButton() {
        idButton.interactable = false;
        idButtonText.text = "ID:"+PlayerPrefs.GetString("user_id");
    }


    /// <summary>
    /// version1.1アップデートのPlayerPrefsの修正
    /// </summary>
    private void ChangePlayerPrefs() {
        PlayerPrefs.SetInt("Version1_1", 1);

        List<int> _easy   = new List<int>(){ 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1};
        List<int> _normal = new List<int>(){-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1};
        List<int> _hard   = new List<int>(){-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1};

        for(int tempDifficulty = 0; tempDifficulty < difficultyNumber; tempDifficulty++) {
            for(int tempStageId = 0; tempStageId < stageNumber; tempStageId++) {
                string stageNameUnlock = "UnlockStage"+tempDifficulty.ToString()+"_"+tempStageId.ToString();
                if(PlayerPrefs.GetInt(stageNameUnlock, 0) == 0) {
                    if(tempDifficulty == 0) _easy[tempStageId] = -1;
                    else if(tempDifficulty == 1) _normal[tempStageId] = -1;
                    else if(tempDifficulty == 2) _hard[tempStageId] = -1;
                    PlayerPrefs.DeleteKey(stageNameUnlock);
                }else {
                    if(tempDifficulty == 0) _easy[tempStageId] = 0;
                    else if(tempDifficulty == 1) _normal[tempStageId] = 0;
                    else if(tempDifficulty == 2) _hard[tempStageId] = 0;
                    PlayerPrefs.DeleteKey(stageNameUnlock);
                }
            }
        }

        for(int tempDifficulty = 0; tempDifficulty < difficultyNumber; tempDifficulty++) {
            for(int tempStageId = 0; tempStageId < stageNumber; tempStageId++) {
                string stageName = "StageScore"+tempDifficulty.ToString()+"_"+tempStageId.ToString();
                if(tempDifficulty == 0) _easy[tempStageId] = Mathf.Max(_easy[tempStageId], PlayerPrefs.GetInt(stageName, -1));
                else if(tempDifficulty == 1) _normal[tempStageId] = Mathf.Max(_normal[tempStageId], PlayerPrefs.GetInt(stageName, -1));
                else if(tempDifficulty == 2) _hard[tempStageId] = Mathf.Max(_hard[tempStageId], PlayerPrefs.GetInt(stageName, -1));
                PlayerPrefs.DeleteKey(stageName);
            }
        }

        _easy[0] = Mathf.Max(0, _easy[0]);

        for(int tempDifficulty = 0; tempDifficulty < difficultyNumber; tempDifficulty++) {
            for(int tempStageId = 0; tempStageId < stageNumber; tempStageId++) {
                string stageName = "StageScore"+tempDifficulty.ToString()+"_"+tempStageId.ToString();
                if(tempDifficulty == 0) PlayerPrefs.SetInt(stageName, _easy[tempStageId]);
                else if(tempDifficulty == 1) PlayerPrefs.SetInt(stageName, _normal[tempStageId]);
                else if(tempDifficulty == 2) PlayerPrefs.SetInt(stageName, _hard[tempStageId]);
            }
        }
    }
}
