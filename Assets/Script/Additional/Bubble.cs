using UnityEngine;
using System.Collections;

public class Bubble : MonoBehaviour {

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

    float windDownAct;
    float windUpAct;

    bool lift = true;

    Rigidbody2D rigidBody;
    SpriteRenderer spriteRenderer;
    public TextMesh textMesh;       // A jó válaszok ellenőrzésénél szükséges tudni, hogy milyen érték van a buborékon, ez ezért publikus


	// Use this for initialization
	void Awake () {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        textMesh = GetComponentInChildren<TextMesh>();

        //rigidBody.AddForce(new Vector2(0, 1));
    }

    public void Initialize(Vector2 position, Color color, string text) {
        transform.position = position;  // Beállítjuk a buborék pozícióját
        spriteRenderer.color = color;   // a színét
        textMesh.text = text;           // a buborékon lévő szöveget

        lift = Common.random.NextDouble() >= 0.5;   // Eldöntjük, hogy emelkedjen a buborék kezdetben vagy süllyedjen
        ChangeWindDown();   // Megváltoztatjuk a buborékhoz tartozó alsó szélírány erősségét
        ChangeWindUp();     // Megváltoztatjuk a buborékhoz tartozó felső szélírány erősségét
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if (transform.position.y - Common.game_Bubble.transform.position.y < -0.7) {
            // Ha elég magasra emelkedett a buborék, akkor megváltoztatjuk az irányát
            lift = true;
            ChangeWindUp();
        }
        if (transform.position.y - Common.game_Bubble.transform.position.y > 0.7)
        {
            // Ha elég alacsonyre sülyedt a buborék, akkor megváltoztatjuk az irányát
            lift = false;
            ChangeWindDown();
        }


        if (lift) {
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

    }

    void ChangeWindDown() {
        windDownAct = (float)(Common.random.NextDouble() * windDown);
    }

    void ChangeWindUp() {
        windUpAct = (float)(Common.random.NextDouble() * windUp);

        /*
        if (Common.random.NextDouble() >= 0.5)
            WindUPAct *= -1; 
            */
    }





}
