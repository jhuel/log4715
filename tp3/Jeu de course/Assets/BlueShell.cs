using UnityEngine;
using System.Collections;

public class BlueShell : MonoBehaviour {

    const int MAX_IDS = 23;

    public int damage = 20;
    public bool isTracking { get; set; }
    public Transform trackedObject { private get; set; }

    private int currentID = -1;

    public float UpwardforceOnCollision;

    public GameObject explosion;

    [SerializeField]
    public float speed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (isTracking)
        {
            GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("BlueCheckpoint");

            float shortestDistance = float.MaxValue;
            Transform closestCheckpoint = null;
            int otherID;


            /* Find closest car */
            foreach (GameObject checkpoint in checkpoints)
            {
                otherID = checkpoint.GetComponentInParent<ID>().getID();
                /* Find magnitude (distance between the two objects) */
                float distance =
                    (checkpoint.transform.position - transform.position).magnitude;
                if (distance < shortestDistance && checkpoint != null && checkpoint.transform != transform && (otherID > currentID || (currentID == MAX_IDS && otherID == 0)) )
                {
                    closestCheckpoint = checkpoint.gameObject.transform;
                    shortestDistance = distance;

                }
            }

            float distanceBetweenPlayer = (transform.position - trackedObject.position).magnitude;

            if (distanceBetweenPlayer < shortestDistance)
            {
                transform.LookAt(trackedObject);
            }
            else
            {
                transform.LookAt(closestCheckpoint);
            }
            

            Vector3 translate = Vector3.forward * speed * Time.deltaTime;

            transform.Translate(translate);
        }
	}

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BlueCheckpoint"))
        {
            currentID = other.GetComponentInParent<ID>().getID();

            if (currentID == MAX_IDS)
            {
                //currentID = 0;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            collision.gameObject.GetComponent<CarController>().getHit(damage);
            collision.gameObject.rigidbody.velocity = new Vector3(0f, 0f, 0f);
            collision.gameObject.rigidbody.AddForce(0f, UpwardforceOnCollision, 0f);

            if (collision.gameObject.transform == trackedObject)
            {
                collision.gameObject.GetComponent<CarController>().getHit(30);
                Instantiate(explosion, transform.position, transform.rotation);
                Destroy(gameObject);
            }

        }
    }
}
