using System;
using System.Collections.Generic;
using UnityEngine;
// class to manage cloud textures and movement.
public class PlanetCloud : MonoBehaviour {

    private Vector2[] tmpUv;
    private Vector3[] newVertices;
    public int[] newTriangles;
    public Vector2[] newUv;

    public Vector3[] TextureCloudMesh(Vector3[] vertices, int[] triangles) {
        // We modify the cloud mesh a bit to avoid the closed polyhedral texture "seam" problem.
        // - assign initial UV corrdiates which include the seam with spherical trig.
        // - cycle through all the triangles.
        // - find which triangles contain verts that have wrap around UV corrdinates.
        // - duplicate those verts.
        // - assign those duplicated verts a corrected UV coordiante. 
        // - remove the offending verts from their original triangles.
        // - assign the duplicates back to the offending triangles.
        // - this will "open" the polyheadral, which is OK with unity.
        float targetU;
        float targetV;
        float normalisedX;
        float normalisedZ;
        Vector3 normal;
        tmpUv = new Vector2[vertices.Length];
        // the typical polar corrdinates / spherical trig method to wrap a flat texture onto a sphere.
        for (int i = 0; i <= vertices.Length - 1; i++) {
            normalisedZ = -1;
            normalisedX = 0;
            normal = Vector3.Normalize(vertices[i]);
            if (((normal.x * normal.x) + (normal.z * normal.z)) > 0) {
                normalisedX = (float)Math.Sqrt((normal.x * normal.x) / ((normal.x * normal.x) + (normal.z * normal.z)));
                if (normal.x < 0) {
                    normalisedX = -normalisedX;
                }
                normalisedZ = (float)Math.Sqrt((normal.z * normal.z) / ((normal.x * normal.x) + (normal.z * normal.z)));
                if (normal.z < 0) {
                    normalisedZ = -normalisedZ;
                }
            }
            if (normalisedZ == 0) {
                targetU = (float)((normalisedX * Math.PI) / 2);
            }
            else {
                targetU = (float)Math.Atan(normalisedX / normalisedZ);
                if (normalisedZ < 0) {
                    targetU += (float)Math.PI;
                }
            }
            if (targetU < 0) {
                targetU += (float)(2 * Math.PI);
            }
            targetU /= (float)(2 * Math.PI);
            targetV = (-normal.y + 1) / 2;
            tmpUv[i] = new Vector2 { x = targetU, y = targetV };
        }

        float x1; float x2; float x3; 

        // figure out how much to grow the UV and vertice array.
        int badVerts = 0;
        for (int i = 0; i <= triangles.Length - 1; i += 3) {
            x1 = tmpUv[triangles[i]].x;
            x2 = tmpUv[triangles[i + 1]].x;
            x3 = tmpUv[triangles[i + 2]].x;
            if ((Math.Abs(x1 - x2) > .8F) || (Math.Abs(x1 - x3) > .8F)) {
                badVerts += 1;
            }
        }

        newVertices = new Vector3[vertices.Length + badVerts];
        newUv = new Vector2[tmpUv.Length + badVerts];
        for (int i = 0; i <= vertices.Length -1; i++ ) {
            newUv[i] = tmpUv[i];
            newVertices[i] = vertices[i];
        }
        tmpUv = null;

        int dupVertsIndex = vertices.Length;

        for (int i = 0; i <= triangles.Length - 1; i += 3) {
            x1 = newUv[triangles[i]].x;
            x2 = newUv[triangles[i + 1]].x;
            x3 = newUv[triangles[i + 2]].x;
            // figure out which vert is the deviant.
            if ((Math.Abs(x1 - x2) > .8F) && (Math.Abs(x1 - x3) > .8F)) {
                newVertices[dupVertsIndex] = vertices[triangles[i]];
                triangles[i] = dupVertsIndex;
                newUv[dupVertsIndex].x = x2;
                newUv[dupVertsIndex].y = newUv[triangles[i + 1]].y;
                dupVertsIndex += 1;
                continue;
            }
            if ((Math.Abs(x2 - x3) > .8F) && (Math.Abs(x2 - x1) > .8F)) {
                newVertices[dupVertsIndex] = vertices[triangles[i + 1]];
                triangles[i + 1] = dupVertsIndex;
                newUv[dupVertsIndex].x = x3;
                newUv[dupVertsIndex].y = newUv[triangles[i + 2]].y;
                dupVertsIndex += 1;
                continue;
            }
            if ((Math.Abs(x3 - x1) > .8F) && (Math.Abs(x3 - x2) > .8F)) {
                newVertices[dupVertsIndex] = vertices[triangles[i + 2]];
                triangles[i + 2] = dupVertsIndex;
                newUv[dupVertsIndex].x = x1;
                newUv[dupVertsIndex].y = newUv[triangles[i]].y;
                dupVertsIndex += 1;
                continue;
            }
        }
        newTriangles = triangles;
        return newVertices;
    }
}
