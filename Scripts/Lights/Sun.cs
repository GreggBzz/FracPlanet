using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour {
    private Light sun;
    public float distance = 1.0F;
    public float zenithAngle = 0f;
    private float twilight = 500F;
    private float sundown = 0F;

	void Start () {
        sun = gameObject.GetComponent<Light>();
    }
    // Update is called once per frame
    void Update () {
        transform.RotateAround(new Vector3(0F, 750F, 3500F), Vector3.right, (.8F / distance) * Time.deltaTime);
        transform.LookAt(new Vector3(0F, 750F, 3500F));
        distance = (transform.position.y - twilight) / (twilight - sundown);
        if (distance < .20F) { distance = .20F; }
        if (distance > 1F) { distance = 1F; }
        zenithAngle = Vector3.Angle(Vector3.up, transform.position - new Vector3(0F, 750F, 3500F));
	}
    public void Disable() {
        // disable and reset the position.
        sun.intensity = 0;
        transform.position = new Vector3(0F, 5750F, 3500F);
        enabled = false;
    }
    public void Enable(float diameter = 5000F) {
        sun.intensity = 1.4F;
        transform.position = new Vector3(0F, (750F + diameter / 1.75F), 3500F);
        enabled = true;
    }
}
