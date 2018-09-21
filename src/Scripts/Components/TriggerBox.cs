#if UNITY_EDITOR
	#define DD_DEBUG
#endif

using UnityEngine;
using UnityEngine.Events;

namespace DunGen.Game
{
	[RequireComponent(typeof(Collider))]
	public class TriggerBox : MonoBehaviour
	{
		[System.Serializable]
		public class EventTriggerEnter : UnityEvent<Collider> { }

		[Tooltip("Broadcast that a collision has happened.")]
		public EventTriggerEnter NotifyOnTriggerEnter = new EventTriggerEnter();

		private void OnTriggerEnter(Collider other)
		{
			NotifyOnTriggerEnter.Invoke(other);
		}

		#if DD_DEBUG
		public void Update()
		{
			BoxCollider collider = GetComponent<BoxCollider>();
			if (collider.enabled)
				DebugExtension.DebugLocalCube(transform, collider.size, Color.red, collider.center);
		}
		#endif
	}
	
}