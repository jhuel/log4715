using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarUserControlMP : MonoBehaviour
{
	private CarController car;  // the car controller we want to use

	[SerializeField]
	private string vertical = "Vertical";

	[SerializeField]
	private string horizontal = "Horizontal";

    [SerializeField]
    private string fireGreen = "Fire1";

    [SerializeField]
    private string fireRed = "Fire2";
	
	void Awake ()
	{
		// get the car controller
		car = GetComponent<CarController>();
	}
	
	void Update()
    { 
        // http://answers.unity3d.com/questions/19710/shooting-a-bullet-projectile-properly.html 

        bool fire = CrossPlatformInput.GetButtonDown(fireGreen);
        if (fire)
        {
            car.ShootGreen();
        }

        if (CrossPlatformInput.GetButtonDown(fireRed))
        {
            car.shootRed();
        }

    }
	void FixedUpdate()
	{



		// pass the input to the car!
		#if CROSS_PLATFORM_INPUT
		float h = CrossPlatformInput.GetAxis(horizontal);
		float v = CrossPlatformInput.GetAxis(vertical);
		
		#else
		float h = Input.GetAxis(horizontal);
		float v = Input.GetAxis(vertical);
		#endif
		car.Move(h,v);
		
	}
}
