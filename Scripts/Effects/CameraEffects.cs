using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraEffects : MonoBehaviour {
    private GameObject aCamera;
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
        int layerMask = LayerMask.GetMask("Water");
        // do something to the camera.
        if (aScene.onWhichPlanet != "") {
            debugcount += 1;
            Vector3 cameraPos = GameObject.Find("[CameraRig]").transform.position;
            if (Physics.Raycast(cameraPos + Vector3.up * 300F, Vector3.down, 300F, layerMask)) { 
                //aCamera.GetComponent<BlurOptimized>().enabled = false;
            }
            else {
                //aCamera.GetComponent<BlurOptimized>().enabled = false;
            }
            //if (debugcount == 100) {
            //    Debug.Log("Underwater: " + underWater);
            //    debugcount = 0;
            //}
        }
        else 
            aCamera.GetComponent<BlurOptimized>().enabled = false;
    }
}
