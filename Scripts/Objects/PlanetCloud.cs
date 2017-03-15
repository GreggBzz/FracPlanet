using System;
using UnityEngine;
// class to manage cloud textures and movement.
public class PlanetCloud : MonoBehaviour {
    public Vector2 GetFlatTextureCorrs(Vector3 vert) {
        Vector3 normal = Vector3.Normalize(vert);

        float targetU;
        float targetV;

        float normalisedX = 0;
        float normalisedZ = -1;
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

        return new Vector2 { x = targetU, y = targetV };
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
