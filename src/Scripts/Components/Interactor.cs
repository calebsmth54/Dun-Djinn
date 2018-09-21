using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour
{
	private SphereCollider itemDetector;
	private Dictionary<int, InteractableItem> selectableInteractables = new Dictionary<int, InteractableItem>();

	public class InteractableItem
	{
		public GameObject gameObject;
		public IInteractable interactable;
		public int indexInList;

		public InteractableItem(GameObject interactableObject, IInteractable interactableComp)
		{
			gameObject = interactableObject;
			interactable = interactableComp;
		}
	}

	// Use this for initialization
	void Start ()
	{
		itemDetector = GetComponent<SphereCollider>(); 
	}
	
	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Interactable"))
			return;

		InteractableItem newInteractable = new InteractableItem(other.gameObject, other.gameObject.GetComponent<IInteractable>());

		selectableInteractables.Add(other.gameObject.GetInstanceID(), newInteractable);
		newInteractable.indexInList = selectableInteractables.Count-1; // Store our index for fast removal

		Debug.Log("Found an interactable item! " + other.gameObject.name + " Current Count: " + selectableInteractables.Count);
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.CompareTag("Interactable"))
			return;

		int instanceId = other.gameObject.GetInstanceID();
		selectableInteractables.Remove(instanceId);
		Debug.Log("Lost an interactable item! " + other.gameObject.name + " Current Count: " + selectableInteractables.Count);
	}

	public InteractableItem InteractWithWorld()
	{
		// Look for closest interactable
		InteractableItem bestInteractable = null;
		float bestInteractableScore = 0.0f;
		foreach(InteractableItem interactableItem in selectableInteractables.Values)
		{
			Vector3 vecToInteractable = interactableItem.gameObject.transform.position - transform.position;
			float distanceToInteractable = 1.0f/Vector3.SqrMagnitude(vecToInteractable);
			float orientationToInteractable = Vector3.Dot(transform.forward, vecToInteractable.normalized);
			float thisScore = distanceToInteractable * orientationToInteractable;

			if (thisScore > bestInteractableScore)
			{
				bestInteractable = interactableItem;
				bestInteractableScore = thisScore;
			}
		}

		return bestInteractable;
	}

	public void PickedUpObject(GameObject pickedupObject)
	{
		int instanceId = pickedupObject.GetInstanceID();
		selectableInteractables.Remove(instanceId);
		Debug.Log("Found an interactable item! " + pickedupObject.name + " Current Count: " + selectableInteractables.Count);
	}

	public void DroppedObject(GameObject droppedObject)
	{
		if (!droppedObject.CompareTag("Interactable"))
			return;

		InteractableItem newInteractable = new InteractableItem(droppedObject, droppedObject.GetComponent<IInteractable>());

		selectableInteractables.Add(droppedObject.GetInstanceID(), newInteractable);
		newInteractable.indexInList = selectableInteractables.Count - 1; // Store our index for fast removal

		Debug.Log("Found an interactable item! " + droppedObject.name + " Current Count: " + selectableInteractables.Count);
	}

	public GameObject GetOwner()
	{
		return transform.parent.gameObject;
	}
}

