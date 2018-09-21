#if UNITY_EDITOR
	#define DD_DEBUG
#endif

using UnityEngine;

using DunGen.Utility;

namespace DunGen.Game
{
	public class RangedWeapon : BaseWeapon
	{

		[Header("Ranged Weapon")]
		public Transform MuzzleTransform;
		public GameObject ProjectileTemplate;
		public float MaxDistance = 100.0f;

		// *TODO* Create object pool
		private static UnityObjectPool projectilePool;

		private void Awake()
		{
			if (projectilePool == null)
			{
				projectilePool = ObjectPoolManager.GetManager().CreatePool(ProjectileTemplate.name, ProjectileTemplate, 20);
			}
		}

		protected override void OnFire(EWeaponState prevState)
		{
			base.OnFire(prevState);

			Transform shootTransform = MuzzleTransform;

			// Fire a ray cast from the player camera
			if (weaponOwner.CompareTag("Player"))
			{
				LNBPlayerCharacter playerOwner = weaponOwner.GetComponent<LNBPlayerCharacter>();
				Transform eyeTran = playerOwner.GetEyeTransform();

				// Fire ray
				int layerMask = 1 << LayerMask.GetMask("AimRay");
				RaycastHit rayHit;
				Vector3 aimDir;
				Ray aimRay = new Ray();
				aimRay.direction = eyeTran.forward;
				aimRay.origin = eyeTran.position;

				// If hit something
				if (Physics.Raycast(aimRay, out rayHit, MaxDistance, layerMask))
				{
					aimDir = rayHit.point - shootTransform.position;
				}
				else
				{
					aimDir = aimRay.direction*MaxDistance;
					rayHit.distance = MaxDistance;
				}

				shootTransform.rotation = Quaternion.LookRotation(aimDir);

				#if DD_DEBUG
					// Aim trace
					DebugExtension.DebugArrow(aimRay.origin, aimRay.direction * rayHit.distance, Color.red, 1.0f);
					//Aim Direction
					DebugExtension.DebugArrow(shootTransform.position, aimDir, Color.green, 1.0f);
				#endif
			}

			// Spawn our projectile
			GameObject newProjObj = projectilePool.GetNextFree();
			if (newProjObj)
			{
				newProjObj.SetActive(true);
				Projectile newProjectile = newProjObj.GetComponent<Projectile>();

				if (!newProjectile)
				{
					Debug.Log("Instantiated something that's not a projectile! Add it in your prefab!");
					return;
				}

				newProjectile.Launch(this, weaponOwner, MuzzleTransform);
			}
		}
	}
}