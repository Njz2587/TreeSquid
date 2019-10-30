using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGuardState_Alerted : AIGuardState
{
    // Inspector Assigned
    [SerializeField] [Range(1, 60)] float _maxDuration = 10.0f;
    [SerializeField] float _waypointAngleThreshold = 90.0f;
    [SerializeField] float _threatAngleThreshold = 10.0f;
    [SerializeField] float _directionChangeTime = 1.5f;
    [SerializeField] float _slerpSpeed = 45.0f;

    // Private Fields
    float _timer = 0.0f;
    float _directionChangeTimer = 0.0f;
    float _alarmChance = 0.0f;
    float _nextAlarm = 0.0f;
    float _alarmFrequency = 120.0f;


    /// <summary>
    /// Returns the type of the state
    /// </summary>
    /// <returns></returns>
    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    /// <summary>
    /// Initializes state machine for Alerted state
    /// </summary>
    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_guardStateMachine == null)
            return;

        //Debug.Log("Entered alert");
        // Configure State Machine
        _guardStateMachine.NavAgentControl(true, false);
        _guardStateMachine.speed = 0;
        _guardStateMachine.seeking = 0;
        _guardStateMachine.attackType = 0;
        //_guardStateMachine.ClearTarget();

        _timer = _maxDuration;
        _directionChangeTimer = 0.0f;
        _alarmChance = _guardStateMachine.alarmChance - Random.value;
    }

    /// <summary>
    /// Essentially the engine of the state, performs all checks on each update frame
    /// </summary>
    /// <returns></returns>
    public override AIStateType OnUpdate()
    {
        // Reduce Timer
        _timer -= Time.deltaTime;
        _directionChangeTimer += Time.deltaTime;

        // Transition into a patrol state if available
        if (_timer <= 0.0f)
        {
            _guardStateMachine.navAgent.SetDestination(_guardStateMachine.GetWaypointPosition(false));
            _guardStateMachine.navAgent.isStopped = false;
            _timer = _maxDuration;
        }

        // Do we have a visual threat that is the player. These take priority over audio threats
        if (_guardStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _guardStateMachine.SetTarget(_guardStateMachine.VisualThreat);

            /*
            if (_alarmChance > 0.0f && Time.time > _nextAlarm)
            {
                if (_guardStateMachine.Alarm())
                {
                    _alarmChance = float.MinValue;
                    _nextAlarm = Time.time + _alarmFrequency;
                    return AIStateType.Alerted;
                }
            }
            */

            // Check if we do not already see the player
            if (!_guardStateMachine.PlayerIsVisible)
            {
                StartCoroutine(_guardStateMachine.ShowAlarmSymbol());
                _guardStateMachine.PlayerIsVisible = true;
            }

            if (_guardStateMachine.inMeleeRange)
            {
                // Go into Attack!
                return AIStateType.Attack;
            }
            else
            {
                Debug.Log("Going into Pursuit from Alert");
                // Go into pursuit!
                return AIStateType.Pursuit;
            }

            
        }

        if (_guardStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _guardStateMachine.SetTarget(_guardStateMachine.AudioThreat);
            // Guard is alerted, play the alert effect
            StartCoroutine(_guardStateMachine.ShowAlertSymbol());
            _timer = _maxDuration;
        }

        if (_guardStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _guardStateMachine.SetTarget(_guardStateMachine.VisualThreat);
            _timer = _maxDuration;
        }


        float angle;

        if (_guardStateMachine.targetType == AITargetType.Audio && !_guardStateMachine.isTargetReached)
        {
            angle = AIState.FindSignedAngle(_guardStateMachine.transform.forward,
                _guardStateMachine.targetPosition - _guardStateMachine.transform.position);

            if (_guardStateMachine.targetType == AITargetType.Audio && Mathf.Abs(angle) < _threatAngleThreshold)
            {

                return AIStateType.Pursuit;
            }

            if (_directionChangeTimer > _directionChangeTime)
            {
                if (Random.value < _guardStateMachine.intelligence)
                {
                    _guardStateMachine.seeking = (int)Mathf.Sign(angle);
                }
                else
                {
                    _guardStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                }

                _directionChangeTimer = 0.0f;
            }
        }
        else
        if (_guardStateMachine.targetType == AITargetType.Waypoint && !_guardStateMachine.navAgent.pathPending)
        {

            angle = AIState.FindSignedAngle(_guardStateMachine.transform.forward,
                _guardStateMachine.navAgent.steeringTarget - _guardStateMachine.transform.position);

            if (Mathf.Abs(angle) < _waypointAngleThreshold)
                return AIStateType.Patrol;
            if (_directionChangeTimer > _directionChangeTime)
            {
                _guardStateMachine.seeking = (int)Mathf.Sign(angle);
                _directionChangeTimer = 0.0f;
            }
        }
        else
        {
            if (_directionChangeTimer > _directionChangeTime)
            {
                _guardStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                _directionChangeTimer = 0.0f;
            }
        }


        if (!_guardStateMachine.useRootRotation) _guardStateMachine.transform.Rotate(new Vector3(0.0f, _slerpSpeed * _guardStateMachine.seeking * Time.deltaTime, 0.0f));

        return AIStateType.Alerted;
    }
}
