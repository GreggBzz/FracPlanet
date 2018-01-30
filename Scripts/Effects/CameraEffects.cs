using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraEffects : MonoBehaviour {
    private int count = 0;
    private GameObject aCamera;
    private float oceanHeight = -10000F; // ocean way down to prevent y < ocean false blur. 
    private Shader blurFastShader;
    private DrawScene aScene;
    private int debugcount = 0;
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
//            debugcount += 1;
            oceanHeight = gameObject.GetComponent<PlanetManager>().GetOceanDiameter();
            float yPos = GameObject.Find("[CameraRig]").transform.position.y;
            if (yPos < (oceanHeight / 2 + 750F)) {
                aCamera.GetComponent<BlurOptimized>().enabled = false;
            }
            else { 
                aCamera.GetComponent<BlurOptimized>().enabled = false;
            }
//            if (debugcount == 100) {
//                Debug.Log("Blur Enable: " + aCamera.GetComponent<BlurOptimized>().enabled);
//                Debug.Log("Ocean Height: " + (oceanHeight / 2 + 750) + ", Camera Height " + yPos);
//                debugcount = 0;
//            }
        }
        else 
            aCamera.GetComponent<BlurOptimized>().enabled = false;
    }
}
