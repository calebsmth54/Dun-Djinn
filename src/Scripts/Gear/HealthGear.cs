using UnityEngine;
using DunGen.Components;

namespace DunGen.Game
{
	public class HealthGear : BaseGear
	{
		[Tooltip("How many hearts are we giving to the player?")]
		public int heartsToGive = 2;
		private bool refilledHealth = true; // Only refill the player's health on the first pickup!

		public override void OnPickUp(LNBPlayerCharacter owner)
		{
			base.OnPickUp(owner);

			playerOwner.GetComponent<Health>().AddHealth(heartsToGive * 100, refilledHealth);
			refilledHealth = false;
		}

		public override void OnDrop()
		{
			base.OnDrop();

			playerOwner.GetComponent<Health>().RemoveHealth(heartsToGive * 100);
		}

	}
}