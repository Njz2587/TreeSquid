using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGuardState_Pursuit : AIGuardState
{
    // Inspector Assigned Variables
    [SerializeField] [Range(0, 10)] private float _speed = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _lookAtWeight = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)] float _lookAtAngleThreshold = 15.0f;
    [SerializeField] private float _slerpSpeed = 5.0f;
    [SerializeField] private float _repathDistanceMultiplier = 0.035f;
    [SerializeField] private float _repathVisualMinDuration = 0.05f;
    [SerializeField] private float _repathVisualMaxDuration = 5.0f;
    [SerializeField] private float _repathAudioMinDuration = 0.25f;
    [SerializeField] private float _repathAudioMaxDuration = 5.0f;
    [SerializeField] private float _maxDuration = 40.0f;

    // Private Variables
    private float _timer = 0.0f;
    private float _repathTimer = 0.0f;
    private float _currentLookAtWeight = 0.0f;

    /// <summary>
    /// Returns the type of this state
    /// </summary>
    /// <returns></returns>
    public override AIStateType GetStateType()
    {
        return AIStateType.Pursuit;
    }

    /// <summary>
    /// Initializes state machine for Pursuit state
    /// </summary>
    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_guardStateMachine == null)
            return;

        // Configure State Machine
        _guardStateMachine.NavAgentControl(true, false);
        _guardStateMachine.seeking = 0;
        _guardStateMachine.attackType = 0;

        // Zombies will only pursue for so long before breaking off
        _timer = 0.0f;
        _repathTimer = 0.0f;


        // Set path
        _guardStateMachine.navAgent.SetDestination(_guardStateMachine.targetPosition);
        _guardStateMachine.navAgent.isStopped = false;

        _currentLookAtWeight = 0.0f;
    }

    /// <summary>
    /// Essentially the engine of the state, performs all checks on each update frame
    /// </summary>
    /// <returns></returns>
    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;
        _repathTimer += Time.deltaTime;

        // If we are in pursuit for too long, go back to patrol
        if (_timer > _maxDuration)
            return AIStateType.Patrol;

        // Otherwise this is navigation to areas of interest so use the standard target threshold
        if (_guardStateMachine.isTargetReached)
        {
            switch (_stateMachine.targetType)
            {

                // If we have reached the source
                case AITargetType.Audio:
                    // Clear the target
                    _stateMachine.ClearTarget();
                    // Go into the alert state
                    return AIStateType.Alerted;  
            }
        }


        // If for any reason the nav agent has lost its path then call then drop into alerted state so it will try to re-aquire the target or eventually giveup and resume patrolling
        if (_guardStateMachine.navAgent.isPathStale ||
            (!_guardStateMachine.navAgent.hasPath && !_guardStateMachine.navAgent.pathPending) ||
            _guardStateMachine.navAgent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            // Go into the alert state
            return AIStateType.Alerted;
        }

        if (_guardStateMachine.navAgent.pathPending)
            _guardStateMachine.speed = 0;
        else
        {
            if (_guardStateMachine.isInvestigating)
            {
                _guardStateMachine.speed = 1.0f;
            }
            else
            {
                _guardStateMachine.speed = _speed;
            }


            // If we are close to the target that was a player and we still have the player in our vision then keep facing right at the player
            if (!_guardStateMachine.useRootRotation && _guardStateMachine.targetType == AITargetType.Visual_Player && _guardStateMachine.VisualThreat.type == AITargetType.Visual_Player && _guardStateMachine.isTargetReached)
            {
                Vector3 targetPos = _guardStateMachine.targetPosition;
                targetPos.y = _guardStateMachine.transform.position.y;
                Quaternion newRot = Quaternion.LookRotation(targetPos - _guardStateMachine.transform.position);
                _guardStateMachine.transform.rotation = newRot;
            }
            else
            // SLowly update our rotation to match the nav agents desired rotation BUT only if we are not pursuing the player and are really close to them
            if (!_stateMachine.useRootRotation && !_guardStateMachine.isTargetReached)
            {
                // Generate a new Quaternion representing the rotation we should have
                Quaternion newRot = Quaternion.LookRotation(_guardStateMachine.navAgent.desiredVelocity);

                // Smoothly rotate to that new rotation over time
                _guardStateMachine.transform.rotation = Quaternion.Slerp(_guardStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
            }
            else if (_guardStateMachine.isTargetReached)
            {
                // Go into alert
                return AIStateType.Alerted;
            }
        }

        // Do we have a visual threat that is the player
        if (_guardStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            // The position is different - maybe same threat but it has moved so repath periodically
            if (_guardStateMachine.targetPosition != _guardStateMachine.VisualThreat.position)
            {
                // Repath more frequently as we get closer to the target (try and save some CPU cycles)
                if (Mathf.Clamp(_guardStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
                {
                    // Repath the agent
                    _guardStateMachine.navAgent.SetDestination(_guardStateMachine.VisualThreat.position);
                    _repathTimer = 0.0f;
                }
            }
            // Make sure this is the current target
            _guardStateMachine.SetTarget(_guardStateMachine.VisualThreat);

            // Remain in pursuit state
            return AIStateType.Pursuit;
        }

        // If our target is the last sighting of a player then remain in pursuit as nothing else can override
        if (_guardStateMachine.targetType == AITargetType.Visual_Player)
            return AIStateType.Pursuit;



        // Check if we have an audio threat
        if (_guardStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            // Check if our current target is the audio threat
            if (_guardStateMachine.targetType == AITargetType.Audio)
            {
                // Get unique ID of the collider of our target
                int currentID = _guardStateMachine.targetColliderID;

                // If this is the same audio threat
                if (currentID == _guardStateMachine.AudioThreat.collider.GetInstanceID())
                {
                    // The position is different - maybe same threat but it has moved so repath periodically
                    if (_guardStateMachine.targetPosition != _guardStateMachine.AudioThreat.position)
                    {
                        // Repath more frequently as we get closer to the target (try and save some CPU cycles)
                        if (Mathf.Clamp(_guardStateMachine.AudioThreat.distance * _repathDistanceMultiplier, _repathAudioMinDuration, _repathAudioMaxDuration) < _repathTimer)
                        {
                            // Repath the agent
                            _guardStateMachine.navAgent.SetDestination(_guardStateMachine.AudioThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }
                    _guardStateMachine.SetTarget(_guardStateMachine.AudioThreat);
                    return AIStateType.Pursuit;
                }
                else
                {
                    _guardStateMachine.SetTarget(_guardStateMachine.AudioThreat);
                    return AIStateType.Alerted;
                }
            }
        }
        // Default
        return AIStateType.Pursuit;
    }

    /// <summary>
    /// Overrides IK goals
    /// </summary>
    public override void OnAnimatorIKUpdated()
    {
        if (_guardStateMachine == null)
            return;

        if (Vector3.Angle(_guardStateMachine.transform.forward, _guardStateMachine.targetPosition - _guardStateMachine.transform.position) < _lookAtAngleThreshold)
        {
            _guardStateMachine.animator.SetLookAtPosition(_guardStateMachine.targetPosition + Vector3.up);
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, _lookAtWeight, Time.deltaTime);
            _guardStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
        }
        else
        {
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, 0.0f, Time.deltaTime);
            _guardStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
        }
    }
}
