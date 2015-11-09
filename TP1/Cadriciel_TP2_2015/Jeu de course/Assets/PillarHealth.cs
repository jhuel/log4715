using UnityEngine;
using System.Collections;

public class PillarHealth : MonoBehaviour {

    public int health = 200;

    public GameObject explosion;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.collider.gameObject.tag);
        switch (collision.collider.gameObject.tag)
        {
            case "CarCollider" :
                Instantiate(explosion, transform.position, transform.rotation);
                collision.collider.gameObject.GetComponentInParent<Rigidbody>().velocity = Vector3.zero;
                collision.collider.gameObject.GetComponentInParent<Rigidbody>().angularVelocity = Vector3.zero;
                Destroy(gameObject);
                return;
            case "BouncyProjectile":
                health -= 10;
                break;
            case "SeekingProjectile":
                health -= 30;
                break;
            default:
                break;
        }

        if (health <= 0)
        {
            Instantiate(explosion, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
