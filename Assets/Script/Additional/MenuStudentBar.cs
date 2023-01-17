using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;

/*
A menuStudentBar három féle üzemmódban tud működni.
1. Fejlécként.
    Ilyenkor az a szerepe, hogy az oszlopok neveit kiírja, illetve az oszlopokra kattintva
    lehessen állítani, hogy milyen szempont szerint rendezze sorba a tanulókat.
    Ebben az objektumban kell elhelyezni a további MenuStudentBar objektumokat, amik csoport fejléc
    típusuak vagy tanuló eredmények megjelenítőként üzemelnek.
2. Csoport fejlécként.
    Ilyenkor a benne tárolt tanuló adatokat összesíti és azokat jeleníti meg egy oszlopokban.
    Az oszlop alatt pedig soronként a tanuló eredményeket.
3. Tanuló egyéni eredmény kijelző.
    Ebben az esetben pedig a tanuló elért eredményeit mutatja az oszlopokban.

A StudentBarStatus felsoroló típus tartalmazza, hogy milyen üzemmódban üzemel az objektum.
*/

public class MenuStudentBar : MonoBehaviour {

    public enum StudentBarStatus
    {
        Undefined,      // Nem meghatározott
        Header,         // Fejléc ahol ki van írva, hogy melyik oszlopba milyen adat van megjelenítve
        GroupHeader,    // Tanulók csoportjainak az összesített eredménye
        StudentData     // Egy tanuló egyéni eredménye
    }

    // Milyen szempont szerint legyen a lista rendezve
    public enum SortBy
    {
        Undefined,      // Nem meghatározott
        ID,             // Sorbarendezés azonosító alapján
        StudentName,    // Diák neve alapján
        Stars,          // Csillagok száma szerint
        Rank,           // Rangsor szerint
        Point,          // Pontszám szerint
        Progress        // teljesítés szerint
    }

    /*
    // A sorbarendezést növekvően, vagy csökkenően kell végezni
    public enum SortDirection
    {
        ascendant,      // Sorbarendezés növekvő sorrendbe (elöl van a kicsi, hátúl a nagy)
        decreasing,     // Sorbarendezés csökkenő sorrendbe
    }*/

    [Tooltip("A diák adat sávok közötti távolság.")]
    public float distanceBetweenStudentBars;

    Image imageBackground;

    Text textID;
    Text textStudentName;
    Text textStars;
    Text textRank;
    Text textPoint;
    Text textProgress;

    Image imageSortByID;
    Image imageSortByStudentName;
    Image imageSortByStars;
    Image imageSortByRank;
    Image imageSortByPoint;
    Image imageSortByProgress;

    Image imageStar;
    Image imageLessonMosaicEnd;
    Image imageDisconnect;

    StudentBarStatus studentBarStatus;

    Color singleColor;              // Milyen színe van a studentBar-nak single player esetben (ez a studentBar default színe)

    static GameObject aCopyFromItself;     // Ezt az objektumot klónozzuk ha további sorokra van szükségünk

    SortBy sortBy;                  // Melyik érték alapján rendezzük a student adatokat
    bool sortInAscendant;           // A sorbarendezés növekvően történjen
    //SortDirection sortDirection;    // A rendezés iránya (növekvő vagy csökkenő)

    static float distance;  // A távolság két studentBar pozíciója között (tehát a tetejétől a következő tetejéig)

    List<MenuStudentBar> studentBarList;    // A diákok listája
    List<MenuStudentBar> headerGroupList;   // fejlécek listája

    // Ebben az objektumban tároljuk az eredményeket
    public ClientData clientData;

    // Ebben az objektumban tároljuk egy csoport összesített eredményeit
    public ClientGroup clientGroup;

    //object data;

    int index;      // A studentBar hányadik a sorban (gomb nyomáskor ezt az információt is vissza kell adni)

    Common.CallBack_In_Int_String callBack; // Mit hívjon meg ha megnyomtak egy gombot

    /*
    string GetID { get { return clientData.clientID; } }
    string GetStudentName { get { return clientData.userName; } }
    int GetStars { get { return clientData.starNumber; } }
    int GetRank { get { return clientData.rank; } }
    int GetPoint { get { return clientData.point; } }
    int GetMosaic { get { return clientData.mosaic; } }
    int GetGame { get { return clientData.game; } }
    int GetScreen { get { return clientData.screen; } }

    // Az alábbi attribútumok csak GroupHeader esetén használatos
    string GetGroupID { get { return ""; } }
    string GetGroupStudentName { get { return ""; } }
    int GetGroupStars { get { return 0; } }
    int GetGroupRank { get { return 0; } }
    int GetGroupPoint { get { return 0; } }
    int GetGroupMosaic { get { return 0; } }
    int GetGroupGame { get { return 0; } }
    int GetGroupScreen { get { return 0; } }
    */

    // Use this for initialization
    void Awake () {
        // Összeszedjük a tanuló sáv kezeléséhez szükséges komponenseket
        imageBackground = Common.SearchGameObject(gameObject, "Background").GetComponent<Image>();

        textID = Common.SearchGameObject(gameObject, "TextID").GetComponent<Text>();
        textStudentName = Common.SearchGameObject(gameObject, "TextStudentName").GetComponent<Text>();
        textStars = Common.SearchGameObject(gameObject, "TextStars").GetComponent<Text>();
        textRank = Common.SearchGameObject(gameObject, "TextRank").GetComponent<Text>();
        textPoint = Common.SearchGameObject(gameObject, "TextPoint").GetComponent<Text>();
        textProgress = Common.SearchGameObject(gameObject, "TextProgress").GetComponent<Text>();

        imageSortByID = Common.SearchGameObject(gameObject, "ImageSortByID").GetComponent<Image>();
        imageSortByStudentName = Common.SearchGameObject(gameObject, "ImageSortByStudentName").GetComponent<Image>();
        imageSortByStars = Common.SearchGameObject(gameObject, "ImageSortByStars").GetComponent<Image>();
        imageSortByRank = Common.SearchGameObject(gameObject, "ImageSortByRank").GetComponent<Image>();
        imageSortByPoint = Common.SearchGameObject(gameObject, "ImageSortByPoint").GetComponent<Image>();
        imageSortByProgress = Common.SearchGameObject(gameObject, "ImageSortByProgress").GetComponent<Image>();

        imageStar = Common.SearchGameObject(gameObject, "ImageStar").GetComponent<Image>();
        imageLessonMosaicEnd = Common.SearchGameObject(gameObject, "ImageLessonMosaicEnd").GetComponent<Image>();
        imageDisconnect = Common.SearchGameObject(gameObject, "DisconnectButton").GetComponent<Image>();

        // Lekérdezzük a default színt
        singleColor = imageBackground.color;

        // Kikapcsoljuk az összes kép láthatóságát
        imageSortByID.enabled = false;
        imageSortByStudentName.enabled = false;
        imageSortByStars.enabled = false;
        imageSortByRank.enabled = false;
        imageSortByPoint.enabled = false;
        imageSortByProgress.enabled = false;

        imageStar.enabled = false;
        imageLessonMosaicEnd.enabled = false;
        imageDisconnect.enabled = false;

        studentBarList = new List<MenuStudentBar>();
    }

    // Update is called once per frame
    void Update () {
	
	}

    #region Header

    /// <summary>
    /// Inicializáljuk a studentBar-t.
    /// </summary>
    /// <param name="studentBarStatus">Mi legyen a studentBar státusza.</param>
    /// <param name="o"></param>
    public void InitializeAsStudentData(int index, ClientData clientData, Common.CallBack_In_Int_String callBack)
    {
        this.index = index;
        this.clientData = clientData;
        this.callBack = callBack;
        this.studentBarStatus = StudentBarStatus.StudentData;

        imageBackground.color =  (clientData.groupID == -1) ? singleColor : Common.configurationController.groupColors[clientData.groupID];

        SetArrows(SortBy.Undefined, false);

        textID.text = clientData.tabletID.ToString();
        textStudentName.text = clientData.name;
        textStars.text = clientData.starNumber.ToString();
        textRank.text = clientData.rank.ToString();
        textPoint.text = ((int)clientData.point).ToString();
        textProgress.text = clientData.mosaic.ToString() + @" \ " + clientData.game.ToString();

        imageLessonMosaicEnd.enabled = clientData.lessomMosaicEnd;

        imageDisconnect.enabled = true;
    }

    public void InitializeAsGroupHeader(ClientData clientData)
    {
        this.clientData = clientData;

        imageBackground.color = Common.configurationController.groupColors[clientData.groupID];

        textID.text = clientData.tabletID.ToString();
        textStudentName.text = clientData.name;
        textStars.text = clientData.starNumber.ToString();
        textRank.text = clientData.rank.ToString();
        textPoint.text = ((int)clientData.point).ToString();
        textProgress.text = clientData.mosaic.ToString() + @" \ " + clientData.game.ToString();

        imageLessonMosaicEnd.enabled = clientData.lessomMosaicEnd;

        SetArrows(SortBy.Undefined, false);
    }

    public void InitializeAsHeader(SortBy sortBy, bool sortIsAscendant, Common.CallBack_In_Int_String callBack)
    {
        this.callBack = callBack;
        this.index = -1;
        this.studentBarStatus = StudentBarStatus.Header;

        textID.text = Common.languageController.Translate("ID");
        textStudentName.text = Common.languageController.Translate("StudentName");
        textStars.text = "";
        imageStar.enabled = true;   // Bekapcsoljuk a csillag kép láthatóságát
        textRank.text = Common.languageController.Translate("Rank");
        textPoint.text = Common.languageController.Translate("Point");
        textProgress.text = Common.languageController.Translate("Progress") + "\n" +
            Common.languageController.Translate("Mosaic") + @" \ " + Common.languageController.Translate("Game");

        SetArrows(sortBy, sortIsAscendant);
    }


/*
    /// <summary>
    /// A StudentBar-t fejlécként inicializáljuk.
    /// </summary>
    public void InitializeHeader() {
        studentBarStatus = StudentBarStatus.Header;

        textID.text = Common.languageController.Translate("ID");
        textStudentName.text = Common.languageController.Translate("StudentName");
        textStars.text = "";
        imageStar.enabled = true;   // Bekapcsoljuk a csillag kép láthatóságát
        textRank.text = Common.languageController.Translate("Rank");
        textPoint.text = Common.languageController.Translate("Point");
        textProgress.text = Common.languageController.Translate("Progress") + "\n" +
            Common.languageController.Translate("Mosaic") + @" \ " + Common.languageController.Translate("Game");

        DeleteAllStudentBar();
    }*/

    /*
    /// <summary>
    /// Törli a StudentBar listában tárolt adatokat.
    /// </summary>
    void DeleteAllStudentBar() {
        // Töröljük az összes csoportot
        foreach (MenuStudentBar headerBar in headerGroupList)
            Destroy(headerBar);

        headerGroupList.Clear();

        // Töröljük az összes tanuló sorát
        foreach (MenuStudentBar studentBar in studentBarList)
            Destroy(studentBar);

        studentBarList.Clear();

    }*/

    /// <summary>
    /// studentBarList-ában tárolt adatokból kiválassza a megadott szempont szerint sorbarendezett
    /// értékekből a legkisebb vagy a legnagyobb értéküt és vissza adja az indexét.
    /// </summary>
    /// <param name="sortBy">Melyik szempont szerint történjen a sorbarendezés.</param>
    /// <param name="sortIsAscendant">Növekvően rendezi ha ez true, tehát a legkisebb értéket fogja vissza adni.</param>
    /// <param name="firstIndex">Az első index amit vizsgálni kell.</param>
    /// <returns></returns>
    float SearchIndex(SortBy sortBy, bool sortIsAscendant, int firstIndex) {

        int bestIndex = firstIndex;

        for (int i = firstIndex+1; i < studentBarList.Count; i++)
        {
            
            bool isBest = false;

            /*
            switch (sortBy)
            {
                case SortBy.ID:
                    isBest = (sortInAscendant) ? string.Compare(studentBarList[i].GetID, studentBarList[bestIndex].GetID) == -1 : string.Compare(studentBarList[i].GetID, studentBarList[bestIndex].GetID) == 1;
                    break;
                case SortBy.StudentName:
                    isBest = (sortInAscendant) ? string.Compare(studentBarList[i].GetStudentName, studentBarList[bestIndex].GetStudentName) == -1 : string.Compare(studentBarList[i].GetStudentName, studentBarList[bestIndex].GetStudentName) == 1;
                    break;
                case SortBy.Stars:
                    isBest = (sortInAscendant) ? studentBarList[i].GetStars < studentBarList[bestIndex].GetStars : studentBarList[i].GetStars > studentBarList[bestIndex].GetStars;
                    break;
                case SortBy.Rank:
                    isBest = (sortInAscendant) ? studentBarList[i].GetRank < studentBarList[bestIndex].GetRank : studentBarList[i].GetRank > studentBarList[bestIndex].GetRank;
                    break;
                case SortBy.Point:
                    isBest = (sortInAscendant) ? studentBarList[i].GetPoint < studentBarList[bestIndex].GetPoint : studentBarList[i].GetPoint > studentBarList[bestIndex].GetPoint;
                    break;
                case SortBy.Progress:
                    if (studentBarList[i].GetMosaic == studentBarList[bestIndex].GetMosaic)
                        isBest = (sortInAscendant) ? studentBarList[i].GetMosaic < studentBarList[bestIndex].GetMosaic : studentBarList[i].GetMosaic > studentBarList[bestIndex].GetMosaic;
                    else
                        isBest = (sortInAscendant) ? studentBarList[i].GetGame < studentBarList[bestIndex].GetGame : studentBarList[i].GetGame > studentBarList[bestIndex].GetGame;
                    break;
            }
            */
            if (isBest)
                bestIndex = i;
        }

        return bestIndex;
    }

    /// <summary>
    /// Sorbarendezi a studentBarList-ában tárolt adatokat a beállított szempont és irány szerint.
    /// </summary>
    void Sort(SortBy? sort = null, bool? sortInAsc = null) {
        if (sort != null)
            sortBy = sort.Value;
        if (sortInAsc != null)
            sortInAscendant = sortInAsc.Value;

        // Sorbarendezzük a studentBar-t is ha GroupHeader típusú
        for (int i = 0; i < studentBarList.Count; i++)
        {
            MenuStudentBar studentBar = studentBarList[i];

            if (studentBar.studentBarStatus == StudentBarStatus.GroupHeader) {
                studentBar.Sort(sortBy, sortInAscendant);

                // Rendezés után meghatározzuk a csoport összesített adatait
                // studentBar.clientData.


            }
        }

        for (int i = 0; i < studentBarList.Count; i++)
        {
            int bestIndex = i;

            // Kiválasztjuk a legkisebb / legnagyobb rendezési iránynak megfelelően a következő elemet
            for (int j = i + 1; j < studentBarList.Count; j++)
            {
                bool isBest = false;

                /*
                switch (sortBy)
                {
                    case SortBy.ID:
                        isBest = (sortInAscendant) ? string.Compare(studentBarList[j].GetID, studentBarList[bestIndex].GetID) == -1 : string.Compare(studentBarList[j].GetID, studentBarList[bestIndex].GetID) == 1;
                        break;
                    case SortBy.StudentName:
                        isBest = (sortInAscendant) ? string.Compare(studentBarList[j].GetStudentName, studentBarList[bestIndex].GetStudentName) == -1 : string.Compare(studentBarList[j].GetStudentName, studentBarList[bestIndex].GetStudentName) == 1;
                        break;
                    case SortBy.Stars:
                        isBest = (sortInAscendant) ? studentBarList[j].GetStars < studentBarList[bestIndex].GetStars : studentBarList[j].GetStars > studentBarList[bestIndex].GetStars;
                        break;
                    case SortBy.Rank:
                        isBest = (sortInAscendant) ? studentBarList[j].GetRank < studentBarList[bestIndex].GetRank : studentBarList[j].GetRank > studentBarList[bestIndex].GetRank;
                        break;
                    case SortBy.Point:
                        isBest = (sortInAscendant) ? studentBarList[j].GetPoint < studentBarList[bestIndex].GetPoint : studentBarList[j].GetPoint > studentBarList[bestIndex].GetPoint;
                        break;
                    case SortBy.Progress:
                        if (studentBarList[j].GetMosaic == studentBarList[bestIndex].GetMosaic)
                            isBest = (sortInAscendant) ? studentBarList[j].GetMosaic < studentBarList[bestIndex].GetMosaic : studentBarList[j].GetMosaic > studentBarList[bestIndex].GetMosaic;
                        else
                            isBest = (sortInAscendant) ? studentBarList[j].GetGame < studentBarList[bestIndex].GetGame : studentBarList[j].GetGame > studentBarList[bestIndex].GetGame;
                        break;
                }
                */

                if (isBest) 
                    bestIndex = j;
            }

            // Megcseréljük a legjobb indexet az i-edik adattal
            MenuStudentBar temp = studentBarList[i];
            studentBarList[i] = studentBarList[bestIndex];
            studentBarList[bestIndex] = temp;
        }

        // Meghatározzuk a sorbarendezés szerinti új pozícióját az elemeknek



        // Beállítjuk a ScrollView Content objektum méretét


    }

    #endregion

    public void SetArrows(SortBy sortBy, bool sortInAscendant) {
        // Beállítjuk a sorbarendezési nyíl irányát
        imageSortByID.transform.localScale = new Vector3(1, (sortInAscendant) ? 1 : -1, 1);
        imageSortByStudentName.transform.localScale = new Vector3(1, (sortInAscendant) ? 1 : -1, 1);
        imageSortByStars.transform.localScale = new Vector3(1, (sortInAscendant) ? 1 : -1, 1);
        imageSortByRank.transform.localScale = new Vector3(1, (sortInAscendant) ? 1 : -1, 1);
        imageSortByPoint.transform.localScale = new Vector3(1, (sortInAscendant) ? 1 : -1, 1);
        imageSortByProgress.transform.localScale = new Vector3(1, (sortInAscendant) ? 1 : -1, 1);

        // Bekapcsoljuk a megfelelő oszlopban a sorbarendezési nyilat, a többiben pedig kikapcsoljuk
        imageSortByID.enabled = sortBy == SortBy.ID;
        imageSortByStudentName.enabled = sortBy == SortBy.StudentName;
        imageSortByStars.enabled = sortBy == SortBy.Stars;
        imageSortByRank.enabled = sortBy == SortBy.Rank;
        imageSortByPoint.enabled = sortBy == SortBy.Point;
        imageSortByProgress.enabled = sortBy == SortBy.Progress;
    }

    /// <summary>
    /// Frissíti a ClientData alapján a StudentBar-t.
    /// </summary>
    public void Refresh() {
        switch (studentBarStatus)
        {
            case StudentBarStatus.Header:
                SetArrows(sortBy, sortInAscendant);


                /*
                // Frissítjük a studentBarList-ában tárolt további elemeket is
                for (int i = 0; i < studentBarList.Count; i++)
                {
                    studentBarList[i].Refresh();
                }
                */
                break;
            case StudentBarStatus.GroupHeader:

                break;
            case StudentBarStatus.StudentData:

                break;
        }
    }

    /*
    /// <summary>
    /// Beállítja a studentBar-ok pozícióit 
    /// </summary>
    /// <param name="aktPos">Hova kerüljön a studentBar.</param>
    /// <returns>A következő studentBar hova kerüljön.</returns>
    public float SetPos(float aktPos) {

        switch (studentBarStatus)
        {
            case StudentBarStatus.Header:
                // A fejléc pozícióját nem kell beállítani, mert az mindig ugyan ott van

                distance = ((RectTransform)transform).sizeDelta.y + distanceBetweenStudentBars;
                aktPos = transform.localPosition.y + distance;

                for (int i = 0; i < studentBarList.Count; i++)
                    aktPos = studentBarList[i].SetPos(aktPos);

                break;
            case StudentBarStatus.GroupHeader:
                // A csoport fejléc pozícióját beállítjuk
                transform.localPosition = transform.localPosition.SetY(aktPos);

                aktPos += distance;

                // Beállítjuk a csoportba tartozó studentBar-ok pozícióit
                for (int i = 0; i < studentBarList.Count; i++)
                    aktPos = studentBarList[i].SetPos(aktPos);

                break;
            case StudentBarStatus.StudentData:
                // Beállítunk egy studentBar pozícióját
                transform.localPosition = transform.localPosition.SetY(aktPos);

                aktPos += distance;
                break;
        }

        return aktPos;
    }
    */



/*
    public void InitializeGroup() {
        studentBarStatus = StudentBarStatus.GroupHeader;

    }
    */

    public void Add(MenuStudentBar studentBar) {

    }

    /// <summary>
    /// Student báron található gomb objektumok hívják ezt a metódust ha megnyomták őket.
    /// </summary>
    /// <param name="buttonName">A student báron megnyomott gomb neve.</param>
    public void ButtonClick(string buttonName) {

        if (callBack != null)
            callBack(index, buttonName);

        switch (buttonName)
        {
            case "Disconnect":
                break;
            case "Progress":
                break;
            case "Point":
                break;
            case "Rank":
                break;
            case "Stars":
                break;
            case "StudentName":
                break;
            case "ID":
                break;
        }
    }
}
