using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour {
    private Light sun;
 	// Use this for initialization
	void Start () {
        sun = gameObject.GetComponent<Light>();
    }
    // Update is called once per frame
    void Update () {
        transform.RotateAround(new Vector3(0F, 750F, 3500F), Vector3.right, 12F * Time.deltaTime);
        transform.LookAt(new Vector3(0F, 750F, 3500F));
	}
    public void Disable() {
        // disable and reset the position.
        sun.intensity = 0;
        transform.position = new Vector3(0F, 5750F, 3500F);
        enabled = false;
    }
    public void Enable(float diameter = 5000F) {
        sun.intensity = 1F;
        transform.position = new Vector3(0F, (750F + diameter / 1.75F), 3500F);
        enabled = true;
    }
}
