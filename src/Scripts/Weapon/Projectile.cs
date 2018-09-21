using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen.Game
{
	public class Projectile : MonoBehaviour
	{
		[Header("Base Properties")]
		public float launchForce = 500.0f;
		public GameObject ExplosionPrefab;
		public bool DestroyOnImpact = true;

		[Header("Audio")]
		public AudioClip travelNoise;
		public AudioClip hitNoise;

		private AudioSource travelNoiseSource;

		private BaseWeapon weaponOwner;
		private GameObject owner;
		private Rigidbody rb;
		private float damage;
		private float maxLifeTime = 10.0f;

		public void Launch(BaseWeapon newWeaponOwner, GameObject newOwner, Transform shootTransform)
		{
			weaponOwner = newWeaponOwner;
			owner = newOwner;

			damage = weaponOwner.BaseProperties.DamageAmount;

			transform.SetPositionAndRotation(shootTransform.position, shootTransform.rotation);

			rb = GetComponent<Rigidbody>();
			rb.AddForce(transform.forward * launchForce);

			// Disable us after maxLifeTime
			StartCoroutine(DisableAfter());

			// Setup our audio
			if (!travelNoiseSource)
			{
				travelNoiseSource = gameObject.AddComponent<AudioSource>();
				travelNoiseSource.spatialBlend = 1.0f;
			}

			travelNoiseSource.clip = travelNoise;
			travelNoiseSource.loop = true;
			travelNoiseSource.Play();
		}

		public IEnumerator DisableAfter()
		{
			yield return new WaitForSeconds(maxLifeTime);

			gameObject.SetActive(false);
		}

		private void OnCollisionEnter(Collision hit)
		{
			if (!hit.gameObject ||
				hit.gameObject == weaponOwner.gameObject ||
				hit.gameObject == owner ||
				hit.gameObject.CompareTag("Perception") ||
				hit.gameObject.CompareTag("Projectile"))
			{
				return;
			}

			// Attempt to get a health component and apply some damage
			// #TODO: Use IDamageableInterface instead of requesting for component
			ITakeDamage otherHealth = hit.gameObject.GetComponent<ITakeDamage>();
			if (otherHealth != null)
			{
				otherHealth.OnTakeDamage(owner, damage, EDamageType.Fire);
			}

			if (DestroyOnImpact)
			{
				if (ExplosionPrefab)
				{
					Vector3 hitLocation = hit.contacts[0].point;
					Quaternion hitOrientation = Quaternion.FromToRotation(Vector3.up, hit.contacts[0].normal);

					GameObject particle = Instantiate(ExplosionPrefab, hitLocation, hitOrientation);

					travelNoiseSource.Stop();

					AudioSource explosionNoiseSource = particle.GetComponent<AudioSource>();
					if (explosionNoiseSource && hitNoise)
						explosionNoiseSource.PlayOneShot(hitNoise);
				}

				// Destroy ourselves
				gameObject.SetActive(false);
			}
		}
	}
}