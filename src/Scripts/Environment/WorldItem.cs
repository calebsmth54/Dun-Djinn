using UnityEngine;

namespace DunGen.Game
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(SphereCollider))]
	public class WorldItem : MonoBehaviour, IInteractable
	{
		public enum EItemType
		{
			Weapon = 0,
			Gear,
			Key,
			Lever
		}

		[Header("Item Properties")]
		public EItemType ItemType;
		public GameObject childObject;

		[Header("Item Bob and Rotation")]
		public float rotationRate = 90.0f;
		private Vector3 itemSpinRotation;

		public float bobUpRate = 0.1f;
		public float maxBobUp = 1.0f;
		private Vector3 bobTranslation;
		private float bobDirection = 1.0f;

		private Rigidbody rb;
		private static GameObject staticWorldItemPrefab = null;

		// Create a new world item based and add our child item to it
		public static GameObject CreateWorldItem(Vector3 position, Quaternion rotation, GameObject itemObject, Vector3 force)
		{
			if (staticWorldItemPrefab == null)
			{
				staticWorldItemPrefab = Resources.Load<GameObject>("Prefabs/WorldItem");
			}

			WorldItem newWorldItem = Instantiate(staticWorldItemPrefab, position, rotation).GetComponent<WorldItem>();

			itemObject.transform.parent = newWorldItem.transform;
			itemObject.transform.localPosition = Vector3.zero;
			itemObject.transform.localRotation = Quaternion.identity;

			newWorldItem.childObject = itemObject;

			newWorldItem.GetComponent<Rigidbody>().AddForce(force);

			return newWorldItem.gameObject;
		}

		private void Start()
		{
			if (!childObject)
				Debug.LogWarning(gameObject.name + " was put in the level and does not have a child object attached to it!");

			rb = GetComponent<Rigidbody>();
			rb.mass = 50.0f;
			rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
		}

		// Update is called once per frame
		void Update()
		{
			itemSpinRotation = Vector3.zero;
			itemSpinRotation.y = rotationRate * Time.deltaTime;

			transform.Rotate(itemSpinRotation, Space.Self);

			bobTranslation = Vector3.zero;
			bobTranslation.y = bobUpRate;

			childObject.transform.localPosition += bobTranslation * Time.deltaTime;

			if (Mathf.Abs(childObject.transform.localPosition.y) > Mathf.Abs(maxBobUp))
			{
				childObject.transform.localPosition = new Vector3(0.0f, maxBobUp, 0.0f);
				bobDirection *= -1.0f;
			}
		}

		public GameObject GetGameObject()
		{
			return childObject.transform.GetChild(0).transform.gameObject;
		}

		public void InitAsInteractable()
		{
			/*BaseWeapon weapon = GetComponentInChildren<BaseWeapon>();

			if (weapon)
			{
				weapon.gameObject.SetActive(false);
			}*/
		}

		public void LeaveInteractable()
		{
			/*BaseWeapon weapon = GetComponentInChildren<BaseWeapon>();

			if (weapon)
			{
				weapon.gameObject.SetActive(true);
			}*/
		}

		public void InteractWith(Interactor interactor)
		{
			LNBPlayerCharacter interactorOwner = interactor.GetComponentInParent<LNBPlayerCharacter>();
			GameObject item = GetGameObject();

			if (!interactorOwner)
				return;

			switch (ItemType)
			{
				case EItemType.Weapon:

					BaseWeapon weapon = item.GetComponent<BaseWeapon>();

					if (!weapon)
					{
						Debug.LogWarning(gameObject.name + " has an invalid item type!");

						return;
					}

					interactorOwner.PickupWeapon(weapon);

					break;

				case EItemType.Gear:
					BaseGear gear = item.GetComponent<BaseGear>();

					if (!gear)
					{
						Debug.LogWarning(gameObject.name + " has an invalid item type!");

						return;
					}

					interactorOwner.PickUpGear(gear);
					break;

				case EItemType.Key:
					interactorOwner.PickUpKey(item);
					break;

				default:
					Debug.LogWarning(gameObject.name + " has an invalid item type!");
					break;
			}

			interactor.PickedUpObject(gameObject);

			Destroy(gameObject);
		}
	}
}