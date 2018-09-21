using UnityEngine;

using DunGen.UI;
using DunGen.Game;

namespace DunGen
{
	public class GameManager : MonoBehaviour
	{

		// An instance of the character prefab as well as the prefab that we create copies of upon death
		public LNBPlayerCharacter PlayerCharacterPrefab;

		public GameObject PlayerHud;

		public Transform PlayerSpawnTransform;

		public World GameWorld;

		public RoomController HubRoom;

		public DungeonController[] Dungeons;

		private static GameManager instance; // singleton -- THE ANTI PATTERN
		private static World gameWorldInstance;
		private static RoomController hubRoomInstance;
		private static DungeonController currentDungeonInstance;

		private LNBPlayerCharacter spawnedPlayerCharacter;
		private LNBPlayerController spawnedPlayerController;

		public static GameManager GetGameManager()
		{
			return instance;
		}

		public static World GetWorld()
		{
			return gameWorldInstance;
		}

		public LNBPlayerCharacter GetPlayerCharacter()
		{
			return spawnedPlayerCharacter;
		}

		// Use this for initialization
		private void Awake()
		{
			// Sigh -- Ensure our singleton is valid
			if (!instance)
			{
				instance = this;
			}
			else
			{
				Debug.LogError("More than one Game Manager instance in the scene! Only one can be present!");
				return;
			}

			// Grab our world instance
			if (!gameWorldInstance)
			{
				gameWorldInstance = GameWorld;
			}
			else
			{
				Debug.LogError("More than one World instance in the scene! Only one can be present!");
				return;
			}

			// Grab our hub instance
			if (!hubRoomInstance)
			{
				hubRoomInstance = HubRoom;
			}
			else
			{
				Debug.LogError("More than one HubRoom instance in the scene! Only one can be present!");
				return;
			}

			// Make sure we have an assigned player character prefab with a valid controller attached
			if (!PlayerCharacterPrefab)
			{
				Debug.LogError("No player prefab assigned in the Game Manager!");
				return;
			}

			RestartGame();
		}

		private void Start()
		{

		}

		private void RestartGame()
		{
			// Spawn our player character
			Vector3 playerSpawnPosition = Vector3.zero;
			Quaternion playerSpawnOrientation = Quaternion.identity;
			Quaternion playerLookOrientation = Quaternion.identity;
			if (PlayerSpawnTransform)
			{
				playerSpawnPosition = PlayerSpawnTransform.position;
				playerSpawnOrientation = PlayerSpawnTransform.rotation;
			}

			spawnedPlayerCharacter = Instantiate(PlayerCharacterPrefab, playerSpawnPosition, playerSpawnOrientation, GetWorld().transform).GetComponent<LNBPlayerCharacter>();
			spawnedPlayerController = spawnedPlayerCharacter.GetComponent<LNBPlayerController>();

			// Transform the pitch and not the character #TODO: Write your own player controller
			playerLookOrientation = spawnedPlayerController.GetComponentInChildren<Camera>().transform.localRotation;
			spawnedPlayerController.GetComponentInChildren<Camera>().transform.localRotation = playerLookOrientation;

			// TODO: Async generate our first dungeon
			// Instantiate our first dungeon
			currentDungeonInstance = Instantiate(Dungeons[0], Vector3.zero, Quaternion.identity, GetWorld().transform);
			hubRoomInstance.dungeonController = currentDungeonInstance;
			currentDungeonInstance.SpawnRoom(hubRoomInstance.exits[0]);
		}

		private void ClearGame()
		{
			// Delete our player character
			DestroyImmediate(spawnedPlayerCharacter.gameObject);

			// Destroy our dungeons
			DestroyImmediate(currentDungeonInstance.gameObject);
			//GetWorld().ClearWorld();
		}

		private void Update()
		{
		}

		public void OnPlayerDeath()
		{
			// Wait for player to confirm respawn
			PlayerHud.GetComponentInChildren<GameOverUI>().ShowGameOverScreen();
		}

		public void OnPlayerConfirmRespawn()
		{
			ClearGame();
			RestartGame();
		}
	}
}