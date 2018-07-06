using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteBox : MonoBehaviour {

    private GameObject deleteBox;
    private GameObject deleteOverlay;
    private GameObject wand;
    private DrawScene aScene;
    private bool wandFound;
    private RadialMenuManager radialMenu;
    private float smoothFactor = 10.0f;
    public bool deleteDisplayed = false;

    void Start() {
        if (GameObject.Find("DeleteBox") != null) {
            deleteBox = GameObject.Find("DeleteBox");
        }
        if (GameObject.Find("DeleteOverlay") != null) {
            deleteOverlay = GameObject.Find("DeleteOverlay");
        }
    }

    void Update() {
        if (!wandFound) {
            if (GameObject.Find("Controller (right)") != null) {
                wand = GameObject.Find("Controller (right)");
                radialMenu = wand.GetComponent<RadialMenuManager>();
                aScene = wand.GetComponent<DrawScene>();
                wandFound = true;
            }
            else {
                return;
            }
        }

        if (radialMenu.whatIsSelected == "Delete") {
            if (!aScene.havePlanet) {
                Dismiss();
                radialMenu.whatIsSelected = "";
                return;
            }

            if (GameObject.Find("ExitBox") != null) {
                GameObject.Find("ExitBox").GetComponent<ExitBox>().Dismiss();
            }
            if (GameObject.Find("HelpBox") != null) {
                GameObject.Find("HelpBox").GetComponent<HelpBox>().Dismiss();
            }

            deleteOverlay.GetComponent<MeshRenderer>().enabled = !deleteDisplayed;
            deleteBox.GetComponent<MeshRenderer>().enabled = !deleteDisplayed;
            deleteDisplayed = !deleteDisplayed;
            radialMenu.whatIsSelected = "";
        }

        Quaternion targetRotation = wand.transform.rotation;

        deleteBox.transform.position = Vector3.Lerp(deleteBox.transform.position, wand.transform.position + (wand.transform.forward * 1.5f), Time.deltaTime * smoothFactor);
        deleteBox.transform.rotation = Quaternion.Slerp(deleteBox.transform.rotation, targetRotation, Time.deltaTime * smoothFactor);
        deleteOverlay.transform.position = Vector3.Lerp(deleteOverlay.transform.position, wand.transform.position + (wand.transform.forward * 1.49f), Time.deltaTime * smoothFactor);
        deleteOverlay.transform.rotation = Quaternion.Slerp(deleteOverlay.transform.rotation, targetRotation, Time.deltaTime * smoothFactor);
    }

    public void DeleteConfirm() {
        if (!aScene.havePlanet) { return; }
        if (deleteDisplayed) {
            aScene.Teleport(true);
            aScene.DestroyPlanets();
            aScene.DestroyPlanetOutline();
            Dismiss();
        }
    }

    public void Dismiss() {
        deleteOverlay.GetComponent<MeshRenderer>().enabled = false;
        deleteBox.GetComponent<MeshRenderer>().enabled = false;
        deleteDisplayed = false;
    }
}
