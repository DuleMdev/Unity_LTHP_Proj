using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class PanelLiveStream : MonoBehaviour {

    AVPro_PlayerControl videoControl;
    Text text;

    List<StreamTime> streamTimeList = new List<StreamTime>();

    void Awake() {
        videoControl = gameObject.SearchChild("AVPro").GetComponent<AVPro_PlayerControl>();
        text = gameObject.SearchChild("Text").GetComponent<Text>();

        //Debug.LogError(DateTime.UtcNow.ToString());
    }



	// Use this for initialization
	void Start () {

    }

    /*
    { 
        "error" : "false", 
        "answer" : { 
           "streamLink" : "http://classyedu.info:8080/hls/stream.m3u8", 
           "streamTimes" : [
              { 
                 "id" : "1",
                 "streamName" : "test",
                 "time_from" : "2018-02-27 16:39:00",
                 "time_to" : "2018-02-27 17:39:00",
                 "gmt_offset" : "+0100",
                 "notification_count" : "0"
              }
           ]
        }
    }
 */
    public void Initialize(JSONNode json) {
        JSONNode answer = json[C.JSONKeys.answer];
        videoControl.path = answer[C.JSONKeys.streamLink].Value;
        videoControl.Open();

        // streamTimeList feltöltése
        streamTimeList.Clear();
        for (int i = 0; i < answer[C.JSONKeys.streamTimes].Count; i++)
            streamTimeList.Add(new StreamTime(answer[C.JSONKeys.streamTimes][i]));

        RefreshText();
    }

    public void Reject() {
        videoControl.Reject();
    }

    // Szöveg beállítás
    public void RefreshText()
    {
        // Aktív UTC időpont megszerzése
        DateTime actUTCDateTime = DateTime.UtcNow;

        // Van-e épp aktív stream
        StreamTime activeStream = null; // tartalmazza az aktív streamet ami éppen most zajlik
        foreach (StreamTime streamTime in streamTimeList)
            if (streamTime.timeFrom < actUTCDateTime && streamTime.timeTo > actUTCDateTime) {
                activeStream = streamTime;
                break;
            }

        // Melyik a következő stream
        StreamTime nextStream = null; // tartalmazza a következő streamet ami a legközelebb lesz
        foreach (StreamTime streamTime in streamTimeList)
            if (streamTime.timeFrom > actUTCDateTime)
                if (nextStream == null || nextStream.timeFrom > streamTime.timeFrom)
                    nextStream = streamTime;

        string showText = Common.languageController.Translate(C.Texts.NoStream);
        if (activeStream != null)
        {
            showText = Common.languageController.Translate(C.Texts.WatchStream) + ": " + activeStream.ToString();
        }
        else if (nextStream != null)
        {
            showText = Common.languageController.Translate(C.Texts.NextStream) + ": " + nextStream.ToString();
        }

        text.text = showText;
    }

    // Update is called once per frame
    void Update () {
		
	}

    class StreamTime {
        public int id;
        public string streamName;
        public DateTime timeFrom;   // UTC
        public DateTime timeTo;     // UTC
        public int notificationCount;

        public StreamTime(JSONNode json) {
            id = json[C.JSONKeys.id].AsInt;
            streamName = json[C.JSONKeys.streamName].Value;

            int offsetMinutes = getOffsetMinutes(json[C.JSONKeys.gmt_offset].Value);

            timeFrom = DateTime.Parse(json[C.JSONKeys.time_from].Value).AddMinutes(-offsetMinutes);
            timeTo = DateTime.Parse(json[C.JSONKeys.time_to].Value).AddMinutes(-offsetMinutes);

            timeFrom = DateTime.Parse("2018-02-28 16:31:00");
            timeTo = DateTime.Parse("2018-02-28 17:39:00");

            notificationCount = json[C.JSONKeys.notification_count].AsInt;
        }

        int getOffsetMinutes(string gmtOffset) {
            int hours = Convert.ToInt32(gmtOffset.Substring(1, 2));
            int minutes = Convert.ToInt32(gmtOffset.Substring(3, 2));

            int sumMinutes = hours * 60 + minutes;

            return gmtOffset.Substring(0, 1) == "+" ? sumMinutes : -sumMinutes;
        }

        public string ToString()
        {
            // kiszámoljuk az idő eltolódást
            int offsetMinutes = (int)(DateTime.Now - DateTime.UtcNow).TotalMinutes;

            return streamName + " " + 
                timeFrom.AddMinutes(offsetMinutes).ToString("yyyy.MM.dd, HH:mm") + " - " +
                timeTo.AddMinutes(offsetMinutes).ToString("HH:mm");
        }
    }

}
