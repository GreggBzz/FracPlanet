using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogManager : MonoBehaviour {
    private int seed;
    private System.Random rnd;

    public const float drawDistance = 400;
    private const int fogCount = 10;
    private int R = 0, G = 0, B = 0, A = 0;
    private Color32 baseColor;
    private Material fogMaterial;

    private int wholePlanetVertCount;
    private float planetRadius;
    private float planetMaxElevation;
    private float planetMinElevation;
    private Vector2[] fogElevations;

    private bool globalFogLocations = false;
    private bool fogPlaced;
    private GameObject planetFogs;
    private GameObject[] fog = new GameObject[fogCount];

    public struct FogAtVert {
        public Vector3 centerLocation;
        public bool haveFog;
        public bool display;
    }

    public FogAtVert[] fogAtVert;

    // Use this for initialization
    void Awake () {
        if (GameObject.Find("Controller (right)") != null) {
            seed = GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetSeed;
        }
        else {
            seed = 100;
        }
        rnd = new System.Random(seed);

        // setup the base color.
        R = rnd.Next(70, 150); B = rnd.Next(70, 150);
        G = rnd.Next(70, 150); A = 6;
        baseColor = new Color32((byte)R, (byte)G, (byte)B, (byte)A);

        fogMaterial = Resources.Load("PlanetTextures/TerraPlanet/Fog/TerraPlanetFog1", typeof(Material)) as Material;
        fogMaterial.SetColor("_TintColor", baseColor);

        wholePlanetVertCount = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().newVertIndex;
        fogAtVert = new FogAtVert[wholePlanetVertCount];
        fogElevations = GameObject.Find("aPlanet").GetComponent<MeshFilter>().mesh.uv4;
        planetRadius = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().diameter / 2F;
        planetMaxElevation = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().maxHeight;
        planetMinElevation = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().minHeight;
        for (int i = 0; i <= fogCount - 1; i++) {
            planetFogs = GameObject.Find("FogEffects");
            fog[i] = planetFogs.transform.GetChild(i).gameObject;
        }
        DisableFog();
	}

    public void PositionFog(string curPlanetType = "Terra") {
        if (globalFogLocations) { return; }
        float waterLine = (planetRadius - planetMinElevation) / (planetMaxElevation - planetMinElevation);
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            // no fog underwater.
            if (fogElevations[i].y <= waterLine) {
                continue;
            }
            // foggy lower plains.
            if ((fogElevations[i].y > waterLine) && (fogElevations[i].y <= waterLine + .35F)) {
                if (UnityEngine.Random.Range(0F, 1F) >= .993F) {
                    fogAtVert[i].haveFog = true;
                }
            }
            // foggy mountains.
            if ((fogElevations[i].y > .8F)) {
                if (UnityEngine.Random.Range(0F, 1F) >= .993F) {
                    fogAtVert[i].haveFog = true;
                }
            }
        }
        globalFogLocations = true;
    }

    public void DisableFog() {
        fogPlaced = false;
        for (int i = 0; i <= fogCount - 1; i++) {
            fog[i].SetActive(false);
        }
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            fogAtVert[i].display = false;
        }
    }

    public void PlaceAndEnableFog() {
        if (fogPlaced) { return; }
        int fogDisplayed = 0;
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (!fogAtVert[i].display) { continue; }
            fog[fogDisplayed].SetActive(true);
            fog[fogDisplayed].transform.localPosition = fogAtVert[i].centerLocation;
            fogDisplayed += 1;
            if (fogDisplayed >= fogCount) { fogPlaced = true; return; }
        }
        fogPlaced = true;
    }

    void Update () {
        // darken the fog and make it less transparent at night.
        float multiplier = GameObject.Find("Sun").GetComponent<Sun>().distance;
        baseColor = new Color32((byte)(R * multiplier), (byte)(G * multiplier) , 
                                (byte)(B * multiplier), (byte)(A + ((1 - multiplier) * 70)));
        fogMaterial.SetColor("_TintColor", baseColor);
    }
}
