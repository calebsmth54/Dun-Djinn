using UnityEngine;

namespace DunGen.AI
{
	public class NoiseSource : MonoBehaviour
	{
		public enum NoiseType
		{
			Weapon,
			Character,
			Environment
		}

		public NoiseType Type;
		public float NoiseLifetime = 1.0f;
		public float LoudnessPriority = 0.5f; // A perceptor won't notice sounds with less priority than the one it currently is focused on

		private NoiseMaker noiseMakerOwner;
		private GameObject noiseOwner;

		public NoiseMaker GetNoiseMakerOwner { get { return noiseMakerOwner; } }
		public GameObject GetNoiseOwner { get { return noiseOwner; } }

		public void StartMakingNoise(NoiseMaker newNoiseMakerOwner, GameObject newNoiseOwner)
		{
			noiseMakerOwner = newNoiseMakerOwner;
			noiseOwner = newNoiseOwner;

			Destroy(gameObject, NoiseLifetime);
		}
	}
}