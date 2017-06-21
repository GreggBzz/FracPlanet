using System;
using System.IO;
using UnityEngine;

public class PlanetOceanDetail : MonoBehaviour {
    // keep track of how many new vertices we've added during tesselate.
    // and the parent vertices, the center of each hexagon.
    private ushort newVertIndex;
    private int vertCount;

    private int debugCount;

    private int[] triangles;
    private int[] tempTriangles;

    // scale variables.
    private double radius;
    private float pScale;

    private struct doneMidpoint {
        public int adjVert;
        public int midPoint;
    }

    // mesh geometry setup is done in another class.
    private PlanetGeometry meshGeometry;

    private doneMidpoint[,] doneMidpoints = new doneMidpoint[40962, 6];
    private Vector3[] vertices = new Vector3[40962];

    public void Generate(int[] curTriangles, Vector3[] curVerts, float curDiameter) {

        meshGeometry = gameObject.AddComponent<PlanetGeometry>();

        pScale = 100.0F / curDiameter;
        radius = 50.0F / pScale;

        int triCount = 0;
        int[] vertexRef = new int[65000];
        Vector3[] tmpVerts = new Vector3[2000];
        int[] tmpTris = new int[10000];

        for (int i = 0; i <= curTriangles.Length - 1; i += 3) {
            // mark the top 1/100th of the ocean to keep.
            if ((curVerts[curTriangles[i]].y) > (curDiameter / 2F - curDiameter / 25F)) {
                // if the vertext hasn't been copied, mark it in the refrence array (vertxRef[oldVerti] = NewVerti)
                // and copy it. 
                if (vertexRef[curTriangles[i]] == 0) {
                    vertexRef[curTriangles[i]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i]];
                    vertCount += 1;
                }
                if (vertexRef[curTriangles[i + 1]] == 0) {
                    vertexRef[curTriangles[i + 1]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i + 1]];
                    vertCount += 1;
                }
                if (vertexRef[curTriangles[i + 2]] == 0) {
                    vertexRef[curTriangles[i + 2]] = vertCount;
                    tmpVerts[vertCount] = curVerts[curTriangles[i + 2]];
                    vertCount += 1;
                }
                tmpTris[triCount] = vertexRef[curTriangles[i]];
                tmpTris[triCount + 1] = vertexRef[curTriangles[i + 1]];
                tmpTris[triCount + 2] = vertexRef[curTriangles[i + 2]];
                triCount += 3;
            }
        }
        triangles = new int[triCount];
        //vertices = new Vector3[vertCount];
        for (int i = 0; i <= triCount - 1; i++) {
            triangles[i] = tmpTris[i];
        }
        for (int i = 0; i <= vertCount - 1; i++) {
            vertices[i] = tmpVerts[i];
            newVertIndex = (ushort)vertCount;
        }

        tmpVerts = null; tmpTris = null; vertexRef = null;

        for (int i = 0; i <= 1; i++) {
            ResetMidpoints();
            tempTriangles = Tesselate(triangles);
            triangles = null;
            triangles = tempTriangles;
            tempTriangles = null;
        }
    }

    private int[] Tesselate(int[] curTriangles) {
        int[] newTriangles = new int[curTriangles.Length * 4];
        int newTriangleIndex = 0;
        // process a triangle for midpoints
        for (int i = 0; i <= curTriangles.Length - 1; i += 3) {
            int[] newMidpoints = new int[3];
            float[] dispalceMag = new float[3];
            int skip;
            // check our 2d refrence array to see if we've done this midpoint, if we have use the vert that's
            // refrenced, otherwise caclulate it. Also, calculate the displaceMag for furture refrence.
            skip = CheckMidpoint(curTriangles[i], curTriangles[i + 1]);
            dispalceMag[0] = Vector3.Distance(vertices[curTriangles[i]], vertices[curTriangles[i + 1]]);
            if (skip == -1) {
                vertices[newVertIndex] = CreateMidpoint(vertices[curTriangles[i]], vertices[curTriangles[i + 1]]);
                newMidpoints[0] = newVertIndex;
                MarkMidpointDone(curTriangles[i], curTriangles[i + 1], newVertIndex);
                newVertIndex += 1;
            }
            else { newMidpoints[0] = skip; }
            skip = CheckMidpoint(curTriangles[i + 1], curTriangles[i + 2]);
            dispalceMag[1] = Vector3.Distance(vertices[curTriangles[i + 1]], vertices[curTriangles[i + 2]]);
            if (skip == -1) {
                vertices[newVertIndex] = CreateMidpoint(vertices[curTriangles[i + 1]], vertices[curTriangles[i + 2]]);
                newMidpoints[1] = newVertIndex;
                MarkMidpointDone(curTriangles[i + 1], curTriangles[i + 2], newVertIndex);
                newVertIndex += 1;
            }
            else { newMidpoints[1] = skip; }
            skip = CheckMidpoint(curTriangles[i + 2], curTriangles[i]);
            dispalceMag[2] = Vector3.Distance(vertices[curTriangles[i + 2]], vertices[curTriangles[i]]);
            if (skip == -1) {
                vertices[newVertIndex] = CreateMidpoint(vertices[curTriangles[i + 2]], vertices[curTriangles[i]]);
                newMidpoints[2] = newVertIndex;
                MarkMidpointDone(curTriangles[i + 2], curTriangles[i], newVertIndex);
                newVertIndex += 1;
            }
            else { newMidpoints[2] = skip; }
            // displace the new midpoints
            for (int i2 = 0; i2 <= 2; i2++) {
                vertices[newMidpoints[i2]] = DisplaceMidpoint(vertices[newMidpoints[i2]], dispalceMag[i2]);
            }
            // build our new trianlges from the bisected vertices
            newTriangles[newTriangleIndex] = newMidpoints[0]; // 1
            newTriangles[newTriangleIndex + 1] = curTriangles[i + 1];
            newTriangles[newTriangleIndex + 2] = newMidpoints[1];
            newTriangles[newTriangleIndex + 3] = curTriangles[i]; // 2
            newTriangles[newTriangleIndex + 4] = newMidpoints[0];
            newTriangles[newTriangleIndex + 5] = newMidpoints[2];
            newTriangles[newTriangleIndex + 6] = newMidpoints[0]; // 3
            newTriangles[newTriangleIndex + 7] = newMidpoints[1];
            newTriangles[newTriangleIndex + 8] = newMidpoints[2];
            newTriangles[newTriangleIndex + 9] = newMidpoints[2]; // 4
            newTriangles[newTriangleIndex + 10] = newMidpoints[1];
            newTriangles[newTriangleIndex + 11] = curTriangles[i + 2];
            newTriangleIndex += 12;
        }
        return newTriangles;
    }

    private int CheckMidpoint(int v1, int v2) {
        // check to see if we've done this midpoint before.
        for (int i = 0; i <= 5; i++) {
            if (doneMidpoints[v1, i].adjVert == v2) {
                return doneMidpoints[v1, i].midPoint;
            }
        }
        for (int i = 0; i <= 5; i++) {
            if (doneMidpoints[v2, i].adjVert == v1) {
                return doneMidpoints[v2, i].midPoint;
            }
        }
        return -1;
    }

    private Vector3 DisplaceMidpoint(Vector3 p1, float displaceMag) {
        return p1 * 1.0F;
    }

    private Vector3 ExtendMidpoint(Vector3 p1, float rApprox) {
        // calculate the current length and extend to the radius, only.
        float currentLength = (float)Math.Sqrt((p1.x * p1.x) + (p1.y * p1.y) + (p1.z * p1.z));
        float lengthMult = (rApprox / currentLength);
        Vector3 extendedP1 = p1 * lengthMult;
        return extendedP1;
    }

    private Vector3 CreateMidpoint(Vector3 p1, Vector3 p2) {
        // create a midpoint.
        float rApprox;
        rApprox = (float)radius;
        Vector3 aMidpoint = new Vector3((p1.x + p2.x) / 2, (p1.y + p2.y) / 2, (p1.z + p2.z) / 2);
        return ExtendMidpoint(aMidpoint, rApprox);
    }


    private void MarkMidpointDone(int v1, int v2, int mp) {
        // mark a midpoint as done so we don't do it twice.
        for (int i = 0; i <= 5; i++) {
            if (doneMidpoints[v1, i].adjVert == -1) {
                doneMidpoints[v1, i].adjVert = v2;
                doneMidpoints[v1, i].midPoint = mp;
                break;
            }
        }
        for (int i = 0; i <= 5; i++) {
            if (doneMidpoints[v2, i].adjVert == -1) {
                doneMidpoints[v2, i].adjVert = v1;
                doneMidpoints[v2, i].midPoint = mp;
                return;
            }
        }
    }

    private void ResetMidpoints() {
        // reset the refrence array for each round of tesselation.
        for (int i = 0; i <= vertCount - 1; i++) {
            for (int i2 = 0; i2 <= 5; i2++) {
                doneMidpoints[i, i2].adjVert = -1;
            }
        }
    }

    public int[] GetTris() {
        return triangles;
    }

    public Vector3[] GetVerts() {
        Vector3[] tmpVerticies = new Vector3[newVertIndex];
        float maxY = 0F;
        // assign the verts to a properly sized array.
        // flatten the partial ocean.
        for (int i = 0; i <= newVertIndex - 1; i++) {
            if (vertices[i].y > maxY) { maxY = vertices[i].y; }
        }
        for (int i = 0; i <= newVertIndex - 1; i++) {
            tmpVerticies[i] = vertices[i];
            tmpVerticies[i].y = maxY;
        }
        vertices = null;
        return tmpVerticies;
    }

    private void debugoutput(float currentLength, float midPointLength) {
        if (debugCount == 30) return;
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Gregg\Desktop\FadeDebug.txt", true)) {
            file.WriteLine("currentLength: " + currentLength);
            file.WriteLine("midpointLength: " + midPointLength);
        }
        debugCount += 1;
    }
}
