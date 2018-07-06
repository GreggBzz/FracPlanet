using UnityEngine;
using System;
public class Rocks {
    private Vector3[] vertices; 
    private Vector2[] uv; 
    private int[] triangles;
    public void Generate(Vector3[] curVerts, int[] curTriangles, Vector2[] curUv, int[] dupeVerts, int type) { 
        float scaleY = 1.0F, scaleX = 1.0F, scaleZ = 1.0F, rotateX = 0F, rotateY = 0F, rotateZ = 0F;
        float jagginess = 1.1F, scaler = .30F;
        int displaceCutoff = 1;

        // Rulees for rock types:
        // 1 - large rough boulder
        // 2 - large smooth boulder
        // 3 - med/large spiky rock
        // 4 - medium flat rock
        // 5 - large rough gravel
        // 6 - large smooth gravel
        // 7 - small rough gravel
        // 8 - small smooth gravel
        // 9 - extra large boulder
        // 10 - extra large perfectly round rock
        // 11 - medium perfectly round rock
        // 12 - very irregular large.
        // 13 - very irregular medium.


        // jaggies. 
        if ((type == 1) || (type == 5) || (type == 7)) { // rough rocks
            jagginess = UnityEngine.Random.Range(1.25F, 1.50F);
        }
        if ((type == 8) || (type == 6) || (type == 2) || (type == 4)) { // smooth rocks, flat rocks
            jagginess = UnityEngine.Random.Range(1.05F, 1.08F);
        }
        if (type == 3) { // "spiky" rocks
            jagginess = UnityEngine.Random.Range(1.15F, 1.25F);
        }

        // scale.
        if ((type == 1) || (type == 2) || (type == 13)) { // boulders
            scaler = UnityEngine.Random.Range(1.25F, 1.55F);
        }
        if ((type == 3) || (type == 4) || (type == 11) || (type == 12)) { // large spiky rocks, flat rocks, perfectly round, irregular
            scaler = UnityEngine.Random.Range(.80F, 1.0F);
        }
        if ((type == 5) || (type == 6)) { // large gravel
            scaler = UnityEngine.Random.Range(.35F, .45F);
        }
        if ((type == 7) || (type == 8)) { // small gravel
            scaler = UnityEngine.Random.Range(.25F, .35F);
        }
        if ((type == 9) || (type == 10)) { // extra large.
            scaler = UnityEngine.Random.Range(1.45F, 1.65F);
        }

        // limit number of verts displaced for irregular rocks
        if ((type == 12) || (type == 13)) {
            displaceCutoff = UnityEngine.Random.Range(1, 5);
        }

        // rotation and dimensional scale.
        if (type == 4) { // flat rock, strech it out along X and Z.
            rotateX = UnityEngine.Random.Range(1, 5);
            rotateZ = UnityEngine.Random.Range(1, 5);
            rotateY = UnityEngine.Random.Range(1, 360);
            scaleX = UnityEngine.Random.Range(scaler, scaler * 7);
            scaleZ = UnityEngine.Random.Range(scaler, scaler * 7);
            scaleY = UnityEngine.Random.Range(scaler, scaler * 1.5F);
        } else if (type == 3) { // spiky rock, strech it out along Y.
            rotateX = UnityEngine.Random.Range(1, 5);
            rotateZ = UnityEngine.Random.Range(1, 5);
            rotateY = UnityEngine.Random.Range(1, 360);
            scaleX = UnityEngine.Random.Range(scaler, scaler * 1.5F);
            scaleZ = UnityEngine.Random.Range(scaler, scaler * 1.5F);
            scaleY = UnityEngine.Random.Range(scaler, scaler * 7);
        } else if ((type == 10) || (type == 11)) { // very round.
            rotateX = UnityEngine.Random.Range(1, 360);
            rotateZ = UnityEngine.Random.Range(1, 360);
            rotateY = UnityEngine.Random.Range(1, 360);
            scaleX = UnityEngine.Random.Range(scaler, scaler * 1.05f);
            scaleZ = UnityEngine.Random.Range(scaler, scaler * 1.05f);
            scaleY = UnityEngine.Random.Range(scaler, scaler * 1.05f);
        } else { // boulders and gravel, keep it mostly round.
            rotateX = UnityEngine.Random.Range(1, 360);
            rotateZ = UnityEngine.Random.Range(1, 360);
            rotateY = UnityEngine.Random.Range(1, 360);
            scaleX = UnityEngine.Random.Range(scaler, scaler * 2);
            scaleZ = UnityEngine.Random.Range(scaler, scaler * 2);
            scaleY = UnityEngine.Random.Range(scaler, scaler * 2);
        }  

        Vector3 scaleVector = new Vector3(scaleX, scaleY, scaleZ);
        Quaternion rotation = Quaternion.Euler(rotateX, rotateY, rotateZ);
        vertices = curVerts;
        triangles = curTriangles;
        uv = curUv;

        float curDisplacement = UnityEngine.Random.Range(1.00F, jagginess);
        for (int i = 0; i <= dupeVerts.Length - 1; i++) {
            if (dupeVerts[i] == -99) {
                curDisplacement = UnityEngine.Random.Range(1.00F, jagginess);
                continue;
            }
            vertices[dupeVerts[i]] = Vector3.Scale(scaleVector, vertices[dupeVerts[i]]);
            vertices[dupeVerts[i]] = rotation * vertices[dupeVerts[i]];
            if ((type != 12) && (type != 13)) {
                vertices[dupeVerts[i]] *= curDisplacement;
                continue;
            }
            else if (i < dupeVerts.Length / displaceCutoff) {
                vertices[dupeVerts[i]] *= curDisplacement;
            }
        }
    }
    public Vector3[] GetVerts() {
        return vertices;
    }
    public int[] GetTris() {
        return triangles;
    }
    public Vector2[] GetUv() {
        return uv;
    }
}
