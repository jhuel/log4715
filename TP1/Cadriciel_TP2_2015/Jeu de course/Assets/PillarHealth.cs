using UnityEngine;
using System.Collections;

public class PillarHealth : MonoBehaviour {

    public int health = 200;

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
            case "Player" :
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

        Debug.Log(health);

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
