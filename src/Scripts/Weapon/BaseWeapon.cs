using UnityEngine;
using UnityEngine.Events;

using DunGen.Utility;

namespace DunGen.Game
{
	//[RequireComponent(typeof(Rigidbody))]
	public class BaseWeapon : MonoBehaviour
	{
		#region Public Members

		/* CJS - This seems redundant now that we have optional animations set 
		// This changes our firing type
		// Melee: Activates kill boxes for close range, aoe, or special attacks.
		// Ranged: Launches a projectile.
		public enum WeaponType
		{
			Melee = 0,
			Ranged
		}*/

		//
		// Weapon State Machine ENUM
		//
		protected enum EWeaponState
		{
			// NULL = 0,
			IDLE = 1,
			WINDUP,
			FIRING,
			COOLDOWN,
		}

		[System.Serializable]
		public class BaseWeaponProperties
		{
			[Tooltip("Controls how much damage is applied to anything the weapon, or its children touch.")]
			public float DamageAmount = 25.0f;

			[Tooltip("True for automatic weapons.\nFalse for single shot weapons.")]
			public bool RepeatCycleFire = false;

			[Tooltip("Controls rate of fire in seconds.")]
			public float FireRate = 0.0f;			

			[Tooltip("A delay in seconds before a weapon can begin firing.")]
			public float WindupDelay = 0.0f;

			[Tooltip("How much heat the weapon generates every second it fires.")]
			public float HeatRate = 0.0f;

			[Tooltip("How much heat the weapon loses every second it doesn't fires.")]
			public float CooldownRate = 0.0f;

			[Tooltip("The max heat the weapon can accumulate before it over heats.")]
			public float MaxHeat = 0.0f;
		}

		//[Header("Base Properties", order = 0)]
		[Tooltip("Properties for all of the weapons base attributes, such as fire rate timing, damage, etc.")]
		public BaseWeaponProperties BaseProperties = new BaseWeaponProperties();

		// Pluggable sound clips and audio sources for easy to edit audio properties
		//[System.Serializable]
		public class BaseWeaponAudio
		{
			[Tooltip("The noise played each time this weapon is fired.")]
			public AudioClip FireNoise;

			[Tooltip("Audio properties for Fire Noies.")]
			public AudioSource FireNoiseSource;
		}

		//[Header("Base Audio", order = 10)]
		[Tooltip("Properties for all of the weapons base attributes, such as fire rate timing, damage, etc.")]
		public BaseWeaponAudio BaseAudioProperties = new BaseWeaponAudio();

		// Notifications for various weapon states/events
		[System.Serializable]
		public class BaseWeaponEvents
		{
			[Tooltip("Event signaled when the weapon fires for the first time.")]
			public UnityEvent OnFireStarted = new UnityEvent();

			[Tooltip("Event signaled each time the weapon fires again.")]
			public UnityEvent OnFire = new UnityEvent();

			[Tooltip("Event signaled When the weapon stops firing.")]
			public UnityEvent OnFireFinished = new UnityEvent();

			[Tooltip("Event signaled When the weapon overheats.")]
			public UnityEvent OnOverHeat = new UnityEvent();

			[Tooltip("Event signaled When the weapon finishes overheating.")]
			public UnityEvent OnCooledDown = new UnityEvent();
		}

		//[Header("Base Events", order = 15)]
		[Tooltip("Signals when the weapon enters certain states. I.E. Firing, cooling down, etc.")]
		public BaseWeaponEvents BaseEvents = new BaseWeaponEvents();

		// Animations that can be played the owning character
		[System.Serializable]
		public class BaseWeaponAnimations
		{
			[Tooltip("Idle animation state name.")]
			public string IdleAnimName;

			[Tooltip("Windup animation state name.")]
			public string WindupAnimName;

			[Tooltip("Primary Fire animation state name.")]
			public string PrimaryFireAnimName;

			[Tooltip("Cooldown animation state name.")]
			public string CooldownAnimName;
		}

		//[Header("Base Animations", order = 15)]
		[Tooltip("Signals when the weapon enters certain states. I.E. Firing, cooling down, etc.")]
		public BaseWeaponAnimations BaseAnimations = new BaseWeaponAnimations();

		// Are we in the firing state?
		public bool isFiring { get { return primaryFireFSM.getState.getID == EWeaponState.FIRING; } }

		#endregion

		#region Private Members

		
		//protected WeaponType type; // Set this in the derived class

		protected GameObject weaponOwner; // Ignore this during collision detection
		private bool triggeredPulled = false;
		private float currentHeat = 0.0f;

		private WeaponFSM primaryFireFSM;

		private float fireRateEndCheckRatio = 0.75f; // At what percent of time do we check for end fire transitions

		#endregion

		// Use this for initialization
		public virtual void Start()
		{
			tag = "Weapon";

			// Initialize our state machine
			primaryFireFSM = new WeaponFSM("FSM_PRIMARYFIRE", this);

			// CJS - Logging player activity only now.
			// Useful for displaying only one FSM's state.
			/*#if UNITY_EDITOR
				if (weaponOwner && weaponOwner.CompareTag("Player"))
				primaryFireFSM.LogThis = true;
			#endif*/

			// Start the first idling
			primaryFireFSM.Start(EWeaponState.IDLE);

			// Initialize our audio components
			if (!BaseAudioProperties.FireNoiseSource)
			{
				BaseAudioProperties.FireNoiseSource = gameObject.AddComponent<AudioSource>();
			}
		}

		public virtual void Update()
		{
			// Update our state
			primaryFireFSM.Update();
		}

		// Call this function to notify melee weapons to enable collision or for ranged weapons to launch projectiles
		public virtual void NotifyDeadly()
		{
			BaseAudioProperties.FireNoiseSource.clip = BaseAudioProperties.FireNoise;
			BaseAudioProperties.FireNoiseSource.Play();
		}

		// Reverses the above
		public virtual void NotifyDeadlyOver()
		{

		}

		// Use this to change ownership of a weapon. I.E From the world to the player
		public virtual void Pickup(GameObject newOwner)
		{
			weaponOwner = newOwner;
		}

		public virtual void Equip(GameObject owner)
		{
			weaponOwner = owner;
		}

		public virtual void Unequip()
		{
			weaponOwner = null;
		}

		public bool CanFire()
		{
			if (primaryFireFSM.getState.getID != EWeaponState.COOLDOWN)
				return true;

			return false;
		}

		// Attempt to fire this weapon
		public void Fire()
		{
			triggeredPulled = true;
		}

		// Stop fire attempts
		public void StopFire()
		{
			triggeredPulled = false;
		}

		// Override this to launch a projetile and/or swing a weapon
		protected virtual void OnFire(EWeaponState prevState)
		{
			if (prevState != EWeaponState.FIRING)
			{
				BaseEvents.OnFireStarted.Invoke();
			}

			BaseEvents.OnFire.Invoke();
		}

		protected virtual void OnFireEnd(EWeaponState nextState)
		{
			BaseEvents.OnFireFinished.Invoke();
		}

		protected virtual void ApplyCooldown()
		{
			currentHeat -= Mathf.Max(0.0f, BaseProperties.CooldownRate * Time.deltaTime);
		}

		protected virtual void ApplyHeat()
		{
			currentHeat += Mathf.Min(BaseProperties.HeatRate * Time.deltaTime);
		}

		//
		// Weapon State Machine
		//
		private class WeaponFSM : FSM<EWeaponState, BaseWeapon>
		{
			private IdleState idleState;
			private WindupState windupState;
			private FireState fireState;
			private CooldownState cooldownState;

			public WeaponFSM(string name, BaseWeapon weapon) : base(name, weapon)
			{
				if (!weapon)
				{
					// TODO: Add log
					LogHalt("BaseWeapon object null!");
					Halt();
				}

				idleState = new IdleState(this, EWeaponState.IDLE, "STATE_IDLE", weapon);
				windupState = new WindupState(this, EWeaponState.WINDUP, "STATE_WINDUP", weapon);
				fireState = new FireState(this, EWeaponState.FIRING, "STATE_FIRING", weapon);
				cooldownState = new CooldownState(this, EWeaponState.COOLDOWN, "STATE_COOLDOWN", weapon);

				idleState.AddTransition(new Tran_IdleToWindup(windupState, weapon));

				windupState.AddTransition(new Tran_WindupToIdle(idleState, weapon));
				windupState.AddTransition(new Tran_WindupToFire(fireState, weapon));

				fireState.AddTransition(new Tran_FireToCooldown(cooldownState, weapon));
				fireState.AddTransition(new Tran_FireToIdle(idleState, weapon));
				fireState.AddTransition(new Tran_FireToFire(fireState, weapon));

				AddState(idleState);
				AddState(windupState);
				AddState(fireState);
				AddState(cooldownState);
			}

			//
			// IDLE STATE
			//
			// Enter: Start idle animations
			//
			// Update: Add heat over time
			//
			private class IdleState : State<EWeaponState, BaseWeapon>
			{
				public IdleState(WeaponFSM parent, EWeaponState ID, string prettyID, BaseWeapon weapon) : base(parent, ID, prettyID, weapon)
				{
				}

				public override void Enter(EWeaponState prevState, BaseWeapon stateActor)
				{
					base.Enter(prevState, stateActor);

					// TODO: Notify animations to begin idle animation
				}

				public override void Update(BaseWeapon weapon)
				{
					base.Update(weapon);

					weapon.ApplyCooldown();
				}
			}

			// 
			// IDLE => WINDUP Transition
			//
			// Condition: Trigger Pulled?
			//
			private class Tran_IdleToWindup : StateTransition<EWeaponState, BaseWeapon>
			{
				public Tran_IdleToWindup(State<EWeaponState, BaseWeapon> stateToEnter, BaseWeapon weapon) : base(stateToEnter)
				{
				}

				public override bool CanTransition(BaseWeapon weapon)
				{
					if (base.CanTransition(weapon))
					{
						if (weapon.triggeredPulled)
							return true;
					}

					return false;
				}
			}

			//
			// WINDUP STATE
			//
			// Delay transitions until the windup delay has elapsed
			//
			// Enter: Start idle animations
			//
			// Update: Add heat over time
			//
			private class WindupState : State<EWeaponState, BaseWeapon>
			{
				public WindupState(WeaponFSM parent, EWeaponState ID, string prettyID, BaseWeapon weapon) : base(parent, ID, prettyID, weapon)
				{
				}
			}

			// 
			// WINDUP => Idle Transition
			//
			// Condition: Trigger Released?
			//
			private class Tran_WindupToIdle : StateTransition<EWeaponState, BaseWeapon>
			{
				public Tran_WindupToIdle(State<EWeaponState, BaseWeapon> stateToEnter, BaseWeapon weapon) : base(stateToEnter)
				{
				}

				public override bool CanTransition(BaseWeapon weapon)
				{
					if (base.CanTransition(weapon))
					{
						if (!weapon.triggeredPulled)
							return true;
					}

					return false;
				}
			}

			// 
			// WINDUP => Idle Transition
			//
			// Condition: Trigger Released?
			//
			private class Tran_WindupToFire : StateTransition<EWeaponState, BaseWeapon>
			{
				public Tran_WindupToFire(State<EWeaponState, BaseWeapon> stateToEnter, BaseWeapon weapon) : base(stateToEnter, weapon.BaseProperties.WindupDelay)
				{
				}

				public override bool CanTransition(BaseWeapon weapon)
				{
					if (base.CanTransition(weapon))
					{
						if (weapon.triggeredPulled)
							return true;
					}

					return false;
				}
			}

			//
			// FIRE STATE
			//
			// Enter: Signal fire + make noise + launch projectile
			//
			// Update: Add heat over time
			//
			// Exit: Signal end firing
			//
			private class FireState : State<EWeaponState, BaseWeapon>
			{
				public FireState(WeaponFSM parent, EWeaponState ID, string prettyID, BaseWeapon weapon) : base(parent, ID, prettyID, weapon)
				{
				}

				public override void Enter(EWeaponState prevState, BaseWeapon weapon)
				{
					base.Enter(prevState, weapon);

					// Update this each time in case it changes value in another state
					DelayTransitionCheckTime = weapon.BaseProperties.FireRate * weapon.fireRateEndCheckRatio;

					weapon.OnFire(prevState);
				}

				public override void Update(BaseWeapon weapon)
				{
					base.Update(weapon);

					weapon.ApplyHeat();
				}

				public override void Exit(EWeaponState nextState, BaseWeapon weapon)
				{
					base.Exit(nextState, weapon);

					// If the weapon is not going to another firing loop, signal that firing has finished
					if (nextState != EWeaponState.FIRING)
						weapon.OnFireEnd(nextState);
				}
			}

			// 
			// Fire => Cooldown Transition
			//
			// Condition: Overheating?
			//
			private class Tran_FireToCooldown : StateTransition<EWeaponState, BaseWeapon>
			{
				public Tran_FireToCooldown(State<EWeaponState, BaseWeapon> stateToEnter, BaseWeapon weapon) : base(stateToEnter)
				{
				}

				public override bool CanTransition(BaseWeapon weapon)
				{
					if (base.CanTransition(weapon))
					{
						if (weapon.BaseProperties.HeatRate > 0.0f &&
							weapon.currentHeat >= weapon.BaseProperties.MaxHeat)
							return true;
					}

					return false;
				}
			}

			// 
			// Fire => Idle Transition
			//
			// Condition: Trigger released?
			//
			private class Tran_FireToIdle : StateTransition<EWeaponState, BaseWeapon>
			{
				public Tran_FireToIdle(State<EWeaponState, BaseWeapon> stateToEnter, BaseWeapon weapon) : base(stateToEnter)
				{
				}

				public override bool CanTransition(BaseWeapon weapon)
				{
					if (base.CanTransition(weapon))
					{
						if (!weapon.triggeredPulled)
							return true;
					}

					return false;
				}
			}

			// 
			// Fire => Fire Transition
			//
			// Condition: Trigger held and automatic weapon?
			//
			private class Tran_FireToFire : StateTransition<EWeaponState, BaseWeapon>
			{
				public Tran_FireToFire(State<EWeaponState, BaseWeapon> stateToEnter, BaseWeapon weapon) : base(stateToEnter)
				{
				}

				public override bool CanTransition(BaseWeapon weapon)
				{
					if (base.CanTransition(weapon))
					{
						if (weapon.BaseProperties.RepeatCycleFire && weapon.triggeredPulled)
							return true;
					}

					return false;
				}
			}

			//
			// Cooldown STATE
			//
			// Delay transitions until the windup delay has elapsed
			//
			// Enter: Start idle animations
			//
			// Update: Add heat over time
			//
			private class CooldownState : State<EWeaponState, BaseWeapon>
			{
				public CooldownState(WeaponFSM parent, EWeaponState ID, string prettyID, BaseWeapon weapon) : base(parent, ID, prettyID, weapon)
				{
				}

				public override void Enter(EWeaponState prevState, BaseWeapon weapon)
				{
					base.Enter(prevState, weapon);

					// TODO: Notify animations to begin idle animation
				}

				public override void Update(BaseWeapon weapon)
				{
					base.Update(weapon);

					weapon.ApplyCooldown();
				}
			}

			// 
			//	Cooldown => Idle Transition
			//
			//	Condition: heat level 0?
			//
			private class Tran_CooldownToIdle : StateTransition<EWeaponState, BaseWeapon>
			{
				public Tran_CooldownToIdle(State<EWeaponState, BaseWeapon> stateToEnter, BaseWeapon weapon) : base(stateToEnter)
				{
				}

				public override bool CanTransition(BaseWeapon weapon)
				{
					if (base.CanTransition(weapon))
					{
						if (weapon.currentHeat >= 0.0f)
							return true;
					}

					return false;
				}
			}
		}
	}
}
