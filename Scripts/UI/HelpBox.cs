using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpBox : MonoBehaviour {

    private GameObject helpBox;
    private GameObject helpOverlay;
    private GameObject wand;
    private RadialMenuManager radialMenu;
    private DrawScene aScene;
    private Vector3 helpBoxHome;
    private Vector3 helpOverlayHome;
    private Quaternion helpBoxHomeR;
    private Quaternion helpOverlayHomeR;
    public bool helpDisplayed = true;
    private bool wandFound;
    private float smoothFactor = 10.0f;

    void Start () {
        if (GameObject.Find("HelpBox") != null) {
            helpBox = GameObject.Find("HelpBox");
            helpBoxHome = helpBox.transform.position;
            helpBoxHomeR = helpBox.transform.rotation;
        }
        if (GameObject.Find("HelpOverlay") != null) {
            helpOverlay = GameObject.Find("HelpOverlay");
            helpOverlayHome = helpOverlay.transform.position;
            helpOverlayHomeR = helpOverlay.transform.rotation;
        }
    }
	
	void Update () {
        if (!wandFound) {
            if (GameObject.Find("Controller (right)") != null) {
                wand = GameObject.Find("Controller (right)");
                radialMenu = wand.GetComponent<RadialMenuManager>();
                helpOverlay.GetComponent<MeshRenderer>().enabled = true;
                helpBox.GetComponent<MeshRenderer>().enabled = true;
                aScene = wand.GetComponent<DrawScene>();
                wandFound = true;
                radialMenu.Cycle("Destroy Menu");
            } else {
                return;
            }
        }

        if (radialMenu.whatIsSelected == "Help") {

            if (GameObject.Find("ExitBox") != null) {
                GameObject.Find("ExitBox").GetComponent<ExitBox>().Dismiss();
            }
            if (GameObject.Find("DeleteBox") != null) {
                GameObject.Find("DeleteBox").GetComponent<DeleteBox>().Dismiss();
            }

            helpOverlay.GetComponent<MeshRenderer>().enabled = !helpDisplayed;
            helpBox.GetComponent<MeshRenderer>().enabled = !helpDisplayed;
            helpDisplayed = !helpDisplayed;
            radialMenu.whatIsSelected = "";
        }

        if (aScene.onWhichPlanet == "") {
            helpBox.transform.position = helpBoxHome;
            helpBox.transform.rotation = helpBoxHomeR;
            helpOverlay.transform.position = helpOverlayHome;
            helpOverlay.transform.rotation = helpOverlayHomeR;
        } else {
            Quaternion targetRotation = wand.transform.rotation;
            helpBox.transform.position = Vector3.Lerp(helpBox.transform.position, wand.transform.position + (wand.transform.forward * 2.9f), Time.deltaTime * smoothFactor);
            helpBox.transform.rotation = Quaternion.Slerp(helpBox.transform.rotation, targetRotation, Time.deltaTime * smoothFactor);
            helpOverlay.transform.position = Vector3.Lerp(helpOverlay.transform.position, wand.transform.position + (wand.transform.forward * 2.87f), Time.deltaTime * smoothFactor);
            helpOverlay.transform.rotation = Quaternion.Slerp(helpOverlay.transform.rotation, targetRotation, Time.deltaTime * smoothFactor);
        }
    }

    public void Dismiss() {
        helpOverlay.GetComponent<MeshRenderer>().enabled = false;
        helpBox.GetComponent<MeshRenderer>().enabled = false;
        helpDisplayed = false;
    }
}
