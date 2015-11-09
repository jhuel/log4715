using UnityEngine;
using System.Collections;

public class WallHit : MonoBehaviour {

    public bool isDestructible = false;
    public GameObject explosion;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (isDestructible) {
            if (other.gameObject.CompareTag("CarCollider"))
            {
                
                Instantiate(explosion, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }
    }
}
