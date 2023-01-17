using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassroomScreens : HHHScreen
{
    static public ClassroomScreens instance;

    Text text; // A felső menü (három csíkos) mellett megjelenő szöveg

    // Képernyők gameObject-jei
    GameObject gameObjectTeacherStudentChoice;
    GameObject gameObjectStudentStart;
    GameObject gameObjectOnlineOrOfflineMenu;
    GameObject gameObjectClassroomGroupList;
    GameObject gameObjectSelectPath;
    GameObject gameObjectSelectPathForDownload;
    GameObject gameObjectTeacherControler;

    ClassroomGroupList classroomGroupList;

    public enum ClassroomScreensEnum
    {
        TeacherStudentChoice,
        StudentStart,
        TeacherOnlineOrOfflineMenu,
        ClassroomGroupList,
        SelectPath,
        SelectPathForDownload,
        TeacherControler,
    }

    public void Awake()
    {
        instance = this;

        text = gameObject.SearchChild("CommonItems").GetComponentInChildren<Text>();

        gameObjectTeacherStudentChoice = gameObject.SearchChild("TeacherStudentChoice").gameObject;
        gameObjectStudentStart = gameObject.SearchChild("StudentStart").gameObject;
        gameObjectOnlineOrOfflineMenu = gameObject.SearchChild("OnlineOrOfflineMenu").gameObject;
        gameObjectSelectPath = gameObject.SearchChild("SelectPath").gameObject;
        gameObjectSelectPathForDownload = gameObject.SearchChild("SelectPathForDownload").gameObject;
        gameObjectTeacherControler = gameObject.SearchChild("TeacherController").gameObject;

        gameObjectClassroomGroupList = gameObject.SearchChild("ClassroomGroupList").gameObject;
        classroomGroupList = gameObjectClassroomGroupList.GetComponent<ClassroomGroupList>();
    }

    public void ChangeScreenWithAnimation(ClassroomScreensEnum screen)
    {
        // Elsötétítjük a képernyőt
        Common.fadeEffect.FadeInColor(Common.screenController.transitionData.fadeColor, Common.screenController.transitionData.effectSpeed, 
            () => {
                // Bekapcsoljuk a kívánt panelt
                ChangeScreen(screen);

                // Megjelenítjük a képernyőt
                Common.fadeEffect.FadeOut(Common.screenController.transitionData.effectSpeed);
            });
    }

    public void ChangeScreen(ClassroomScreensEnum screen)
    {
        // Kikapcsoljuk az összes képernyőt
        gameObjectTeacherStudentChoice.SetActive(false);
        gameObjectStudentStart.SetActive(false);
        gameObjectOnlineOrOfflineMenu.SetActive(false);
        gameObjectClassroomGroupList.SetActive(false);
        gameObjectSelectPath.SetActive(false);
        gameObjectSelectPathForDownload.SetActive(false);
        gameObjectTeacherControler.SetActive(false);

        classroomGroupList.gameObject.SetActive(false);

        // Bekapcsoljuk a kívánt képernyőt
        switch (screen)
        {
            case ClassroomScreensEnum.TeacherStudentChoice:
                text.text = Common.languageController.Translate(C.Texts.ChoiceYouAreTeacherOrStudent);
                gameObjectTeacherStudentChoice.SetActive(true);
                break;
            case ClassroomScreensEnum.TeacherOnlineOrOfflineMenu:
                text.text = Common.languageController.Translate(C.Texts.SelectEducationMode);
                gameObjectOnlineOrOfflineMenu.SetActive(true);
                break;
            case ClassroomScreensEnum.StudentStart:
                text.text = Common.languageController.Translate(C.Texts.ClassroomEducationStart);
                gameObjectStudentStart.SetActive(true);
                break;
            case ClassroomScreensEnum.ClassroomGroupList:
                text.text = Common.languageController.Translate(C.Texts.ClassroomGroupSelect);
                gameObjectClassroomGroupList.SetActive(true);
                classroomGroupList.gameObject.SetActive(true);

                string[] list = new string[] {
                    "Első sor",
                    "Második sor",
                    "Harmadik sor",
                    "Negyedik sor",
                    "Ötödik sor",
                    "Hatodik sor",
                    "Hetedik sor",
                };
                classroomGroupList.Initialize(list, true);
                break;
            case ClassroomScreensEnum.SelectPath:
                text.text = Common.languageController.Translate(C.Texts.DownloadedCurriculum);
                gameObjectSelectPath.SetActive(true);
                classroomGroupList.gameObject.SetActive(true);

                // Betöltjük a letöltött útvonalak neveit és megjelenítjük

                string[] list2 = new string[] {
                    "Letöltött útvonal 01",
                    "Letöltött útvonal 02",
                    "Letöltött útvonal 03",
                    "Letöltött útvonal 04",
                    "Letöltött útvonal 05",
                    "Letöltött útvonal 06",
                    "Letöltött útvonal 07",
                    "Letöltött útvonal 08",
                    "Letöltött útvonal 09",
                };
                classroomGroupList.Initialize(list2, true);
                break;
            case ClassroomScreensEnum.SelectPathForDownload:

                // Letöltjük a szerverről a rendelkezésre álló útvonalakat

                text.text = Common.languageController.Translate(C.Texts.DownloadCurriculum);
                gameObjectSelectPathForDownload.SetActive(true);
                classroomGroupList.gameObject.SetActive(true);

                // Betöltjük a letöltött útvonalak neveit és megjelenítjük

                string[] list3 = new string[] {
                    "Rendelkezésre álló útvonal 01",
                    "Rendelkezésre álló útvonal 02",
                    "Rendelkezésre álló útvonal 03",
                    "Rendelkezésre álló útvonal 04",
                    "Rendelkezésre álló útvonal 05",
                    "Rendelkezésre álló útvonal 06",
                    "Rendelkezésre álló útvonal 07",
                    "Rendelkezésre álló útvonal 08",
                    "Rendelkezésre álló útvonal 09",
                    "Rendelkezésre álló útvonal 10",
                };
                classroomGroupList.Initialize(list3, true);
                break;
            case ClassroomScreensEnum.TeacherControler:
                //gameObjectTeacherControler.SetActive(true);
                break;
        }
    }

    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Menu":
                SideMenu.instance.Show(gameObject);
                break;
        }

        Debug.Log(buttonName);
    }

    public void ButtonClick_TeacherOrStudent(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;
            case "Teacher":
                ChangeScreenWithAnimation(ClassroomScreensEnum.TeacherOnlineOrOfflineMenu);
                break;
            case "Student":
                ChangeScreenWithAnimation(ClassroomScreensEnum.StudentStart);
                break;
        }

        Debug.Log(buttonName);
    }

    public void ButtonClick_StudentStart(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                ChangeScreenWithAnimation(ClassroomScreensEnum.TeacherStudentChoice);
                break;
            case "Start":
                // Megpróbálunk a kliensel csatlakozni a szerverre

                break;
        }

        Debug.Log(buttonName);
    }


    public void ButtonClick_OnlineOrOffline(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                ChangeScreenWithAnimation(ClassroomScreensEnum.TeacherStudentChoice);
                break;
            case "OnlineStart":
                ChangeScreenWithAnimation(ClassroomScreensEnum.SelectPath);
                break;
            case "PathDownload":
                ChangeScreenWithAnimation(ClassroomScreensEnum.SelectPathForDownload);
                break;
            case "ClassroomGroupsDownload":
                // Letöltjük a osztálytermi csoportokat
                break;
            case "OfflineStart":
                ChangeScreenWithAnimation(ClassroomScreensEnum.SelectPath);
                break;
            case "OfflineDatasUpload":
                // Feltöljük a korábbi játék adatait
                break;
        }

        Debug.Log(buttonName);
    }

    // Az útvonal kiválasztó panelen található gombok eseménykezelője
    public void ButtonClick_SelectPath(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                ChangeScreenWithAnimation(ClassroomScreensEnum.TeacherOnlineOrOfflineMenu);
                break;
            case "Preview":
                ChangeScreenWithAnimation(ClassroomScreensEnum.TeacherControler);
                break;
            case "Play":
                ChangeScreenWithAnimation(ClassroomScreensEnum.TeacherControler);
                break;

        }

        Debug.Log(buttonName);
    }

    // Az útvonal letöltő panelen található gombok eseménykezelője
    public void ButtonClick_SelectPathForDownload(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                ChangeScreenWithAnimation(ClassroomScreensEnum.TeacherOnlineOrOfflineMenu);
                break;
            case "Download":
                // Letöltjük a kiválasztott útvonalakat
                break;
        }

        Debug.Log(buttonName);
    }




}
