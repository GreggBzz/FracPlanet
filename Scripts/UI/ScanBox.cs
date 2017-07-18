using UnityEngine;

public class ScanBox : MonoBehaviour {
    public GameObject scanBox;

	private void Awake () {
        scanBox = new GameObject("Scan Box");
        scanBox.AddComponent<TextMesh>();
        scanBox.GetComponent<TextMesh>().text = "";
        scanBox.GetComponent<TextMesh>().fontSize = 80;
        scanBox.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;
        scanBox.GetComponent<TextMesh>().characterSize = 2.3F;
        scanBox.transform.position = new Vector3(-700, 250, 400);
        scanBox.transform.eulerAngles = new Vector3(0, -25, 0);
        scanBox.GetComponent<Renderer>().enabled = false;
    }

    public void DisplayScanBox(string newText) {
        scanBox.GetComponent<Renderer>().enabled = true;
        scanBox.GetComponent<TextMesh>().text = newText;
    }

    public void HideScanBox() {
        scanBox.GetComponent<Renderer>().enabled = false;
        scanBox.GetComponent<TextMesh>().text = "";
    }
}
