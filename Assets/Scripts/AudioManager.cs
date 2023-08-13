using UnityEngine;

public class AudioManager
{
    static bool bgmStatus = false;
    static bool seStatus = false;

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
