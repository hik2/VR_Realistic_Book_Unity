using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Script uses all of InteractionObject capabilities
public class RWVR_SnapToController : MonoBehaviour {
	protected Transform cachedTranform; //Cached to improve performance
	public bool hideControllerModel; //Flag for if controller's model hidden once player grabs object
	//Position added after snapping, object snaps to controller's position by default
	public Vector3 snapPositionOffset;
	public Vector3 snapRotationOffset;	//Same as above, but for rotation
	private Rigidbody rb;	//Cached reference to object's Rigidbody component

	//To control book
	private MegaBookBuilder book; //Alice in Wonderland book
	private float yControllerRot;
	private DateTime prevtime = DateTime.Now;
	private bool wasForward;

	[HideInInspector]	//makes below variable invisible to inspector window
	//controller the object is currently interacting with
	public RWVR_InteractionController currentController;

	//Override base Awake method. Calls base and caches RigidBody component
	public virtual void Awake()
	{
		cachedTranform = transform;	//Cache transform for better performance
		//Check if InteractionObject has proper tag, if not, execute code below
		if( !gameObject.CompareTag("InteractionObject") )	
		{
			//Log warning to inspector to warn of forgotten tag
			Debug.LogWarning( "This InteractionObject does not have the correct tag, setting it now.", gameObject );
			gameObject.tag = "InteractionObject";	//Assign tag so object functions as expected
		}
		rb = GetComponent<Rigidbody> ();
	}

	//Different from SimpleGrab script, doesn't use FixedJoint, makes itself child of controller.
	//Accept controller as param to connect to
	private void ConnectToController( RWVR_InteractionController controller )
	{
		cachedTranform.SetParent (controller.transform); //Set object's parent to be controller

		//Runs in PositionToController function instead
		//Make object's rotation same as controller and add offset
		/*cachedTranform.rotation = controller.transform.rotation; 
		cachedTranform.Rotate (snapRotationOffset);
		//Make object's position same as controller and add offset
		cachedTranform.position = controller.snapColliderOrigin.position;
		cachedTranform.Translate (snapPositionOffset, Space.Self);*/

		rb.useGravity = false; //Disable gravity on object
		rb.isKinematic = true; //Make object kinematic, object not influenced by physics
	}

	public void PositionToController( RWVR_InteractionController controller )
	{
		//Make object's rotation same as controller and add offset
		cachedTranform.rotation = controller.transform.rotation; 
		cachedTranform.Rotate (snapRotationOffset);
		//Make object's position same as controller and add offset
		cachedTranform.position = controller.snapColliderOrigin.position;
		cachedTranform.Translate (snapPositionOffset, Space.Self);
	}

	//Unparents object, resets rigidbody, applies controller velocities
	//Accept controller to release as param
	private void ReleaseFromController( RWVR_InteractionController controller )
	{
		cachedTranform.SetParent (null); //Unparent object
		//Re-enable gravity and make object non-kinematic
		rb.useGravity = true; 
		rb.isKinematic = false;
		rb.velocity = controller.velocity; //Apply controller's velocities to object
		rb.angularVelocity = controller.angularVelocity;
	}

	//Override OnTriggerWasPressed to add snap code
	public virtual void OnTriggerWasPressed( RWVR_InteractionController controller )
	{
		Debug.Log( "RWVR_SnapToController onTriggerWasPressed function running" );
		currentController = controller;
		if (hideControllerModel) //If set, hide controller model
		{
			controller.HideControllerModel ();
		}
		ConnectToController (controller);
	}

	public virtual void OnTriggerIsBeingPressed( RWVR_InteractionController controller )
	{
		
	}

	//Override OnTriggerWasReleased to add release code
	public virtual void OnTriggerWasReleased( RWVR_InteractionController controller )
	{
		currentController = null;
		if (hideControllerModel) //If set, show controller model
		{
			controller.ShowControllerModel ();
		}
		ReleaseFromController( controller ); //Release object from controller
	}

	public bool IsFree()	//Indicates whether or not this object is currently in use by controller
	{
		return currentController == null;
	}

	public virtual void OnDestroy()		//When object gets destroyed, release it from current controller
	{
		if (currentController)
		{
			OnTriggerWasReleased( currentController );
		}
	}
}
