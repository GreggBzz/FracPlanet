using System;
using UnityEngine;

public class PlanetSplatMap : MonoBehaviour {
    // builds a splat map based on slope and elevation for the planet.
    private float waterLine;
    private float maxHeight;
    private float minHeight;
    private float averageHeight;
    private float lowerPlains;
    private Vector2[] uv4;
    private Vector2[] uv3;
    
    // Use this for initialization
    void Start() {
    }

    public Vector2[] assignSplatMap(Vector3[] vertices, Vector3[] normals = null) {
        // splat map shader blends 6 textures.
        // control texture fades, bottom to top, red->green->blue->black->red->green 
        // approximate color heights are: 
        // txr #1 red from   .0000 to .4125
        // fade to green     .4125 to .4425
        // txr #2 green from .4425 to .5125
        // fade to blue      .5125 to .5425
        // txr #3 blue from  .5425 to .6000
        // fade to black     .6000 to .6250
        // txr #4 black      .6250 to .6750
        // fade to red       .6750 to .7000
        // txr #5 red        .7000 to .7375
        // fade to green     .7375 to .7750
        // txr $6 green      .7750 to 1.000 
        float txr1 = 0F;              // underwater
        float txr1to2 = 0.4275F;      //
        float txr2 = 0.4425F;         // beach sand
        float txr2to3 = 0.5275F;      // 
        float txr3 = 0.5425F;         // first plain
        float txr3to4 = 0.6125F;      //
        float txr4 = 0.6250F;         // rocky cliff
        float txr4to5 = 0.6875F;      // 
        float txr5 = 0.7F;            // second plain
        float txr5to6 = 0.75625F;     //
        float txr6 = 0.775F;          // mountain heights

        uv4 = new Vector2[vertices.Length];
        uv3 = new Vector2[vertices.Length];

        waterLine = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().waterLine;
        maxHeight = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().maxHeight;
        minHeight = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().minHeight;
        averageHeight = GameObject.Find("aPlanet").GetComponent<PlanetGeometryDetail>().averageHeight;
        lowerPlains = (waterLine + ((maxHeight - waterLine) * .50F));


        float normalizedHeight = 0f;
        float vertHeight = 0f;
        float angle = 0f;

        float roughness = 0f;
        float slopeAngle = 20f;
        roughness = GameObject.Find("aPlanet").GetComponent<PlanetGeometry>().roughness;

        if (roughness >= .000040F) { slopeAngle = 35; }

        for (int i = 0; i <= vertices.Length - 1; i++) {
            angle = Vector3.Angle(normals[i], vertices[i].normalized);
            vertHeight = (float)Math.Sqrt((vertices[i].x * vertices[i].x) +
                                          (vertices[i].y * vertices[i].y) +
                                          (vertices[i].z * vertices[i].z));
            uv3[i].x = 0; uv3[i].y = 0;

            // below the waterline - flow between txr1 and txr2to3
            if (vertHeight < waterLine) {
                normalizedHeight = (vertHeight - minHeight) / (waterLine - minHeight);
                uv4[i].y = normalizedHeight * txr2to3; uv4[i].x = 0;
                if (angle > slopeAngle) { // clifs get higher texture.
                    uv3[i].x = 1f;
                }
                continue;
            }
            // low plains? (the bottom 50% of above water terrain) - flow between txr2to3 and txr4;
            // and make inclines a rocky texture.
            if (vertHeight < lowerPlains) {
                normalizedHeight = (vertHeight - waterLine) / (lowerPlains - waterLine);
                uv4[i].y = normalizedHeight * (txr4 - txr2to3) + txr2to3; uv4[i].x = 0f;
                if (angle > slopeAngle) {
                    uv3[i].x = 1f;
                }
                continue;
            }
            // high plains and mountains - flow between txr4 and 1.0
            if (vertHeight < maxHeight) {
                normalizedHeight = (vertHeight - lowerPlains) / (maxHeight - lowerPlains);
                uv4[i].y = normalizedHeight * (1.0f - txr4) + txr4; uv4[i].x = 0f;
                if (angle > slopeAngle) {
                    uv3[i].x = 1f;
                }
                if (uv4[i].y > .65f) { uv4[i].x = 1f; }
            }
        }
        return uv4;
    }
    public Vector2[] GetUV3() {
        return uv3;
    }
}

