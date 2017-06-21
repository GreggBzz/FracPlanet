using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moon : MonoBehaviour {
    private Light moon;
    // Use this for initialization
    void Start () {
        moon = gameObject.GetComponent<Light>();
    }
    // Update is called once per frame
    void Update () {
        transform.RotateAround(new Vector3(0F, 750F, 3500F), Vector3.right, 2F * Time.deltaTime);
        transform.LookAt(new Vector3(0F, 750F, 3500F));
    }
    public void Disable() {
        // disable and reset the position.
        moon.intensity = 0;
        transform.position = new Vector3(0F, -4250F, 3500F);
        enabled = false;
    }
    public void Enable(float diameter = 5000F) {
        moon.intensity = .4F;
        transform.position = new Vector3(0F, (750F - diameter / 1.75F), 3500F);
        enabled = true;
    }
}
