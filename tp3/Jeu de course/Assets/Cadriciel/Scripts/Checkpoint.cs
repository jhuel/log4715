using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private CheckpointManager _manager;

    [SerializeField]
    private bool nextTurnIsLeft;

    [SerializeField]
    private int _index;

    public bool getDirectionIsLeft()
    {
        return nextTurnIsLeft;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other as WheelCollider == null)
        {
            CarController car = other.transform.GetComponentInParent<CarController>();
            if (car)
            {
                _manager.CheckpointTriggered(car, _index);
            }
        }
    }
}
