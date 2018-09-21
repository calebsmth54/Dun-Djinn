using UnityEngine;
using UnityEngine.AI;

using DunGen.AI;

namespace DunGen.Game
{
	// TODO: Make attacks modular and have multiple slots per monster
	/*
	[System.Serializable]
	public class EnemyAttack
	{
		public BaseWeapon weapon;

		private bool attacking = false;
		public bool isAttacking { get { return attacking; } }

		public void InitAttack(GameObject owner)
		{
			weapon.OnFireFinished.AddListener(OnAttackEnd);
			weapon.Equip(owner);
		}

		public void Attack()
		{

		}

		public void OnAttackEnd()
		{

		}
	}
	*/

	[RequireComponent(typeof(NavMeshAgent))]
	public class BaseEnemyCharacter : BaseCharacter
	{
		// AI and navigation
		private NavMeshAgent agent;

		// Attacks and weapons
		public BaseWeapon primaryWeapon;
		private bool attacking = false;
		public bool IsAttacking { get { return attacking; } }

		// Audio
		[Header("Audio")]
		public AudioClip IdleNoise;
		public AudioClip HitNoise;
		public AudioClip DeathNoise;

		public AudioSource IdleNoiseSource;
		public AudioSource HitNoiseSource;
		public AudioSource DeathNoiseSource;

		// Use this for initialization
		public override void Start()
		{
			base.Start();

			// Grab nav agent
			agent = gameObject.GetComponent<NavMeshAgent>();

			// Grab weapon
			//primaryWeapon = gameObject.GetComponentInChildren<BaseWeapon>();
			if (primaryWeapon)
			{
				primaryWeapon.BaseEvents.OnFireFinished.AddListener(AttackEnd);
				primaryWeapon.Equip(gameObject);
			}

			// Setup audio
			if (!IdleNoiseSource)
				IdleNoiseSource = gameObject.AddComponent<AudioSource>();

			IdleNoiseSource.clip = IdleNoise;
			IdleNoiseSource.loop = true;
			IdleNoiseSource.spatialBlend = 1.0f;
			IdleNoiseSource.maxDistance = 100.0f;
			IdleNoiseSource.Play();

			if (!HitNoiseSource)
				HitNoiseSource = gameObject.AddComponent<AudioSource>();

			HitNoiseSource.clip = HitNoise;
			HitNoiseSource.loop = false;

			if (!DeathNoiseSource)
				DeathNoiseSource = gameObject.AddComponent<AudioSource>();

			DeathNoiseSource.clip = DeathNoise;
			DeathNoiseSource.loop = false;
		}

		// Update is called once per frame
		void Update()
		{
			if (!IsAttacking)
			{
				if (agent.velocity.sqrMagnitude > 0.0f)
				{
					animatorComp.SetBool("isIdle?", false);
					animatorComp.SetBool("isWalking?", true);
				}

				else
				{
					animatorComp.SetBool("isWalking?", false);
					animatorComp.SetBool("isIdle?", true);
				}
			}
		}

		public void Attack(GameObject attackTarget)
		{
			if (primaryWeapon && primaryWeapon.CanFire())
			{
				primaryWeapon.Fire();
				attacking = true;
				animatorComp.SetBool("isAttacking?", true);
			}
		}

		public void AttackEnd()
		{
			attacking = false;
			animatorComp.SetBool("isAttacking?", false);

			if (agent.enabled)
				agent.isStopped = false;
		}

		public override void OnTakeDamage(GameObject attacker, float dmgAmount, EDamageType dmgType = EDamageType.Generic)
		{
			base.OnTakeDamage(attacker, dmgAmount, dmgType);
			HitNoiseSource.Play();
			
			// Alert nearby AI
			NoiseMaker.MakeNoise(transform, 100.0f, attacker);
		}

		protected override void OnNoHealth()
		{
			animatorComp.SetBool("isDead?", true);
			GetComponent<CapsuleCollider>().enabled = false;
			agent.enabled = false;

			primaryWeapon.StopFire();

			DeathNoiseSource.Play();
			IdleNoiseSource.Stop();
		}
	}
}