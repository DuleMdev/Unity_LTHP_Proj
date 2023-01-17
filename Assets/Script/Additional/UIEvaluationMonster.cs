using UnityEngine;
using System.Collections;

public class UIEvaluationMonster : MonoBehaviour {

    SpriteRenderer monster;

    Sprite defaultMonster;

    void Awake () {
        monster = gameObject.SearchChild("monster").GetComponent<SpriteRenderer>();
        defaultMonster = monster.sprite;
    }

    /// <summary>
    /// Beállítja a szörny képét a megadott sprite-ra.
    /// Ha null értéket adunk meg, akkor az alapértelmezett szörny fog megjelenni.
    /// </summary>
    /// <param name="monsterSprite"></param>
    public void SetMonster(Sprite monsterSprite = null)
    {
        monster.sprite = (monsterSprite == null) ? defaultMonster : monsterSprite;
    }

    public void SetMonsterAnim(Sprite monsterSprite, float time)
    {
        SetMonster(monsterSprite);

        monster.transform.parent.localScale = Vector3.one * 0.001f;
        iTween.ScaleTo(monster.transform.parent.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", time, "easeType", iTween.EaseType.easeOutElastic));
        Common.audioController.SFXPlay("boing");
    }
}
