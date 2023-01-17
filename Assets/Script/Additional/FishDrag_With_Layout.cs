using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/*
A hal minden ütközésnél sebességet vált



    
*/




public class FishDrag_With_Layout : DragItem {

    public enum SwimDirection
    {
        Right, Left,
    }

    [Tooltip("Minimális úszási sebesség")]
    public float swimSpeedMin;
    [Tooltip("Maximális úszási sebesség")]
    public float swimSpeedMax;
    [Tooltip("Milyen időközönként fordulhat meg a hal")]
    public float turnTime;



    [HideInInspector]
    public bool swimEnabled;                     // Engedélyezve van a halaknak az úszás?

    Text text;                              // Szöveg megjelenítéséhez
    List<GameObject> listOfFish;
    
    public int fishNumber { get; private set; }

    RectTransform fishShadowTransform;
    RectTransform fishTransform;
    BoxCollider2D boxCollider;
    Rigidbody2D rigidBody;

    string defaultText;                     // A halak mérete melyik szöveghez képest lett megadva

    float fishSizeDiff;                     // A különbség a szöveg mérete és a hal képének mérete között
    float fishShadowDiff;                   // A különbség a szöveg mérete és a hal árnyékának mérete között
    float fishColliderDiff;                 // A különbség a szöveg mérete és a halon levő collider mérete között
    float textMinSize;                      // Mennyinek kell lennie a szöveg minimális méretének, hogy a képek ne torzuljanak

    SwimDirection swimDirection;            // Merre úszik a hal false = jobbra, true = balra

    float horizontalSwimVelocity;           // Vízszintes úszási sebesség
    float verticalSwimVelocity;             // Függőleges úszási sebesség

    float previousXPos;                     // Hol volt a halnak az előző X pozíciója

    float frameWithoutCollision;            // Az ütközés nélkül eltelt framek (Csak akkor fordítjuk a halat az irányba ha ez legalább három frame)

    Color flashingColor;                    // Milyen szinnel villogjon a szöveg a FlashingCoroutine eljárásban

    float lastTurn;                         // Mennyi idő telt el az utolsó fordulás óta

    // Use this for initialization
    public override void Awake () {

        base.Awake();

        // Összeszedjük a hal formákat
        listOfFish = new List<GameObject>();
        int index = 1;
        while (true)
        {
            GameObject go = Common.SearchGameObject(gameObject, "Fish" + index);
            if (go == null) break;
            listOfFish.Add(go);
            fishNumber = index++;
        }

        BaseTransform = transform;
        MoveTransform = Common.SearchGameObject(gameObject, "FishMove").transform;

        rigidBody = GetComponentInChildren<Rigidbody2D>();

        // Megkeressük a szöveg kirajzolásához használatos komponenst
        text = Common.SearchGameObject(gameObject, "Text").GetComponent<Text>();

        defaultText = text.text;
    }

    // Létrehozzuk a kívánt halat a kívánt szöveggel
    // A fishIndex a létrehozandó hal indexét tartalmazza 
    // nulla az első és nem lehet annyi vagy több mint a fishNumber
    public void Initialize(int fishIndex, string text) {
        // Ha nem megfelelő indext adtunk meg, akkor nem csinál semmit
        if (fishIndex < 0 || fishIndex >= fishNumber) return;

        // Bekapcsolja a megfelelő indexű halat
        for (int i = 0; i < listOfFish.Count; i++)
            listOfFish[i].SetActive(fishIndex == i);

        // Megvizsgáljuk, hogy az aktuális szöveg méret és az egyébb komponensek mérete, hogy viszonyul egymáshoz
        fishShadowTransform = Common.SearchGameObject(listOfFish[fishIndex], "Shadow").GetComponent<RectTransform>();
        fishTransform = Common.SearchGameObject(listOfFish[fishIndex], "Picture").GetComponent<RectTransform>();
        boxCollider = Common.SearchGameObject(listOfFish[fishIndex], "Picture").GetComponent<BoxCollider2D>();
        itemRenderer = Common.SearchGameObject(listOfFish[fishIndex], "Picture").GetComponent<Renderer>();

        // Kiszámoljuk a szöveg minimális szélességét, mivel ha ennél kisebb szöveget rajzolnánk ki, akkor a hal képe zsugorodna, ami problémákhoz vezetne
        textMinSize = Mathf.Max(
            (fishShadowTransform.GetComponent<Image>().sprite == null) ? 0 : this.text.preferredWidth + fishShadowTransform.GetComponent<Image>().sprite.texture.width - fishShadowTransform.sizeDelta.x,
            this.text.preferredWidth + fishTransform.GetComponent<Image>().sprite.texture.width - fishTransform.sizeDelta.x
            );

        /*
        textMinSize = Mathf.Max(
            fishShadowTransform.GetComponent<Image>().sprite.border.x + fishShadowTransform.GetComponent<Image>().sprite.border.z,
            fishTransform.GetComponent<Image>().sprite.border.x + fishTransform.GetComponent<Image>().sprite.border.z);
            */

        Debug.Log("minWidth : " + textMinSize);

        fishSizeDiff = fishTransform.sizeDelta.x - this.text.preferredWidth;
        fishShadowDiff = fishShadowTransform.sizeDelta.x - this.text.preferredWidth;
        fishColliderDiff = boxCollider.size.x - this.text.preferredWidth;

        ChangeText(text);

        // Beállítjuk a hal irányát a 

        MakeNewDirection();

        swimEnabled = true;

        //ChangeDirection(SwimDirection.Left);
    }

    public void ChangeFish(int fishIndex)
    {
        string text = this.text.text;

        bool swimEnabledTemp = swimEnabled;

        ChangeText(defaultText);
        ChangeDirection(SwimDirection.Right);
        Initialize(fishIndex, text);

        // Az Initialize engedélyezi a hal úszását, ezért meg kell jegyezni és azt beállítani, hogy ha egy hal a helyén van már, akkor ne ússzon el
        swimEnabled = swimEnabledTemp;
    }

    // Kicseréli a halon megjelenő szöveget
    public void ChangeText(string newText) {
        itemName = newText;

        text.text = newText;

        float textWidth = Mathf.Max(text.preferredWidth, textMinSize);

        fishTransform.sizeDelta = new Vector2(textWidth + fishSizeDiff, fishTransform.sizeDelta.y);
        fishShadowTransform.sizeDelta = new Vector2(textWidth + fishShadowDiff, fishShadowTransform.sizeDelta.y);
        boxCollider.size = new Vector2(textWidth + fishColliderDiff, boxCollider.size.y);

        /*/
        fishTransform.sizeDelta = new Vector2(text.preferredWidth + fishSizeDiff, fishTransform.sizeDelta.y);
        fishShadowTransform.sizeDelta = new Vector2(text.preferredWidth + fishShadowDiff, fishShadowTransform.sizeDelta.y);
        boxCollider.size = new Vector2(text.preferredWidth + fishColliderDiff, boxCollider.size.y);
        /*/
    }

    // Megváltoztatja a hal úszási irányát
    public void ChangeDirection(SwimDirection newSwimDirection) {
        // Ha már a hal egyébként is abba az irányba úszott, akkor nem csinálunk semmit
        if (swimDirection == newSwimDirection) return;

        // Megfordítjuk a hal képét
        Vector3 v3 = fishTransform.localScale;
        //fishTransform.localScale = new Vector3(v3.x * -1, v3.y, v3.z);
        fishTransform.localScale = fishTransform.localScale.MulX(-1);


        // Megfordítjuk az árnyék képét
        //v3 = fishShadowTransform.localScale;
        //fishShadowTransform.localScale = new Vector3(v3.x * -1, v3.y, v3.z);
        fishShadowTransform.localScale = fishShadowTransform.localScale.MulX(-1);

        // Módosítjuk a hal pozícióját, hogy a szöveghez passzoljon továbbra is
        Vector3 pos = fishTransform.localPosition;
        fishTransform.localPosition = new Vector3(pos.x * -1, pos.y, pos.z);

        // Módosítjuk a hal árnyékának a pozícióját pont olyan mértékben mint amilyen mértékben a hal képét módosítottuk
        v3 = fishShadowTransform.localPosition;
        fishShadowTransform.localPosition = new Vector3((fishTransform.localPosition.x - pos.x) + v3.x, v3.y, v3.z);

        swimDirection = newSwimDirection;

        lastTurn = 0;
    }

    // Letiltja a hal mozgását, ekkor a hal jobbra fordul és kikapcsolódik a collidere, hogy ki lehessen úsztatni a képernyőből
    public void Enabled(bool enabled) {
        swimEnabled = enabled;
        boxCollider.enabled = enabled;
        ChangeDirection(SwimDirection.Right);
        MakeNewDirection();
    }

    // Update is called once per frame
    void Update () {
        //Debug.Log("size :" + text.rectTransform.sizeDelta);




	}

    // Megváltoztatjuk a hal sebességét
    void MakeNewDirection()
    {
        // Kitaláljuk az új sebességet
        horizontalSwimVelocity = ((float)Common.random.NextDouble()) * (swimSpeedMax - swimSpeedMin) + swimSpeedMin;
        //Debug.Log("horizontal swim velocity : " + horizontalSwimVelocity);
        horizontalSwimVelocity *= (Common.random.NextDouble() < 0.5) ? 1 : -1;

        verticalSwimVelocity = ((float)Common.random.NextDouble()) * (swimSpeedMax - swimSpeedMin) + swimSpeedMin * 0.1f;
        //Debug.Log("vertical swim velocity : " + verticalSwimVelocity);
        verticalSwimVelocity *= (Common.random.NextDouble() < 0.5) ? 1 : -1;

        frameWithoutCollision = 0;
    }

    void FixedUpdate()
    {
        // Az inicializálást megelőzően a boxCollider null
        if (boxCollider != null)
            boxCollider.enabled = swimEnabled;

        if (swimEnabled)
        {
            /*
            if (frameWithoutCollision >= 5)
                ChangeDirection((gameObject.transform.position.x < previousXPos) ? SwimDirection.Left : SwimDirection.Right);
                */
            lastTurn += Time.deltaTime;

            if (lastTurn > turnTime)
                ChangeDirection((gameObject.transform.position.x < previousXPos) ? SwimDirection.Left : SwimDirection.Right);

            rigidBody.AddForce(new Vector2(horizontalSwimVelocity, verticalSwimVelocity) * Time.deltaTime, ForceMode2D.Impulse);

            //Debug.Log("horizontal swim velocity : " + horizontalSwimVelocity);
            //Debug.Log("vertical swim velocity : " + verticalSwimVelocity);

            frameWithoutCollision++;
            previousXPos = gameObject.transform.position.x;
        }
        else {
            rigidBody.velocity = Vector2.zero;
        }
    }

    // Beállítjuk a rétegsorrendjét az elemnek
    public override void SetOrderInLayer(int order)
    {
        gameObject.GetComponent<Canvas>().sortingOrder = order;
    }

    public override void SetDragPos(Vector3 grabWorldPos)
    {
        base.SetDragPos(grabWorldPos);
        swimEnabled = false;
    }

    // A bázis pozícióra mozgatta az iTween a move gameObject-et
    override public void MoveBasePosEnd()
    {
        base.MoveBasePosEnd();

        // Ha az elem még nincs a céljában, akkor engedélyezzük a mozgását
        swimEnabled = !itemInPlace;
    }

    // Villogtatja az elemet
    public override void FlashingPositive()
    {
        flashingColor = Color.green;
        base.FlashingPositive();
    }


    public override void FlashingNegative()
    {
        flashingColor = Color.red;
        base.FlashingNegative();
    }

    public override IEnumerator FlashingCoroutine()
    {
        Color textOriginalColor = text.color;

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.2f);
            text.color = flashingColor;

            yield return new WaitForSeconds(0.2f);
            text.color = textOriginalColor;
        }

        animRun = false;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        MakeNewDirection();
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        MakeNewDirection();
    }

    public override float GetHeight()
    {
        //return Common.FindGameObject(gameObject, "Picture").GetComponent<Renderer>().bounds.size.y;
        return fishTransform.sizeDelta.y * Mathf.Abs(fishTransform.lossyScale.y);
        //return itemRenderer.bounds.size.y;
    }

    public override float GetWidth()
    {
        //return Common.FindGameObject(gameObject, "Picture").GetComponent<Renderer>().bounds.size.x;
        return fishTransform.sizeDelta.x * Mathf.Abs(fishTransform.lossyScale.x);

        //return itemRenderer.bounds.size.x;
    }
}
