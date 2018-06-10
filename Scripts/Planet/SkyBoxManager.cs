using System;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class SkyBoxManager : MonoBehaviour {
    private System.Random rnd;
    private int seed;
    private PlanetLayers atmosphereMesh;

    private GameObject starField;
    private GameObject theSun;
    private GameObject theClouds;
    private GameObject theAtmosphere;
    private GameObject eyeCamera;
    private bool objectsSet;

    private Material atmosphereMaterial;
    private bool planetSideSky;
    private PlanetMaterial materialManager;
    private Color32 cloudColor = new Color32(0, 0, 0, 0);
    private bool cloudsFetched = false;

    // Use this for initialization
    void Start() {
        planetSideSky = false;
        materialManager = gameObject.AddComponent<PlanetMaterial>();
        starField = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        starField.name = "Star Field";
        Material starFieldMaterial = materialManager.AssignMaterial("starfield");
        starField.GetComponent<Renderer>().material = starFieldMaterial;
        // reverse the sphere primitive's triangles, to invet the normals.
        starField.GetComponent<MeshFilter>().mesh.triangles =
            starField.GetComponent<MeshFilter>().mesh.triangles.Reverse().ToArray();
        // transform and disable the collider.
        starField.GetComponent<Collider>().enabled = false;
        Destroy(starField.GetComponent<Collider>());
        starField.GetComponent<Renderer>().enabled = false;
        if (GameObject.Find("Controller (right)") != null) {
            seed = GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetSeed;
        }
        else {
            seed = 100;
        }
        rnd = new System.Random(seed);
    }

    public void setObjects() {
        if (GameObject.Find("Sun") != null) {
            theSun = GameObject.Find("Sun");
        }
        if (GameObject.Find("aPlanetCloud") != null) {
            theClouds = GameObject.Find("aPlanetCloud");
        }
        if (GameObject.Find("aPlanetAtmosphere") != null) {
            theAtmosphere = GameObject.Find("aPlanetAtmosphere");
        }
        if (GameObject.Find("Camera (eye)") != null) {
            eyeCamera = GameObject.Find("Camera (eye)");
        }
        objectsSet = true;
    }

    public void setSkyOnPlanet(string planetType, int planetSeed, float planetDiameter = 2500F) {
        if (!objectsSet) { setObjects(); }
        
        // Fetch the cloud's color (once), so we can transition it to darker.
        float multiplier = GameObject.Find("Sun").GetComponent<Sun>().distance;
        if (theClouds != null && !cloudsFetched) {
            cloudColor = theClouds.GetComponent<Renderer>().material.GetColor("_TintColor");
            cloudsFetched = true;
        }
        // if theres a sun, based on it's distance multipler: 
        // dim the sun a bit, cutoff the stars more sharply, 
        // darken the clouds, adjust the global fog cutoff.
        if (theSun != null) {
            float globalFogDistance = 9900;

            theSun.GetComponent<Light>().intensity = multiplier;
            starField.GetComponent<Renderer>().material.SetFloat("_Cutoff", multiplier);

            if (theClouds != null) {
                Color32 curColor = new Color32((byte)(cloudColor.r * multiplier),
                                               (byte)(cloudColor.g * multiplier),
                                               (byte)(cloudColor.b * multiplier),
                                               cloudColor.a);
                theClouds.GetComponent<Renderer>().material.SetColor("_TintColor", curColor);
            }
            if (theAtmosphere == null) {
                globalFogDistance = 10000;
            }
            else {
                globalFogDistance = 10000 - (100 * multiplier);
            }

            eyeCamera.GetComponent<GlobalFog>().startDistance = globalFogDistance;
            // Move the starbox so that it always looks right.
            float playerHeight = eyeCamera.transform.position.y;
            starField.transform.position = new Vector3(0, playerHeight - 1350, 3500);
        }
        if (planetSideSky) return;
        if (theAtmosphere != null) {
            Material planetSkyBox = new Material(Shader.Find("Skybox/Procedural"));
            planetSkyBox.SetColor("_SkyTint", AtmosphereColor(planetType));
            planetSkyBox.SetColor("_GroundColor", AtmosphereColor(planetType));
            planetSkyBox.SetFloat("_SunSize", SunSize(planetType));
            if (GameObject.Find("Camera (eye)") != null) {
                GameObject.Find("Camera (eye)").GetComponent<Skybox>().material = planetSkyBox;
            }
            RenderSettings.sun = theSun.GetComponent<Light>();
        }
        // disable the Atmosphere mesh renderer
        if (theAtmosphere != null) {
            theAtmosphere.GetComponent<MeshRenderer>().enabled = false;
        }
        // Enable and transform the starbox.
        starField.transform.localScale = new Vector3(2790, 2790, 2790);
        // rotate the starfield a bit to give it a "random" look.
        starField.transform.localEulerAngles =
            new Vector3(starFieldRotation(), starFieldRotation(), starFieldRotation());
        starField.GetComponent<Renderer>().enabled = true;
        planetSideSky = true;
    }

    public void setSkyOffPlanet() {
        if (!planetSideSky) return;
        if (theAtmosphere != null) {
            Material starSkyBox = Resources.Load("Materials/Skybox/starSkyBox01", typeof(Material)) as Material;
            if (eyeCamera != null) {
                eyeCamera.GetComponent<Skybox>().material = starSkyBox;
            }
            RenderSettings.sun = null;
        }
        starField.GetComponent<Renderer>().enabled = false;
        eyeCamera.GetComponent<GlobalFog>().startDistance = 20000;
        planetSideSky = false;
        cloudsFetched = false;
        objectsSet = false;
        theAtmosphere = null; theSun = null; theClouds = null; eyeCamera = null;
    }

    private Color32 AtmosphereColor(string curPlanetType) {
        if (curPlanetType.Contains("Terra")) {
            // random earth atmosphere.
            int R = rnd.Next(0, 20); int B = rnd.Next(40, 150);
            int G = rnd.Next(40, 150); int A = rnd.Next(220, 255);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        if (curPlanetType.Contains("Icy")) {
            // more Titan/yellow kind atmosphere
            int R = rnd.Next(130, 255); int B = rnd.Next(0, 150);
            int G = rnd.Next(130, 255); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        if (curPlanetType.Contains("Molten")) {
            // more Venus/Hellplanet kind of atmosphere
            int R = rnd.Next(130, 255); int B = rnd.Next(130, 255);
            int G = rnd.Next(130, 255); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        return new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    }
    private float SunSize(string curPlanetType) {
        if (curPlanetType.Contains("Terra")) {
            // a relativly close sun.
            return (float)(rnd.NextDouble() * (0.15 - 0.04) + 0.04);
        }
        if (curPlanetType.Contains("Icy")) {
            // a distant sun.
            return (float)(rnd.NextDouble() * (0.04 - 0.01) + 0.01);
        }
        if (curPlanetType.Contains("Molten")) {
            // a big glowing sun!
            return (float)(rnd.NextDouble() * (0.28 - 0.08) + 0.08);
        }
        return (float)rnd.NextDouble();
    }

    private float starFieldRotation() {
        return (float)(rnd.NextDouble() * (130 - 100) + 100);
    }
}
