using UnityEngine;

public class GrassManager : MonoBehaviour {
    private bool grassMade = false;
    private bool grassEnabled = false;
    private bool grassPlaced = false;

    // For ~100FPS on a 1060, shoot for < 2500 grass in the FOV using 10 materials.
    // Because of dynamic batching, performance roughly scales with (grassMaterials * grassCount).
    public const int wholePlanetVertCount = 40962;
    public const int grassMaxCount = 10000;
    public const int grassMaterials = 10;
    public const float grassScatterArea = 6;
    private const int grassClusterSize = 10;
    
    public struct GrassCluster {
        public Vector2 centerLocation;
        public Vector3[] offSetAndType;
        public bool haveGrass;
        public bool display;
    }

    public GrassCluster[] grassCluster = new GrassCluster[wholePlanetVertCount];

    public bool globalGrassLocations = false;
    
    private GameObject allTheGrass;
    private Grass[] grassMesh = new Grass[grassMaxCount];
    private GameObject[] aGrass = new GameObject[grassMaxCount];
    private Material[] grassMaterial = new Material[grassMaterials];
    public Vector2[] grassPos = new Vector2[grassMaxCount];

    void Awake () {
        // partent object to stuff all the little grass children into.
        allTheGrass = new GameObject("allTheGrass");
    }
    
    public void AddGrass() {
        if (grassMade) { return; }

        int materialIndex = 0;

        for (int i = 0; i <= grassMaterials - 1; i++) {
            grassMaterial[i] = GetMaterial();
        }      
        for (int i = 0; i <= grassMaxCount - 1; i++) {
            grassMesh[i] = new Grass();
            grassMesh[i].Generate();
        }
        for (int i = 0; i <= grassMaxCount - 1; i++) {
            aGrass[i] = new GameObject("aGrass" + i);
            aGrass[i].AddComponent<MeshFilter>();
            aGrass[i].AddComponent<MeshRenderer>();

            aGrass[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            aGrass[i].GetComponent<Renderer>().receiveShadows = false;
            
            aGrass[i].GetComponent<MeshFilter>().mesh.vertices = grassMesh[i].GetVerts();
            aGrass[i].GetComponent<MeshFilter>().mesh.triangles = grassMesh[i].GetTris();
            aGrass[i].GetComponent<MeshFilter>().mesh.uv = grassMesh[i].GetUv();

            aGrass[i].GetComponent<Renderer>().material = grassMaterial[materialIndex];
            materialIndex += 1; if (materialIndex >= grassMaterials) { materialIndex = 0; }       

            grassMesh[i].SetNormals(aGrass[i].GetComponent<MeshFilter>().mesh.normals);
            aGrass[i].GetComponent<MeshFilter>().mesh.normals = grassMesh[i].GetNormals();
            aGrass[i].GetComponent<MeshFilter>().mesh.RecalculateBounds();
            aGrass[i].GetComponent<Renderer>().enabled = true;
            // put all the grass under the parent object in the inspector.
            aGrass[i].transform.parent = allTheGrass.transform;
        }
        grassMade = true;
        DisableGrass();
    }

    // On first teleport, position the grass around the entire planet.
    // Vertcount is the count for the whole planet not the tesselated detail.
    public void PositionGrass(int GlobalGrassCount = 35000, int vertCount = wholePlanetVertCount) {
        if (globalGrassLocations) { return; }
        int aGrassSpotTemp = 0;
        int grassCount = 0;
        do {
            aGrassSpotTemp = UnityEngine.Random.Range(0, vertCount);
            if (grassCluster[aGrassSpotTemp].haveGrass) {
                continue;
            }
            // assign our vert spot true for grass and create a cluster around it.
            grassCluster[aGrassSpotTemp].offSetAndType = new Vector3[grassClusterSize];
            grassCluster[aGrassSpotTemp].haveGrass = true;
            for (int i = 0; i <= grassClusterSize - 1; i++) {
                float xPos = Random.Range(-grassScatterArea, grassScatterArea);
                float zPos = Random.Range(-grassScatterArea, grassScatterArea);
                grassCluster[aGrassSpotTemp].offSetAndType[i] = new Vector3(xPos, 0F, zPos);
            }          
            grassCount += 1;
        } while (grassCount < GlobalGrassCount);
        globalGrassLocations = true;
    }

    private Material GetMaterial() {
        Material aGrassMaterial;
        Texture grassTexture = Resources.Load("BiomeTextures/Grass" + Random.Range(1, 5)) as Texture;
        //Texture grassTexture = Resources.Load("BiomeTextures/grassTest3") as Texture;
        aGrassMaterial = new Material(Shader.Find("Custom/SimpleGrassSine"));
        aGrassMaterial.SetTexture("_MainTex", grassTexture);
        aGrassMaterial.SetTexture("_Illum", grassTexture);
        aGrassMaterial.SetColor("_Color", GetColor());
        aGrassMaterial.renderQueue = 1000;
        aGrassMaterial.SetFloat("_XStrength", Random.Range(.01F, .1F));
        aGrassMaterial.SetFloat("_XDisp", Random.Range(-.7F, .7F));
        aGrassMaterial.SetFloat("_WindFrequency", Random.Range(.09F, .25F));
        return aGrassMaterial;
    }
    private Color32 GetColor() {
        int R = Random.Range(180, 255); int G = Random.Range(180, 255);
        int B = Random.Range(180, 255); int A = Random.Range(180, 255);
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    public void DestroyGrass() {
        for (int i = 0; i <= grassMaxCount - 1; i++) {
            Destroy(aGrass[i]);
        }
        Destroy(allTheGrass);
        grassMade = false;
    }

    public void DisableGrass() {
        if (!grassMade) { return; }
        grassPlaced = false;
        for (int i = 0; i <= grassMaxCount - 1; i++) {
            aGrass[i].GetComponent<Renderer>().enabled = false;
        }
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            grassCluster[i].display = false;
        }
    }

    public void PlaceAndEnableGrass() {
        if (!grassMade) { return; }
        if (grassPlaced) { return; }
        RaycastHit hit;
        int displayedGrassCount = 0;
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (!grassCluster[i].display) { continue; }
            if (displayedGrassCount >= grassMaxCount) { break; }
            for (int i2 = 0; i2 <= grassClusterSize - 1; i2++) {
                Vector2 offset = new Vector2(grassCluster[i].offSetAndType[i2].x, grassCluster[i].offSetAndType[i2].z);
                Vector2 centerLocation = grassCluster[i].centerLocation;
                Vector3 dropPoint = new Vector3(centerLocation.x + offset.x, 25000, centerLocation.y + offset.y);
                if (Physics.Raycast(dropPoint, Vector3.down, out hit, 30000)) {
                    aGrass[displayedGrassCount].transform.position = hit.point;
                    aGrass[displayedGrassCount].GetComponent<Renderer>().enabled = true;
                    displayedGrassCount += 1;
                }
            }
        }
        for (int i = displayedGrassCount; i <= grassMaxCount - 1; i++) {
            aGrass[i].GetComponent<Renderer>().enabled = false;
        }
        grassPlaced = true;
        Debug.Log("Displaed Grass Count: " + displayedGrassCount);
    }   
}