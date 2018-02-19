using System;
using System.Linq;
using UnityEngine;

public class PlanetTerrainDetail : MonoBehaviour {
    // keep track of how many new vertices we've added during tesselate.
    // and the parent vertices, the center of each hexagon.
    private int vertCount;

    private int debugCount;

    private int[] triangles;
    private int[] tempTriangles;

    // scale variables.
    private double radius;
    private float pScale;

    // mesh tesselation setup is done in the geometry class.
    private PlanetGeometry meshGeometry;

    private Vector3[] vertices = new Vector3[40962];
    private Vector3[] tmpVerticies;

    public void Generate(int[] curTriangles, Vector3[] curVerts, float curDiameter) {

        meshGeometry = gameObject.AddComponent<PlanetGeometry>();

        int triCount = 0;
        int[] vertexRef = new int[40962];
        Vector3[] tmpVerts = new Vector3[40962];
        int[] vertSeeds = new int[40962]; // store the added verts indicies for
        // instantiating a rnd that can do consistent fractal displacement post transform and during an arbitrary order.
        int[] tmpTris = new int[245000];
        // caclulate the position of each verticie in world space, post transforms,
        // to ensure that we tesselate the verticies around the player.
        Transform tr = GameObject.Find("aPlanet").transform;

        for (int i = 0; i <= curTriangles.Length - 1; i += 3) {
            // if the verticie is located near the player, who is top dead center on the planet, add it.
            if (tr.TransformPoint(curVerts[curTriangles[i]]).y > (curDiameter / 2F - curDiameter / 50F) + 750F) { 
                // if the vertex hasn't been copied, mark it in the refrence array (vertxRef[oldVerti] = NewVerti)
                // and copy it. 
                if (vertexRef[curTriangles[i]] == 0) {
                    vertexRef[curTriangles[i]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i]];
                    // which old vertex indicie does this new vert corrospond to?
                    // use that for consistent fractal displacement later.
                    vertSeeds[vertCount] = curTriangles[i];
                    vertCount += 1;
                }
                if (vertexRef[curTriangles[i + 1]] == 0) {
                    vertexRef[curTriangles[i + 1]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i + 1]];
                    vertSeeds[vertCount] = curTriangles[i + 1];
                    vertCount += 1;
                }
                if (vertexRef[curTriangles[i + 2]] == 0) {
                    vertexRef[curTriangles[i + 2]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i + 2]];
                    vertSeeds[vertCount] = curTriangles[i + 2];
                    vertCount += 1;
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

        meshGeometry.newVertIndex = vertCount;
        meshGeometry.SetVerts(vertices);
        meshGeometry.SetTris(triangles);
        meshGeometry.SetVertSeeds(vertSeeds);
        meshGeometry.tessRounds = 1;
        meshGeometry.Generate("terrain", curDiameter, 100, false);

        triangles = meshGeometry.GetTriangles();
        vertices = meshGeometry.GetVerts();
        vertCount = meshGeometry.GetVertIndex();
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
