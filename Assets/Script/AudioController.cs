using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioController : MonoBehaviour
{
    [System.Serializable]
    public class AudioClipData
    {
        public string name = "Clip Name";   // A képernyő neve vagy ha úgy jobban tetszik az azonosítója (Ezt a string-et kell megadni a képernyő váltásnál)
        public AudioClip audioClip;         // A képernyőhöz tartozó vezérlő szkript
    }

    public AudioClipData[] audioClips;      // A különböző audioClip-ek egy hivatkozási névvel ellátva (a hivatkozási név alapján lehet kérni őket)

    // Céges audioClipek
    public AudioClipData[] audioClipsCompany; // A különböző audioClip-ek egy hivatkozási névvel ellátva (a hivatkozási név alapján lehet kérni őket)


    public AudioGroup[] audioGroups;

    public float backgroundFadeTime;        // Mennyi idő alatt növekedjen a hangerő a maximálisra
    public AudioClip[] backgroundMusics;    // Ebben a tömbben a háttérzenéket kell megadni
    int aktBackgroundMusicIndex;            // Melyik háttérzenét játszuk aktuálisan

    AudioSource[] audioSources;             // Ezeken megy a háttérzene
    float[] volumes;                        // Az egyes audioforrásoknak mekkora legyen a hangereje
    float[] fadeTimes;                      // Melyik audioSoruce mennyi idő alatt fade-eljen
    int aktAudioSourceIndex;                // Melyik audioSource az aktuális

    //float targetVolume;                   // A háttér zenének milyen hangerőre kell emelkednie

    public void Awake() {
        Common.audioController = this;

        audioSources = GetComponents<AudioSource>();
        volumes = new float[audioSources.Length];
        fadeTimes = new float[audioSources.Length];
    }

    public void SetAudio(AudioList audioList, AudioClip audioClip, float volume = 1f, float delay = 0f)
    {
        if (audioList != null && audioClip != null)
        {
            audioList.audioSource.clip = audioClip;
            audioList.audioSource.volume = volume;
            audioList.audioSource.PlayDelayed(delay);
            audioList.isPlaying = true;
        }
    }

    public void SetAudio(AudioList audioList, string audioGroupName, float volume = 1f, float delay = 0f)
    {
        AudioGroup audioGroup = GetAudioGroupFromName(audioGroupName);

        if (audioGroup != null)
        {
            AudioClip audioClip = audioGroup.GetNextClip();
            print("Audio group : " + audioClip.name);
            SetAudio(audioList, audioClip, volume, delay);
        }
    }

    // Lejátsza a megadott nevű hangot
    // sfxName - Lehet egy szimpla audionév vagy egy csoport neve
    public void SFXPlay(string sfxName) {

        AudioClip audioClip = GetAudioClip(sfxName);

        if (audioClip != null)
            AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position); // Vector3.zero);
    }

    public AudioClip GetAudioClip(string sfxName) {

        AudioGroup audioGroup = GetAudioGroupFromName(sfxName);

        return (audioGroup != null) ? audioGroup.GetNextClip() : GetAudioClipFromName(sfxName);
    }

    // Beállítja a háttérzenét
    // backgroundMucisIndex - Hányadik háttérzenét játsza le a background tömbben felsoroltak közül (az első a nulladik)
    // 
    public void SetBackgroundMusic(int backgroundMusicIndex, float volume = 1, float fadeIn = -1, bool loop = true, float delay = 0) {
        if (backgroundMusicIndex < 0 || 
            backgroundMusicIndex >= backgroundMusics.Length ||
            backgroundMusicIndex == aktBackgroundMusicIndex) {
            return;
        }

        // Meghatározzuk a következő audioSource-ot amibe a háttérzenét tölteni kell
        aktAudioSourceIndex++;
        if (aktAudioSourceIndex >= audioSources.Length)
            aktAudioSourceIndex = 0;

        fadeTimes[aktAudioSourceIndex] = (fadeIn > 0) ? fadeIn : backgroundFadeTime;
        volumes[aktAudioSourceIndex] = volume;
        AudioSource audioSource = audioSources[aktAudioSourceIndex];
        audioSource.volume = 0;
        audioSource.clip = backgroundMusics[backgroundMusicIndex];
        audioSource.loop = loop;
        audioSource.PlayDelayed(delay);

        aktBackgroundMusicIndex = backgroundMusicIndex; 
    }

    public void MuteBackgroundMusic(float fadeTime = -1) {
        if (fadeTime >= 0 && aktAudioSourceIndex >= 0)
            fadeTimes[aktAudioSourceIndex] = fadeTime;

        aktAudioSourceIndex = -1;
        aktBackgroundMusicIndex = -1;
    }

    public void MuteBackgroundMusicImmediatelly() {
        MuteBackgroundMusic();

        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.Stop();
        }
    }

    // Megkeresi a megadott nevű audio csoportot
    // Ha nincs olyan, akkor null-t ad vissza
    AudioGroup GetAudioGroupFromName(string name)
    {
        foreach (AudioGroup item in audioGroups)
        {
            if (item.groupName == name)
                return item;
        }

        return null;
    }

    AudioClip GetAudioClipFromName(string name) {

        if (Common.configurationController.appID != ConfigurationController.AppID.mInspire)
            foreach (AudioClipData audioClipData in audioClipsCompany)
            {
                if (audioClipData.name == name)
                    return audioClipData.audioClip;
            }

        foreach (AudioClipData audioClipData in audioClips)
        {
            if (audioClipData.name == name)
                return audioClipData.audioClip;
        }

        return null;
    }

    void Update()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            AudioSource audioSource = audioSources[i];

            float volumeDelta = Time.deltaTime * volumes[i] / fadeTimes[i];
            float volume = audioSource.volume + ((aktAudioSourceIndex == i) ? volumeDelta : -volumeDelta);

            if (volume > volumes[i])
                volume = volumes[i];
            if (volume < 0)
                volume = 0;
            audioSource.volume = volume;
        }
    }

    /*
    Ezzel az objektummal egy audio listát tudunk lejátszani, ami azt jelenti, hogy a megadott audiókat egymás után lejátsza.
    
    Az objektum létrehozásakor meg kell adni, hogy melyik AudioSource-ban játszuk le a megadott AudioClip-eket.

    Add - Ezzel a metódussal tudunk egy új klipet a listához adni.
    SpeakThisImmediatelly - Ekkor a megadott klippet azonnal elkezdi játszani úgy, hogy a listát törli.
    SpeakListClear - Törli a listát a lejátszást azonnal abbahagyja
    Update - Az update metódust kell hívogatni, hogy működjön a rendszer.
    */
    public class AudioList
    {
        public bool isPlaying; // { get; private set; }

        List<AudioClip> listAudioClip = new List<AudioClip>();
        public AudioSource audioSource;

        public AudioList(AudioSource audioSource)
        {
            this.audioSource = audioSource;
        }

        // Egy újabb clipet adunk a listához
        public void Add(AudioClip audioClip)
        {
            if (audioClip == null) return;

            listAudioClip.Add(audioClip);
            isPlaying = true;
        }

        public void SpeakThisImmediatelly(AudioClip audioClip)
        {
            SpeakListClear();
            Add(audioClip);
        }

        public void SpeakListClear()
        {
            listAudioClip.Clear();
            audioSource.Stop();
            isPlaying = false;
        }

        public void Update()
        {
            isPlaying = false;

            if (!audioSource.isPlaying) // Ha az előző klipp befejeződött (azaz nem játszik már az AudioSource)
            {
                if (listAudioClip.Count != 0) // Ha nem üret a lista, akkor jöhet a következő klipp
                { // get the next clip
                    audioSource.clip = listAudioClip[0];
                    listAudioClip.RemoveAt(0);              // Eltávolítjuk a listából a most elkezdett klippet
                    audioSource.Play();
                    isPlaying = true;
                }
            }
            else {
                isPlaying = true;
            }
        }
    }

    /* 
    Egy audio csoport létrehozásához
    */
    [System.Serializable]
    public class AudioGroup
    {
        public string groupName;
        public bool random; // A csoportból véletlenszerűen játszon egy elemet vagy sorrendben
        public List<AudioClip> audioClipList;

        [HideInInspector]
        public int audioClipCounter; // A következő hányadik clip a csoportból, ha sorrendben kell lejátszani, az utolsó után újra az első következik

        // Visszaadja a csoportban a következő lájátszandó audio klipet
        public AudioClip GetNextClip()
        {
            int index = audioClipCounter;
            if (random)
            {
                index = Random.Range(0, audioClipList.Count);
            }
            else {
                audioClipCounter = audioClipCounter >= audioClipList.Count - 1 ? 0 : audioClipCounter + 1;
            }

            return index >= audioClipList.Count ? null : audioClipList[index];
        }
    }
}