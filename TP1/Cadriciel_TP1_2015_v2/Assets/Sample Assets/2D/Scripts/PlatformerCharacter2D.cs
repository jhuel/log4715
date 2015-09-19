using UnityEngine;
using System.Collections;

public class PlatformerCharacter2D : MonoBehaviour 
{
	bool facingRight = true;							// For determining which way the player is currently facing.

	[SerializeField] float maxSpeed = 10f;					// The fastest the player can travel in the x axis.
	[SerializeField] float jumpForce = 10f;					// Amount of force added when the player jumps.	
	[SerializeField] float jumpWallHorizontalForce = 50f;		// Amount of force added when the player jumps when he touches the wall. This is an offset to the X component
	[SerializeField] float jetpackForce = 1f;			//
	[SerializeField] bool showJumpLine = false;			// A boolean marking the maximum jump distance relative to the character

	[Range(0, 1)]
	[SerializeField] float crouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%

	[Range(0.01f, 0.2f)]
	[SerializeField] float jumpTime = 0.05f;			// Maximum jump time
	[SerializeField] float floatingFactor = .5f;		// Floating factor when the character is moving in the air
	[SerializeField] LayerMask whatIsGround;			// A mask determining what is ground to the character
	[SerializeField] LayerMask whatIsWall;				// A mask determining what is wall to the character
	[SerializeField] int maximumJumps = 1;				// A mask determining what is ground to the character

	
	Transform ceilingCheck;								// A position marking where to check for ceilings
	Transform groundCheck;								// A position marking where to check if the player is grounded
	Transform wallCheck;								// A position marking where to check for walls

	int jumpCounter = 0;

	float ceilingRadius = .01f;							// Radius of the overlap circle to determine if the player can stand up
	float groundedRadius = .05f;						// Radius of the overlap circle to determine if grounded
	float wallRadius  = 0.2f;							// Radius of the overlap circle to determine if touching the wall									
	float jumpTimer = 0;

	bool grounded = false;								// Whether or not the player is grounded.
	bool isTouchingWall = false;						// Whether or not the player is touching the wall
	bool isJumping = false; 							// Character is currently jumping

	Animator anim;										// Reference to the player's animator component.
	
    void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("GroundCheck");
		ceilingCheck = transform.Find("CeilingCheck");
		wallCheck = transform.Find("WallCheck");
		anim = GetComponent<Animator>();
	}


	void FixedUpdate()
	{
		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundedRadius, whatIsGround);
		anim.SetBool("Ground", grounded);

		// Set the vertical animation
		anim.SetFloat("vSpeed", rigidbody2D.velocity.y);

		if (showJumpLine) {
			//float maxJumpDist = (jumpForce - Physics.gravity.y) * Mathf.Pow(jumpTime, 2);
			float maxJumpDist = Mathf.Pow(jumpForce, 2)/(2 * Physics.gravity.y );
			Debug.DrawLine (new Vector3 (rigidbody2D.position.x - 200, rigidbody2D.position.y + maxJumpDist), new Vector3 (rigidbody2D.position.x + 200, rigidbody2D.position.y + maxJumpDist), Color.green, 0, true);
		}
	}

	public void Move(float move, bool crouch, bool jump)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch && anim.GetBool ("Crouch")) {
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle (ceilingCheck.position, ceilingRadius, whatIsGround))
				crouch = true; 
		}


		//only control the player if grounded or floatingFactor is bigger than 0
		if (grounded || floatingFactor > 0) {
			// Set whether or not the character is crouching in the animator
			anim.SetBool ("Crouch", crouch);
			
			// Reduce the speed if crouching by the crouchSpeed multiplier
			move = (crouch ? move * crouchSpeed : move);
			
			// The Speed animator parameter is set to the absolute value of the horizontal input.
			anim.SetFloat ("Speed", Mathf.Abs (move));

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !facingRight)
				// ... flip the player.
				Flip ();
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && facingRight)
				// ... flip the player.
				Flip ();
		}

		if (grounded) { // not in the air

			rigidbody2D.velocity = new Vector2 (move * maxSpeed, rigidbody2D.velocity.y);

		} else if (floatingFactor > 0) {

			rigidbody2D.velocity = new Vector2 (move * maxSpeed * floatingFactor, rigidbody2D.velocity.y);

			isTouchingWall = Physics2D.OverlapCircle (wallCheck.position, wallRadius, whatIsWall);
		}

		// If the player should jump...
		if (((jumpCounter < maximumJumps && !isJumping) || isTouchingWall) && jump) {

			if (!isTouchingWall) {
				jumpCounter++;
			} 

			isJumping = true;
	
			StartCoroutine (JumpRoutine ());
		} else if (jumpCounter == maximumJumps && CrossPlatformInput.GetButton ("Jump")) {
			Debug.Log ("Jetpack");
			rigidbody2D.AddForce (new Vector2 (0f, jetpackForce));
		}

		if (grounded && !isJumping) {
			Debug.Log ("I am grounded and I reset jump counter");
			jumpCounter = 0;
		}
	}

	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	// http://gamasutra.com/blogs/DanielFineberg/20150825/244650/Designing_a_Jump_in_Unity.php
	IEnumerator JumpRoutine()
	{


		float xComponent = 0f;
		if (isTouchingWall) {
			xComponent = jumpWallHorizontalForce;

			if(facingRight)
			{
				xComponent = -xComponent;
			}
			Flip ();
		}

		rigidbody2D.velocity = new Vector2 (rigidbody2D.velocity.x, 0);

		jumpTimer = 0;
		
		// Check if jump button is still pressed
		while(CrossPlatformInput.GetButton ("Jump") && jumpTimer < jumpTime){
			// Jump time proportion
			float proportionCompleted = jumpTimer / jumpTime;
			
			Vector2 thisFrameJumpVector = Vector2.Lerp (new Vector2 (0f, jumpForce), Vector2.zero, proportionCompleted);

			thisFrameJumpVector += Vector2.Lerp ( Vector2.zero,new Vector2 (xComponent, 0f), proportionCompleted);
			

			// Increment the force relative to the time spent with the jump button pressed
			rigidbody2D.AddForce (thisFrameJumpVector);
			
			jumpTimer += Time.deltaTime;
			yield return null;
		}
			isJumping = false;
		
	}
}
