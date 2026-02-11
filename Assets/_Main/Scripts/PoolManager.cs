using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
	// Dictionary to store pools by prefab name hash
	// Using dynamic lists with Queue
	private static Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();
	// Container holders for organization
	private static Dictionary<string, GameObject> poolHolder = new Dictionary<string, GameObject>();
	// Limits for each pool
	private static Dictionary<string, int> poolLimit = new Dictionary<string, int>();
	// Track active objects for efficient availability checking
	private static Dictionary<string, HashSet<GameObject>> activeObjects = new Dictionary<string, HashSet<GameObject>>();

	// Default pool limit
	private const int DEFAULT_POOL_LIMIT = 5;

	/// <summary>
	/// Clear all pools and destroy holder GameObjects
	/// </summary>
	public static void ClearPools()
	{
		// Destroy all pool holder GameObjects to prevent memory leaks
		foreach (var holder in poolHolder.Values)
		{
			if (holder != null)
			{
				Destroy(holder);
			}
		}

		pool.Clear();
		poolHolder.Clear();
		poolLimit.Clear();
		activeObjects.Clear();
	}

	/// <summary>
	/// Pre-instantiate a specified amount of objects for a prefab
	/// </summary>
	/// <param name="prefab">The prefab to spawn</param>
	/// <param name="amount">Number of instances to pre-create</param>
	public static void PreSpawn(GameObject prefab, int amount)
	{
		if (prefab == null)
		{
			Debug.LogError("PoolManager.PreSpawn: prefab is null");
			return;
		}

		// Create the pool if it doesn't exist
		MakePool(prefab);

		string poolKey = GetPoolKey(prefab);

		// Create N initial instances
		for (int i = 0; i < amount; i++)
		{
			AddNewItemToQueue(prefab, pool[poolKey].Count);
		}
	}

	/// <summary>
	/// Add a new item to the queue
	/// </summary>
	private static void AddNewItemToQueue(GameObject prefab, int index)
	{
		string poolKey = GetPoolKey(prefab);

		// Instantiate the GameObject
		GameObject go = (GameObject)Instantiate(prefab);
		// Set name for debugging
		go.name = prefab.name + "_PoolInstance(" + index + ")";
		// Add to the appropriate pool
		pool[poolKey].Enqueue(go);
		// Deactivate the instance
		go.SetActive(false);
		// Set parent to the pool holder for organization
		go.transform.SetParent(poolHolder[poolKey].transform);
	}

	/// <summary>
	/// Spawn an object from the pool (similar to Instantiate)
	/// </summary>
	/// <param name="prefab">The prefab type to spawn</param>
	/// <param name="position">World position</param>
	/// <param name="rotation">World rotation</param>
	/// <returns>Transform of the spawned object, or null if pool limit reached</returns>
	public static Transform Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (prefab == null)
		{
			Debug.LogError("PoolManager.Spawn: prefab is null");
			return null;
		}

		string poolKey = GetPoolKey(prefab);

		// Check if pool exists, create it if not
		if (!pool.ContainsKey(poolKey))
		{
			// Create a pool with initial instances
			PreSpawn(prefab, 2);
		}

		// Avoid recursion by using a loop
		GameObject go = GetAvailableObject(prefab);

		if (go == null)
		{
			// All objects are in use and pool limit reached
			Debug.LogWarning($"PoolManager.Spawn: Pool limit reached for {prefab.name}. Returning null.");
			return null;
		}

		// Set position and rotation as requested
		go.transform.position = position;
		go.transform.rotation = rotation;
		// Activate the object
		go.SetActive(true);
		// Track as active
		activeObjects[poolKey].Add(go);
		// Notify the object it has been spawned (like a fake Start)
		go.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);

		// Return the reference to the instantiated object
		return go.transform;
	}

	/// <summary>
	/// Get an available object from the pool or create a new one if possible
	/// </summary>
	private static GameObject GetAvailableObject(GameObject prefab)
	{
		string poolKey = GetPoolKey(prefab);
		int poolCount = pool[poolKey].Count;

		// Search for an available object efficiently
		for (int i = 0; i < poolCount; i++)
		{
			GameObject candidate = pool[poolKey].Dequeue();

			if (!candidate.activeSelf)
			{
				// Found an available object
				pool[poolKey].Enqueue(candidate);
				return candidate;
			}
			else
			{
				// Object is in use, put it back at the end
				pool[poolKey].Enqueue(candidate);
			}
		}

		// No available objects found, check if we can create a new one
		if (pool[poolKey].Count < poolLimit[poolKey])
		{
			// Pool hasn't reached the limit, create a new object
			AddNewItemToQueue(prefab, pool[poolKey].Count);
			// Get the newly created object (it's at the end of the queue)
			GameObject newObj = pool[poolKey].Dequeue();
			pool[poolKey].Enqueue(newObj);
			return newObj;
		}

		// Pool limit reached and all objects are in use
		return null;
	}

	/// <summary>
	/// Despawn an object (similar to Destroy)
	/// </summary>
	/// <param name="obj">The GameObject to despawn</param>
	public static void Despawn(GameObject obj)
	{
		if (obj == null)
		{
			Debug.LogWarning("PoolManager.Despawn: object is null");
			return;
		}

		// Remove from active tracking
		foreach (var activeSet in activeObjects.Values)
		{
			if (activeSet.Contains(obj))
			{
				activeSet.Remove(obj);
				break;
			}
		}

		// Notify the object it's being despawned
		obj.SendMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
		// Deactivate the object
		obj.SetActive(false);
	}

	/// <summary>
	/// Internal method to create a pool
	/// </summary>
	private static void MakePool(GameObject prefab)
	{
		string poolKey = GetPoolKey(prefab);

		// Create a new queue to store ALL objects of this type
		// Create dictionaries to reference later
		if (!poolLimit.ContainsKey(poolKey))
		{
			pool.Add(poolKey, new Queue<GameObject>());
			poolHolder.Add(poolKey, new GameObject("_Pool[" + prefab.name + "]"));
			poolLimit.Add(poolKey, DEFAULT_POOL_LIMIT);
			activeObjects.Add(poolKey, new HashSet<GameObject>());
		}
	}

	/// <summary>
	/// Set the limit for a specific pool (like a constructor)
	/// </summary>
	/// <param name="prefab">The prefab to set limit for</param>
	/// <param name="newLimit">The new limit value</param>
	public static void SetPoolLimit(GameObject prefab, int newLimit)
	{
		if (prefab == null)
		{
			Debug.LogError("PoolManager.SetPoolLimit: prefab is null");
			return;
		}

		if (newLimit < 1)
		{
			Debug.LogError("PoolManager.SetPoolLimit: limit must be at least 1");
			return;
		}

		string poolKey = GetPoolKey(prefab);

		// If the pool already exists, set the limit
		if (poolLimit.ContainsKey(poolKey))
		{
			poolLimit[poolKey] = newLimit;

			// If new limit is lower than current pool size, log a warning
			if (pool[poolKey].Count > newLimit)
			{
				Debug.LogWarning($"PoolManager.SetPoolLimit: New limit ({newLimit}) is lower than current pool size ({pool[poolKey].Count}). Existing objects won't be destroyed.");
			}
		}
		else
		{
			// If it doesn't exist, create the pool with this limit
			MakePool(prefab);
			poolLimit[poolKey] = newLimit;
		}
	}

	/// <summary>
	/// Get the pool count for a specific prefab
	/// </summary>
	public static int GetPoolCount(GameObject prefab)
	{
		if (prefab == null)
		{
			return 0;
		}

		string poolKey = GetPoolKey(prefab);
		return pool.ContainsKey(poolKey) ? pool[poolKey].Count : 0;
	}

	/// <summary>
	/// Get the number of active objects for a specific prefab
	/// </summary>
	public static int GetActiveCount(GameObject prefab)
	{
		if (prefab == null)
		{
			return 0;
		}

		string poolKey = GetPoolKey(prefab);
		return activeObjects.ContainsKey(poolKey) ? activeObjects[poolKey].Count : 0;
	}

	/// <summary>
	/// Get a reliable pool key from a prefab (using name instead of InstanceID)
	/// </summary>
	private static string GetPoolKey(GameObject prefab)
	{
		// Using prefab name as key is more reliable than InstanceID across builds
		// For multiple prefabs with the same name, consider adding a unique identifier
		return prefab.name;
	}
}