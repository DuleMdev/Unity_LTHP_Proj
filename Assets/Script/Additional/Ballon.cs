using UnityEngine;
using System.Collections;

public class Ballon : MonoBehaviour {

    SpriteRenderer ballonPicture;
    SpriteRenderer ballonShadow;

    // Use this for initialization
    void Awake () {
        ballonPicture = gameObject.SearchChild("ballonPicture").GetComponent<SpriteRenderer>();
        ballonShadow = gameObject.SearchChild("ballonShadow").GetComponent<SpriteRenderer>();
    }

    public void SetPictures(Sprite ballon, Sprite shadow, Color shadowColor)
    {
        ballonPicture.sprite = ballon;
        ballonShadow.sprite = shadow;
        ballonShadow.color = shadowColor;
    }
}
