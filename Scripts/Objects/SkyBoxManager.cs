using System;
using System.Linq;
using UnityEngine;

public class SkyBoxManager : MonoBehaviour {
    private System.Random rnd;
    private PlanetLayers atmosphereMesh;
    private GameObject starField;
    private Material atmosphereMaterial;
    private bool planetSideSky;
    private PlanetMaterial materialManager;
    private float starsCutoff = 0F;

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
        // if we're already planetside, adjust the stars on/off and fadeing during sunset/sunrise.
        if (GameObject.Find("Sun") != null) {
            float sunPos = GameObject.Find("Sun").transform.eulerAngles.x;
            float curStarsCutoff = starsCutoff;
            if (sunPos > 270 && sunPos <= 330) {
                curStarsCutoff = starsCutoff;
            }
            if (sunPos < 200) {
                curStarsCutoff = 1F;
            }
            // if it's "sunrise" or "sunsed" fade the alpha cutoff for the starfield.
            if (sunPos > 330 && sunPos < 360) {
                float fadei = (1F - starsCutoff) / 30;
                float fadem = fadei * (sunPos - 330);
                curStarsCutoff = starsCutoff + fadem;
            }
            starField.GetComponent<Renderer>().material.SetFloat("_Cutoff", curStarsCutoff);
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
        float starFieldDiameter = planetDiameter + 100F;
        starField.transform.localScale = new Vector3(starFieldDiameter, starFieldDiameter, starFieldDiameter);
        starField.transform.position = new Vector3(0, 750, 3500);
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
        planetSideSky = false;
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
            // more Titan/yellow kind of clouds.
            int R = rnd.Next(150, 255); int B = rnd.Next(0, 150);
            int G = rnd.Next(150, 255); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        if (curPlanetType.Contains("Molten")) {
            // more Venus/Hellplanet kind of clouds.
            int R = rnd.Next(200, 255); int B = rnd.Next(200, 255);
            int G = rnd.Next(200, 255); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        return new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    }
    private float SunSize(string curPlanetType, int seed) {
        rnd = new System.Random(seed);
        if (curPlanetType.Contains("Terra")) {
            // a relativly close sun.
            // 0.04 to 0.15
            return (float)(rnd.NextDouble() * (0.15 - 0.04) + 0.04) ;
        }
        if (curPlanetType.Contains("Icy")) {
            // a distant sun.
            // 0.01 to 0.04
            return (float)(rnd.NextDouble() * (0.04 - 0.01) + 0.01);
        }
        if (curPlanetType.Contains("Molten")) {
            // a big glowing sun!
            // .08 to .28
            return (float)(rnd.NextDouble() * (0.28 - 0.08) + 0.08);
        }
        return (float)rnd.NextDouble();
    }

    private float starFieldRotation(int seed) {
        rnd = new System.Random(seed);
        return (float)(rnd.NextDouble() * (130 - 100) + 100);
    }
}
