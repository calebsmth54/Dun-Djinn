using UnityEngine;

namespace DunGen.UI
{
	public class GameOverUI : MonoBehaviour
	{

		// Use this for initialization
		void Start()
		{
			transform.GetChild(0).gameObject.SetActive(false);
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void ShowGameOverScreen()
		{
			transform.GetChild(0).gameObject.SetActive(true);
		}
	}
}