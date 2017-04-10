using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLight : MonoBehaviour {
    private Light mainLight;
    // Use this for initialization
    void Start () {
        mainLight = gameObject.GetComponent<Light>();
    }
    // Update is called once per frame
    void Update () {
		
	}
    public void Disable() {
        mainLight.intensity = 0;
    }
    public void Enable() {
        mainLight.intensity = .8F;
    }
}
