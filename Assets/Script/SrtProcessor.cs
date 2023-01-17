using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/*
Az objektum egy srt fájlt dolgoz fel.
Majd egy megadott időre vissza adja a megjelenítendő felíratot.

*/

public class SrtProcessor
{
    public class SrtData
    {
        public int index;
        public int startTime;
        public int endTime;
        public string text;

        public SrtData(string index, string startTime, string endTime, string text)
        {
            this.index = int.Parse(index);
            this.startTime = (int)(Common.StringToDateTime(startTime, "HH:mm:ss,fff").Ticks / 10000);
            this.endTime = (int)(Common.StringToDateTime(endTime, "HH:mm:ss,fff").Ticks / 10000);
            this.text = text;
        }

        public SrtData(int index, int startTime, int endTime, string text)
        {
            this.index = index;
            this.startTime = startTime;
            this.endTime = endTime;
            this.text = text;
        }
    }

    List<SrtData> listOfSrtData = new List<SrtData>();

    public SrtProcessor(string text)
    {
        // Szöveg feldolgozása
        Match match = Regex.Match(text, @"((\d+)\r?\n?(\d{2}:\d{2}:\d{2},\d{3}) --> (\d{2}:\d{2}:\d{2},\d{3})\r?\n?((.+\r?\n?)+)\r?\n?)+");
        if (match.Success)
        {
            for (int captureCtr = 0; captureCtr < match.Groups[1].Captures.Count; captureCtr++)
            {
                listOfSrtData.Add(new SrtData(
                    match.Groups[2].Captures[captureCtr].Value,
                    match.Groups[3].Captures[captureCtr].Value,
                    match.Groups[4].Captures[captureCtr].Value,
                    match.Groups[5].Captures[captureCtr].Value)
                );
            }
        }
    }

    /// <summary>
    /// A megadot idő alapján meghatározza, hogy melyik szövegnek kell megjelennie.
    /// Ha a visszatérési érték null, akkor nem kell szöveget megjeleníteni.
    /// </summary>
    /// <param name="time">Az idő ezredmásodpercben</param>
    /// <returns></returns>
    public SrtData GetText(int time)
    {
        for (int i = 0; i < listOfSrtData.Count; i++)
        {
            SrtData srtData = listOfSrtData[i];

            if (srtData.startTime < time && srtData.endTime > time)
                return srtData;
        }

        return null;
    }
}
