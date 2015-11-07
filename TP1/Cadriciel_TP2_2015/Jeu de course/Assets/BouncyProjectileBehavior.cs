using UnityEngine;
using System.Collections;

public class BouncyProjectileBehavior : MonoBehaviour {

    public int allowedBounces;

    private int doneBounces;

	// Use this for initialization
	void Start () 
    {
	
	}

	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision collision)
    {
        doneBounces++;

        if(collision.collider.gameObject.name == "")
        {

        }

        if(doneBounces == allowedBounces)
        {
            Debug.Log("Done bounces is damn too high");
            Destroy(gameObject);
        }

    }
}
