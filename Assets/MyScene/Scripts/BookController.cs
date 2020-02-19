using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class BookController : MonoBehaviour {
	private MegaBookBuilder book;

	// Use this for initialization
	void Start ()
	{
		//Gets book object
		book = FindObjectOfType<MegaBookBuilder> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		//If Z key pressed down, turn page of book to the left
		if (Input.GetKeyDown (KeyCode.Z))
		{
			book.NextPage ();
		}

		//If X key pressed down, turn page of book to the right
		if (Input.GetKeyDown (KeyCode.X))
		{
			book.PrevPage ();
		}
	}
}
