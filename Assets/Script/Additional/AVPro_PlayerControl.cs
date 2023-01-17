using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AVPro_PlayerControl : MonoBehaviour, IVideoPlayer
{
    MediaPlayer mediaPlayer;
    Common.CallBack onEndReachedCallBack;
    Common.CallBack onErrorCallBack;
    string _path;

    // IVideoPlayer interface implementation
    public string path
    {
        get { return _path; }
        set { _path = value; }
    }

    // A videó pozíciója 0 - 1 intervallumban
    public float position
    {
        get { return mediaPlayer.Control.GetCurrentTimeMs() / length; }
        set { mediaPlayer.Control.Seek(value * length); }
    }

    // A videó jelenlegi pozíciója milisecond-ban
    public float time
    {
        get { return mediaPlayer.Control.GetCurrentTimeMs(); }
        set { mediaPlayer.Control.Seek(value); }
    }

    // A videó hossza milisecond-ban
    public float length
    {
        get { return mediaPlayer.Info.GetDurationMs(); }
    }

    public bool isPlaying
    {
        get { return mediaPlayer.Control.IsPlaying(); }
    }

    public bool isBuffering
    {
        get { return mediaPlayer.Control.IsBuffering(); }
    }

    public int bufferingProgress
    {
        get { return (int)mediaPlayer.Control.GetBufferingProgress(); }
    }

    public void Open()
    {
        mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, _path);
    }

    public void Play()
    {
        mediaPlayer.Play();
    }

    public void Pause()
    {
        mediaPlayer.Pause();
    }

    public void Stop()
    {
        mediaPlayer.Stop();
    }

    public void Reject()
    {
        if (mediaPlayer != null && mediaPlayer.Control != null)
            mediaPlayer.Control.CloseVideo();
    }

    public string GetError() {
        return mediaPlayer.Control.GetLastError().ToString();
    }

    void Awake()
    {
        mediaPlayer = GetComponent<MediaPlayer>();
        mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
    }

    // Callback function to handle events
    public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        Debug.Log("AVPro : " + et.ToString() + ", errorCode : " + errorCode.ToString());

        switch (et)
        {
            case MediaPlayerEvent.EventType.MetaDataReady:
                break;
            case MediaPlayerEvent.EventType.ReadyToPlay:
                break;
            case MediaPlayerEvent.EventType.Started:
                break;
            case MediaPlayerEvent.EventType.FirstFrameReady:
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                onEndReachedCallBack();
                break;
            case MediaPlayerEvent.EventType.Closing:
                break;
            case MediaPlayerEvent.EventType.Error:
                onErrorCallBack();
                break;
            case MediaPlayerEvent.EventType.SubtitleChange:
                break;
            case MediaPlayerEvent.EventType.Stalled:
                break;
            case MediaPlayerEvent.EventType.Unstalled:
                break;
            default:
                break;
        }
    }

    public void OnEndReached(Common.CallBack callback)
    {
        onEndReachedCallBack = callback;
    }

    public void OnError(Common.CallBack callback)
    {
        onErrorCallBack = callback;
    }


    // Update is called once per frame
    void Update () {
        // Ha megy a videó lejátszó, akkor ne aludjon el a készülék
        if (mediaPlayer != null && mediaPlayer.Control != null)
            Screen.sleepTimeout = (isPlaying) ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
    }
}
