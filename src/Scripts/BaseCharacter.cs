using UnityEngine;

using DunGen.Components;

namespace DunGen.Game
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Health))]
	public class BaseCharacter : MonoBehaviour, ITakeDamage
	{
		// Basic properties
		public bool IsDead => (healthComp) ? healthComp.IsDead : true;

		// Components
		protected Health healthComp;
		protected Animator animatorComp;

		// Use this for initialization
		public virtual void Start()
		{
			// Setup health component and listen for events
			healthComp = GetComponent<Health>();
			healthComp.OnNoHealth.AddListener(OnNoHealth);

			animatorComp = GetComponentInChildren<Animator>();
		}

		// Update is called once per frame
		void Update()
		{

		}

		public virtual void OnTakeDamage(GameObject attacker, float dmgAmount, EDamageType dmgType = EDamageType.Generic)
		{
			healthComp.TakeDamage(attacker, dmgAmount);
		}

		// This is a notification sent from the character's animation to tell an equipped weapon that it can enable collision/shoot a projectile
		public virtual void NotifyAnimationDeadly()
		{

		}

		public virtual void NotifyAnimationDeadlyOver()
		{

		}

		protected virtual void OnNoHealth()
		{

		}
	}
}