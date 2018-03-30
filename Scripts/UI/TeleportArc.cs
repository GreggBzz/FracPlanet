using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportArc : MonoBehaviour {
    public LineRenderer lineRenderer;
    private Vector3[] lineRendererVertices = new Vector3[2];
    public GameObject teleportLines;
    private Color baseColor;
    private Color hitColor;
    private WandController wand;
    private RaycastHit hit, hit2;
    public float teleDistance;


    // Use this for initialization
    void Start () {
        wand = GameObject.Find("Controller (right)").GetComponent<WandController>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DestroyTeleportLine() {
        if (lineRenderer != null) {
            lineRenderer = null;
            Destroy(teleportLines);
        }
    }

    public void AddTeleportLine(Color newBaseColor, Color newHitColor) {
        if (lineRenderer != null) {
            return;
        }
        baseColor = newBaseColor;
        hitColor = newHitColor;
        teleportLines = new GameObject("Pointer Line");
        lineRenderer = teleportLines.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.numPositions = 2;
    }

    public void UpdateTeleportLine() {
        // Update our LineRenderer
        if (lineRenderer && lineRenderer.enabled) {
            Vector3 startPos = wand.transform.position;
            // If our raycast hits (line will be green) end the line at that positon. Otherwise,
            // make our line point straight out for 4500 meters, and red.
            if (Physics.Raycast(startPos, wand.transform.forward, out hit, teleDistance)) {
                lineRendererVertices[1] = hit.point;
                lineRenderer.startColor = baseColor;
                lineRenderer.endColor = baseColor;
            }
            else {
                lineRendererVertices[1] = startPos + wand.transform.forward * teleDistance;
                lineRenderer.startColor = hitColor;
                lineRenderer.endColor = hitColor;
            }
            lineRendererVertices[0] = wand.transform.position;
            lineRenderer.SetPositions(lineRendererVertices);
        }
    }
    public void SetShortRange() {

    }
}
