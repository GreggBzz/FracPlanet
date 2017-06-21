using UnityEngine;

public class PlanetManager : MonoBehaviour {
    // Meshes.
    private PlanetLayers terrainMesh;
    private PlanetLayers oceanMesh;
    private PlanetLayers atmosphereMesh;
    private PlanetLayers cloudMesh;
    private PlanetOceanDetail partialOceanMesh;
    // Gameobjects and boolean base attributes.
    private GameObject terrain;
    private GameObject ocean;
    private GameObject partialOcean;
    public GameObject atmosphere;
    public GameObject cloud;
    public bool hasOcean;
    public bool hasAtmosphere;
    public bool hasClouds;
    // lights
    private Sun aSun;
    private Moon aMoon;
    // sounds
    private PlanetSounds planetSound;
    // dimensions, seed, type.
    private const float distFromCenter = 3500F;
    public float planetDiameter = 2500F;
    public string curPlanetType = "";
    public int curPlanetSeed = 100;
    private Vector3 centerPos = new Vector3(0, 750, 0);

    public GameObject planetOutline; // public for user(wand) manipulated transforms.
    private PlanetMaterial materialManager; // Manage the planet layer materials with a class.
    public PlanetMetaData planetMetaData; // The metadata for the planet, compounds, mass, weather etc.

    private System.Random rnd; // chance for current planet atmosphere, clouds?

    public void Start() {
        materialManager = gameObject.AddComponent<PlanetMaterial>();
        planetMetaData = gameObject.AddComponent<PlanetMetaData>();
        planetSound = gameObject.AddComponent<PlanetSounds>();
        Material planetOutlineMaterial = materialManager.AssignMaterial("outline");
        planetOutline = new GameObject("Planet Outline");
        planetOutline = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        planetOutline.GetComponent<Renderer>().material = planetOutlineMaterial;
        planetOutline.transform.localScale = new Vector3(planetDiameter, planetDiameter, planetDiameter);
        planetOutline.GetComponent<Renderer>().enabled = false;
        // all the lights.
        aSun = GameObject.Find("Sun").GetComponent<Sun>();
        aMoon = GameObject.Find("Moon").GetComponent<Moon>();
    }

    public void DestroyPlanet() {
        if (terrainMesh != null) {
            Destroy(terrain); terrainMesh = null;
        }
        if (oceanMesh != null) {
            Destroy(ocean); oceanMesh = null;
        }
        if (atmosphereMesh != null) {
            Destroy(atmosphere); atmosphereMesh = null;
        }
        if (cloud != null) { 
            Destroy(cloud); cloud = null;
        }
        if (partialOcean != null) {
            Destroy(partialOcean); partialOcean = null;
        }
        planetSound.DisableSounds();
    }

    public void PausePlanet(string planetName) {
        // "pause" a planet we've teleported to.
        // "unpause" all the other planets.
        if ((terrain != null) && (terrain.name == planetName)) {
            terrainMesh.rotate = false;
            if (oceanMesh != null) { oceanMesh.rotate = false; }
            // if we're on the planet, change the cloud shader so it looks right, change the terrain textures so they're detailed.
            if (cloudMesh != null) {
                Material cloudMaterial = materialManager.AssignMaterial("cloud", curPlanetType, curPlanetSeed, true);
                cloud.GetComponent<Renderer>().material = cloudMaterial;
            }
            if (oceanMesh != null) {
                Material oceanMaterial = materialManager.AssignMaterial("ocean", curPlanetType, curPlanetSeed, true);
                oceanMesh.GetComponent<Renderer>().material = oceanMaterial;
            }
            Material terrainMaterial = materialManager.AssignMaterial("terrain", curPlanetType, curPlanetSeed, true);
            terrainMesh.GetComponent<Renderer>().material = terrainMaterial;
            // activate the lights!
            if (!aMoon.enabled) aMoon.Enable(planetDiameter);
            if (!aSun.enabled) aSun.Enable(planetDiameter);
            planetSound.EnableSounds(curPlanetType, hasOcean, hasAtmosphere, planetDiameter, terrain.GetComponent<PlanetTexture>().maxElev);
        }

        else {
            if (terrain != null) { terrainMesh.rotate = true; }
            if (oceanMesh != null) { oceanMesh.rotate = true; }
            // if we're not on a planet, change the terrain textures so they look blended and not tiled, change the clouds back.
            if (cloudMesh != null) {
                Material cloudMaterial = materialManager.AssignMaterial("cloud", curPlanetType, curPlanetSeed, false);
                cloud.GetComponent<Renderer>().material = cloudMaterial;
            }
            if (oceanMesh != null) {
                Material oceanMaterial = materialManager.AssignMaterial("ocean", curPlanetType, curPlanetSeed, false);
                oceanMesh.GetComponent<Renderer>().material = oceanMaterial;
            }
            if (terrainMesh != null) {
                Material terrainMaterial = materialManager.AssignMaterial("terrain", curPlanetType, curPlanetSeed, false);
                terrainMesh.GetComponent<Renderer>().material = terrainMaterial;
            }
            aMoon.Disable();
            aSun.Disable();
            planetSound.DisableSounds();
        }
    }

    public void DestroyPlanetOutline() {
        planetOutline.GetComponent<Renderer>().enabled = false;
        planetOutline.transform.localScale = new Vector3(0, 0, 0);
    }

    public void AddPlanet() {
        // setup the planet.
        AddTerrain(distFromCenter, planetDiameter);
        if (hasOcean) AddOcean(distFromCenter, planetDiameter);
        if (hasAtmosphere) AddAtmosphere(distFromCenter);
        if (hasClouds) AddClouds(distFromCenter);
    }

    public void ManageOcean(bool onplanet) {
        if (!hasOcean) return;
        if (GameObject.Find("aPlanetPartialOcean") && onplanet) { return; }
        if (onplanet) { AddPartialOcean(); return; }
        if (!GameObject.Find("aPlanetOcean")) {
            AddOcean(distFromCenter, planetDiameter);
            Destroy(partialOcean); partialOceanMesh = null;
        }
        return;
    }

    private void AddTerrain(float dist, float planetScale) {
        Material planetSurfaceMaterial = materialManager.AssignMaterial("terrain", curPlanetType, curPlanetSeed);
        terrain = new GameObject("aPlanet");
        terrain.AddComponent<MeshFilter>();
        terrain.AddComponent<MeshRenderer>();
        terrain.AddComponent<PlanetLayers>();
        terrain.GetComponent<Renderer>().material = planetSurfaceMaterial;
        terrainMesh = terrain.GetComponent<PlanetLayers>();
        terrainMesh.GenerateFull("terrain", planetDiameter, curPlanetSeed);
        terrainMesh.transform.position = centerPos + Vector3.forward * dist;
        terrainMesh.center = terrainMesh.transform.position;
    }
    
    private void AddOcean(float dist, float planetScale) {
        Material oceanMaterial = materialManager.AssignMaterial("ocean", curPlanetType, curPlanetSeed);
        ocean = new GameObject("aPlanetOcean");
        ocean.AddComponent<MeshFilter>();
        ocean.AddComponent<MeshRenderer>();
        ocean.AddComponent<PlanetLayers>();
        ocean.GetComponent<Renderer>().material = oceanMaterial;
        oceanMesh = ocean.GetComponent<PlanetLayers>();
        oceanMesh.GenerateFull("ocean", planetDiameter, curPlanetSeed);
        oceanMesh.transform.position =  centerPos + Vector3.forward * dist;
        oceanMesh.center = oceanMesh.transform.position;
    }

    private void AddPartialOcean() {
        // create a smaller, more highly tesselated ocean mesh for better looking water.
        Material partialOceanMaterial = materialManager.AssignMaterial("partialOcean", curPlanetType, curPlanetSeed);
        partialOcean = new GameObject("aPlanetPartialOcean");
        partialOcean.AddComponent<MeshFilter>();
        partialOcean.AddComponent<MeshRenderer>();
        partialOceanMesh = gameObject.AddComponent<PlanetOceanDetail>();
        partialOcean.GetComponent<Renderer>().material = partialOceanMaterial;
        partialOceanMesh.Generate(ocean.GetComponent<MeshFilter>().mesh.triangles, ocean.GetComponent<MeshFilter>().mesh.vertices, planetDiameter);
        partialOcean.GetComponent<MeshFilter>().mesh.vertices = partialOceanMesh.GetVerts();
        partialOcean.GetComponent<MeshFilter>().mesh.triangles = partialOceanMesh.GetTris();
        partialOcean.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        partialOcean.transform.position = centerPos + Vector3.forward * 3500F;
        Destroy(ocean); oceanMesh = null;
    }

    private void AddAtmosphere(float dist) {
        Material atmosphereMaterial = materialManager.AssignMaterial("atmosphere", curPlanetType, curPlanetSeed);
        atmosphere = new GameObject("aPlanetAtmosphere");
        atmosphere.AddComponent<MeshFilter>();
        atmosphere.AddComponent<MeshRenderer>();
        atmosphere.AddComponent<PlanetLayers>();
        // scale the atmosphere to just above the highest mountain.
        float atmosphereScale = terrainMesh.GetMaxElevation() * 2 / planetDiameter;
        atmosphere.GetComponent<Renderer>().material = atmosphereMaterial;
        atmosphereMesh = atmosphere.GetComponent<PlanetLayers>();
        atmosphereMesh.GenerateFull("atmosphere", planetDiameter * atmosphereScale, curPlanetSeed);
        atmosphereMesh.transform.position = centerPos + Vector3.forward * dist;
    }

    public void AddClouds(float dist) {
        Material cloudMaterial = materialManager.AssignMaterial("cloud", curPlanetType, curPlanetSeed);
        cloud = new GameObject("aPlanetCloud");
        cloud.AddComponent<MeshFilter>();
        cloud.AddComponent<MeshRenderer>();
        cloud.AddComponent<PlanetLayers>();
        // scale the clouds to just around the highest mountain.
        float cloudScale = terrainMesh.GetMaxElevation() * 2 / planetDiameter;
        cloudScale -= .01F;
        cloud.GetComponent<Renderer>().material = cloudMaterial;
        cloudMesh = cloud.GetComponent<PlanetLayers>();
        cloudMesh.GenerateFull("cloud", planetDiameter * cloudScale, curPlanetSeed);
        cloudMesh.transform.position = centerPos + Vector3.forward * dist;
    }

    public void AddPlanetOutline() {
        planetOutline.GetComponent<Renderer>().enabled = true;
    }

    public void UpdatePotentialPlanet(int seed) {
        if (planetOutline != null) {
            // update the outline.
            planetOutline.transform.position = centerPos + Vector3.forward * distFromCenter;
            planetOutline.transform.localScale = new Vector3(planetDiameter, planetDiameter, planetDiameter);
            // setup planet metadata and base data.
            rnd = new System.Random(seed); curPlanetSeed = seed;
            // always with atmosphere and clouds.
            if (curPlanetType.Contains("Terra")) { hasAtmosphere = true; hasOcean = true; hasClouds = true; }
            // chance for atmosphere and clouds.
            if (curPlanetType.Contains("Molten")) { hasAtmosphere = true; hasOcean = true; hasClouds = true; }
            // chance for atmosphere and clouds.
            if (curPlanetType.Contains("Icy")) {
                hasOcean = (rnd.NextDouble() < .7F);
                hasAtmosphere = (rnd.NextDouble() < .7F);
                if (hasAtmosphere) { hasClouds = (rnd.NextDouble() < .7F); }
            }
            // no chace for atmosphere and clouds, no ocean.
            if (curPlanetType.Contains("Rock")) { hasAtmosphere = false; hasOcean = false; hasClouds = false; }
            planetMetaData.initialize(seed, planetDiameter, curPlanetType, hasAtmosphere, hasClouds, hasOcean);
        }
    }
}
