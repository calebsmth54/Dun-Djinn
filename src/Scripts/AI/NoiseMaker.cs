using UnityEngine;

namespace DunGen.AI
{
	public class NoiseMaker : MonoBehaviour
	{
		private GameObject instigator;
		private float heardRadius;

		public static NoiseMaker MakeNoise(Transform transform, float radius, GameObject instigator)
		{
			GameObject newObject = new GameObject();
			newObject.transform.position = transform.position;
			newObject.name = "Noise!";

			NoiseMaker newNoise = newObject.AddComponent<NoiseMaker>();
			newNoise.instigator = instigator;
			newNoise.heardRadius = radius;

			newNoise.AlertNearby();

			return newNoise;
		}

		public void AlertNearby()
		{
			// TODO: Find a more effecient way of querying all nearby enemies (look through separate list?)
			// Grab all nearby enemies
			Collider[] enemiesCollider = Physics.OverlapSphere(transform.position, heardRadius, LayerMask.GetMask("Enemy"), QueryTriggerInteraction.Ignore);

			for(int x = 0; x < enemiesCollider.Length; x++)
			{
				EnemyController enemy = enemiesCollider[x].GetComponent<EnemyController>();

				if (enemy == null)
					return;

				Vector3 distToNoise = Vector3.zero;

				if (instigator)
				{
					distToNoise = instigator.transform.position - enemy.transform.position;
				}

				float hearDistance = enemy.HearingDistance * enemy.HearingDistance;

				if (hearDistance > distToNoise.sqrMagnitude)
					enemy.HeardNoise(transform.position, instigator);
			}

			Destroy(gameObject, 5.0f);
		}
	}
}