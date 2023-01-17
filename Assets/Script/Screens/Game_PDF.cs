using Paroxe.PdfRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_PDF : GameAncestor {

    PDFViewer pdfViewer;

    public TaskPDF taskData;

    // Use this for initialization
    override public void Awake()
    {
        base.Awake();

        pdfViewer = gameObject.SearchChild("PDFViewer").GetComponent<PDFViewer>();
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        // Lekérdezzük a feladat adatait
        taskData = (TaskPDF)Common.taskController.task;

        // Beállítjuk az inaktivitás mérőt
        inactiveTimeLimit = inactiveTimeLimit; // Marad a default // 300; // 5 perc

        // Becsukjuk a pdfViewer leftPanelját
        //pdfViewer.m_Internal.m_LeftPanel.SetOpened(false);

        yield return null;
    }

    public override IEnumerator ScreenShowStartCoroutine()
    {
        StartCoroutine(base.ScreenShowStartCoroutine());

        
        //pdfViewer.LinksActionHandler = new PDFViewerActionHandler(
        //    (string s) => {
        //        Debug.Log(s);
        //        // Megnyítjuk a webböngészőt a megadott linkkel
        //    }
        //    );
            
            //Paroxe.PdfRenderer.Internal.Viewer.PDFViewerDefaultActionHandler();
        pdfViewer.LoadDocumentFromBuffer(taskData.pdfData, "");
        //System.IO.File.WriteAllBytes("D:\\1.pdf", taskData.pdfData);

        //pdfViewer.LoadDocumentFromWeb("http://test.classyedu.com/WebGL/testFullOTP/1.pdf", "");
        //Common.pdfController.Show(taskData.pdfData);

        yield return null;
    }

    // A menüből kiválasztották a kilépést a játékból
    /*
    IEnumerator ExitCoroutine()
    {
        Common.taskController.GameExit();
        yield return null;
    }
    */

    // Ha rákattintottak egy gombra, akkor meghívódik ez az eljárás a gombon levő Button szkript által
    /*
    override protected void ButtonClick(Button button)
    {
        if (userInputIsEnabled)
        {
            switch (button.buttonType)
            {
                case Button.ButtonType.Exit: // Ha megnyomták a kilépés gombot
                    StartCoroutine(ExitCoroutine());
                    break;

                case Button.ButtonType.SwitchLayout: // Megnyomták a layout váltó gombot
                    //layoutManager.ChangeLayout();
                    //SetPictures();
                    break;
            }
        }
    }
    */
}
