using System;
using UnityEngine;
// class to manage materials for planet / layer types.

public class PlanetMaterial : MonoBehaviour {
    public int seed = 100;
    private System.Random rnd;

    public Material AssignMaterial(string layer, string curPlanetType = "", int NewSeed = 100) {
        seed = NewSeed;
        if (layer == "terrain") {
            Texture curTypeTexture = Resources.Load("PlanetTextures/" + curPlanetType + "/txr" + curPlanetType) as Texture;
            Texture curTypeNormalMap = Resources.Load("PlanetTextures/" + curPlanetType + "/nm" + curPlanetType) as Texture;
            Material planetSurfaceMaterial = new Material(Shader.Find("Bumped Diffuse"));
            planetSurfaceMaterial.SetTexture("_MainTex", curTypeTexture);
            planetSurfaceMaterial.SetTexture("_BumpMap", curTypeNormalMap);
            return planetSurfaceMaterial;
        }

        if (layer == "ocean") {
            Texture curTypeTexture = Resources.Load("PlanetTextures/" + curPlanetType + "/txrOcean" + curPlanetType) as Texture;
            Material oceanMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
            oceanMaterial.SetTexture("_MainTex", curTypeTexture);
            return oceanMaterial;
        }

        if (layer == "atmosphere") {
            Texture curTypeTexture = Resources.Load("PlanetTextures/" + curPlanetType + "/txrAtmosphere" + curPlanetType) as Texture;
            Texture curTypeNormalMap = Resources.Load("PlanetTextures/" + curPlanetType + "/nmAtmosphere" + curPlanetType) as Texture;
            Material atmosphereMaterial = new Material(Shader.Find("Standard"));
            atmosphereMaterial.SetTexture("_MainTex", curTypeTexture);
            atmosphereMaterial.SetTexture("_BumpMap", curTypeNormalMap);
            atmosphereMaterial.SetFloat("_Mode", 2);
            atmosphereMaterial.SetFloat("_Glossiness", .6F);
            atmosphereMaterial.SetFloat("_Metallic", .7F);
            atmosphereMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            atmosphereMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            atmosphereMaterial.SetInt("_ZWrite", 0);
            atmosphereMaterial.DisableKeyword("_ALPHATEST_ON");
            atmosphereMaterial.EnableKeyword("_ALPHABLEND_ON");
            atmosphereMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            atmosphereMaterial.renderQueue = 3000;
            if (curPlanetType.Contains("Terra")) {
                atmosphereMaterial.SetColor("_SpecColor", new Color32(0xFF, 0xFF, 0xFF, 0x00));
                atmosphereMaterial.SetColor("_Color", new Color32(0x52, 0xA5, 0xC3, 0xFF));
            }
            else {
                atmosphereMaterial.SetColor("_SpecColor", new Color32(0xFF, 0xEF, 0x96, 0x00));
                atmosphereMaterial.SetColor("_Color", new Color32(0x92, 0xAE, 0x5C, 0xFF));
            }
            return atmosphereMaterial;
        }

        if (layer == "cloud") {
            Texture curTypeTexture = Resources.Load("PlanetTextures/" + curPlanetType + "/txrCloud" + curPlanetType) as Texture;
            Material cloudMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
            cloudMaterial.SetTexture("_MainTex", curTypeTexture);
            cloudMaterial.SetColor("_TintColor", CloudColor(curPlanetType, seed));
            cloudMaterial.mainTextureScale = CloudTiling(curPlanetType, seed);
            return cloudMaterial;
        }

        if (layer == "outline") {
            Texture planetOutlineTexture = Resources.Load("PlanetTextures/txrPlanetOutline") as Texture;
            Material planetOutlineMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
            planetOutlineMaterial.SetTexture("_MainTex", planetOutlineTexture);
            return planetOutlineMaterial;
        }
        return new Material(Shader.Find("Standard"));
    }

    private Color32 CloudColor(string curPlanetType, int seed) {
        Debug.Log("In clouds, curPlanetType: " + curPlanetType);
        rnd = new System.Random(seed);
        if (curPlanetType.Contains("Terra")) {
            // more Earthy white/gray kind of clouds.
            // setup RGB and Alpha.
            int R = rnd.Next(150, 255); int B = rnd.Next(150, 255);
            int G = rnd.Next(150, 255); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        if (curPlanetType.Contains("Icy")) {
            // more Titan/yellow kind of clouds.
            int R = rnd.Next(150, 255); int B = rnd.Next(0, 150);
            int G = rnd.Next(150, 255); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        if (curPlanetType.Contains("Molten")) {
            // more Venus/Hellplanet kind of clouds.
            int R = rnd.Next(200, 255); int B = rnd.Next(200, 255);
            int G = rnd.Next(200, 255); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }

        // more Venus/Hellplanet kind of clouds.
        return new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    }

    private Vector2 CloudTiling(string curPlanetType, int seed) {
        rnd = new System.Random(seed);
        // nice clouds..
        if (curPlanetType.Contains("Terra")) {
            int xTile = rnd.Next(1, 5);
            int yTile = rnd.Next(1, 5);
            return new Vector2((float)xTile, (float)yTile);
        }
        // thin clouds
        if (curPlanetType.Contains("Icy")) {
            int xTile = rnd.Next(1, 3);
            int yTile = rnd.Next(1, 3);
            return new Vector2((float)xTile, (float)yTile);
        }
        // dense nasty clouds
        if (curPlanetType.Contains("Molten")) {
            int xTile = rnd.Next(5, 20);
            int yTile = rnd.Next(5, 20);
            return new Vector2((float)xTile, (float)yTile);
        }
        return new Vector2(1F, 1F);
    }
}
