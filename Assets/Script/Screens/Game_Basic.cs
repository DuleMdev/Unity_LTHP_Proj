using UnityEngine;
using System.Collections;


/// <summary>
/// Ez az objektum magában nem használható semmire.
/// Csupán csak egy váz, amiből egy új játék készítésénél ki lehet indulni.
/// 
/// Tehát ha egy új játékot akarunk létrehozni, akkor ennek az objektumnak a tartalmát belekopizzuk 
/// az új játék objektumába, majd át írjuk a szükségleteknek megfelelően.
/// </summary>
public class Game_Basic : TrueGameAncestor {

    public float animSpeed = 1f;

    DragAndDropControl dragAndDropControl;

    // ------------------------------------------------------------
    TaskAncestor taskData;  // A feladatot tartalmazó objektum

    int succesfullTask;     // A jó helyre húzott elemek száma

    new void Awake()
    {
        base.Awake();

        background = GetComponentInChildren<Background>();
        
        // DragAndDropControl feedBack eseményeinek beállítása
        dragAndDropControl = gameObject.GetComponent<DragAndDropControl>();
        dragAndDropControl.itemReleased = (DragItem dragItem) => { dragItem.MoveBasePos(0); };                  // Az elemet elengedtük nem egy célpont felett
        dragAndDropControl.itemReleasedOverADragTarget = (DragItem dragItem, DragTarget dragTarget) => { };    // Elengedtük az elemet egy célpont felett, még nem tudjuk, hogy jó helyen vagy rossz helyen
        dragAndDropControl.itemPutWrongPlace = (DragItem dragItem, DragTarget dragTarget) => { dragItem.MoveBasePos(); };               // Az elemet rossz helyre helyeztük
        dragAndDropControl.itemPutGoodPlace =
            (DragItem dragItem, DragTarget dragTarget) => {
                /*
                succesfullTask++;
                if (succesfullTask == taskData.questions.Count)
                    StartCoroutine(GameEnd());
                */
            };                // Az elemet jó helyre helyeztük

        // Gombok szkriptjének beállítása
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            if (Common.IsDescendant(menu.transform, button.transform)) continue;
            button.buttonClick = ButtonClick;
        }
    }

	// Use this for initialization
	void Start () {
	
	}

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        StartCoroutine(base.InitCoroutine());

        status = Status.Init;

        // Lekérdezzük a feladat adatait
        // ---- A megfelelő objektumra kell kasztolni ------------------------------
        taskData = (TaskAffixData)Common.taskControllerOld.GetTask();

        //clock.timeInterval = taskData.time; ---------------------------------------
        clock.Reset(0);
        menu.Reset();

        yield return null;
    }

    // Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowStartCoroutine()
    {
        StartCoroutine(base.ScreenShowStartCoroutine());

        yield return null;
    }

    // Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        StartCoroutine(base.ScreenShowFinishCoroutine());

        yield return new WaitForSeconds(animSpeed); // Várunk amíg az animáció befejeződik

        Common.audioController.SetBackgroundMusic(1, 0.05f, 4); // Elindítjuk a háttérzenét

        status = Status.Play;

        yield return null;
    }

    // A kérdéshez tartozó játék elemeket kell eltávolítani, ha meghívják ezt a metódust. A TaskController fogja új feladatnál ha az új feladatot ugyan annak a képernyőnek kell megjelenítenie
    // Meglehet adni neki egy callBack függvényt, amit akkor hív meg ha végzet a játék elemek elrejtésével, mivel ez sokáig is eltarthat és addig nem kéne tovább menni az új feladatra.
    override public IEnumerator HideGameElement()
    {
        StartCoroutine(base.HideGameElement());

        clock.Reset(1); // Az órát alaphelyzetbe állítja

        yield return null;
    }

    // A képernyő eltüntetése megkezdődött. Csinálhatunk valamit ilyenkor ha szükséges
    // Ezt a ScreenController hívja meg képernyő váltásnál
    override public IEnumerator ScreenHideStart()
    {
        StartCoroutine(base.ScreenHideStart());

        yield return null;
    }

    // A képernyő teljesen eltünt. Csinálhatunk valamit ilyenkor ha szükséges
    // De figyelni kell, mivel a következő pillanatban már inaktív lesz az egész képernyő. 
    // Tehát azonnal meg kell tennünk amit akarunk nem indíthatunk a képernyőn egy coroutine-t mivel úgy sem fog lefutni.
    // Kikapcsolt gameObject-eken nem fut a coroutine.
    // Ezt a ScreenController hívja meg képernyő váltásnál
    override public IEnumerator ScreenHideFinish()
    {
        StartCoroutine(base.ScreenHideFinish());

        yield return null;
    }

    // Update is called once per frame
    void Update () {
        menu.menuEnabled = (status == Status.Play);
        dragAndDropControl.dragAndDropEnabled = (status == Status.Play);

        if (status == Status.Play) // Ha megy a játék, akkor megy az óra
            clock.Go();
        else
            clock.Stop();
    }

    // Játéknak vége letelt az idő, vagy a játék befejeződött
    override public IEnumerator GameEnd()
    {
        StartCoroutine(base.GameEnd());

        status = Status.Result;
        //clock.Stop();

        yield return new WaitForSeconds(2);

        // Tájékoztatjuk a feladatkezelőt, hogy vége a játéknak és átadjuk a játékos eredményeit
        Common.taskControllerOld.TaskEnd(null);
    }

    // A menüből kiválasztották a kilépést a játékból
    IEnumerator ExitCoroutine()
    {
        status = Status.Exit;
        clock.Stop();

        Common.taskControllerOld.GameExit(null);
        yield return null;
    }

    // Ha rákattintottak egy gombra, akkor meghívódik ez az eljárás a Button szkript által
    override protected void ButtonClick(Button button)
    {
        base.ButtonClick(button);

        if (userInputIsEnabled)
        {
            switch (button.buttonType)
            {
                case Button.ButtonType.GoMenu:
                    StartCoroutine(ExitCoroutine());
                    break;
            }
        }
    }
}
