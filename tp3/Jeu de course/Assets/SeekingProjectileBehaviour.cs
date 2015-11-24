using UnityEngine;
using System.Collections;

public class SeekingProjectileBehaviour : MonoBehaviour
{

    public bool isTracking { get; set; }

    public float speed;

    public float UpwardforceOnCollision;

    public int damage = 10;

    public Transform trackedObject { private get; set; }

    // Use this for initialization
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        if (isTracking)
        {
            transform.LookAt(trackedObject);

            Vector3 translate = Vector3.forward * speed * Time.deltaTime;

            transform.Translate(translate);
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<CarController>().getHit(damage);

            collision.gameObject.rigidbody.velocity = new Vector3(0f, 0f, 0f);
            collision.gameObject.rigidbody.AddForce(0f, UpwardforceOnCollision, 0f);

            Destroy(gameObject);
        }
        else if ((collision.gameObject.CompareTag("Wall")))
        {
            Destroy(gameObject);
        }
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
