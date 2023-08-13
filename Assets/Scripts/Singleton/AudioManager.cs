using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static bool bgmStatus = false;
    static bool seStatus = false;
    private static GameObject audioManagerObject;


    private static AudioManager singlton;

    public static AudioManager Instance
    {
        get {
            if (singlton == null)
            {
                // AudioManager作成
                audioManagerObject = new GameObject("AudioManager");
                audioManagerObject.AddComponent<AudioManager>();

                // 遷移先シーンでもオブジェクトを破棄しない。
                DontDestroyOnLoad(audioManagerObject);

                singlton = audioManagerObject.GetComponent<AudioManager>();
            }
            return singlton;
        }
    }


    public bool BGMStatus
    {
        get { return bgmStatus; }
        set { bgmStatus = value; }
    }

    public bool SEStatus
    {
        get { return seStatus; }
        set { seStatus = value; }
    }
}
