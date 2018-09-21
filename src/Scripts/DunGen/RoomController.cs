#if UNITY_EDITOR
	//#define DD_DEBUG
#endif

using UnityEngine;

public class RoomController : MonoBehaviour
{

	public enum roomTypeEnum
	{
		Hub,
		Normal,
		Trap,
		Boss
	}

	public roomTypeEnum roomType;
	public DungeonController dungeonController;
	public Transform enterDoor;
	public Transform[] exits;
	public bool goodSpawn;

	public GameObject itemPlacementParent;
	public Transform[] itemPlacementNodes;

	// Use this for initialization
	void Start()
	{
		// We do this hack in order to cause collision events when triggers are instatiated inside of each other
		// This causes them to move into collision, causing OnTriggerEnter
		Rigidbody rb = gameObject.GetComponent<Rigidbody>();
		rb.useGravity = false;
		rb.isKinematic = true;
		rb.MovePosition(transform.position);

		// TODO: Kind of a hack? Find a better way to finalize a room's spawn
		Invoke("FinalizeSpawn", 0.1f);
	}

	private void FinalizeSpawn()
	{
		goodSpawn = true;
		gameObject.name = "Room" + dungeonController.currentRooms;
		Rigidbody rb = gameObject.GetComponent<Rigidbody>();
		DestroyImmediate(rb);

		// Notify the dungeon controller that we've finished spawning
		if (roomType.Equals(roomTypeEnum.Boss))
		{
			// Remove our exits from the exit list
			for (int x = 0; x < exits.Length; x++)
			{
				dungeonController.exitsLeft.Remove(exits[x]);
			}

			dungeonController.finishedSpawningDungeon = true;
			dungeonController.SpawnRoom(null);
		}
		else
		{
			dungeonController.currentRooms++;
			dungeonController.roomList.Add(this);
		}

		SpawnExits();
	}

	private void SpawnExits()
	{
		for (int x = 0; x < exits.Length; x++)
		{
			if (!exits[x])
			{
				Debug.Log("Invalid exit for room " + gameObject.name);
				return;
			}

			dungeonController.SpawnRoom(exits[x]);
		}
	}

	public void SpawnEnemies()
	{
		// Grab all the transforms of viable spawn points for items
		if (!itemPlacementParent)
			return;

		itemPlacementNodes = itemPlacementParent.GetComponentsInChildren<Transform>();
		int numOfNodes = itemPlacementNodes.Length;

		// TODO: Replace this with a room setting
		float roomSpawnRate = UnityEngine.Random.Range(0.0f, 1.0f);
		if (roomSpawnRate <= 0.25f)
			return;

		for (int x = 0; x < numOfNodes; x++)
		{
			// TODO: Replace this with a room setting
			float spawnRate = UnityEngine.Random.Range(0.0f, 100.0f);

			if (spawnRate <= 50.0f)
			{
				#if DD_DEBUG
					Debug.Log("Spawn item with roll of " + spawnRate);
				#endif

				dungeonController.AttemptToSpawnObject(gameObject, itemPlacementNodes[x]);
			}
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (goodSpawn)
		{
			if (other.tag == "Room")
			{
				RoomController otherController = other.gameObject.GetComponent<RoomController>();
				if (otherController.roomType.ToString() != "Boss")
				{
					Destroy(other.gameObject);
					//dungeonController.bossSpawned = false;
					//dungeonController.RespawnBoss();
				}

				// Do not try to finalize our spawn if we're being destroyed!
				otherController.CancelInvoke("FinalizeSpawn");

				//Call DungeonController to spawn dead end
				Destroy(other.gameObject);
				//dungeonController.exitsLeft.Remove(enterDoor);
				//dungeonController.bossSpawned = false;
				//dungeonController.RespawnBoss();
			}
		}
		else
		{
			if (roomType == roomTypeEnum.Boss && other.tag == "Room")
			{
				//dungeonController.exitsLeft.Remove(enterDoor);
				dungeonController.bossSpawned = false;
				dungeonController.RespawnBoss();
				Destroy(gameObject);
			}
		}
	}
}

