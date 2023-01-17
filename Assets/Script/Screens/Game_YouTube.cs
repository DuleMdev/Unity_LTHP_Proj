using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_YouTube : GameAncestor
{
    [Tooltip("Mennyi idő alatt jelenjen meg a vezérlő panel.")]
    public float showAnimTime;
    [Tooltip("Mennyi idő múlva tűnjön el automatikusan a vezérlő panel.")]
    public float stayTime;
    [Tooltip("Mennyi idő alatt tűnjön el a vezérlő panel.")]
    public float hideAnimTime;

    Text textInfo;
    Text textBuffering;
    Text textError;

    //UniversalMediaPlayer universalMediaPlayer;

    //YoutubePlayer youtubePlayer;

    UMP_PlayerControl UMP_playerControl;
    AVPro_PlayerControl AVPro_playerControl;
    Youtube_PlayerControl youtube_playerControl;

    CanvasGroup canvasGroup;
    GameObject playButton;
    GameObject pauseButton;
    Slider sliderPos;

    GameObject cover;

    GameObject imageYoutube;


    IVideoPlayer actVideoPlayer;

    TaskYouTube taskData;

    bool controlPanelIsVisible { get { return canvasGroup.alpha != 0; } }

    bool _wasPlayingOnScrub;    // Lejátszás alatt alakrták állítgatni a videó pozícióját
    float _setVideoSeekSliderValue; // 

    bool needShow;  // Elő kell jönnie a panelnak
    float remainStayTime;   // Mennyi idő maradt az eltűnésig

    float previousVideoTime;    // A videó működési idejének mérése

    enum VideoPlayerType
    {
        None,
        UMP_Player,
        AVPro_Player,
        Youtube_Player,
    }

    VideoPlayerType actVideoPlayerType;

    // Use this for initialization
    override public void Awake()
    {
        base.Awake();



        textInfo = gameObject.SearchChild("TextInfo").GetComponent<Text>();
        textBuffering = gameObject.SearchChild("TextBuffering").GetComponent<Text>();
        textError = gameObject.SearchChild("TextError").GetComponent<Text>();

        //universalMediaPlayer = gameObject.SearchChild("UniversalMediaPlayer").GetComponent<UniversalMediaPlayer>();

        //youtubePlayer = gameObject.GetComponent<YoutubePlayer>();

        UMP_playerControl = gameObject.SearchChild("UniversalMediaPlayer").GetComponent<UMP_PlayerControl>();
        AVPro_playerControl = gameObject.SearchChild("AVPro").GetComponent<AVPro_PlayerControl>();
        youtube_playerControl = gameObject.SearchChild("YoutubePlayer").GetComponent<Youtube_PlayerControl>();

        canvasGroup = gameObject.SearchChild("Panel").GetComponent<CanvasGroup>();
        playButton = gameObject.SearchChild("PlayButton").gameObject;
        pauseButton = gameObject.SearchChild("PauseButton").gameObject;
        sliderPos = gameObject.SearchChild("SliderPos").GetComponent<Slider>();

        cover = gameObject.SearchChild("Cover").gameObject;

        imageYoutube = gameObject.SearchChild("ImageYoutube").gameObject;
        imageYoutube.SetActive(false);

        //actVideoPlayer = (IVideoPlayer)UMP_playerControl;
        actVideoPlayer = (IVideoPlayer)AVPro_playerControl;

        inactiveTimeLimit = 3; // Három másodperc
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        // Lekérdezzük a feladat adatait
        taskData = (TaskYouTube)Common.taskController.task;

        // Bekapcsoljuk a megfelelő videólejátszót
        /*
        if (1 == 2)
        {
            UMP_playerControl.gameObject.SetActive(true);
            actVideoPlayer = (IVideoPlayer)UMP_playerControl;

            AVPro_playerControl.gameObject.SetActive(false);
        }
        else
        {
            AVPro_playerControl.gameObject.SetActive(true);
            actVideoPlayer = (IVideoPlayer)AVPro_playerControl;

            UMP_playerControl.gameObject.SetActive(false);
        }*/

        //SelectVideoPlayer(VideoPlayerType.None);
        SelectVideoPlayer(VideoPlayerType.AVPro_Player);



        /*
        actVideoPlayer.OnEndReached( () => {
            needShow = true;
            actVideoPlayer.Stop();
            //actVideoPlayer.time = 0;
        });

        actVideoPlayer.OnError(() => {
            textError.text = actVideoPlayer.GetError();
        });
        */

        canvasGroup.alpha = 0;
        remainStayTime = 0;
        needShow = false;

        yield return null;
    }

    public override IEnumerator ScreenShowStartCoroutine()
    {
        StartCoroutine(base.ScreenShowStartCoroutine());

        //actVideoPlayer.path = taskData.link;
        Debug.Log(taskData.link);
        //actVideoPlayer.path = "http://test.classyedu.com/testFullClassy/video.mp4";

        yield return null;
    }

    public override IEnumerator ScreenShowFinishCoroutine()
    {
        yield return StartCoroutine(base.ScreenShowFinishCoroutine());

        string link = taskData.link;

        /*
        if (Common.configurationController.YoutubeTest)
            link = Common.configurationController.YoutubeTestLink;
            */






        /*
        string url = youtube_playerControl.youtubePlayer.CheckVideoUrlAndExtractThevideoId(link);
        SelectVideoPlayer(url == "none" ? VideoPlayerType.AVPro_Player : VideoPlayerType.Youtube_Player);

        actVideoPlayer.path = link;
        actVideoPlayer.Open();
        */


        SelectVideoPlayer(VideoPlayerType.AVPro_Player);

        string url = youtube_playerControl.youtubePlayer.CheckVideoUrlAndExtractThevideoId(link);
        if (url == "none")
        {
            // Ha nem Youtube link
            // Akkor az eredeti linket nyitjuk meg
            actVideoPlayer.path = link;
            actVideoPlayer.Open(); 
        }
        else
        {
#if UNITY_WEBGL
            // Ha youtube link és WebGL, akkor nem töltjük be, hanem a youtube gombra kattintással lejátszuk a böngészőben egy új ablakban
            imageYoutube.SetActive(true);
#else
            // Ha Youtube link és nem webgl, akkor betöltjük
            youtube_playerControl.youtubePlayer.LoadUrl(link);
#endif
        }







        //link = "https://www.youtube.com/watch?v=ETcOGmdUTS0";
        //youtubePlayer.Play(link);

        /*
        // Megvizsgáljuk, hogy Youtube linkről van-e szó
        string url = youtubePlayer.CheckVideoUrlAndExtractThevideoId(link);

        if (url == "none")
        {
            // Ha nem Youtube link
            // Akkor az eredeti linket nyitjuk meg
            actVideoPlayer.path = taskData.link;
            actVideoPlayer.Open();
        }
        else
        {
            // Ha Youtube link
            // Akkor megszerezzük a tényleges linket
#if UNITY_WEBGL
                StartCoroutine(WebGlRequest(youtubeUrl));
#else
                gettingYoutubeURL = true;
                GetDownloadUrls(UrlsLoaded, youtubeUrl, this);
#endif


        }

        link = (url == "none") ? link : url;
        */


        //youtubePlayer.PlayYoutubeVideo("https://www.youtube.com/watch?v=ETcOGmdUTS0");
        //youtubePlayer.Play(taskData.link);

        //actVideoPlayer.path = taskData.link;
        //actVideoPlayer.Open();
    }

    public void ClickYoutubeButton()
    {
        Application.ExternalEval("window.open(\"" + taskData.link + "\")"); // Link megnyitása böngészőben egy új ablakban
    }

    void SelectVideoPlayer(VideoPlayerType? videoPlayerType = null)
    {
        if (videoPlayerType != null)
            actVideoPlayerType = videoPlayerType.Value;

        UMP_playerControl.gameObject.SetActive(actVideoPlayerType == VideoPlayerType.UMP_Player);
        AVPro_playerControl.gameObject.SetActive(actVideoPlayerType == VideoPlayerType.AVPro_Player);
        //youtube_playerControl.gameObject.SetActive(actVideoPlayerType == VideoPlayerType.Youtube_Player);

        switch (actVideoPlayerType)
        {
            case VideoPlayerType.None:          actVideoPlayer = null; break;
            case VideoPlayerType.UMP_Player:    actVideoPlayer = (IVideoPlayer)UMP_playerControl; break;
            case VideoPlayerType.AVPro_Player:  actVideoPlayer = AVPro_playerControl; break;
            case VideoPlayerType.Youtube_Player:actVideoPlayer = youtube_playerControl; break;
        }

        if (actVideoPlayer != null)
        {
            actVideoPlayer.OnEndReached(() => {
                needShow = true;
                actVideoPlayer.Stop();
            });

            actVideoPlayer.OnError(() => {
                textError.text = actVideoPlayer.GetError();
            });
        }
    }

    
    public void YoutubeLinkIsReady()
    {
        Debug.Log(Common.Now() + "- GameYoutube: Link is ready\n" + youtube_playerControl.youtubePlayer.videoUrl);
        actVideoPlayer.path = youtube_playerControl.youtubePlayer.videoUrl;
        actVideoPlayer.Open();


        Debug.Log("GetVideoTitle : \n" + youtube_playerControl.youtubePlayer.GetVideoTitle());

    }
    

    public override IEnumerator ScreenHideFinish()
    {
        StartCoroutine(base.ScreenHideFinish());

        actVideoPlayer.Reject();

        yield break;
    }

    override public void Update()
    {
        base.Update();

        textError.text = actVideoPlayer.GetError();

        textBuffering.gameObject.SetActive(actVideoPlayer.isBuffering);
        textBuffering.text = "Buffering : " + actVideoPlayer.bufferingProgress;

        if (actVideoPlayer.isBuffering)
            Debug.LogError("isBuffering : " + actVideoPlayer.bufferingProgress);

        // beállítjuk a slider pozícióját
        if (actVideoPlayer.length > 0)
        {
            _setVideoSeekSliderValue = actVideoPlayer.position;
            sliderPos.value = actVideoPlayer.position;
        }
        

        //slider.enabled = universalMediaPlayer.Position >= 0;
        //slider.value = universalMediaPlayer.Position;

        textInfo.text =
            "Position: " + actVideoPlayer.position +
            "\nTime: " + actVideoPlayer.time +
            "\nLength: " + actVideoPlayer.length;

        /*
        if (!universalMediaPlayer.IsPlaying) {
            needShow = true;
            remainStayTime = stayTime;
        }*/

        cover.SetActive(canvasGroup.alpha != 1);

        if (canvasGroup.alpha == 1)
        {
            if (actVideoPlayer.isPlaying) {
                remainStayTime -= Time.deltaTime;
                if (remainStayTime < 0)
                    needShow = false;
            }
        }
        else {
            remainStayTime = stayTime;
        }

        //float deltaChange = (needShow) ? Time.deltaTime / showAnimTime : -Time.deltaTime / hideAnimTime;
        canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha + ((needShow) ? Time.deltaTime / showAnimTime : -Time.deltaTime / hideAnimTime));

        playButton.SetActive(!actVideoPlayer.isPlaying);
        pauseButton.SetActive(actVideoPlayer.isPlaying);
    }

    public void OnVideoSeekSlider(float value)
    {
        actVideoPlayer.position = value;

        /*
        if (slider.value != _setVideoSeekSliderValue && _wasPlayingOnScrub)
        {
            actVideoPlayer.position = slider.value;
            Debug.Log("Slider change by hand");
        }
        */
    }

    
    public void OnVideoSliderDown()
    {
        _wasPlayingOnScrub = actVideoPlayer.isPlaying;
        if (_wasPlayingOnScrub)
        {
            actVideoPlayer.Pause();
        }

        //OnVideoSeekSlider();
    }

    public void OnVideoSliderUp()
    {
        if (_wasPlayingOnScrub)
        {
            actVideoPlayer.Play();
            _wasPlayingOnScrub = false;
        }
    }
    

    /*
    public void SliderValueChange() {
        
        if (slider.value != universalMediaPlayer.Position) {
            Debug.Log("SliderValueChange");
            universalMediaPlayer.Position = slider.value;
        }
        
    }*/

    public void Log(string message) {
        //Debug.Log(message);
    }

    // A menüből kiválasztották a kilépést a játékból
    /*
    IEnumerator ExitCoroutine()
    {
        Common.taskController.GameExit();
        yield return null;
    }
    */

    /*
    public void EndReached() {
        needShow = true;
    }
    */

    public void ControlButtonClick(string buttonName)
    {
        if (controlPanelIsVisible)
        {
            switch (buttonName)
            {
                case "Cover":
                    needShow = true;
                    remainStayTime = stayTime;
                    break;

                case "ReStart":
                    actVideoPlayer.Open();
                    //actVideoPlayer.Stop();
                    //actVideoPlayer.Play();
                    remainStayTime = 0;
                    break;

                case "Play":
                    actVideoPlayer.Play();
                    remainStayTime = 0;
                    break;

                case "Pause":
                    actVideoPlayer.Pause();
                    needShow = true;
                    remainStayTime = 0;
                    break;

                case "Finish":
                    ButtonClick(C.Program.GameMenuNext);
                    //StartCoroutine(ExitCoroutine());
                    remainStayTime = 0;
                    break;

                default:

                    break;
            }

        }
        else
        {
            needShow = true;
        }
    }

    // Ha rákattintottak egy gombra, akkor meghívódik ez az eljárás a gombon levő Button szkript által
    /*
    override protected void ButtonClick(Button button)
    {
        if (userInputIsEnabled)
        {
            switch (button.buttonType)
            {
                case Button.ButtonType.Exit: // Ha megnyomták a kilépés gombot
                    StartCoroutine(ExitCoroutine());
                    break;

                case Button.ButtonType.SwitchLayout: // Megnyomták a layout váltó gombot
                    //layoutManager.ChangeLayout();
                    //SetPictures();
                    break;
            }
        }
    }
    */

    override protected void MeasureElapsedTime()
    {
        // Ha a videó megy és nem telt el az inactiveTimeLimit, akkor jóváírjuk az utolsó
        // videó idő változásástól eltelt időt. Ha túl vagyunk már az inactiveTimeLimit-en, akkor nem.
        // Tehát ha a videót leállították, majd egy fél óra múlva újra indítják, akkor nem fogja jóvá írni a fél órát
        // mivel az már túl van az inaktiveTimeLimiten, ami 3 másodperc
        float videoTime = actVideoPlayer.time; // 

        if (previousVideoTime != videoTime)
        {
            if (inactiveTime < inactiveTimeLimit)
                elapsedGameTime += inactiveTime;

            inactiveTime = 0;
        }

        inactiveTime += Time.deltaTime;

        previousVideoTime = videoTime;
    }
}

