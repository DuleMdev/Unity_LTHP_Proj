using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IVideoPlayer 
{
    string path { get; set; }       // A videó útvonala
    float position { get; set; }    // A videó pozíciója 0-1 intervallumban
    float time { get; set; }        // A videó pozíciója milisecundumban
    float length { get; }           // A videó fájl hossza

    bool isPlaying { get; } // Megy-e a lejátszás
    bool isBuffering { get; }   // Bufferelés történik éppen

    int bufferingProgress { get; } // Hol tart a bufferelés 0 - 100 egész értékek

    void Open();    // Megnyitja a path-ban megadott fájlt lejátszásra
    void Play();    // Folytatja a lejátszást
    void Pause();   // Szünetelteti a lejátszást
    void Stop();    // Leállítja a lejátszást, a pozíciót az elejére állítja
    void Reject();  // Bezárja a megnyitott fájlt

    string GetError();  // Megadja, hogy mi volt az utolsó hiba

    void OnEndReached(Common.CallBack callback); // Elértük a videó végét
    void OnError(Common.CallBack callback); // Hiba történt
}
