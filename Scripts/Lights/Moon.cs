using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : MonoBehaviour {
    private Light moon;
    public float distance = 1.0F;
    private float twilight = 500F;
    private float sundown = 0F;
    // Use this for initialization
    void Start () {
        moon = gameObject.GetComponent<Light>();
    }
    // Update is called once per frame
    void Update () {
        transform.RotateAround(new Vector3(0F, 750F, 3500F), Vector3.right, (1.8F * distance) * Time.deltaTime);
        transform.LookAt(new Vector3(0F, 750F, 3500F));
        distance = (transform.position.y - twilight) / (twilight - sundown);
        if (distance < .20F) { distance = .20F; }
        if (distance > 1F) { distance = 1F; }
    }
    public void Disable() {
        // disable and reset the position.
        moon.intensity = 0;
        transform.position = new Vector3(0F, -4250F, 3500F);
        enabled = false;
    }
    public void Enable(float diameter = 5000F) {
        moon.intensity = .03F;
        transform.position = new Vector3(0F, (750F - diameter / 1.75F), 3500F);
        enabled = true;
    }
}
