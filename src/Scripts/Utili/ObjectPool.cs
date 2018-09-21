using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DunGen.Utility
{
	public class UnityObjectPool
	{
		private GameObject objectPoolParent;
		private List<GameObject> objectList;
		private int currentObjectIndex;

		// Initialize our object pool as a continuous block of memory
		public UnityObjectPool(string ParentName, GameObject objectToPool, int size)
		{
			// Create a parent object so that we can keep all these objects in a nice, tidy location
			objectPoolParent = new GameObject(ParentName);
			objectPoolParent.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			GameObject.DontDestroyOnLoad(objectPoolParent);

			objectList = new List<GameObject>(size); // Allocated for initial size of pool

			// Instatiate all of our pooled objects
			for (int x = 0; x < size; x++)
			{
				GameObject newObject = GameObject.Instantiate<GameObject>(objectToPool, Vector3.zero, Quaternion.identity);
				newObject.transform.SetParent(objectPoolParent.transform);
				newObject.SetActive(false);

				objectList.Add(newObject);
			}

			currentObjectIndex = 0;
		}

		// Returns the next free game object in the pool or null if non were found (Remember to activate the returned object!)
		public GameObject GetNextFree()
		{
			// Wrap indexEnd so it is always on the opposite end of the list of where it started
			int indexEnd = currentObjectIndex-1;

			if (indexEnd < 0)
				indexEnd = objectList.Count - 1;
			else if (indexEnd >= objectList.Count)
				indexEnd = 0;

			return SearchForFree(currentObjectIndex, indexEnd);
		}

		// Searches through the entire list from the startLocation -> to endlocation (startLocation - 1) (loops back around) for an inactive item
		private GameObject SearchForFree(int startLocation, int endLocation )
		{
			for (int x = startLocation; x != endLocation; x++)
			{
				// Check that our current location is within bounds, if not, wrap it around
				if (x >= objectList.Count)
					x = 0;

				GameObject currentObject = objectList[x];

				// Found a free object, return it!
				if (!currentObject.activeSelf)
				{
					currentObjectIndex = x;
					return currentObject;
				}
			}

			// Found no free objects
			return null;
		}
	}

	public class ObjectPoolManager
	{
		private static ObjectPoolManager instance;

		private static Dictionary<string, UnityObjectPool> objectPoolsDict;

		private ObjectPoolManager()
		{
			objectPoolsDict = new Dictionary<string, UnityObjectPool>();
		}

		public static ObjectPoolManager GetManager()
		{
			if (instance == null)
			{
				instance = new ObjectPoolManager();
			}

			return instance;
		}

		public UnityObjectPool CreatePool(string name, GameObject objectToPool, int initialSize)
		{
			// First, try to retrieve an existing pool before we create a new one
			UnityObjectPool newPool;
			if (objectPoolsDict.TryGetValue(name, out newPool))
			{
				return newPool;
			}

			// Since non were found, create a new one
			newPool = new UnityObjectPool(name, objectToPool, initialSize);
			objectPoolsDict.Add(name, newPool);

			return newPool;
		}

		public void DestroyPool(string name)
		{
			objectPoolsDict.Remove(name);
		}
	}
}