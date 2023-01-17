using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour
{

    public enum ButtonType
    {
        // Common
        Default, Exit, Next, Menu, GoMenu, SwitchLayout, Play,

        Game_TrueOrFalse = 50, Game_Bubble, Game_Sets, Game_Math_Monster, Game_Fish, Game_Affix,
        Game_Boom, Game_Hangman, Game_Read, Game_Millionaire,

        // Games
        Bubble = 100,

        // True or False
        TrueAnswer = 200, FalseAnswer,

        // Boom Game
        Switch = 300,
        
        // Read Game
        ShowStory = 400, ShowQuestion,



        // Canvas button


    }

    public ButtonType buttonType;

    public Sprite defaultSprite;
    public Sprite anotherSprite;

    public GameObject clickReceiverObject;
    public string methodName;

    public bool onMouseDown; // Az OnMouseDown-nak kell aktívnak lennie vagy a Clicked metódusnak?

    [HideInInspector]
    public bool isDefaultSprite = true;
    [HideInInspector]
    public bool disable = false; // if disable = true then not may click
    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    public delegate void ButtonClick(Button button);
    public ButtonClick buttonClick;

    public void Awake()
    {
        SpriteRenderer[] spriteRendererArray = GetComponentsInChildren<SpriteRenderer>(true);

        if (spriteRendererArray.Length != 0)
            spriteRenderer = spriteRendererArray[0];
        //else Debug.LogError (Common.GetGameObjectHierarchy (gameObject));

        if (spriteRenderer != null)
        {
            if (!defaultSprite)
                defaultSprite = spriteRenderer.sprite;
            else
                ChangeButtonFace(defaultSprite);
        }
    }

    // This event call MouseClick script
    public void Clicked()
    {
        if (!onMouseDown)
            clickProcess();
    }

    // This event call GUI system
    public void OnMouseDown()
    {
        if (onMouseDown && !Common.fadeActive)
            clickProcess();
    }

    public void clickProcess()
    {
        // Ha nincs letiltva
        if (!disable)
        {
            // Ha beállítottak egy függvényt, amit hívni kell kattintás esetén
            if (buttonClick != null)
                buttonClick(this);

            // Ha beállítottak egy GameObject-et és megadtak egy metódus nevet, amit hívni kell kattintás esetén
            if (clickReceiverObject && !string.IsNullOrEmpty(methodName))
                clickReceiverObject.SendMessage(methodName);
        }
    }

    public void ChangeButtonFace(bool DefaultFace)
    {
        spriteRenderer.sprite = (DefaultFace) ? defaultSprite : anotherSprite;
        isDefaultSprite = DefaultFace;
    }

    public void Visible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
