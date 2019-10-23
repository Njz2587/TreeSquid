using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGuardState_Patrol : AIGuardState
{
    // Inpsector Assigned 
    [SerializeField] float _turnOnSpotThreshold = 80.0f;
    [SerializeField] float _slerpSpeed = 5.0f;

    [SerializeField] [Range(0.0f, 3.0f)] float _speed = 1.0f;

    /// <summary>
    /// Returns the type of this state
    /// </summary>
    /// <returns></returns>
    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    /// <summary>
    /// Initializes state machine for Patrol state
    /// </summary>
    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_guardStateMachine == null)
            return;

        //Debug.Log("Entered patrol");
        // Configure State Machine
        _guardStateMachine.NavAgentControl(true, false);
        _guardStateMachine.seeking = 0;
        _guardStateMachine.attackType = 0;

        // Set Destination
        _guardStateMachine.navAgent.SetDestination(_guardStateMachine.GetWaypointPosition(false));

        // Make sure NavAgent is switched on
        _guardStateMachine.navAgent.isStopped = false;


    }


    /// <summary>
    /// Essentially the engine of the state, performs all checks on each update frame
    /// </summary>
    /// <returns></returns>
    public override AIStateType OnUpdate()
    {
        // Do we have a visual threat that is the player
        if (_guardStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            // Set the target to be the player (the current visual threat)
            _guardStateMachine.SetTarget(_guardStateMachine.VisualThreat);
            // Go into pursuit!
            return AIStateType.Pursuit;
        }

        // Check if the threat is an audio emitter
        if (_guardStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            // Set the target to be the audio threat
            _guardStateMachine.SetTarget(_guardStateMachine.AudioThreat);
            // Alert the guard
            return AIStateType.Alerted;
        }

        // If path is still be computed then wait
        if (_guardStateMachine.navAgent.pathPending)
        {
            _guardStateMachine.speed = 0;
            Debug.Log("Path was pending, going into patrol again!");
            return AIStateType.Patrol;
        }
        else
            _guardStateMachine.speed = _speed;

        // Calculate angle we need to turn through to be facing our target
        float angle = Vector3.Angle(_guardStateMachine.transform.forward, (_guardStateMachine.navAgent.steeringTarget - _guardStateMachine.transform.position));

        // If its too big then drop out of Patrol and into Altered
        if (angle > _turnOnSpotThreshold)
        {
            //Debug.Log("Angle was too big: " + angle + " > " + _turnOnSpotThreshold);
            return AIStateType.Alerted;
        }

        // If root rotation is not being used then we are responsible for keeping guard rotated and facing in the right direction. 
        if (!_guardStateMachine.useRootRotation)
        {
            // Generate a new Quaternion representing the rotation we should have
            Quaternion newRot = Quaternion.LookRotation(_guardStateMachine.navAgent.desiredVelocity);

            // Smoothly rotate to that new rotation over time
            _guardStateMachine.transform.rotation = Quaternion.Slerp(_guardStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }

        // If for any reason the nav agent has lost its path then call the NextWaypoint function  so a new waypoint is selected and a new path assigned to the nav agent.
        if (_guardStateMachine.navAgent.isPathStale ||
            !_guardStateMachine.navAgent.hasPath ||
            _guardStateMachine.navAgent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            Debug.Log("Path was stale or something, getting new waypoint!");
            // Get a new waypoint position
            _guardStateMachine.navAgent.SetDestination(_guardStateMachine.GetWaypointPosition(true));
        }

        // By default, state in patrol state
        return AIStateType.Patrol;
    }

    /// <summary>
    /// Called by parent state machine when guard has reached its target
    /// </summary>
    /// <param name="isReached"></param>
    public override void OnDestinationReached(bool isReached)
    {
        // Only interesting in processing arricals not departures
        if (_guardStateMachine == null || !isReached)
            return;

        // Select the next waypoint in the waypoint network
        if (_guardStateMachine.targetType == AITargetType.Waypoint)
        {
            //Debug.Log("Destination reached, getting new waypoint!");
            _guardStateMachine.navAgent.SetDestination(_guardStateMachine.GetWaypointPosition(true));
        }
           
    }

}
