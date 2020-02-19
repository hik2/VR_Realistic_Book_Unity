using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Derive from RWVR_InteractionObject, provides hooks onto controller's input to handle input
public class RWVR_SimpleGrab : RWVR_InteractionObject {
	public bool hideControllerModelOnGrab; //Controller model hidden or not when object picked up
	private Rigidbody rb;	//Cahce rigidbody component

	public override void Awake()
	{
		//Call Awake on base class RWVR_InteractionObject, caches object's Transform component and
		//checks if InteractionObject tag is assigned
		base.Awake ();
		rb = GetComponent<Rigidbody> (); //Store attached Rigidbody component
	}

	//Accepts controller to stick to as param, create FixedJoint component, attach to controller,
	//connect to current InteractionObject
	private void AddFixedJointToController( RWVR_InteractionController controller )
	{
		FixedJoint fx = controller.gameObject.AddComponent<FixedJoint> ();
		fx.breakForce = 20000;
		fx.breakTorque = 20000;
		fx.connectedBody = rb;
	}

	//Controller passed as param relieved of FixedJoint component, connection to objec removed
	//and FixedJoint destroyed
	private void RemoveFixedJointFromController( RWVR_InteractionController controller )
	{
		if (controller.gameObject.GetComponent<FixedJoint> ())
		{
			FixedJoint fx = controller.gameObject.GetComponent<FixedJoint> ();
			fx.connectedBody = null;
			Destroy (fx);
		}
	}

	//Override base OnTriggerWasPressed method
	//Adds FixedJoint when trigger button pressed to interact with object
	public override void OnTriggerWasPressed( RWVR_InteractionController controller )
	{
		base.OnTriggerWasPressed (controller); //Calls base method to initialize controller
		if (hideControllerModelOnGrab) //If set, hide controller model
		{
			controller.HideControllerModel();
		}
		AddFixedJointToController (controller);	//Add FixedJoint to controller
	}

	//Override base OnTriggerWasReleased method
	//Removes FixedJoint, passes controller's velocities to create realistic throwing effect
	public override void OnTriggerWasReleased( RWVR_InteractionController controller )
	{
		base.OnTriggerWasReleased (controller); //Call base method to unassign controller
		if (hideControllerModelOnGrab) //If set, show controller model
		{
			controller.ShowControllerModel ();
		}
		//Pass controller's velocity and angular velocity to object's rigidbody. Object reacts in
		//realistic manner when released
		rb.velocity = controller.velocity;
		rb.angularVelocity = controller.angularVelocity;
		RemoveFixedJointFromController (controller); //Remove FixedJoint
	}
}