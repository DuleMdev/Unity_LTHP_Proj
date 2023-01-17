using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class Bubble2 : MonoBehaviour
{

    public enum ForceType
    {
        Velocity,
        AddForce,
        AddRelativForce,
    }

    public ForceType forceType;
    public float force;
    public ForceMode2D forceMode;

    public float windDown;
    public float windUp;

    public float maxDirectionTime;     // Mennyit mehet maximum egy irányba a buborék
    public float minDirectionTime;     // Minimum mennyi ideig kell mennie egy irányba a buboréknak
    public float directionChangeTime;  // Mennyi idő alatt vált át az egyik irányból a másikba

    public float maxVelocity;          // Maximális sebesség
    public float maxAcceleration;      // Milyen erővel gyorsulhat a buborék

    float directionTimeRemain;          // Mennyi van még az aktuális időből
    float directionChangeRemain;        // Mennyi idő maradt még az irány váltásból
    public Vector2 newDirection;               // Az új irány

    GameObject move;                    // A buborék skálázásához
    Rigidbody2D rigidBody;

    SpriteRenderer bubbleSpriteRenderer;
    SpriteRenderer gleamSpriteRenderer;
    SpriteRenderer glowSpriteRenderer;

    GameObject pictureRoot;             // A kép ki/be kapcsolásához
    GameObject textRoot;                // A szöveg ki/be kapcsolásához

    Image imagePicture;                 // A buborékon megjelenő kép
    Image imageBorder;                  // A buborék kerete

    [HideInInspector]
    public int answerID;

    TaskBubbleData.AnswerData answerData; // A válasz adatait tartalmazza

    Text text;                   // A jó válaszok ellenőrzésénél szükséges tudni, hogy milyen érték van a buborékon, ez ezért publikus
    TEXDraw texDraw;

    // Use this for initialization
    void Awake()
    {
        move = transform.Find("move").gameObject;
        rigidBody = GetComponentInChildren<Rigidbody2D>();

        bubbleSpriteRenderer = move.GetComponent<SpriteRenderer>();
        gleamSpriteRenderer = Common.SearchGameObject(gameObject, "Gleam").GetComponent<SpriteRenderer>();
        glowSpriteRenderer = Common.SearchGameObject(gameObject, "Glow").GetComponent<SpriteRenderer>();

        pictureRoot = gameObject.SearchChild("PictureRoot").gameObject;
        textRoot = gameObject.SearchChild("TextRoot").gameObject;

        imagePicture = Common.SearchGameObject(gameObject, "Image").GetComponent<Image>();
        imageBorder = Common.SearchGameObject(gameObject, "ImageBorder").GetComponent<Image>();

        text = gameObject.SearchChild("Text").GetComponent<Text>();
        texDraw = gameObject.SearchChild("TEXDraw").GetComponent<TEXDraw>();
    }

    /// <summary>
    /// A buborékot beállítja kezdeti pozícióba. Meg lehet adni a színét és a megjelenő szöveget is.
    /// Ha szövegnek egy képet adunk meg, akkor a szöveg helyet egy kép fog megjelenni a buborékon.
    /// Képet a # előtaggal lehet megadni.
    /// </summary>
    /// <param name="position">A buborék kezdeti pozíciója.</param>
    /// <param name="color">A buborék színe.</param>
    /// <param name="text">A buborékon megjelenő szöveg vagy a buborék képe # előtaggal.</param>
    /// <returns></returns>
    public IEnumerator Initialize(TaskBubbleData task = null, int answerID = -1, Vector2? position = null, Color? color = null)
    {
        if (Common.configurationController.isServer2020)
            yield return StartCoroutine(InitializeServer2020(task, answerID, position, color));
        else
            yield return StartCoroutine(InitializeServerOld(task, answerID, position, color));
    }


    /// <summary>
    /// A buborékot beállítja kezdeti pozícióba. Meg lehet adni a színét és a megjelenő szöveget is.
    /// Ha szövegnek egy képet adunk meg, akkor a szöveg helyet egy kép fog megjelenni a buborékon.
    /// Képet a # előtaggal lehet megadni.
    /// </summary>
    /// <param name="position">A buborék kezdeti pozíciója.</param>
    /// <param name="color">A buborék színe.</param>
    /// <param name="text">A buborékon megjelenő szöveg vagy a buborék képe # előtaggal.</param>
    /// <returns></returns>
    public IEnumerator InitializeServerOld(TaskBubbleData task = null, int answerID = -1, Vector2? position = null, Color? color = null)
    {
        if (position != null)
            transform.position = position.Value;  // Beállítjuk a buborék pozícióját
        if (color != null)
            bubbleSpriteRenderer.color = color.Value;   // a színét

        if (answerID > -1)
        {
            this.answerID = answerID;
            string text = answerID < task.goodAnswers.Count ? task.goodAnswers[answerID] : task.wrongAnswers[answerID - task.goodAnswers.Count];

            // a buborékon lévő szöveget
            this.text.text = text;
            texDraw.text = text;
            this.text.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text;
            texDraw.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.TEXDraw;

            textRoot.SetActive(true);
            pictureRoot.SetActive(false);

            if (text[0] == '#')
            {
                Sprite sprite = task.gameData.GetSprite(text.Substring(1));

                // ?????????????????????????????????????????????????????????????????????????
                // Mivel nem a háttértárról olvasom a képet, ezért a coroutine nem szükséges
                yield return null;

                //yield return Common.pictureController.LoadSpriteFromFileSystemCoroutine(text.Substring(1));
                //Sprite sprite = Common.pictureController.resultSprite;

                if (sprite != null)
                {
                    // Ha sikeres volt a kép beolvasás
                    imagePicture.sprite = sprite;
                    imagePicture.enabled = true;
                    imageBorder.enabled = true;
                    bubbleSpriteRenderer.enabled = false;

                    textRoot.SetActive(false);
                    pictureRoot.SetActive(true);
                }
            }
        }

        MakeNewDirection();
    }

    /// <summary>
    /// A buborékot beállítja kezdeti pozícióba. Meg lehet adni a színét és a megjelenő szöveget is.
    /// Ha szövegnek egy képet adunk meg, akkor a szöveg helyet egy kép fog megjelenni a buborékon.
    /// Képet a # előtaggal lehet megadni.
    /// </summary>
    /// <param name="position">A buborék kezdeti pozíciója.</param>
    /// <param name="color">A buborék színe.</param>
    /// <param name="text">A buborékon megjelenő szöveg vagy a buborék képe # előtaggal.</param>
    /// <returns></returns>
    public IEnumerator InitializeServer2020(TaskBubbleData task = null, int answerID = -1, Vector2? position = null, Color? color = null)
    {
        if (position != null)
            transform.position = position.Value;  // Beállítjuk a buborék pozícióját
        if (color != null)
            bubbleSpriteRenderer.color = color.Value;   // a színét

        if (answerID > -1)
        {
            this.answerID = answerID;
            TaskBubbleData.AnswerData answerData = task.GetAnswerDataByAnswerID(answerID);

            string text = answerData.answer;

            // a buborékon lévő szöveget
            this.text.text = text;
            texDraw.text = text;
            this.text.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text;
            texDraw.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.TEXDraw;

            textRoot.SetActive(true);
            pictureRoot.SetActive(false);

            if (answerData.isImage)
            {
                Sprite sprite = task.gameData.GetSprite(answerData.answer);

                // ?????????????????????????????????????????????????????????????????????????
                // Mivel nem a háttértárról olvasom a képet, ezért a coroutine nem szükséges
                yield return null;

                //yield return Common.pictureController.LoadSpriteFromFileSystemCoroutine(text.Substring(1));
                //Sprite sprite = Common.pictureController.resultSprite;

                if (sprite != null)
                {
                    // Ha sikeres volt a kép beolvasás
                    imagePicture.sprite = sprite;
                    imagePicture.enabled = true;
                    imageBorder.enabled = true;
                    bubbleSpriteRenderer.enabled = false;

                    textRoot.SetActive(false);
                    pictureRoot.SetActive(true);
                }
            }
        }

        MakeNewDirection();
    }

    public void FlashGlow(Color color) {
        glowSpriteRenderer.color = color;
        StartCoroutine(FlasGlowCoroutine());
    }

    // Háromszor villogtatjuk a fényt a buborék körül
    public IEnumerator FlasGlowCoroutine() {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.2f);
            glowSpriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.2f);
            glowSpriteRenderer.enabled = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Csak akkor mozgatjuk a dolgokat, ha a szöveg mozgatás engedélyezve van
        if (Common.configurationController.playAnimation)
        {
            rigidBody.AddForce(newDirection * Time.deltaTime, ForceMode2D.Impulse);




            directionTimeRemain -= Time.deltaTime;
            if (directionTimeRemain <= 0)
                MakeNewDirection();

        }



        /*
        if (lift)
        {
            switch (forceType)
            {
                case ForceType.Velocity:
                    rigidBody.velocity = new Vector2(0, force * Time.deltaTime);
                    break;
                case ForceType.AddForce:
                    rigidBody.AddForce(new Vector2(0, force * Time.deltaTime), forceMode);
                    break;
                case ForceType.AddRelativForce:
                    rigidBody.AddRelativeForce(new Vector2(0, force * Time.deltaTime), forceMode);
                    break;
            }
        }

        float windSide = (transform.position.y > 0) ? windUp : windDown;

        rigidBody.AddForce(new Vector2(transform.position.y * windSide * Time.deltaTime, 0), forceMode);
        */
    }

    // A buborék megszületése
    // Az aninSpeed tartalmazza a megszületés sebességét (azaz annyi idő alatt fog előpattanni)
    public void BubbleBorn(float animSpeed) {
        move.transform.localScale = Vector3.one * 0.0001f;
        iTween.ScaleTo(move, iTween.Hash("scale", Vector3.one, "time", animSpeed, "easeType", iTween.EaseType.easeOutElastic));
    }

    // Megváltoztatjuk az irányát a buboréknak
    void MakeNewDirection() {
        // Kitaláljuk az új irányt
        newDirection = new Vector2((float)Common.random.NextDouble() - 0.5f, (float)Common.random.NextDouble() - 0.5f);
        newDirection.Normalize();

        // Kitaláljuk az új sebességet
        newDirection = newDirection * ((float)Common.random.NextDouble() + 0.1f) * maxAcceleration;

        // Kitaláljuk, hogy mennyi ideig menjen az új irányba
        directionTimeRemain = (float)Common.random.NextDouble() * (maxDirectionTime - minDirectionTime) + minDirectionTime;

        directionChangeRemain = directionChangeTime;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        MakeNewDirection();
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        MakeNewDirection();
    }

    public void SetPictures(Sprite bubble, Color bubbleColor, Sprite gleam, Color gleamColor, Sprite pictureBorder, Color pictureBorderColor)
    {
        bubbleSpriteRenderer.sprite = bubble;
        bubbleSpriteRenderer.color = bubbleSpriteRenderer.color.SetA(bubbleColor.a);
        gleamSpriteRenderer.sprite = gleam;
        gleamSpriteRenderer.color = gleamColor;
        imageBorder.sprite = pictureBorder;
        imageBorder.color = pictureBorderColor;
    }
}

