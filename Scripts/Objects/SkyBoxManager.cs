using System;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class SkyBoxManager : MonoBehaviour {
    private System.Random rnd;
    private PlanetLayers atmosphereMesh;
    private GameObject starField;
    private Material atmosphereMaterial;
    private bool planetSideSky;
    private PlanetMaterial materialManager;
    private float starsCutoff = 0F;
    private Color32 cloudCutOff = new Color32(0, 0, 0, 0);
    private bool cloudsFetched = false;

    // Use this for initialization
    void Start () {
        planetSideSky = false;
        materialManager = gameObject.AddComponent<PlanetMaterial>();
        //starField = new GameObject("Star Field");
        starField = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        starField.name = "Star Field";
        Material starFieldMaterial = materialManager.AssignMaterial("starfield");
        starField.GetComponent<Renderer>().material = starFieldMaterial;
        // reverse the sphere primitive's triangles, to invet the normals.
        starField.GetComponent<MeshFilter>().mesh.triangles =
            starField.GetComponent<MeshFilter>().mesh.triangles.Reverse().ToArray();
        // transform and disable the collider.
        starField.GetComponent<Collider>().enabled = false;
        starField.GetComponent<Renderer>().enabled = false;
        starsCutoff = starFieldMaterial.GetFloat("_Cutoff");
    }

    public void setSkyOnPlanet (string planetType, int planetSeed, float planetDiameter = 2500F) {
        // Fetch the cloud's color (once), so we can transition the alpha later.
        if (GameObject.Find("aPlanetCloud") != null && !cloudsFetched) {
            cloudCutOff = GameObject.Find("aPlanetCloud").GetComponent<Renderer>().material.GetColor("_TintColor");
            cloudsFetched = true;
        }
        // if we're already planetside, adjust the stars clouds on/off and fadeing during sunset/sunrise.
        if (GameObject.Find("Sun") != null) {
            float sunPos = GameObject.Find("Sun").transform.eulerAngles.x;
            float curStarsCutoff = starsCutoff;
            int curCloudCutoff = cloudCutOff.a;

            float globalFogDistance = 9910;
            // if it's nighttime, nix the global fog and don't cutoff the stars.
            if (sunPos > 270 && sunPos <= 330) {
                curStarsCutoff = starsCutoff;
                globalFogDistance = 10000;
                curCloudCutoff = 2;
            }
            // if it's "daytime" alpha cutoff the stars, enable global fog at the far reaches..
            if (sunPos < 200) {
                curStarsCutoff = 1F;
                globalFogDistance = 9910;
                curCloudCutoff = cloudCutOff.a;
            }
            // if it's "sunrise" or "sunset" fade the alpha cutoff for the starfield.
            if (sunPos > 330 && sunPos < 360) {
                // fade the stars out or in.
                float fadei = (1F - starsCutoff) / 30;
                float fadem = fadei * (sunPos - 330);
                curStarsCutoff = starsCutoff + fadem;
                // fade the clouds out or in.
                float cFadei = (cloudCutOff.a - 2) / 30;
                float cFadem = cFadei * (sunPos - 330);
                curCloudCutoff = 2 + (int)(cFadem);
                // fade the fog off for night, on for day.
                globalFogDistance = 10000 - ((sunPos - 330) * 3);
            }
            starField.GetComponent<Renderer>().material.SetFloat("_Cutoff", curStarsCutoff);
            // You'll need to set the standard asset global fog to a public class to access it. 
            // no fog if there's no atmosphere.
            if (GameObject.Find("aPlanetAtmosphere") == null) {
                globalFogDistance = 10000;
            }
            GameObject.Find("Camera (eye)").GetComponent<GlobalFog>().startDistance = globalFogDistance;
            // Adjust the cloud alpha cutoff.
            Color32 newCloudCutoff = new Color32(cloudCutOff.r, cloudCutOff.g, cloudCutOff.b, (byte)curCloudCutoff);
            GameObject.Find("aPlanetCloud").GetComponent<Renderer>().material.SetColor("_TintColor", newCloudCutoff);
            // Move the starbox so that it always looks right.
            float playerHeight = GameObject.Find("Camera (eye)").transform.position.y;
            starField.transform.position = new Vector3(0, playerHeight - 1350, 3500);
        }
        if (planetSideSky) return;
        if (GameObject.Find("aPlanetAtmosphere") != null) {
            Material planetSkyBox = new Material(Shader.Find("Skybox/Procedural"));
            planetSkyBox.SetColor("_SkyTint", AtmosphereColor(planetType, planetSeed));
            planetSkyBox.SetColor("_GroundColor", AtmosphereColor(planetType, planetSeed));
            planetSkyBox.SetFloat("_SunSize", SunSize(planetType, planetSeed));
            if (GameObject.Find("Camera (eye)") != null) {
                GameObject.Find("Camera (eye)").GetComponent<Skybox>().material = planetSkyBox;
            }
            RenderSettings.sun = GameObject.Find("Sun").GetComponent<Light>();
        }
        // disable the Atmosphere mesh renderer
        GameObject.Find("aPlanetAtmosphere").GetComponent<MeshRenderer>().enabled = false;
        // Enable and transform the starbox.
        starField.transform.localScale = new Vector3(2790, 2790, 2790);
        // rotate the starfield a bit to give it a "random" look.
        starField.transform.localEulerAngles = 
            new Vector3(starFieldRotation(planetSeed), starFieldRotation(planetSeed), starFieldRotation(planetSeed));
        starField.GetComponent<Renderer>().enabled = true;
        planetSideSky = true;
    }

    public void setSkyOffPlanet () {
        if (!planetSideSky) return;
        if (GameObject.Find("aPlanetAtmosphere") != null) {
            Material starSkyBox = Resources.Load("Materials/Skybox/starSkyBox01", typeof(Material)) as Material;
            if (GameObject.Find("Camera (eye)") != null) {
                GameObject.Find("Camera (eye)").GetComponent<Skybox>().material = starSkyBox;
            }
            RenderSettings.sun = null;
        }
        starField.GetComponent<Renderer>().enabled = false;
        GameObject.Find("Camera (eye)").GetComponent<GlobalFog>().startDistance = 20000;
        planetSideSky = false;
        cloudsFetched = false;
    }

    private Color32 AtmosphereColor(string curPlanetType, int seed) {
        rnd = new System.Random(seed);
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
    private float SunSize(string curPlanetType, int seed) {
        rnd = new System.Random(seed);
        if (curPlanetType.Contains("Terra")) {
            // a relativly close sun.
            return (float)(rnd.NextDouble() * (0.15 - 0.04) + 0.04) ;
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

    private float starFieldRotation(int seed) {
        rnd = new System.Random(seed);
        return (float)(rnd.NextDouble() * (130 - 100) + 100);
    }
}
