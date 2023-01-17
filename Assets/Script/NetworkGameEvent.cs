using UnityEngine;
using System.Collections;

// Konstansokat tartalmazó osztály
public class C {

    /// <summary>
    /// Programban használatos konstansok
    /// </summary>
    public class Program
    {
        // Tananyag fa bejárásához
        public const string MainMenu = "MainMenu";
        public const string Subject = "Subject";
        public const string Topic = "Topic";
        public const string Course = "Course";
        public const string Curriculum = "Curriculum";

        public const string LanguageSelector = "LanguageSelector";
        public const string FadeClickLanguageSelector = "FadeClickLanguageSelector";

        public const string CurriculumPath = "CurriculumPath";
        public const string GetLastXCurriculum = "GetLastXCurriculum";
        public const string GetUnderXPercentCurriculum = "GetUnderXPercentCurriculum";
        public const string PlayRoutes = "PlayRoutes";
        public const string MainPageGroupBrowser = "MainPageGroupBrowser";
        public const string MyGroupsEdit = "MyGroupsEdit";

        public const string Dots = "Dots";
        public const string Default = "Default";
        public const string BrowseCurriculum = "BrowseCurriculum";
        public const string Flag = "Flag";

        public const string SortChange = "SortChange";  // Megváltoztatták a tananyag rendezés szempontját

        // Tananyag listázásnak a scope-jai
        public const string shared = "shared";
        public const string own = "own";
        public const string groupShared = "groupShared";

        // A játék menüben 
        public const string GameMenuPrevious = "GameMenuPrevious";
        public const string GameMenuNext = "GameMenuNext";
        public const string GameMenuExit = "GameMenuExit";

        // Értékelő képernyő gombjai
        public const string EvaluateNext = "EvaluateNext";
        public const string EvaluateExit = "EvaluateExit";

        // Email csoportok listázásánál megtalálható gombok eseményei
        public const string Subscribe = "Subscribe";
        public const string Unsubscribe = "Unsubscribe";
        public const string InvitedAcceptance = "InvitedAcceptance";
        public const string InvitedRejection = "InvitedRejection";
        public const string Play = "Play";

        public const string FadeClick = "FadeClick";
        /*
        public const string gameIsFinished = "gameIsFinished";
        public const string gameOutOfTime = "gameOutOfTime";
        public const string gameMenuExit = "gameMenuExit";
        */
    }

    /// <summary>
    /// Képernyők nevei
    /// </summary>
    public class Screens
    {
        public const string EmptyScreen = "EmptyScreen";

        public const string WebGLScreen = "WebGLScreen";
        public const string IntroScreen = "IntroScreen";

        public const string HelloMentorLogin = "HelloMentorLogin";
        public const string ProvocentLogin = "ProvocentLogin";
        public const string TanletLogin = "TanletLogin";
        public const string LearnThenPlayLogin = "LearnThenPlayLogin";
        public const string entrepreneurLogin = "entrepreneurLogin";
        public const string LinglandLogin = "LinglandLogin";
        public const string SzekelyTermelokLogin = "SzekelyTermelokLogin";
        public const string StorieLogin = "StorieLogin";
        public const string SmartEmathsLogin = "SmartEmathsLogin";
        public const string StartUPLogin = "StartUPLogin";
        public const string MarketLogin = "MarketLogin";

        public const string OTPLogin = "OTPLogin";
        public const string OTPMain = "OTPMain";

        public const string ClassYIntroScreen = "ClassYIntroScreen";
        public const string ClassYLogin = "ClassYLogin";

        public const string ClassYEDUDrive = "ClassYEDUDrive";
        public const string ClassYEDUStore = "ClassYEDUStore";

        public const string MenuClassYLogin = "MenuClassYLogin";
        public const string MenuClassYEmailRegistration = "MenuClassYEmailRegistration";
        public const string MenuClassYEmailLogin = "MenuClassYEmailLogin";
        public const string MenuClassYMain = "MenuClassYMain";

        public const string ClassroomScreens = "ClassroomScreens";

        public const string MenuLogin = "MenuLogin";
        public const string MenuSynchronize = "MenuSynchronize";
        public const string MenuLessonPlanList = "MenuLessonPlanList";
        public const string MenuLessonPlan = "MenuLessonPlan";
        public const string MenuSetup = "MenuSetup";

        public const string ClientStartScreen = "ClientStartScreen";
        public const string MenuClientWaitStart = "MenuClientWaitStart";
        public const string MenuClientGrouping = "MenuClientGrouping";

        public const string BubbleGame3 = "BubbleGame3";
        public const string MillionaireGame = "MillionaireGame";
        public const string TrueOrFalseGame = "TrueOrFalseGame";
        public const string SetGame = "SetGame";
        public const string MathMonsterGame = "MathMonsterGame";
        public const string FishGame = "FishGame";
        public const string AffixGame = "AffixGame";
        public const string BoomGame = "BoomGame";
        public const string HangmanGame = "HangmanGame";
        public const string ReadGame = "ReadGame";

        public const string MarketPlace2021 = "MarketPlace2021";

        public const string Ball_Game = "Ball_Game";
        public const string CastleGameInstructionScreen = "CastleGameInstructionScreen";
        public const string CastleGameSelectCharacters = "CastleGameSelectCharacters";
        public const string CastleGameRoomScreen = "CastleGameRoomScreen";
        public const string CastleGameLevelUpOrDownScreen = "CastleGameLevelUpOrDownScreen";
        public const string CastleGameLevelUpScreen = "CastleGameLevelUpScreen";
        public const string CastleGameLevelDownScreen = "CastleGameLevelDownScreen";
        public const string CastleGameFinishScreen = "CastleGameFinishScreen";
        public const string CastleGameInventoryScreen = "CastleGameInventoryScreen";
    }

    public class DirFileNames {
        // Directory nevek
        public const string logDirName = "log"; // A lecketervek tárolására használt directory neve
        public const string lessonsDirName = "lessons"; // A lecketervek tárolására használt directory neve
        public const string classesDirName = "classes"; // Az osztályok tárolására használt directory neve
        public const string reportsDirName = "reports"; // Az óratervben a tanulók által elért eredmények tárolására

        // File nevek
        public const string configFileName = "config.json"; // A program beállításainak tárolására használt filenév
        public const string teacherConfigFileName = "config.json"; // A tanárokhoz rendelt adatok tárolására használt filenév
        public const string lastLessonPlanName = "lastLessonPlane.json"; // A kliensen az utoljára futtatot lecketerv
    }

    public class JSONKeys
    {
        // Email csoport listák képeinek kezeléséhez
        public const string EmailGroupPictureFiles = "EmailGroupPictureFiles";
        public const string LastDay = "LastDay";
        public const string fileName = "fileName";
        public const string unusedDays = "unusedDays";


        // Config fájlban szereplő json kulcs nevek
        public const string playAnimations = "playAnimations";
        public const string statusTableInSuper = "statusTableInSuper";
        public const string statusTableBetweenSuper = "statusTableBetweenSuper";

        public const string serverAddress = "serverAddress";
        public const string portNumber = "portNumber";
        public const string maxConnectionNumber = "maxConnectionNumber";

        public const string teachers = "teachers";
        public const string languages = "languages";
        public const string appLanguage = "appLanguage";
        public const string appLanguages = "appLanguages";
        public const string curriculumLanguage = "curriculumLanguage";
        public const string publicEmailGroupsLanguage = "publicEmailGroupsLanguage";
        public const string code = "code";
        public const string translate_change = "translate_change";
        public const string playRoutes_ScrollPosOfEmailGroup = "playRoutes_ScrollPosOfEmailGroup";

        // Times osztályban található json kulcs nevek
        public const string day = "day";
        public const string startHour = "sh";
        public const string startMinute = "sm";
        public const string endHour = "eh";
        public const string endMinute = "em";

        // Szerver - Tanári tablet kommunikációban szereplő json kulcs nevek
        public const string task = "task";

        public const string command = "command"; // ClassY
        public const string userName = "userName"; // ClassY
        public const string loginPassword = "loginPassword"; // ClassY
        public const string reNewPassword = "reNewPassword"; // ClassY
        public const string LoginEmail = "loginEmail"; // ClassY
        public const string thisIsTablet = "thisIsTablet"; // ClassY
        public const string scope = "scope"; // ClassY
        public const string appID = "appID"; // ClassY
        public const string duck = "duck"; // ???????????
        public const string duckID = "duckID"; // AppID
        public const string duckVersion = "duckVersion"; // Az alkalmazás verzió száma
        public const string pushProvider = "pushProvider"; // Az alkalmazás verzió száma
        public const string deviceIDforPush = "deviceIDforPush"; // Az alkalmazás verzió száma
        public const string deviceFingerprint = "deviceFingerprint"; // Az alkalmazás verzió száma
        public const string deviceModel = "deviceModel"; // Az alkalmazás verzió száma
        public const string graphicsDeviceName = "graphicsDeviceName"; // Az alkalmazás verzió száma
        public const string operatingSystem = "operatingSystem"; // Az alkalmazás verzió száma
        public const string where = "where";


        public const string language = "language"; // GetLanguageData lekérdezéshez
        public const string frontendVersion = "frontendVersion"; // GetLanguageData lekérdezéshez

        public const string languageID = "languageID"; // ClassY
        public const string langID = "langID"; // ClassY
        public const string langCode = "langCode"; // ClassY
        public const string langName = "langName"; // ClassY
        public const string langFlag = "langFlag"; // ClassY
        public const string usableLangCodes = "usableLangCodes";

        public const string error = "error"; // ClassY
        public const string answer = "answer"; // ClassY
        public const string pathData = "pathData"; // ClassY

        public const string sessionToken = "sessionToken"; // ClassY
        public const string searchString = "searchString"; // ClassY

        public const string mailListID = "mailListID"; // ClassY
        public const string invitationID = "invitationID"; // ClassY
        public const string invitationAnswer = "invitationAnswer"; // ClassY
        public const string learnRoutePathID = "learnRoutePathID"; // ClassY
        public const string learnRoutePathStart = "learnRoutePathStart"; // ClassY
        public const string logID = "logID"; // ClassY
        public const string subjectID = "subjectID"; // ClassY
        public const string topicID = "topicID"; // ClassY
        public const string courseID = "courseID"; // ClassY
        public const string curriculumID = "curriculumID"; // ClassY
        public const string newCurriculumIsolation = "newCurriculumIsolation"; // ClassY - tananyag lejátszáshoz
        public const string lastCurriculumIsolation = "lastCurriculumIsolation"; // ClassY - tananyag lejátszáshoz
        public const string lastLogID = "lastLogID"; // ClassY - tananyag lejátszáshoz
        public const string cssClass = "cssClass"; // ClassY - tananyag lejátszáshoz

        public const string previousLogID = "previousLogID"; // ClassY
        public const string nextLogID = "nextLogID"; // ClassY

        public const string testToken = "testToken"; // ClassY
        public const string replayToken = "replayToken"; // ClassY
        public const string shareToken = "shareToken"; // ClassY
        public const string version = "version"; // ClassY

        public const string appSettings = "appSettings"; // ClassY




        // statusData json kulcs nevei
        public const string statusData = "statusData";
        public const string show = "show";
        public const string isComplete = "isComplete";
        public const string gameScore = "gameScore";
        public const string maxGameScore = "maxGameScore";
        public const string starCount = "starCount";
        public const string learnRoutePathProgress = "learnRoutePathProgress";
        public const string learnRoutePathScore = "learnRoutePathScore";
        public const string levelIndicator = "levelIndicator";
        public const string levelIndicatorText = "levelIndicatorText";
        public const string statusInfo = "statusInfo";
        public const string nextGameScore = "nextGameScore";
        public const string currentLevelNumber = "currentLevelNumber";
        public const string possibleMaxLevelNumber = "possibleMaxLevelNumber";
        public const string coinReceived = "coinReceived";
        public const string collectedFrameGameChests = "collectedFrameGameChests";
        public const string characters = "characters";
        public const string treasures = "treasures";
        public const string disposableChestScore = "disposableChestScore";
        public const string nextChestValue = "nextChestValue";
        public const string frameGameWalk = "frameGameWalk";
        public const string nextGamePartOfLastGame = "nextGamePartOfLastGame";

        // liveStream
        public const string streamLink = "streamLink";
        public const string streamTimes = "streamTimes";
        public const string streamName = "streamName";
        public const string time_from = "time_from";
        public const string time_to = "time_to";
        public const string gmt_offset = "gmt_offset";
        public const string notification_count = "notification_count";

        // SubjectList
        public const string id = "id";
        public const string name = "name";
        public const string noticeNumber = "noticeNumber";
        public const string incompleteCurriculumNumber = "incompleteCurriculumNumber";

        // CurriculumInfoForCurriculumPath
        public const string countPlannedGames = "countPlannedGames";
        public const string maxCurriculumProgress = "maxCurriculumProgress";
        public const string scorePercent = "scorePercent";
        public const string playTimeString = "playTimeString";
        public const string replayable = "replayable";
        public const string newLearnRoutePathIsolationTimeForContinue = "newLearnRoutePathIsolationTimeForContinue";
        public const string lastLearnRoutePathIsolationTimeForContinue = "lastLearnRoutePathIsolationTimeForContinue";
        public const string flow_Style = "flow_Style";
        public const string gameTheme = "gameTheme";
        public const string gameEnding = "gameEnding";
        public const string playAnimation = "playAnimation";
        public const string isLaTeX = "isLaTeX";
        public const string frameGame = "frameGame";
        public const string collectBonusCoins = "collectBonusCoins";
        public const string hasFrameGameChests = "hasFrameGameChests";
        public const string frameGameCharacters = "frameGameCharacters";
        public const string appPlayTimeString = "appPlayTimeString";

        // CastleGame
        public const string bonusCoins = "bonusCoins"; // Rendelkezésre álló játék érmék (coin-ok) száma
        public const string availableFrameGameCharacters = "availableFrameGameCharacters";
        public const string availableFrameGameTreasure = "availableFrameGameTreasure";
        public const string bonusGames = "bonusGames"; // Az érméken választható játékok
        public const string heroID = "heroID"; 
        public const string monsterID = "monsterID"; 
        public const string victimID = "victimID";
        public const string hero = "hero";
        public const string monster = "monster";
        public const string victim = "victim";
        public const string isDefault = "isDefault"; // A Caracter mindenkinek alapból megvan
        public const string price = "price"; 
        public const string extraURL = "extraURL"; 


        // CurriculumPathData
        public const string progress = "progress";
        public const string curriculumData = "curriculumData";

        public const string subjects = "subjects";
        public const string subject = "subject";
        public const string topics = "topics";
        public const string topic = "topic";
        public const string courses = "courses";
        public const string curriculums = "curriculums";

        // keret játék (labdás ugrálós)
        public const string bonusGameID = "bonusGameID";    // A választott játék azonosítója
        public const string bonusGamePurchaseID = "bonusGamePurchaseID";    // A vásárolt játék végeredményének elküldéséhez
        //public const string maxGameScore = "maxGameScore";    // Az eddigi elért legtöbb pont // StatusData-ban is van egy hasonló nevű kulcs
        public const string maxGameLevel = "maxGameLevel";    // Az eddigi elért letmagasabb szint
        public const string currentBonusCoins = "currentBonusCoins";    // Hány Coinnal rendelkezünk
        public const string bonusGameLogData = "bonusGameLogData";  
        //public const string gameScore = "gameScore"; Az értékelő táblán is van egy hasonló nevű érték
        public const string gameLevel = "gameLevel";


        // MarketPlace
        public const string currentModuleRemainingTime = "currentModuleRemainingTime";
        public const string nextModuleRemainingTime = "nextModuleRemainingTime";

        public const string ownProfile = "ownProfile";
        public const string yield = "yield";


        public const string companyProfile = "companyProfile";
        public const string partners = "partners";
        public const string coinsFromLearning = "coinsFromLearning";
        public const string coinsFromItems = "coinsFromItems";

        public const string marketSituation = "marketSituation";
        public const string minCompanyValue = "minCompanyValue";
        public const string maxCompanyValue = "maxCompanyValue";
        public const string ownCompanyValue = "ownCompanyValue";
        public const string ownCompanyPlace = "ownCompanyPlace";

        // MarketPlace2021 - Certificate
        public const string achievements = "achievements";
        public const string playTime = "playTime";
        public const string playCount = "playCount";
        public const string bronzeCoinsFromLearning = "bronzeCoinsFromLearning";
        public const string maxCoinsFromLearning = "maxCoinsFromLearning";
        public const string coinsFromLearningPercent = "coinsFromLearningPercent";

        public const string copanyName = "copanyName";
        public const string silverCoinsFromLearning = "silverCoinsFromLearning";
        public const string capitalGains = "capitalGains";
        public const string companyValue = "companyValue";
        public const string companyPlace = "companyPlace";
        //public const string investable = "investable";
        public const string invested = "invested";
        public const string mostValuable = "mostValuable";
        public const string leastValuable = "leastValuable";
        public const string lastPlace = "lastPlace";

        public const string investmentsYield = "investmentsYield";
        public const string exitYield = "exitYield";
        //public const string yield = "yield";
        public const string allGoldCoins = "allGoldCoins";
        public const string mostGoldCoins = "mostGoldCoins";
        public const string leastGoldCoins = "leastGoldCoins";
        public const string place = "place";


        //public const string investable = "investable";

        //public const string ownProfile = "ownProfile";


        // MarketPlace ProjectProductData
        public const string marketModuleID = "marketModuleID";
        public const string observedGroupName = "observedGroupName";
        public const string gameName = "gameName";
        public const string investable = "investable";
        public const string isOwnInvestment = "isOwnInvestment";



        // Befektetés elküdéséhez a szervernek 
        public const string  marketModuleItemID = "marketModuleItemID";

        // MarketPlace2021
        public const string badges = "badges";
        public const string badgeCount = "badgeCount";
        public const string badgeName = "badgeName";

        // WebShop
        public const string webshop = "webshop";
        public const string webshopID = "webshopID";
        public const string itemName = "itemName";
        public const string itemImage = "itemImage";
        public const string itemPrice = "itemPrice";
        public const string purchasedQuantity = "purchasedQuantity";

        public const string webshopItemID = "webshopItemID";
        public const string quantity = "quantity";

        // Email csoportok listázásához
        public const string isPublic = "isPublic";
        public const string joinLevel = "joinLevel";
        public const string autoAddUser = "autoAddUser";
        public const string picture = "picture";
        public const string description = "description";
        public const string userStatus = "userStatus";
        public const string joinedDate = "joinedDate";
        public const string profilePicture = "profilePicture";
        public const string userEmail = "userEmail";
        public const string currentPrivileges = "currentPrivileges";
        public const string privilege = "privilege";
        public const string hasPrivilege = "hasPrivilege";
        public const string isOwn = "isOwn";
        public const string owner = "owner";

        public const string requestName = "requestName";
        public const string color = "color";
        public const string bigItems = "bigItems";
        public const string languageFilter = "languageFilter";


        // CurriculumForPlay
        public const string automatedGames = "automatedGames";
        public const string plannedGames = "plannedGames";

        // Játék adatok beolvasásához
        public const string gameData = "gameData";
        public const string engine = "engine";
        public const string images = "images";

        // Játék képernyők beolvasásához
        // Általános
        public const string image = "image";
        public const string imageBorder = "imageBorder";
        public const string goodAnswerNeed = "goodAnswerNeed";
        public const string tries = "tries";
        public const string info = "info";

        public const string userAnswers = "userAnswers";

        public const string questionIndex = "questionIndex"; // A szervertől így jön vissza a subQuestionID

        // Játék specifikus
        // Affix
        public const string treeItemID = "treeItemID";  // A fán megtalálható részkérdés azonosítója
        public const string solutions = "solutions";
        public const string solutionID = "solutionID";
        public const string solution = "solution";
        public const string distractors = "distractors";
        public const string distractorID = "disctractorID";
        public const string distractor = "distractor";

        // Fish
        public const string fishSentenceID = "fishSentenceID";
        public const string sentence = "sentence";
        public const string is_swappable = "is_swappable";

        // MathMonster
        public const string mathMonsterSentenceID = "mathMonsterSentenceID";
        public const string answerIndex = "answerIndex";

        // Boom
        public const string is_image = "is_image";
        public const string is_correct = "is_correct";

        // Bubble
        public const string goodAnswerPiece = "goodAnswerPiece";

        // Hangman
        public const string puzzle = "puzzle";

        // Read
        public const string text_title = "text_title";
        public const string text_content = "text_content";
        public const string answerGroups = "answerGroups";

        // Sets
        public const string setsGroupID = "setsGroupID";
        public const string sets = "sets";
        public const string items = "items";
        public const string setsID = "setsID";

        // TrueOrFalse
        public const string true_or_false = "true_or_false";

        // PDF
        public const string pdf = "pdf";
        public const string pdfLinks = "pdfLinks";


        // YouTube
        public const string link = "link";
        public const string subtitle = "subtitle";

        // Texty
        public const string text = "text";
        public const string inOrder = "inOrder";

        // ClassY addGameLog-ban található json kulcs nevek
        public const string gamePercent = "gamePercent";
        public const string gameStart = "gameStart";
        public const string gameEnd = "gameEnd";
        public const string extraData = "extraData";
        public const string curriculumStart = "curriculumStart";
        public const string curriculumProgress = "curriculumProgress";

        // Bug report
        public const string comment = "comment";
        public const string gameJson = "gameJson";



        public const string username = "username";
        public const string password = "password";
        public const string recentUserNames = "recentUserNames";
        public const string passMD5 = "passMD5";
        public const string response = "response";
        public const string userID = "userid";
        public const string email = "email";
        public const string realName = "realname";
        public const string usertype = "usertype";

        public const string classes = "classes";
        public const string lessons = "lessons";
        public const string times = "times";
        public const string students = "students";

        public const string studentDataUploadNeed = "studentDataUploadNeed";
        public const string studentID = "sid";
        public const string studentName = "sname";
        public const string studentNotes = "snotes";
        public const string studentStars = "stars";
        public const string studentPoints = "points";
        public const string studentTabletUniqueIdentifier = "studentTabletUniqueIdentifier";

        public const string classid = "classid";
        public const string className = "classname";
        public const string schoolName = "schoolname";
        public const string classNotes = "classnotes";
        public const string classStudents = "students";

        public const string lessonid = "id"; // Előrébb is megtalálható
        public const string lessonName = "name";
        public const string lessonLabels = "labels";
        public const string lessonSubject = "subject";
        public const string lessonLanguage = "language";
        public const string lessonSynchronizeTime = "synchronizeTime";

        public const string serverDateTime = "serverDateTime";

        // LessonMosaicData objektumhoz használatos json kulcs nevek
        public const string lessonMosaicName = "name";
        public const string multiPlayer = "multiplayer";
        public const string fixGames = "fixGames";
        public const string games = "games";

        // CurriculumData
        public const string lastCurriculumIsolationTimeForContinue = "lastCurriculumIsolationTimeForContinue";
        public const string newCurriculumIsolationTimeForContinue = "newCurriculumIsolationTimeForContinue";
        public const string levelInfo = "levelInfo";

        // GameData objektumhoz használatos json kulcs nevek
        //public const string id = "id"; // Előrébb is megtalálható
        public const string gameID = "gameID";
        public const string gameDifficulty = "gameDifficulty";
        public const string avgPlayTime = "avgPlayTime";
        public const string lastGamePercent = "lastGamePercent";
        public const string labels = "labels";
        public const string fixScreen = "fixScreens";
        public const string screens = "screens";

        // TaskData objektumokban használatos json kulcs nevek
        public const string screenQuestion = "screenQuestion";
        public const string screenquestion = "screenquestion";
        public const string question = "question";
        public const string keyset = "keyset";
        public const string psycho_question = "psycho_question";
        public const string psycho_instructions = "psycho_instructions";

        // Képernyőkben használatos json kulcs nevek
        public const string time = "time";
        public const string lifeNumber = "lifeNumber";

        // Játék vezérlésben használatos json kulcs nevek
        public const string lessonMosaicIndex = "lessonMosaicIndex";
        public const string gameIndex = "gameIndex";
        public const string screenIndex = "screenIndex";

        public const string extraInfo = "extraInfo";
        public const string questionOrder = "questionOrder";
        public const string answerOrder = "answerOrder";

        public const string gameEventType = "gameEventType";

        // Játék kiértékelésében használatos kulcsnevek
        public const string multi = "multi";
        public const string onlyBadge = "onlyBadge";
        public const string allStar = "allStar";
        public const string evaluate = "evaluate";

        public const string result = "result";
        public const string resultNew = "resultNew";
        public const string groupStar = "groupStar";
        public const string groupStarNew = "groupStarNew";
        public const string itWasLastGame = "itWasLastGame";

        public const string allGroupStar = "allGroupStar";
        public const string monsterOrder = "monsterOrder";
        public const string bestTeamMember = "bestTeamMember";
        public const string cleverestStudent = "cleverestStudent";
        public const string fastestStudent = "fastestStudent";
        public const string longest3StarSeries = "longest3StarSeries";
        public const string showTime = "showTime";
        public const string levelBorder = "levelBorder";

        public const string lessonPlanEnd = "lessonPlanEnd";

        // Tanári tablet - Diák tablet kommunikációban szereplő json kulcs nevek
        public const string dataContent = "dataContent";

        public const string clientID = "clientID";
        public const string clientName = "clientName";
        public const string clientUniqueIdentifier = "uniqueIdentifier";
        public const string clientGroupID = "clientGroupID";
        public const string clientGroupScreenShow = "clientGroupScreenShow";

        public const string player = "player";

        public const string lessonPlan = "lessonPlan";

        public const string gameEvent = "gameEvent";
        public const string gameEventData = "gameEventData";

        // Általános játék események
        public const string timeEvent = "timeEvent";    // Szerver küldi a játékban megmaradt idővel
        public const string answers = "answers";
        public const string finalMessage = "finalMessage";          // Végső üzenet a játékokból
        public const string elapsedGameTime = "elapsedGameTime";    // A játékban eltöltött idő

        // Igaz vagy hamis (TrueOrFalse) események
        public const string selectedQuestion = "selectedQuestion";
        public const string selectedSubQuestion = "selectedSubQuestion";
        public const string selectedAnswer = "selectedAnswer";
        public const string evaluateAnswer = "evaluateAnswer";
        public const string answerCount = "answerCount";    // Hányadik válasz

        // 
        public const string replayMode = "replayMode";

        // Boom játék eseményei
        public const string showItem = "showItem";  // Melyik elemet kell a tévén mutatni, ha üres string, akkor az üres (vibráló) képet kell mutatni
        public const string showItemIsPicture = "showItemIsPicture";  // Az elem képet tartalmaz-e

        // MathMonster és Fish játék eseménye
        public const string questions = "questions";    // A kérdéseket megadó tömb a MathMonster és a Fish játékokban

        // Psycho játék 
        public const string prevAnswers = "prevAnswers";
        //public const string gameEnd = "date";
        public const string psychoAnswer = "psychoAnswer";

        public const string type = "type"; // ExtraData megadásához
        public const string data = "data"; // ExtraData megadásához 

        // Drag & Drop játékok eseményei
        public const string dragPosX = "dragPosX";  // 
        public const string dragPosY = "dragPosY";  //

        // ReportDatas
        public const string screenID = "screenID";
        public const string questionID = "questionID";
        public const string subQuestionID = "subQuestionID";
        public const string answerID = "answerID";
        public const string isGood = "isGood";
        public const string answerTime = "answerTime";

        // ReportEvent 
        public const string goodAnswers = "goodAnswers";
        public const string wrongAnswers = "wrongAnswers";
        public const string resultPercent = "resultPercent";
        public const string startTime = "startTime";
        public const string endTime = "endTime";
        public const string gameEndType = "gameType";

        // ReportLessonPlan
        public const string events = "events";

        public const string unlockPin = "unlockPin";
        public const string parents = "parents";

    }

    public class JSONValues {

        // Szerver - Communikációban szereplő json kulcs nevek - ClassY
        public const string userLogin = "userLogin";
        public const string userForgotPassword = "userForgotPassword";
        public const string userRegistration = "userRegistration";

        public const string getHomeworks = "getHomeworks";

        public const string getUsableLanguages = "getUsableLanguages"; // olyan nyelvek listája amilyen email listákban tagok vagyunk
        public const string getPublicMailListsLanguages = "getPublicMailListsLanguages"; // olyan nyelvek listája amilyen nyelveken vannak publikus csosportok

        public const string getSupportedLanguages = "getSupportedLanguages";
        public const string getAllSupportedLanguagesData = "getAllSupportedLanguagesData";
        public const string getLanguageData = "getLanguageData";

        public const string getLearnRoutePathForPlay = "getLearnRoutePathForPlay";
        public const string getPlayableLearnRoutePathList = "getPlayableLearnRoutePathList";

        public const string getStreamData = "getStreamData"; // Stream info

        public const string getCurriculumSubjects = "getCurriculumSubjects"; // tantárgyak listája
        public const string getCurriculumTopics = "getCurriculumTopics"; // tantárgy témái
        public const string getCurriculumCourses = "getCurriculumCourses"; // témák kurzusai
        public const string getCurriculums = "getCurriculums"; // Kurzusok tananyagai
        // Nem menü
        public const string getSubjectsByMailList = "getSubjectsByMailList"; // tantárgyak listája
        public const string getTopicsByMailList = "getTopicsByMailList"; // tantárgy témái
        public const string getCoursesByMailList = "getCoursesByMailList"; // témák kurzusai
        public const string getCurriculumsByMailList = "getCurriculumsByMailList"; // Kurzusok tananyagai

        public const string getCurriculumItems = "getCurriculumItems"; // 
        public const string getMarketModuleItems = "getMarketModuleItems"; // 
        public const string getMarketAchievements = "getMarketAchievements"; // 
        public const string setUserInvestment = "setUserInvestment"; // 
        public const string getWebshopItems = "getWebshopItems"; // 
        public const string purchaseWebshopItem = "purchaseWebshopItem"; // 
        public const string getGameForPlay = "getGameForPlay"; // 
        public const string getCurriculumForPlay = "getCurriculumForPlay"; // 
        public const string getNextGame = "getNextGame"; // 
        public const string getGameForTest = "getGameForTest"; // 
        public const string getReplayData = "getReplayData"; // 
        public const string getLoggedGame = "getLoggedGame"; // 
        public const string getNextPracticeGame = "getNextPracticeGame"; // 
        public const string getPracticeLoggedGame = "getPracticeLoggedGame"; // 

        public const string addGameLog = "addGameLog";
        public const string getLastX = "getLastX";
        public const string getUnderXPercent = "getUnderXPercent";
        public const string eraseUserLog = "eraseUserLog";

        public const string getUserMailListInfo = "getUserMailListInfo";
        public const string getListOfMailLists = "getListOfMailLists";
        public const string getPlayRoutes = "getPlayRoutes"; // Csoport böngésző panelhez
        public const string getMyGroupsEmailLists = "getMyGroupsEmailLists"; // Csoportjaim panelhez
        public const string getMyGroupEditEmailLists = "getMyGroupEditEmailLists"; // Csoportjaim szerkesztése panelhez
        public const string getOwnMailLists = "getOwnMailLists"; // Azoknak a csoportoknak a lekérdezéséhez amit én hoztam létre
        public const string getWhereIAmAdminMailLists = "getWhereIAmAdminMailLists"; // Azoknak a csoportoknak a lekérdezéséhez ahol admin vagyok
        public const string getWhereIHavePermissionMailLists = "getWhereIHavePermissionMailLists"; // Azoknak a csoportoknak a lekérdezéséhez amikben editContent joga van a felhasználónak
        public const string getWhereIAmOnMailLists = "getWhereIAmOnMailLists"; // Azoknak a csoportoknak a lekérdezéséhez ahol tag vagyok
        public const string getPublicMailLists = "getPublicMailLists"; // Publikus csoportoknak a lekérdezéséhez
        public const string getInvitedMailLists = "getInvitedMailLists"; // Azoknak a csoportoknak a lekérdezéséhez amikbe meghívtak
        public const string getSubPendingMailLists = "getSubPendingMailLists"; // Azoknak a csoportoknak a lekérdezéséhez ahová jelentkeztem, de el kell fogadniuk a jelentkezést

        public const string subToMailList = "subToMailList"; // Feliratkozás egy csoportra
        public const string unsubFromMailList = "unsubFromMailList"; // Leiratkozás egy csoportról
        public const string forceAnswerInvitation = "forceAnswerInvitation"; // Válasz a meghívásra
        public const string appMyGroupsEdit = "appMyGroupsEdit";
        public const string appMainPageGroupBrowser = "appMainPageGroupBrowser";


        public const string getUserBonusCoins = "getUserBonusCoins"; // Lekérdezzük a felhasználó zsetonjainak számát
        public const string purchaseBonusGame = "purchaseBonusGame";
        public const string updateBonusGameLog = "updateBonusGameLog";
        public const string getUserInventory = "getUserInventory";
        public const string setFrameGameCharacters = "setFrameGameCharacters";
        
        public const string getShareTokenData = "getShareTokenData";

        public const string sendBugReport = "sendBugReport";

        public const string getUserProfile = "getUserProfile";
        public const string saveAppSettings = "saveAppSettings";




        // Szerver - Tanári tablet kommunikációban szereplő json kulcs nevek
        public const string login = "login";
        public const string getalldata = "getalldata";
        public const string Success = "Success";
        public const string Loginfailed = "Login failed";

        // dataContent lehetséges értékei
        public const string IDRequest = "IDRequest";
        public const string clientID = "clientID";
        public const string lessonPlanRequest = "lessonPlanRequest";
        public const string lessonPlan = "lessonPlan";
        public const string lessonPlanTransferOk = "lessonPlanTransferOk";

        public const string groupID = "groupID";
        public const string gameEvent = "gameEvent";
        public const string playStart = "playStart";
        public const string nextPlayer = "nextPlayer";
        public const string pauseOn = "pauseOn";
        public const string pauseOff = "pauseOff";

        public const string EvaluationScreen = "EvaluationScreen";
        public const string EvaluationScreenSingle = "EvaluationScreenSingle";
        public const string EvaluationScreenMulti = "EvaluatonScreenMulti";

        public const string nextOk = "nextOk";  // A játékos felkészült a következő játékra 

        // gameEventType lehetséges értékei
        public const string answer = "answer";              // Játékokban a választás
        public const string show = "show";                  // Boom játékban a képernyőn megjelenő elem
        public const string goodSolution = "goodSolution";  // A játék megoldva (buborékos)
        public const string wrongSolution = "wrongSolution";// A játék megoldva (buborékos)

        public const string drag = "drag";                  // Megfogtak egy mozgatható elemet
        public const string dragMove = "dragMove";          // Mozgatják a megfogott elemet
        public const string dragReleased = "dragReleased";  // Elengedték a megfogott elemet

        public const string finalMessage = "finalMessage";  // Végső üzenet

        public const string exitScreen = "exitScreen";      // Valamilyen okból kifolyólag ki kell lépni az aktuális képernyőből. pl. A game menüben megnyomták az előző, vagy a következő gombot, esetleg megnyomták a következő gombot Teszt játék vég esetén
        public const string exitGame = "exitGame";          // Valamilyen okból kifolyólag ki kell lépni a játékból. pl. A game menüben megnyomták az exit gombot
        public const string screenAgain = "screenAgain";    // Megnyomták az újra gombot a játékban. Újra indul a képernyő
        public const string nextReplay = "nextReplay";      // Replay módban megnyomták a következő gombot, ami a következő válasz mutatását jelenti

        // selectedAnswer lehetséges értékei

        // evaluateAnswer lehetséges értékei
        public const string evaluateIsTrue = "evaluateIsTrue";
        public const string evaluateIsFalse = "evaluateIsFalse";
        public const string evaluateIsSilent = "evaluateIsSilent"; // A kiértékelés eredményét nem kell közölni a felhasználóval, tehát nem fog villogni a válasz sem zöldel sem pirossal
        public const string evaluateIsIgnore = "evaluateIsIgnore"; // A válasz nem kiértékelhető, pl. drag and drop játékoknál a target már foglalt (nem lehet még egy elemet bele húzni)

        // Általános játék események
        public const string outOfTime = "outOfTime";    // Lejárt az idő
        public const string gameEnd = "gameEnd";        // Véget ért a játék

        public const string playerActive = "active";    // A játékos jön lépéssel player kulcs lehetséges értékei
        public const string playerPassive = "passive";  // Nem a játékos jön lépéssel

        public const string groupData = "groupData";
        public const string playStop = "playStop";

        public const string getFamilyConnections = "getFamilyConnections";
        public const string getCurrentHomeworks = "getCurrentHomeworks";
    }

    public class NetworkGameEvent {

        public const string SelectTrue = "SelectTrue";      // Az igaz hamis játékban az igaz gombot választották
        public const string SelectFalse = "SelectFalse";    // Az igaz hamis játékban a hamis gombot választották

        public const string KeyPressed = "KeyPressed";      // A hangman játékban kiválasztottak egy betűt

        public const string ViewStory = "ViewStory";        // A read játék a történetet mutatja
        public const string ViewQuestion = "ViewQuestion";  // A read játék a kérdéseket mutatja
        public const string ScrollBarPos = "ScrollBarPos";  // A read játékben állítottak a görgetősáv pozícióján
        public const string SelectQuestion = "SelectQuestion"; // A read játékban kiválaszottak egy kérdést
        public const string DeselectQuestion = "DeselectQuestion";   // A read játékban visszavonták a kérdés kiválasztását

        public const string SelectAnswer = "SelectAnswer";  // A read játékban kiválaszottak egy választ vagy
                                                            // A Milionaire játékban kiválasztottak egy választ            



        public const string OutOfTime = "OutOfTime";        // Az aktuális játékban lejárt az idő

        public const string FlowEnd = "FlowEnd";            // Vége a flow-nak (be kell fejezni a játékot és kilépni)
                                                            // Lehet, hogy át lesz nevezve LessonMosaicEnd-re

    }

    public class Texts // Ezek a szövegek vannak nyelvesítve
    {
        // WebGL
        public const string UnknownApp = "UnknownApp";

        // ClassY Login main
        public const string FacebookLogin = "FacebookLogin";
        public const string EmailLogin = "EmailLogin";
        public const string EmailRegistration = "EmailRegistration";
        public const string ForgotPass = "ForgotPass";

        // Üdvözlő kéeprnyő
        public const string Grettings = "Grettings";
        public const string GrettingsInfo = "GrettingsInfo";
        public const string GrettingsInfo2 = "GrettingsInfo2";

        public const string OTPGrettings = "OTPGrettings";
        public const string OTPGrettingsInfo = "OTPGrettingsInfo";

        // ClassY Email regisztration screen
        // ClassY Email login screen
        public const string UserName = "UserName";
        public const string Password = "Password";
        public const string PasswordAgain = "PasswordAgain";
        public const string EmailAddress = "EmailAddress";
        public const string Registration = "Registration";
        public const string RegistrationSuccess = "RegistrationSuccess"; // Regisztráció sikerült
        public const string ForgotPasswordSuccess = "ForgotPasswordSuccess"; // 
        public const string PrivacyPolicy = "PrivacyPolicy";

        // Kilépés
        public const string ExitConfirmation = "ExitConfirmation";

        // ClassY menü
        public const string SharedWithMe = "SharedWithMe";
        public const string OwnCurriculum = "OwnCurriculum";
        public const string MakeNewCurriculum = "MakeNewCurriculum";
        public const string EducationInClassroom = "EducationInClassroom";
        public const string Students = "Students";

        // OTP menü
        public const string CurriculumsPlay = "CurriculumsPlay";
        public const string LiveStream = "LiveStream";
        public const string MarketPlace = "MarketPlace";
        public const string MarketPlace2021 = "MarketPlace2021";
        public const string SubjectsList = "SubjectsList";
        public const string ExercisedCurriculum = "ExercisedCurriculum";
        public const string LastPlayedCurriculums = "LastPlayedCurriculums";
        public const string PlayRoutes = "PlayRoutes";
        public const string MainPageGroupBrowser = "MainPageGroupBrowser";
        public const string MyGroupsEdit = "MyGroupsEdit";
        public const string LearnThenPlay = "LearnThenPlay";
        public const string MyResult = "MyResult";
        public const string MyRewards = "MyRewards";

        // OTP oldal menü (A három csíkos menü gomb megnyomása után megjelenő oldal menü)
        public const string classYCurriculums = "classYCurriculums";
        public const string ClassroomPlay = "ClassroomPlay";

        // ClassY submenü
        public const string Back = "Back";

        // ClassY sort buttons
        public const string SortByNewCurriculum = "SortByNewCurriculum";
        public const string SortByUnaccomplishedCurriculum = "SortByUnaccomplishedCurriculum";
        public const string SortByName = "SortByName";
        public const string SortByMakeTime = "SortByMakeTime";
        public const string SortByResult = "SortByResult"; // Eredmény
        public const string SortByProgress = "SortByProgress"; // Haladás

        // Ha nincs megjelenítendő tananyag
        public const string NotCurriculum = "NotCurriculum";

        // LiveStream
        public const string NextStream = "NextStream";
        public const string WatchStream = "WatchStream";
        public const string NoStream = "NoStream";

        // MarketPlace MenuStripe
        public const string ProjectProducts = "ProjectProducts";
        public const string CompaniesProfiles = "CompaniesProfiles";
        public const string OwnProfile = "OwnProfile";

        //  MarketPlace projekt termékek
        public const string StartInvestmentPeriod = "StartInvestmentPeriod";
        public const string EndInvestmentPeriod = "EndInvestmentPeriod";
        public const string OwnInvestment = "OwnInvestment";
        public const string Company = "Company";
        public const string Hours = "Hours";

        // MarketPlace céges profilok
        public const string AllLearn = "AllLearn";
        public const string AllProjectProduct = "AllProjectProduct";
        public const string CompanyMembers = "CompanyMembers";
        public const string CollectByLearn = "CollectByLearn";
        public const string OwnerShip = "OwnerShip";

        public const string CompanyMarketState = "CompanyMarketState";
        public const string HighestCompanyValue = "HighestCompanyValue";
        public const string OwnCompanyValue = "OwnCompanyValue";
        public const string LowestCompanyValue = "LowestCompanyValue";

        // MarketPlace saját profil
        public const string AllYield = "AllYield";
        public const string ThisPeriodInvestable = "ThisPeriodInvestable";
        public const string EvenXHours = "EvenXHours";
        public const string InvestedPcoin = "InvestedPcoin";

        public const string MarketState = "MarketState";
        public const string MostSuccessfulInvestor = "MostSuccessfulInvestor";
        public const string OwnPlace = "OwnPlace";
        public const string LeastSuccessfulInvestor = "LeastSuccessfulInvestor";

        // MarketPlace felbukkanó ablak
        public const string ImInvestingMyPcoin = "ImInvestingMyPcoin";
        public const string ImInvesting = "ImInvesting";
        public const string PlayProjectProduct = "PlayProjectProduct";

        // Befektetés biztonsági kérdése
        public const string SureInvestment = "SureInvestment";

        // MarketPlace 2021
        public const string ExperianceBadges = "ExperianceBadges";

        public const string PersonalProfil = "PersonalProfil";
        public const string StartUpperPage = "StartUpperPage";
        public const string InvestorPage = "InvestorPage";
        public const string ProfitPage = "ProfitPage";
        public const string CertificatePage = "CertificatePage";

        public const string BronzeCoinDescription = "BronzeCoinDescription";
        public const string SilverCoinDescription = "SilverCoinDescription";
        public const string GoldCoinDescription = "GoldCoinDescription";

        public const string NoBusinessDevelopmentOpportunity = "NoBusinessDevelopmentOpportunity";
        public const string DividendSilver = "DividendSilver";
        public const string ReturnOnInvestment = "ReturnOnInvestment";

        public const string ProfitPageGoldDescription = "ProfitPageGoldDescription";
        public const string InvestorPageSilverDescription = "InvestorPageSilverDescription";

        public const string SureInvestmentSilver = "SureInvestmentSilver";

        public const string SureWebshopBuy = "SureWebshopBuy";


        // OTP tananyag lejátszás panelen található szövegek
        public const string ResultSum = "ResultSum";
        public const string StartLearning = "StartLearning";
        public const string PlayTime = "PlayTime";
        public const string AppPlayTime = "AppPlayTime";
        public const string CurriculumsOfGroup = "CurriculumsOfGroup";

        // ClassY CurriculumInfo
        public const string Completed = "Completed";
        public const string New = "New";
        public const string MadeBy = "MadeBy"; // Plusz előfordulás : CurriculumListDrive
        public const string Date = "Date";
        public const string Subject = "Subject";
        public const string SearchWords = "SearchWords";
        public const string Points = "Points";

        public const string ShareWithStudentsForLearn = "ShareWithStudentsForLearn";
        public const string ShareInCommonWork = "ShareInCommonWork";
        public const string ShareOnFacebook = "ShareOnFacebook";
        public const string ShareOnClassYStore = "ShareOnClassYStore";
        public const string Play = "Play";

        // ClassY LanguageSelector
        public const string SelectCurriculumLanguage = "SelectCurriculumLanguage";
        public const string SelectPublicEmailListLanguage = "SelectPublicEmailListLanguage"; // Válaszd ki a publikus csoport nyelvét!
        public const string SelectApplicationLanguage = "SelectApplicationLanguage";

        // EmailGroupJumpingPanel - Email csoport felugró panelján megjelenő szövegek
        public const string maker = "maker";

        // Játék értékelés (GameEvaluation) ablakban megjelenő szövegek
        public const string OnTheRouteSoFar = "OnTheRouteSoFar"; // Az útvonalban eddig: 
        public const string Level = "Level"; // Szint: 
        public const string ChipsGoToTreasury = "ChipsGoToTreasury"; // A jutalom-zsetonod bekerült a kincstárba. 

        // BallGame játékban megjelenő szövegek
        public const string BestScore = "BestScore"; 
        public const string Go = "Go";
        public const string YouMistake = "YouMistake";

        // FrameGame szövegei
        public const string FrameGameInstructionScreenTitle = "FrameGameInstructionScreenTitle";
        public const string FrameGameInstructionScreenDescription = "FrameGameInstructionScreenDescription";
        public const string FrameGameInstructionScreenChestDescription = "FrameGameInstructionScreenChestDescription";
        public const string FrameGameInstructionScreenCoinDescription = "FrameGameInstructionScreenCoinDescription";




        // Classroom képernyőkön megjelenő szövegek
        // TeacherStudentChoice
        public const string ChoiceYouAreTeacherOrStudent = "ChoiceYouAreTeacherOrStudent";
        public const string Teacher = "Teacher";
        public const string Student = "Student";

        // StudentStart (A tanuló tantermi oktatást indító képernyője)
        public const string ClassroomEducationStart = "ClassroomEducationStart";

        // OnlineOrOfflineMenu ( A tanár itt választhatja ki, hogy milyen módon szeretné az oktatást megtartani )
        public const string SelectEducationMode = "SelectEducationMode";
        public const string Online = "Online";
        public const string Offline = "Offline";
        public const string Start = "Start";
        public const string PathDownload = "PathDownload";
        public const string ClassroomGroupsDownload = "ClassroomGroupsDownload";
        public const string OfflineDatasUpload = "OfflineDatasUpload";

        // SelectEmailList
        public const string ClassroomGroupSelect = "ClassroomGroupSelect";

        // SelectPathForDownload (A tananyagok letöltése képernyőn megjelenő szövegek)
        public const string DownloadCurriculum = "DownloadCurriculum";
        public const string Download = "Download";

        // SelectPath (A tananyag kiválastása megtekintésere vagy tanórai lejátszásra képernyő)
        public const string DownloadedCurriculum = "DownloadedCurriculum";
        public const string Preview = "Preview";


        // Error messages
        public const string userNotLoggedIn = "userNotLoggedIn"; // A felhasználó nincs bejelentkezve
        public const string existUsername = "existUsername";
        public const string emptyUsername = "emptyUsername";
        public const string lengthUsername = "lengthUsername";
        public const string emptyPass = "emptyPass";
        public const string lengthPass = "lengthPass";
        public const string matchPass = "matchPass";
        public const string existEmail = "existEmail";
        public const string wrongEmail = "wrongEmail";
        public const string wrongPassword = "wrongPassword";
        public const string wrongPasswordError = "wrongPasswordError";
        public const string userAlreadyInvestedPoints = "userAlreadyInvestedPoints";
        public const string UserCanNotInvestPointsThere = "UserCanNotInvestPointsThere";

        // Error panel-en megjelenő szövegek
        public const string ThisLearnRouteIsFinished = "ThisLearnRouteIsFinished";
        public const string ThisCurriculumDoesNotIncludeGames = "ThisCurriculumDoesNotIncludeGames";
        public const string curriculumPlayWasStopped = "curriculumPlayWasStopped";
        public const string continue_ = "continue";
        public const string restart = "restart";
        public const string gameResultResend = "gameResultResend";
        public const string serverCommunicationRepeat = "serverCommunicationRepeat";

        public const string confirmationUnsubscribe = "confirmationUnsubscribe";
        public const string successSubscribe = "successSubscribe";
        public const string successSubscribeConfirmation = "successSubscribeConfirmation";
        public const string successUnsubscribe = "successUnsubscribe";
        public const string successInvitedAcceptance = "successInvitedAcceptance";
        public const string successInvitedRejection = "successInvitedRejection";

        public const string BugSendSuccess = "BugSendSuccess";



        // Game menü
        public const string nextGame = "nextGame";
        public const string nextCurriculum = "nextCurriculum";
        public const string exit = "exit";
        public const string gameGratulation = "gameGratulation";






        public const string TeacherTablet = "TeacherTablet";
        public const string StudentTablet = "StudentTablet";
        public const string Server = "Server";
        public const string Client = "Client";
        public const string ServerAddress = "ServerAddress";
        public const string Portnumber = "PortNumber";
        public const string MaximumClientNumber = "MaximumClientNumber";

        public const string Save = "Save";
        public const string Cancel = "Cancel";
        public const string Ok = "Ok";
        public const string Selection = "Selection";

        public const string PortNumberError = "PortNumberError";
        public const string ConnectNumberError = "ConnectNumberError";
        public const string TabletID = "TabletID";
        public const string OrderNumber = "OrderNumber";
        public const string ClientConfig = "ClientConfig";
        public const string Connect = "Connect";
        public const string ServerConfig = "ServerConfig";
        public const string ServerStart = "ServerStart";

        public const string AllClient = "AllClient";

        public const string GroupNumber = "GroupNumber";
        public const string Grouping = "Grouping";

        public const string GroupNumberError = "GroupNumberError";

        public const string ConnectSuccess = "ConnectSuccess";

        public const string UpdateDownload = "UpdateDownload";
        public const string Update = "Update";
        public const string IgnoreUpdate = "IgnoreUpdate";

        //public const string UserName = "UserName";
        //public const string Password = "Password";
        //public const string Login = "Login";
        public const string ServerError = "ServerError";

        public const string Logout = "Logout";
        public const string Synchronize = "Syncronize";
        public const string LessonPlan = "LessonPlan";

        public const string LessonPlanID = "LessonPlanID";
        public const string LessonPlanLabels = "LessonPlanLabels";

        public const string ExactTime = "ExactTime";
        public const string ElapsedTime = "ElapsedTime";

        public const string ID = "ID";

        // StudentBar-on megjelenő szövegek
        public const string StudentName = "StudentName";
        public const string Rank = "Rank";
        public const string Point = "Point";
        public const string Progress = "Progress";
        public const string Mosaic = "Mosaic";
        public const string Game = "Game";
        public const string WaitStudentTabletConnect = "WaitStudentTabletConnect";

        public const string GameID = "GameID";
        public const string Labels = "Labels";
        public const string Screen = "Screen";

        public const string group = "group";

        // Lecketerv betöltése
        public const string SureStartLessonPlan = "SureStartLessonPlan";
        public const string LoadLessonPlan = "LoadLessonPlan";
        public const string ProcessingLessonPlan = "ProcessingLessonPlan";

        // Lecketerv elhagyása
        public const string SureLeaveLessonPlan = "SureLeaveLessonPlan";
        public const string Attention = "Attention";
        public const string ConnectBreakBetweenTeacherAndStudent = "ConnectBreakBetweenTeacherAndStudent";

        //
        public const string CooperativLessonMosaicStartAutoGroup = "CooperativLessonMosaicStartAutoGroup";
        public const string AutoGroup = "AutoGroup";
        public const string CustomGroup = "CustomGroup";

        public const string IdealPeopleNumberInAGroup = "IdealPeopleNumberInAGroup";

        public const string SelectClassToLesson = "SelectClassToLesson";

        public const string second = "second";
        public const string sec = "sec";

        //
        public const string GroupingHappened = "GroupingHappened";
        public const string GroupsOrderingByScreenColor = "GroupOrderingByScreenColor";
        public const string GroupsOrderedLessonMosaicStart = "GroupOrderedLessonMosaicStart";
        public const string TheLessonMosaicStartAfterTimeSeconds = "TheLessonMosaicStartAfter(time)Second";

        public const string StartSinglePlayer = "StartSinglePlayer";
        public const string AutomaticStartTimeSec = "AutomaticStart(time)Sec";

        // InfoPanelPauseLessonPlan szövegei
        public const string LessonPlanIsBreaking = "LessonPlanIsBreaking";
        public const string PressPlayButtonToContinue = "PressPlayButtonToContinue";

        // InfoPanelInformation szövegei
        public const string LessonPlanIsPaused = "LessonPlanIsPaused";

        // Setup menü
        public const string ServerIPaddress = "ServerIPaddress";
        public const string ServerPortNumber = "ServerPortNumber";

        // Bejelentkezés
        public const string ServerResponse = "ServerResponse";

        public const string WrongUserNameOrPassword = "WrongUserNameOrPassword";
        public const string ErrorUnderProcessing = "ErrorUnderProcessing";

        public const string SwitchOfflineMode = "SwitchOfflineMode";

        public const string ConnectToServer = "ConnectToServer";
        public const string ConnectToServerUnsuccessful = "ConnectToServerUnsuccessful";

        public const string ConnectIsBroken = "ConnectIsBroken";


        public const string LessonPlanError = "LessonPlanError";

        public const string SearchGroupmates = "SearchGroupmates";

        // Értékelő képernyők
        public const string WaitForGroupmates = "WaitForGroupmates";

        public const string YouAreFinishedTheLessonPlan = "YouAreFinishedTheLessonPlan";

        // TrueOrFalseGame Igaz, Hamis felírata
        public const string True = "True";
        public const string False = "False";

        // Buborékos játék
        public const string PopOutWrongAnswers = "PopOutWrongAnswers";

        // MathMonsterGame kérdése
        public const string PickTheRightAnswerAndFindItsPlace = "PickTheRightAnswerAndFindItsPlace";

        // PsychoGame szövegei
        public const string Answer = "Answer";
        public const string History = "History";
        public const string SendAndNext = "SendAndNext";

        // ReadGame2 gombokon megjelenő szövegei
        public const string Questions = "Questions";
        public const string Task = "Task";
        public const string Verify = "Verify";
        public const string Next = "Next";
        public const string Again = "Again";

        // CastleGame inventory
        public const string YouGetaNewCharacter = "YouGetaNewCharacter"; // Gratulálunk! Szereztél egy új karaktert.
        public const string InventoryPlayGameConfirmation = "InventoryPlayGameConfirmation"; // Elindítod a játékot 3 zsetonért?

    }


}

/*
public enum NetworkGameEvent  {

    SelectTrue,         // Az igaz hamis játékban az igaz gombot választották
    SelectFalse,        // Az igaz hamis játékban a hamis gombot választották

    KeyPressed,         // A hangman játékban kiválasztottak egy betűt

    ViewStory,          // A read játék a történetet mutatja
    ViewQuestion,       // A read játék a kérdéseket mutatja
    ScrollBarPos,       // A read játékben állítottak a görgetősáv pozícióján
    SelectQuestion,     // A read játékban kiválaszottak egy kérdést
    DeselectQuestion,   // A read játékban visszavonták a kérdés kiválasztását

    SelectAnswer,       // A read játékban kiválaszottak egy választ vagy
                        // A Milionaire játékban kiválasztottak egy választ            



    OutOfTime,          // Az aktuális játékban lejárt az idő

    FlowEnd,            // Vége a flow-nak (be kell fejezni a játékot és kilépni)
                        // Lehet, hogy át lesz nevezve LessonMosaicEnd-re



}

public enum JSONKeys {
    dataContent,
    gameEvent,
    gameEventData,
}

public enum JSONValues {
    IDRequest,              // dataContent - Azonosítót kérünk a szervertől

    clientID,               // dataContent - A szerver azonosítót küld a kliensnek

    getLessonPlan,          // dataContent - Lekérdezzük az óratervet a szervertől

    lessonPlan,             // dataContent - A szerver elküldi az óratervet

    lessonPlanTransferOk,   // dataContent - Az óraterv megérkezett a kliensre


    groupData,  // A dataContent értéke lehet
    playStop,   // gameEvent értéke lehet

}
*/