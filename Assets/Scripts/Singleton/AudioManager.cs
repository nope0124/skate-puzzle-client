using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static bool bgmStatus = true;
    static bool seStatus = true;
    
    private static GameObject audioManagerObject;
    private static AudioSource bgmAudioSource;
    private static AudioSource seAudioSource;

    private static AudioManager singleton;

    public static AudioManager Instance
    {
        get {
            if (singleton == null)
            {
                // AudioManager作成
                audioManagerObject = new GameObject("AudioManager");
                audioManagerObject.AddComponent<AudioManager>();

                // BGMAudioSource作成
                GameObject bgmObject = new GameObject("BGM");
                bgmAudioSource = bgmObject.AddComponent<AudioSource>();
                bgmAudioSource.playOnAwake = true;
                bgmAudioSource.loop = true;

                // SEAudioSource作成
                GameObject seObject = new GameObject("SE");
                seAudioSource = seObject.AddComponent<AudioSource>();
                seAudioSource.playOnAwake = false;

                // AudioManagerに付ける
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
        set {
            seStatus = value;
            if(seStatus) {
                seAudioSource.mute = false;
            }else {
                seAudioSource.mute = true;
            }
        }
    }

    public void SetBGMAudioClip(AudioClip audioClip) {
        bgmAudioSource.clip = audioClip;
        bgmAudioSource.Play();
    }

    public void PlaySE(string seName) {
        // if(seStatus == false) return;
        seAudioSource.PlayOneShot(Resources.Load<AudioClip>($"Music/SE/{seName}"));
    }

}
