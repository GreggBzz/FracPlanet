using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraEffects : MonoBehaviour {
    private GameObject cameraRig;
    private GameObject cameraEye;
    private GameObject fogSphere; 

    private Shader blurFastShader;
    private DrawScene aScene;

    private byte inkyNess; // to fade to black.
    private float foggieness; // to make things foggier.
    private Material fogSphereMaterial;
    private Texture fogSphereTexture;
    private Color32 fogSphereColor;
    private Color32 oceanColor = new Color32();

    private Vector3 eyePos;
    private Vector3 eyeForward;
    private RaycastHit hit;

    private int layerMaskWater;
    private int layerMaskObjects;

    private int debugcount = 0;
    private bool oceanColorSet = false;

    // color of the fog to match the color of the ocean.
    private int R = 0, G = 0, B = 0, A = 0;


    // Use this for initialization
    void Start() {
        cameraEye = GameObject.Find("Camera (eye)");
        cameraRig = GameObject.Find("[CameraRig]");
        aScene = gameObject.GetComponent<DrawScene>();
        fogSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fogSphere.GetComponent<SphereCollider>().enabled = false;
        fogSphere.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        fogSphere.GetComponent<MeshRenderer>().receiveShadows = false;
        fogSphere.gameObject.name = "CameraFogSphere";
        fogSphereTexture = Resources.Load("PlanetTextures/fogSphere") as Texture;
        fogSphereMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
        fogSphereMaterial.renderQueue = 4000;
        fogSphereMaterial.SetTexture("_MainTex", fogSphereTexture);
        fogSphere.GetComponent<Renderer>().material = fogSphereMaterial;
        fogSphere.transform.localScale = new Vector3(.25f, .25f, .25f);
        fogSphere.GetComponent<Renderer>().enabled = false;
        foggieness = 0.0F;
        layerMaskWater = LayerMask.GetMask("Water");
        layerMaskObjects = LayerMask.GetMask("Rocks", "Trees", "Default");
    }

    // Update is called once per frame
    void Update () {
        if (!oceanColorSet) {
            if (GameObject.Find("aPlanetTopOcean")) {
                oceanColor = GameObject.Find("aPlanetTopOcean").GetComponent<Renderer>().material.GetColor("_BaseColor");
                R = oceanColor.r; G = oceanColor.g; B = oceanColor.b; A = oceanColor.a;
                oceanColorSet = true;
            }
            else {
                R = oceanColor.r; G = oceanColor.g; B = oceanColor.b; A = oceanColor.a;
            }
        }
        RenderEffects();
        fogSphere.transform.position = cameraEye.transform.position;
    }

    private void RenderEffects() {
        // do something to the camera when underwater, or inside of an object.
        eyeForward = cameraEye.transform.forward;
        if (aScene.onWhichPlanet != "") {
            //adjust eye position to test for a rock/tree collision.
            eyePos = cameraRig.transform.position + cameraEye.transform.localPosition;
            if (Physics.Raycast(eyePos + (eyeForward * -.4f), eyeForward, out hit, .55f, layerMaskObjects)) {
                if (inkyNess >= 240) {
                    inkyNess = 255;
                } else {
                    inkyNess += 5;
                }
                fogSphere.GetComponent<Renderer>().enabled = true;
                fogSphereMaterial.SetColor("_TintColor", new Color32(0, 0, 0, inkyNess));
                return;
            }
            else {
                inkyNess = 0;
            }
            // tweak the eye position in world space because our water collider is .35F lower than the shader makes the waves.
            eyePos = cameraRig.transform.position + cameraEye.transform.localPosition - new Vector3(0F, .4F, 0F);
            if (Physics.Raycast(eyePos + Vector3.up * 300F, Vector3.down, 300F, layerMaskWater)) {
                fogSphere.GetComponent<Renderer>().enabled = true;
                foggieness += .01F;
                if (foggieness >= .99F) {
                    foggieness = .99F;
                } else {
                    fogSphereColor = new Color32((byte)(R * foggieness), (byte)(G * foggieness),
                        (byte)(B * foggieness), (byte)(A * foggieness * .6));
                    fogSphereMaterial.SetColor("_TintColor", fogSphereColor);
                    return;
                }
            }
            else {
                foggieness -= .01F;
                if (foggieness <= .01F) {
                    foggieness = 0F;
                    fogSphere.GetComponent<Renderer>().enabled = false;
                } else {
                    fogSphereColor = new Color32((byte)(R * foggieness), (byte)(G * foggieness),
                            (byte)(B * foggieness), (byte)(A * foggieness * .6));
                    fogSphereMaterial.SetColor("_TintColor", fogSphereColor);
                }
            }
        }
        else {
            fogSphere.GetComponent<Renderer>().enabled = false;
        }
    }
}
