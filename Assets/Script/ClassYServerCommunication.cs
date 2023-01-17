using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ClassYServerCommunication : MonoBehaviour
{
    static int requestCounter;

    [System.Serializable]
    public class CurriculumTestData
    {
        public bool enabled;
        public TextAsset gameData;
    }
    
    [System.Serializable]
    public class ListOfRequestTest
    {
        public List<RequestTest> list;

        [System.Serializable]
        public class RequestTest
        {
            [Tooltip("Ha ez be van jelölve, akkor a command-nál megadott lekérdezésre a válasz a answerData-ban megadott json lesz.")]
            public bool enabled = true;
            public string command;
            [Tooltip("Elmenti a szerverről letöltött adatokat egy a kéréssel azonos nevű fájlba")]
            public bool saveToFile;
            public bool continuously;
            public int nextIndex;
            public bool nextIndexIsValid;
            public List<AnswerData> answerDatas;

            public TextAsset GetNext()
            {
                ValidateIndex();

                TextAsset result = null;
                if (nextIndexIsValid)
                    result = answerDatas[nextIndex].answer;

                if (continuously) nextIndex++;
                ValidateIndex();

                return result;
            }

            // Megvizsgálja, hogy az aktuális index engedélyezett elemre mutat-e, 
            // ha nem így van, akkor megkeresei a sorban a következő engedélyezett elemet.
            // Ha nincs ilyen, akkor a nextIndexIsValid változót false-ra állítja.
            void ValidateIndex()
            {
                List<bool> enabledList = new List<bool>();

                for (int i = 0; i < answerDatas.Count; i++)
                {
                    enabledList.Add(answerDatas[i].enabled);
                }

                int newIndex;
                nextIndexIsValid = ValidateIndex(enabledList, nextIndex, out newIndex);
                nextIndex = newIndex;
                
                /*
                if (nextIndex >= answerDatas.Count)
                    nextIndex = 0;

                int searchIndex = nextIndex;

                do
                {
                    if (answerDatas[searchIndex].enabled)
                    {
                        nextIndex = searchIndex;
                        nextIndexIsValid = true;
                        return;
                    }

                    searchIndex++;

                    if (searchIndex >= answerDatas.Count)
                        searchIndex = 0;
                } while (searchIndex != nextIndex);

                nextIndexIsValid = false;
                */
            }

            static public bool ValidateIndex(List<bool> indexes, int nextIndex, out int newIndex)
            {
                if (indexes.Count == 0)
                {
                    newIndex = nextIndex;
                    return false;
                }

                if (nextIndex >= indexes.Count)
                    nextIndex = 0;

                newIndex = nextIndex;

                do
                {
                    if (indexes[newIndex])
                    {
                        nextIndex = newIndex;
                        return true;
                    }

                    newIndex++;

                    if (newIndex >= indexes.Count)
                        newIndex = 0;
                } while (newIndex != nextIndex);

                return false;
            }

            [System.Serializable]
            public class AnswerData
            {
                public bool enabled;
                public TextAsset answer;
            }
        }

        /// <summary>
        /// A megadott commandhoz van válasz amit offline kell használni.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Ha a válasz nem null, akkor ott a válasz</returns>
        public JSONNode IsAnswer(string command)
        {
            // Megkeressük a parancshoz tartozó adatokat
            RequestTest requestTest = null;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].command == command)
                    requestTest = list[i];
            }

            // Ha van és engedélyezve van és offline munka üzemmódba vagyunk, akkor visszaadjuk a választ
            if (requestTest != null && requestTest.enabled && Common.configurationController.offlineWork && Common.configurationController.editor)
            {
                TextAsset result = requestTest.GetNext();
                if (result != null)
                    return JSON.Parse(result.text);
            }

            return null;
        }

        public bool AnswerNeedSave(string command)
        {
            foreach (RequestTest request in list)
            {
                if (request.command == command && request.saveToFile)
                    return true;
            }

            return false;
        }
    }

    static public ClassYServerCommunication instance;

    public string sessionToken = "rBBueiYrFPWuxkaG2AFxOY4ajSQHn9Xt"; // A belépett felhasználó ideiglenes azonosítója

    //[Tooltip("Ha tesztelni szeretnénk egy játékok, akkor ezt bejelölve egy olyan tananyag fog betöltődni, ami az alábbi játékokat tartalmazza!")]
    //public bool curriculumTest;



    [Tooltip("A tananyag alapja amibe a játékokat tenni kell!")]
    [HideInInspector]
    public TextAsset curriculumMain;

    [HideInInspector]
    public List<CurriculumTestData> ListOfGameData;

    [Tooltip("Az itt megadott könyvtárba menti a szerver válaszait a lekérdezéssel azonos névvel")]
    public string fileDir;

    public ListOfRequestTest listOfRequestTest;

    void Awake() {
        instance = this;

        // A kulture infót beállítjuk, hogy a float.ToString mindig pontot tegyen a szövegbe és ne vesszőt, mert a szerver pontot szeretne
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        Debug.Log("Kulture name : " + System.Globalization.CultureInfo.InvariantCulture.EnglishName);
    }

    public void LoginEmail(string emailAddress, string password, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.userLogin;
        jsonClass[C.JSONKeys.LoginEmail] = emailAddress;
        jsonClass[C.JSONKeys.loginPassword] = Common.configurationController.isServer2020 ? password : Common.GetMd5Hash(password);
        Debug.Log("EZ AZ APP ID: " + Common.configurationController.appID.ToString());
        jsonClass[C.JSONKeys.appID] = Common.configurationController.appID.ToString();

        // Belépésnél elküldünk egy kevés rendszer információt is
        JSONClass duck = new JSONClass();
        jsonClass[C.JSONKeys.duck] = duck;

        //duck[C.JSONKeys.graphicsDeviceName] = SystemInfo.graphicsDeviceName;
        //duck["graphicsDeviceVendor"] = SystemInfo.graphicsDeviceVendor;
        //duck["graphicsDeviceVersion"] = SystemInfo.graphicsDeviceVersion;
        //duck["graphicsMemorySize"] = SystemInfo.graphicsMemorySize.ToString();
        //duck["maxTextureSize"] = SystemInfo.maxTextureSize.ToString();
        //
        //duck[C.JSONKeys.deviceModel] = SystemInfo.deviceModel;
        //duck[C.JSONKeys.operatingSystem] = SystemInfo.operatingSystem;
        //duck["systemMemorySize"] = SystemInfo.systemMemorySize.ToString();
        //
        //duck["processorCount"] = SystemInfo.processorCount.ToString();
        //duck["processorFrequency"] = SystemInfo.processorFrequency.ToString();
        //duck["processorType"] = SystemInfo.processorType;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                //Debug.Log(response.ToString(" "));

                if (success) {
                    sessionToken = response[C.JSONKeys.answer];
                    Common.configurationController.Save();
                }

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void ForgotPassword(string loginEmail, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.userForgotPassword;

        jsonClass[C.JSONKeys.LoginEmail] = loginEmail;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void EmailRegistration(string userName, string password, string passwordAgain, string emailAddress, bool errorHandling, Common.CallBack_In_Bool callBack = null) {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.userRegistration;

        jsonClass[C.JSONKeys.userName] = userName.Split('@')[0];
        jsonClass[C.JSONKeys.loginPassword] = password;
        jsonClass[C.JSONKeys.reNewPassword] = passwordAgain;
        jsonClass[C.JSONKeys.LoginEmail] = emailAddress;
        jsonClass[C.JSONKeys.langCode] = LanguageHelper.Get2LetterISOCodeFromSystemLanguage();

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success);
            }
            );
    }

    /// <summary>
    /// Lekérdezzük a szerveren található nyelvi fájlokat. (A lehetséges felhasználói felületek nyelvei)
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="errorHandling"></param>
    /// <param name="callBack"></param>
    public void GetSupportedLanguages(bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getSupportedLanguages;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            },
            true
            );
    }

    public void getAllSupportedLanguagesData(bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getAllSupportedLanguagesData;
        jsonClass[C.JSONKeys.frontendVersion] = "app";

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            },
            true
            );
    }

    /// <summary>
    /// A megadott nyelv fordításait letöltjük a szerverről
    /// </summary>
    /// <param name="language"></param>
    /// <param name="errorHandling"></param>
    /// <param name="callBack"></param>
    public void GetLanguageData(string language, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getLanguageData;
        jsonClass[C.JSONKeys.language] = language;
        jsonClass[C.JSONKeys.frontendVersion] = "app";

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            },
            true
            );
    }

    /*
    {
        "error":false,
        "answer":[
            {
                "id":"62",
                "name":"angol",
                "languageID":"26",
                "code":"HU"
            },
        ],
    }
    */
    /// <summary>
    /// Azoknak a nyelveknek a listáját kapjuk meg amilyen nyelvű csoportokba tagok vagyunk.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="errorHandling"></param>
    /// <param name="callBack"></param>
    public void GetUsableLanguages(string scope, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getUsableLanguages;
        jsonClass[C.JSONKeys.scope] = scope;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Azoknak a nyelveknek a listáját kapjuk meg amilyen nyelven vannak publikus email csoportok.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="errorHandling"></param>
    /// <param name="callBack"></param>
    public void getPublicMailListsLanguages(string scope, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getPublicMailListsLanguages;
        jsonClass[C.JSONKeys.scope] = scope;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void GetCurriculumSubjects(string scope, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getCurriculumSubjects;
        jsonClass[C.JSONKeys.searchString] = "";
        jsonClass[C.JSONKeys.scope] = scope;
        jsonClass[C.JSONKeys.langCode] = Common.configurationController.curriculumLang.langCode;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                callBack(success, response);
            }
            );
    }

    public void GetCurriculumTopics(string subjectID, string scope, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getCurriculumTopics;
        jsonClass[C.JSONKeys.subjectID] = subjectID;
        jsonClass[C.JSONKeys.searchString] = "";
        jsonClass[C.JSONKeys.scope] = scope;
        jsonClass[C.JSONKeys.langCode] = Common.configurationController.curriculumLang.langCode;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void GetCurriculumCourses(string topicID, string scope, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getCurriculumCourses;
        jsonClass[C.JSONKeys.topicID] = topicID;
        jsonClass[C.JSONKeys.searchString] = "";
        jsonClass[C.JSONKeys.scope] = scope;
        jsonClass[C.JSONKeys.langCode] = Common.configurationController.curriculumLang.langCode;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void GetCurriculums(string courseID, string scope, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getCurriculums;
        jsonClass[C.JSONKeys.courseID] = courseID;
        jsonClass[C.JSONKeys.searchString] = "";
        jsonClass[C.JSONKeys.scope] = scope;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    // command : 'getSubjectsByMailList'
    //  -> mailListID
    public void GetSubjectsByMailList(string mailListID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getSubjectsByMailList;
        jsonClass[C.JSONKeys.mailListID] = mailListID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    // command : 'getTopicsByMailList'
    //  -> mailListID
    //  -> subjectID
    public void GetTopicsByMailList(string mailListID, string subjectID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getTopicsByMailList;
        jsonClass[C.JSONKeys.mailListID] = mailListID;
        jsonClass[C.JSONKeys.subjectID] = subjectID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    // command : 'getCoursesByMailList'
    //  -> mailListID
    //  -> topicID
    public void GetCoursesByMailList(string mailListID, string topicID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getCoursesByMailList;
        jsonClass[C.JSONKeys.mailListID] = mailListID;
        jsonClass[C.JSONKeys.topicID] = topicID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    // command : 'getCurriculumsByMailList'
    //  -> mailListID
    //  -> courseID
    public void GetCurriculumsByMailList(string mailListID, string CourseID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getCurriculumsByMailList;
        jsonClass[C.JSONKeys.mailListID] = mailListID;
        jsonClass[C.JSONKeys.courseID] = CourseID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void GetCurriculumItems(string curriculumID, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getCurriculumItems;
        jsonClass[C.JSONKeys.curriculumID] = curriculumID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }
    
    public void getMarketModuleItems(bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getMarketModuleItems;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /*
    public void getMarketModuleItems(bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getMarketModuleItems;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (success)
                    getMarketAchievements(errorHandling,
                        (bool success2, JSONNode response2) =>
                        {
                            response[C.JSONKeys.answer][C.JSONValues.getMarketAchievements] = response2[C.JSONKeys.answer];

                            if (callBack != null)
                                callBack(success, response);
                        });
                else
                    if (callBack != null)
                    callBack(success, response);
            }
            );
    }
    */

    public void getMarketModuleItemsAll(bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        StartCoroutine(getMarketModuleItemsAllCoroutine(errorHandling, callBack));
    }

    public IEnumerator getMarketModuleItemsAllCoroutine(bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        bool wait = true;
        bool success = false;
        JSONNode response = null;

        getMarketModuleItems(errorHandling,
            (bool success1, JSONNode response1) =>
            {
                success = success1;
                response = response1;
                wait = false;
            });

        while (wait) { yield return null; };

        if (success)
        {
            wait = true;
            getMarketAchievements(errorHandling,
                (bool success2, JSONNode response2) =>
                {
                    success = success2;
                    response[C.JSONKeys.answer][C.JSONValues.getMarketAchievements] = response2[C.JSONKeys.answer];
                    wait = false;
                });
        }

        while (wait) { yield return null; };

        if (success)
        {
            wait = true;
            getWebshopItems(errorHandling,
                (bool success2, JSONNode response2) =>
                {
                    success = success2;
                    response[C.JSONKeys.answer][C.JSONValues.getWebshopItems] = response2[C.JSONKeys.answer];
                    wait = false;
                });
        }

        while (wait) { yield return null; };

        if (callBack != null)
            callBack(success, response);
    }





    /// <summary>
    /// Lekérdezzük, hogy a felhasználó milyen eredményt ért el a MarketPlace2021-ben
    /// </summary>
    /// <param name="errorHandling"></param>
    /// <param name="callBack"></param>
    public void getMarketAchievements(bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getMarketAchievements;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Elküldjük a szervernek, hogy a felhasználó hová szeretné a Pcoin-jait befektetni.
    /// </summary>
    /// <param name="marketModuelItemID">A termék azonosítója amibe befektetünk.</param>
    /// <param name="callBack"></param>
    public void SetUserInvestment(string marketModuelItemID ,Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.setUserInvestment;
        jsonClass[C.JSONKeys.marketModuleItemID] = marketModuelItemID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void getWebshopItems(bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getWebshopItems;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void purchaseWebshopItem(string webShopItemID, int quantity, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.purchaseWebshopItem;
        jsonClass[C.JSONKeys.webshopItemID] = webShopItemID;
        jsonClass[C.JSONKeys.quantity].AsInt = quantity;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Egy játék adatait kéri le a szervertől.
    /// </summary>
    /// <param name="gameID"></param>
    /// <param name="callBack"></param>
    public void GetGameForPlay(string gameID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getGameForPlay;
        jsonClass[C.JSONKeys.gameID] = gameID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void GetCurriculumForPlay(string learnRoutePathID, string learnRoutePathStart, string courseID, string curriculumID, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        // Ha egy json formában megadott tananyagot akarunk tesztelni, akkor minden tananyag letöltésnél azt a tananyagot kapjuk vissza
        if (Common.configurationController.offlineWork)
        {
            if (callBack != null)
            {
                Debug.Log("GetCurriculumForPlay test answer");

                // Létrehozzuk a tananyagot a megadott játékokból
                JSONNode curriculumJson = JSON.Parse(curriculumMain.text);

                JSONNode plannedGamesJson = curriculumJson[C.JSONKeys.answer][C.JSONKeys.plannedGames];

                foreach (var item in ListOfGameData)
                {
                    if (item.enabled && item.gameData)
                        plannedGamesJson.Add(JSON.Parse(item.gameData.text));
                }

                //callBack(true, JSON.Parse(Common.configurationController.CurriculumTestData.text));
                callBack(true, curriculumJson);
            }

            return;
        }

        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getCurriculumForPlay;

        jsonClass[C.JSONKeys.learnRoutePathID] = learnRoutePathID;
        jsonClass[C.JSONKeys.learnRoutePathStart] = learnRoutePathStart;

        jsonClass[C.JSONKeys.courseID] = courseID;
        jsonClass[C.JSONKeys.curriculumID] = curriculumID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /*
    {
		command : 'addGameLog',
		subjectID : 'subjectID',
		topicID : 'topicID',
		courseID : 'courseID',
		curriculumID : 'curriculumID',
		gameID : 'gameID',
		gamePercent : '000.000',
		gameStart : 'TIMESTAMP',
		gameEnd : 'TIMESTAMP',
        curriculumStart : 'TIMESTAMP',
		curriculumProgress : '000.000',
		* sessionToken : '...'
	}
    */
    public void addGameLog(
        string learnRoutePathID, string learnRoutePathStart,
        string subjectID, string topicID, string courseID, string curriculumID, string gameID,
        float gamePercent, string gameStart, string gameEnd, JSONNode extraData,
        string curriculumStart, float curriculumProgress, 
        bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        //return;

        // Ha egy json formában megadott tananyagot tesztelünk, akkor nem küldünk game logot
        if (Common.configurationController.offlineWork)
        {
            Debug.LogWarning("!!!!! Game log letiltva tananyag teszt miatt !!!!!");

            if (callBack != null)
                callBack(true, JSON.Parse("{}"));

            return;
        }

        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.addGameLog;

        jsonClass[C.JSONKeys.learnRoutePathID] = learnRoutePathID;
        jsonClass[C.JSONKeys.learnRoutePathStart] = learnRoutePathStart;

        jsonClass[C.JSONKeys.subjectID] = subjectID;
        jsonClass[C.JSONKeys.topicID] = topicID;
        jsonClass[C.JSONKeys.courseID] = courseID;
        jsonClass[C.JSONKeys.curriculumID] = curriculumID;
        jsonClass[C.JSONKeys.gameID] = gameID;

        jsonClass[C.JSONKeys.gamePercent].AsFloat = gamePercent;
        jsonClass[C.JSONKeys.gameStart] = gameStart;
        jsonClass[C.JSONKeys.gameEnd] = gameEnd;

        if (extraData != null)
            jsonClass[C.JSONKeys.extraData] = extraData;

        jsonClass[C.JSONKeys.curriculumStart] = curriculumStart;
        jsonClass[C.JSONKeys.curriculumProgress].AsFloat = curriculumProgress;

        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /*
{
    command : 'addGameLog',
    subjectID : 'subjectID',
    topicID : 'topicID',
    courseID : 'courseID',
    curriculumID : 'curriculumID',
    gameID : 'gameID',
    gamePercent : '000.000',
    gameStart : 'TIMESTAMP',
    gameEnd : 'TIMESTAMP',
    curriculumStart : 'TIMESTAMP',
    curriculumProgress : '000.000',
    * sessionToken : '...'
}
*/
    public void addGameLogServer2020(
        JSONNode pathData,
        JSONNode screensEvaluations,
        string gameID,
        float gamePercent, string gameStart, string gameEnd, JSONNode extraData,
        string gameTheme, string gameEnding,
        bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.addGameLog;

        jsonClass[C.JSONKeys.gameID] = gameID;

        jsonClass[C.JSONKeys.gamePercent].AsFloat = float.IsNaN(gamePercent) ? 0 : gamePercent;
        jsonClass[C.JSONKeys.gameStart] = gameStart;
        jsonClass[C.JSONKeys.gameEnd] = gameEnd;

        if (extraData != null)
            jsonClass[C.JSONKeys.extraData] = extraData;

        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        jsonClass[C.JSONKeys.pathData] = pathData;
        jsonClass[C.JSONKeys.screens] = screensEvaluations;

        // Vissza küldjük a gameTheme és a gameEnding értékeket
        jsonClass[C.JSONKeys.gameData][C.JSONKeys.gameTheme] = gameTheme;
        jsonClass[C.JSONKeys.gameData][C.JSONKeys.gameEnding] = gameEnding;

        // Ha egy json formában megadott tananyagot tesztelünk, akkor nem küldünk game logot
        if (Common.configurationController.offlineWork || Common.configurationController.gameLogDisabled)
        {
            Debug.LogWarning("!!!!! Game log letiltva tananyag teszt miatt !!!!!");
            Debug.Log(jsonClass.ToString(" "));

            if (callBack != null)
                callBack(true, JSON.Parse("{}"));

            return;
        }

        SendMessageToServer(jsonClass, errorHandling,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }



    /// <summary>
    /// === Server2020 ===
    /// </summary>
    /// <param name="callBack"></param>
    public void GetPlayableLearnRoutePathList(string emailGroupID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getPlayableLearnRoutePathList;
        jsonClass[C.JSONKeys.mailListID] = emailGroupID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void GetLearnRoutePathForPlay(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getLearnRoutePathForPlay;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// === Server2020 ===
    /// Lekédezi a következő lejátszandó játékot az útvonalban
    /// </summary>
    /// <param name="learnRoutePathID">Útvonal azonosítója</param>
    /// <param name="mailListID">Email lista azonosítója</param>
    /// <param name="isolationTime">Az útvonal utolsó indításának ideje</param>
    /// <param name="callBack">Hova küldje a szerver válaszát</param>
    public void GetNextGame(string learnRoutePathID, string mailListID, string isolationTime, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getNextGame;

        jsonClass[C.JSONKeys.learnRoutePathID] = learnRoutePathID;
        jsonClass[C.JSONKeys.mailListID] = mailListID;
        jsonClass[C.JSONKeys.learnRoutePathStart] = isolationTime;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// === Server2020 ===
    /// Lekérdezi egy korrábban már loggolt játékot a log azonosítója alapján
    /// </summary>
    /// <param name="learnRoutePathID">Útvonal azonosítója</param>
    /// <param name="mailListID">Email lista azonosítója</param>
    /// <param name="isolationTime">Az útvonal utolsó indításának ideje</param>
    /// <param name="callBack">Hova küldje a szerver válaszát</param>
    public void GetLoggedGame(string learnRoutePathID, string mailListID, string logID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getLoggedGame;

        jsonClass[C.JSONKeys.learnRoutePathID] = learnRoutePathID;
        jsonClass[C.JSONKeys.mailListID] = mailListID;
        jsonClass[C.JSONKeys.logID] = logID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// === Server2020 ===
    /// Lekédezi a következő lejátszandó játékot a tananyagban
    /// </summary>
    public void GetNextPracticeGame(string subjectID, string topicID, string courseID, string curriculumID, string curriculumStart, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getNextPracticeGame;

        jsonClass[C.JSONKeys.subjectID] = subjectID;
        jsonClass[C.JSONKeys.topicID] = topicID;
        jsonClass[C.JSONKeys.courseID] = courseID;
        jsonClass[C.JSONKeys.curriculumID] = curriculumID;
        jsonClass[C.JSONKeys.curriculumStart] = curriculumStart;

        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// === Server2020 ===
    /// Lekérdez egy korrábban már loggolt játékot a tananyagban
    /// </summary>
    public void GetPracticeLoggedGame(string courseID, string curriculumID, string logID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getPracticeLoggedGame;

        jsonClass[C.JSONKeys.courseID] = courseID;
        jsonClass[C.JSONKeys.curriculumID] = curriculumID;
        jsonClass[C.JSONKeys.logID] = logID;

        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// === Server2020 ===
    /// Lekérdez egy játékot a szerveren kipróbálásra a böngészőbe. Ez akkor van amikor egy játékot készítenek és ki szeretnék próbnálni
    /// </summary>
    /// <param name="testToken"></param>
    /// <param name="callBack"></param>
    public void getGameForTest(string testToken, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getGameForTest;

        jsonClass[C.JSONKeys.testToken] = testToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// === Server2020 ===
    /// Lekérdez egy játékot a szerveren kipróbálásra a böngészőbe. Ez akkor van amikor egy játékot készítenek és ki szeretnék próbnálni
    /// </summary>
    /// <param name="replayToken"></param>
    /// <param name="callBack"></param>
    public void getReplayData(string replayToken, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getReplayData;

        jsonClass[C.JSONKeys.replayToken] = replayToken;

        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Lekérdezi az utolsó X játszott tananyagot.
    /// </summary>
    /// <param name="callBack"></param>
    public void GetLastX(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getLastX;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Lekérdezi azokat a tananyagokat amelyek nem értek el egy bizonyos százalékot.
    /// </summary>
    /// <param name="callBack"></param>
    public void GetUnderXPercent(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getUnderXPercent;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Lekérdezi a liveStream adatait.
    /// </summary>
    /// <param name="callBack"></param>
    public void GetStreamData(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getStreamData;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /*
command      : 'getUsableLanguages'
mas nem kell neki

command      : 'getListOfMailLists',
where        : 'appMyGroupsEdit' vagy 'appMyGroups', (elvileg a 2 json amit kuldtel, az alapjan van elnevezve)
searchString : 'kacsaaaaaaaa',
languageID   : x

command      : 'getUserMailListInfo',
mailListID   : y
base64Image  : true/false
    */

    public void GetUserMailListInfo(string emailListID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getUserMailListInfo;
        jsonClass[C.JSONKeys.mailListID] =  emailListID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Csoportjaim szerkesztése menüponthoz.
    /// </summary>
    /// <param name="callBack"></param>
    public void appMainPageGroupBrowser(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getListOfMailLists;
        jsonClass[C.JSONKeys.where] = C.JSONValues.appMainPageGroupBrowser;
        jsonClass[C.JSONKeys.languageID] = Common.configurationController.publicEmailGroupsLang.langID;

        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Csoportjaim szerkesztése menüponthoz.
    /// </summary>
    /// <param name="callBack"></param>
    public void appMyGroupsEdit(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getListOfMailLists;
        jsonClass[C.JSONKeys.where] =   C.JSONValues.appMyGroupsEdit;
        jsonClass[C.JSONKeys.languageID] = Common.configurationController.publicEmailGroupsLang.langID;

        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void getPlayRoutes(string languageID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        List<EmailGroupListRequestData> list = new List<EmailGroupListRequestData>();
        list.Add(new EmailGroupListRequestData(C.JSONValues.getWhereIAmOnMailLists));

        StartCoroutine(GetGroupEmailListsCoroutine(C.JSONValues.getPlayRoutes, list, languageID, callBack));
    }

    public void getMyGroupsEmailLists(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        List<EmailGroupListRequestData> list = new List<EmailGroupListRequestData>();
        list.Add(new EmailGroupListRequestData(C.JSONValues.getInvitedMailLists, color: "#B89AA0"));
        list.Add(new EmailGroupListRequestData(C.JSONValues.getPublicMailLists, bigItems: true));

        StartCoroutine(GetGroupEmailListsCoroutine(C.JSONValues.getMyGroupsEmailLists, list, "", callBack));
    }

    public void getMyGroupEditEmailLists(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        List<EmailGroupListRequestData> list = new List<EmailGroupListRequestData>();
        list.Add(new EmailGroupListRequestData(C.JSONValues.getOwnMailLists));
        list.Add(new EmailGroupListRequestData(C.JSONValues.getWhereIAmAdminMailLists));
        list.Add(new EmailGroupListRequestData(C.JSONValues.getWhereIHavePermissionMailLists));
        list.Add(new EmailGroupListRequestData(C.JSONValues.getWhereIAmOnMailLists));
        list.Add(new EmailGroupListRequestData(C.JSONValues.getPublicMailLists));
        list.Add(new EmailGroupListRequestData(C.JSONValues.getInvitedMailLists));
        list.Add(new EmailGroupListRequestData(C.JSONValues.getSubPendingMailLists));

        StartCoroutine(GetGroupEmailListsCoroutine(C.JSONValues.getMyGroupEditEmailLists, list, "", callBack));
    }

    /// <summary>
    /// A lekérdezések eredményét a json-ba gyúrja.
    /// 
    /// {
    ///     "error" : "false",
    ///     "answer" : [
    ///         {
    ///             "requestName" : "getWhereIAmOnMailLists",
    ///             "answer" : 
    ///         },
    ///         {
    ///             "requestName" : "getWhereIAmOnMailLists",
    ///             "answer" : 
    ///         },
    ///         {
    ///             "requestName" : "getWhereIAmOnMailLists",
    ///             "answer" : 
    ///         },
    ///         {
    ///             "requestName" : "getWhereIAmOnMailLists",
    ///             "answer" : 
    ///         }
    ///     ]
    /// }
    /// 
    /// </summary>
    /// <param name="callBack"></param>
    public IEnumerator GetGroupEmailListsCoroutine(string requestName, List<EmailGroupListRequestData> requestList, string languageID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        // Ha teszt üzemmódban vagyunk, akkor nem a szervertől várjuk a választ
        JSONNode answerIsImmediatelly = listOfRequestTest.IsAnswer(requestName);
        // Ha van mit azonnal válaszolni, akkor válaszolunk
        if (answerIsImmediatelly != null)
        {
            Debug.Log(requestName + " - OFFLINE ANSWER");

            Debug.Log(answerIsImmediatelly.ToString(" "));

            // Vissza hívjuk a hívót
            if (callBack != null)
                callBack(!answerIsImmediatelly[C.JSONKeys.error].AsBool, answerIsImmediatelly);

            yield break;
        }

        // Valóban le kell töltenünk a listákat
        ServerBusy.instance.Show();

        JSONArray jsonArray = new JSONArray();
        bool success = false;

        for (int i = 0; i < requestList.Count; i++)
        {
            JSONClass jsonClass = new JSONClass();
            jsonClass[C.JSONKeys.command] = requestList[i].requestName;
            jsonClass[C.JSONKeys.sessionToken] = sessionToken;

            if (!string.IsNullOrEmpty(languageID))
                jsonClass[C.JSONKeys.languageID] = languageID;

            bool ready = false;
            success = false;
            JSONNode response = null;

            SendMessageToServer(jsonClass, true,
                (bool success2, JSONNode response2) => {

                    success = success2;
                    response = response2;
                    ready = true;
                },
                true
                );

            while (!ready) yield return null;

            if (success)
            {
                JSONClass json = new JSONClass();
                json[C.JSONKeys.requestName] = requestList[i].requestName;
                json[C.JSONKeys.color] = requestList[i].color;
                json[C.JSONKeys.bigItems].AsBool = requestList[i].bigItems;

                json[C.JSONKeys.answer] = response[C.JSONKeys.answer];

                jsonArray.Add(json);
            }
            else break;
        }

        JSONClass jsonClassResult = new JSONClass();
        jsonClassResult[C.JSONKeys.error].AsBool = !success;
        jsonClassResult[C.JSONKeys.answer] = jsonArray;

        ServerBusy.instance.Hide();

        Debug.Log("EMAIL LISTS\n" + jsonClassResult.ToString(" "));

        SaveIfNeed(requestName, jsonClassResult);

        if (callBack != null)
            callBack(success, jsonClassResult);
    }

    //    // azok a listak amiknek tagja vagyok
    //    coreData: {
    //      duck        : common.duck,
    //      command     : 'getWhereIAmOnMailLists',
    //      searchString: searchString
    //    }
        public void GetHomeworks(string mailListID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getHomeworks;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;
        jsonClass[C.JSONKeys.mailListID] = mailListID;

        //jsonClass["searchString"] = "";

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void GetWhereIAmOnMailLists(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getWhereIAmOnMailLists;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;
        jsonClass[C.JSONKeys.languageID] = Common.configurationController.publicEmailGroupsLang.langID;

        //jsonClass["searchString"] = "";

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Feliratkozás egy email listára
    /// </summary>
    /// <param name="mailListID"></param>
    /// <param name="description">Miért akarunk feliratkozni, meg ilyenek</param>
    /// <param name="callBack"></param>
    public void SubToMailList(string mailListID, string description, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.subToMailList;
        jsonClass[C.JSONKeys.mailListID] = mailListID;
        jsonClass[C.JSONKeys.description] = description;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Leliratkozás egy email listáról
    /// </summary>
    /// <param name="mailListID"></param>
    /// <param name="description">Miért akarunk feliratkozni, meg ilyenek</param>
    /// <param name="callBack"></param>
    public void UnsubFromMailList(string mailListID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.unsubFromMailList;
        jsonClass[C.JSONKeys.mailListID] = mailListID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Feliratkozás egy email listára
    /// </summary>
    /// <param name="mailListID"></param>
    /// <param name="description">Miért akarunk feliratkozni, meg ilyenek</param>
    /// <param name="callBack"></param>
    public void ForceAnswerInvitation(string invitationID, bool invitationAnswer, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.forceAnswerInvitation;
        jsonClass[C.JSONKeys.invitationID] = invitationID;
        jsonClass[C.JSONKeys.invitationAnswer].AsBool = invitationAnswer;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// Törli a felhasználó játék logjait. (Tesztelési célból lett elkészítve)
    /// </summary>
    /// <param name="callBack"></param>
    public void EraseUserLog(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.eraseUserLog;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void CreateFatalError(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = "createFatalError";
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void GetUserBonusCoins(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getUserBonusCoins;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void GetUserInventory(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getUserInventory;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }


    /*
     * 
setFrameGameCharacters
-> learnRoutePathID
-> mailListID
-> heroID
-> monsterID
-> victimID

     * 
     */
    public void SetFrameGameCharacters(string learnRoutePathID, string mailListID, string heroID, string monsterID, string victimID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.setFrameGameCharacters;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;
        jsonClass[C.JSONKeys.learnRoutePathID] = learnRoutePathID;
        jsonClass[C.JSONKeys.mailListID] = mailListID;
        jsonClass[C.JSONKeys.heroID] = heroID;
        jsonClass[C.JSONKeys.monsterID] = monsterID;
        jsonClass[C.JSONKeys.victimID] = victimID;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameID">A játék id-ja később lehet több játék is.</param>
    /// <param name="callBack"></param>
    public void PurchaseBonusGame(string gameID, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.purchaseBonusGame;
        jsonClass[C.JSONKeys.bonusGameID] = gameID;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="purchasedID">A megvett játék azonosítója</param>
    /// <param name="score">elért pontszám</param>
    /// <param name="level">szint</param>
    /// <param name="callBack"></param>
    public void updateBonusGameLog(string purchasedID, int score, int level, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.updateBonusGameLog;
        jsonClass[C.JSONKeys.bonusGamePurchaseID] = purchasedID;
        jsonClass[C.JSONKeys.bonusGameLogData][C.JSONKeys.gameScore].AsInt = score;
        jsonClass[C.JSONKeys.bonusGameLogData][C.JSONKeys.gameLevel].AsInt = level;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void getShareTokenData(string shareToken, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getShareTokenData;
        jsonClass[C.JSONKeys.shareToken] = shareToken;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void sendBugReport(string comment, string image, JSONNode gameJson, Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        // Eltávolítjuk ezeket a gameData-ból mert ezek base64-el kódolt dolgok és nagyon nagyok
        Common.RemoveKeysFromJson(gameJson, "pdf", "images");

        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.sendBugReport;
        jsonClass[C.JSONKeys.comment] = comment;
        jsonClass[C.JSONKeys.image] = image;
        jsonClass[C.JSONKeys.gameJson] = gameJson.ToString();
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void getUserProfile(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getUserProfile;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (success)
                {
                    Common.configurationController.playAnimations = response[C.JSONKeys.answer][C.JSONKeys.appSettings][C.JSONKeys.playAnimations].AsBool;
                    Common.configurationController.statusTableInSuper = response[C.JSONKeys.answer][C.JSONKeys.appSettings][C.JSONKeys.statusTableInSuper].AsBool;
                    Common.configurationController.statusTableBetweenSuper = response[C.JSONKeys.answer][C.JSONKeys.appSettings][C.JSONKeys.statusTableBetweenSuper].AsBool;
                }

                if (callBack != null)
                    callBack(success, response);
            }
            );
    }

    public void getFamilyConnections(Common.CallBack_In_Bool_JSONNode callBack = null)  //getFamilyConnections Ádám
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getFamilyConnections;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
        );
    }

    public void getCurrentHomeworks(Common.CallBack_In_Bool_JSONNode callBack = null)  //getCurrentHomeworks Ádám
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.getCurrentHomeworks;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            }
        );
    }

    public void saveAppSettings(Common.CallBack_In_Bool_JSONNode callBack = null)
    {
        JSONClass jsonClass = new JSONClass();
        jsonClass[C.JSONKeys.command] = C.JSONValues.saveAppSettings;
        jsonClass[C.JSONKeys.playAnimations].AsBool = Common.configurationController.playAnimations;
        jsonClass[C.JSONKeys.statusTableInSuper].AsBool = Common.configurationController.statusTableInSuper;
        jsonClass[C.JSONKeys.statusTableBetweenSuper].AsBool = Common.configurationController.statusTableBetweenSuper;
        jsonClass[C.JSONKeys.sessionToken] = sessionToken;

        SendMessageToServer(jsonClass, true,
            (bool success, JSONNode response) => {

                if (callBack != null)
                    callBack(success, response);
            },
            mustSuccess: false
            );
    }

    /// <summary>
    /// Hozzá csapunk a küldendő json-hoz néhány adatot. (alkalmazás azonosító pl. Storie : , alkalmazás verzió száma, push üzenet küldéséhez adatok)
    /// </summary>
    /// <param name="json"></param>
    /// <param name="errorHandling"></param>
    /// <param name="callBack"></param>
    /// <param name="silent">Nem ad visszajelzést a kommunikáció sikertelenségéről</param>
    /// <param name="mustSuccess">A szerver kommunikációnak muszáj sikerülnie, ha nem sikerült újra küldi</param>
    public void SendMessageToServer(JSONClass json, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null, bool silent = false, bool mustSuccess = false) {
        json[C.JSONKeys.thisIsTablet] = "dcf5de6bef6d4ff370f98300b224f253abdf682cd72be29d59f7495cce4d0183";

        // Megnézzük, hogy létre van-e hozva már egy duck osztály a json-ban
        // Login-nál már létrehozzuk
        JSONClass duck;
        if (!json.ContainsKey(C.JSONKeys.duck))
        {
            // Ha nincs létrehozzuk
            duck = new JSONClass();
            json[C.JSONKeys.duck] = duck;
        }
        else
        {
            // Ha már létre van hozva, akkor azt használjuk
            duck = json[C.JSONKeys.duck] as JSONClass;
        }

        duck[C.JSONKeys.duckID] = Common.configurationController.appID.ToString().ToLower(); // Alkalmazás azonosító
        duck[C.JSONKeys.duckVersion] = Common.configurationController.getVersionCode; // Alkalmazás verzió szám

        // Ha megvannak az adatok a push értesítésekhez, akkor elküldjük a szervernek
        if (Notifications.instance.pushProvider != null && Notifications.instance.userPushID != null)
        {
            duck[C.JSONKeys.pushProvider] = Notifications.instance.pushProvider;
            duck[C.JSONKeys.deviceIDforPush] = Notifications.instance.userPushID;
            duck[C.JSONKeys.deviceFingerprint] = SystemInfo.deviceUniqueIdentifier;
        }

        // Ki lettek ezek az információk törölva, mert az Android Store-nak nem felelt meg
        // Ezeknek az információknak a gyűjtéséhez tudatni kell a felhasználóval
        /*
        // Grafikus chipről információk
        duck[C.JSONKeys.graphicsDeviceName] = SystemInfo.graphicsDeviceName;
        duck["graphicsDeviceVendor"] = SystemInfo.graphicsDeviceVendor;
        duck["graphicsDeviceVersion"] = SystemInfo.graphicsDeviceVersion;
        duck["graphicsMemorySize"] = SystemInfo.graphicsMemorySize.ToString();
        duck["maxTextureSize"] = SystemInfo.maxTextureSize.ToString();
        
        // Rendszer információk
        duck[C.JSONKeys.deviceModel] = SystemInfo.deviceModel;
        duck[C.JSONKeys.operatingSystem] = SystemInfo.operatingSystem;
        duck["systemMemorySize"] = SystemInfo.systemMemorySize.ToString();
        
        // CPU-ról információk
        duck["processorCount"] = SystemInfo.processorCount.ToString();
        duck["processorFrequency"] = SystemInfo.processorFrequency.ToString();
        duck["processorType"] = SystemInfo.processorType;
        */
        Debug.Log("EZ A JSON: " + json[C.JSONKeys.thisIsTablet]);

        StartCoroutine(CoroutineSendMessageToServer(json, errorHandling, callBack, silent, mustSuccess));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="json">A szervernek küldendő adatok</param>
    /// <param name="errorHandling">Ha hiba volt, akkor ez az eljárás megjeleníti a felhasználónak a hibaüzenetet, majd mikor azt bezárta adja vissza a vezérlést a hívás helyére.</param>
    /// <param name="callBack">A szerver válaszát hová küldje</param>
    /// <param name="silent">A háttérben történik a szerver kommunikáció, nincs homokórázás amikor adatokat vár a szervertől</param>
    /// <param name="mustSuccess">A szerver kommunikációnak muszáj sikerülnie, ha nem sikerült újra küldi</param>
    /// <returns></returns>
    public IEnumerator CoroutineSendMessageToServer(JSONClass json, bool errorHandling, Common.CallBack_In_Bool_JSONNode callBack = null, bool silent = false, bool mustSuccess = false)
    {
        do
        {
            string requestCounter = $"{json[C.JSONKeys.command].Value} <<<<<<<<<< {ClassYServerCommunication.requestCounter} <<<<<<<<<< ";
            Debug.Log($"Send json : {json[C.JSONKeys.command].Value} >>>>>>>>>> {ClassYServerCommunication.requestCounter++} >>>>>>>>>> " + Common.TimeStamp() + "\n" + json.ToString(" "));

            // Ha teszt üzemmódban vagyunk, akkor nem a szervertől várjuk a választ
            JSONNode answerIsImmediatelly = listOfRequestTest.IsAnswer(json[C.JSONKeys.command]);
            // Ha van mit azonnal válaszolni, akkor válaszolunk
            if (answerIsImmediatelly != null)
            {
                Debug.Log(json[C.JSONKeys.command] + $" - OFFLINE ANSWER {requestCounter}");

                // Vissza hívjuk a hívót
                if (callBack != null)
                    callBack(!answerIsImmediatelly[C.JSONKeys.error].AsBool, answerIsImmediatelly);

                yield break;
            }

            // Ha nem tesztelünk, akkor a szervertől várjuk a választ
            if (!silent)
                ServerBusy.instance.Show();

            /*
            bool testDatabase = false;
    #if UNITY_EDITOR || UNITY_WEBGL
            testDatabase = Common.configurationController.test;
    #else
            if (Common.configurationController.loginEmailAddress == "test@classyedu.com")
                testDatabase = true;
    #endif
            WWW www = new WWW(testDatabase ? Common.configurationController.testLink : Common.configurationController.liveLink, form);
            */

            string link = Common.configurationController.getServerCommunicationLink();






            string wwwError = "";
            JSONNode jsonAnswer = null;
            string wwwText = "";

            if (Common.configurationController.isServer2020)
            {
                // Az új kommunikáció JSON alapu

                // byte[] myData = System.Text.Encoding.UTF8.GetBytes(json.ToString());
                // using (UnityWebRequest www = UnityWebRequest.Put(link, myData))
                // {
                //     www.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
                //     yield return www.SendWebRequest();

                //     if (www.result != UnityWebRequest.Result.Success)
                //     {
                //         Debug.Log(www.error);
                //     }
                //     else
                //     {
                //         Debug.Log("Upload complete!");
                //         Debug.Log(www.result);
                //     }
                // }

                UnityWebRequest www = new UnityWebRequest(link, "POST");
                www.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
                www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json.ToString()));
                www.downloadHandler = new DownloadHandlerBuffer();

                Debug.Log("Elküldjük a kérést : " + Common.TimeStamp() + "\n" + Common.configurationController.appID + " - " + link);
                Debug.Log("Várunk a szerver válaszára : " + Common.TimeStamp());
                yield return www.SendWebRequest();
                Debug.Log("Szerver válasz megérkezett : " + Common.TimeStamp());

                if (!silent)
                    ServerBusy.instance.Hide();

                wwwError = (www.isNetworkError || www.isHttpError) ? www.error : null;

                // Megvizsgáljuk, jött-e válasz
                jsonAnswer = new JSONClass();
                wwwText = (wwwError == null) ? www.downloadHandler.text : "";

                if (wwwText.Length < 10000)
                    Debug.Log(wwwText);

            }
            else
            {
                // Régi kommunikáció

                WWWForm form = new WWWForm();
                form.AddField("coreDataStringify", json.ToString());

                Debug.Log("Elküldjük a kérést : " + Common.TimeStamp() + "\n" + Common.configurationController.appID + " - " + link);
                WWW www = new WWW(link, form);

                Debug.Log("Várunk a szerver válaszára : " + Common.TimeStamp());
                yield return www; // Várunk amíg befejeződik a letöltés
                Debug.Log("Szerver válasz megérkezett : " + Common.TimeStamp());

                if (!silent)
                    ServerBusy.instance.Hide();

                wwwError = www.error;

                // Megvizsgáljuk, jött-e válasz
                jsonAnswer = new JSONClass();
                wwwText = (wwwError == null) ? www.text : "";

                if (www.text.Length < 10000)
                    Debug.Log(www.text);
            }






            if (!string.IsNullOrEmpty(wwwText))
            {
                Debug.Log("válasz feldolgozás : " + Common.TimeStamp());
                try
                {
                    jsonAnswer = JSON.Parse(wwwText);
                }
                catch (System.Exception e)
                {
                    Debug.Log("JSON Parse ERROR\n" + e.ToString());
                    jsonAnswer = null;
                }
                Debug.Log("válasz feldolgozva : " + Common.TimeStamp());

                if (jsonAnswer == null)
                {
                    jsonAnswer = new JSONClass();
                    jsonAnswer[C.JSONKeys.error].AsBool = true;
                    jsonAnswer[C.JSONKeys.answer] = wwwText;
                }

                if (wwwText.Length > 10000)
                {
                    Debug.Log($"10000 karakternél hosszabb a válasz. {requestCounter}");
                }
                else
                {
                    string response = $"Response : {requestCounter} " + Common.TimeStamp() + " \n" + jsonAnswer.ToString(" ");
                    Debug.Log("Response string elkészítve : " + Common.TimeStamp());

                    Debug.Log(response);
                    Debug.Log("Response string kiírva : " + Common.TimeStamp());
                }

                // Kitöröljük a debug információkat
                if (Common.configurationController.removeDebugInfoFromJson)
                {
                    Common.RemoveKeysFromJson(jsonAnswer, "fapapucs", "debug");
                    Debug.Log(jsonAnswer.ToString(" "));
                }

                //Debug.Log("Response : " + Common.TimeStamp() + " \n" + jsonAnswer.ToString(" "));
                //Debug.Log("Response : " + Common.TimeStamp() + " \n" + www.text);
            }
            else
            {
                jsonAnswer[C.JSONKeys.error].AsBool = true;
                jsonAnswer[C.JSONKeys.answer] = wwwError.Trim();
            }

            /*
            Lehetséges válaszok :
            invalid
            */

            Debug.LogWarning("#0 : " + Common.TimeStamp());

            if (wwwError != null)
                Debug.LogError("WWW.error : \n" + wwwError);

            Debug.Log("#1 : " + Common.TimeStamp());
            // Ha hiba volt és a hibát nem tömbben kaptam, akkor átalakítom tömb formátumuvá
            if (jsonAnswer[C.JSONKeys.error].AsBool && !(jsonAnswer[C.JSONKeys.answer] is JSONArray))
            {
                JSONArray jsonArray = new JSONArray();
                jsonArray[0] = jsonAnswer[C.JSONKeys.answer];
                jsonAnswer[C.JSONKeys.answer] = jsonArray; // [0] = jsonAnswer[C.JSONKeys.answer].Value;
            }

            Debug.Log("#2 : " + Common.TimeStamp());
            bool forth = false; // tovább
            if ((jsonAnswer[C.JSONKeys.error].AsBool && (errorHandling || mustSuccess)) || !string.IsNullOrEmpty(wwwError))
            {

                // Összeszedjük a hibaüzeneteket
                string errorText = json[C.JSONKeys.command] + ":";
                for (int i = 0; i < jsonAnswer[C.JSONKeys.answer].Count; i++)
                {
                    if (errorText != "")
                        errorText += "\n";
                    errorText += Common.languageController.Translate(jsonAnswer[C.JSONKeys.answer][i].Value);
                }

                ErrorPanel.instance.Show(errorText, Common.languageController.Translate(C.Texts.Ok), callBack: (string buttonName) =>
                {
                    ErrorPanel.instance.Hide(() =>
                    {
                        forth = true;
                    });
                });

                while (!forth) yield return null;
            }

            Debug.Log("#3 : " + Common.TimeStamp());

            // Ha a kérést azért nem tudta teljesíteni a szerver, mert bejelentkezés szükséges, akkor a bejelentkező képernyőre
            // irányítjuk a felhasználót miután elolvasta a hibaüzenetet
            if (jsonAnswer[C.JSONKeys.error].AsBool && jsonAnswer[C.JSONKeys.answer][0].Value == C.Texts.userNotLoggedIn)
            {
                // Töröljük a korábbi sessionToken-t
                ClassYServerCommunication.instance.sessionToken = "";
                Common.configurationController.Save();

                ServerBusy.instance.Hide();

                Common.screenController.LoadScreenAfterIntro();
                //Common.screenController.ChangeScreen(C.Screens.ClassYLogin);
                yield break;
            }

            SaveIfNeed(json[C.JSONKeys.command], jsonAnswer);

            // Ha szerver hiba volt a küldés során, akkor a felhasználót értesítsük, hogy újra küldjük az adatokat
            if ((jsonAnswer[C.JSONKeys.error].AsBool || !string.IsNullOrEmpty(wwwError)) && mustSuccess)
            {
                forth = false;

                ErrorPanel.instance.Show(
                    Common.languageController.Translate(C.Texts.serverCommunicationRepeat),
                    Common.languageController.Translate(C.Texts.Ok),
                    callBack: (string buttonName) =>
                    {
                        forth = true;
                    });

                // Várunk amíg a felhasználó tudomásul vesz az újra küldést
                while (!forth) yield return null;

                // Eltüntetjük az error panelt
                forth = false;
                ErrorPanel.instance.Hide(() =>
                {
                    forth = true;
                });

                // Várunk amíg az errorPanel eltűnik
                while (!forth) yield return null;
            }
            else
            {
                // Kilépünk a do-while ciklusból
                // ha nem volt hiba, vagy nem muszáj hibátlannak lennie a kommunikációnak
                Debug.Log("callBack : " + Common.TimeStamp());

                // Vissza hívjuk a hívót
                if (callBack != null)
                    callBack(!jsonAnswer[C.JSONKeys.error].AsBool, jsonAnswer);

                break;

            }

            /*
            // Kilépünk a do-while ciklusból
            // ha nem volt hiba, vagy nem muszáj hibátlannak lennie a kommunikációnak
            if (string.IsNullOrEmpty(wwwError) || !mustSuccess)
            {
                Debug.Log("callBack : " + Common.TimeStamp());

                // Vissza hívjuk a hívót
                if (callBack != null)
                    callBack(!jsonAnswer[C.JSONKeys.error].AsBool, jsonAnswer);

                break;
            }
            */

        } while (true);
    }

    public void SaveIfNeed(string command, JSONNode jsonAnswer)
    {
        // Ha a választ el kell menteni, akkor azt itt tesszük meg
#if UNITY_EDITOR_WIN
        // Megvizsgáljuk, hogy a szerver válaszát el kell-e menteni a háttértárra
        if (listOfRequestTest.AnswerNeedSave(command))
        {
            string fileName = System.IO.Path.Combine(fileDir, command + ".txt");
            System.IO.File.WriteAllText(fileName, jsonAnswer.ToString(" "));
            Debug.Log("A " + command + " lekérdezésre érkező választ elmentettük a " + fileName + " file-ba");
        }
#endif
    }


}
