using UnityEngine;
using System;

public class GrassManager : MonoBehaviour {
    private int seed;
    private bool grassMade = false;

    // variables used to space out grass placement over many frames without using an IENUmerator (slow!).
    private int grassPlacedDelay = 0;
    private bool grassPlaced;

    // for ~100FPS on a 1060, shoot for < 3500 grass in the FOV using 10 materials (texture * variety).
    // because of dynamic batching, performance roughly scales with (grassMaterials * grassCount).
    public const float drawDistance = 60;
    private const float animateDistance = 10;
    private int wholePlanetVertCount;
    public const int grassTextures = 8; // unique textures, we'll use a texture atlas with n grasses in a strip.
    private const int grassColorVariety = 8; // unique color varieties.
    private const int grassMaxCount = 35000; // total grass gameobjects, displayed or not.
    private float grassScatterArea = 10;
    private int grassClusterSize = 700;
    private float sizeMultiplier = 1.0f;
    private GameObject cameraRig;

    private GameObject allTheGrass;
    private Grass[,] grassMesh = new Grass[grassTextures, 6];
    private GameObject[,] aParentGrass = new GameObject[grassTextures, 6];
    private GameObject[,] aGrass = new GameObject[grassTextures, (grassMaxCount / grassTextures)];
    private bool[,] grassEnabled = new bool[grassTextures, (grassMaxCount / grassTextures)];

    private int[] displayedGrassCount = new int[grassTextures];
    private Material[,] grassMaterial = new Material[grassTextures, grassColorVariety];
    private Material[,] staticGrassMaterial = new Material[grassTextures, grassColorVariety];
    private Vector2[] grassElevations;
    private Vector2[] grassSlopes;
    private float planetRadius;
    private float planetMaxElevation;
    private float planetMinElevation;
    private bool globalGrassLocations = false;

    private string curPlanetType = "";

    // a cluster of grass to place. 
    public struct GrassCluster {
        public Vector2 centerLocation;
        public Vector2[] offset;
        public int[] type;
        public bool[] checkedForDisplay;
        public bool[] doNotDisplay;
        public int[] colorVariety;
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
        cameraRig = GameObject.Find("[CameraRig]");
        wholePlanetVertCount = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().newVertIndex;
        grassCluster = new GrassCluster[wholePlanetVertCount];
        // partent object to stuff all the little grass children into.
        allTheGrass = new GameObject("allTheGrass");
        // parameters to help with grass placement.
        grassElevations = GameObject.Find("aPlanet").GetComponent<MeshFilter>().mesh.uv4;
        grassSlopes = GameObject.Find("aPlanet").GetComponent<MeshFilter>().mesh.uv3;
        planetRadius = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().diameter / 2F;
        planetMaxElevation = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().maxHeight;
        planetMinElevation = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().minHeight;
        curPlanetType = (GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetType).Replace("Planet", "");
        sizeMultiplier = .6f + (planetRadius - (PlanetManager.minDiameter / 2)) / ((PlanetManager.maxDiameter / 2) - (PlanetManager.minDiameter / 2));
        grassClusterSize = (int)(sizeMultiplier * grassClusterSize);
        grassScatterArea *= sizeMultiplier;
    }

    public void AddGrass() {
        if (grassMade) { return; }
        int grassToClone;
        // setup the materials.
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= grassColorVariety - 1; i2++) {
                GetMaterials(i + 1, out grassMaterial[i, i2], out staticGrassMaterial[i, i2]);
            }
        }
        // make the grass meshes and gameobjects.
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= 5; i2++) {
                grassMesh[i, i2] = new Grass();
                grassMesh[i, i2].Generate(.5f, .8f, 1.0f, i);
                MakeAGrass(i, i2);
            }
        }

        for (int i = 0; i <= grassTextures - 1; i++) {
            grassToClone = 0;
            for (int i2 = 0; i2 <= (grassMaxCount / grassTextures) - 1; i2++) {
                aGrass[i, i2] = Instantiate(aParentGrass[i, grassToClone]);
                aGrass[i, i2].gameObject.name = ("aGrass_" + i + "_" + i2);
                aGrass[i, i2].gameObject.transform.parent = allTheGrass.transform;
                grassToClone += 1;
                if (grassToClone == 5) { grassToClone = 0; }
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
            if (grassSlopes[i].x > 0F) { continue; } // skip sloped terrain.
            if (grassElevations[i].y <= waterLine - .015F) {
                // underwater, no grass here. 
                continue;
            }
            else if ((grassElevations[i].y > waterLine - .005F) && (grassElevations[i].y <= waterLine + .08F)) {
                // big chance of grass.
                if (UnityEngine.Random.Range(0F, 1F) >= .10F) {
                    grassCluster[i].haveGrass = true;
                    grassCluster[i].type = new int[grassClusterSize];
                    grassCluster[i].checkedForDisplay = new bool[grassClusterSize];
                    grassCluster[i].doNotDisplay = new bool[grassClusterSize];
                    for (int i2 = 0; i2 <= grassClusterSize - 1; i2++ ) {
                        grassCluster[i].type[i2] = UnityEngine.Random.Range(1, 4);
                    }
                    
                }
            }
            else if ((grassElevations[i].y > waterLine + .08F) && (grassElevations[i].y <= waterLine + .35F)) {
                // sorta chance of grass.
                if (UnityEngine.Random.Range(0F, 1F) >= .4F) {
                    grassCluster[i].haveGrass = true;
                    grassCluster[i].type = new int[grassClusterSize];
                    grassCluster[i].checkedForDisplay = new bool[grassClusterSize];
                    grassCluster[i].doNotDisplay = new bool[grassClusterSize];
                    for (int i2 = 0; i2 <= grassClusterSize - 1; i2++) {
                        grassCluster[i].type[i2] = UnityEngine.Random.Range(5, 8);
                    }
                }
            }
            else {
                // small chance for grass
                if (UnityEngine.Random.Range(0F, 1F) >= .8F) {
                    grassCluster[i].haveGrass = true;
                    grassCluster[i].type = new int[grassClusterSize];
                    grassCluster[i].checkedForDisplay = new bool[grassClusterSize];
                    grassCluster[i].doNotDisplay = new bool[grassClusterSize];
                    for (int i2 = 0; i2 <= grassClusterSize - 1; i2++) {
                        grassCluster[i].type[i2] = UnityEngine.Random.Range(7, 9);
                    }
                }
            }
            if (grassCluster[i].haveGrass) {
                MakeGrassCluster(i);
            }
        }
        globalGrassLocations = true;
    }

    private void MakeGrassCluster(int i) {
        // create a grass cluster at the terrain vert index, i.
        grassCluster[i].offset = new Vector2[grassClusterSize];
        grassCluster[i].colorVariety = new int[grassClusterSize];
        float scatterX = UnityEngine.Random.Range(grassScatterArea / 1.5f, grassScatterArea);
        float scatterZ = UnityEngine.Random.Range(grassScatterArea / 1.5f, grassScatterArea);
        for (int i2 = 0; i2 <= grassClusterSize - 1; i2++) {
            float phi = UnityEngine.Random.Range(0F, 2 * (float)Math.PI);
            float rand = UnityEngine.Random.Range(0F, 1F);
            float xPos = (float)(Math.Sqrt(rand) * Math.Cos(phi));
            float zPos = (float)(Math.Sqrt(rand) * Math.Sin(phi));
            xPos = xPos * scatterX;
            zPos = zPos * scatterZ;
            grassCluster[i].offset[i2] = new Vector2(xPos, zPos);
            grassCluster[i].colorVariety[i2] = UnityEngine.Random.Range(1, grassColorVariety + 1);
        }
    }

    private void GetMaterials(int type, out Material aGrassMaterial, out Material aStaticGrassMaterial) {
        aGrassMaterial = new Material(Shader.Find("Custom/SimpleGrassSine"));
        UnityEngine.Random.InitState(System.Environment.TickCount);
        aGrassMaterial.SetFloat("_XStrength", UnityEngine.Random.Range(.01F, .1F));
        aGrassMaterial.SetFloat("_XDisp", UnityEngine.Random.Range(-.7F, .7F));
        aGrassMaterial.SetFloat("_WindFrequency", UnityEngine.Random.Range(.09F, .25F));
        Color32 grassColor = GetColor();
        UnityEngine.Random.InitState(seed + type);
        int grassTextureFolder = UnityEngine.Random.Range(1, 5);
        Texture grassTexture = Resources.Load("SurfaceObjects/Grass/" + curPlanetType + "/" + grassTextureFolder + "/" + grassTextureFolder + "_" + type) as Texture;
        aGrassMaterial.SetTexture("_MainTex", grassTexture);
        aGrassMaterial.SetTexture("_Illum", grassTexture);
        aGrassMaterial.SetColor("_Color", grassColor);
        aGrassMaterial.renderQueue = 2450;
        aGrassMaterial.enableInstancing = false;
        // set the corrosponding static material.
        aStaticGrassMaterial = new Material(Shader.Find("Custom/SimpleGrassStatic"));
        aStaticGrassMaterial.SetTexture("_MainTex", grassTexture);
        aStaticGrassMaterial.SetTexture("_Illum", grassTexture);
        aStaticGrassMaterial.SetColor("_Color", grassColor);
        aStaticGrassMaterial.renderQueue = 2450;
        aStaticGrassMaterial.enableInstancing = false;
    }

    private void MakeAGrass(int textureIndex, int countIndex) {
        aParentGrass[textureIndex, countIndex] = new GameObject("aParentGrass_" + textureIndex + "_" + countIndex);
        aParentGrass[textureIndex, countIndex].AddComponent<MeshFilter>();
        aParentGrass[textureIndex, countIndex].AddComponent<MeshRenderer>();
        aParentGrass[textureIndex, countIndex].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        aParentGrass[textureIndex, countIndex].GetComponent<Renderer>().receiveShadows = false;
        aParentGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.vertices = grassMesh[textureIndex, countIndex].GetVerts();
        aParentGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.triangles = grassMesh[textureIndex, countIndex].GetTris();
        aParentGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.uv = grassMesh[textureIndex, countIndex].GetUv();
        grassMesh[textureIndex, countIndex].SetNormals(aParentGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.normals);
        aParentGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.normals = grassMesh[textureIndex, countIndex].GetNormals();
        aParentGrass[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.RecalculateBounds();
        aParentGrass[textureIndex, countIndex].GetComponent<Renderer>().enabled = false;
        // put all the grass under the parent object in the inspector.
        aParentGrass[textureIndex, countIndex].transform.parent = allTheGrass.transform;
    }

    private Color32 GetColor() {
        int R = UnityEngine.Random.Range(220, 255); int G = UnityEngine.Random.Range(220, 255);
        int B = UnityEngine.Random.Range(220, 255); int A = 255;
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    public void DestroyGrass() {
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= 5; i2++) {
                Destroy(aGrass[i, i2]);
                grassMesh[i, i2] = null;
            }
        }
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= 5; i2++) {
                Destroy(aParentGrass[i, i2]);
            }
        }
        Destroy(allTheGrass);
        grassPlacedDelay = 0;
        grassPlaced = false;
        grassMade = false;
    }

    public void DisableGrass() {
        if (!grassMade) { return; }
        grassPlacedDelay = 0;
        grassPlaced = false;
        for (int i = 0; i <= grassTextures - 1; i++) {
            for (int i2 = 0; i2 <= (grassMaxCount / grassTextures) - 1; i2++) {
                aGrass[i, i2].GetComponent<Renderer>().enabled = false; 
            }
        }
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            grassCluster[i].display = false;
        }
    }
    
    public void PlaceAndEnableGrass() {
        if ((!grassMade) || (grassPlaced)) { return; }
        // hack to deal with a race condition where the mesh collider isn't calculated before raycast grass placement.
        if (grassPlacedDelay < 2 ) { grassPlacedDelay += 1; return; }

        Vector3 cameraPos =  cameraRig.transform.position;
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Default"); // hit only terrain.
        int colorVariety = 0;
        Vector2 offset;
        Vector3 dropPoint;
        float dropHeight = (planetRadius * 1.1F) + 750f;
        float dropDistance = (planetRadius * 1.5F) + 750f;


        for (int i = 0; i <= aGrass.GetLength(0) - 1; i++) {
            for (int i2 = 0; i2 <= aGrass.GetLength(1) - 1; i2++) {
                if (grassEnabled[i, i2]) {
                    aGrass[i, i2].GetComponent<Renderer>().enabled = false;
                    grassEnabled[i, i2] = false;
                }
            }
            displayedGrassCount[i] = 0;
        }

        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (!grassCluster[i].display) { continue; }
            UnityEngine.Random.InitState(i);

            for (int i2 = 0; i2 <= grassClusterSize - 1; i2++) {
                int curType = grassCluster[i].type[i2] - 1;

                if (grassCluster[i].doNotDisplay[i2]) { continue; }

                if (displayedGrassCount[curType] >= (int)(grassMaxCount / grassTextures) - 1) {
                    break;
                }

                int grassToDrop = UnityEngine.Random.Range(0, (int)(grassMaxCount / grassTextures));

                if (grassEnabled[curType, grassToDrop]) {
                    continue;
                }
                
                offset = new Vector2(grassCluster[i].offset[i2].x, grassCluster[i].offset[i2].y);
                dropPoint = new Vector3(grassCluster[i].centerLocation.x + offset.x, dropHeight, grassCluster[i].centerLocation.y + offset.y);
                colorVariety = grassCluster[i].colorVariety[i2];

                if (Physics.Raycast(dropPoint, Vector3.down, out hit, dropDistance, layerMask)) {
                    if ((Vector3.Angle(hit.normal, Vector3.up) >= 20) && !(grassCluster[i].checkedForDisplay[i2])) { // no grass on slopes.
                        grassCluster[i].doNotDisplay[i2] = true;
                        grassCluster[i].checkedForDisplay[i2] = true;
                        continue;
                    }
                    aGrass[curType, grassToDrop].transform.position = hit.point;
                    aGrass[curType, grassToDrop].GetComponent<Renderer>().enabled = true;
                    grassEnabled[curType, grassToDrop] = true;              
                    if (Vector3.Distance(hit.point, cameraPos) >= animateDistance) {
                        aGrass[curType, grassToDrop].GetComponent<Renderer>().sharedMaterial = staticGrassMaterial[curType, colorVariety - 1];
                    } else {
                        aGrass[curType, grassToDrop].GetComponent<Renderer>().sharedMaterial = grassMaterial[curType, colorVariety - 1];
                    }
                    displayedGrassCount[curType] += 1;
                }
            }
        }
        grassPlaced = true;
    }
}