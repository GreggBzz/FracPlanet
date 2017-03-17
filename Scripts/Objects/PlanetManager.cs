using UnityEngine;

public class PlanetManager : MonoBehaviour {
    private PlanetMesh[] terrains = new PlanetMesh[10];
    private PlanetMesh[] oceans = new PlanetMesh[10];
    private PlanetMesh[] atmospheres = new PlanetMesh[10];
    private PlanetMesh[] clouds = new PlanetMesh[10];

    private int terrainMeshCount = 0;
    private int oceanMeshCount = 0;
    private int atmosphereMeshCount = 0;
    private int cloudMeshCount = 0;

    private GameObject[] terrain = new GameObject[10];
    private GameObject[] ocean = new GameObject[10];
    public GameObject[] atmosphere = new GameObject[10];
    public GameObject[] cloud = new GameObject[10]; 

    public float distScale = 2000F;
    public float planetCircumference = 2500F;
    public string curPlanetType = "";
    private int curPlanetSeed = 100;

    public GameObject planetOutline; // public for user(wand) manipulated transforms.
    private PlanetMaterial materialManager; // Manage the planet layer materials with a class.

    private System.Random rnd; // chance for current planet atmosphere, clouds?

    public void Start() {
        materialManager = gameObject.AddComponent<PlanetMaterial>();
    }

    public void DestroyPlanets() {
        for (int i = 0; i <= terrainMeshCount; i++) {
            Destroy(terrain[i]);
            terrains[i] = null;
        }
        terrainMeshCount = 0;
        for (int i = 0; i <= oceanMeshCount; i++) {
            Destroy(ocean[i]);
            oceans[i] = null;
        }
        oceanMeshCount = 0;
        for (int i = 0; i <= atmosphereMeshCount; i++) {
            Destroy(atmosphere[i]);
            atmospheres[i] = null;
        }
        atmosphereMeshCount = 0;
        for (int i = 0; i <= cloudMeshCount; i++) {
            Destroy(cloud[i]);
            cloud[i] = null;
        }
        cloudMeshCount = 0;
    }

    public void PausePlanet(string planetName) {
        // "pause" a planet we've teleported to.
        // "unpause" all the other planets.
        for (int i = 0; i <= terrainMeshCount - 1; i++) {
                if ((terrain[i] != null) && (terrain[i].name == planetName)) {
                    terrains[i].rotate = false;
                    if (oceans[i] != null) { oceans[i].rotate = false; }
                }
                else {
                    if (terrain[i] != null) { terrains[i].rotate = true; }
                    if (oceans[i] != null) { oceans[i].rotate = true; }
                }
            }
        }

    public void DestroyPlanetOutline() {
        Destroy(planetOutline);
    }

    public void AddPlanet(Transform controllerTransform, float distScale, float planetScale, bool hasOcean = false, 
                          bool hasAtmosphere = false, bool hasClouds = false, int seed = 100) {
        if (seed == 100) { seed = Random.Range(0, 20000); }
        rnd = new System.Random(seed); curPlanetSeed = seed;

        // always with atmosphere and clouds.
        if (curPlanetType.Contains("Terra")) { hasAtmosphere = true;  hasOcean = true; hasClouds = true; }
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

        AddTerrain(controllerTransform, distScale, planetScale);
        // setup the planet.
        if (hasOcean) AddOcean(controllerTransform, distScale, planetScale);
        if (hasAtmosphere) AddAtmosphere(controllerTransform, distScale);
        if (hasClouds) AddClouds(controllerTransform, distScale);

    }

    private void AddTerrain(Transform controllerTransform, float distScale, float planetScale) {
        Material planetSurfaceMaterial = materialManager.AssignMaterial("terrain", curPlanetType);
        terrain[terrainMeshCount] = new GameObject("aPlanet[" + terrainMeshCount + "]");
        terrain[terrainMeshCount].AddComponent<MeshFilter>();
        terrain[terrainMeshCount].AddComponent<MeshRenderer>();
        terrain[terrainMeshCount].AddComponent<PlanetMesh>();
        terrain[terrainMeshCount].GetComponent<PlanetMesh>().circumference = planetCircumference;
        terrain[terrainMeshCount].GetComponent<Renderer>().material = planetSurfaceMaterial;
        terrains[terrainMeshCount] = terrain[terrainMeshCount].GetComponent<PlanetMesh>();
        terrains[terrainMeshCount].planetLayer = "terrain";
        terrains[terrainMeshCount].seed = curPlanetSeed;
        terrains[terrainMeshCount].Generate();
        terrains[terrainMeshCount].transform.position = controllerTransform.position + controllerTransform.forward * distScale;
        terrains[terrainMeshCount].transform.eulerAngles = controllerTransform.eulerAngles;
        terrains[terrainMeshCount].center = terrains[terrainMeshCount].transform.position;
        terrainMeshCount += 1;
    }

    private void AddOcean(Transform controllerTransform, float distScale, float planetScale) {
        Material oceanMaterial = materialManager.AssignMaterial("ocean", curPlanetType);
        ocean[oceanMeshCount] = new GameObject("aPlanetOcean[" + oceanMeshCount + "]");
        ocean[oceanMeshCount].AddComponent<MeshFilter>();
        ocean[oceanMeshCount].AddComponent<MeshRenderer>();
        ocean[oceanMeshCount].AddComponent<PlanetMesh>();
        ocean[oceanMeshCount].GetComponent<PlanetMesh>().circumference = planetCircumference;
        ocean[oceanMeshCount].GetComponent<Renderer>().material = oceanMaterial;
        oceans[oceanMeshCount] = ocean[oceanMeshCount].GetComponent<PlanetMesh>();
        oceans[oceanMeshCount].planetLayer = "ocean";
        oceans[oceanMeshCount].seed = curPlanetSeed;
        oceans[oceanMeshCount].Generate();
        oceans[oceanMeshCount].transform.position = controllerTransform.position + controllerTransform.forward * distScale;
        oceans[oceanMeshCount].transform.eulerAngles = controllerTransform.eulerAngles;
        oceans[oceanMeshCount].center = oceans[oceanMeshCount].transform.position;
        oceanMeshCount += 1;
    }

    private void AddAtmosphere(Transform controllerTransform, float distScale) {
        Material atmosphereMaterial = materialManager.AssignMaterial("atmosphere", curPlanetType);
        atmosphere[atmosphereMeshCount] = new GameObject("aPlanetAtmosphere[" + atmosphereMeshCount + "]");
        atmosphere[atmosphereMeshCount].AddComponent<MeshFilter>();
        atmosphere[atmosphereMeshCount].AddComponent<MeshRenderer>();
        atmosphere[atmosphereMeshCount].AddComponent<PlanetMesh>();
        // scale the atmosphere to just above the highest mountain.
        float atmosphereScale = terrains[atmosphereMeshCount].GetMaxElevation() * 2 / planetCircumference;
        atmosphere[atmosphereMeshCount].GetComponent<PlanetMesh>().circumference = planetCircumference * atmosphereScale;
        atmosphere[atmosphereMeshCount].GetComponent<Renderer>().material = atmosphereMaterial;
        atmospheres[atmosphereMeshCount] = atmosphere[atmosphereMeshCount].GetComponent<PlanetMesh>();
        atmospheres[atmosphereMeshCount].planetLayer = "atmosphere";
        atmospheres[atmosphereMeshCount].seed = curPlanetSeed;
        atmospheres[atmosphereMeshCount].Generate();
        atmospheres[atmosphereMeshCount].transform.position = controllerTransform.position + controllerTransform.forward * distScale;
        atmospheres[atmosphereMeshCount].transform.eulerAngles = controllerTransform.eulerAngles;
        atmosphereMeshCount += 1;
    }

    public void AddClouds(Transform controllerTransform, float distScale) {
        Material cloudMaterial = materialManager.AssignMaterial("cloud", curPlanetType, curPlanetSeed);
        cloud[cloudMeshCount] = new GameObject("aPlanetCloud[" + cloudMeshCount + "]");
        cloud[cloudMeshCount].AddComponent<MeshFilter>();
        cloud[cloudMeshCount].AddComponent<MeshRenderer>();
        cloud[cloudMeshCount].AddComponent<PlanetMesh>();
        // scale the clouds to just around the highest mountain.
        float cloudScale = terrains[cloudMeshCount].GetMaxElevation() * 2 / planetCircumference;
        cloudScale -= .01F;
        cloud[cloudMeshCount].GetComponent<PlanetMesh>().circumference = planetCircumference * cloudScale;
        cloud[cloudMeshCount].GetComponent<Renderer>().material = cloudMaterial;
        clouds[cloudMeshCount] = cloud[cloudMeshCount].GetComponent<PlanetMesh>();
        clouds[cloudMeshCount].planetLayer = "cloud";
        clouds[cloudMeshCount].seed = curPlanetSeed;
        clouds[cloudMeshCount].Generate();
        clouds[cloudMeshCount].transform.position = controllerTransform.position + controllerTransform.forward * distScale;
        clouds[cloudMeshCount].transform.eulerAngles = controllerTransform.eulerAngles;
        cloudMeshCount += 1;
    }

    public void AddPlanetOutline() {
        Material planetOutlineMaterial = materialManager.AssignMaterial("outline");
        planetOutline = new GameObject("Planet Outline");
        planetOutline = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        planetOutline.GetComponent<Renderer>().material = planetOutlineMaterial;
        planetOutline.transform.localScale = new Vector3(planetCircumference, planetCircumference, planetCircumference);
    }

    public void UpdatePlanetOutline(Transform controllerTransform) {
        if (planetOutline != null) {
            planetOutline.transform.position = controllerTransform.position + controllerTransform.forward * distScale;
            planetOutline.transform.localScale = new Vector3(planetCircumference, planetCircumference, planetCircumference);
        }
    }
}
