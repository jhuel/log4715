/* Source for speedometer : http://answers.unity3d.com/questions/15414/how-can-i-make-an-on-screen-speedometer.html */

using UnityEngine;

[RequireComponent(typeof(GUIText))]
public class CarGUI : MonoBehaviour
{

    public CarController car;                   // reference to the car controller to get the information needed for the display
    private const float MphtoMps = 2.237f;      // constant for converting miles per hour to metres per second

	public Texture2D texture = null;
	public float angle = 0;
	public Vector2 size = new Vector2(128, 128);
	Vector2 pos = new Vector2(0, 0);
	Rect rect;
	Vector2 pivot;
	
	void Start() {
		UpdateSettings();
	}

	void UpdateSettings() {
		pos = new Vector2(transform.localPosition.x, transform.localPosition.y);
		rect = new Rect(pos.x, pos.y, size.x, size.y);
		pivot = new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
	}
	
    void Update()
    {
        //object[] args = new object[] { car.CurrentSpeed * MphtoMps, car.GearNum + 1, car.NumGears, car.RevsFactor, car.AccelInput };

        // display the car gui information
        // guiText.text = string.Format(display, args);
    }

	void OnGUI() {
		angle = (car.CurrentSpeed / car.MaxSpeed) * 180;

		if (Application.isEditor) { UpdateSettings(); }
		Matrix4x4 matrixBackup = GUI.matrix;
		GUIUtility.RotateAroundPivot(angle, pivot);
		GUI.DrawTexture(rect, Texture2D.whiteTexture);
		GUI.matrix = matrixBackup;
	}
}
