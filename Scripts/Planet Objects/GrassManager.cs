using UnityEngine;
using System.Collections;

public class GrassManager : MonoBehaviour {
    private bool grassMade = false;
    private bool grassEnabled = false;
    private int grassPlacedTimes = 0;

    // For ~100FPS on a 1060, shoot for < 3500 grass in the FOV using 10 materials (texture * variety).
    // Because of dynamic batching, performance roughly scales with (grassMaterials * grassCount).
    public const int wholePlanetVertCount = 40962;
    public const int grassTextures = 4; // unique textures;
    public const int grassTextureVariety = 4; // unique varieties per texture;
    public const int grassMaxCount = 10000; // total grass gameobjects, displayed or not.
    public const float grassScatterArea = 4.5F;
    private const int grassClusterSize = 175;
    public const float drawDistance = 80;
    private int curTextureVariety = 0;

    private GameObject allTheGrass;
    private Grass[,] grassMesh = new Grass[grassTextures, (int)(grassMaxCount / grassTextures)];
    private GameObject[,] aGrass = new GameObject[grassTextures, (int)(grassMaxCount / grassTextures)];
    private int[] displayedGrassCount = new int[grassTextures];
    private Material[,] grassMaterial = new Material[grassTextures, grassTextureVariety];
    public Vector2[] grassPos = new Vector2[grassMaxCount];  

    // a cluster of grass to place. 
    public struct GrassCluster {
        public Vector2 centerLocation;
        public Vector2[] offset;
        public int type;
        public bool haveGrass;
        public bool display;
    }
    public GrassCluster[] grassCluster = new GrassCluster[wholePlanetVertCount];

    public bool globalGrassLocations = false;  


    void Awake () {
        // partent object to stuff all the little grass children into.
        allTheGrass = new GameObject("allTheGrass");
    }
    
    public void AddGrass() {
        if (grassMade) { return; }
        // Setup the materials.
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= grassTextureVariety - 1; i2++) {
                grassMaterial[i,i2] = GetMaterial(i + 1);
            }
        }
        // Make the grass meshes and gameobjects.
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= ((int)(grassMaxCount / grassTextures)) - 1; i2++) {
                grassMesh[i, i2] = new Grass();
                grassMesh[i, i2].Generate();
                MakeAGrass(i, i2);
            }
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
            grassCluster[aGrassSpotTemp].offset = new Vector2[grassClusterSize];
            grassCluster[aGrassSpotTemp].haveGrass = true;
            for (int i = 0; i <= grassClusterSize - 1; i++) {
                float xPos = Random.Range(-grassScatterArea, grassScatterArea);
                float zPos = Random.Range(-grassScatterArea, grassScatterArea);
                grassCluster[aGrassSpotTemp].offset[i] = new Vector2(xPos, zPos);
            }
            grassCluster[aGrassSpotTemp].type = Random.Range(1, grassTextures + 1);
            grassCount += 1;
        } while (grassCount < GlobalGrassCount);
        globalGrassLocations = true;
    }

    private Material GetMaterial(int type) {
        Material aGrassMaterial;
        Texture grassTexture = Resources.Load("BiomeTextures/Grass" + type) as Texture;
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

    private void MakeAGrass(int textureIndex, int countIndex) {
        aGrass[textureIndex, countIndex] = new GameObject("aGrass_" + textureIndex + "_" + countIndex);
        aGrass[textureIndex, countIndex].AddComponent<MeshFilter>();
        aGrass[textureIndex, countIndex].AddComponent<MeshRenderer>();
        aGrass[textureIndex, countIndex].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        aGrass[textureIndex, countIndex].GetComponent<Renderer>().receiveShadows = false;
        aGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.vertices = grassMesh[textureIndex, countIndex].GetVerts();
        aGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.triangles = grassMesh[textureIndex, countIndex].GetTris();
        aGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.uv = grassMesh[textureIndex, countIndex].GetUv();
        aGrass[textureIndex, countIndex].GetComponent<Renderer>().material = grassMaterial[textureIndex,curTextureVariety];
        curTextureVariety += 1; if (curTextureVariety >= grassTextureVariety) { curTextureVariety = 0; }
        grassMesh[textureIndex, countIndex].SetNormals(aGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.normals);
        aGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.normals = grassMesh[textureIndex, countIndex].GetNormals();
        aGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.RecalculateBounds();
        aGrass[textureIndex, countIndex].GetComponent<Renderer>().enabled = true;
        // put all the grass under the parent object in the inspector.
        aGrass[textureIndex, countIndex].transform.parent = allTheGrass.transform;
    }

    private Color32 GetColor() {
        int R = Random.Range(180, 255); int G = Random.Range(180, 255);
        int B = Random.Range(180, 255); int A = Random.Range(180, 255);
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    public void DestroyGrass() {
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= (int)(grassMaxCount / grassTextures) - 1; i2++) {
                Destroy(aGrass[i, i2]);
                grassMesh[i, i2] = null;
            }
        }
        Destroy(allTheGrass);
        grassPlacedTimes = 0;
        grassMade = false;
    }

    public void DisableGrass() {
        if (!grassMade) { return; }
        grassPlacedTimes = 0;
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= (int)(grassMaxCount / grassTextures) - 1; i2++) {
                aGrass[i, i2].GetComponent<Renderer>().enabled = false; ;
            }
        }
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            grassCluster[i].display = false;
        }
    }
    
    public void PlaceAndEnableGrass() {
        if (!grassMade) { return; }
        // hack to deal with the race condition where the mesh collider isn't set before grass placement.
        if (grassPlacedTimes >= 3) { return; } 
        RaycastHit hit;
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (!grassCluster[i].display) { continue; }

            int curType = grassCluster[i].type - 1;

            for (int i2 = 0; i2 <= grassClusterSize - 1; i2++) {
                Vector2 offset = new Vector2(grassCluster[i].offset[i2].x, grassCluster[i].offset[i2].y);
                Vector2 centerLocation = grassCluster[i].centerLocation;
                Vector3 dropPoint = new Vector3(centerLocation.x + offset.x, 25000, centerLocation.y + offset.y);
                if (displayedGrassCount[curType] >= (int)(grassMaxCount / grassTextures)) {
                    break;
                }
                if (Physics.Raycast(dropPoint, Vector3.down, out hit, 30000)) {
                   // try {
                        aGrass[curType, displayedGrassCount[curType]].transform.position = hit.point;
                        aGrass[curType, displayedGrassCount[curType]].GetComponent<Renderer>().enabled = true;
                        displayedGrassCount[curType] += 1;
                   // }
                   // catch (System.IndexOutOfRangeException e)
                   // {
                   //     Debug.Log(curType);
                   //     Debug.Log(displayedGrassCount[curType]);
                   //     Debug.Log(e);
                   // }
                }
            }
        }
        Debug.Log("Displayed Grass: " + displayedGrassCount[0] + "," + displayedGrassCount[1] + "," + displayedGrassCount[2] + "," + displayedGrassCount[3]);
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = displayedGrassCount[i]; i2 <= (int)(grassMaxCount / grassTextures) - 1; i2++) {
                aGrass[i, i2].GetComponent<Renderer>().enabled = false;
            }
            displayedGrassCount[i] = 0;
        }
        grassPlacedTimes += 1;
    }
}