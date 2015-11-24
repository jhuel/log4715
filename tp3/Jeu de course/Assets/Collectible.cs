using UnityEngine;
using System.Collections;

public class Collectible : MonoBehaviour
{

    private float timer = 0;

    [Range(1f, 15f)]
    [SerializeField]
    float respawnTime = 5f;

    // Update is called once per frame
    void Update()
    {
        if (!collider.enabled)
        {
            timer += Time.deltaTime;
            if (timer > respawnTime)
            {
                renderer.enabled = true;
                collider.enabled = true;
                timer = 0;
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag.CompareTo("CarCollider") == 0)
        {
            renderer.enabled = false;
            collider.enabled = false;
        }
    }
}
