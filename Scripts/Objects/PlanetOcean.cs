using System;
using UnityEngine;

public class PlanetOcean : MonoBehaviour {
    // fake waves.    
    private bool[] waveDirection;
    private float[] waveSize;
    private int tideDirection = 1; 

    public bool skipframe = true;
    public float tideStrength = 1.5F;

    public Vector3[] MakeWaves(Vector3[] vertices, Vector3 center) {
        // A simple wave function. Let's move this to a shader, later.
        for (int i = 0; i <= vertices.Length - 1; i += 1) {
            if (waveDirection[i]) {
                vertices[i] = vertices[i] * (1.0F + (.00002F / (float)Math.Sqrt(Math.Abs(waveSize[i]))));
                waveSize[i] += .3F;
            }
            else {
                vertices[i] = vertices[i] / (1.0F + (.00002F / (float)Math.Sqrt(Math.Abs(waveSize[i]))));
                waveSize[i] += -.3F;
            }
            if (Math.Abs(waveSize[i]) > 30F) waveDirection[i] = !waveDirection[i];
        }

        if (tideStrength >= 3F) {
            SetTide();
            tideDirection = tideDirection * -1;
            tideStrength = 2.999F;
        }
        if (tideStrength <= .1F) {
            tideDirection = tideDirection * -1;
            tideStrength = .111F;
        }
        tideStrength += ((Time.deltaTime / tideStrength) * tideDirection);
        return vertices;
    }

    private void SetTide() {
        for (int i = 0; i <= waveDirection.Length - 1; i += 1) {
            // Invert one of ten vertices.
            if (UnityEngine.Random.value > 0.3f) waveDirection[i] = !waveDirection[i];
        }
    }

    public void InitializeWaves(int vertCount) {
        waveDirection = new bool[vertCount];
        waveSize = new float[vertCount];
        for (int i = 0; i <= vertCount - 1; i += 1) {
            waveDirection[i] = (UnityEngine.Random.value > 0.5f);
            if (waveDirection[i]) {
                waveSize[i] = (UnityEngine.Random.value * 30F) + .05F;
            }
            else {
                waveSize[i] = -1F * ((UnityEngine.Random.value * 30F) + .05F);
            }
        }
    }
}
