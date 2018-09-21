using UnityEngine;

namespace DunGen.Game
{
	public class BaseGear : MonoBehaviour
	{
		protected LNBPlayerCharacter playerOwner;

		public virtual void OnPickUp(LNBPlayerCharacter owner)
		{
			playerOwner = owner;
		}

		public virtual void OnDrop()
		{

		}

	}
}