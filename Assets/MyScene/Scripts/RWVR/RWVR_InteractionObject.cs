using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RWVR_InteractionObject : MonoBehaviour {
	protected Transform cachedTranform; //Cached to improve performance
	[HideInInspector]	//makes below variable invisible to inspector window
	//controller the object is currently interacting with
	public RWVR_InteractionController currentController;

	public virtual void OnTriggerWasPressed( RWVR_InteractionController controller )
	{
		currentController = controller;
		Debug.Log ( "OnTriggerWasPressed called" );
	}

	public virtual void OnTriggerIsBeingPressed( RWVR_InteractionController controller )
	{

	}

	public virtual void OnTriggerWasReleased( RWVR_InteractionController controller )
	{
		currentController = null;
	}

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