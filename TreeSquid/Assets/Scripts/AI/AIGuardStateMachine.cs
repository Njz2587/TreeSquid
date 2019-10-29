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
    [SerializeField] [Range(0, 100)] int _minForceToKnockOut = 10;
    [SerializeField] [Range(0.0f, 1.0f)] float _intelligence = 0.5f;
    [SerializeField] [Range(0.0f, 50.0f)] float _alarmRadius = 20.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _alarmChance = 1.0f;
    [SerializeField] AIAlarmPosition _alarmPosition = AIAlarmPosition.Entity;
    [SerializeField] AISoundEmitter _alarmPrefab = null;
    [SerializeField] AudioCollection _ragdollCollection = null;

    [SerializeField] [Range(0.0f, 1.0f)] float _symbolDuration = 0.25f;
    [SerializeField] LayerMask _geometryLayers = 0;

    [SerializeField] GameObject _alertSymbol;
    [SerializeField] GameObject _alarmSymbol;
    [SerializeField] GameObject _torchObject;

    // Private Variables
    private int _seeking = 0;
    private int _attackType = 0;
    private float _speed = 0.0f;
    private float _isAlarming = 0.0f;
    private bool _isInvestigating = false;
    private bool _swordDrawn = false;
    private float _nextRagdollSoundTime = 0.0f;
    // Ragdoll Stuff
    private AIBoneControlType _boneControlType = AIBoneControlType.Animated;
    private List<BodyPartSnapshot> _bodyPartSnapShots = new List<BodyPartSnapshot>();
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
    public int minForceToKnockOut { get { return _minForceToKnockOut; } set { _minForceToKnockOut = value; } }
    public int attackType { get { return _attackType; } set { _attackType = value; } }
    public int seeking { get { return _seeking; } set { _seeking = value; } }
    public float alarmChance { get { return _alarmChance; } }
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }
    public bool isAlarming
    {
        get { return _isAlarming > 0.1f; }
    }
    public bool isInvestigating
    {
        get { return _isInvestigating; }
        set { _isInvestigating = value; }
    }
    public bool swordDrawn
    {
        get { return _swordDrawn; }
        set { _swordDrawn = value; }
    }
    public bool PlayerIsVisible { get; set; }
    public GameObject AlertSymbol { get { return _alertSymbol; } }
    public GameObject AlarmSymbol { get { return _alarmSymbol; } }
    public GuardHeadControl HeadControl { get; set; }
    public AILineOfSightCone SightCone { get; set; }

    protected override void Start()
    {
        // Call base Start functionality
        base.Start();

        // Set IsAwake to true by default
        IsAwake = true;
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
        /*
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsAwake)
            {
                KnockOut();
            }
        }
        */
        // Make sure the animator is valid, and if so, update animator with state machine values
        if (_animator != null && IsAwake)
        {
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
            _animator.SetInteger(_stateHash, (int)_currentStateType);

            /*
            // Are we alarming or not
            _isAlarming= IsLayerActive("Cinematic") ? 0.0f : _animator.GetFloat(_alarmingHash);
            */

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
            // Check if the magnitude of the force is enough to knock out the guard
            if (inputtedForce.magnitude >= minForceToKnockOut)
            {
                // Knock out the guard
                KnockOut();
            }
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
            Debug.Log("Died");
            // Set IsAwake to false
            IsAwake = false;
            _currentState = null;

            
            // Activate the ragdoll!
            foreach (Rigidbody body in _bodyParts)
            {
                body.useGravity = true;
                body.isKinematic = false;
            }
            // Disable the animator
            _animator.enabled = false;
            _navAgent.enabled = false;

            // Disable the GuardHeadControl component
            HeadControl.enabled = false;
            // Check if we have a torch
            if (_torchObject)
            {
                // Unparent the torch
                _torchObject.transform.parent = null;
                // Get the rigidbody
                Rigidbody torchRigidbody = _torchObject.GetComponent<Rigidbody>();
                // Make sure the rigidbody is valid
                if (torchRigidbody)
                {
                    // Make it not kinematic
                    torchRigidbody.isKinematic = false;
                    // Enable gravity
                    torchRigidbody.useGravity = true;
                }
            }
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

    #region Effect-Related Functionality
    public IEnumerator ShowAlertSymbol()
    {
        _alertSymbol.SetActive(true);
        yield return new WaitForSeconds(_symbolDuration);
        _alertSymbol.SetActive(false);
    }

    public IEnumerator ShowAlarmSymbol()
    {
        _alarmSymbol.SetActive(true);
        yield return new WaitForSeconds(_symbolDuration);
        _alarmSymbol.SetActive(false);
    }
    #endregion
}
