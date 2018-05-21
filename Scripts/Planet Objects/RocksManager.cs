using UnityEngine;
using System;

public class RocksManager : MonoBehaviour {
    private int seed;

    private bool rocksMade = false;

    // variables used to space out rock placement over many frames without using an IENUmerator (slow!).
    private int rocksPlacedDelay = 0;
    private bool rocksPlaced;

    // for ~100FPS on a 1060, shoot for < 3500 rocks in the FOV using 10 materials (texture * varietyF).
    // because of dynamic batching, performance roughly scales with (rocksMaterials * rocksCount).
    public const float drawDistance = 125;
    private int wholePlanetVertCount;
    private const int rocksTextures = 8; // unique textures;
    private const int rocksTextureVariety = 3; // unique texture + displacement varieties of rock;
    private const int rocksMaxCount = 5000; // total rocks gameobjects, displayed or not.
    private const float rocksScatterArea = 30F;
    private const int rocksClusterSize = 15;

    private GameObject allTheRocks;
    private Rocks[,] rocksMesh = new Rocks[rocksTextures, (rocksMaxCount / rocksTextures)];
    private GameObject[,] aRock = new GameObject[rocksTextures, (rocksMaxCount / rocksTextures)];
    private int[] displayedrocksCount = new int[rocksTextures];
    private Material[,] rocksMaterial = new Material[rocksTextures, rocksTextureVariety];
    private Vector2[] rocksElevations;
    private float planetRadius;
    private float planetMaxElevation;
    private float planetMinElevation;
    private bool globalrocksLocations = false;

    private string curPlanetType = "";

    // a cluster of rocks to place. 
    public struct RocksCluster {
        public Vector2 centerLocation;
        public Vector2[] offset;
        public int type;
        public bool haveRocks;
        public bool display;
    }
    public RocksCluster[] rocksCluster;


    void Awake() {
        if (GameObject.Find("Controller (right)") != null) {
            seed = GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetSeed;
        }
        else {
            seed = 100;
        }

        UnityEngine.Random.InitState(seed);

        wholePlanetVertCount = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().newVertIndex;
        rocksCluster = new RocksCluster[wholePlanetVertCount];
        // partent object to stuff all the little rocks children into.
        allTheRocks = new GameObject("allTheRocks");
        // parameters to help with rocks placement.
        rocksElevations = GameObject.Find("aPlanet").GetComponent<MeshFilter>().mesh.uv4;
        planetRadius = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().diameter / 2F;
        planetMaxElevation = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().maxHeight;
        planetMinElevation = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().minHeight;
        curPlanetType = (GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetType).Replace("Planet", "");
    }

    public void AddRocks(string curPlanetType = "Terra", float curDiameter = 2500) {
        if (rocksMade) { return; }
        // make the mother of all rocks as our starting primative.
        GameObject mamaRock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // Mark all the duplicate verts in mamaRock. 901 is a magic number that indicates
        // the number of verts + the number of duplicated vert collections in the sphere primative.
        int[] dupeVerts = new int[901];
        dupeVerts = MarkDupeVerts(mamaRock.GetComponent<MeshFilter>().mesh.vertices);
        // setup the materials.
        for (int i = 0; i <= rocksTextures - 1; i++) {
            for (int i2 = 0; i2 <= rocksTextureVariety - 1; i2++) {
                rocksMaterial[i, i2] = GetMaterial(i + 1);
            }
        }
        // make the rocks meshes and gameobjects.
        for (int i = 0; i <= rocksTextures - 1; i++) {
            for (int i2 = 0; i2 <= (rocksMaxCount / rocksTextures) - 1; i2++) {
                
                rocksMesh[i, i2] = new Rocks();
                // first half of rock types are are smooth(er), assuming close to hydrosphere.
                if (i <= (rocksTextures / 2)) {
                    rocksMesh[i, i2].Generate(mamaRock.GetComponent<MeshFilter>().mesh.vertices, mamaRock.GetComponent<MeshFilter>().mesh.triangles,
                                          mamaRock.GetComponent<MeshFilter>().mesh.uv, dupeVerts, 1.03F, .25F);
                } else {
                // second half of rock types are jaggy and bigger.
                    rocksMesh[i, i2].Generate(mamaRock.GetComponent<MeshFilter>().mesh.vertices, mamaRock.GetComponent<MeshFilter>().mesh.triangles,
                                          mamaRock.GetComponent<MeshFilter>().mesh.uv, dupeVerts, 1.35F, .6F);
                }

                MakeaRock(i, i2);
            }
        }
        rocksMade = true;
        Destroy(mamaRock);
        DisableRocks();
    }

    private int[] MarkDupeVerts(Vector3[] verts) {
        int[] dupeVerts = new int[901];
        int arraySize = 0;
        bool[] checkedVerts = new bool[verts.Length];
        for (int i = 0; i <= verts.Length - 1; i++) {
            if (checkedVerts[i]) { continue; }
            Vector3 tmpVert = verts[i];
            dupeVerts[arraySize] =  i; arraySize += 1;
            checkedVerts[i] = true;
            for (int i2 = 0; i2 <= verts.Length - 1; i2++) {
                if (checkedVerts[i2]) { continue; }
                if (tmpVert == verts[i2]) {
                    dupeVerts[arraySize] = i2; arraySize += 1;
                    checkedVerts[i2] = true;
                }
            }
            // mark the end of a duplicated vert collection in the array.
            dupeVerts[arraySize] = -99; arraySize += 1;
        }
        return dupeVerts;
    }


    // on first teleport, position the rocks around the entire planet.
    public void PositionRocks(string curPlanetType = "Terra", float curDiameter = 2500) {
        if (globalrocksLocations) { return; }
        float waterLine = (planetRadius - planetMinElevation) / (planetMaxElevation - planetMinElevation);
        // cycle around the entire planets parent verts and place rocks based on elevation and waterline.
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (rocksElevations[i].y <= waterLine - .015F) {
                // underwater, no rocks here. 
                continue;
            }
            else if ((rocksElevations[i].y > waterLine - .015F) && (rocksElevations[i].y <= waterLine + .08F)) {
                // big chance of rocks.
                if (UnityEngine.Random.Range(0F, 1F) >= .14F) {
                    rocksCluster[i].haveRocks = true;
                    rocksCluster[i].type = UnityEngine.Random.Range(1, 4);
                }
            }
            else if ((rocksElevations[i].y > waterLine + .08F) && (rocksElevations[i].y <= waterLine + .35F)) {
                // sorta chance of rocks.
                if (UnityEngine.Random.Range(0F, 1F) >= .6F) {
                    rocksCluster[i].haveRocks = true;
                    rocksCluster[i].type = UnityEngine.Random.Range(3, 9);
                }
            }
            else {
                // good chance for rocks, last 2 types.
                if (UnityEngine.Random.Range(0F, 1F) >= .8F) {
                    rocksCluster[i].haveRocks = true;
                    rocksCluster[i].type = UnityEngine.Random.Range(3, 9);
                }
            }
            if (rocksCluster[i].haveRocks) {
                MakeRocksCluster(i);
            }
        }
        globalrocksLocations = true;
    }

    private void MakeRocksCluster(int i) {
        rocksCluster[i].offset = new Vector2[rocksClusterSize];
        float scatterX = UnityEngine.Random.Range(rocksScatterArea / 3, rocksScatterArea);
        float scatterZ = UnityEngine.Random.Range(rocksScatterArea / 3, rocksScatterArea);
        for (int i2 = 0; i2 <= rocksClusterSize - 1; i2++) {
            float phi = UnityEngine.Random.Range(0F, 2 * (float)Math.PI);
            float rand = UnityEngine.Random.Range(0F, 1F);
            float xPos = (float)(Math.Sqrt(rand) * Math.Cos(phi));
            float zPos = (float)(Math.Sqrt(rand) * Math.Sin(phi));
            xPos = xPos * scatterX;
            zPos = zPos * scatterZ;
            rocksCluster[i].offset[i2] = new Vector2(xPos, zPos);
        }
    }

    private Material GetMaterial(int type) {
        string rockTexturePath = "SurfaceObjects/Rocks/" + curPlanetType + "/" + curPlanetType.ToLower() + "Rocks" + type;
        string rockNormalPath = "SurfaceObjects/Rocks/" + curPlanetType + "/nm" + curPlanetType + "Rocks" + type;
        Texture rocksTexture = Resources.Load(rockTexturePath) as Texture;
        Texture rocksNormal = Resources.Load(rockNormalPath) as Texture;
        Material arockMaterial = new Material(Shader.Find("Custom/SimpleRockBump"));
        arockMaterial.SetTexture("_MainTex", rocksTexture);
        arockMaterial.SetTexture("_BumpMap", rocksNormal);
        arockMaterial.SetColor("_Color", GetColor());
        arockMaterial.SetTextureScale("_MainTex", new Vector2(5F, 5F));
        arockMaterial.SetTextureScale("_BumpMap", new Vector2(5F, 5F));
        arockMaterial.renderQueue = 1000;
        return arockMaterial;
    }

    private void MakeaRock(int textureIndex, int countIndex) {
        aRock[textureIndex, countIndex] = new GameObject("aRock_" + textureIndex + "_" + countIndex);
        aRock[textureIndex, countIndex].AddComponent<MeshFilter>();
        aRock[textureIndex, countIndex].AddComponent<MeshRenderer>();
        aRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.vertices = rocksMesh[textureIndex, countIndex].GetVerts();
        aRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.triangles = rocksMesh[textureIndex, countIndex].GetTris();
        aRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.uv = rocksMesh[textureIndex, countIndex].GetUv();
        aRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.RecalculateNormals();
        aRock[textureIndex, countIndex].GetComponent<Renderer>().enabled = false;
        aRock[textureIndex, countIndex].AddComponent<MeshCollider>();
        aRock[textureIndex, countIndex].GetComponent<MeshCollider>().enabled = true;
        aRock[textureIndex, countIndex].layer = 8;
        // put all the rocks under the parent object in the inspector.
        aRock[textureIndex, countIndex].transform.parent = allTheRocks.transform;
    }

    private Color32 GetColor() {
        int R = UnityEngine.Random.Range(230, 255); int G = UnityEngine.Random.Range(230, 255);
        int B = UnityEngine.Random.Range(230, 255); int A = UnityEngine.Random.Range(230, 255);
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    public void DestroyRocks() {
        for (int i = 0; i <= rocksTextures - 1; i++) {
            for (int i2 = 0; i2 <= (rocksMaxCount / rocksTextures) - 1; i2++) {
                Destroy(aRock[i, i2]);
                rocksMesh[i, i2] = null;
            }
        }
        Destroy(allTheRocks);
        rocksPlacedDelay = 0;
        rocksPlaced = false;
        rocksMade = false;
    }

    public void DisableRocks() {
        if (!rocksMade) { return; }
        rocksPlacedDelay = 0;
        rocksPlaced = false;
        for (int i = 0; i <= rocksTextures - 1; i++) {
            for (int i2 = 0; i2 <= (rocksMaxCount / rocksTextures) - 1; i2++) {
                aRock[i, i2].GetComponent<Renderer>().enabled = false; ;
            }
        }
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            rocksCluster[i].display = false;
        }
    }

    public void PlaceAndEnableRocks() {
        if ((!rocksMade) || (rocksPlaced)) { return; }
        // hack to deal with a race condition where the mesh collider isn't calculated before raycast rocks placement.
        if (rocksPlacedDelay < 2 ) { rocksPlacedDelay += 1; return; }

        for (int i = 0; i <= rocksTextures - 1; i++) {
            for (int i2 = 0; i2 <= (rocksMaxCount / rocksTextures) - 1; i2++) {
                aRock[i, i2].GetComponent<Renderer>().enabled = false;
            }
            displayedrocksCount[i] = 0;
        }

        Vector3 cameraPos = GameObject.Find("[CameraRig]").transform.position;
        RaycastHit hit;

        int layerMask = LayerMask.GetMask("Default"); // only hit the terrain.
        
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (!rocksCluster[i].display) { continue; }
            UnityEngine.Random.InitState(i);
            int curType = rocksCluster[i].type - 1;

            for (int i2 = 0; i2 <= rocksClusterSize - 1; i2++) {
                if (displayedrocksCount[curType] >= (int)(rocksMaxCount / rocksTextures)) {
                    break;
                }

                int rockToDrop = UnityEngine.Random.Range(0, (int)(rocksMaxCount / rocksTextures));
                
                Vector2 offset = new Vector2(rocksCluster[i].offset[i2].x, rocksCluster[i].offset[i2].y);
                Vector3 dropPoint = new Vector3(rocksCluster[i].centerLocation.x + offset.x, 25000, rocksCluster[i].centerLocation.y + offset.y);

                if (Physics.Raycast(dropPoint, Vector3.down, out hit, 30000, layerMask)) {
                    aRock[curType, rockToDrop].transform.position = hit.point;
                    aRock[curType, rockToDrop].GetComponent<Renderer>().enabled = true;
                    aRock[curType, rockToDrop].GetComponent<Renderer>().material = rocksMaterial[curType, Math.Abs((int)offset.x) % rocksTextureVariety];
                    displayedrocksCount[curType] += 1;
                }
            }
        }
        rocksPlaced = true;
    }
}