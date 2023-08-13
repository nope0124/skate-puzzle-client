using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static bool bgmStatus = true;
    static bool seStatus = true;
    private static GameObject audioManagerObject;
    private static AudioSource bgmAudioSource;
    private static GameObject seObject;


    private static AudioManager singleton;

    public static AudioManager Instance
    {
        get {
            if (singleton == null)
            {
                // AudioManager作成
                audioManagerObject = new GameObject("AudioManager");
                audioManagerObject.AddComponent<AudioManager>();

                GameObject bgmObject = new GameObject("BGM");
                bgmAudioSource = bgmObject.AddComponent<AudioSource>();
                bgmAudioSource.playOnAwake = true;
                bgmAudioSource.loop = true;

                seObject = new GameObject("SE");
                seObject.AddComponent<AudioSource>();

                bgmObject.transform.SetParent(audioManagerObject.transform, false);
                seObject.transform.SetParent(audioManagerObject.transform, false);


                // 遷移先シーンでもオブジェクトを破棄しない。
                DontDestroyOnLoad(audioManagerObject);

                singleton = audioManagerObject.GetComponent<AudioManager>();
            }
            return singleton;
        }
    }


    public bool BGMStatus
    {
        get { return bgmStatus; }
        set {
            bgmStatus = value;
            if(bgmStatus) {
                bgmAudioSource.mute = false;
            }else {
                bgmAudioSource.mute = true;
            }
        }
    }

    public bool SEStatus
    {
        get { return seStatus; }
        set { seStatus = value; }
    }

    public void SetBGMAudioClip(AudioClip audioClip) {
        bgmAudioSource.clip = audioClip;
        bgmAudioSource.Play();
    }


}
