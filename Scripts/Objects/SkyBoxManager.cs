using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBoxManager : MonoBehaviour {
    //private Camera eyeCamera;
    private System.Random rnd;
    private PlanetMesh atmosphereMesh;
    private bool planetSideSky;

	// Use this for initialization
	void Start () {
        //eyeCamera = GameObject.Find("Camera (eye)").GetComponent<Camera>();
        planetSideSky = false;
    }

    public void setSkyOnPlanet (string planetType, int planetSeed) {
        if (planetSideSky) return;
        //rnd = new System.Random(planetSeed);
        //int R = rnd.Next(50, 75); int B = rnd.Next(100, 150);
        //int G = rnd.Next(0, 25); int A = rnd.Next(5, 100);
        //eyeCamera.clearFlags = CameraClearFlags.SolidColor;
        //eyeCamera.backgroundColor = new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        if (atmosphereMesh == null) {
            atmosphereMesh = GameObject.Find("aPlanetAtmosphere").GetComponent<PlanetMesh>();
        }
        if (atmosphereMesh != null) atmosphereMesh.invertTriangles();
        planetSideSky = true;
    }

    public void setSkyBox () {
        if (!planetSideSky) return;
        //if (eyeCamera != null) {
        //    eyeCamera.clearFlags = CameraClearFlags.Skybox;
        //}
        if (atmosphereMesh == null) {
            atmosphereMesh = GameObject.Find("aPlanetAtmosphere").GetComponent<PlanetMesh>();
        }
        if (atmosphereMesh != null) atmosphereMesh.invertTriangles();
        planetSideSky = false;
    }
}
