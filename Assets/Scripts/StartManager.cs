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

    static string uid = "";
    static string access_token = "";
    static string client = "";


    public string Uid
    {
        get { return uid; }
        set { uid = value; }
    }

    public string AccessToken
    {
        get { return access_token; }
        set { access_token = value; }
    }

    public string Client
    {
        get { return client; }
        set { client = value; }
    }



    const string letters = "abcdefghijklmnopqrstuvwxyz1234567890";
    private string generateRandomString(uint strLength)
    {
        string randStr = "";
        for (int i = 0; i < strLength; i++)
        {
            char randLetter = letters[Mathf.FloorToInt(UnityEngine.Random.value * letters.Length)];
            randStr += randLetter;
        }
        return randStr;
    }

    string baseURL = "http://localhost:3000";

    IEnumerator UserRegistrationCoroutine()
    {
        string url = baseURL + "/api/v1/auth";
        UnityWebRequest request = UnityWebRequest.Post(url, "POST");

        string email = generateRandomString(16) + "@example.com";
        string userName = generateRandomString(16);
        string password = generateRandomString(16);
        string jsonBody = "{\"name\":\"" + userName + "\",\"email\":\"" + email + "\",\"password\":\"" + password + "\",\"password_confirmation\":\"" + password + "\"}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");


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
                PlayerPrefs.SetString("userName", userName);
                PlayerPrefs.SetString("email", email);
                PlayerPrefs.SetString("password", password);
                Dictionary<string, string> headers = request.GetResponseHeaders();
                uid = headers["uid"];
                access_token = headers["access-token"];
                client = headers["client"];
                Debug.Log(headers["uid"]);
                Debug.Log(headers["access-token"]);
                Debug.Log(headers["client"]);
                Test.SetActive(false);
            }
        }
    }

    IEnumerator UserSessionCoroutine()
    {
        string url = baseURL + "/api/v1/auth/sign_in";
        UnityWebRequest request = UnityWebRequest.Post(url, "POST");

        string userName = PlayerPrefs.GetString("userName");
        string password = PlayerPrefs.GetString("password");
        string jsonBody = "{\"name\":\"" + userName + "\",\"password\":\"" + password + "\"}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");


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
                Dictionary<string, string> headers = request.GetResponseHeaders();
                uid = headers["uid"];
                access_token = headers["access-token"];
                client = headers["client"];
                Debug.Log(headers["uid"]);
                Debug.Log(headers["access-token"]);
                Debug.Log(headers["client"]);
                Test.SetActive(false);
            }
        }
    }

    IEnumerator CreateFirstUserStageProgressCoroutine()
    {
        string url = baseURL + "/api/v1/stages/1/user_stage_progresses";
        UnityWebRequest request = UnityWebRequest.Post(url, "POST");

        string jsonBody = "{\"name\":\"name\"}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("uid", uid);
        request.SetRequestHeader("access-token", access_token);
        request.SetRequestHeader("client", client);

        Debug.Log("呼び出し中StageProgress");
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if(request.responseCode == 200)
            {
                Test.SetActive(false);
            }
        }
    }

    IEnumerator DeleteAllUserStageProgressCoroutine()
    {
        string url = baseURL + "/api/v1/stages/1/user_stage_progresses";
        UnityWebRequest request = UnityWebRequest.Delete(url);

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("uid", uid);
        request.SetRequestHeader("access-token", access_token);
        request.SetRequestHeader("client", client);

        Debug.Log("呼び出し中StageProgress");
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if(request.responseCode == 200)
            {
                Test.SetActive(false);
            }
        }
    }






    void Awake()
    {
        string userName = PlayerPrefs.GetString("userName", "");
        string password = PlayerPrefs.GetString("password", "");

        if (userName == "")
        {
            StartCoroutine(UserRegistrationCoroutine());
            StartCoroutine(CreateFirstUserStageProgressCoroutine());
        }
        else
        {
            StartCoroutine(UserSessionCoroutine());
        }
    }


    void Start()
    {
        // バナー広告呼び出し
        AdmobManager.Instance.RequestDefaultBanner("Bottom");

        // BGMの設定
        AudioManager.Instance.SetBGMAudioClip(null);
        if(PlayerPrefs.GetInt("Version1_1", 0) == 0) ChangePlayerPrefs();
    }

    /// <summary>
    /// スタートボタン
    /// </summary>
    public void OnClickStartButton()
    {
        AudioManager.Instance.PlaySE("Decision");
        AdmobManager.Instance.DestroyDefaultBanner();
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

        StartCoroutine(DeleteAllUserStageProgressCoroutine());
        dataResetPanel.SetActive(false);
        dataResetFinishPanel.SetActive(true);
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
        idButtonText.text = "ID:"+PlayerPrefs.GetString("userName");
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
