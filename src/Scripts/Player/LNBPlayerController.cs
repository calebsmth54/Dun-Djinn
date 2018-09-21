using UnityEngine;

namespace DunGen.Game
{
	public class LNBPlayerController : MonoBehaviour
	{

		private LNBPlayerCharacter playerCharacter;

		private bool fireButtonDown = false;

		// Use this for initialization
		void Start()
		{
			playerCharacter = GetComponentInChildren<LNBPlayerCharacter>();
			if (!playerCharacter)
			{
				Debug.Log("No LNBPlayerCharacter found for this LNBPlayerController!");
			}
		}

		private void HandleInput()
		{
			// If we're dead, don't handle any input
			if (playerCharacter.IsDead)
			{
				// Let the game manager know we're ready to respawn
				if (Input.GetButtonDown("Submit"))
				{
					GameManager.GetGameManager().OnPlayerConfirmRespawn();
				}

				return;
			}

			if (fireButtonDown)
			{
				if (Input.GetAxis("Fire") <= 0)
				{
					fireButtonDown = false;
					playerCharacter.StopWeapon();
				}
			}

			else
			{
				if (Input.GetAxis("Fire") > 0)
				{
					fireButtonDown = true;
					playerCharacter.FireWeapon();
				}
			}

			if (Input.GetButtonDown("ToggleWeapon"))
			{
				playerCharacter.HolsterWeapon();
			}

			if (Input.GetButtonDown("Interact"))
			{
				playerCharacter.Interact();
			}

		}

		// Update is called once per frame
		void Update()
		{
			HandleInput();
		}
	}
}