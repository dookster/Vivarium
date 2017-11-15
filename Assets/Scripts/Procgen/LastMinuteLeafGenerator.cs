using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastMinuteLeafGenerator : MonoBehaviour {

    public Gradient color;

	void Start ()
    {
        Generate();
	}

    public void Generate()
    {
        Material mat = GetComponent<Renderer>().material;
        mat.color = color.Evaluate(Random.value);
        transform.localScale = new Vector2(Random.Range(0.05f, 0.2f), Random.Range(0.05f, 0.2f));
        transform.Rotate(0, 0, Random.Range(-45, 45));
    }
	
	void Update ()
    {
		
	}
}
