using UnityEngine;

namespace DunGen.UI
{
	public class GearUI : MonoBehaviour
	{
		[Tooltip("What gear mesh are we looking at?")]
		public GameObject gearSubject;

		private void Start()
		{

		}

		public void EquipNewGear(GameObject newSubject)
		{
			// Unparent our old subject if it exists
			GameObject oldGear = null;
			if (gearSubject.transform.childCount > 0)
			{
				oldGear = gearSubject.transform.GetChild(0).gameObject;
			}

			if (oldGear)
			{
				oldGear.transform.SetParent(null);
			}

			// Parent our new subject
			newSubject.transform.SetParent(gearSubject.transform);
			newSubject.transform.localPosition = Vector3.zero;
			newSubject.transform.localRotation = Quaternion.identity;
		}
	}
}