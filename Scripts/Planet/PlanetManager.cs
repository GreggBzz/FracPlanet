using UnityEngine;

public class PlanetManager : MonoBehaviour {
    // meshes.
    private PlanetLayers terrainMesh;
    private PlanetLayers oceanMesh;
    private PlanetLayers atmosphereMesh;
    private PlanetLayers cloudMesh;
    private PlanetOceanDetail partialOceanTopMesh;
    private PlanetOceanDetail partialOceanBottomMesh;
    private PlanetTerrainDetail partialTerrainTopMesh;
    // Gameobjects and boolean base attributes.
    private GameObject terrain;
    private GameObject ocean;
    private GameObject partialOceanTop;
    private GameObject partialOceanBottom;
    private GameObject partialTerrainTop;
    public GameObject atmosphere;
    public GameObject cloud;
    public bool hasOcean;
    public bool hasAtmosphere;
    public bool hasClouds;
    // lights
    private Sun aSun;
    // sounds
    private PlanetSounds planetSound;
    // dimensions, seed, type.
    private const float distFromCenter = 3500F;
    public float planetDiameter = 2500F;
    public string curPlanetType = "";
    public int curPlanetSeed = 100;
    private Vector3 centerPos = new Vector3(0, 750, 0);

    public GameObject planetOutline; // public for user(wand) manipulated transforms.
    private PlanetMaterial materialManager; // manage the planet layer materials with a class.
    public PlanetMetaData planetMetaData; // the metadata for the planet, compounds, mass, weather etc.

    // planet objects
    private GrassManager grassManager;
    private RocksManager rocksManager;

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
    }

    public void DestroyPlanet() {
        if (GameObject.Find("allTheGrass") != null) {
            terrain.GetComponent<GrassManager>().DestroyGrass();
        }
        if (GameObject.Find("allTheRocks") != null) {
            terrain.GetComponent<RocksManager>().DestroyRocks();
        }
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
        if (partialOceanTop != null) {
            Destroy(partialOceanTop); partialOceanTop = null;
        }
        if (partialOceanBottom != null) {
            Destroy(partialOceanBottom); partialOceanBottom = null;
        }
        if (partialTerrainTop != null) {
            Destroy(partialTerrainTop); partialTerrainTop = null;
        }
        planetSound.DisableSounds();

    }

    public void PausePlanet(string planetName) {
        // "pause" a planet we've teleported to.
        // "unpause" all the other planets.
        if (((terrain != null) && (terrain.name == planetName)) || ((partialTerrainTop != null) && (partialTerrainTop.name == planetName))) {
            terrainMesh.rotate = false;
            if (oceanMesh != null) { oceanMesh.rotate = false; }
            // if we're on the planet, change the cloud shader so it looks right. 
            if (cloudMesh != null) {
                Material cloudMaterial = materialManager.AssignMaterial("cloud", curPlanetType, curPlanetSeed, true);
                cloud.GetComponent<Renderer>().material = cloudMaterial;
            }
            // activate the lights!
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
        if (!hasOcean) return; // if it's a planet w/o an ocean, return.       
        if (GameObject.Find("aPlanetTopOcean") && onplanet) {
            return; // we've already made the ocean detail, peace out.
        }
        if (onplanet) {
            AddPartialOceans();
            return;
        }
        if (!GameObject.Find("aPlanetOcean")) {
            Destroy(partialOceanTop); partialOceanTopMesh = null;
            Destroy(partialOceanBottom); partialOceanBottomMesh = null;
            AddOcean(distFromCenter, planetDiameter);
        }
        return;
    }

    public void ManageTerrain(bool onplanet) {
        if (onplanet) {
            if (GameObject.Find("aPlanetTopTerrain")) {
                Destroy(partialTerrainTop); partialTerrainTopMesh = null;
            }
            AddPartialTerrain();
            return;
        }
        if (!onplanet) {
            if (GameObject.Find("aPlanetTopTerrain")) {
                Destroy(partialTerrainTop); partialTerrainTopMesh = null;
            }
            if (GameObject.Find("aPlanet")) {
                GameObject.Find("aPlanet").GetComponent<Renderer>().enabled = true;
                GameObject.Find("aPlanet").GetComponent<MeshCollider>().enabled = true;
            }
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
        // we'll need Biomes for later.
        grassManager = GameObject.Find("aPlanet").AddComponent<GrassManager>();
        // we'll need rocks for later.
        rocksManager = GameObject.Find("aPlanet").AddComponent<RocksManager>();
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

    private void AddPartialOceans() {
        partialOceanTop = new GameObject("aPlanetTopOcean");
        // create a smaller, more highly tesselated ocean mesh at the top for better looking water.
        partialOceanTopMesh = GameObject.Find("aPlanetTopOcean").AddComponent<PlanetOceanDetail>();
        // truncate the existing ocean mesh leaving the bottom part so everything looks right.
        partialOceanBottomMesh = GameObject.Find("aPlanetTopOcean").AddComponent<PlanetOceanDetail>();
        // make the top part.
        Material partialOceanMaterial = materialManager.AssignMaterial("partialOcean", curPlanetType, curPlanetSeed);
        partialOceanTop.AddComponent<MeshFilter>();
        partialOceanTop.AddComponent<MeshRenderer>();
        partialOceanTop.GetComponent<Renderer>().material = partialOceanMaterial;
        partialOceanTopMesh.Generate(ocean.GetComponent<MeshFilter>().mesh.triangles, ocean.GetComponent<MeshFilter>().mesh.vertices, planetDiameter);
        partialOceanTop.GetComponent<MeshFilter>().mesh.vertices = partialOceanTopMesh.GetVerts();
        partialOceanTop.GetComponent<MeshFilter>().mesh.triangles = partialOceanTopMesh.GetTris();
        partialOceanTop.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        partialOceanTop.transform.position = centerPos + Vector3.forward * 3500F;
        // make the bottom part
        Color tmpColor = partialOceanMaterial.GetColor("_BaseColor");
        partialOceanMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
        partialOceanMaterial.SetColor("_TintColor", tmpColor);
        partialOceanBottom = new GameObject("aPlanetBottomOcean");
        partialOceanBottom.AddComponent<MeshFilter>();
        partialOceanBottom.AddComponent<MeshRenderer>();
        partialOceanBottom.GetComponent<Renderer>().material = partialOceanMaterial;
        partialOceanBottomMesh.Generate(ocean.GetComponent<MeshFilter>().mesh.triangles, ocean.GetComponent<MeshFilter>().mesh.vertices, planetDiameter, true);
        partialOceanBottom.GetComponent<MeshFilter>().mesh.vertices = partialOceanBottomMesh.GetVerts(true);
        partialOceanBottom.GetComponent<MeshFilter>().mesh.triangles = partialOceanBottomMesh.GetTris();
        partialOceanBottom.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        partialOceanBottom.transform.position = centerPos + Vector3.forward * 3500F;
        Destroy(ocean); oceanMesh = null;
    }

    private void AddPartialTerrain() {
        // create a smaller, more highly tesselated terrain mesh at the top.
        partialTerrainTop = new GameObject("aPlanetTopTerrain");
        partialTerrainTopMesh = GameObject.Find("aPlanetTopTerrain").AddComponent<PlanetTerrainDetail>();
        Material planetSurfaceMaterial = materialManager.AssignMaterial("terrain", curPlanetType, curPlanetSeed, true);
        // we'll need a texture manager to calculate the textures.
        PlanetTexture textureManager = GameObject.Find("aPlanetTopTerrain").AddComponent<PlanetTexture>();
        partialTerrainTop.AddComponent<MeshFilter>();
        partialTerrainTop.AddComponent<MeshRenderer>();
        partialTerrainTop.GetComponent<Renderer>().material = planetSurfaceMaterial;
        partialTerrainTopMesh.Generate(terrain.GetComponent<MeshFilter>().mesh.triangles, terrain.GetComponent<MeshFilter>().mesh.vertices, planetDiameter);
        partialTerrainTop.GetComponent<MeshFilter>().mesh.vertices = partialTerrainTopMesh.GetVerts();
        partialTerrainTop.GetComponent<MeshFilter>().mesh.triangles = partialTerrainTopMesh.GetTris();
        partialTerrainTop.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        // setup some shorthand variables for the texture method.
        int vertCount = partialTerrainTopMesh.GetVertCount();
        Vector3[] verts = partialTerrainTop.GetComponent<MeshFilter>().mesh.vertices;
        int[] tris = partialTerrainTop.GetComponent<MeshFilter>().mesh.triangles;
        float partialMaxElev = terrain.GetComponent<PlanetTexture>().maxElev;
        float partialMinElev = terrain.GetComponent<PlanetTexture>().minElev;
        partialTerrainTop.GetComponent<MeshFilter>().mesh.uv = textureManager.Texture(vertCount, verts, tris);
        partialTerrainTop.GetComponent<MeshFilter>().mesh.uv4 = textureManager.AssignSplatElev(vertCount, verts, true, partialMinElev, partialMaxElev);
        partialTerrainTop.AddComponent<MeshCollider>();
        partialTerrainTop.GetComponent<MeshCollider>().enabled = true;
        partialTerrainTop.transform.position = terrain.transform.position;
        // clean up.
        textureManager = null; verts = null; tris = null;
        // deal with our biomes.
        grassManager.PlaceAndEnableGrass();
        rocksManager.PlaceAndEnableRocks();
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

    public float GetOceanDiameter() {
        if (partialOceanTopMesh != null) {
            return partialOceanTopMesh.getDiameter();
        }
        return -10000.0F; // ocean way down to prevent y < ocean false blur. 
    }

    public void UpdatePotentialPlanet(int seed) {
        if (planetOutline != null) {
            // update the outline of our potential planet.
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
