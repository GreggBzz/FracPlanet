using UnityEngine;
using System;

public class RocksManager : MonoBehaviour {
    private int seed;
    // increase scatter area and cluster sizes proportial to the planet's diameter.
    private bool rocksMade = false;

    // variables used to space out rock placement over many frames without using an IENUmerator (slow!).
    private int rocksPlacedDelay = 0;
    private bool rocksPlaced;

    public const float drawDistance = 125;
    private int wholePlanetVertCount;
    private const int rocksTextures = 8; // unique textures;
    private const int rocksTextureVariety = 3; // unique texture + displacement varieties of rock;
    private const int rocksMaxCount = 9000; // total rocks gameobjects, displayed or not.
    private float rocksScatterArea;
    public float sizeMultiplier = 1.0f;
    private GameObject cameraRig;

    private GameObject allTheRocks;
    private Rocks[,] rocksMesh = new Rocks[rocksTextures, 6];
    private GameObject[,] aParentRock = new GameObject[rocksTextures, 6];
    private GameObject[,] aRock = new GameObject[rocksTextures, (rocksMaxCount / rocksTextures)];
    private bool[,] rocksEnabled = new bool[rocksTextures, (rocksMaxCount / rocksTextures)];
    private int[] displayedrocksCount = new int[rocksTextures];
    private Material[,] rocksMaterial = new Material[rocksTextures, rocksTextureVariety];
    private Vector2[] rocksElevations;
    private float planetRadius;
    private float planetMaxElevation;
    private float planetMinElevation;
    private bool globalRocksLocations = false;

    private string planetType = "";

    // a cluster of rocks to place. 
    public struct RocksCluster {
        public int count;
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
        cameraRig = GameObject.Find("[CameraRig]");
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
        planetType = (GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetType).Replace("Planet", "");
        // will help set the number of visible rocks, scaled for planet diameter. Between .5 and 1.5.
        sizeMultiplier = .6f + (planetRadius - (PlanetManager.minDiameter / 2)) / ((PlanetManager.maxDiameter / 2) - (PlanetManager.minDiameter / 2));
    }

    public void AddRocks() { // Create the rock gameobjects.
        if (rocksMade) { return; }
        // make the mother of all rocks as our starting primative.
        GameObject mamaRock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // Mark all the duplicate verts in mamaRock. 901 is a magic number that indicates
        // the number of verts + the number of duplicated vert collections in the sphere primative.
        int[] dupeVerts = new int[901];
        dupeVerts = MarkDupeVerts(mamaRock.GetComponent<MeshFilter>().mesh.vertices);
        int rockToClone;

        // setup the materials.
        for (int i = 0; i <= rocksTextures - 1; i++) {
            int textureFolder = UnityEngine.Random.Range(1, 9);
            for (int i2 = 0; i2 <= rocksTextureVariety - 1; i2++) {
                rocksMaterial[i, i2] = GetMaterial(textureFolder);
            }
        }
        // make the rocks meshes and gameobjects.
        for (int i = 0; i <= rocksTextures - 1; i++) {
            for (int i2 = 0; i2 <= 5; i2++) {
                rocksMesh[i, i2] = new Rocks();
                rocksMesh[i, i2].Generate(mamaRock.GetComponent<MeshFilter>().mesh.vertices, mamaRock.GetComponent<MeshFilter>().mesh.triangles,
                                          mamaRock.GetComponent<MeshFilter>().mesh.uv, dupeVerts, i + 1);
                MakeaRock(i, i2);
            }
        }

        for (int i = 0; i <= rocksTextures - 1; i++) {
            rockToClone = 0;
            for (int i2 = 0; i2 <= (rocksMaxCount / rocksTextures) - 1; i2++) {
                aRock[i, i2] = Instantiate(aParentRock[i, rockToClone]);
                aRock[i, i2].gameObject.name = ("aRock_" + i + "_" + i2);
                aRock[i, i2].gameObject.transform.parent = allTheRocks.transform;
                rockToClone += 1;
                if (rockToClone == 5) { rockToClone = 0; }
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
        if (globalRocksLocations) { return; }
        float waterLine = (planetRadius - planetMinElevation) / (planetMaxElevation - planetMinElevation);
        // cycle around the entire planets parent verts and place rocks based on elevation and waterline.
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (rocksElevations[i].y <= waterLine - .015F) {
                // underwater, no rocks here. 
                continue;
            }
            else if ((rocksElevations[i].y > waterLine - .015F) && (rocksElevations[i].y <= waterLine + .08F)) {
                if (UnityEngine.Random.Range(0F, 1F) >= .14F) {
                    rocksCluster[i].haveRocks = true;
                    rocksCluster[i].type = UnityEngine.Random.Range(1, 14);
                }
            }
            else if ((rocksElevations[i].y > waterLine + .08F) && (rocksElevations[i].y <= waterLine + .35F)) {
                if (UnityEngine.Random.Range(0F, 1F) >= .6F) {
                    rocksCluster[i].haveRocks = true;
                    rocksCluster[i].type = UnityEngine.Random.Range(1, 14);
                }
            }
            else {
                if (UnityEngine.Random.Range(0F, 1F) >= .8F) {
                    rocksCluster[i].haveRocks = true;
                    rocksCluster[i].type = UnityEngine.Random.Range(1, 14);
                }
            }
            if (rocksCluster[i].haveRocks) {
                MakeRocksCluster(i);
            }
        }
        globalRocksLocations = true;
    }

    private void MakeRocksCluster(int i) {
        if ((rocksCluster[i].type == 1) || (rocksCluster[i].type == 2)) { // boulders
            rocksCluster[i].count = (int)(8 * sizeMultiplier);
            rocksScatterArea = (15 * sizeMultiplier);
        }
        else if ((rocksCluster[i].type == 3) || (rocksCluster[i].type == 4)) { // large rocks
            rocksCluster[i].count = (int)(15 * sizeMultiplier);
            rocksScatterArea = (10 * sizeMultiplier);
        }
        else if ((rocksCluster[i].type == 5) || (rocksCluster[i].type == 6)) { // large gravel
            rocksCluster[i].count = (int)(50 * sizeMultiplier);
            rocksScatterArea = (10 * sizeMultiplier);
        }
        else if ((rocksCluster[i].type == 7) || (rocksCluster[i].type == 8)) { // small gravel
            rocksCluster[i].count = (int)(50 * sizeMultiplier);
            rocksScatterArea = (10 * sizeMultiplier);
        }

        rocksCluster[i].offset = new Vector2[rocksCluster[i].count];
        float scatterX = UnityEngine.Random.Range(rocksScatterArea / 3, rocksScatterArea);
        float scatterZ = UnityEngine.Random.Range(rocksScatterArea / 3, rocksScatterArea);
        for (int i2 = 0; i2 <= rocksCluster[i].count - 1; i2++) {
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
        UnityEngine.Random.InitState(seed + type);
        int rockTexture = UnityEngine.Random.Range(1, 5);
        string rockTexturePath = "SurfaceObjects/Rocks/" + type + "/" + type + "_" + rockTexture;
        string rockNormalPath = "SurfaceObjects/Rocks/" + type + "/n" + type + "_" + rockTexture;
        Texture rocksTexture = Resources.Load(rockTexturePath) as Texture;
        Texture rocksNormal = Resources.Load(rockNormalPath) as Texture;
        Material arockMaterial = new Material(Shader.Find("Custom/SimpleRockBump"));
        arockMaterial.SetTexture("_MainTex", rocksTexture);
        arockMaterial.SetTexture("_BumpMap", rocksNormal);
        arockMaterial.SetColor("_Color", GetColor());
        arockMaterial.SetTextureScale("_MainTex", new Vector2(5F, 5F));
        arockMaterial.SetTextureScale("_BumpMap", new Vector2(5F, 5F));
        arockMaterial.renderQueue = 2000;
        arockMaterial.enableInstancing = true;
        return arockMaterial;
    }

    private void MakeaRock(int textureIndex, int countIndex) {
        aParentRock[textureIndex, countIndex] = new GameObject("aParentRock_" + textureIndex + "_" + countIndex);
        aParentRock[textureIndex, countIndex].AddComponent<MeshFilter>();
        aParentRock[textureIndex, countIndex].AddComponent<MeshRenderer>();
        aParentRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.vertices = rocksMesh[textureIndex, countIndex].GetVerts();
        aParentRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.triangles = rocksMesh[textureIndex, countIndex].GetTris();
        aParentRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.uv = rocksMesh[textureIndex, countIndex].GetUv();
        aParentRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.RecalculateNormals();
        aParentRock[textureIndex, countIndex].GetComponent<Renderer>().enabled = false;
        aParentRock[textureIndex, countIndex].AddComponent<MeshCollider>();
        aParentRock[textureIndex, countIndex].GetComponent<MeshCollider>().enabled = true;
        aParentRock[textureIndex, countIndex].layer = 8;
        // put all the rocks under the parent object in the inspector.
        aParentRock[textureIndex, countIndex].transform.parent = allTheRocks.transform;
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
            }
        }
        for (int i = 0; i <= rocksTextures - 1; i++) {
            for (int i2 = 0; i2 <= 5; i2++) {
                Destroy(aParentRock[i, i2]);
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

        Vector3 cameraPos = cameraRig.transform.position;
        int layerMask = LayerMask.GetMask("Default"); // only hit the terrain, or water.
        RaycastHit hit;
        Vector2 offset;
        Vector3 dropPoint;
        float dropHeight = (planetRadius * 1.1F) + 750f;
        float dropDistance = (planetRadius * 1.5F) + 750f;

        for (int i = 0; i <= rocksTextures - 1; i++) {
            for (int i2 = 0; i2 <= (rocksMaxCount / rocksTextures) - 1; i2++) {
                if (rocksEnabled[i,i2]) {
                    aRock[i, i2].GetComponent<Renderer>().enabled = false;
                    rocksEnabled[i, i2] = false;
                }
            }
            displayedrocksCount[i] = 0;
        }

        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (!rocksCluster[i].display) { continue; }
            UnityEngine.Random.InitState(i);

            int curType = rocksCluster[i].type - 1;

            for (int i2 = 0; i2 <= rocksCluster[i].count - 1; i2++) {
                if (displayedrocksCount[curType] >= (int)(rocksMaxCount / rocksTextures)) {
                    break;
                }

                int rockToDrop = UnityEngine.Random.Range(0, (int)(rocksMaxCount / rocksTextures));

                if (rocksEnabled[curType, rockToDrop]) {
                    continue;
                }
                
                offset = new Vector2(rocksCluster[i].offset[i2].x, rocksCluster[i].offset[i2].y);
                dropPoint = new Vector3(rocksCluster[i].centerLocation.x + offset.x, dropHeight, rocksCluster[i].centerLocation.y + offset.y);

                if (Physics.Raycast(dropPoint, Vector3.down, out hit, dropDistance, layerMask)) {
                    aRock[curType, rockToDrop].transform.position = hit.point;
                    aRock[curType, rockToDrop].GetComponent<Renderer>().enabled = true;
                    rocksEnabled[curType, rockToDrop] = true;
                    aRock[curType, rockToDrop].GetComponent<Renderer>().sharedMaterial = rocksMaterial[curType, Math.Abs((int)offset.x) % rocksTextureVariety];
                    displayedrocksCount[curType] += 1;
                }
            }
        }
        rocksPlaced = true;
    }
}