using UnityEngine;
using UnityEngine.Events;

namespace DunGen.AI
{
	[System.Serializable]
	public class OnSeenTargetEvent : UnityEvent<GameObject> { }

	[System.Serializable]
	public class OnHeardSoundEvent : UnityEvent<NoiseMaker> { }

	// Controls perception of Audio as well as sight
	public class Perception : MonoBehaviour
	{
		// Visual perception
		[Header("Visual Perception")]
		public float fieldOfView = 90.0f;

		[Tooltip("The amount of time it takes for us to lose vision of something that isn't in our FOV")]
		public float LoseTargetTime = 2.0f;

		public OnSeenTargetEvent OnSeenTarget;
		public UnityEvent OnLostTarget;

		public bool seesTarget { get; set; }

		public GameObject seenTarget { get { return target; } }
		private GameObject target;

		// Audatory Perception
		[Header("Audatory Perception")]
		public OnHeardSoundEvent OnHeardSound;

		public NoiseMaker lastHeardNoise;

		// Use this for initialization
		void Start()
		{
			if (OnSeenTarget == null)
				OnSeenTarget = new OnSeenTargetEvent();

			if (OnLostTarget == null)
				OnLostTarget = new UnityEvent();

			if (OnHeardSound == null)
				OnHeardSound = new OnHeardSoundEvent();
		}

		// Update is called once per frame
		void Update()
		{
			// Check if we see the player
			if (target)
			{
				// Is the player within our field of view?
				Vector3 toTarget = target.transform.position - transform.position;
				toTarget.y = 0;
				float angularToTarget = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(transform.forward, toTarget.normalized));

				if (!seesTarget && angularToTarget <= fieldOfView)
				{
					GainSightOfTarget();
				}
				else if (seesTarget && angularToTarget > fieldOfView)
				{
					LoseSightOfTarget();
				}
			}
		}

		private void GainSightOfTarget()
		{
			seesTarget = true;
			OnSeenTarget.Invoke(target);
			Debug.Log("Saw player!");
		}

		private void LoseSightOfTarget()
		{
			seesTarget = false;
			OnLostTarget.Invoke();
			Debug.Log("lost sight of player!");
		}

		private void OnTriggerEnter(Collider other)
		{
			// Player entered our perception!
			if (other.tag == "Player")
			{
				target = other.gameObject;

				// Cancel losing our target because we regained sight
				CancelInvoke("LoseSightOfTarget");
			}

			// A sound has entered our perception!
			if (other.CompareTag("Perception"))
			{
				NoiseMaker newNoise = other.GetComponent<NoiseMaker>();
				if (newNoise)
				{
					lastHeardNoise = newNoise;
					OnHeardSound.Invoke(lastHeardNoise);
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			// Player entered our perception!
			if (other.tag == "Player")
			{
				// Target moved outside of our perception range
				if (seenTarget)
				{
					Invoke("LoseSightOfTarget", LoseTargetTime);
				}

				target = null;
			}
		}
	}
}