using UnityEngine;
using System.Collections;

public class HangmanMonster : MonoBehaviour {

    [Tooltip("Milyen mértékkel lebeg a szörny")]
    public float floatSize;
    [Tooltip("Milyen frekvenciával lebeg 1 = 1 másodperc")]
    public float floatTime;

    GameObject move;        // Szörny mozgatásához felbukkanásához és eltüntetéséhez
    GameObject staticMove;  // A szörny állandó fel-le történő mozgásához
    SpriteRenderer monster; // A szörny képe
    SpriteRenderer monsterShadow;   // A szörny árnyéka
    SpriteRenderer smile;   // A szörny mosolygó arca
    SpriteRenderer scary;   // A szörny rémült arca

    Ballon[] listOfBallons; // A lufik listája

    const int MAX_BALLON = 10;          // Lufik száma
    const float MONSTER_OUT_POS = -1f;// A szörny lokális pozíciója ha nincs a képernyőn
    public const float MONSTER_SPEED = 4.5f;   // Mennyi idő alatt jön be a szörny illetve megy ki a képernyőből
    const float BALLON_OUT_POS = 1.1f;    // A lufi globális pozíciója ha kiszállt a képernyőből
    const float BALLON_SPEED = 5f;      // Mennyi idő alatt megy ki a ballon a képernyőből

    public int ballonNumber { get; private set; }     // Hány lufi van még

    float scaryTime;        // Még mennyi ideig vág ijedt képet a szörny
    float aktFloatTime;     // A szörny fel-le történő mozgásához


    // Use this for initialization
    void Awake () {
        move = Common.SearchGameObject(gameObject, "move").gameObject;
        staticMove = Common.SearchGameObject(gameObject, "staticMove").gameObject;
        monster = Common.SearchGameObject(gameObject, "monster").GetComponent<SpriteRenderer>();
        monsterShadow = Common.SearchGameObject(gameObject, "shadow").GetComponent<SpriteRenderer>();
        smile = Common.SearchGameObject(gameObject, "smile").GetComponent<SpriteRenderer>();
        scary = Common.SearchGameObject(gameObject, "scary").GetComponent<SpriteRenderer>();

        // Lufik megkeresése
        listOfBallons = new Ballon[MAX_BALLON];
        Ballon[] list = gameObject.GetComponentsInChildren<Ballon>(true);
        foreach (Ballon ballon in list)
            listOfBallons[int.Parse(ballon.name.Substring(6)) - 1] = ballon;
    }

    /// <summary>
    /// Alap helyzetbe állítjuk a szörnyet
    /// </summary>
    /// <param name="ballonNumber">Hány elszálló lufija legyen a szörnynek (Az utolsó lufi nem száll el).</param>
    public void Init(int ballonNumber) {
        // Végig megy a lufik listáján és minden lufit alap helyzetbe tesz és bekapcsol annyi lufit láthatóvá tesz amennyire szükség van
        for (int i = 0; i < MAX_BALLON; i++)
        {
            listOfBallons[i].transform.SetParent(staticMove.transform);
            listOfBallons[i].transform.localPosition = Vector3.zero;
            listOfBallons[i].transform.Rotate(Vector3.zero);
            listOfBallons[i].gameObject.SetActive(i < ballonNumber);
            iTween.Stop(listOfBallons[i].gameObject);
        }

        // Nem kérhetünk több lufit, mint amennyi van
        this.ballonNumber = (ballonNumber < MAX_BALLON) ? ballonNumber : MAX_BALLON;

        scaryTime = 0;

        // A szörny pozíciója a képernyőn kívűl van
        move.transform.position = move.transform.position.AddY(MONSTER_OUT_POS);
    }

    /// <summary>
    /// Megadhatjuk, hogy a szörny mosolyogjon vagy ijedt legyen
    /// </summary>
    /// <param name="isSmile">Ha true, akkor a szörny mosolyog</param>
    void Smile(bool isSmile) {
        smile.gameObject.SetActive(isSmile);
        scary.gameObject.SetActive(!isSmile);
    }

    /// <summary>
    /// Megjeleníti a szörnyet, azaz alulról beúszik.
    /// </summary>
    public float MonsterIn(float animTime = MONSTER_SPEED) {
        if (animTime > 0)
            iTween.MoveTo(move, iTween.Hash("islocal", true, "position", Vector3.zero, "time", animTime, "easeType", iTween.EaseType.easeOutBack));
        else
            move.transform.localPosition = Vector3.zero;

        return animTime;
    }

    /// <summary>
    /// Kimegy a szörny a képernyőből, azaz lepottyan.
    /// </summary>
    public float MonsterOut() {
        iTween.MoveTo(move, iTween.Hash("islocal", false, "y", move.transform.position.AddY(MONSTER_OUT_POS), "time", MONSTER_SPEED / 2, "easeType", iTween.EaseType.easeInCirc));

        return MONSTER_SPEED / 2;
    }

    /// <summary>
    /// A szörnytől elszáll egy lufi ha van még lufija.
    /// </summary>
    public void BallonFlyAway() {
        if (ballonNumber > 0) {
            ballonNumber--;
            // Áttesszük a lufit a szörnyet tartalmazó GameObject-re, hogy a szörny esetleges mozgásától függetlenül tudjon mozogni a lufi
            listOfBallons[ballonNumber].transform.SetParent(transform);

            // A lufit elrepítjük iTween-nel, az iTween majd visszaszól ha befejezte a reptetést, mert akkor kikapcsoljuk a lufit
            iTween.MoveTo(listOfBallons[ballonNumber].gameObject, iTween.Hash("y", BALLON_OUT_POS, "time", BALLON_SPEED, "easeType", iTween.EaseType.linear, "oncomplete", "BallonFlyAwayComplete", "oncompletetarget", gameObject, "oncompleteparams", listOfBallons[ballonNumber]));

            scaryTime = 1;
        }
    }

    /// <summary>
    /// Az iTween komponens hívja meg ezt a metódust, ha a lufi kiszált a képernyőből.
    /// </summary>
    /// <param name="o">A kiszállt lufira mutató referencia</param>
    void BallonFlyAwayComplete(object o) {
        ((Ballon)o).gameObject.SetActive(false);
    }

    void Update() {
        scaryTime -= Time.deltaTime;
        Smile(scaryTime <= 0 && ballonNumber != 0);

        // kiszámoljuk az aktuális lebegés értéket

        aktFloatTime += Time.deltaTime / floatTime * Mathf.PI;
        float yPos = Mathf.Sin(aktFloatTime);

        staticMove.transform.localPosition = new Vector2(move.transform.localPosition.x, yPos * floatSize);
    }

    public void SetPictures(LayoutManager layoutManager)
    {
        monster.sprite = layoutManager.GetSprite("monster");
        monsterShadow.sprite = layoutManager.GetSprite("monsterShadow");
        smile.sprite = layoutManager.GetSprite("monsterSmile");
        scary.sprite = layoutManager.GetSprite("monsterScary");

        // Beállítjuk a lufik képeit
        for (int i = 0; i < listOfBallons.Length; i++)
        {
            listOfBallons[i].SetPictures(
                layoutManager.GetSprite("ballon" + (i + 1)),
                layoutManager.GetSprite("ballonShadow" + (i + 1)),
                layoutManager.GetColor("ballonShadow" + (i + 1))
                );
        }
    }
}




