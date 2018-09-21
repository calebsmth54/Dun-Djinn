using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen.Game
{
	public class BossDoor : MonoBehaviour, IInteractable
	{
		//void InitAsInteractable(); // Initialize ourself as an interactable. Great for items that need to turn off their own collision
		//void LeaveInteractable(); // Leave an interactable state. IE allow weapons to function again

		public void InteractWith(Interactor interactor)
		{
			LNBPlayerCharacter player = interactor.GetOwner().GetComponent<LNBPlayerCharacter>();

			if (!player)
				return;

			GameObject key = player.UseKey();

			if (!key)
				return;

			StartCoroutine(HandleDoorOpenSequence(key));
		}

		private IEnumerator HandleDoorOpenSequence(GameObject key)
		{
			// Get our lock position
			Transform lockPosition = transform.parent.transform.Find("LockPosition");

			key.transform.SetParent(lockPosition);
			key.transform.localPosition = Vector3.zero;
			key.transform.localRotation = Quaternion.identity;

			// play out key animation and wait for it to finish
			Animator keyAnimator = key.GetComponent<Animator>();
			keyAnimator.SetBool("UseKey?", true);
			float animLength = 1.5f;

			yield return new WaitForSeconds(animLength);

			// Open our door!
			Animator doorAnimator = transform.parent.GetComponentInChildren<Animator>();

			Destroy(key);

			doorAnimator.SetBool("Open?", true);
		}
	}
}