using System;
using UnityEngine;
// class to manage materials for planet / layer types.
// procedually modify the shader properties to give each planet a uniqe look.
public class PlanetMaterial : MonoBehaviour {
    private int seed;
    private System.Random rnd;
    private string txrPostFix = "";

    public Material AssignMaterial(string layer, string curPlanetType = "", bool reassign = false) {
        if (GameObject.Find("Controller (right)") != null) {
            seed = GameObject.Find("Controller (right)").GetComponent<PlanetManager>().curPlanetSeed;
        } else {
            seed = 100;
        }

        rnd = new System.Random(seed);

        txrPostFix = "";
        if (layer == "atmosphere") {
            Texture curTypeTexture = null;
            curTypeTexture = Resources.Load("PlanetTextures/" + curPlanetType + "/txrAtmosphere" + curPlanetType) as Texture;
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
            atmosphereMaterial.SetColor("_SpecColor", AtmosphereColor(curPlanetType));
            atmosphereMaterial.SetColor("_Color", AtmosphereColor(curPlanetType));
            return atmosphereMaterial;
        }

        if (layer == "cloud") {
            int cloudTxrN = rnd.Next(1, 4);
            Texture curTypeTexture = Resources.Load("PlanetTextures/txrCloud" + cloudTxrN) as Texture;
            Material cloudMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
            cloudMaterial.SetTexture("_MainTex", curTypeTexture);
            cloudMaterial.SetColor("_TintColor", CloudColor(curPlanetType));
            cloudMaterial.mainTextureScale = CloudTiling(curPlanetType);
            cloudMaterial.renderQueue = 3000;
            // Adjust the render queue if we're on the surface so it looks right.
            if (reassign) {
                cloudMaterial.renderQueue = 2900;
            }
            return cloudMaterial;
        }

        if (layer == "ocean") {
            txrPostFix = "f";
            if (reassign) { txrPostFix = ""; }
            Texture curTypeTexture = Resources.Load("PlanetTextures/" + curPlanetType + "/txrOcean" + curPlanetType + txrPostFix) as Texture;
            Material oceanMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
            oceanMaterial.SetColor("_TintColor", OceanColor(curPlanetType));
            oceanMaterial.SetTexture("_MainTex", curTypeTexture);
            oceanMaterial.mainTextureScale = OceanTiling();
            return oceanMaterial;
        }

        if (layer == "outline") {
            Texture planetOutlineTexture = Resources.Load("PlanetTextures/txrPlanetOutline") as Texture;
            Material planetOutlineMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
            planetOutlineMaterial.SetTexture("_MainTex", planetOutlineTexture);
            return planetOutlineMaterial;
        }

        if (layer == "partialOcean") {
            Texture waterFallBack = Resources.Load("PlanetTextures/" + curPlanetType + "/txrOcean" + curPlanetType) as Texture;
            Texture waterBumpMap = Resources.Load("PlanetTextures/" + curPlanetType + "/nm" + curPlanetType + "Ocean" + OceanTexture(curPlanetType)) as Texture;
            Texture waterGradient = Resources.Load("HydroTextures/WaterProDaytimeGradient") as Texture;
            Material partialOceanMaterial = new Material(Shader.Find("Custom/SimpleWater4VR"));
            partialOceanMaterial.SetTexture("_ReflectionTex", waterGradient);
            partialOceanMaterial.SetTexture("_MainTex", waterFallBack);
            partialOceanMaterial.SetTexture("_BumpMap", waterBumpMap);
            partialOceanMaterial.SetVector("_DistortParams", new Vector4(.5F, 1F, 2F, 1.15F));
            partialOceanMaterial.SetVector("_InvFadeParemeter", new Vector4(1F, 1F, 0.5F, 1F));
            partialOceanMaterial.SetVector("_AnimationTiling", new Vector4(2.2F, 2.2F, -1.1F, -1.1F));
            partialOceanMaterial.SetVector("_AnimationDirection", new Vector4(1F, 1F, 1F, 1F));
            partialOceanMaterial.SetVector("_BumpTiling", new Vector4(0.05F, 0.05F, 0.05F, 0.05F));
            partialOceanMaterial.SetVector("_BumpDirection", WaveBumpSpeed(curPlanetType));
            partialOceanMaterial.SetFloat("_FresnelScale", 0.6F);
            partialOceanMaterial.SetColor("_BaseColor", OceanColor(curPlanetType));
            partialOceanMaterial.SetColor("_ReflectionColor", OceanColor(curPlanetType));
            partialOceanMaterial.SetColor("_SpecularColor", OceanColor(curPlanetType, true));
            partialOceanMaterial.SetVector("_WorldLightDir", new Vector4(0.05F, 0.15F, -0.5F, 0.0F));
            partialOceanMaterial.SetFloat("_Shininess", 2.0F);
            partialOceanMaterial.SetFloat("_GerstnerIntensity", 1.0F);
            partialOceanMaterial.SetVector("_GAmplitude", new Vector4(0.2F, 0.2F, 0.1F, 0.1F));
            partialOceanMaterial.SetVector("_GFrequency", new Vector4(0.5F, 0.5F, 0.5F, 0.5F));
            partialOceanMaterial.SetVector("_GSteepness", new Vector4(1.0F, 1.0F, 1.0F, 1.0F));
            partialOceanMaterial.SetVector("_GSpeed", WaveSpeed(curPlanetType));
            partialOceanMaterial.SetVector("_GDirectionAB", new Vector4(0.3F, 0.85F, 0.85F, 0.25F));
            partialOceanMaterial.SetVector("_GDirectionCD", new Vector4(0.1F, 0.9F, 0.5F, 0.5F));
            return partialOceanMaterial;
        }

        if (layer == "starfield") {
            Texture starFieldTexture = Resources.Load("PlanetTextures/StarMap") as Texture;
            Material starFieldMaterial = new Material(Shader.Find("Standard"));
            starFieldMaterial.SetTexture("_MainTex", starFieldTexture);
            starFieldMaterial.SetFloat("_Mode", 1);
            starFieldMaterial.SetFloat("_Glossiness", 0F);
            starFieldMaterial.SetFloat("_Metallic", 0F);
            starFieldMaterial.SetFloat("_Cutoff", starsAlphaCutoff());
            starFieldMaterial.mainTextureScale = new Vector2(100F, 100F);
            starFieldMaterial.SetOverrideTag("RenderType", "TransparentCutout");
            starFieldMaterial.SetInt("_SrcBlend", 1);
            starFieldMaterial.SetInt("_DstBlend", 0);
            starFieldMaterial.SetInt("_ZWrite", 1);
            starFieldMaterial.EnableKeyword("_ALPHATEST_ON");
            starFieldMaterial.DisableKeyword("_ALPHABLEND_ON");
            starFieldMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            starFieldMaterial.renderQueue = 2900;
            starFieldMaterial.SetColor("_SpecColor", Color.white);
            starFieldMaterial.SetColor("_Color", Color.white);
            return starFieldMaterial;
        }

        if (layer == "terrain") {
            Texture controlTexture = Resources.Load("PlanetTextures/SplatElevation") as Texture;
            Texture[] terrainTextures = new Texture[6];
            Texture[] terrainNormals = new Texture[6];
            Material planetSurfaceMaterial = new Material(Shader.Find("Custom/Splatmap"));
            planetSurfaceMaterial.SetColor("_Color", TerrainColor(curPlanetType));
            planetSurfaceMaterial.SetTexture("_Control", controlTexture);
            planetSurfaceMaterial.SetFloat("_Smoothness", TerrainShininess(curPlanetType));
            planetSurfaceMaterial.SetColor("_Specular", TerrainColor(curPlanetType, true));
            for (int i = 0; i <= terrainTextures.Length - 1; i++) {
                txrPostFix = curPlanetType + (i + 1);
                terrainNormals[i] = Resources.Load("PlanetTextures/" + curPlanetType + "/nm" + txrPostFix) as Texture;
                // if we're far away, load the less detailed diffuse textures.
                if (reassign) { txrPostFix = curPlanetType + (i + 1); } else { txrPostFix = curPlanetType + (i + 1) + "f"; }
                terrainTextures[i] = Resources.Load("PlanetTextures/" + curPlanetType + "/txr" + txrPostFix) as Texture;
                planetSurfaceMaterial.SetTexture("_Texture" + (i + 1), terrainTextures[i]);
                planetSurfaceMaterial.SetTexture("_Normal" + (i + 1), terrainNormals[i]);
                planetSurfaceMaterial.SetTextureScale("_Texture" + (i + 1), TerrainTiling(curPlanetType));
                planetSurfaceMaterial.SetTextureScale("_Normal" + (i + 1), planetSurfaceMaterial.GetTextureScale("_Texture" + (i + 1)));
            }
            return planetSurfaceMaterial;
        }

        return new Material(Shader.Find("Standard"));
    }

    private Color32 CloudColor(string curPlanetType) {
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
            int R = rnd.Next(200, 255); int B = rnd.Next(80, 200);
            int G = rnd.Next(80, 200); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        // more Venus/Hellplanet kind of clouds.
        return new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    }

    private Color32 OceanColor(string curPlanetType, bool shiny = false) {
        // Give the ocean a unique tint.
        int R = 0, G = 0, B = 0, A = 0;
        if (shiny) {
            R = rnd.Next(150, 255); B = rnd.Next(150, 255);
            G = rnd.Next(150, 255); A = rnd.Next(150, 255);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        if (curPlanetType.Contains("Terra")) {
            R = rnd.Next(30, 80); B = rnd.Next(40, 160);
            G = rnd.Next(30, 90); A = rnd.Next(130, 210);
        }
        if (curPlanetType.Contains("Icy")) {
            R = rnd.Next(30, 255); B = rnd.Next(30, 255);
            G = rnd.Next(30, 255); A = rnd.Next(130, 210);
        }
        if (curPlanetType.Contains("Molten")) {
            R = rnd.Next(40, 105); B = rnd.Next(40, 105);
            G = rnd.Next(40, 105); A = rnd.Next(130, 210);
        }
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    private Color32 AtmosphereColor(string curPlanetType) {
        if (curPlanetType.Contains("Terra")) {
            // random earth atmosphere.
            int R = rnd.Next(100, 215); int B = rnd.Next(100, 215);
            int G = rnd.Next(100, 215); int A = rnd.Next(100, 255);
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
        return new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    }

    private Color32 TerrainColor(string curPlanetType, bool specular = false) {
        int cMin = 220; int cMax = 255;
        if (!curPlanetType.Contains("Terra")) { cMax = 255; cMin = 150; }
        if (specular) { cMax = 70; cMin = 0; } 
        int R = rnd.Next(cMin, cMax); int B = rnd.Next(cMin, cMax);
        int G = rnd.Next(cMin, cMax); int A = rnd.Next(cMin, cMax);
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    private float TerrainBumpiness(string curPlanetType) {
        // not a lot of bumpy shadows, it's Earthy.
        if (curPlanetType.Contains("Terra")) {
            return (float)rnd.NextDouble() * (.6F - .2F) + .2F;
        }
        // bumpy shadows
        if (curPlanetType.Contains("Icy")) {
            return (float)rnd.NextDouble() * (1.3F - .6F) + .6F;
        }
        if (curPlanetType.Contains("Molten")) {
            return (float)rnd.NextDouble() * (3F - 1F) + 1F;
        }
        return 1F;
    }

    private Vector4 WaveSpeed(string curPlanetType) {
        if (curPlanetType.Contains("Terra")) {
            return new Vector4(
                (float)rnd.NextDouble() * (2.2F - 1.2F) + 1.2F,
                (float)rnd.NextDouble() * (2.2F - 1.2F) + 1.2F,
                (float)rnd.NextDouble() * (2.2F - 1.2F) + 1.2F,
                (float)rnd.NextDouble() * (2.2F - 1.2F) + 1.2F);
        }
        if (curPlanetType.Contains("Icy")) {
            return new Vector4(
               (float)rnd.NextDouble() * (1.0F - .2F) + .2F,
               (float)rnd.NextDouble() * (1.0F - .2F) + .2F,
               (float)rnd.NextDouble() * (1.0F - .2F) + .2F,
               (float)rnd.NextDouble() * (1.0F - .2F) + .2F);
        }
        if (curPlanetType.Contains("Molten")) {
            return new Vector4(
               (float)rnd.NextDouble() * (.6F - .2F) + .2F,
               (float)rnd.NextDouble() * (.6F - .2F) + .2F,
               (float)rnd.NextDouble() * (.6F - .2F) + .2F,
               (float)rnd.NextDouble() * (.6F - .2F) + .2F);
        }
        return new Vector4(1, 1, 1, 1);
    }

    private Vector4 WaveBumpSpeed(string curPlanetType) {
        if (curPlanetType.Contains("Terra")) {
            return new Vector4(
                (float)rnd.NextDouble() * (12F - 9F) + 9F,
                (float)rnd.NextDouble() * (12F - 9F) + 9F,
                -(float)rnd.NextDouble() * (12F - 9F) + 9F,
                1F);
        }
        if (curPlanetType.Contains("Icy")) {
            return new Vector4(
                (float)rnd.NextDouble() * (13F - 10F) + 10F,
                (float)rnd.NextDouble() * (13F - 10F) + 10F,
                -(float)rnd.NextDouble() * (13F - 10F) + 10F,
                1F);
        }
        if (curPlanetType.Contains("Molten")) {
            return new Vector4(
                (float)rnd.NextDouble() * (4F - 2F) + 2F,
                (float)rnd.NextDouble() * (4F - 2F) + 2F,
                -(float)rnd.NextDouble() * (4F - 2F) + 2F,
                1F);
        }
        return new Vector4(10, 10, -10, 1);
    }

    private float TerrainShininess(string curPlanetType) {
        // diffuse non-shiny earth and gas.
        if (curPlanetType.Contains("Terra")) {
            return .02F;
        }
        else if (curPlanetType.Contains("Rock")) {
            return (float)rnd.NextDouble() * (.5F);
        }
        else {
            return (float)rnd.NextDouble();
        }
    }

    private Vector2 OceanTiling() {
        int xTile = rnd.Next(1, 4);
        int yTile = rnd.Next(1, 4);
        return new Vector2(xTile, yTile);
    }

    private Vector2 CloudTiling(string curPlanetType) {
        // nice clouds..
        if (curPlanetType.Contains("Terra")) {
            int xTile = rnd.Next(1, 5);
            int yTile = rnd.Next(1, 5);
            return new Vector2(xTile, yTile);
        }
        // thin clouds
        if (curPlanetType.Contains("Icy")) {
            int xTile = rnd.Next(1, 3);
            int yTile = rnd.Next(1, 3);
            return new Vector2(xTile, yTile);
        }
        // dense nasty clouds
        if (curPlanetType.Contains("Molten")) {
            int xTile = rnd.Next(1, 4);
            int yTile = rnd.Next(3, 7);
            return new Vector2(xTile, yTile);
        }
        return new Vector2(1F, 1F);
    }
    private Vector2 TerrainTiling(string curPlanetType) {
        float tile = (float)rnd.NextDouble() * (1.375F - .625F) + .625F;
        return new Vector2(tile, tile);
    }

    private string OceanTexture(string curPlanetType) {
        int txrNumber = rnd.Next(1, 4);
        return txrNumber.ToString();
    }

    private float starsAlphaCutoff() {
        // set the density of the stars by adjusting the 
        // standard shader alpha cutoff
        return (float)rnd.NextDouble() * (.7F - .220F) + .220F;
    }
}
