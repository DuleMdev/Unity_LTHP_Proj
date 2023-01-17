using Paroxe.PdfRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PDFController : HHHScreen {

    PDFViewer pdfViewer;

	// Use this for initialization
	void Awake () {
        Common.pdfController = this;

        pdfViewer = gameObject.SearchChild("PDFViewer").GetComponent<PDFViewer>();

        transform.position = Vector3.zero;

        Hide();
    }

 
    public void Show(byte[] pdfBytes) {
        pdfViewer.LoadDocumentFromBuffer(pdfBytes, "");
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
