using UnityEngine;
using System.Collections;

public class ID : MonoBehaviour {

    [SerializeField]
    public int inOrderID;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public int getID()
    {
        return inOrderID;
    }
}
