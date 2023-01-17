using SimpleJSON;

public class CurriculumInfoForCurriculumPath {
    public string subjectID { get; private set; }
    public string topicID { get; private set; }
    public string courseID { get; private set; }
    public string curriculumID { get; private set; }
    public int countPlannedGames { get; private set; }
    public float maxCurriculumProgress { get; private set; }

    public CurriculumInfoForCurriculumPath(JSONNode json)
    {
        subjectID = json[C.JSONKeys.subjectID].Value;
        topicID = json[C.JSONKeys.topicID].Value;
        courseID = json[C.JSONKeys.courseID].Value;
        curriculumID = json[C.JSONKeys.curriculumID].Value;
        countPlannedGames = json[C.JSONKeys.countPlannedGames].AsInt;
        maxCurriculumProgress = json[C.JSONKeys.maxCurriculumProgress].AsFloat;
    }

    public void SetMaxCurriculumProgress(float value) {
        maxCurriculumProgress = value;
    }
}
