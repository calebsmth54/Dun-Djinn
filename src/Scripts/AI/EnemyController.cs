using UnityEngine;
using UnityEngine.AI;

using DunGen.Game;
using DunGen.Utility;
using DunGen.Components;

namespace DunGen.AI
{
	public class EnemyController : MonoBehaviour
	{
		#region Public Members

		enum EAIState
		{
			// NULL = 0,
			IDLE,
			ALERT,
			AGGRESSIVE,
			FLEEING,
		}

		[Tooltip("How long after being alerted (seeing the player) will the AI transition into the aggressive state.")]
		public float AlertDelay = 0.75f;

		[Tooltip("How close to the target do we have to be before attacking.")]
		public float AttackDistance = 2.0f;

		[Tooltip("How close to the target do we have to be before attacking.")]
		public float HearingDistance = 100.0f;

		#endregion

		#region Private Members

		private bool think = true; // Sometimes its necessary to slow down the 'ol grey matter clump

		private BaseEnemyCharacter character;
		private NavMeshAgent agent;
		private AI.Perception eyes;
		private GameObject target;

		private AIFSM aiStateMachine;

		#endregion

		void Start()
		{
			// Start up the FSM
			aiStateMachine = new AIFSM("BaseController of " + gameObject.name, this);

			aiStateMachine.Start(EAIState.IDLE);

			// Cache a ref to our controlled character
			character = gameObject.GetComponent<BaseEnemyCharacter>();
			character.gameObject.GetComponent<Health>().OnNoHealth.AddListener(OnCharacterDied);

			// Grab our nav mesh agent
			agent = gameObject.GetComponent<NavMeshAgent>();

			// Bind to perception events
			eyes = GetComponentInChildren<Perception>();
			eyes.OnSeenTarget.AddListener((newTarget) => { SeePlayer(newTarget); });
			eyes.OnLostTarget.AddListener(LostPlayer);
		}

		private void Update()
		{
			if (!think)
				return;

			aiStateMachine.Update();
		}

		public void HeardNoise(Vector3 location, GameObject instigator)
		{
			SeePlayer(instigator);
		}

		private void SeePlayer(GameObject newTarget)
		{
			target = newTarget;
		}


		private void LostPlayer()
		{
			target = null;
		}

		private void OnCharacterDied()
		{
			think = false;
			aiStateMachine.Halt();
			enabled = false;
		}

		public void MoveTowardsTarget()
		{
			if (!target)
				return;

			float distToTarget = Vector3.Distance(target.transform.position, transform.position);
			agent.isStopped = false;
			agent.SetDestination(target.transform.position);
		}

		public bool TargetWithinAttackRadius()
		{
			float distToTarget = Vector3.Distance(target.transform.position, transform.position);
			if (distToTarget < AttackDistance && !character.IsAttacking)
			{
				return true;
			}

			return false;
		}

		public void AttackTarget()
		{
			if (character.IsAttacking || !target)
				return;

			if (!TargetWithinAttackRadius())
			{
				MoveTowardsTarget();
				return;
			}

			agent.isStopped = true;

			Quaternion lookAtTarget = Quaternion.LookRotation(target.transform.position - transform.position);

			character.transform.rotation = lookAtTarget;
			character.Attack(target);
		}

		#region FSM

		//
		// AI State Machine
		//
		private class AIFSM : FSM<EAIState, EnemyController>
		{
			private IdleState idleState;
			private AlertState alertState;
			private AggressiveState aggressiveState;

			public AIFSM(string name, EnemyController controller) : base(name, controller)
			{
				if (!controller)
				{
					// TODO: Add log
					LogHalt("EnemyController object null!");
					Halt();
				}

				#if UNITY_EDITOR
					LogThis = true;
				#endif

				idleState = new IdleState(this, EAIState.IDLE, "STATE_IDLE", controller);
				alertState = new AlertState(this, EAIState.ALERT, "STATE_ALERT", controller);
				aggressiveState = new AggressiveState(this, EAIState.AGGRESSIVE, "STATE_AGGRESSIVE", controller);

				idleState.AddTransition(new Tran_IdleToAlert(alertState, controller));

				alertState.AddTransition(new Tran_AlertToAggressive(aggressiveState, controller));
				alertState.AddTransition(new Tran_AlertToIdle(idleState, controller));

				aggressiveState.AddTransition(new Tran_AggressiveToAlert(alertState, controller));

				AddState(idleState);
				AddState(alertState);
				AddState(aggressiveState);
			}


			//
			// IDLE STATE
			//
			// Enter: Start Idling
			//
			// Update: Find something to do
			//
			private class IdleState : State<EAIState, EnemyController>
			{
				public IdleState(AIFSM parent, EAIState ID, string prettyID, EnemyController controller) : base(parent, ID, prettyID, controller)
				{
				}

				public override void Enter(EAIState prevState, EnemyController stateActor)
				{
					base.Enter(prevState, stateActor);

					// TODO: Notify animations to begin idle animation
				}

				public override void Update(EnemyController stateActor)
				{
					base.Update(stateActor);

				}
			}

			// 
			// IDLE => ALERT Transition
			//
			// Condition: Saw player?
			//
			private class Tran_IdleToAlert : StateTransition<EAIState, EnemyController>
			{
				public Tran_IdleToAlert(State<EAIState, EnemyController> stateToEnter, EnemyController stateActor) : base(stateToEnter)
				{
				}

				public override bool CanTransition(EnemyController stateActor)
				{
					// Saw the player!
					if (base.CanTransition(stateActor) && stateActor.target)
					{
						return true;
					}

					return false;
				}
			}
		}

		//
		// ALERT STATE
		//
		// Enter: Start investigating
		//
		// Update: Move towards target
		//
		private class AlertState : State<EAIState, EnemyController>
		{
			public AlertState(AIFSM parent, EAIState ID, string prettyID, EnemyController controller) : base(parent, ID, prettyID, controller, controller.AlertDelay)
			{
			}

			public override void Enter(EAIState prevState, EnemyController stateActor)
			{
				base.Enter(prevState, stateActor);

				stateActor.MoveTowardsTarget();
			}

			public override void Update(EnemyController stateActor)
			{
				base.Update(stateActor);

			}
		}

		// 
		// ALERT => AGGRESSIVE Transition
		//
		// Condition: Within attack distance or Still seing the player after alert time?
		//
		private class Tran_AlertToAggressive : StateTransition<EAIState, EnemyController>
		{
			public Tran_AlertToAggressive(State<EAIState, EnemyController> stateToEnter, EnemyController stateActor) : base(stateToEnter)
			{
			}

			public override bool CanTransition(EnemyController stateActor)
			{
				// player still visible after a time or within attacking distance?
				if (base.CanTransition(stateActor) || stateActor.TargetWithinAttackRadius())
				{
					return true;
				}

				return false;
			}
		}

		// 
		// ALERT => IDLE Transition
		//
		// Condition: Lost track of player after alert delay?
		//
		private class Tran_AlertToIdle : StateTransition<EAIState, EnemyController>
		{
			public Tran_AlertToIdle(State<EAIState, EnemyController> stateToEnter, EnemyController stateActor) : base(stateToEnter)
			{
			}

			public override bool CanTransition(EnemyController stateActor)
			{
				// Lost sight of the player!
				if (base.CanTransition(stateActor) && !stateActor.target)
				{
					return true;
				}

				return false;
			}
		}

		//
		// AGRESSIVE STATE
		//
		// Enter: Start idle animations
		//
		// Update: Add heat over time
		//
		private class AggressiveState : State<EAIState, EnemyController>
		{
			public AggressiveState(AIFSM parent, EAIState ID, string prettyID, EnemyController controller) : base(parent, ID, prettyID, controller)
			{
			}

			public override void Enter(EAIState prevState, EnemyController stateActor)
			{
				base.Enter(prevState, stateActor);

				// TODO: Notify animations to begin idle animation
			}

			public override void Update(EnemyController stateActor)
			{
				base.Update(stateActor);

				stateActor.AttackTarget();
			}
		}

		// 
		// AGGRESSIVE => ALERT Transition
		//
		// Condition: Still seing the player after alert time?
		//
		private class Tran_AggressiveToAlert : StateTransition<EAIState, EnemyController>
		{
			public Tran_AggressiveToAlert(State<EAIState, EnemyController> stateToEnter, EnemyController stateActor) : base(stateToEnter)
			{
			}

			public override bool CanTransition(EnemyController stateActor)
			{
				// Saw the player!
				if (base.CanTransition(stateActor) && stateActor.target)
				{
					return true;
				}

				return false;
			}
		}

		#endregion
	}
}