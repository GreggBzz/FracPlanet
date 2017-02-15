using System.Collections;
using UnityEngine;
using System;

public class PlanetTexture : MonoBehaviour {
    private struct doneMidpoint {
        public int adjVert;
        public int midPoint;
    }

    public Vector2[] TexturePlanet(int newVertIndex, Vector3[] vertices, double radius) {
        Vector2[] uv = new Vector2[newVertIndex];
        float minElev = (float)radius * 100F;
        float maxElev = 0F;
        float[] vertLength = new float[newVertIndex];
        for (int i = 0; i <= vertices.Length - 1; i++) {
            vertLength[i] = (float)Math.Sqrt((vertices[i].x * vertices[i].x) +
                                             (vertices[i].y * vertices[i].y) +
                                             (vertices[i].z * vertices[i].z));
            if (vertLength[i] <= minElev) { minElev = vertLength[i]; }
            if (vertLength[i] >= maxElev) { maxElev = vertLength[i]; }
        }

        for (int i = 0; i <= vertices.Length - 1; i++) {
            uv[i].y = 1 - (vertLength[i] - minElev) / (maxElev - minElev);
            uv[i].x = .5F;
        }
        return uv;
    }
}

