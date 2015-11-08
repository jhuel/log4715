using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour
{

    // https://unity3d.com/learn/tutorials/projects/roll-a-ball/creating-collectables?playlist=17141

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }
}
