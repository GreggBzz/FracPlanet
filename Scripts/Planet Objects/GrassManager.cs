using UnityEngine;
using System;

public class GrassManager : MonoBehaviour {
    private int seed;

    private bool grassMade = false;
    private bool grassEnabled = false;
    private int grassPlacedTimes = 0;

    // for ~100FPS on a 1060, shoot for < 3500 grass in the FOV using 10 materials (texture * variety).
    // because of dynamic batching, performance roughly scales with (grassMaterials * grassCount).
    public const float drawDistance = 100;
    private const float animateDistance = 15;
    private int wholePlanetVertCount;
    private const int grassTextures = 8; // unique textures;
    private const int grassTextureVariety = 5; // unique varieties per texture;
    private const int grassMaxCount = 25000; // total grass gameobjects, displayed or not.
    private const float grassScatterArea = 20F;
    private const int grassClusterSize = 800;


    private GameObject allTheGrass;
    private Grass[,] grassMesh = new Grass[grassTextures, (grassMaxCount / grassTextures)];
    private GameObject[,] aGrass = new GameObject[grassTextures, (grassMaxCount / grassTextures)];
    private int[] displayedGrassCount = new int[grassTextures];
    private Material[,] grassMaterial = new Material[grassTextures, grassTextureVariety];
    private Material[,] staticGrassMaterial = new Material[grassTextures, grassTextureVariety];
    private Vector2[] grassElevations;
    private float planetRadius;
    private float planetMaxElevation;
    private float planetMinElevation;
    private bool globalGrassLocations = false;

    private string curPlanetType = "";

    // a cluster of grass to place. 
    public struct GrassCluster {
        public Vector2 centerLocation;
        public Vector2[] offset;
        public int type;
        public bool haveGrass;
        public bool display;
    }
    public GrassCluster[] grassCluster;


    void Awake () {
        if (GameObject.Find("Controller (right)") != null) {
            seed = GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetSeed;
        }
        else {
            seed = 100;
        }

        UnityEngine.Random.InitState(seed);

        wholePlanetVertCount = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().newVertIndex;
        grassCluster = new GrassCluster[wholePlanetVertCount];
        // partent object to stuff all the little grass children into.
        allTheGrass = new GameObject("allTheGrass");
        // parameters to help with grass placement.
        grassElevations = GameObject.Find("aPlanet").GetComponent<MeshFilter>().mesh.uv4;
        planetRadius = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().diameter / 2F;
        planetMaxElevation = GameObject.Find("aPlanet").GetComponent<PlanetTexture>().maxElev;
        planetMinElevation = GameObject.Find("aPlanet").GetComponent<PlanetTexture>().minElev;
        curPlanetType = (GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetType).Replace("Planet", "");
    }

    public void AddGrass() {
        if (grassMade) { return; }
        // setup the materials.
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= grassTextureVariety - 1; i2++) {
                GetMaterials(i + 1, out grassMaterial[i, i2], out staticGrassMaterial[i, i2]);
            }
        }
        // make the grass meshes and gameobjects.
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= (grassMaxCount / grassTextures) - 1; i2++) {
                grassMesh[i, i2] = new Grass();
                grassMesh[i, i2].Generate();
                MakeAGrass(i, i2);
            }
        }
        grassMade = true;
        DisableGrass();
    }

    // on first teleport, position the grass around the entire planet.
    public void PositionGrass() {
        if (globalGrassLocations) { return; }
        float waterLine = (planetRadius - planetMinElevation) / (planetMaxElevation - planetMinElevation);
        // cycle around the entire planets parent verts and place grass based on elevation and waterline.
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (grassElevations[i].y <= waterLine - .015F) {
                // underwater, no grass here. 
                continue;
            }
            else if ((grassElevations[i].y > waterLine - .015F) && (grassElevations[i].y <= waterLine + .08F)) {
                // big chance of grass.
                if (UnityEngine.Random.Range(0F, 1F) >= .14F) {
                    grassCluster[i].haveGrass = true;
                    grassCluster[i].type = UnityEngine.Random.Range(1, 4);
                }
            }
            else if ((grassElevations[i].y > waterLine + .08F) && (grassElevations[i].y <= waterLine + .35F)) {
                // sorta chance of grass.
                if (UnityEngine.Random.Range(0F, 1F) >= .6F) {
                    grassCluster[i].haveGrass = true;
                    grassCluster[i].type = UnityEngine.Random.Range(5, 8);
                }
            }
            else {
                // small chance for grass
                if (UnityEngine.Random.Range(0F, 1F) >= .8F) {
                    grassCluster[i].haveGrass = true;
                    grassCluster[i].type = UnityEngine.Random.Range(8, 9);
                }
            }
            if (grassCluster[i].haveGrass) {
                MakeGrassCluster(i);
            }
        }
        globalGrassLocations = true;
    }

    private void MakeGrassCluster(int i) {
        // create a grass cluster at the vert index, i.
        grassCluster[i].offset = new Vector2[grassClusterSize];
        float scatterX = UnityEngine.Random.Range(grassScatterArea / 3, grassScatterArea);
        float scatterZ = UnityEngine.Random.Range(grassScatterArea / 3, grassScatterArea);
        for (int i2 = 0; i2 <= grassClusterSize - 1; i2++) {
            float phi = UnityEngine.Random.Range(0F, 2 * (float)Math.PI);
            float rand = UnityEngine.Random.Range(0F, 1F);
            float xPos = (float)(Math.Sqrt(rand) * Math.Cos(phi));
            float zPos = (float)(Math.Sqrt(rand) * Math.Sin(phi));
            xPos = xPos * scatterX;
            zPos = zPos * scatterZ;
            grassCluster[i].offset[i2] = new Vector2(xPos, zPos);
        }
    }

    private void GetMaterials(int type, out Material aGrassMaterial, out Material aStaticGrassMaterial) {
        Texture grassTexture = Resources.Load("SurfaceObjects/Grass/" + curPlanetType + "/" + curPlanetType.ToLower() + "Grass" + type) as Texture;
        aGrassMaterial = new Material(Shader.Find("Custom/SimpleGrassSine"));
        aGrassMaterial.SetTexture("_MainTex", grassTexture);
        aGrassMaterial.SetTexture("_Illum", grassTexture);
        aGrassMaterial.SetColor("_Color", GetColor());
        aGrassMaterial.renderQueue = 1000;
        aGrassMaterial.SetFloat("_XStrength", UnityEngine.Random.Range(.01F, .1F));
        aGrassMaterial.SetFloat("_XDisp", UnityEngine.Random.Range(-.7F, .7F));
        aGrassMaterial.SetFloat("_WindFrequency", UnityEngine.Random.Range(.09F, .25F));
        // set the corrosponding static material.
        aStaticGrassMaterial = new Material(Shader.Find("Custom/SimpleGrassStatic"));
        aStaticGrassMaterial.SetTexture("_MainTex", grassTexture);
        aStaticGrassMaterial.SetTexture("_Illum", grassTexture);
        aStaticGrassMaterial.SetColor("_Color", aGrassMaterial.GetColor("_Color"));
        aStaticGrassMaterial.renderQueue = 1000;
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
        grassMesh[textureIndex, countIndex].SetNormals(aGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.normals);
        aGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.normals = grassMesh[textureIndex, countIndex].GetNormals();
        aGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.RecalculateBounds();
        aGrass[textureIndex, countIndex].GetComponent<Renderer>().enabled = false;
        // put all the grass under the parent object in the inspector.
        aGrass[textureIndex, countIndex].transform.parent = allTheGrass.transform;
    }

    private Color32 GetColor() {
        int R = UnityEngine.Random.Range(180, 255); int G = UnityEngine.Random.Range(180, 255);
        int B = UnityEngine.Random.Range(180, 255); int A = UnityEngine.Random.Range(180, 255);
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    public void DestroyGrass() {
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= (grassMaxCount / grassTextures) - 1; i2++) {
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
            for (int i2 = 0; i2 <= (grassMaxCount / grassTextures) - 1; i2++) {
                aGrass[i, i2].GetComponent<Renderer>().enabled = false; ;
            }
        }
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            grassCluster[i].display = false;
        }
    }
    
    public void PlaceAndEnableGrass() {
        if (!grassMade) { return; }
        // hack to deal with a race condition where the mesh collider isn't calculated before raycast grass placement.
        if (grassPlacedTimes > 3) { return; }

        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= (grassMaxCount / grassTextures) - 1; i2++) {
                aGrass[i, i2].GetComponent<Renderer>().enabled = false;
            }
            displayedGrassCount[i] = 0;
        }

        Vector3 cameraPos = GameObject.Find("[CameraRig]").transform.position;

        RaycastHit hit;
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (!grassCluster[i].display) { continue; }
            UnityEngine.Random.InitState(i);
            int curType = grassCluster[i].type - 1;

            for (int i2 = 0; i2 <= grassClusterSize - 1; i2++) {
                if (displayedGrassCount[curType] >= (int)(grassMaxCount / grassTextures) - 1) {
                    break;
                }

                int grassToDrop = UnityEngine.Random.Range(0, (int)(grassMaxCount / grassTextures));
                if (aGrass[curType, grassToDrop].GetComponent<Renderer>().enabled) { continue; }

                Vector2 offset = new Vector2(grassCluster[i].offset[i2].x, grassCluster[i].offset[i2].y);
                Vector3 dropPoint = new Vector3(grassCluster[i].centerLocation.x + offset.x, 25000, grassCluster[i].centerLocation.y + offset.y);

                if (Physics.Raycast(dropPoint, Vector3.down, out hit, 30000)) {
                    aGrass[curType, grassToDrop].transform.position = hit.point;
                    aGrass[curType, grassToDrop].GetComponent<Renderer>().enabled = true;
                    if (Vector3.Distance(hit.point, cameraPos) >= 15) {
                        aGrass[curType, grassToDrop].GetComponent<Renderer>().material = staticGrassMaterial[curType, Math.Abs((int)offset.x) % grassTextureVariety];
                    } else {
                        aGrass[curType, grassToDrop].GetComponent<Renderer>().material = grassMaterial[curType, Math.Abs((int)offset.x) % grassTextureVariety];
                    }
                    displayedGrassCount[curType] += 1;
                }
            }
        }
        grassPlacedTimes += 1;
    }
}