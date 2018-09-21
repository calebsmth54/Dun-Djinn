#if UNITY_EDITOR
	//#define DD_DEBUG
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DungeonController : MonoBehaviour
{
	[System.Serializable]
	public class RoomGroup
	{
		[System.Serializable]
		public class RoomProperties
		{
			public string roomName;

			[Tooltip("Put your room variation here.")]
			public GameObject roomPrefab;

			// TODO: Limit this from 0.0f to 1.0f;
			[Tooltip("0 means there is no chance for this to spawn. 1.0 means that it has equal chance to spawn alongside members of its room group")]
			[Range(0.0f, 1.0f)]
			public float spawnWeight = 1.0f;
		}

		public string groupName; // Hallways, Arenas, Traps, etc.

		[Tooltip("0 means there is no chance for this to spawn. 1.0 means that it has equal chance to spawn alongside members of its room group")]
		[Range(0.0f, 1.0f)]
		public float spawnWeight = 1.0f;

		[Tooltip("A list of different room variations for this group (I.E. a bunch of hallways")]
		public RoomProperties[] roomVariations; // A list of different prefab rooms and their spawning options

		public static RoomGroup ChooseRandomGroup(RoomGroup[] roomGroups)
		{
			float randomChoice = Random.Range(0.0f, 0.9999999f);

			foreach (RoomGroup roomGroup in roomGroups)
			{
				#if DD_DEBUG
					Debug.Log(roomGroup.groupName + " has " + roomGroup.spawnWeight + " weight.");
				#endif

				if (randomChoice < roomGroup.spawnWeight)
					return roomGroup;

				randomChoice -= roomGroup.spawnWeight;
			}

			#if DD_DEBUG
				Debug.LogError("Room variations do not add up to 1! Double check your dungeon room groups.");
			#endif

			return null;
		}

		public RoomProperties ChooseRandomRoom()
		{
			float randomChoice = Random.Range(0.0f, 1.0f);

			foreach (RoomProperties roomProp in roomVariations)
			{
				#if DD_DEBUG
					Debug.Log(roomProp.roomName + " has " + roomProp.spawnWeight + " of being selected.");
				#endif

				if (randomChoice < roomProp.spawnWeight)
					return roomProp;

				randomChoice -= roomProp.spawnWeight;
			}

			#if DD_DEBUG
				Debug.LogError("Room variations for " + groupName + " do not add up to 1! Double check your room groups.");
			#endif

			return null;
		}
	}

	[System.Serializable]
	public class SpawnableObject
	{
		public string spawnableName; // Monster, environment, etc.

		[Tooltip("0 means there is no chance for this to spawn. 1.0 means that it has equal chance to spawn alongside members of its room group")]
		[Range(0.0f, 1.0f)]
		public float spawnWeight = 1.0f;

		[Tooltip("A list of different object variations")]
		public GameObject spawnableObject; // A list of different prefab objects to be spawned and their spawning options

		public static SpawnableObject ChooseRandomSpawnable(SpawnableObject[] spawnables)
		{
			float randomChoice = Random.Range(0.0f, 1.0f);

			foreach (SpawnableObject spawnable in spawnables)
			{
				#if DD_DEBUG
					Debug.Log(spawnable.spawnableName + " has " + spawnable.spawnWeight + " of being selected.");
				#endif

				if (randomChoice < spawnable.spawnWeight)
					return spawnable;

				randomChoice -= spawnable.spawnWeight;
			}

			#if DD_DEBUG
				Debug.LogError("Spawnable weights do not add up to 1! Double check your spawnable tab.");
			#endif 

			return null;
		}
	}

	[Header("Room Spawning Properties")]
	[Tooltip("Contains a group of room variations. I.E. Hallway, arena, etc.")]
	public RoomGroup[] RoomGroups;
	
	[Tooltip("Contains a group of room variations. I.E. Hallway, arena, etc.")]
	public RoomGroup.RoomProperties BossRoomProperties;

	[Tooltip("Contains a listing for all spawnable objects and their weights")]
	public SpawnableObject[] SpawnableObjects;

	public GameObject RoomPlug;
	public GameObject DungeonKey;
	private bool dungeonKeySpawned = false;

	public List<RoomController> roomList;
	public List<Transform> exitsLeft;
	public int currentRooms = 0, minRooms = 10;
	public int roomAttempts = 0;
	public bool bossSpawned = false;
	public bool finishedSpawningDungeon = false;
	public bool finalizedDungeon = false;

	private GameObject dungeonRoot;

	private int bossExitAttempt = 0;

	// Use this for initialization
	private void Awake()
	{
		Random.InitState(System.DateTime.UtcNow.Millisecond);

		roomList = new List<RoomController>();
		exitsLeft = new List<Transform>();

		dungeonRoot = new GameObject();
		dungeonRoot.name = "DungeonRoot";
		dungeonRoot.transform.SetParent(transform);
		dungeonRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
	}

	void Start()
	{


		//SpawnRoom(this.transform);
	}

	public void SpawnRoom(Transform exitDoor)
	{
		// Roll for group type
		GameObject newRoom = null;

		// Spawn regular rooms until we've reached our max count
		if (currentRooms < minRooms)
		{
			//newRoom = Instantiate(Resources.Load("DungeonRooms/Room" + Random.Range(0, roomCount)), exitDoor.position, exitDoor.rotation) as GameObject;
			
			// Choose a random roomPrefab from a random group
			GameObject roomPrefab = RoomGroup.ChooseRandomGroup(RoomGroups).ChooseRandomRoom().roomPrefab;
			newRoom = Instantiate(roomPrefab, exitDoor.position, exitDoor.rotation);

			RoomController newController = newRoom.GetComponent<RoomController>();
			if (!newController)
			{
				Debug.Log(newRoom.name + " is missing a room controller!");
				return;
			}

			newController.dungeonController = this;
			newController.enterDoor = exitDoor;

			newRoom.transform.SetParent(dungeonRoot.transform);
		}
		else if (!bossSpawned)
		{
			//newRoom = Instantiate(Resources.Load("BossRooms/Room" + Random.Range(0, bossRoomCount)), exitDoor.position, exitDoor.rotation) as GameObject;
			newRoom = Instantiate(BossRoomProperties.roomPrefab, exitDoor.position, exitDoor.rotation);

			RoomController newController = newRoom.GetComponent<RoomController>();
			newController.dungeonController = this;
			newController.enterDoor = exitDoor;

			roomList.Add(newController);
			newRoom.transform.SetParent(dungeonRoot.transform);

			bossSpawned = true;
		}
		else if (bossSpawned && finishedSpawningDungeon && !finalizedDungeon)
		{
			finalizedDungeon = true;
			GameObject plug = null;

			// If all the rooms connect properly, the nav mesh will build all connecting surfaces
			roomList[0].GetComponent<NavMeshSurface>().BuildNavMesh();

			for (int x = 0; x < roomList.Count; x++)
			{
				if (!roomList[x])
					continue;

				// If this is the first room, fooooooget about it
				if (x > 0)
					roomList[x].SpawnEnemies();

				// If we're in the last 4th of the dungeons, spawn a key
				if (!dungeonKeySpawned && ((float)(x)/roomList.Count) >= 0.75f)
				{
					dungeonKeySpawned = true;
					// If we're in the last room and we haven't spawned a key yet, do so
					GameObject key = Instantiate(DungeonKey);
					int randomNode = Random.Range(0, roomList[x].itemPlacementNodes.Length);

					key.transform.position = roomList[x].itemPlacementNodes[randomNode].transform.position;
					key.transform.SetParent(roomList[x].transform);
				}
			}

			// No longer needed. The first room will always be connected to the hub!
			// Add a plug to our first room's entrance
			/*plug = Instantiate(RoomPlug);
			plug.transform.SetParent(roomList[0].GetComponent<RoomController>().enterDoor);
			plug.transform.localPosition = Vector3.zero;
			plug.transform.localRotation = Quaternion.identity;
			plug.transform.SetParent(roomList[0].GetComponentInParent<RoomController>().gameObject.transform);*/

			// Add plugs to all the exits
			foreach (Transform exit in exitsLeft)
			{
				plug = Instantiate(RoomPlug);
				plug.transform.SetParent(exit);
				plug.transform.localPosition = Vector3.zero;
				plug.transform.localRotation = Quaternion.identity;
				plug.transform.SetParent(exit.GetComponentInParent<RoomController>().gameObject.transform);
			}
		}
		else
		{
			exitsLeft.Add(exitDoor);
		}
	}

	public void RespawnBoss()
	{
		if (!bossSpawned)
		{
			currentRooms += 1;
			GameObject newRoom = null;
			bossSpawned = true;
			//newRoom = Instantiate(Resources.Load("BossRooms/Room" + Random.Range(0, bossRoomCount)), exitsLeft[Random.Range(0, exitsLeft.Count - 1)].position, exitsLeft[Random.Range(0, exitsLeft.Count)].rotation) as GameObject;
			newRoom = Instantiate(BossRoomProperties.roomPrefab, exitsLeft[bossExitAttempt].position, exitsLeft[bossExitAttempt].rotation);
			RoomController newController = newRoom.GetComponentInChildren<RoomController>();

			newController.dungeonController = this;
			roomList.Add(newController);
			newRoom.transform.SetParent(dungeonRoot.transform);
			bossExitAttempt++;
		}
	}

	public void RespawnRoom(Transform currentDoor, string roomType, GameObject oldRoom)
	{
		roomAttempts += 1;
		Destroy(oldRoom);
		GameObject newRoom = null;

		if (roomAttempts > 10 || bossSpawned)
		{
			//Spawn Closed Door
			currentRooms -= 1;
			roomAttempts = 0;
		}
		else
		{
			/*switch (roomType)
			{
				case "Boss":
					//newRoom = Instantiate(Resources.Load("BossRooms/Room" + Random.Range(0, bossRoomCount)), currentDoor.position, currentDoor.rotation) as GameObject;
					newRoom.GetComponentInChildren<RoomController>().dungeonController = this;
					break;
				case "Normal":
					//newRoom = Instantiate(Resources.Load("DungeonRooms/Room" + Random.Range(0, roomCount)), currentDoor.position, currentDoor.rotation) as GameObject;
					newRoom.GetComponentInChildren<RoomController>().dungeonController = this;
					break;
				case "Trap":
					//newRoom = Instantiate(Resources.Load("TrapRooms/Room" + Random.Range(0, trapRoomCount)), currentDoor.position, currentDoor.rotation) as GameObject;
					newRoom.GetComponentInChildren<RoomController>().dungeonController = this;
					break;
			}*/

		}
	}

	public void AttemptToSpawnObject(GameObject room, Transform spawnTransform)
	{
		SpawnableObject spawmedObjectPrefab = SpawnableObject.ChooseRandomSpawnable(SpawnableObjects);

		if (spawmedObjectPrefab == null || spawmedObjectPrefab.spawnableObject == null || !spawnTransform)
			return;

		GameObject newSpawnable = Instantiate(spawmedObjectPrefab.spawnableObject, spawnTransform.position, spawnTransform.rotation);
		newSpawnable.transform.SetParent(room.transform);
	}
}

