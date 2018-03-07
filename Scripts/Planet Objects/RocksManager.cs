using UnityEngine;
using System;

public class RocksManager : MonoBehaviour {
    private bool rocksMade = false;
    private bool rocksEnabled = false;
    private int rocksPlacedTimes = 0;

    // for ~100FPS on a 1060, shoot for < 3500 rocks in the FOV using 10 materials (texture * variety).
    // because of dynamic batching, performance roughly scales with (rocksMaterials * rocksCount).
    public const float drawDistance = 100;
    private int wholePlanetVertCount;
    private const int rocksTextures = 3; // unique textures;
    private const int rocksTextureVariety = 5; // unique varieties per texture;
    private const int rocksMaxCount = 20000; // total rocks gameobjects, displayed or not.
    private const float rocksScatterArea = 20F;
    private const int rocksClusterSize = 10;


    private GameObject allTherocks;
    private Rocks[,] rocksMesh = new Rocks[rocksTextures, (rocksMaxCount / rocksTextures)];
    private GameObject[,] aRock = new GameObject[rocksTextures, (rocksMaxCount / rocksTextures)];
    private int[] displayedrocksCount = new int[rocksTextures];
    private Material[,] rocksMaterial = new Material[rocksTextures, rocksTextureVariety];
    private Vector2[] rocksElevations;
    private float planetRadius;
    private float planetMaxElevation;
    private float planetMinElevation;
    private bool globalrocksLocations = false;

    // a cluster of rocks to place. 
    public struct RocksCluster {
        public Vector2 centerLocation;
        public Vector2[] offset;
        public int type;
        public bool haverocks;
        public bool display;
    }
    public RocksCluster[] rocksCluster;


    void Awake() {
        wholePlanetVertCount = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().newVertIndex;
        rocksCluster = new RocksCluster[wholePlanetVertCount];
        // partent object to stuff all the little rocks children into.
        allTherocks = new GameObject("allTherocks");
        // parameters to help with rocks placement.
        rocksElevations = GameObject.Find("aPlanet").GetComponent<MeshFilter>().mesh.uv4;
        planetRadius = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().diameter / 2F;
        planetMaxElevation = GameObject.Find("aPlanet").GetComponent<PlanetTexture>().maxElev;
        planetMinElevation = GameObject.Find("aPlanet").GetComponent<PlanetTexture>().minElev;
    }

    public void Addrocks() {
        if (rocksMade) { return; }
        // make the mother of all rocks as our starting primative.
        GameObject mamaRock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
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
                rocksMesh[i, i2].Generate(mamaRock.GetComponent<MeshFilter>().mesh.vertices, mamaRock.GetComponent<MeshFilter>().mesh.triangles);
                MakeaRock(i, i2);
            }
        }
        rocksMade = true;
        Disablerocks();
    }

    // on first teleport, position the rocks around the entire planet.
    public void Positionrocks() {
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
                    rocksCluster[i].haverocks = true;
                    rocksCluster[i].type = 1;
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
            }
            else if ((rocksElevations[i].y > waterLine + .08F) && (rocksElevations[i].y <= waterLine + .35F)) {
                // sorta chance of rocks.
                if (UnityEngine.Random.Range(0F, 1F) >= .6F) {
                    rocksCluster[i].haverocks = true;
                    rocksCluster[i].type = UnityEngine.Random.Range(2, 4);
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
            }
            else {
                // small chance for rocks, last 2 types.
                if (UnityEngine.Random.Range(0F, 1F) >= .8F) {
                    rocksCluster[i].haverocks = true;
                    rocksCluster[i].type = 4;
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
            }
        }
        globalrocksLocations = true;
    }

    private Material GetMaterial(int type) {
        Texture rocksTexture = Resources.Load("ObjectsTextures/icerocks" + type) as Texture;
        Material arockMaterial = new Material(Shader.Find("Custom/SimplerockStatic"));
        arockMaterial.SetTexture("_MainTex", rocksTexture);
        arockMaterial.SetTexture("_Illum", rocksTexture);
        arockMaterial.SetColor("_Color", GetColor());
        arockMaterial.renderQueue = 1000;
        return arockMaterial;
    }

    private void MakeaRock(int textureIndex, int countIndex) {
        aRock[textureIndex, countIndex] = new GameObject("aRock_" + textureIndex + "_" + countIndex);
        aRock[textureIndex, countIndex].AddComponent<MeshFilter>();
        aRock[textureIndex, countIndex].AddComponent<MeshRenderer>();
        aRock[textureIndex, countIndex].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        aRock[textureIndex, countIndex].GetComponent<Renderer>().receiveShadows = false;
        aRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.vertices = rocksMesh[textureIndex, countIndex].GetVerts();
        aRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.triangles = rocksMesh[textureIndex, countIndex].GetTris();
        aRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.uv = rocksMesh[textureIndex, countIndex].GetUv();
        aRock[textureIndex, countIndex].GetComponent<MeshFilter>().mesh.RecalculateBounds();
        aRock[textureIndex, countIndex].GetComponent<Renderer>().enabled = false;
        // put all the rocks under the parent object in the inspector.
        aRock[textureIndex, countIndex].transform.parent = allTherocks.transform;
    }

    private Color32 GetColor() {
        int R = UnityEngine.Random.Range(180, 255); int G = UnityEngine.Random.Range(180, 255);
        int B = UnityEngine.Random.Range(180, 255); int A = UnityEngine.Random.Range(180, 255);
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    public void Destroyrocks() {
        for (int i = 0; i <= rocksTextures - 1; i++) {
            for (int i2 = 0; i2 <= (rocksMaxCount / rocksTextures) - 1; i2++) {
                Destroy(aRock[i, i2]);
                rocksMesh[i, i2] = null;
            }
        }
        Destroy(allTherocks);
        rocksPlacedTimes = 0;
        rocksMade = false;
    }

    public void Disablerocks() {
        if (!rocksMade) { return; }
        rocksPlacedTimes = 0;
        for (int i = 0; i <= rocksTextures - 1; i++) {
            for (int i2 = 0; i2 <= (rocksMaxCount / rocksTextures) - 1; i2++) {
                aRock[i, i2].GetComponent<Renderer>().enabled = false; ;
            }
        }
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            rocksCluster[i].display = false;
        }
    }

    public void PlaceAndEnablerocks() {
        if (!rocksMade) { return; }
        // hack to deal with a race condition where the mesh collider isn't calculated before raycast rocks placement.
        if (rocksPlacedTimes >= 3) { return; }

        Vector3 cameraPos = GameObject.Find("[CameraRig]").transform.position;

        RaycastHit hit;
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (!rocksCluster[i].display) { continue; }

            int curType = rocksCluster[i].type - 1;

            for (int i2 = 0; i2 <= rocksClusterSize - 1; i2++) {
                Vector2 offset = new Vector2(rocksCluster[i].offset[i2].x, rocksCluster[i].offset[i2].y);
                Vector3 dropPoint = new Vector3(rocksCluster[i].centerLocation.x + offset.x, 25000, rocksCluster[i].centerLocation.y + offset.y);
                if (displayedrocksCount[curType] >= (rocksMaxCount / rocksTextures)) {
                    break;
                }
                if (Physics.Raycast(dropPoint, Vector3.down, out hit, 30000)) {
                    aRock[curType, displayedrocksCount[curType]].transform.position = hit.point;
                    aRock[curType, displayedrocksCount[curType]].GetComponent<Renderer>().enabled = true;
                    aRock[curType, displayedrocksCount[curType]].GetComponent<Renderer>().material = rocksMaterial[curType, Math.Abs((int)offset.x) % rocksTextureVariety];
                    displayedrocksCount[curType] += 1;
                }
            }

        }
        for (int i = 0; i <= rocksTextures - 1; i++) {
            for (int i2 = displayedrocksCount[i]; i2 <= (rocksMaxCount / rocksTextures) - 1; i2++) {
                aRock[i, i2].GetComponent<Renderer>().enabled = false;
            }
            displayedrocksCount[i] = 0;
        }
        rocksPlacedTimes += 1;
    }
}