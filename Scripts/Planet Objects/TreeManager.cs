using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Wasabimole.ProceduralTree;

public class TreeManager : MonoBehaviour {
    private int seed;
    private int wholePlanetVertCount;
    private string curPlanetType;

    private bool treesPlaced;
    private bool treesMade;

    public const float drawDistance = 130;
    private const int treeMaxCount = 1200;
    private const int treeTypes = 3;
    private const int possibleTreeTypes = 12;
    private int treeClusterSize = 15;
    private float treeScatterArea = 20;
    private float sizeMultiplier = 1.0f;
    private GameObject cameraRig;

    private GameObject allTheTrees;
    private ProceduralTree[,] aTreeParent = new ProceduralTree[treeTypes, 6];
    private ProceduralTree[,] aTree = new ProceduralTree[treeTypes, (treeMaxCount / treeTypes)];

    private int[] displayedTreesCount = new int[treeTypes];
    private Material[] treeMaterial = new Material[treeTypes];
    private Vector2[] treeElevations;
    private Vector2[] treeSlopes;

    private float planetRadius;
    private float planetMaxElevation;
    private float planetMinElevation;
    private float waterLine;
    private float underWater;
    private bool globalTreeLocations = false;
    private int treesPlacedDelay = 0;

    public struct TreeCluster {
        public int count;
        public Vector2 centerLocation;
        public Vector2[] offset;
        public int[] type;
        public bool[] checkedForDisplay;
        public bool[] doNotDisplay;
        public bool haveTrees;
        public bool display;
    }

    public TreeCluster[] treeCluster;

    // an array of parameters that will define the fundamental "types" of trees.
    private struct TreeParameters {
        public int[] numVerts;
        public int[] numSides;
        public float[] baseRadius;
        public float[] branchProbability;
        public float[] branchRoundness;
        public float[] minRadius;
        public float[] radiusStep;
        public float[] segmentLength;
        public float[] twisting;
    }
    private TreeParameters[] tp;

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
        allTheTrees = new GameObject("allTheTrees");
        treeCluster = new TreeCluster[wholePlanetVertCount];
        tp = new TreeParameters[possibleTreeTypes];
        treeElevations = GameObject.Find("aPlanet").GetComponent<MeshFilter>().mesh.uv4;
        treeSlopes = GameObject.Find("aPlanet").GetComponent<MeshFilter>().mesh.uv3;
        planetRadius = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().diameter / 2F;
        planetMaxElevation = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().maxHeight;
        planetMinElevation = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().minHeight;
        waterLine = (planetRadius - planetMinElevation) / (planetMaxElevation - planetMinElevation);
        underWater = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().diameter / 2F + 750f;
        curPlanetType = (GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetType).Replace("Planet", "");
        // will help set the number of visible trees, scaled for planet diameter. Between .75 and 1.75.
        sizeMultiplier = .6f + (planetRadius - (PlanetManager.minDiameter / 2)) / ((PlanetManager.maxDiameter / 2) - (PlanetManager.minDiameter / 2));
        treeClusterSize = (int)(sizeMultiplier * treeClusterSize);
        treeScatterArea *= sizeMultiplier;
        SetTreeTypes();
    }

    public void AddTrees() {
        if (treesMade) { return; }
        for (int i = 0; i <= treeTypes - 1; i++) {
            treeMaterial[i] = GetMaterial(i + 1);
        }

        int treeSeed = UnityEngine.Random.Range(5000, 50000);
        int treeToClone;

        for (int i = 0; i <= treeTypes - 1; i++) {
            int treeType = UnityEngine.Random.Range(0, possibleTreeTypes);
            //make 6 trees for each type that are unique.
            for (int i2 = 0; i2 <= 5; i2++) {
                aTreeParent[i, i2] = new GameObject("aTreeParent_" + i + "_" + i2).AddComponent<ProceduralTree>();
                aTreeParent[i, i2].Seed = treeSeed - ((i + 1) * (i2 + 1));
                SetTreeProperties(ref aTreeParent[i, i2], treeSeed - ((i + 1)), treeType);
                aTreeParent[i, i2].Update();
                aTreeParent[i, i2].gameObject.GetComponent<MeshRenderer>().material = treeMaterial[i];
                aTreeParent[i, i2].gameObject.AddComponent<MeshCollider>();
                aTreeParent[i, i2].gameObject.GetComponent<MeshCollider>().enabled = true;
                aTreeParent[i, i2].gameObject.GetComponent<Renderer>().enabled = false;
                aTreeParent[i, i2].gameObject.transform.parent = allTheTrees.transform;
                aTreeParent[i, i2].gameObject.layer = 9;
            }
        }
        // the remaining trees for each type will be copies of the original 6 for each type.
        for (int i = 0; i <= treeTypes - 1; i++) {
            treeToClone = 0;
            for (int i2 = 0; i2 <= (treeMaxCount / treeTypes) - 1; i2++) {
                aTree[i, i2] = Instantiate(aTreeParent[i, treeToClone]);
                aTree[i, i2].gameObject.name = ("aTree_" + i + "_" + i2);
                aTree[i, i2].gameObject.transform.parent = allTheTrees.transform;
                treeToClone += 1;
                if (treeToClone == 5) { treeToClone = 0; }
            }
        }
        treesMade = true;
        DisableTrees();
    }

    private void SetTreeProperties(ref ProceduralTree curTree, int seed, int type) {
        UnityEngine.Random.InitState(seed);
        curTree.MaxNumVertices = UnityEngine.Random.Range(tp[type].numVerts[0], tp[type].numVerts[1]);
        curTree.NumberOfSides = UnityEngine.Random.Range(tp[type].numSides[0], tp[type].numSides[1]);
        curTree.BaseRadius = UnityEngine.Random.Range(tp[type].baseRadius[0], tp[type].baseRadius[1]);
        curTree.BranchProbability = UnityEngine.Random.Range(tp[type].branchProbability[0], tp[type].branchProbability[1]);
        curTree.MinimumRadius = UnityEngine.Random.Range(tp[type].minRadius[0], tp[type].minRadius[1]);
        curTree.RadiusStep = UnityEngine.Random.Range(tp[type].radiusStep[0], tp[type].radiusStep[1]);
        curTree.BranchRoundness = UnityEngine.Random.Range(tp[type].branchRoundness[0], tp[type].branchRoundness[1]);
        curTree.SegmentLength = UnityEngine.Random.Range(tp[type].segmentLength[0], tp[type].segmentLength[1]);
        curTree.Twisting = UnityEngine.Random.Range(tp[type].twisting[0], tp[type].twisting[1]);
    }

    private void SetTreeTypes() {
        int type = 0;
        for (int i = 0; i <= tp.Length - 1; i++) {
            tp[i].numVerts = new int[2];
            tp[i].numSides = new int[2];
            tp[i].baseRadius = new float[2];
            tp[i].branchProbability = new float[2];
            tp[i].branchRoundness = new float[2];
            tp[i].minRadius = new float[2];
            tp[i].radiusStep = new float[2];
            tp[i].segmentLength = new float[2];
            tp[i].twisting = new float[2];
        }
        type = 0; // tall, skinny, lots of branches up top, straight.
        tp[type].numVerts[0] = 7000; tp[type].numVerts[1] = 10000;
        tp[type].numSides[0] = 3; tp[type].numSides[1] = 15;
        tp[type].baseRadius[0] = .25f; tp[type].baseRadius[1] = .80f;
        tp[type].branchProbability[0] = .25f; tp[type].branchProbability[1] = .25f;
        tp[type].radiusStep[0] = .94f; tp[type].radiusStep[1] = .95f;
        tp[type].minRadius[0] = .01f; tp[type].minRadius[1] = .015f;
        tp[type].branchRoundness[0] = 0f; tp[type].branchRoundness[1] = 1f;
        tp[type].segmentLength[0] = .3f; tp[type].segmentLength[1] = .5f;
        tp[type].twisting[0] = 0f; tp[type].twisting[1] = 3f;
        type += 1; // medium/short shrub like, many branches.
        tp[type].numVerts[0] = 7000; tp[type].numVerts[1] = 10000;
        tp[type].numSides[0] = 6; tp[type].numSides[1] = 12;
        tp[type].baseRadius[0] = .25f; tp[type].baseRadius[1] = .37f;
        tp[type].branchProbability[0] = .1f; tp[type].branchProbability[1] = .13f;
        tp[type].radiusStep[0] = .94f; tp[type].radiusStep[1] = .95f;
        tp[type].minRadius[0] = .02f; tp[type].minRadius[1] = .03f;
        tp[type].branchRoundness[0] = 0f; tp[type].branchRoundness[1] = 1f;
        tp[type].segmentLength[0] = .1f; tp[type].segmentLength[1] = .15f;
        tp[type].twisting[0] = 5f; tp[type].twisting[1] = 10f;
        type += 1; // medium, mostly straight, only 1 or 2 top branches.
        tp[type].numVerts[0] = 7000; tp[type].numVerts[1] = 10000;
        tp[type].numSides[0] = 8; tp[type].numSides[1] = 15;
        tp[type].baseRadius[0] = .5f; tp[type].baseRadius[1] = .7f;
        tp[type].branchProbability[0] = .003f; tp[type].branchProbability[1] = .01f;
        tp[type].radiusStep[0] = .92f; tp[type].radiusStep[1] = .935f;
        tp[type].minRadius[0] = .02f; tp[type].minRadius[1] = .03f;
        tp[type].branchRoundness[0] = .5f; tp[type].branchRoundness[1] = .6f;
        tp[type].segmentLength[0] = .2f; tp[type].segmentLength[1] = .28f;
        tp[type].twisting[0] = 7f; tp[type].twisting[1] = 12f;
        type += 1; // medium, normalish tree, like a hardwood.
        tp[type].numVerts[0] = 20000; tp[type].numVerts[1] = 30000;
        tp[type].numSides[0] = 4; tp[type].numSides[1] = 10;
        tp[type].baseRadius[0] = .52f; tp[type].baseRadius[1] = .78f;
        tp[type].branchProbability[0] = .13f; tp[type].branchProbability[1] = .16f;
        tp[type].radiusStep[0] = .94f; tp[type].radiusStep[1] = .95f;
        tp[type].minRadius[0] = .017f; tp[type].minRadius[1] = .027f;
        tp[type].branchRoundness[0] = .5f; tp[type].branchRoundness[1] = .6f;
        tp[type].segmentLength[0] = .17f; tp[type].segmentLength[1] = .25f;
        tp[type].twisting[0] = 0f; tp[type].twisting[1] = 1.5f;
        type += 1; // Thick, twisty, ugly tree with few large branches.
        tp[type].numVerts[0] = 7000; tp[type].numVerts[1] = 10000;
        tp[type].numSides[0] = 4; tp[type].numSides[1] = 10;
        tp[type].baseRadius[0] = 3.5f; tp[type].baseRadius[1] = 4.0f;
        tp[type].branchProbability[0] = .03f; tp[type].branchProbability[1] = .04f;
        tp[type].radiusStep[0] = .89f; tp[type].radiusStep[1] = .91f;
        tp[type].minRadius[0] = .15f; tp[type].minRadius[1] = .18f;
        tp[type].branchRoundness[0] = .5f; tp[type].branchRoundness[1] = .6f;
        tp[type].segmentLength[0] = .65f; tp[type].segmentLength[1] = .75f;
        tp[type].twisting[0] = 20f; tp[type].twisting[1] = 70f;
        type += 1; // Very Large, Tall, Lots Of Branchs High
        tp[type].numVerts[0] = 15000; tp[type].numVerts[1] = 20000;
        tp[type].numSides[0] = 3; tp[type].numSides[1] = 6;
        tp[type].baseRadius[0] = 3.95f; tp[type].baseRadius[1] = 4.0f;
        tp[type].branchProbability[0] = .24f; tp[type].branchProbability[1] = .25f;
        tp[type].radiusStep[0] = .945f; tp[type].radiusStep[1] = .95f;
        tp[type].minRadius[0] = .14f; tp[type].minRadius[1] = .15f;
        tp[type].branchRoundness[0] = .7f; tp[type].branchRoundness[1] = 1.0f;
        tp[type].segmentLength[0] = 1.99f; tp[type].segmentLength[1] = 2.0f;
        tp[type].twisting[0] = 12f; tp[type].twisting[1] = 15f;
        type += 1; // Completely Random
        tp[type].numVerts[0] = 5000; tp[type].numVerts[1] = 20000;
        tp[type].numSides[0] = 3; tp[type].numSides[1] = 32;
        tp[type].baseRadius[0] = .25f; tp[type].baseRadius[1] = 4.0f;
        tp[type].branchProbability[0] = 0; tp[type].branchProbability[1] = .25f;
        tp[type].radiusStep[0] = .75f; tp[type].radiusStep[1] = .95f;
        tp[type].minRadius[0] = .01f; tp[type].minRadius[1] = .2f;
        tp[type].branchRoundness[0] = 0f; tp[type].branchRoundness[1] = 1.0f;
        tp[type].segmentLength[0] = .1f; tp[type].segmentLength[1] = 2.0f;
        tp[type].twisting[0] = 0; tp[type].twisting[1] = 20f;
        // remaining are random.
        for (int i = type +1; i <= possibleTreeTypes - 1; i++) {
            tp[i].numVerts[0] = 5000; tp[i].numVerts[1] = 20000;
            tp[i].numSides[0] = 3; tp[i].numSides[1] = 32;
            tp[i].baseRadius[0] = .25f; tp[i].baseRadius[1] = 4.0f;
            tp[i].branchProbability[0] = 0; tp[i].branchProbability[1] = .25f;
            tp[i].radiusStep[0] = .75f; tp[i].radiusStep[1] = .95f;
            tp[i].minRadius[0] = .01f; tp[i].minRadius[1] = .2f;
            tp[i].branchRoundness[0] = 0f; tp[i].branchRoundness[1] = 1.0f;
            tp[i].segmentLength[0] = .1f; tp[i].segmentLength[1] = 2.0f;
            tp[i].twisting[0] = 0; tp[i].twisting[1] = 20f;
        }
    }


    public void DestroyTrees() {
        if (!treesMade) { return; }
        for (int i = 0; i <= treeTypes - 1; i++) {
            for (int i2 = 0; i2 <= (treeMaxCount / treeTypes) - 1; i2++) {
                Destroy(aTree[i, i2].gameObject);
                aTree[i, i2] = null;
            }
        }
        for (int i = 0; i <= treeTypes - 1; i++) {
            for (int i2 = 0; i2 <= 5; i2++) {
                Destroy(aTreeParent[i, i2]);
            }
        }
        Destroy(allTheTrees);
        treesPlacedDelay = 0;
        treesPlaced = false;
        treesMade = false;
    }

    public void DisableTrees() {
        if (!treesMade) { return; }
        treesPlacedDelay = 0;
        treesPlaced = false;
        for (int i = 0; i <= treeTypes - 1; i++) {
            for (int i2 = 0; i2 <= (treeMaxCount / treeTypes) - 1; i2++) {
                aTree[i, i2].GetComponent<Renderer>().enabled = false; ;
            }
        }
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            treeCluster[i].display = false;
        }
    }

    // on first teleport, position the trees around the entire planet.
    public void PositionTrees() {
        if (globalTreeLocations) { return; }
        // cycle around the entire planets parent verts and place trees based on elevation and waterline.
        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (treeSlopes[i].x > 0F) { continue; } // skip sloped terrain.
            if (treeElevations[i].y <= waterLine) {
                // underwater, no trees here. 
                continue;
            }
            else if ((treeElevations[i].y > waterLine) && (treeElevations[i].y <= waterLine + .08F)) {
                // sorta chance of trees.
                if (UnityEngine.Random.Range(0F, 1F) >= .6F) {
                    treeCluster[i].haveTrees = true;
                    treeCluster[i].type = new int[treeClusterSize];
                    treeCluster[i].checkedForDisplay = new bool[treeClusterSize];
                    treeCluster[i].doNotDisplay = new bool[treeClusterSize];
                    for (int i2 = 0; i2 <= treeClusterSize - 1; i2++) {
                        treeCluster[i].type[i2] = 1;
                    }
                }
            }
            else if ((treeElevations[i].y > waterLine + .08F) && (treeElevations[i].y <= waterLine + .35F)) {
                // bigger chance of trees.
                if (UnityEngine.Random.Range(0F, 1F) >= .4F) {
                    treeCluster[i].haveTrees = true;
                    treeCluster[i].type = new int[treeClusterSize];
                    treeCluster[i].checkedForDisplay = new bool[treeClusterSize];
                    treeCluster[i].doNotDisplay = new bool[treeClusterSize];
                    for (int i2 = 0; i2 <= treeClusterSize - 1; i2++) {
                        treeCluster[i].type[i2] = UnityEngine.Random.Range(1, 3);
                    }
                }
            }
            else {
                // small chance for tree
                if (UnityEngine.Random.Range(0F, 1F) >= .9F) {
                    treeCluster[i].haveTrees = true;
                    treeCluster[i].type = new int[treeClusterSize];
                    treeCluster[i].checkedForDisplay = new bool[treeClusterSize];
                    treeCluster[i].doNotDisplay = new bool[treeClusterSize];
                    for (int i2 = 0; i2 <= treeClusterSize - 1; i2++) {
                        treeCluster[i].type[i2] = UnityEngine.Random.Range(2, 4);
                    }
                }
            }
            if (treeCluster[i].haveTrees) {
                MakeTreeCluster(i);
            }
        }
        globalTreeLocations = true;
    }

    private void MakeTreeCluster(int i) {
        // create a tree cluster at the terrain vert index, i.
        treeCluster[i].offset = new Vector2[treeClusterSize];
        float scatterX = UnityEngine.Random.Range(treeScatterArea / 1.5f, treeScatterArea);
        float scatterZ = UnityEngine.Random.Range(treeScatterArea / 1.5f, treeScatterArea);
        for (int i2 = 0; i2 <= treeClusterSize - 1; i2++) {
            float phi = UnityEngine.Random.Range(0F, 2 * (float)Math.PI);
            float rand = UnityEngine.Random.Range(0F, 1F);
            float xPos = (float)(Math.Sqrt(rand) * Math.Cos(phi));
            float zPos = (float)(Math.Sqrt(rand) * Math.Sin(phi));
            xPos = xPos * scatterX;
            zPos = zPos * scatterZ;
            treeCluster[i].offset[i2] = new Vector2(xPos, zPos);
        }
    }

    private Material GetMaterial(int type) {
        UnityEngine.Random.InitState(seed + type);
        int treeTexture = UnityEngine.Random.Range(1, 21);
        string treeTexturePath = "SurfaceObjects/Trees/" + curPlanetType + "/" + treeTexture;
        string treeNormalPath = "SurfaceObjects/Trees/" + curPlanetType + "/" + "n" + treeTexture;
        Texture treesTexture = Resources.Load(treeTexturePath) as Texture;
        Texture treesNormal = Resources.Load(treeNormalPath) as Texture;
        Material aTreeMaterial = new Material(Shader.Find("Custom/SimpleTreeBump"));
        aTreeMaterial.SetTexture("_MainTex", treesTexture);
        aTreeMaterial.SetTexture("_BumpMap", treesNormal);
        aTreeMaterial.SetColor("_Color", GetColor());
        int treeTile = UnityEngine.Random.Range(1, 5);
        aTreeMaterial.SetTextureScale("_MainTex", new Vector2(treeTile, treeTile));
        aTreeMaterial.SetTextureScale("_BumpMap", new Vector2(treeTile, treeTile));
        aTreeMaterial.renderQueue = 2000;
        aTreeMaterial.enableInstancing = true;
        return aTreeMaterial;
    }

    private Color32 GetColor() {
        int R = UnityEngine.Random.Range(225, 255); int G = UnityEngine.Random.Range(225, 255);
        int B = UnityEngine.Random.Range(225, 255); int A = 255;
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    public void PlaceAndEnableTrees() {
        if ((!treesMade) || (treesPlaced)) { return; }
        // hack to deal with a race condition where the mesh collider isn't calculated before raycast tree placement.
        if (treesPlacedDelay < 2) { treesPlacedDelay += 1; return; }

        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Default"); // hit only terrain
        Vector2 offset;
        Vector3 dropPoint;
        float dropHeight = (planetRadius * 1.1F) + 750f;
        float dropDistance = (planetRadius * 1.5F) + 750f;

        for (int i = 0; i <= aTree.GetLength(0) - 1; i++) {
            for (int i2 = 0; i2 <= aTree.GetLength(1) - 1; i2++) {
                aTree[i, i2].GetComponent<Renderer>().enabled = false;
            }
            displayedTreesCount[i] = 0;
        }

        for (int i = 0; i <= wholePlanetVertCount - 1; i++) {
            if (!treeCluster[i].display) { continue; }
            UnityEngine.Random.InitState(i);

            for (int i2 = 0; i2 <= treeClusterSize - 1; i2++) {
                // find the current type, if we're out of potential tress to display, skip it.
                int curType = treeCluster[i].type[i2] - 1;
                if (treeCluster[i].doNotDisplay[i2]) { continue; }
                if (displayedTreesCount[curType] >= (int)(treeMaxCount / treeTypes) - 1) {
                    break;
                }

                int treeToDrop = UnityEngine.Random.Range(0, (int)(treeMaxCount / treeTypes));
                if (aTree[curType, treeToDrop].GetComponent<Renderer>().enabled) {
                    continue;
                }

                offset = new Vector2(treeCluster[i].offset[i2].x, treeCluster[i].offset[i2].y);
                dropPoint = new Vector3(treeCluster[i].centerLocation.x + offset.x, dropHeight, treeCluster[i].centerLocation.y + offset.y);

                if (Physics.Raycast(dropPoint, Vector3.down, out hit, dropDistance, layerMask)) {
                    if ((Vector3.Angle(hit.normal, Vector3.up) >= 20) && !(treeCluster[i].checkedForDisplay[i2])) { // no trees on steep slopes, mark it to not display in the future.
                        treeCluster[i].doNotDisplay[i2] = true;
                        treeCluster[i].checkedForDisplay[i2] = true;
                        continue;
                    }
                    if ((hit.point.y <= underWater) && !(treeCluster[i].checkedForDisplay[i2])) {
                        // no trees underwater, mark it to not display in the future.
                        treeCluster[i].doNotDisplay[i2] = true;
                        treeCluster[i].checkedForDisplay[i2] = true;
                        continue;
                    }
                    // drop the trees down a bit so they're in the ground.
                    aTree[curType, treeToDrop].transform.position = hit.point + new Vector3(0, -0.65f, 0f);
                    aTree[curType, treeToDrop].GetComponent<Renderer>().enabled = true;
                    aTree[curType, treeToDrop].GetComponent<Renderer>().sharedMaterial = treeMaterial[curType];
                    treeCluster[i].checkedForDisplay[i2] = true;
                    displayedTreesCount[curType] += 1;
                }
            }
        }
        treesPlaced = true;
    }

}
