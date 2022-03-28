using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	static bool bgmFlag = false;
    static bool seFlag = false;

    public bool GetBGMFlag() {
        return bgmFlag;
    }
	
	public void SetBGMFlag(bool argBGMFlag) {
        bgmFlag = argBGMFlag;
    }

    public bool GetSEFlag() {
        return seFlag;
    }

	public void SetSEFlag(bool argSEFlag) {
        seFlag = argSEFlag;
    }
}
