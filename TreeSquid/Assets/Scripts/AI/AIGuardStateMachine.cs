using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIBoneControlType { Animated, Ragdoll, RagdollToAnim }
public enum AIAlarmPosition { Entity, Player }

public class BodyPartSnapshot
{
    public Transform transform;
    public Vector3 position;
    public Quaternion rotation;
}

public class AIGuardStateMachine : AIStateMachine
{
    // Inspector Assigned Behaviour Variables
    [SerializeField] [Range(10.0f, 360.0f)] float _fov = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _sight = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _hearing = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _aggression = 0.5f;
    [SerializeField] [Range(0, 100)] int _health = 100;
    [SerializeField] [Range(0.0f, 1.0f)] float _intelligence = 0.5f;
    [SerializeField] [Range(0.0f, 50.0f)] float _alarmRadius = 20.0f;
    [SerializeField] AIAlarmPosition _alarmPosition = AIAlarmPosition.Entity;
    [SerializeField] AISoundEmitter _alarmPrefab = null;
    [SerializeField] AudioCollection _ragdollCollection = null;

    [SerializeField] float _replenishRate = 0.5f;
    [SerializeField] float _depletionRate = 0.1f;
    [SerializeField] float _reanimationBlendTime = 1.5f;
    [SerializeField] float _reanimationWaitTime = 3.0f;
    [SerializeField] LayerMask _geometryLayers = 0;

    // Private Variables
    private int _seeking = 0;
    private int _attackType = 0;
    private float _speed = 0.0f;
    private float _isAlarming = 0.0f;
    private float _nextRagdollSoundTime = 0.0f;
    // Ragdoll Stuff
    private AIBoneControlType _boneControlType = AIBoneControlType.Animated;
    private List<BodyPartSnapshot> _bodyPartSnapShots = new List<BodyPartSnapshot>();
    private float _ragdollEndTime = float.MinValue;
    private Vector3 _ragdollHipPosition;
    private Vector3 _ragdollFeetPosition;
    private Vector3 _ragdollHeadPosition;
    private IEnumerator _reanimationCoroutine = null;
    private float _mecanimTransitionTime = 0.1f;
    // Animator Hashes
    private int _speedHash = Animator.StringToHash("Speed");
    private int _seekingHash = Animator.StringToHash("Seeking");
    private int _attackHash = Animator.StringToHash("Attack");
    private int _alarmingHash = Animator.StringToHash("Alarming");
    private int _alarmHash = Animator.StringToHash("Alarm");
    private int _stateHash = Animator.StringToHash("State");
    private int _upperBodyLayer = -1;
    private int _lowerBodyLayer = -1;

    // Public Properties
    public float fov { get { return _fov; } }
    public float hearing { get { return _hearing; } }
    public float sight { get { return _sight; } }
    public float intelligence { get { return _intelligence; } }
    public int health { get { return _health; } set { _health = value; } }
    public int attackType { get { return _attackType; } set { _attackType = value; } }
    public int seeking { get { return _seeking; } set { _seeking = value; } }
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }
    public bool isAlarming
    {
        get { return _isAlarming > 0.1f; }
    }



    protected override void Start()
    {
        // Call base Start functionality
        base.Start();

        // Make sure animator is valid
        if (_animator != null)
        {
            // Cache Layer Indices
            _lowerBodyLayer = _animator.GetLayerIndex("Lower Body");
            _upperBodyLayer = _animator.GetLayerIndex("Upper Body");
        }

        // Create BodyPartSnapShot List
        if (_rootBone != null)
        {
            Transform[] transforms = _rootBone.GetComponentsInChildren<Transform>();
            foreach (Transform trans in transforms)
            {
                BodyPartSnapshot snapShot = new BodyPartSnapshot();
                snapShot.transform = trans;
                _bodyPartSnapShots.Add(snapShot);
            }
        }
    }


    protected override void Update()
    {
        // Call base Update functionality
        base.Update();

        // Make sure the animator is valid, and if so, update animator with state machine values
        if (_animator != null)
        {
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
            _animator.SetInteger(_stateHash, (int)_currentStateType);

            // Are we screaming or not
            _isAlarming= IsLayerActive("Cinematic") ? 0.0f : _animator.GetFloat(_alarmingHash);

        }
    }

    #region Player-Enemy Related Functionality

    /// <summary>
    /// Checks if an inputted force is enough to knock the guard out and performs functionality based on this
    /// </summary>
    /// <param name="inputtedForce"></param>
    public void CheckKnockOutForce(Vector3 inputtedForce)
    {
        // Before anything, make sure the guard is awake
        if (IsAwake)
        {
            /* ------ TO-DO: Calculate whether or not the force is enough to knock out the enemy */
        }
    }

    /// <summary>
    /// Disables state machine functionality and ragdolls the guard
    /// </summary>
    public void KnockOut()
    {
        // Make sure we ain't already knocked out
        if (IsAwake)
        {
            // Set IsAwake to false
            IsAwake = false;
            _currentState = null;
            // Disable the animator
            _animator.enabled = false;
            _navAgent.enabled = false;
            
 
           
            // Activate the ragdoll!
            foreach (Rigidbody body in _bodyParts)
            {
                body.useGravity = true;
                body.isKinematic = false;
            }
            // Disable the animator
            _animator.enabled = false;
            _navAgent.enabled = false;
        }
    }

    #endregion
    #region Enemy-Player Related Functionality
    public bool HandleAlert()
    {
        if (isAlarming) return true;
        if (_animator == null || IsLayerActive("Cinematic") || _alarmPrefab == null) return false;

        _animator.SetTrigger(_alarmHash);
        Vector3 spawnPos = _alarmPosition == AIAlarmPosition.Entity ? transform.position : VisualThreat.position;
        AISoundEmitter screamEmitter = Instantiate(_alarmPrefab, spawnPos, Quaternion.identity) as AISoundEmitter;

        if (screamEmitter != null)
            screamEmitter.SetRadius(_alarmRadius);
        return true;
    }
    #endregion
}
