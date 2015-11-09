using UnityEngine;
using System.Collections;

public class BouncyProjectileBehavior : MonoBehaviour {

    public int allowedBounces;

    private int doneBounces;

    private bool isShooting;

    public int speed = 80;

    public float duration = 5f;

	// Use this for initialization
	void Start () 
    {
	
	}

	// Update is called once per frame
	void Update () {
	
	}


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            doneBounces++;
        }

        if(doneBounces == allowedBounces)
        {
            Destroy(gameObject);
        }

    }

    void FixedUpdate()
    {
        duration -= Time.deltaTime;

        if (duration <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void Shoot()
    {
        isShooting = true;
        this.rigidbody.velocity = this.rigidbody.velocity + (this.transform.forward * speed);
    }
}
