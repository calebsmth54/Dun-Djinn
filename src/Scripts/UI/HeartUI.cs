using UnityEngine;

namespace DunGen.UI
{
	public class HeartUI : MonoBehaviour
	{
		[Tooltip("This gameObject will be enabled when the player health is over 3/4ths of their health (75%).")]
		public GameObject FullGem;

		[Tooltip("This gameObject will be enabled when the player health is between 3/4ths (75%) && 1/3rd (33.33%).")]
		public GameObject ChippedGem;

		[Tooltip("This gameObject will be enabled when the player health is between 1/3rd (33.33%) && 0 (dead).")]
		public GameObject ShatteredGem;

		[Tooltip("This gameObject will be enabled when the player has no health.")]
		public GameObject GhostGem;

		[Tooltip("This particle is played every time health is changed.")]
		public ParticleSystem ChipEffect;

	}
}