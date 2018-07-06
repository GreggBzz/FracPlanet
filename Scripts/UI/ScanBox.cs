using UnityEngine;

public class ScanBox : MonoBehaviour {
    public GameObject scanBox;

	private void Awake () {
        Font ScanBoxFont = Resources.Load("UI/Fonts/BroshK") as Font;
        scanBox = new GameObject("Scan Box");
        scanBox.AddComponent<TextMesh>();
        scanBox.GetComponent<TextMesh>().text = "";
        scanBox.GetComponent<TextMesh>().font = ScanBoxFont;
        scanBox.GetComponent<Renderer>().material = ScanBoxFont.material;
        scanBox.GetComponent<TextMesh>().fontSize = 110;
        scanBox.GetComponent<TextMesh>().characterSize = 2.3F;
        scanBox.transform.position = new Vector3(-730, 450, 400);
        scanBox.transform.eulerAngles = new Vector3(0, -25, 0);
        scanBox.GetComponent<Renderer>().enabled = false;
    }

    public void DisplayScanBox(string newText) {
        if (GameObject.Find("ScanBoxBackGround")) {
            GameObject.Find("ScanBoxBackGround").GetComponent<Renderer>().enabled = true;
        }
        scanBox.GetComponent<Renderer>().enabled = true;
        scanBox.GetComponent<TextMesh>().text = newText;
    }

    public void HideScanBox() {
        scanBox.GetComponent<Renderer>().enabled = false;
        scanBox.GetComponent<TextMesh>().text = "";
        if (GameObject.Find("ScanBoxBackGround")) {
            GameObject.Find("ScanBoxBackGround").GetComponent<Renderer>().enabled = false;
        }
    }
}
