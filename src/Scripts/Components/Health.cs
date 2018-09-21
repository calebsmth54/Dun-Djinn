using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TakeDamageEvent : UnityEvent<GameObject, float> { }

[System.Serializable]
public class HealEvent : UnityEvent<float> { }

[System.Serializable]
public class AddHealthEvent : UnityEvent<float, bool> { }

[System.Serializable]
public class SubtractHealthEvent : UnityEvent<float> { }

[System.Serializable]
public class NoHealthEvent : UnityEvent { };

namespace DunGen.Components
{
	public class Health : MonoBehaviour
	{

		public float startingHealth; // Starting health amount
		private float currentHealth; // Current health amount
		private float maxHealth; // Maximum allowed health

		public float GetCurrentHealth { get { return currentHealth; } }
		public float GetMaxHealth { get { return maxHealth; } }
		public bool IsDead { get { return (currentHealth < 0.0f) ? true : false; } }

		// Controls how long we ignore incoming damage
		public float invincibleTime = 0.0f;
		private bool isInvincible = false;
		public bool GetIsInvincible { get { return isInvincible; } }

		// Public events for notifying listeners of damage/death events
		public TakeDamageEvent OnTakeDamage;
		public HealEvent OnHeal;
		public AddHealthEvent OnAddHealth;
		public SubtractHealthEvent OnSubtractHeath;
		public NoHealthEvent OnNoHealth;

		// Use this for initialization
		void Awake()
		{
			if (OnTakeDamage == null)
				OnTakeDamage = new TakeDamageEvent();

			if (OnHeal == null)
				OnHeal = new HealEvent();

			if (OnAddHealth == null)
				OnAddHealth = new AddHealthEvent();

			if (OnSubtractHeath == null)
				OnSubtractHeath = new SubtractHealthEvent();

			if (OnNoHealth == null)
				OnNoHealth = new NoHealthEvent();

			maxHealth = startingHealth;
			currentHealth = maxHealth;
		}

		// Whenever we take damage
		public virtual void TakeDamage(GameObject Attacker, float dmgAmount)
		{
			if (isInvincible || IsDead)
				return;

			// Apply damage
			currentHealth = currentHealth - dmgAmount;

			// Notify our parent that we took damage
			if (OnTakeDamage != null)
			{
				OnTakeDamage.Invoke(Attacker, dmgAmount);
			}

			// No health left?
			if (currentHealth <= 0.0f)
			{
				// Notify our parent that we're dead!
				if (OnNoHealth != null)
				{
					OnTakeDamage.Invoke(Attacker, dmgAmount);
					OnNoHealth.Invoke();
				}
			}

			// Start a timer to timeout our invincibility
			isInvincible = true;
			Invoke("OnInvincibilityEnd", invincibleTime);
		}

		private void OnInvincibilityEnd()
		{
			isInvincible = false;
		}

		// Used when healing our healhth, limited by maxHealth
		public virtual void Heal(float healAmount)
		{
			currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);

			OnHeal.Invoke(healAmount);
		}

		// Used to update our maximum health amount
		public virtual void AddHealth(float additionalHealth, bool healToFull = false)
		{
			maxHealth += additionalHealth;

			OnAddHealth.Invoke(additionalHealth, healToFull);

			// With our new health pool, are we completely healing up?
			if (healToFull)
			{
				Heal(maxHealth);
			}
		}

		// Used to update our maximum health amount
		public virtual void RemoveHealth(float subtractedHealth)
		{
			maxHealth = maxHealth - subtractedHealth;

			OnSubtractHeath.Invoke(subtractedHealth);

			// Did we die from having this much health removed?
			if (maxHealth <= 0.0f)
			{
				OnNoHealth.Invoke();
			}
		}
	}
}