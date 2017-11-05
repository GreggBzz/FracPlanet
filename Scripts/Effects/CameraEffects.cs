using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraEffects : MonoBehaviour {
    private int count = 0;
    private GameObject aCamera;
    private float oceanHeight = 0F;
    private Shader blurFastShader;
    private DrawScene aScene; 
    // Use this for initialization
    void Start() {
        aCamera = GameObject.Find("Camera (eye)");
        aCamera.gameObject.GetComponent<BlurOptimized>().enabled = false;
        aScene = gameObject.GetComponent<DrawScene>();
    }

    // Update is called once per frame
    void Update () {
        RenderEffects();
	}

    private void RenderEffects() {
        // do something to the camera.
        if (aScene.onWhichPlanet != "") {
            if (oceanHeight == 0F) {
                oceanHeight = gameObject.GetComponent<PlanetManager>().GetOceanDiameter();
            }
            float yPos = GameObject.Find("[CameraRig]").transform.position.y;
            if (yPos < (oceanHeight / 2 + 750F)) {
                aCamera.GetComponent<BlurOptimized>().enabled = true;
            }
            else { 
                aCamera.GetComponent<BlurOptimized>().enabled = false;
            }
        }
        else 
            aCamera.GetComponent<BlurOptimized>().enabled = false;
            oceanHeight = 0F;
    }
}
