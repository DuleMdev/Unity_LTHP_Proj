using UnityEngine;

[CreateAssetMenu(fileName = "AppSetup", menuName = "AppSetupData", order = 1)]
public class AppSetup : ScriptableObject
{
    public string defaultLanguage;
    [Header("Version codes")]
    public string TestServerOld;
    public string LiveServerOld;
    public string TestServer2020;
    public string LiveServer2020;
    [Header("Main menu items")]
    [ToggleLeft]
    public bool PlayRoutes;
    [ToggleLeft]
    public bool MainPageGroupBrowser;
    [ToggleLeft]
    public bool MyGroupsEdit;
    [ToggleLeft]
    
    public bool CurriculumPlay;
    [ToggleLeft]
    public bool SubjectList;
    [ToggleLeft]
    public bool LiveStream;
    [ToggleLeft]
    public bool MarketPlace;
    [ToggleLeft]
    public bool MarketPlace2021;
    [ToggleLeft]
    public bool ExercisedCurriculum;
    [ToggleLeft]
    public bool LastPlayedCurriculum;
    [ToggleLeft]
    public bool MyResult;
    [ToggleLeft]
    public bool LearnPlay;
    [ToggleLeft]
    public bool MyRewards;
}