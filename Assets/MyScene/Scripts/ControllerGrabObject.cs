using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;

public class ControllerGrabObject : MonoBehaviour
{
	private MegaBookBuilder book;
	private SteamVR_TrackedObject trackedObj;
	private GameObject collidingObject;
	private GameObject objectInHand;
	private SteamVR_Controller.Device Controller
	{
		get { return SteamVR_Controller.Input ((int)trackedObj.index); }
	}
	private Vector2 touchpad;
	private float yControllerRot;
	private float yControllerRotPrev;
	private DateTime prevtime = DateTime.Now;
	private bool wasForward;

	private int turningPage;
	private float turningDelay;
	private int turningCount;

	//Gets book object and controller objects
	void Awake ()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject> ();
		book = FindObjectOfType<MegaBookBuilder> ();
	}

	private void SetCollidingObject(Collider col)
	{
		if (collidingObject || !col.GetComponent<Rigidbody> ())
		{
			return;
		}
		collidingObject = col.gameObject;
	}

	public void OnTriggerEnter(Collider other)
	{
		SetCollidingObject (other);
	}

	public void OnTriggerStay(Collider other)
	{
		SetCollidingObject (other);
	}

	public void OnTriggerExit(Collider other)
	{
		if (!collidingObject)
		{
			return;
		}
		collidingObject = null;
	}

	private void GrabObject()
	{
		objectInHand = collidingObject;
		collidingObject = null;
		var joint = AddFixedJoint ();
		joint.connectedBody = objectInHand.GetComponent<Rigidbody> ();
	}

	//Hides the controller so it looks like the object picked up replaces the controller
	private void ReplaceController()
	{
		trackedObj.gameObject.transform.GetChild(0).gameObject.SetActive(false);
	}

	private FixedJoint AddFixedJoint()
	{
		FixedJoint fx = gameObject.AddComponent<FixedJoint> ();
		fx.breakForce = 20000;
		fx.breakTorque = 20000;
		return fx;
	}

	private void ReleaseObject()
	{
		if (GetComponent<FixedJoint>())
		{
			GetComponent<FixedJoint>().connectedBody = null;
			Destroy(GetComponent<FixedJoint>());
			objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
			objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
		}
		objectInHand = null;
	}

	//Unhides the controller so when object is dropped the controller becomes visible
	private void ReturnController()
	{
		trackedObj.gameObject.transform.GetChild(0).gameObject.SetActive(true);
	}

	private void controlBook()
	{
		//Gets the area of where the touchpad was pressed
		touchpad = Controller.GetAxis (EVRButtonId.k_EButton_SteamVR_Touchpad);
		//Debug.Log(gameObject.name + touchpad);
		turningDelay = 6.0f;
		turningCount = 0;

		//If left part of face button pressed on controller, turn page of book to the left
		if (touchpad.x > -1 && touchpad.x < 0)
		{
			//book.NextPage ();
			turningPage = -1;
			Debug.Log ("Turning page is " + turningPage);
		}

		//If right part of face button pressed on controller, turn page of book to the right
		if (touchpad.x < 1 && touchpad.x > 0)
		{
			//book.PrevPage ();
			turningPage = 1;
			Debug.Log ("Turning page is " + turningPage);
		}
	}

	void Update ()
	{
		if (Controller.GetHairTriggerDown())
		{
			if (collidingObject)
			{
				GrabObject();
				ReplaceController();
			}
		}

		//If trigger let go still hold onto book, switch to grip to release
		//if (Controller.GetHairTriggerUp())
		if( Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip) )
		{
			if (objectInHand)
			{
				ReleaseObject();
				ReturnController();
			}
		}

		if (turningPage > 0) {
			turningCount++;
			if (book.GetPage () < 32){
				if (turningCount >= turningDelay) {
					book.NextPage ();
					turningCount = 0;
				}
			}
			else
			{
				Debug.Log("Reset turning page");
				turningPage = 0;
				turningCount = 0;
			}
		} else if( turningPage < 0 ) {
			turningCount++;
			if( book.GetPage() > 0)
			{
				if (turningCount >= turningDelay) {
					book.PrevPage ();
					turningCount = 0;
				}
			}
			else{
				Debug.Log("Reset turning page");
				turningPage = 0;
				turningCount = 0;
			}
		}

		//If touchpad on controller pressed once (GetPressDown is single page mode)
		//Pressing touchpad again while page is still turning, quickly turns the current page over, so the next page turn be triggered
		if (Controller.GetPressDown (SteamVR_Controller.ButtonMask.Touchpad))
		{
			controlBook ();
		}

		/*GetPress takes precedence over GetPressDown
		//If touchpad on controller pressed continously (GetPress is turning many pages at once)
		if (Controller.GetPress (SteamVR_Controller.ButtonMask.Touchpad))
		{
			controlBook ();
		}*/

		//If controller rotates super fast(does the flick motion), print a (local) rotation of controller
		//Debug.Log( trackedObj.name + "'s y rotation: " + trackedObj.transform.rotation.y );
		yControllerRot = trackedObj.transform.rotation.y;

		if (yControllerRot > 0 && yControllerRot > -0.1)
		{
			DateTime newTime = DateTime.Now;
			double diffTime = newTime.Subtract( prevtime ).TotalSeconds;
			if (diffTime <= (double)1 && !wasForward) {
				Debug.Log (trackedObj.name + " position is forward!!!"); //prints
				prevtime = newTime;
			}
			else if( diffTime > (double)1 && !wasForward )
			{
				prevtime = DateTime.Now;
			}

			//Debug.Log ( "Forward Motion TimeStamp: " + diffTime);

			wasForward = true;
		}
		if (yControllerRot < -0.4 && yControllerRot > -0.7)
		{
			//timestamp
			DateTime newTime = DateTime.Now;
			double diffTime = newTime.Subtract( prevtime ).TotalSeconds;

			if (diffTime <= (double)1 && wasForward ) {
				Debug.Log (trackedObj.name + " is has been flicked to the left!!");	//prints
				prevtime = newTime;

				//Need to flick book from its spine position towards facing the user now
				//Position book into rotation we want, find the best rotation and fix it to that
				book.transform.Rotate( 0,90,0,Space.World );	//world rotation of 45 degrees of y
				//Book rotation set in RWVR_InteractionController, this not used
			}
			else if( diffTime > (double)1 && wasForward )
			{
				prevtime = DateTime.Now;
			}
				
			//Debug.Log ( "Left Motion TimeStamp: " + diffTime);

			wasForward = false;
		}

		//Print rotation of book
		//Debug.Log( book.name + "'s rotation: "  + book.transform.rotation );
	}
}
