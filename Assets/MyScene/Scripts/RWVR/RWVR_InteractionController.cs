using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Valve.VR;

public class RWVR_InteractionController : MonoBehaviour {
	public Transform snapColliderOrigin;	//Save reference to tip of controller
	public GameObject ControllerModel;		//Visual represention of controller

	[HideInInspector]
	public Vector3 velocity;				//Speed and direction of controller
	[HideInInspector]
	public Vector3 angularVelocity;			//Rotation of controller

	//InteractionObject controller is currently interacting with
	//private RWVR_InteractionObject objectBeingInteractedWith;
	private RWVR_SnapToController objectBeingInteractedWith;
	private SteamVR_TrackedObject trackedObj;	//Ref to actual controller

	//To turn book pages
	private Vector2 touchpad;
	private MegaBookBuilder book;

	//To snap book in time
	private float yControllerRot;
	private DateTime prevtime = DateTime.Now;
	private bool wasForward;

	//To turn book pages; not used
	private float yControllerPos;
	private float xControllerPos;
	private float yBookPos;

	//variable is shortcut to steamvr controller class from tracked object
	private SteamVR_Controller.Device Controller	
	{
		get { return SteamVR_Controller.Input ((int)trackedObj.index); }
	}

	//Returns InteractionObject controller is currently interfacing with
	public RWVR_SnapToController InteractionObject
	{
		get{ return objectBeingInteractedWith; }
	}

	//Save reference to TrackedObject component attached to controller
	void Awake()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject> ();
		book = FindObjectOfType<MegaBookBuilder> (); //Get alice in wonderland book
	}

	//Method searches for InteractionObjects in a certain range from the controller's snap collider.
	//Once it finds one, it populates the objectBeingInteractedWith with a reference to it
	private void CheckForInteractionObject()
	{
		//Creates new array of colliders, fill it with all colliders found by OverlapSphere at position
		//and scale of snapColliderOrigin(transparent sphere)
		Collider[] overlappedColliders = Physics.OverlapSphere (snapColliderOrigin.position,
			snapColliderOrigin.lossyScale.x / 2f);
		foreach( Collider overlappedCollider in overlappedColliders )	//Iterate over array
		{
			//If any of the colliders has an InteractionObject tag and is free
			if (overlappedCollider.CompareTag ("InteractionObject") &&
				overlappedCollider.GetComponent<RWVR_SnapToController> ().IsFree ())
			{
				//Save reference to RWVR_InteractionObject
				objectBeingInteractedWith = overlappedCollider.GetComponent<RWVR_SnapToController>();  //<RWVR_InteractionObject> ();
				//Call OnTriggerWasPressed and give it current controller as param
				objectBeingInteractedWith.OnTriggerWasPressed (this);
				return;	//break out of loop once InteractionObject found
			}
		}
	}

	void Update()
	{
		//When trigger pressed, call CheckForInteractionObject
		if (Controller.GetHairTriggerDown ())
		{
			CheckForInteractionObject ();
		}

		//While trigger held down and object being interacted with, call object's OnTriggerIsBeingPressed
		if (Controller.GetHairTrigger ())
		{
			if (objectBeingInteractedWith)
			{
				objectBeingInteractedWith.OnTriggerIsBeingPressed (this);
			}
		}

		//When trigger released and object being interacted with, call object's OnTriggerWasReleased and
		//stop interacting with it
		//Don't let go of the book even if user releases trigger, changed so that pressing grip releases book instead
		//if (Controller.GetHairTriggerUp ())
		if( Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip) )
		{
			if (objectBeingInteractedWith)
			{
				objectBeingInteractedWith.OnTriggerWasReleased (this);
				objectBeingInteractedWith = null;
			}
		}

		//If user does flick gesture (rotates controller to the left very quickly), set book to be facing the user
		yControllerRot = trackedObj.transform.rotation.y;
		//Debug.Log ( "Rotation of controller is: " + yControllerRot );

		if (yControllerRot > 0 && yControllerRot < 0.2)
		{
			DateTime newTime = DateTime.Now;
			double diffTime = newTime.Subtract( prevtime ).TotalSeconds;
			if (diffTime <= (double)1 && !wasForward)
			{
				Debug.Log (trackedObj.name + " position is forward!!!"); //prints
				prevtime = newTime;
			}
			else if( diffTime > (double)1 && !wasForward )
			{
				prevtime = DateTime.Now;
			}
			wasForward = true;
		}

		if (yControllerRot < -0.4 && yControllerRot > -0.7)
		{
			DateTime newTime = DateTime.Now;
			double diffTime = newTime.Subtract( prevtime ).TotalSeconds;
			if (diffTime <= (double)1 && wasForward )
			{
				objectBeingInteractedWith.PositionToController(objectBeingInteractedWith.currentController);
				Debug.Log (trackedObj.name + " is has been flicked to the left!!"); //prints
				prevtime = newTime;
			}
			else if( diffTime > (double)1 && wasForward )
			{
				prevtime = DateTime.Now;
			}
			wasForward = false;
		}

		//If touchpad on controller pressed once (GetPressDown is single page mode)
		//Pressing touchpad again while page is still turning, quickly turns the current page over, so the next page turn be triggered
		if (Controller.GetPressDown (SteamVR_Controller.ButtonMask.Touchpad))
		{
			controlBook ();
		}
	}

	private void controlBook()
	{
		//Gets the area of where the touchpad was pressed
		touchpad = Controller.GetAxis (EVRButtonId.k_EButton_SteamVR_Touchpad);

		//If left part of face button pressed on controller, turn page of book to the left
		if (touchpad.x > -1 && touchpad.x < 0)
		{
			book.NextPage ();
		}

		//If right part of face button pressed on controller, turn page of book to the right
		if (touchpad.x < 1 && touchpad.x > 0)
		{
			book.PrevPage ();
		}
	}

	//Updates velocity and angularVelocity vars
	private void UpdateVelocity()
	{
		velocity = Controller.velocity;
		angularVelocity = Controller.angularVelocity;
	}

	//Calls UpdateVelocity every frame at fixed framrate
	void FixedUpdate()
	{
		UpdateVelocity ();
	}

	//Hide controller by disabling GameObject representing controller
	public void HideControllerModel()
	{
		ControllerModel.SetActive (false);
	}

	//Show controller by enabling GameObject representing controller
	public void ShowControllerModel()
	{
		ControllerModel.SetActive (true);
	}

	//Vibrates controller for a certain amount of time, longer it vibrates, strong the vibration
	//Range between 1 and 3999
	public void Vibrate( ushort strength )
	{
		Controller.TriggerHapticPulse (strength);
	}

	//Switches active InteractionObject to one specified in param
	public void SwitchInteractionObject( RWVR_SnapToController interactionObject )  //( RWVR_InteractionObject interactionObject )
	{
		objectBeingInteractedWith = interactionObject; //Switches active InteractionObject to active one
		//Call function on new InteractionObject and pass to controller
		objectBeingInteractedWith.OnTriggerWasPressed (this);
	}
}