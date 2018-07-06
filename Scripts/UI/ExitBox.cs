using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitBox : MonoBehaviour {

    private GameObject exitBox;
    private GameObject exitOverlay;
    private GameObject wand;
    private bool wandFound;
    private RadialMenuManager radialMenu;
    private float smoothFactor = 10.0f;

    public bool exitDisplayed = false;

    void Start() {
        if (GameObject.Find("ExitBox") != null) {
            exitBox = GameObject.Find("ExitBox");
        }
        if (GameObject.Find("ExitOverlay") != null) {
            exitOverlay = GameObject.Find("ExitOverlay");
        }
    }

    void Update() {
        if (!wandFound) {
            if (GameObject.Find("Controller (right)") != null) {
                wand = GameObject.Find("Controller (right)");
                radialMenu = wand.GetComponent<RadialMenuManager>();
                wandFound = true;
            }
            else {
                return;
            }
        }

        if (radialMenu.whatIsSelected == "Exit") {

            if (GameObject.Find("DeleteBox") != null) {
                GameObject.Find("DeleteBox").GetComponent<DeleteBox>().Dismiss();
            }
            if (GameObject.Find("HelpBox") != null) {
                GameObject.Find("HelpBox").GetComponent<HelpBox>().Dismiss();
            }

            exitOverlay.GetComponent<MeshRenderer>().enabled = !exitDisplayed;
            exitBox.GetComponent<MeshRenderer>().enabled = !exitDisplayed;
            exitDisplayed = !exitDisplayed;
            radialMenu.whatIsSelected = "";
        }

        Quaternion targetRotation = wand.transform.rotation;

        exitBox.transform.position = Vector3.Lerp(exitBox.transform.position, wand.transform.position + (wand.transform.forward * 1.5f), Time.deltaTime * smoothFactor);
        exitBox.transform.rotation = Quaternion.Slerp(exitBox.transform.rotation, targetRotation, Time.deltaTime * smoothFactor);
        exitOverlay.transform.position = Vector3.Lerp(exitOverlay.transform.position, wand.transform.position + (wand.transform.forward * 1.49f), Time.deltaTime * smoothFactor);
        exitOverlay.transform.rotation = Quaternion.Slerp(exitOverlay.transform.rotation, targetRotation, Time.deltaTime * smoothFactor);
    }

    public void ExitConfirm() {
        if (exitDisplayed) {
            Application.Quit();
            Debug.Log("Hey, editor is running and I like you.. where are you going?!?");
        }
    }

    public void Dismiss() {
        exitOverlay.GetComponent<MeshRenderer>().enabled = false;
        exitBox.GetComponent<MeshRenderer>().enabled = false;
        exitDisplayed = false;
    }
}
