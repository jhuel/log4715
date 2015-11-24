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

    void FixedUpdate()
    {
        if (isTemporarelyDisabled)
        {
            timer += Time.deltaTime;

            if (timer >= 4.0f)
            {
                GetComponent<MeshRenderer>().enabled = true;
                collider.enabled = true;
                timer = 0;
                isTemporarelyDisabled = false;
            }
        }
    }

    private bool isTemporarelyDisabled = false;
    private float timer;

    void OnTriggerEnter(Collider other)
    {
        if (isDestructible) {
            if (other.gameObject.CompareTag("CarCollider") || other.gameObject.CompareTag("BouncyProjectile"))
            {
                if (other.gameObject.CompareTag("BouncyProjectile"))
                {
                    Destroy(other.gameObject);
                }
                Instantiate(explosion, transform.position, transform.rotation);


                GetComponent<MeshRenderer>().enabled = false;
                collider.enabled = false;
                isTemporarelyDisabled = true;
            }
        }
    }
}
