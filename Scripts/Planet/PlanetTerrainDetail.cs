using System;
using System.Linq;
using UnityEngine;

public class PlanetTerrainDetail : MonoBehaviour {
    // planet objects
    private GrassManager grassManager;
    private RocksManager rocksManager;
    private string curPlanetType = "";

    // keep track of how many verts we've added to each section.    
    private int closeVertCount;
    private int farVertCount;   

    // verts and triangles.
    private Vector3[] closeVertices = new Vector3[maxVertCount];
    private Vector3[] farVertices = new Vector3[maxVertCount];
    private Vector3[] tmpVerticies; // for swapping.
    private int[] closeTriangles;
    private int[] farTriangles;

    // uv setup for far mesh.
    private Vector2[] farUv;
    private Vector2[] farUv3;
    private Vector2[] farUv4;

    // 40962 = number of edges after you tesselate a "d20" polyheadral 5 times.
    private const int maxVertCount = 40962;
        
    // to rotate to top dead center.
    private Transform toTop;

    // tesselation rounds for the close mesh.
    public int tessRounds = 1;

    // mesh tesselation setup is done in the geometry class.
    private PlanetGeometry meshGeometry;

    // LOD cutoffs
    public float minDistance = 0F;
    public float maxDistance = 95F;

    public void Generate(int[] curTriangles, Vector3[] curVerts, float curDiameter, Vector2[] curUv, Vector2[] curUv3, Vector2[] curUv4) {

        if (GameObject.Find("aPlanet")) {
            // disable the bigger less detailed mesh's collider. 
            GameObject.Find("aPlanet").GetComponent<MeshCollider>().enabled = false;
            // reset it's position before we tuck it under the player a tad.
            grassManager = GameObject.Find("aPlanet").GetComponent<GrassManager>();
            rocksManager = GameObject.Find("aPlanet").GetComponent<RocksManager>();
            toTop = GameObject.Find("aPlanet").transform;
            curPlanetType = (GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetType).Replace("Planet", "");
        }
  
        // setup the tesselate script for later.
        meshGeometry = gameObject.AddComponent<PlanetGeometry>();

        // setup planet objects
        if (curPlanetType == "Terra" || curPlanetType == "Icy") {
            grassManager.AddGrass();
            grassManager.PositionGrass();
            grassManager.DisableGrass();
        }
        rocksManager.AddRocks();
        rocksManager.PositionRocks();
        rocksManager.DisableRocks();

        // how many triangles and verts we're adding to the partial mesh.
        int closeTriCount = 0; closeVertCount = 0;
        int farTriCount = 0; farVertCount = 0;

        // a refrence array of which old verts to copy.
        int[] closeVertsRef = new int[maxVertCount];
        int[] farVertsRef = new int[maxVertCount];

        // the temporary triangles array that will end up being the new mesh.
        Vector3[] closeTmpVerts = new Vector3[maxVertCount];
        Vector3[] farTmpVerts = new Vector3[maxVertCount];

        // uv setup for far mesh
        Vector2[] farTmpUv = new Vector2[maxVertCount];
        Vector2[] farTmpUv3 = new Vector2[maxVertCount];
        Vector2[] farTmpUv4 = new Vector2[maxVertCount];

        // store the added close vert indexes for starting a rnd that will do consistent fractal 
        // displacement of the same vert, post tesselation and transform.
        int[] vertSeeds = new int[maxVertCount];

        // the temporary triangle array that will end up being the new tris.
        int[] closeTmpTris = new int[(int)(maxDistance * 20)];
        int[] farTmpTris = new int[260000];

        Vector3 playerPos = new Vector3(0F, 750F + curDiameter / 2F, 3500F); 

        for (int i = 0; i <= curTriangles.Length - 1; i += 3) {
            Vector3 vertPos = toTop.TransformPoint(curVerts[curTriangles[i]]);
            // check the distance of the current vert, save the close ones for more tesselation.
            if (checkLODDistance(vertPos, playerPos)) {
                // if the vertex hasn't been copied, mark it in the refrence array: vertexRef[curTriangles[i]] = vertCount
                if (closeVertsRef[curTriangles[i]] == 0) {
                    closeVertsRef[curTriangles[i]] = closeVertCount;
                    // copy the vertex to the tmpVerts array, with which we'll build a new mesh.
                    closeTmpVerts[closeVertCount] = curVerts[curTriangles[i]];
                    // keep track of which old vert the copied one came from, so we can have consistend seeds to use.
                    vertSeeds[closeVertCount] = curTriangles[i];
                    closeVertCount += 1;
                    CheckForGrass(curTriangles[i], toTop.TransformPoint(curVerts[curTriangles[i]]), curDiameter);
                    CheckForRocks(curTriangles[i], toTop.TransformPoint(curVerts[curTriangles[i]]), curDiameter);
                }
                if (closeVertsRef[curTriangles[i + 1]] == 0) {
                    closeVertsRef[curTriangles[i + 1]] = closeVertCount;
                    closeTmpVerts[closeVertCount] = curVerts[curTriangles[i + 1]];
                    vertSeeds[closeVertCount] = curTriangles[i + 1];
                    closeVertCount += 1;
                    CheckForGrass(curTriangles[i], toTop.TransformPoint(curVerts[curTriangles[i + 1]]), curDiameter);
                    CheckForRocks(curTriangles[i], toTop.TransformPoint(curVerts[curTriangles[i + 1]]), curDiameter);
                }
                if (closeVertsRef[curTriangles[i + 2]] == 0) {
                    closeVertsRef[curTriangles[i + 2]] = closeVertCount;
                    closeTmpVerts[closeVertCount] = curVerts[curTriangles[i + 2]];
                    vertSeeds[closeVertCount] = curTriangles[i + 2];
                    closeVertCount += 1;
                    CheckForGrass(curTriangles[i], toTop.TransformPoint(curVerts[curTriangles[i + 2]]), curDiameter);
                    CheckForRocks(curTriangles[i], toTop.TransformPoint(curVerts[curTriangles[i + 2]]), curDiameter);
                }
                closeTmpTris[closeTriCount] = closeVertsRef[curTriangles[i]];
                closeTmpTris[closeTriCount + 1] = closeVertsRef[curTriangles[i + 1]];
                closeTmpTris[closeTriCount + 2] = closeVertsRef[curTriangles[i + 2]];
                closeTriCount += 3;
            }
            else {
                // re-organize the far verts, and rebuild the triangles.
                if (farVertsRef[curTriangles[i]] == 0) {
                    farVertsRef[curTriangles[i]] = farVertCount;
                    farTmpVerts[farVertCount] = curVerts[curTriangles[i]];
                    farTmpUv[farVertCount] = curUv[curTriangles[i]];
                    farTmpUv3[farVertCount] = curUv3[curTriangles[i]];
                    farTmpUv4[farVertCount] = curUv4[curTriangles[i]];
                    farVertCount += 1;
                }
                if (farVertsRef[curTriangles[i + 1]] == 0) {
                    farVertsRef[curTriangles[i + 1]] = farVertCount;
                    farTmpVerts[farVertCount] = curVerts[curTriangles[i + 1]];
                    farTmpUv[farVertCount] = curUv[curTriangles[i + 1]];
                    farTmpUv3[farVertCount] = curUv3[curTriangles[i + 1]];
                    farTmpUv4[farVertCount] = curUv4[curTriangles[i + 1]];
                    farVertCount += 1;
                }
                if (farVertsRef[curTriangles[i + 2]] == 0) {
                    farVertsRef[curTriangles[i + 2]] = farVertCount;
                    farTmpVerts[farVertCount] = curVerts[curTriangles[i + 2]];
                    farTmpUv[farVertCount] = curUv[curTriangles[i + 2]];
                    farTmpUv3[farVertCount] = curUv3[curTriangles[i + 2]];
                    farTmpUv4[farVertCount] = curUv4[curTriangles[i + 2]];
                    farVertCount += 1;
                }
                farTmpTris[farTriCount] = farVertsRef[curTriangles[i]];
                farTmpTris[farTriCount + 1] = farVertsRef[curTriangles[i + 1]];
                farTmpTris[farTriCount + 2] = farVertsRef[curTriangles[i + 2]];
                farTriCount += 3;
            }
        }

        closeTriangles = new int[closeTriCount];
        farTriangles = new int[farTriCount];
        farUv = new Vector2[farVertCount];
        farUv3 = new Vector2[farVertCount];
        farUv4 = new Vector2[farVertCount];

        for (int i = 0; i <= closeTriCount - 1; i++) {
            closeTriangles[i] = closeTmpTris[i];
        }
        for (int i = 0; i <= closeVertCount - 1; i++) {
            closeVertices[i] = closeTmpVerts[i];
        }
        for (int i = 0; i <= farTriCount - 1; i++) {
            farTriangles[i] = farTmpTris[i];
        }
        for (int i = 0; i <= farVertCount - 1; i++) {
            farVertices[i] = farTmpVerts[i];
            farUv[i] = farTmpUv[i];
            farUv3[i] = farTmpUv3[i];
            farUv4[i] = farTmpUv4[i];
        }

        // clean up
        closeTmpVerts = null; closeTmpTris = null;
        farTmpVerts = null; farTmpTris = null;
        farTmpUv = null; farTmpUv3 = null; farTmpUv4 = null;

        // tesselate the top mesh
        meshGeometry.newVertIndex = closeVertCount;
        meshGeometry.SetVerts(closeVertices);
        meshGeometry.SetTris(closeTriangles);
        meshGeometry.SetVertSeeds(vertSeeds);
        meshGeometry.tessRounds = 1;
        meshGeometry.Generate("terrain", curDiameter, 100, false);
        // assign the tesselated mesh geometry to our top mesh values
        closeTriangles = meshGeometry.GetTriangles();
        closeVertices = meshGeometry.GetVerts();
        closeVertCount = meshGeometry.GetVertIndex();
    }

    private bool checkLODDistance(Vector3 vertPos, Vector3 playerPos) {
        if ((Vector3.Distance(vertPos, playerPos) > minDistance) && (Vector3.Distance(vertPos, playerPos) < maxDistance)) {
            return true;
        }
        return false;
    }

    private void CheckForGrass(int curVertIndex, Vector3 curVertPos, float curDiameter) {
        if (grassManager.grassCluster[curVertIndex].haveGrass) {
            if (Vector3.Distance(curVertPos, new Vector3(0, 750 + curDiameter / 2, 3500)) >= GrassManager.drawDistance) { return; }
            grassManager.grassCluster[curVertIndex].centerLocation.x = curVertPos.x;
            grassManager.grassCluster[curVertIndex].centerLocation.y = curVertPos.z;
            grassManager.grassCluster[curVertIndex].display = true;
        }
    }

    private void CheckForRocks(int curVertIndex, Vector3 curVertPos, float curDiameter) {
        if (rocksManager.rocksCluster[curVertIndex].haveRocks) {
            if (Vector3.Distance(curVertPos, new Vector3(0, 750 + curDiameter / 2, 3500)) >= RocksManager.drawDistance) { return; }
            rocksManager.rocksCluster[curVertIndex].centerLocation.x = curVertPos.x;
            rocksManager.rocksCluster[curVertIndex].centerLocation.y = curVertPos.z;
            rocksManager.rocksCluster[curVertIndex].display = true;
        }
    }

    public int[] GetTris(string closeOrFar = "close") {
        if (closeOrFar == "close") {
            return closeTriangles;
        }
        return farTriangles;
    }

    public Vector3[] GetVerts(string closeOrFar = "close") {
        // assign the verts to a properly sized array.
        if (closeOrFar == "close") {
            tmpVerticies = new Vector3[closeVertCount];
            for (int i = 0; i <= closeVertCount - 1; i++) {
                tmpVerticies[i] = closeVertices[i];
            }
        }
        else {
            tmpVerticies = new Vector3[farVertCount];
            for (int i = 0; i <= farVertCount - 1; i++) {
                tmpVerticies[i] = farVertices[i];
            }
        }      
        return tmpVerticies;
    }

    public int GetVertCount(string closeOrFar = "close") {
        if (closeOrFar == "close") {
            return closeVertCount;
        }
        return farVertCount;
    }

    public Vector2[] GetFarUv() {
        return farUv;
    }

    public Vector2[] GetFarUv3() {
        return farUv3;
    }

    public Vector2[] GetFarUv4() {
        return farUv4;
    }
}
