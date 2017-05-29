using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBoxManager : MonoBehaviour {
    private System.Random rnd;
    private PlanetMesh atmosphereMesh;
    private Material atmosphereMaterial;
    private bool planetSideSky;
    private PlanetMaterial materialManager;

    // Use this for initialization
    void Start () {
        planetSideSky = false;
        materialManager = gameObject.AddComponent<PlanetMaterial>();
    }

    public void setSkyOnPlanet (string planetType, int planetSeed) {
        if (planetSideSky) return;
        if (GameObject.Find("aPlanetAtmosphere") != null) {
            Texture curTypeTexture = Resources.Load("PlanetTextures/" + planetType + "/txrAtmosphere" + planetType) as Texture;
            Texture curTypeNormalMap = Resources.Load("PlanetTextures/" + planetType + "/nmAtmosphere" + planetType) as Texture;
            Material atmosphereMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
            atmosphereMaterial.SetTexture("_MainTex", curTypeTexture);
            atmosphereMaterial.SetTexture("_BumpMap", curTypeNormalMap);
            atmosphereMaterial.SetColor("_TintColor", AtmosphereColor(planetType, planetSeed));
            atmosphereMaterial.renderQueue = 2800;
            GameObject.Find("aPlanetAtmosphere").GetComponent<Renderer>().material = atmosphereMaterial;
        }
        planetSideSky = true;
    }

    public void setSkyOffPlanet (string planetType, int planetSeed) {
        if (!planetSideSky) return;
        if (GameObject.Find("aPlanetAtmosphere") != null) {
            Material atmosphereMaterial = materialManager.AssignMaterial("atmosphere", planetType, planetSeed);
            GameObject.Find("aPlanetAtmosphere").GetComponent<Renderer>().material = atmosphereMaterial;
            GameObject.Find("aPlanetAtmosphere").GetComponent<Renderer>().material.renderQueue = 2000;
        }
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
}
