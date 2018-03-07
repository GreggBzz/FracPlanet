using System;
using System.Linq;
using UnityEngine;

public class PlanetTerrainDetail : MonoBehaviour {
    // keep track of how many new vertices we've added during tesselate.
    // and the parent vertices, the center of each hexagon.
    private int vertCount;
    private const int maxVertCount = 40962;

    private int[] triangles;
    private int[] tempTriangles;

    // scale variables.
    private double radius;
    private float pScale;
    private Transform toTop;

    // mesh tesselation setup is done in the geometry class.
    private PlanetGeometry meshGeometry;

    // planet objects
    private GrassManager grassManager;
    private RocksManager rocksManager;
      
    // verts
    private Vector3[] vertices = new Vector3[maxVertCount];
    private Vector3[] tmpVerticies;

    void Awake() {
        grassManager = GameObject.Find("aPlanet").GetComponent<GrassManager>();
        toTop = GameObject.Find("aPlanet").transform;
    }

    public void Generate(int[] curTriangles, Vector3[] curVerts, float curDiameter) {
        // setup the tesselate script for later.
        meshGeometry = gameObject.AddComponent<PlanetGeometry>();
        // setup planet objects
        grassManager.AddGrass();
        grassManager.PositionGrass();
        grassManager.DisableGrass(); 
        // how many new triangles we're adding.
        int triCount = 0;
        // a refrence array of which old verts to copy.
        int[] vertexRef = new int[maxVertCount];
        // the temporary triangles array that will end up being the new mesh.
        Vector3[] tmpVerts = new Vector3[maxVertCount];
        // store the added verts indicies for instantiating a rnd that can do consistent fractal 
        // displacement post transform and during an arbitrary order.
        int[] vertSeeds = new int[maxVertCount];
        // the temporary triangle array that will end up being the new tris.
        int[] tmpTris = new int[245000];

        for (int i = 0; i <= curTriangles.Length - 1; i += 3) {
            // check the height of the current vert, only cutoff the topmost bit of the origin mesh, near the player at top dead center.
            float vertHeight = toTop.TransformPoint(curVerts[curTriangles[i]]).y;
            float cutoffHeight = (curDiameter / 2F - curDiameter / 50F) + 750F;
            if (vertHeight > cutoffHeight) {
                // if the vertex hasn't been copied, mark it in the refrence array: vertexRef[curTriangles[i]] = vertCount
                if (vertexRef[curTriangles[i]] == 0) {
                    vertexRef[curTriangles[i]] = vertCount;
                    // copy the vertex to the tmpVerts array, with which we'll build a new mesh.
                    tmpVerts[vertCount] = curVerts[curTriangles[i]];
                    // keep track of which old vert the copied one came from, so we can have consistend seeds to use.
                    vertSeeds[vertCount] = curTriangles[i];
                    vertCount += 1;
                    CheckForGrass(curTriangles[i], toTop.TransformPoint(curVerts[curTriangles[i]]), curDiameter);
                }
                if (vertexRef[curTriangles[i + 1]] == 0) {
                    vertexRef[curTriangles[i + 1]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i + 1]];
                    vertSeeds[vertCount] = curTriangles[i + 1];
                    vertCount += 1;
                    CheckForGrass(curTriangles[i], toTop.TransformPoint(curVerts[curTriangles[i + 1]]), curDiameter);
                }
                if (vertexRef[curTriangles[i + 2]] == 0) {
                    vertexRef[curTriangles[i + 2]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i + 2]];
                    vertSeeds[vertCount] = curTriangles[i + 2];
                    vertCount += 1;
                    CheckForGrass(curTriangles[i], toTop.TransformPoint(curVerts[curTriangles[i + 2]]), curDiameter);
                }
                tmpTris[triCount] = vertexRef[curTriangles[i]];
                tmpTris[triCount + 1] = vertexRef[curTriangles[i + 1]];
                tmpTris[triCount + 2] = vertexRef[curTriangles[i + 2]];
                triCount += 3;
            }
        }

        triangles = new int[triCount];

        for (int i = 0; i <= triCount - 1; i++) {
            triangles[i] = tmpTris[i];
        }
        for (int i = 0; i <= vertCount - 1; i++) {
            vertices[i] = tmpVerts[i];
        }

        // clean up
        tmpVerts = null; tmpTris = null; vertexRef = null;
        curVerts = null; curTriangles = null;

        // Tesselate the new mesh.
        meshGeometry.newVertIndex = vertCount;
        meshGeometry.SetVerts(vertices);
        meshGeometry.SetTris(triangles);
        meshGeometry.SetVertSeeds(vertSeeds);
        meshGeometry.tessRounds = 1;
        meshGeometry.Generate("terrain", curDiameter, 100, false);
        // Assign the tesselated mesh geometry to our mesh.
        triangles = meshGeometry.GetTriangles();
        vertices = meshGeometry.GetVerts();
        vertCount = meshGeometry.GetVertIndex();
    }

    private void CheckForGrass(int curVertIndex, Vector3 curVertPos, float curDiameter) {
        if (grassManager.grassCluster[curVertIndex].haveGrass) {
            if (Vector3.Distance(curVertPos, new Vector3(0, 750 + curDiameter / 2, 3500)) >= GrassManager.drawDistance) { return; }
            grassManager.grassCluster[curVertIndex].centerLocation.x = curVertPos.x;
            grassManager.grassCluster[curVertIndex].centerLocation.y = curVertPos.z;
            grassManager.grassCluster[curVertIndex].display = true;
        }
    }

    public int[] GetTris(bool reverse = false) {
        if (reverse) {
            InvertTriangles();
        }
        return triangles;
    }

    private void InvertTriangles() {
        triangles = triangles.Reverse().ToArray();
    }

    public Vector3[] GetVerts() {
        tmpVerticies = new Vector3[vertCount];
        // assign the verts to a properly sized array.
        for (int i = 0; i <= vertCount - 1; i++) {
            tmpVerticies[i] = vertices[i];
        }
        vertices = null;
        return tmpVerticies;
    }

    public int GetVertCount() {
        return vertCount;
    }
}
