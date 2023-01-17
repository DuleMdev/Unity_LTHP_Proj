using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;

public class Badges : MonoBehaviour {

    GameObject backButton;

    SwitchPicture[] switchPictureMonsterArray;

    Text textAllStars;
    Text textMultiCup;

    SwitchPicture star3x;
    SwitchPicture star5x;
    SwitchPicture star10x;

    SwitchPicture bestTeamMember;
    SwitchPicture cleverestStudent;
    SwitchPicture fastestStudent;

    JSONNode jsonData;

    int[] monsterOrder;

    static public Badges instance;

    // Use this for initialization
    void Awake () {
        instance = this;

        backButton = gameObject.SearchChild("CanvasBackButton").gameObject;

        // Összegyűjtjük a szörnyeket
        switchPictureMonsterArray = new SwitchPicture[9];
        for (int i = 1; i < 10; i++)
            switchPictureMonsterArray[i-1] = gameObject.SearchChild("monster" + i).GetComponent<SwitchPicture>();

        textAllStars = gameObject.SearchChild("textAllStars").GetComponent<Text>();
        textMultiCup = gameObject.SearchChild("textMultiCup").GetComponent<Text>();

        star3x = gameObject.SearchChild("3xStar").GetComponent<SwitchPicture>();
        star5x = gameObject.SearchChild("5xStar").GetComponent<SwitchPicture>();
        star10x = gameObject.SearchChild("10xStar").GetComponent<SwitchPicture>();

        bestTeamMember = gameObject.SearchChild("bestTeamMember").GetComponent<SwitchPicture>();
        cleverestStudent = gameObject.SearchChild("cleverestStudent").GetComponent<SwitchPicture>();
        fastestStudent = gameObject.SearchChild("fastestStudent").GetComponent<SwitchPicture>();
    }

    /// <summary>
    /// Inicializáljuk a jelvény képernyőt
    /// </summary>
    public void Init(JSONNode jsonData) {
        this.jsonData = jsonData;

        // Kikapcsoljuk a vissza gombot ha csak az érmeket nézhetjük meg
        backButton.SetActive(!jsonData[C.JSONKeys.onlyBadge].AsBool && !jsonData[C.JSONKeys.multi].AsBool);

        // Bekapcsoljuk a megfelelő szörnyeket
        int levelBorder = jsonData[C.JSONKeys.levelBorder].AsInt;
        int allStar = jsonData[C.JSONKeys.allStar].AsInt;
        int level = allStar / ((levelBorder != 0) ? levelBorder : 1);
        if (level > 9)
            level = 9;

        if (jsonData.ContainsKey(C.JSONKeys.monsterOrder))
        {
            monsterOrder = Common.JSONToArray(jsonData[C.JSONKeys.monsterOrder]);
            for (int i = 0; i < switchPictureMonsterArray.Length; i++)
                switchPictureMonsterArray[monsterOrder[i]].ChangeSprite(i < level);
        }

        // Beállítjuk az összes csillag számát
        textAllStars.text = jsonData[C.JSONKeys.allStar];

        // Beállítjuk a multi csillag számot a kupán
        textMultiCup.text = jsonData[C.JSONKeys.allGroupStar];

        // Beállítjuk a legjobb csapat tag díjat
        bestTeamMember.ChangeSprite(jsonData[C.JSONKeys.bestTeamMember].AsBool);

        // Beállítjuk a legokosabb tanuló díjat
        cleverestStudent.ChangeSprite(jsonData[C.JSONKeys.cleverestStudent].AsBool);

        // Beállítjuk a leggyorsabb tanuló díjat
        fastestStudent.ChangeSprite(jsonData[C.JSONKeys.fastestStudent].AsBool);

        // Beállítjuk a legnagyobb három csillag sorozat díjakat
        int longest3xSeries = jsonData[C.JSONKeys.longest3StarSeries].AsInt;
        star3x.ChangeSprite(longest3xSeries >= 3);
        star5x.ChangeSprite(longest3xSeries >= 5);
        star10x.ChangeSprite(longest3xSeries >= 10);
    }

    /// <summary>
    /// Visszaadja a megadott indexű monster sprite képét. Az indexek nullától kezdődnek.
    /// </summary>
    /// <param name="monsterIndex">A kívánt indexű monster.</param>
    /// <returns>A kért monstar sprite képe.</returns>
    public Sprite GetMonster(int monsterIndex) {
        if (monsterIndex >= switchPictureMonsterArray.Length)
            monsterIndex = switchPictureMonsterArray.Length - 1;

        if (monsterIndex < 0)
            monsterIndex = 0;

        return switchPictureMonsterArray[monsterOrder[monsterIndex]].GetMonster();
    }
}
