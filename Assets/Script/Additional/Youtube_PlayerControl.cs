using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Youtube_PlayerControl : MonoBehaviour, IVideoPlayer
{
    public YoutubePlayer youtubePlayer;
    Common.CallBack onEndReachedCallBack;
    Common.CallBack onErrorCallBack;

    public int bufferingProgress
    {
        get
        {
            return 0;
            //throw new NotImplementedException();
        }
    }

    public bool isBuffering
    {
        get
        {
            return false;

            //throw new NotImplementedException();
        }
    }

    public bool isPlaying
    {
        get { return youtubePlayer.videoPlayer.isPlaying; }
    }

    public float length
    {
        get { return youtubePlayer.videoPlayer.frameCount / youtubePlayer.videoPlayer.frameRate * 1000; }
    }

    public string path
    {
        get { return youtubePlayer.youtubeUrl; }
        set { youtubePlayer.youtubeUrl = value; }
    }

    public float position
    {
        get { return (float)youtubePlayer.videoPlayer.frame / (float)youtubePlayer.videoPlayer.frameCount; }
        set { youtubePlayer.Seek(value * youtubePlayer.videoPlayer.frameCount); }
    }

    public float time
    {
        get { return (float)youtubePlayer.videoPlayer.frame / youtubePlayer.videoPlayer.frameRate * 1000; }
        set { youtubePlayer.videoPlayer.frame = (long)(value / 1000 * youtubePlayer.videoPlayer.frameRate); }
    }

    public string GetError()
    {
        return "";
        //throw new NotImplementedException();
    }

    public void OnEndReached(Common.CallBack callback)
    {
        onEndReachedCallBack = callback;
    }

    public void OnError(Common.CallBack callback)
    {
        onErrorCallBack = callback;
    }

    public void Open()
    {
        youtubePlayer.Play(youtubePlayer.youtubeUrl);
        //throw new NotImplementedException();
    }

    public void Pause()
    {
        youtubePlayer.Pause();
    }

    public void Play()
    {
        youtubePlayer.Play();
    }

    public void Reject()
    {

        //throw new NotImplementedException();
    }

    public void Stop()
    {
        youtubePlayer.Stop();
    }

    // Start is called before the first frame update
    void Awake()
    {
        //GameObject go = gameObject.SearchChild("YoutubePlayer/YoutubePlayer");
        //Debug.Log("YoutubePlayer pos\n" + Common.GetGameObjectHierarchy(go));

        youtubePlayer = gameObject.SearchChild("YoutubePlayer").GetComponent<YoutubePlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
