using UnityEngine;

public class PlanetManager : MonoBehaviour {
    // basic planet properties
    public bool hasOcean;
    public bool hasAtmosphere;
    public bool hasClouds;
    // "from a distance" gameobjects
    private GameObject terrain;
    private GameObject ocean;
    public GameObject atmosphere;
    public GameObject cloud;
    // "from a distance" meshes
    private PlanetLayers terrainMesh;
    private PlanetLayers oceanMesh;
    private PlanetLayers atmosphereMesh;
    private PlanetLayers cloudMesh;
    // "while planetside" gameobjects and meshes.
    private PlanetOceanDetail partialOceanGenerator;
    private PlanetTerrainDetail partialTerrainGenerator;
    private GameObject partialOceanTop;
    private GameObject partialTerrainTop;
    private GameObject partialTerrainBottom;
    // lights
    private Sun aSun;
    // sounds
    private PlanetSounds planetSound;
    // dimensions, seed, type.
    private const float distFromCenter = 3500F;
    public const int maxDiameter = 3000;
    public const int minDiameter = 1500;
    public float planetDiameter = 2500F;
    public string curPlanetType = "";
    public int curPlanetSeed = 100;
    private int[] textureFolder;
    private Vector3 centerPos = new Vector3(0, 750, 0);

    public GameObject planetOutline; // public for user(wand) manipulated transforms.
    private PlanetMaterial materialManager; // manage the planet layer materials with a class.
    private PlanetTexture textureManager; // manage textures.
    public PlanetMetaData planetMetaData; // the metadata for the planet, compounds, mass, weather etc.

    // planet objects
    private GrassManager grassManager;
    private TreeManager treeManager;
    private RocksManager rocksManager;
    private FogManager fogManager;

    private System.Random rnd; // chance for current planet atmosphere, clouds?

    public void Start() {
        materialManager = gameObject.AddComponent<PlanetMaterial>();
        planetMetaData = gameObject.AddComponent<PlanetMetaData>();
        textureManager = gameObject.AddComponent<PlanetTexture>();
        planetSound = gameObject.AddComponent<PlanetSounds>();
        partialTerrainGenerator = gameObject.AddComponent<PlanetTerrainDetail>();

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
        if (GameObject.Find("allTheTrees") != null) {
            terrain.GetComponent<TreeManager>().DestroyTrees();
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
        if (partialTerrainBottom != null) {
            Destroy(partialTerrainBottom); partialTerrainBottom = null;
        }
        if (partialTerrainTop != null) {
            Destroy(partialTerrainTop); partialTerrainTop = null;
        }
        if (fogManager != null) {
            fogManager.DisableFog();
        }
        planetSound.DisableSounds();
    }

    public void PausePlanet(string planetName) {
        // stop rotating terrain and ocean once we've teleported to a planet
        if (((terrain != null) && (terrain.name == planetName)) || ((partialTerrainTop != null) && (partialTerrainTop.name == planetName))) {
            terrainMesh.rotate = false;
            if (oceanMesh != null) { oceanMesh.rotate = false; }
            // if we're on the planet, change the cloud shader so it looks right. 
            if (cloudMesh != null) {
                Material cloudMaterial = materialManager.AssignMaterial("cloud", curPlanetType, true);
                cloud.GetComponent<Renderer>().material = cloudMaterial;
            }
            // activate the lights
            if (!aSun.enabled) aSun.Enable(planetDiameter);
            planetSound.EnableSounds(curPlanetType, hasOcean, hasAtmosphere, planetDiameter, terrain.GetComponent<PlanetGeometryDetail>().maxHeight);
        }
        else {
            if (terrain != null) { terrainMesh.rotate = true; }
            if (oceanMesh != null) { oceanMesh.rotate = true; }
            // if we're not on a planet change the cloud and ocean materials back so they look right, disable the sun and sound
            if (cloudMesh != null) {
                Material cloudMaterial = materialManager.AssignMaterial("cloud", curPlanetType, false);
                cloud.GetComponent<Renderer>().material = cloudMaterial;
            }
            if (oceanMesh != null) {
                Material oceanMaterial = materialManager.AssignMaterial("ocean", curPlanetType, false);
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
        materialManager.SetTextureFolders();
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
            Destroy(partialOceanTop); partialOceanGenerator = null;
            AddOcean(distFromCenter, planetDiameter);
        }
        return;
    }

    public void ManageTerrain(bool onplanet) {
        if (onplanet) {
            if (GameObject.Find("aPlanetTopTerrainCollide")) {
                Destroy(partialTerrainTop);
                Destroy(partialTerrainBottom);
            }
            SplitTerrainAddCollider();
            return;
        }
        if (!onplanet) {
            if (GameObject.Find("aPlanetTopTerrainCollide")) {
                Destroy(partialTerrainTop);
                Destroy(partialTerrainBottom);
            }
            if (GameObject.Find("aPlanet")) {
                GameObject.Find("aPlanet").GetComponent<Renderer>().enabled = true;
                GameObject.Find("aPlanet").GetComponent<MeshCollider>().enabled = true;
            }
        }
        return;
    }

    private void AddTerrain(float dist, float planetScale) {
        Material planetSurfaceMaterial = materialManager.AssignMaterial("terrain", curPlanetType);
        terrain = new GameObject("aPlanet");
        terrain.AddComponent<MeshFilter>();
        terrain.AddComponent<MeshRenderer>();
        terrain.AddComponent<PlanetLayers>();
        terrain.GetComponent<Renderer>().material = planetSurfaceMaterial;
        terrainMesh = terrain.GetComponent<PlanetLayers>();
        terrainMesh.GenerateFull("terrain", planetDiameter, curPlanetSeed);
        terrainMesh.transform.position = centerPos + Vector3.forward * dist;
        terrainMesh.center = terrainMesh.transform.position;
        // we'll need grass for later
        grassManager = GameObject.Find("aPlanet").AddComponent<GrassManager>();
        // we'll need rocks for later
        rocksManager = GameObject.Find("aPlanet").AddComponent<RocksManager>();
        // fog for most planets.
        fogManager = GameObject.Find("aPlanet").AddComponent<FogManager>();
        // trees for most planets.
        treeManager = GameObject.Find("aPlanet").AddComponent<TreeManager>();
    }

    private void AddOcean(float dist, float planetScale) {
        Material oceanMaterial = materialManager.AssignMaterial("ocean", curPlanetType);
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
        // create a smaller, more highly tesselated ocean mesh at the top for better looking water
        partialOceanGenerator = GameObject.Find("aPlanetTopOcean").AddComponent<PlanetOceanDetail>();
        Material partialOceanMaterial = materialManager.AssignMaterial("partialOcean", curPlanetType);
        partialOceanTop.AddComponent<MeshFilter>();
        partialOceanTop.AddComponent<MeshRenderer>();
        partialOceanTop.GetComponent<Renderer>().material = partialOceanMaterial;
        partialOceanGenerator.Generate(ocean.GetComponent<MeshFilter>().mesh.triangles, ocean.GetComponent<MeshFilter>().mesh.vertices, planetDiameter);
        partialOceanTop.GetComponent<MeshFilter>().mesh.vertices = partialOceanGenerator.GetVerts();
        partialOceanTop.GetComponent<MeshFilter>().mesh.triangles = partialOceanGenerator.GetTris();
        partialOceanTop.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        partialOceanTop.GetComponent<MeshFilter>().mesh.RecalculateTangents();
        partialOceanTop.AddComponent<MeshCollider>();
        partialOceanTop.GetComponent<MeshCollider>().enabled = true;
        partialOceanTop.layer = 4; // builtin water.
        partialOceanTop.transform.position = centerPos + Vector3.forward * 3500F;
        // Texture the partial ocean.
        int vertCount = partialOceanGenerator.GetVertCount();
        Vector3[] verts = partialOceanTop.GetComponent<MeshFilter>().mesh.vertices;
        int[] tris = partialOceanTop.GetComponent<MeshFilter>().mesh.triangles;
        partialOceanTop.GetComponent<MeshFilter>().mesh.uv = textureManager.Texture(vertCount, verts, tris, true);
        Destroy(ocean); oceanMesh = null;
    }

    private void SplitTerrainAddCollider() {
        // create a small highly tesselated terrain mesh near top dead center, close to the player with a detailed collider.
        // add material and texture manager
        Material planetSurfaceMaterial = materialManager.AssignMaterial("terrain", curPlanetType, true);
        // run the mesh generator.
        partialTerrainGenerator.maxDistance = 150F;
        partialTerrainGenerator.minDistance = 0F;
        partialTerrainGenerator.Generate(terrain.GetComponent<MeshFilter>().mesh.triangles, 
                                         terrain.GetComponent<MeshFilter>().mesh.vertices, planetDiameter,
                                         terrain.GetComponent<MeshFilter>().mesh.uv,
                                         terrain.GetComponent<MeshFilter>().mesh.uv3,
                                         terrain.GetComponent<MeshFilter>().mesh.uv4);
        // build out the top mesh.
        partialTerrainTop = new GameObject("aPlanetTopTerrainCollide");
        partialTerrainTop.AddComponent<MeshFilter>();
        partialTerrainTop.AddComponent<MeshRenderer>();
        partialTerrainTop.GetComponent<Renderer>().material = planetSurfaceMaterial;
        partialTerrainTop.GetComponent<MeshFilter>().mesh.vertices = partialTerrainGenerator.GetVerts("close");
        partialTerrainTop.GetComponent<MeshFilter>().mesh.triangles = partialTerrainGenerator.GetTris("close");
        partialTerrainTop.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        // some shorthand variables for the texture method, and texture the top part
        int vertCount = partialTerrainGenerator.GetVertCount("close");
        Vector3[] verts = partialTerrainTop.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] normals = partialTerrainTop.GetComponent<MeshFilter>().mesh.normals;
        int[] tris = partialTerrainTop.GetComponent<MeshFilter>().mesh.triangles;
        partialTerrainTop.GetComponent<MeshFilter>().mesh.uv = textureManager.Texture(vertCount, verts, tris);
        partialTerrainTop.GetComponent<MeshFilter>().mesh.uv4 = textureManager.AssignSplatElev(verts, normals);
        partialTerrainTop.GetComponent<MeshFilter>().mesh.uv3 = textureManager.GetSplatSpecials();
        partialTerrainTop.AddComponent<MeshCollider>();
        partialTerrainTop.GetComponent<MeshCollider>().enabled = true;
        partialTerrainTop.transform.position = terrain.transform.position;
        // setup the bottom mesh, which was textured in the mesh generator.
        partialTerrainBottom = new GameObject("aPlanetBottomTerrain");
        partialTerrainBottom.AddComponent<MeshFilter>();
        partialTerrainBottom.AddComponent<MeshRenderer>();
        partialTerrainBottom.GetComponent<Renderer>().material = planetSurfaceMaterial;
        partialTerrainBottom.GetComponent<MeshFilter>().mesh.vertices = partialTerrainGenerator.GetVerts("far");
        partialTerrainBottom.GetComponent<MeshFilter>().mesh.triangles = partialTerrainGenerator.GetTris("far");
        partialTerrainBottom.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        // setup some variables for the texture method, and texture the bottom part
        vertCount = partialTerrainGenerator.GetVertCount("far");
        partialTerrainBottom.GetComponent<MeshFilter>().mesh.uv = partialTerrainGenerator.GetFarUv();
        partialTerrainBottom.GetComponent<MeshFilter>().mesh.uv3 = partialTerrainGenerator.GetFarUv3();
        partialTerrainBottom.GetComponent<MeshFilter>().mesh.uv4 = partialTerrainGenerator.GetFarUv4();
        // tuck the bottom part a down smidge to reduce noticable mesh seams.
        partialTerrainBottom.transform.position = terrain.transform.position - new Vector3(0F, 0.35F, 0F);
        // set the bottom mesh texture tiling = 3 * the detail mesh tiling so it blends.
        for (int i = 0; i <= 6 - 1; i++) {
            partialTerrainBottom.GetComponent<Renderer>().material.SetTextureScale("_Texture" + (i + 1), 
                planetSurfaceMaterial.GetTextureScale("_Texture" + (i + 1)) * 3);
            partialTerrainBottom.GetComponent<Renderer>().material.SetTextureScale("_Normal" + (i + 1), 
                planetSurfaceMaterial.GetTextureScale("_Normal" + (i + 1)) * 3);
        }
    }

    public void placePlanetObjects() {
        rocksManager.PlaceAndEnableRocks();
        grassManager.PlaceAndEnableGrass();
        fogManager.PlaceAndEnableFog();
        treeManager.PlaceAndEnableTrees();
    }

    private void AddAtmosphere(float dist) {
        Material atmosphereMaterial = materialManager.AssignMaterial("atmosphere", curPlanetType);
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
        Material cloudMaterial = materialManager.AssignMaterial("cloud", curPlanetType);
        cloud = new GameObject("aPlanetCloud");
        cloud.AddComponent<MeshFilter>();
        cloud.AddComponent<MeshRenderer>();
        cloud.AddComponent<PlanetLayers>();
        // scale the clouds to just around the highest mountain.
        float cloudScale = terrainMesh.GetMaxElevation() * 2 / planetDiameter;
        cloudScale -= .005F;
        cloud.GetComponent<Renderer>().material = cloudMaterial;
        cloudMesh = cloud.GetComponent<PlanetLayers>();
        cloudMesh.GenerateFull("cloud", planetDiameter * cloudScale, curPlanetSeed);
        cloudMesh.transform.position = centerPos + Vector3.forward * dist;
    }

    public void AddPlanetOutline() {
        planetOutline.GetComponent<Renderer>().enabled = true;
    }

    public float GetOceanDiameter() {
        if (partialOceanGenerator != null) {
            return partialOceanGenerator.getDiameter();
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
                if (hasOcean) {
                    hasAtmosphere = true;
                } else {
                    hasAtmosphere = (rnd.NextDouble() < .7F);
                }
                if (hasAtmosphere) { hasClouds = (rnd.NextDouble() < .7F); }
            }
            // no chace for atmosphere and clouds, no ocean.
            if (curPlanetType.Contains("Rock")) { hasAtmosphere = false; hasOcean = false; hasClouds = false; }
            planetMetaData.initialize(curPlanetSeed, planetDiameter, curPlanetType, hasAtmosphere, hasClouds, hasOcean);
        }
    }
}
