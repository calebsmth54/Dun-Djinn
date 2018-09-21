using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
	//void InitAsInteractable(); // Initialize ourself as an interactable. Great for items that need to turn off their own collision
	//void LeaveInteractable(); // Leave an interactable state. IE allow weapons to function again

	void InteractWith(Interactor interactor); // Whenever we are interacted with


}

