using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelePortParabola : MonoBehaviour {
    private WandController wand;
    private RaycastHit hit, hit2;

    public float teleDistance = 1000f;
    private float animationOffset = 0f;

    private float dashLength = .1f;
    private const int arcPointCount = 700;
    private Vector3[] arcPoints = new Vector3[arcPointCount];
    // solve with initial gravity of 50m/s^2 and velocity of 25.0m/s.
    private const float g = 50.0f;
    private const float v0 = 25.0f;

    public bool lineEnabled = false;
    private const int numDashes = 50;
    //private float dashLength = .03f;
    private Color arcColor = Color.red;

    private struct dash {
        public Vector3[] positions;
        public GameObject gameObject;
    }

    private dash[] dashes = new dash[numDashes];

    private GameObject aTeleportArc;

    // Use this for initialization
    void Start() {
        wand = GameObject.Find("Controller (right)").GetComponent<WandController>();
        aTeleportArc = new GameObject("aTeleportArc");
        Material dashMaterial = new Material(Shader.Find("Particles/Additive"));
        for (int i = 0; i <= numDashes - 1; i++) {
            dashes[i].gameObject = new GameObject("aTeleportDash" + i);
            dashes[i].gameObject.AddComponent<LineRenderer>();
            dashes[i].gameObject.GetComponent<LineRenderer>().material = dashMaterial;
            dashes[i].gameObject.GetComponent<LineRenderer>().startWidth = 0.01f;
            dashes[i].gameObject.GetComponent<LineRenderer>().endWidth = 0.01f;
            dashes[i].gameObject.GetComponent<LineRenderer>().positionCount = 2;
            dashes[i].positions = new Vector3[2];
            dashes[i].gameObject.GetComponent<LineRenderer>().SetPositions(dashes[i].positions);
            dashes[i].gameObject.GetComponent<LineRenderer>().enabled = false;
            dashes[i].gameObject.transform.parent = aTeleportArc.transform;
        }
    }
    
    public void DisableTeleportLine() {
        for (int i = 0; i <= numDashes - 1; i++) {
            dashes[i].gameObject.GetComponent<LineRenderer>().enabled = false;
        }
        lineEnabled = false;
    }

    public void EnableTeleportLine(Color newBaseColor, Color newHitColor) {
        for (int i = 0; i <= numDashes - 1; i++) {
            dashes[i].gameObject.GetComponent<LineRenderer>().enabled = true;
        }
        lineEnabled = true;
    }

    public RaycastHit UpdateTeleportLine(bool arc = true) {
        // Update our dashed teleporting line.
        if (!lineEnabled) { return new RaycastHit(); }
        if (arc) { return DrawArc(); }
        return DrawStraght();
    }

    public RaycastHit DrawArc() {
        dashLength = .1f;
        // the dash's (teleport arc's) horozontal direction.
        Vector3 flatForward = new Vector3(wand.transform.forward.x, 0F, wand.transform.forward.z);
        Vector3 startPos = wand.transform.position + wand.transform.forward * 0f;
        // initial angle of the arc.
        float theta = Vector3.Angle(flatForward, wand.transform.forward) * Mathf.Sign(wand.transform.forward.y) * Mathf.Deg2Rad;
        // invert the y velocity if pointed downward.
        float down = Mathf.Sign(theta);
        // time our "projectile" is airborne, and let's add 5 seconds.
        float totalTime = ((2 * v0 * Mathf.Sin(theta * down)) / g) + 5f;
        // slice the parabola into arcPointCount calulated positions.
        float timeSlice = totalTime / (float)arcPointCount;
        // solve for arcPointCount points along the parabola.
        int p = 0;
        for (float t = 0f; t <= (totalTime - timeSlice); t += timeSlice) {
            float hDistance = (v0 * t * Mathf.Cos(theta * down));
            arcPoints[p] = startPos + flatForward * hDistance;
            arcPoints[p].y = startPos.y + ((v0 * t * Mathf.Sin(theta)) - (.5F * g * Mathf.Pow(t, 2)));
            p += 1;
        }
        // dash it up.
        int p0 = 0; int p1 = 0; bool skip = false;
       
        int layerMask = 1 << 4; layerMask = ~layerMask; // hit everythig but water. 

        for (int i = 0; i <= (numDashes * 2) - 1; i++) {
            do {
                p1 += 1;
                if (p1 > arcPointCount - 1) { p1 = arcPointCount - 1; break; }
            } while (Vector3.Distance(arcPoints[p0], arcPoints[p1]) < dashLength);
            if (!skip) {
                dashes[i / 2].positions[0] = arcPoints[p0];
                dashes[i / 2].positions[1] = arcPoints[p1];
                dashes[i / 2].gameObject.GetComponent<LineRenderer>().SetPositions(dashes[i / 2].positions);
                dashes[i / 2].gameObject.GetComponent<LineRenderer>().startColor = arcColor;
                dashes[i / 2].gameObject.GetComponent<LineRenderer>().endColor = arcColor;
                dashes[i / 2].gameObject.GetComponent<LineRenderer>().enabled = true;
                if (Physics.Raycast(arcPoints[p0], arcPoints[p1] - arcPoints[p0], out hit, dashLength * 5f, layerMask)) {
                    for (int i2 = i / 2; i2 <= numDashes - 1; i2++) {
                        dashes[i2].gameObject.GetComponent<LineRenderer>().enabled = false;
                    }
                    if (hit.transform.gameObject.name.Contains("Rock")) {
                        arcColor = Color.red;
                        return new RaycastHit();
                    }
                    arcColor = Color.green;
                    return hit;
                }
            }
            p0 = p1;
            skip = !skip;
        }
        arcColor = Color.red;
        return new RaycastHit();
    }

    public RaycastHit DrawStraght() {
        Vector3 dashDirection = wand.transform.forward;
        Vector3 dashStartPos = wand.transform.position + dashDirection * animationOffset;
        dashLength = teleDistance / (numDashes * 2);
        int curDash = 0;

        for (int i = 0; i <= numDashes * 2 - 1; i += 2) {
            dashes[curDash].positions[0] = dashStartPos + dashDirection * dashLength * i;
            dashes[curDash].positions[1] = dashStartPos + dashDirection * dashLength * (i + 1);
            dashes[curDash].gameObject.GetComponent<LineRenderer>().SetPositions(dashes[curDash].positions);
            dashes[curDash].gameObject.GetComponent<LineRenderer>().startColor = arcColor;
            dashes[curDash].gameObject.GetComponent<LineRenderer>().endColor = arcColor;
            curDash += 1;
        }
        if (Physics.Raycast(dashStartPos, wand.transform.forward, out hit, teleDistance)) {
            arcColor = Color.green;
            animationOffset += Time.deltaTime * 4 * dashLength;
            if (animationOffset > 2 * dashLength) { animationOffset = 0f; }
            return hit;
        }
        else {
            arcColor = Color.red;
            animationOffset = 0f;
        }
        return new RaycastHit();
    }
}
