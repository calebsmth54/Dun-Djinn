using System.Collections.Generic;
using UnityEngine;

using DunGen.Components;
using DunGen.Game;

namespace DunGen.UI
{
	public class HealthUI : MonoBehaviour
	{
		[Tooltip("How many hearts can be contained in a row before we add a new row.")]
		public int maxHeartsInRow = 5;

		public GameObject heartsColumn;
		public GameObject heartsRow;
		public HeartUI heartPrefab;

		private LNBPlayerCharacter playerOwner;
		private Health playerHealthComp;
		private List<HeartUI> heartList;
		private int currentHeartIndex = 0; // This is the current heart that we have remaining health in

		// Use this for initialization
		void Start()
		{
			// Make sure our column and rows are valid
			if (!heartsColumn)
			{
				Debug.LogError("No valid column setup for UI!");
				return;
			}

			if (!heartsRow)
			{
				Debug.LogError("No valid row setup for UI!");
				return;
			}

			// Look for our player and health components
			playerOwner = GameManager.GetGameManager().GetPlayerCharacter();
			if (!playerOwner)
			{
				Debug.LogError("No playerCharacter in world!");
				return;
			}

			playerHealthComp = playerOwner.GetComponent<Health>();
			if (!playerHealthComp)
			{
				Debug.LogError("Health UI could not find a health component in the player!");
				return;
			}

			// Listen for updates from our health component
			playerHealthComp.OnAddHealth.AddListener((float amt, bool refill) => { OnHealthAdded(amt); }); // Ignore refill because we'll listen for it on the OnHeal event
			playerHealthComp.OnSubtractHeath.AddListener((float amt) => { OnHealthRemoved(amt); });
			playerHealthComp.OnTakeDamage.AddListener((GameObject attacker, float amt) => { OnHealthChanged(amt, true); }); // For these two listeners we only care about the change in amount
			playerHealthComp.OnHeal.AddListener((float amt) => { OnHealthChanged(amt, false); });
			if (!heartPrefab)
			{
				Debug.LogError("No heart prefab set!");
				return;
			}

			// Create our heart list and add our prefab
			heartList = new List<HeartUI>();
			heartList.Add(heartPrefab);

			// Figure out how many hearts we need to add
			OnHealthAdded(playerHealthComp.GetMaxHealth - 100); // We subtract a hundred from the current amount accounting for the first heart we added
		}

		// Add more hearts when we get 'em
		private void OnHealthAdded(float amount)
		{
			int numOfHearts = (int)(amount / 100.0f);
			int startIndex = heartList.Count;
			int endIndex = startIndex + numOfHearts;

			for (int x = startIndex; x < endIndex; x++)
			{
				// Create a new heart and added it to our row
				GameObject newHeartObj = Instantiate(heartPrefab.gameObject, heartsRow.transform);
				HeartUI newHeartUI = newHeartObj.GetComponent<HeartUI>();

				// Cache this new heart in our list
				heartList.Add(newHeartUI);
			}

			currentHeartIndex = heartList.Count - 1;

			// Update our crystal states
			HeartUI currentHeart = heartList[currentHeartIndex];

			currentHeart.FullGem.SetActive(true);
			currentHeart.ChippedGem.SetActive(false);
			currentHeart.ShatteredGem.SetActive(false);
			currentHeart.GhostGem.SetActive(false);
		}

		// Remove hearts
		private void OnHealthRemoved(float amount)
		{
			int numOfHearts = (int)(amount / 100);
			int lastIndex = heartList.Count - 1;

			// Get rid of the UI heart element
			for (int x = lastIndex; x > lastIndex - numOfHearts; x--)
			{
				heartList[x].transform.SetParent(null);
				Destroy(heartList[x], 0.1f); // This has to be delayed because ownership of the hearts by our list prevents garbage collection
				heartList.RemoveAt(x);
			}

			// Removed it from our cached list as well
			currentHeartIndex = heartList.Count - 1;
		}

		// Update our current heart's current state
		private void OnHealthChanged(float amountChange, bool dmg = false)
		{
			// Don't update health if we have none
			if (currentHeartIndex < 0)
				return;

			HeartUI currentHeart = heartList[currentHeartIndex];

			// Figure out previous health and see if we crossed over state transitions
			float currentHealth = playerHealthComp.GetCurrentHealth - (currentHeartIndex * 100.0f); // TODO: Weird conversion?
			float negateAmount = (dmg) ? 1.0f : -1.0f; // Reversed so we can know the old amount; positive for damage, negative for healing
			float oldHealth = currentHealth + negateAmount * amountChange;

			currentHealth = currentHealth / 100.0f;
			oldHealth = oldHealth / 100.0f;

			if (negateAmount < 0)
				return;

			// 0 health transition
			if (oldHealth > 0.0f && currentHealth <= 0.0f)
			{
				currentHeart.FullGem.SetActive(false);
				currentHeart.ChippedGem.SetActive(false);
				currentHeart.ShatteredGem.SetActive(false);
				currentHeart.GhostGem.SetActive(true);

				// Shatter effect
				currentHeart.ChipEffect.Play();

				currentHeartIndex--;
			}

			// 1/3rd Damaged Gem transition
			else if (oldHealth > 1.0f / 3.0f && currentHealth <= 1.0f / 3.0f)
			{
				currentHeart.FullGem.SetActive(false);
				currentHeart.ChippedGem.SetActive(false);
				currentHeart.ShatteredGem.SetActive(true);
				currentHeart.GhostGem.SetActive(false);

				// Shatter effect
				currentHeart.ChipEffect.Play();
			}

			// 2/3rd Damaged Gem transition
			else if (oldHealth > 3.0f / 4.0f && currentHealth <= 3.0f / 4.0f)
			{
				currentHeart.FullGem.SetActive(false);
				currentHeart.ChippedGem.SetActive(true);
				currentHeart.ShatteredGem.SetActive(false);
				currentHeart.GhostGem.SetActive(false);

				// Shatter effect
				currentHeart.ChipEffect.Play();
			}
		}
	}
}