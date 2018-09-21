using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace DunGen.Game
{
	public class LNBPlayerCharacter : BaseCharacter
	{
		// Inventory
		public GameObject startingWeapon;
		public GameObject startingHolsteredWeapon;
		public GameObject rightHand;
		public GameObject leftHand;
		public Interactor interactor;
		public float itemDropForce = 500.0f;

		private BaseWeapon heldWeapon;
		private BaseWeapon savedWeapon;

		private BaseGear equippedGear;

		private bool hasKey = false;
		private GameObject bossKey = null;

		private bool holsterInProgress = false;
		private float holsterTime = 1.0f;

		// Audio
		[Header("Audio")]
		public AudioClip HolsterNoise;
		private AudioSource inventoryNoiseSource;

		// UI

		// Use this for initialization
		public override void Start()
		{
			base.Start();

			interactor = GetComponentInChildren<Interactor>();

			inventoryNoiseSource = gameObject.AddComponent<AudioSource>();

			// Equip and save our starting weapons
			if (startingHolsteredWeapon)
			{
				BaseWeapon newSavedWeapon = Instantiate(startingHolsteredWeapon).GetComponent<BaseWeapon>();
				newSavedWeapon.Pickup(gameObject);
				EquipWeapon(newSavedWeapon);
				//PickupWeapon(newSavedWeapon);
			}

			if (startingWeapon)
			{
				HandleHolster(); // Holster previous weapon

				BaseWeapon newEquippedWeapon = Instantiate(startingWeapon).GetComponent<BaseWeapon>();
				newEquippedWeapon.Pickup(gameObject);
				EquipWeapon(newEquippedWeapon);
				//PickupWeapon(newEquippedWeapon);
			}
		}

		// Update is called once per frame
		void Update()
		{
		}

		//**********************************************
		// Start Equipment
		public void Interact()
		{
			Interactor.InteractableItem interactableItem = interactor.InteractWithWorld();

			if (interactableItem == null || interactableItem.interactable == null)
				return;

			IInteractable interactable = interactableItem.interactable;

			interactable.InteractWith(interactor);
		}

		public void PickupWeapon(BaseWeapon newWeapon)
		{
			// Check if we have a weapon equipped
			if (heldWeapon)
			{
				// Check if our backpack has a weapon
				if (savedWeapon)
				{
					// Drop our equipped weapon
					DropHeldWeapon();
				}

				// No weapon in backpack
				else
				{
					// Put our equipped weapon away
					HolsterWeapon();
				}
			}

			// Equip the weapon directly to our hand
			newWeapon.Pickup(gameObject);
			EquipWeapon(newWeapon);
		}

		private void DropHeldWeapon()
		{
			Vector3 launchForce = transform.forward * itemDropForce;
			GameObject droppedWeapon = rightHand.GetComponentInChildren<BaseWeapon>().gameObject;

			if (!droppedWeapon)
				return;

			GameObject newWorldItem = WorldItem.CreateWorldItem(transform.position, transform.rotation, droppedWeapon, launchForce);

			interactor.DroppedObject(newWorldItem);
		}

		public void HolsterWeapon()
		{
			if (holsterInProgress)
				return;

			rightHand.GetComponent<Animator>().Play("HolsterAndEquip");
			inventoryNoiseSource.PlayOneShot(HolsterNoise);

			StartCoroutine(HolsterWait());
		}

		private IEnumerator HolsterWait()
		{
			holsterInProgress = true;

			yield return new WaitForSeconds(holsterTime / 2.0f); // Wait for the halfway point to actually switch weapons

			HandleHolster();

			yield return new WaitForSeconds(holsterTime / 2.0f);
			holsterInProgress = false;
		}

		private void HandleHolster()
		{
			BaseWeapon swappedWeapon = null;

			// Unhide our saved weapon
			if (savedWeapon)
			{
				savedWeapon.gameObject.SetActive(true);
				swappedWeapon = savedWeapon.GetComponent<BaseWeapon>();
			}

			// Hide our held weapon
			if (heldWeapon)
			{
				heldWeapon.transform.parent = transform;
				heldWeapon.transform.localPosition = transform.forward * -1.0f * 5;
				heldWeapon.gameObject.SetActive(false);
			}

			savedWeapon = heldWeapon;

			EquipWeapon(swappedWeapon);
		}

		private void EquipWeapon(BaseWeapon newWeapon)
		{
			if (!newWeapon)
				return;

			newWeapon.transform.SetParent(rightHand.transform);
			newWeapon.transform.localPosition = Vector3.zero;
			newWeapon.transform.localRotation = Quaternion.identity;

			heldWeapon = newWeapon;

			// Listen/unlisten to weapon firing events
			if (savedWeapon)
			{
				savedWeapon.BaseEvents.OnFireStarted.RemoveListener(OnWeaponStartedFiring);
				savedWeapon.BaseEvents.OnFire.RemoveListener(OnWeaponFired);
				savedWeapon.BaseEvents.OnFireFinished.RemoveListener(OnWeaponFinishedFiring);

				// Notify FPV interface that we are holding a weapon
				FirstPersonView.IFPV_Object fpvObject = savedWeapon.GetComponent<FirstPersonView.IFPV_Object>();
				fpvObject.RemoveAsFirstPersonObject();
			}

			if (heldWeapon)
			{
				heldWeapon.BaseEvents.OnFireStarted.AddListener(OnWeaponStartedFiring);
				heldWeapon.BaseEvents.OnFire.AddListener(OnWeaponFired);
				heldWeapon.BaseEvents.OnFireFinished.AddListener(OnWeaponFinishedFiring);

				// Notify FPV interface that we are holding a weapon
				FirstPersonView.IFPV_Object fpvObject = heldWeapon.GetComponent<FirstPersonView.IFPV_Object>();
				fpvObject.SetAsFirstPersonObject();
			}

			// Start animating with our idle state
			Animator handAnimator = rightHand.GetComponent<Animator>();
			handAnimator.Play(heldWeapon.BaseAnimations.IdleAnimName, 0);
		}

		public void PickUpGear(BaseGear gear)
		{
			// Make room for our new gear piece
			if (equippedGear)
			{
				DropEquippedGear();
			}

			equippedGear = gear;
			equippedGear.OnPickUp(this);

			// TODO: Make events so that the UI can listen to player status updates.
			// Update our UI
			GetComponentInChildren<UI.GearUI>().EquipNewGear(equippedGear.gameObject);
		}

		public void DropEquippedGear()
		{
			equippedGear.OnDrop();

			Vector3 launchForce = transform.forward * itemDropForce;
			GameObject newWorldItem = WorldItem.CreateWorldItem(transform.position, transform.rotation, equippedGear.gameObject, launchForce);
			interactor.DroppedObject(newWorldItem);
		}

		public void PickUpKey(GameObject key)
		{
			hasKey = true;
			bossKey = key;

			// Parent this to our left hand
			bossKey.transform.SetParent(leftHand.transform);
			bossKey.transform.localPosition = Vector3.zero;
			bossKey.transform.localRotation = Quaternion.identity;
		}

		// Returns the boss key object if we current possess one
		public GameObject UseKey()
		{
			if (!hasKey || !bossKey)
				return null;

			hasKey = false;

			return bossKey;
		}

		//**********************************************
		// End Equipment

		// Start Health
		public override void OnTakeDamage(GameObject attacker, float dmgAmount, EDamageType dmgType = EDamageType.Generic)
		{
			base.OnTakeDamage(attacker, dmgAmount, dmgType);

			Debug.Log("Player took " + dmgAmount + " of " + dmgType.ToString() + " damage from " + attacker.name);
		}

		protected override void OnNoHealth()
		{
			base.OnNoHealth();

			Debug.Log("We have no more health!");

			GetComponent<FirstPersonController>().enabled = false; // Disable movement from the FPS controller
			GameManager.GetGameManager().OnPlayerDeath();
		}

		// End Health
		//**********************************************

		//**********************************************
		// Start Weapon
		public Transform GetEyeTransform()
		{
			return GetComponentInChildren<Camera>().transform;
		}

		public void FireWeapon()
		{
			if (holsterInProgress)
				return;

			if (!heldWeapon)
				return;

			heldWeapon.Fire();
		}

		public void StopWeapon()
		{
			GetComponentInChildren<BaseWeapon>().StopFire();
		}

		// This is a notification sent from the character's animation to tell an equipped weapon that it can enable collision/shoot a projectile
		public override void NotifyAnimationDeadly()
		{
			if (heldWeapon)
			{
				Debug.Log("Player was notified by animation of deadly start");

				heldWeapon.NotifyDeadly();
			}
		}

		public override void NotifyAnimationDeadlyOver()
		{
			if (heldWeapon)
			{
				Debug.Log("Player was notified by animation of deadly over");

				heldWeapon.NotifyDeadlyOver();
			}
		}

		private void OnWeaponStartedFiring()
		{
			Animator handAnimator = rightHand.GetComponent<Animator>();

			handAnimator.Play(heldWeapon.BaseAnimations.PrimaryFireAnimName);
			handAnimator.SetBool("Firing", true);
			handAnimator.SetBool("Idling", false);

			// Change speed of animation to scale with weapon fire rate
			float animScale = handAnimator.GetCurrentAnimatorStateInfo(0).length / heldWeapon.BaseProperties.FireRate;
			handAnimator.SetFloat("AttackSpeedMultiplier", animScale);
		}

		private void OnWeaponFired()
		{
		}

		private void OnWeaponFinishedFiring()
		{
			Animator handAnimator = rightHand.GetComponent<Animator>();
			handAnimator.SetBool("Firing", false);
			handAnimator.SetBool("Idling", true);
		}

		// End Weapon
		//**********************************************
	}
}