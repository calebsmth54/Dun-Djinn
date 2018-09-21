using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Env_TriggerPattern : MonoBehaviour
{
	[System.Serializable]
	public class Notify_TriggerFired : UnityEvent { }

	[System.Serializable]
	public struct SequenceGroup
	{
		[Tooltip("Optional name property for organization")]
		public string name;

		[Tooltip("Place what events you want triggered here")]
		public Notify_TriggerFired[] OnTriggerFired;
	};

	[Tooltip("Group of patterns that can be triggered ( (0,1,2,3), (0,1,2), (1) )")]
	public SequenceGroup[] TriggerSequenceGroups;
	private int currentGroup;
	private int currentChild;

	[Tooltip("Variable time inbetween triggering groups")]
	public float GroupDelayTime = 0.0f;

	[Tooltip("Variable time inbetween triggering children in groups")]
	public float SequenceDelayTime = 0.0f;

	private void Awake()
	{
		StartNewSequence();
	}

	private void ResetTrigger()
	{
		currentGroup = 0;
		currentChild = 0;
	}

	private void StartNewSequence()
	{
		// Go back to the first group if we've hit our bounds
		if (currentGroup >= TriggerSequenceGroups.Length)
			ResetTrigger();

		ChildTriggered();
	}

	private void ChildTriggered()
	{
		// See if we reached the last of our children for this group
		if (currentChild >= TriggerSequenceGroups[currentGroup].OnTriggerFired.Length)
		{
			currentChild = 0;
			currentGroup++;

			CancelInvoke("ChildTriggered");
			Invoke("StartNewSequence", GroupDelayTime);

			return;
		}

		TriggerSequenceGroups[currentGroup].OnTriggerFired[currentChild].Invoke();

		currentChild++;
		Invoke("ChildTriggered", SequenceDelayTime);
	}
}
