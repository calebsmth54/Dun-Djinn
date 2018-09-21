//	FSM 
//
//	Derive from these classes in order to make an easy to use Finite State Machine.
//	
//	Inspired heavily by the implementation in "Game Programming Patterns"
//	http://gameprogrammingpatterns.com/state.html
//
//	This implementation uses generics so that when implementing your own versions of 
//	these classes, you can easily setup the TEnum and TStateActor with custom types.
//
//	TEnum - Use with an enum who's first enumerator is 1 (0 is reserved for null states)
//	TStateActor - Use with any class you want to manipulate with your state machine implementation
//
//	CJS - blasted2smithereens@gmail.com
//

// Locally enable debug code
#if UNITY_EDITOR
	#define DD_DEBUG
#endif

using System.Collections.Generic;
using UnityEngine;

namespace DunGen.Utility
{
	// Base transition class. Used to create transition conditions from one state to another
	// Optionally a delay can be set to wait before this transition can be checked
	public class StateTransition<TEnum, TStateActor>
	{
		// Controls how long after activation this transition will wait before checking conditions
		// Any value <= 0.0f will disable the wait time
		public float TransitionWaitTime;

		protected float endDelayTime;

		private State<TEnum, TStateActor> desiredState;

		// Target state the FSM will transition to if this condition returns ture
		public State<TEnum, TStateActor> getTargetState { get { return desiredState; }  }
		
		// The state to enter if CanTransition returns true and an optional delay time that prevents CanTransition from return true
		public StateTransition(State<TEnum, TStateActor> stateToEnter, float waitToTransitionDelay = 0.0f)
		{
			desiredState = stateToEnter;
			TransitionWaitTime = waitToTransitionDelay;
		}

		// Setup
		public virtual void ParentStateActivated(TStateActor stateActor)
		{
			// If delaying transition, immediately check start a timer
			if (TransitionWaitTime > 0.0f)
			{
				endDelayTime = TransitionWaitTime + Time.time;
			}
		}

		// Determines whether this transition can be made
		public virtual bool CanTransition(TStateActor stateActor)
		{
			if (TransitionWaitTime > 0.0f)
			{
				if (endDelayTime <= Time.time)
				{
					// Enough time has passed to allow this state to transition
					return true;
				}

				// Continue waiting
				return false;
			}

			// By default, nothing prevents a transition from firing
			return true;
		}
	}

	// Base state class. Contains an array of transitions to other states.
	// Optionally, can be prevented from transitioning until a delay has passed
	public class State<TEnum, TStateActor>
	{
		// Messy log code. Avert yours eyes!
		#region DEBUG
		#if UNITY_EDITOR

		// Prints a log message for halting and a reason why the halt happened
		public void LogEnterState()
		{
			if (!parent.LogThis)
				return;

			string message = parent.LogHeader() + "<color=red>**FSM**</color> Started with state: <color=green> " + getPrettyID + "</color>";
			Debug.Log(message);
		}

		#endif
		#endregion

		// Controls how long condition checks will be ignored and automatically fail
		// Any value <= 0.0f will disable the wait time
		public float DelayTransitionCheckTime;

		protected List<StateTransition<TEnum, TStateActor>> transitionList;

		protected FSM<TEnum, TStateActor> parent;

		protected float endDelayTime;

		private TEnum enumID; // Fast; For comparison operations.

		private string stringID; // Slow; Don't use unless you need to print this to the log.

		
		// An enum identifier for this state.
		// TODO: CJS - Currently this identifier should be a unique enum. Should the FSM allow duplicate states?
		// Fast; For comparison operations.
		public TEnum getID { get { return enumID; } }

		// State name used for debug logging
		// Slow; Don't use unless you need to print this to the log.
		public string getPrettyID { get { return stringID; } }

		public State(FSM<TEnum, TStateActor> parentFSM, TEnum ID, string prettyID, TStateActor stateActor, float waitToCheckDelay = 0.0f)
		{
			parent = parentFSM;
			enumID = ID;
			stringID = prettyID;
			DelayTransitionCheckTime = waitToCheckDelay;

			transitionList = new List<StateTransition<TEnum, TStateActor>>();
		}

		// Add a transition to a list of transitions
		public void AddTransition(StateTransition<TEnum, TStateActor> newTranstion)
		{
			transitionList.Add(newTranstion);
		}
		
		// First time the state is activated
		public virtual void Enter(TEnum prevState, TStateActor stateActor)
		{
			// Start the delay timer
			endDelayTime = DelayTransitionCheckTime + Time.time;

			// Setup for transitions
			foreach (StateTransition<TEnum, TStateActor> tran in transitionList)
			{
				tran.ParentStateActivated(stateActor);
			}
		}

		// Returns the first StateTransition that is able to transition immediately
		// If a wait time was set or no valid transitions were found, this will return null
		public StateTransition<TEnum, TStateActor> CheckTransitions(TStateActor stateActor)
		{
			if (DelayTransitionCheckTime > 0.0f)
			{
				if (endDelayTime > Time.time)
					return null;
			}

			foreach (StateTransition<TEnum, TStateActor> tran in transitionList)
			{
				if (tran.CanTransition(stateActor))
				{
					return tran;
				}
			}

			return null;
		}

		public virtual void Update(TStateActor stateActor) { }
		public virtual void Exit(TEnum nextState, TStateActor stateActor) { }
	}

	// Base state machine class that transitions between states and updates an active state
	// Overide this class with your 
	public class FSM<TEnum, TStateActor>
	{
		// Messy log code. Avert yours eyes!
		#region Debug
		#if UNITY_EDITOR

		// Enables debug logging for this state machine
		public bool LogThis = false;

		public string LogHeader()
		{
			string stateMachineName = prettyName;
			string ownerActorName = (ownerActor != null) ? ownerActor.ToString() : "null";
			return "<color=red>**FSM**</color> State Machine: <color=green>" + stateMachineName + "</color> Owened by: <color=green>" + ownerActorName + "</color>\n";
		}

		// Prints a log message for halting and a reason why the halt happened
		public void LogStart(State<TEnum, TStateActor> startState)
		{
			if (!LogThis)
				return;

			string stateName = (startState != null) ? startState.getPrettyID : "null";
			string message = LogHeader() + "<color=red>**FSM**</color> Started with state: <color=green>" + stateName + "</color>";
			Debug.Log(message);
		}

		// Prints a log message for halting and a reason why the halt happened
		public void LogHalt(string reason)
		{
			if (!LogThis)
				return;

			string message = LogHeader() + "<color=red>**FSM**</color> Halting state machine because: <color=red>" + reason + "</color>";
			Debug.Log(message);
		}

		// Prints a log message with the currently active state
		public void LogStateChange(State<TEnum, TStateActor> oldState, State<TEnum, TStateActor> newState)
		{
			if (!LogThis)
				return;

			string oldStateName = (oldState != null) ? oldState.getPrettyID : "null";
			string newStateName = (newState != null) ? newState.getPrettyID : "null";
			string message = LogHeader() + "<color=red>**FSM**</color> Transition from <color=green>" + oldStateName + "</color> to <color=green>" + newStateName + "</color>";
			Debug.Log(message);
		}

		#endif
		#endregion

		// A dictionary of all the states for this FSM
		protected Dictionary<TEnum, State<TEnum, TStateActor>> dictOfStates;

		// The currently running state
		protected State<TEnum, TStateActor> activeState;

		// The class/actor we are actively changing the state of
		protected TStateActor ownerActor;

		// Has the machine started and is running?
		protected bool active = false;

		// Name for debugging purposes
		private string prettyName;

		// Returns the currently active state
		public State<TEnum, TStateActor> getState { get { return activeState; } }

		// returns an easy to read name for this FSM
		public string getPrettyID { get { return prettyName; } }

		// returns the object the state is manipulating
		public TStateActor getOwner { get { return ownerActor; } }

		// Takes an easy to read name and an object that the state will manipulate
		public FSM(string name, TStateActor stateActor)
		{
			prettyName = name;
			ownerActor = stateActor;
			dictOfStates = new Dictionary<TEnum, State<TEnum, TStateActor>>();
		}

		// Add a state to the available state selection
		public void AddState(State<TEnum, TStateActor> newState)
		{
			dictOfStates.Add(newState.getID, newState);
		}

		// Select a starting state and turn on the FSM. States begin checking their transition conditions
		public virtual void Start(TEnum stateToStart)
		{
			// An FSM should never try to start mid execution
			if (active)
			{
				#if DD_DEBUG
					LogHalt("Tried to start the machine while it was already running!");
				#endif

				Halt();

				return;
			}

			// Initialize FSM
			active = true;
			State<TEnum, TStateActor> newState = dictOfStates[stateToStart];

			#if DD_DEBUG
				LogStart(newState);
			#endif

			activeState = newState;
			activeState.Enter(stateToStart, ownerActor);
		}

		// Stop updating
		public virtual void Halt()
		{
			active = false;
		}

		public virtual void Update()
		{
			// Don't execute if halted or never started
			if (!active)
			{
				#if DD_DEBUG
					LogHalt("Tried to update while the machine was inactive!");
				#endif

				return;
			}

			// FSM should never have bad refs
			if (activeState == null)
			{
				#if DD_DEBUG
					LogHalt("Tried to run an update with no active state to run!");
				#endif

				Halt();
				return;
			}

			// Check a state's list of transitions and grab the first one that returns true
			StateTransition<TEnum, TStateActor> newStateTran = activeState.CheckTransitions(ownerActor);

			// Transition to a new state
			if (newStateTran != null)
			{
				#if DD_DEBUG
					LogStateChange(activeState, newStateTran.getTargetState);
				#endif

				// Notify the old state of the transition
				TEnum oldState = activeState.getID;
				activeState.Exit(newStateTran.getTargetState.getID, ownerActor);

				// Notify the new state of the transition
				activeState = newStateTran.getTargetState;
				activeState.Enter(oldState, ownerActor);
			}

			// Handle state manipulation of the FSM's owning object
			activeState.Update(ownerActor);
		}
	}
}

