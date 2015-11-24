

using UnityEngine;
using System.Collections;

public class ArrowDirection : MonoBehaviour
{

    private bool leftDirection = true;
    private float timer = 0;
    private CarController car;  // the car controller we want to use

    [Range(0.5f, 10f)]
    [SerializeField]
    float arrowTime = 3.5f;

    void Awake()
    {
        // get the car controller
        renderer.enabled = false;
        car = transform.parent.gameObject.GetComponent<CarController>();
        rigidbody.position = car.rigidbody.position + Vector3.up * 5;
    }

    void FixedUpdate()
    {
        // get the car controller
        rigidbody.position = car.rigidbody.position + Vector3.up * 5;
        if (leftDirection)
        {
            rigidbody.rotation = car.rigidbody.rotation;
            rigidbody.rotation *= Quaternion.Euler(-90, 90, 0);
        }
        else
        {
            rigidbody.rotation = car.rigidbody.rotation;
            rigidbody.rotation *= Quaternion.Euler(90, 90, 0);
        }
        if (renderer.enabled)
        {
            timer += Time.deltaTime;
            if (timer > arrowTime)
                renderer.enabled = false;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Contains("checkpoint"))
        {
            renderer.enabled = true;
            timer = 0;
            Checkpoint cp = other.GetComponent<Checkpoint>();
            if (cp.getDirectionIsLeft())
            {
                leftDirection = true;
            }
            else
            {
                leftDirection = false;
            }
        }
    }
}

