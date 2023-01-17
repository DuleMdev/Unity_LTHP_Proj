using SimpleJSON;
using System.Collections.Generic;

public class CurriculumPathData
{
    //public JSONNode jsonData;

    public string ID { get; private set; }
    public string mailListID { get; private set; }
    public string name { get; private set; }

    public float progress { get; private set; }
    public float scorePercent { get; private set; }
    public string playTimeString { get; private set; }
    public bool replayable { get; private set; }

    public string newLearnRoutePathIsolationTimeForContinue { get; private set; }
    public string lastLearnRoutePathIsolationTimeForContinue { get; private set; }
    public string flowStyle { get; private set; }

    //public int gameTheme { get; private set; }
    //public int gameEnding { get; private set; }

    public string appPlayTimeString;

    // E helyett van a scorePercent
    //public float sumGameResult { get; private set; } 

    public bool frameGameExists { get; private set; }
    public bool coinGameExists { get; private set; }
    public bool chestExists { get; private set; }

    public bool characterSelectExists { get; private set; }
    public CharacterData heroData { get; private set; }
    public CharacterData enemyData { get; private set; }
    public CharacterData victimData { get; private set; }
    public string currentLevelNumber { get; private set; }
    public string possibleMaxLevelNumber { get; private set; }

    


    public List<CurriculumInfoForCurriculumPath> listOfCurriculumInfo = new List<CurriculumInfoForCurriculumPath>();

    public CurriculumPathData()
    {
        chestExists = true;
        coinGameExists = true;
    }

    public CurriculumPathData(JSONNode json) {
        //jsonData = json;

        ID = json[C.JSONKeys.learnRoutePathID].Value;
        mailListID = json[C.JSONKeys.mailListID].Value;
        name = json[C.JSONKeys.name].Value;

        progress = json[C.JSONKeys.progress].AsFloat;
        scorePercent = json[C.JSONKeys.scorePercent].AsFloat;
        playTimeString = json[C.JSONKeys.playTimeString].Value;
        replayable = json[C.JSONKeys.replayable].Value != "0";

        newLearnRoutePathIsolationTimeForContinue = json[C.JSONKeys.newLearnRoutePathIsolationTimeForContinue].Value;
        lastLearnRoutePathIsolationTimeForContinue = json[C.JSONKeys.lastLearnRoutePathIsolationTimeForContinue].Value;
        flowStyle = json[C.JSONKeys.flow_Style].Value;

        //gameTheme = json[C.JSONKeys.gameTheme].AsInt;
        //gameEnding = json[C.JSONKeys.gameEnding].AsInt;

        for (int i = 0; i < json[C.JSONKeys.curriculumData].Count; i++)
            listOfCurriculumInfo.Add(new CurriculumInfoForCurriculumPath(json[C.JSONKeys.curriculumData][i]));

        appPlayTimeString = json[C.JSONKeys.appPlayTimeString].Value;

        frameGameExists = json[C.JSONKeys.frameGame].AsBool;
        coinGameExists = json[C.JSONKeys.collectBonusCoins].AsBool;
        chestExists = json[C.JSONKeys.hasFrameGameChests].AsBool;

        characterSelectExists = json[C.JSONKeys.frameGameCharacters].Count != 0;
        if (characterSelectExists)
        {
            heroData = new CharacterData(json[C.JSONKeys.frameGameCharacters][C.JSONKeys.hero]);
            enemyData = new CharacterData(json[C.JSONKeys.frameGameCharacters][C.JSONKeys.monster]);
            victimData = new CharacterData(json[C.JSONKeys.frameGameCharacters][C.JSONKeys.victim]);
        }

        currentLevelNumber = json[C.JSONKeys.levelInfo][C.JSONKeys.currentLevelNumber].Value;
        possibleMaxLevelNumber = json[C.JSONKeys.levelInfo][C.JSONKeys.possibleMaxLevelNumber].Value;

        /*
        // Megszámoljuk, hogy a teljesített tananyagoknak mennyi az átlaga
        sumGameResult = 0;
        int gameResultCount = 0;
        for (int i = 0; i < listOfCurriculumInfo.Count; i++)
        {
            if (listOfCurriculumInfo[i].maxCurriculumProgress == 100) {
                sumGameResult += listOfCurriculumInfo[i].scorePercent;
                gameResultCount++;
            }
        }
        sumGameResult = (gameResultCount > 0) ? sumGameResult / gameResultCount : 0;
        */
    }

    public void SetFrameGameCharacters(CharacterData heroData, CharacterData monsterData, CharacterData victimData)
    {
        this.heroData = heroData;
        this.enemyData = monsterData;
        this.victimData = victimData;

        characterSelectExists = true;
    }

    /// <summary>
    /// Feldolgoz egy  olyan json-t amely tanulási útvonalakat tartalmaz. A feldolgozott útvonalakat egy listában adja vissza.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static List<CurriculumPathData> JsonArrayToList(JSONNode json)
    {
        List<CurriculumPathData> list = new List<CurriculumPathData>();

        for (int i = 0; i < json.Count; i++)
            list.Add(new CurriculumPathData(json[i]));

        return list;
    }
}
