using UnityEngine;
using UnityEngine.Events;

namespace DunGen.Utility
{
	// Dispatchers of animation events to selectable functions
	public class AnimNotifier : MonoBehaviour
	{
		[Tooltip("Use the array index value to signal that specific event in the animation event window")]
		public UnityEvent[] EventList;

		// ID is the index of the event in the above array
		public void NotifyAnimation(int eventID)
		{
			if (eventID < 0 || eventID > EventList.Length)
			{
				Debug.Log("AnimNotifier tried to trigger an event that was outside of the EventList's range!");
				return;
			}

			EventList[eventID].Invoke();
		}

		public void OnDestroy()
		{
			for (int x = 0; x < EventList.Length; x++)
				EventList[x].RemoveAllListeners();
		}
	}
}