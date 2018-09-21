using System.Collections.Generic;
using UnityEngine;

namespace DunGen.Game
{
	public class MeleeWeapon : BaseWeapon
	{
		[Header("Melee Properties")]
		[Tooltip("A list of colliders that damage things they come into contact with when the weapon fires")]
		public TriggerBox[] DamageColliders;

		[Header("Melee Audio")]
		[Tooltip("Sound played when the weapon hits something")]
		public AudioClip HitNoiseClip;

		public override void Start()
		{
			base.Start();

			// Go through our list of colliders and listen in on their collision events
			for (int x = 0; x < DamageColliders.Length; x++)
			{
				TriggerBox currCollider = DamageColliders[x];
				if (currCollider)
				{
					DamageColliders[x].NotifyOnTriggerEnter.AddListener(OnHit);
					DamageColliders[x].GetComponent<Collider>().enabled = false;
				}
			}
		}

		public override void NotifyDeadly()
		{
			base.NotifyDeadly();

			for (int x = 0; x < DamageColliders.Length; x++)
			{
				TriggerBox currCollider = DamageColliders[x];
				if (currCollider)
				{
					DamageColliders[x].GetComponent<Collider>().enabled = true;
				}
			}
		}

		public override void NotifyDeadlyOver()
		{
			base.NotifyDeadlyOver();

			for (int x = 0; x < DamageColliders.Length; x++)
			{
				TriggerBox currCollider = DamageColliders[x];
				if (currCollider)
				{
					DamageColliders[x].GetComponent<Collider>().enabled = false;
				}
			}
		}

		protected void OnHit(Collider otherCollider)
		{
			// CJS - TODO: Find a more elegant way to ignore collisions with rooms
			if (otherCollider.gameObject == weaponOwner || otherCollider.gameObject.CompareTag("Weapon") || otherCollider.gameObject.CompareTag("Room"))
			{
				return;
			}

			Debug.Log(weaponOwner.name + " hit " + otherCollider.gameObject.name);

			// Do damage to our opponent
			ITakeDamage targetHealth = otherCollider.GetComponent<ITakeDamage>();
			if (targetHealth != null)
			{
				targetHealth.OnTakeDamage(weaponOwner, BaseProperties.DamageAmount, EDamageType.Slash);
			}

			// Interrupt our firing sound to play a contact hit sound
			BaseAudioProperties.FireNoiseSource.Stop();
			BaseAudioProperties.FireNoiseSource.clip = HitNoiseClip;
			BaseAudioProperties.FireNoiseSource.Play();
		}
	}
}