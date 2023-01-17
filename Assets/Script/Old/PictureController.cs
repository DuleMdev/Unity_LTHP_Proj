using UnityEngine;
using System.Collections;
using System.IO;


/*
Ez az objektum szolgáltatja a szükséges képeket, amelyeket a háttértárról tölt be.



*/

public class PictureController : MonoBehaviour {
    /*
    string termsOfUse = "TermsOfUse";
    string privacyPolicy = "PrivacyPolicy";
    */

    public float offest;

    // Use this for initialization
    void Awake () {
        Common.pictureControllerr = this;
	}

    static public string GetDocumentsDir()
    {
        //return Application.streamingAssetsPath;

#if UNITY_STANDALONE_WIN
        return Application.streamingAssetsPath;
#endif

#if UNITY_EDITOR
        Debug.Log("StreamingAsset könyvtár : " + Application.streamingAssetsPath);
        return Application.streamingAssetsPath;
#endif

#if UNITY_IOS
		return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
#endif

#if UNITY_ANDROID
        return Application.streamingAssetsPath; // Amíg nem a szerverről szedjük a képeket addig a streamingAsset-ben vannak
        //return Application.persistentDataPath;
#endif

#if UNITY_STANDALONE_OSX
		return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
#endif

#if UNITY_WEBGL
        return Application.streamingAssetsPath;
#endif


    }

    /*
    // A megadott kép nevet letölti a netről vagy ha a háttértáron megtalálható, akkor betölti onnan
    // Ha egyik sem megy, akkor nullt ad vissza
    [HideInInspector]
    public bool pictureDownloadWork; // Ha az internetről vagy a háttértárról töltjük a képet, akkor ez True, ha false, akkor a kép a texture változóban van
    [HideInInspector]
    public Texture2D downloadedPicture; // A letöltött vagy betöltött kép, null, ha nem sikerült

    public void GetPicture(string url)
    {
        pictureDownloadWork = true;
        this.downloadedPicture = null;
        StartCoroutine(GetPictureCoroutine(url));
    }

    IEnumerator GetPictureCoroutine(string url)
    {
        string picturesFolder = System.IO.Path.Combine(GetDocumentsDir(), "Pictures");
        string fileName = System.IO.Path.GetFileName(url);  // Az url-ből kiszedjük a nevet.
        string filePath = System.IO.Path.Combine(picturesFolder, fileName);

        Texture2D texture = new Texture2D(4, 4, TextureFormat.ARGB32, false);

        // megpróbáljuk betölteni a háttértárról a képet
        if (!LoadPictureFromFileSystem(texture, filePath))
        {
            // Ha nincs a háttértáron, akkor megpróbáljuk letölteni az internetről
            WWW www = new WWW(url); // download picture
            yield return www; // wait for download finish

            if (www.error == null)
            {
                if (texture.LoadImage(www.bytes))
                {
                    // Ha sikerült letölteni az internetről, akkor tároljuk a képet a háttértáron
                    System.IO.Directory.CreateDirectory(picturesFolder);
                    System.IO.File.WriteAllBytes(filePath, texture.EncodeToPNG());

#if UNITY_IOS
					iPhone.SetNoBackupFlag (fileName);
#endif
                }
                else {
                    texture = null;
                }
            }
            else
                texture = null;
        }

        // A kapott képet visszaadjuk
        this.downloadedPicture = texture;

        pictureDownloadWork = false;
    }

    // A megadott helyről megpróbálja letölteni a képet a megadott texturába
    bool LoadPictureFromFileSystem(Texture2D texture, string fileName)
    {
        try
        {
            texture.LoadImage(System.IO.File.ReadAllBytes(fileName));
            return true;
        }
        catch
        {
            return false;
        }
    }
    */

    public Sprite resultSprite;

    // A megadott helyről megpróbálja letölteni a képet a resués visszaadja a letöltött texturát, ha nem sikerült, akkor null-t ad vissza
    public IEnumerator LoadSpriteFromFileSystemCoroutine(string fileName)
    {
        resultSprite = null;
        Texture2D texture = null;

#if UNITY_EDITOR
        Debug.Log("UNITY_EDITOR");

        /*
        if (texture == null)
        {
            if (Common.pictureRepository != null)
            {
                Debug.Log("Picture Repository Activated.");
                texture = Common.pictureRepository.GetPicture(fileName);
                Debug.Log("Picture Repository : " + fileName + " load " + ((texture == null) ? "fail" : "success"));
            }
        }*/
        
        if (!string.IsNullOrEmpty(fileName))
        {
            string picturesFolder = System.IO.Path.Combine(GetDocumentsDir(), "Pictures");
            string filePath = System.IO.Path.Combine(picturesFolder, fileName);

            texture = new Texture2D(4, 4, TextureFormat.ARGB32, false);
            texture.LoadImage(System.IO.File.ReadAllBytes(filePath));
        }
#else


#if UNITY_WEBGL
        /*
        Debug.Log("WebGL");
        if (Common.pictureRepository != null)
            texture = Common.pictureRepository.GetPicture(fileName);
        */

        if (!string.IsNullOrEmpty(fileName)) {
            string picturesFolder = System.IO.Path.Combine(GetDocumentsDir(), "Pictures");
            string filePath = System.IO.Path.Combine(picturesFolder, fileName);

            //texture = new Texture2D(4, 4, TextureFormat.ARGB32, false);
            WWW www = new WWW(filePath);

            yield return www;

            if (string.IsNullOrEmpty(www.error))
                texture = www.texture;
        }

#endif

#if UNITY_ANDROID
        Debug.Log("UNITY_ANDROID");

        if (!string.IsNullOrEmpty(fileName)) {
            string picturesFolder = System.IO.Path.Combine(GetDocumentsDir(), "Pictures");
            string filePath = System.IO.Path.Combine(picturesFolder, fileName);

            //texture = new Texture2D(4, 4, TextureFormat.ARGB32, false);
            WWW www = new WWW(filePath);

            yield return www;

            if (string.IsNullOrEmpty(www.error))
                texture = www.texture;
        }
#endif

#endif

        /*
        if (texture == null)
        {
            if (Common.pictureRepository != null)
            {
                Debug.Log("Picture Repository Activated.");
                texture = Common.pictureRepository.GetPicture(fileName);
                Debug.Log("Picture Repository : " + fileName + ((texture == null) ? "fail" : "success"));
            }
        }
        */
        resultSprite = (texture == null) ? null: Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        yield return null;
    }





    /*
    // Betölti a háttértáron tárolt Terms Of Use szöveget
    public string GetTermsOfUse()
    {
        return LoadText(termsOfUse);
    }

    // Betölti a háttértáron tárolt Privacy Policy szöveget
    public string GetPrivacyPolicy()
    {
        return LoadText(privacyPolicy);
    }

    // Betölti a megadott fájlt egy string-be és visszaadja
    // Ha valamilyen hiba lépet fel, akkor üres stringet ad vissza
    public static string LoadText(string fileName)
    {
        try
        {
            return File.ReadAllText(System.IO.Path.Combine(GetDocumentsDir(), fileName));
        }
        catch
        {
            return "";
        }
    }

    // Elmenti a megadott fájlba a megadott szöveget
    public static void SaveText(string fileName, string text)
    {
        string fn = System.IO.Path.Combine(GetDocumentsDir(), fileName);
        File.WriteAllText(fn, text);

#if UNITY_IOS
		iPhone.SetNoBackupFlag (fn);
#endif
    }
    */
}
