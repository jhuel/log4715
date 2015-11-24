using UnityEngine;
using System.Collections;

public class BouncyProjectileBehavior : MonoBehaviour
{

    public int allowedBounces;

    private int doneBounces;

    public int speed = 80;

    public float duration = 5f;

    public int damage = 10;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<CarController>().getHit(damage);
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            doneBounces++;
        }

        if (doneBounces == allowedBounces)
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
        this.rigidbody.velocity = this.rigidbody.velocity + (this.transform.forward * speed);
    }

    private int mapTriggers = 0;
    public void OnTriggerEnter(Collider other)
    {
        string otherTag = other.gameObject.tag;
        if (otherTag.CompareTo("MapCollider") == 0)
        {
            mapTriggers++;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("MapCollider"))
        {
            mapTriggers--;
        }

        if (mapTriggers <= 0)
        {
            Destroy(gameObject);
        }
    }
}
