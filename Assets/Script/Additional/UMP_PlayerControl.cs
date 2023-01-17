using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UMP_PlayerControl : MonoBehaviour//, IVideoPlayer
{
    /*
    UniversalMediaPlayer universalMediaPlayer;

    public bool isPlaying {
        get { return universalMediaPlayer.IsPlaying; }
    }

    public float length {
        get { return universalMediaPlayer.Length; }
    }

    public string path {
        get { return universalMediaPlayer.Path; }
        set { universalMediaPlayer.Path = value; }
    }

    public float position {
        get { return universalMediaPlayer.Position; }
        set { universalMediaPlayer.Position = value; }
    }

    public float time {
        get { return universalMediaPlayer.Time; }
        set { universalMediaPlayer.Time = (long)value; }
    }

    public void Open()
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        universalMediaPlayer.Pause();
    }

    public void Play()
    {
        universalMediaPlayer.Play();
    }

    public void Reject()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        universalMediaPlayer.Stop();
    }

    void Awake () {
        universalMediaPlayer = GetComponent<UniversalMediaPlayer>();
    }

    /*
    // Update is called once per frame
    void Update () {
		
	}
    */
}
