using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderArea : MonoBehaviour {
    public GameObject Chair;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided!");
        if (collision.collider.gameObject.tag == "CollisionAreas")
        {
            Debug.Log("Removing " + gameObject.name);
            gameObject.SetActive(false);
        }
    }
}
