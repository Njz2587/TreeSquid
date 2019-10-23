using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Public Enums of the AI System
public enum AIStateType { None, Idle, Alerted, Patrol, Wander, Attack, Pursuit, Dead, Stunned}
public enum AITargetType { None, Waypoint, WanderPoint, Visual_Player, Visual_Light, Visual_Food, Audio }
public enum AITriggerEventType { Enter, Stay, Exit }
public enum AIBoneAlignmentType { XAxis, YAxis, ZAxis, XAxisInverted, YAxisInverted, ZAxisInverted }

/// <summary>
/// Describes a potential target to the AI
/// </summary>
public struct AITarget
{
    private AITargetType _type;         // The type of target
    private Collider _collider;     // The collider
    private Vector3 _position;      // Current position in the world
    private float _distance;        // Distance from player
    private float _time;            // Time the target was last ping'd

    public AITargetType type { get { return _type; } }
    public Collider collider { get { return _collider; } }
    public Vector3 position { get { return _position; } }
    public float distance { get { return _distance; } set { _distance = value; } }
    public float time { get { return _time; } }

    public void Set(AITargetType t, Collider c, Vector3 p, float d)
    {
        _type = t;
        _collider = c;
        _position = p;
        _distance = d;
        _time = Time.time;
    }

    public void Clear()
    {
        _type = AITargetType.None;
        _collider = null;
        _position = Vector3.zero;
        _time = 0.0f;
        _distance = Mathf.Infinity;
    }
}

/// <summary>
/// Base class for all AI State Machines
/// </summary>
public abstract class AIStateMachine : MonoBehaviour
{
    // Public Variables
    public AITarget VisualThreat = new AITarget();
    public AITarget AudioThreat = new AITarget();

    // Protected Variables
    protected AIState _currentState = null;
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();
    protected int _rootPositionRefCount = 0;
    protected int _rootRotationRefCount = 0;
    public bool _isTargetReached = false;
    protected List<Rigidbody> _bodyParts = new List<Rigidbody>();
    protected int _aiBodyPartLayer = 11;

    // Animation Layer Manager
    protected Dictionary<string, bool> _animLayersActive = new Dictionary<string, bool>();

    // Protected Inspector Assigned Variables
    [SerializeField] protected AIStateType _currentStateType = AIStateType.Idle;
    [SerializeField] protected Transform _rootBone = null;
    [SerializeField] protected AIBoneAlignmentType _rootBoneAlignment = AIBoneAlignmentType.ZAxis;
    [SerializeField] protected SphereCollider _targetTrigger = null;
    [SerializeField] protected SphereCollider _sensorTrigger = null;
    [SerializeField] protected AIWaypointNetwork _waypointNetwork = null;
    [SerializeField] protected bool _randomPatrol = false;
    [SerializeField] protected int _currentWaypoint = -1;
    [SerializeField]
    [Range(0, 15)]
    protected float _stoppingDistance = 1.0f;

    // Layered Audio Control
    protected ILayeredAudioSource _layeredAudioSource = null;

    // Component Cache
    protected Animator _animator = null;
    protected UnityEngine.AI.NavMeshAgent _navAgent = null;
    protected Collider _collider = null;
    protected Transform _transform = null;

    // Public Properties
    public bool IsAwake { get; set; }
    public bool isTargetReached { get { return _isTargetReached; } }
    public AIStateType currentStateType { get { return _currentStateType; } }
    public bool inMeleeRange { get; set; }
    public Animator animator { get { return _animator; } }
    public NavMeshAgent navAgent { get { return _navAgent; } }
    public Vector3 sensorPosition
    {
        get
        {
            if (_sensorTrigger == null) return Vector3.zero;
            Vector3 point = _sensorTrigger.transform.position;
            point.x += _sensorTrigger.center.x * _sensorTrigger.transform.lossyScale.x;
            point.y += _sensorTrigger.center.y * _sensorTrigger.transform.lossyScale.y;
            point.z += _sensorTrigger.center.z * _sensorTrigger.transform.lossyScale.z;
            return point;
        }
    }

    public float sensorRadius
    {
        get
        {
            if (_sensorTrigger == null) return 0.0f;
            float radius = Mathf.Max(_sensorTrigger.radius * _sensorTrigger.transform.lossyScale.x,
                                        _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.y);

            return Mathf.Max(radius, _sensorTrigger.radius * _sensorTrigger.transform.lossyScale.z);
        }
    }

    public bool useRootPosition { get { return _rootPositionRefCount > 0; } }
    public bool useRootRotation { get { return _rootRotationRefCount > 0; } }
    public AITargetType targetType { get { return _target.type; } }
    public SphereCollider targetTrigger { get { return _targetTrigger; } }
    public Vector3 targetPosition { get { return _target.position; } }
    public int targetColliderID
    {
        get
        {
            if (_target.collider)
                return _target.collider.GetInstanceID();
            else
                return -1;
        }
    }


    /// <summary>
    /// Sets an animation layer as active or inactive
    /// </summary>
    /// <param name="layerName"></param>
    /// <param name="active"></param>
    public void SetLayerActive(string layerName, bool active)
    {
        _animLayersActive[layerName] = active;
        if (active == false && _layeredAudioSource != null)
            _layeredAudioSource.Stop(_animator.GetLayerIndex(layerName));
    }

    /// <summary>
    /// Checks whether the specified animation layer is active
    /// </summary>
    /// <param name="layerName"></param>
    /// <returns></returns>
    public bool IsLayerActive(string layerName)
    {
        bool result;
        if (_animLayersActive.TryGetValue(layerName, out result))
        {
            return result;
        }
        return false;
    }


    public bool PlayAudio(AudioCollection clipPool, int bank, int layer, bool looping = true)
    {
        if (_layeredAudioSource == null) return false;
        return _layeredAudioSource.Play(clipPool, bank, layer, looping);
    }

    public void StopAudio(int layer)
    {
        if (_layeredAudioSource != null)
            _layeredAudioSource.Stop(layer);
    }

    public void MuteAudio(bool mute)
    {
        if (_layeredAudioSource != null)
            _layeredAudioSource.Mute(mute);
    }

    /// <summary>
    /// Caches all components of the state machine
    /// </summary>
    protected virtual void Awake()
    {
        // Cache all frequently accessed components
        _transform = transform;
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _collider = GetComponent<Collider>();

        // Cache Audio Source Reference for Layered AI Audio
        AudioSource audioSource = GetComponent<AudioSource>();

        // Get BodyPart Layer
        _aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

        // Do we have a valid Game Scene Manager
        if (AIManager.Instance != null)
        {
            // Register State Machines with Scene Database
            if (_collider) AIManager.Instance.RegisterAIStateMachine(_collider.GetInstanceID(), this);
            if (_sensorTrigger) AIManager.Instance.RegisterAIStateMachine(_sensorTrigger.GetInstanceID(), this);
        }

        if (_rootBone != null)
        {
            // Get all the rigid bodies in the bone hierarchy
            Rigidbody[] bodies = _rootBone.GetComponentsInChildren<Rigidbody>();
            // Loop through each of the body parts
            foreach (Rigidbody bodyPart in bodies)
            {
                // Make sure the body part is valid and its layer is the AI Body Part Layer
                if (bodyPart != null && bodyPart.gameObject.layer == _aiBodyPartLayer)
                {
                    // Add this body part to the list of body parts
                    _bodyParts.Add(bodyPart);
                    // Register the state machine
                    AIManager.Instance.RegisterAIStateMachine(bodyPart.GetInstanceID(), this);
                }
            }
        }

        /*
        // Register the Layered Audio Source
        if (_animator && audioSource && AudioManager.instance)
        {
            _layeredAudioSource = AudioManager.instance.RegisterLayeredAudioSource(audioSource, _animator.layerCount);
        }
        */
    }

    /// <summary>
    /// Set up the object
    /// </summary>
    protected virtual void Start()
    {
        // Set the sensor trigger's parent to this state machine
        if (_sensorTrigger != null)
        {
            AISensor sensorScript = _sensorTrigger.GetComponent<AISensor>();
            if (sensorScript != null)
            {
                sensorScript.parentStateMachine = this;
            }
        }


        // Fetch all states on this game object
        AIState[] states = GetComponents<AIState>();

        // Loop through all states and add them to the state dictionary
        foreach (AIState state in states)
        {
            if (state != null && !_states.ContainsKey(state.GetStateType()))
            {
                // Add this state to the state dictionary
                _states[state.GetStateType()] = state;

                // And set the parent state machine of this state
                state.SetStateMachine(this);
            }
        }

        // Set the current state
        if (_states.ContainsKey(_currentStateType))
        {
            _currentState = _states[_currentStateType];
            _currentState.OnEnterState();
        }
        else
        {
            _currentState = null;
        }

        // Fetch all AIStateMachineLink derived behaviours from the animator and set their State Machine references to this state machine
        if (_animator)
        {
            AIStateMachineLink[] scripts = _animator.GetBehaviours<AIStateMachineLink>();
            foreach (AIStateMachineLink script in scripts)
            {
                script.stateMachine = this;
            }
        }
    }

    /// <summary>
    /// Allows any external method to force the AI out of its current state and into the specified state
    /// </summary>
    /// <param name="state"></param>
    public void SetStateOverride(AIStateType state)
    {
        // Set the current state
        if (state != _currentStateType && _states.ContainsKey(state))
        {
            if (_currentState != null)
                _currentState.OnExitState();

            _currentState = _states[state];
            _currentStateType = state;
            _currentState.OnEnterState();
        }
    }

    /// <summary>
    /// Gets the world space position of the state machine's current set waypoint
    /// </summary>
    /// <param name="increment">An optional increment</param>
    /// <returns></returns>
    public Vector3 GetWaypointPosition(bool increment)
    {
        if (_currentWaypoint == -1)
        {
            if (_randomPatrol)
                _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);
            else
                _currentWaypoint = 0;
        }
        else if (increment)
        {
            Debug.Log("Next Waypoint called!");
            NextWaypoint();
        }


        // Fetch the new waypoint from the waypoint list
        if (_waypointNetwork.Waypoints[_currentWaypoint] != null)
        {
            Transform newWaypoint = _waypointNetwork.Waypoints[_currentWaypoint];

            // This is our new target position
            SetTarget(AITargetType.Waypoint,
                        null,
                        newWaypoint.position,
                        Vector3.Distance(newWaypoint.position, transform.position));

            return newWaypoint.position;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Called to select a new waypoint, wihch can either be randomly selected or incremented to visit waypoints in sequence
    /// Sets the new waypoint as the target and generates a nav agent path for it
    /// </summary>
    private void NextWaypoint()
    {
        // Increase the current waypoint with wrap-around to zero (or choose a random waypoint)
        if (_randomPatrol && _waypointNetwork.Waypoints.Count > 1)
        {
            // Keep generating random waypoint until we find one that isn't the current one
            // NOTE: Very important that waypoint networks do not only have one waypoint :)
            int oldWaypoint = _currentWaypoint;
            while (_currentWaypoint == oldWaypoint)
            {
                _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);
            }
        }
        else
        {
            Debug.Log("Next Waypoint Called: " + _currentWaypoint);
            _currentWaypoint = _currentWaypoint == _waypointNetwork.Waypoints.Count - 1 ? 0 : _currentWaypoint + 1;
            Debug.Log("New Waypoint: " + _currentWaypoint);
        }
       
    }

    /// <summary>
    /// (Overload) Sets the current target and configures the target trigger
    /// </summary>
    /// <param name="t">Type of the target</param>
    /// <param name="c">Collider of the target</param>
    /// <param name="p">Position of the target</param>
    /// <param name="d">Distance to the target</param>
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        // Set the target info
        _target.Set(t, c, p, d);

        // Configure and enable the target trigger at the correct
        // position and with the correct radius
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    /// <summary>
    /// Sets the current target and cofnigures the target trigger
    /// This variant allows for specifying a custom stopping distance
    /// </summary>
    /// <param name="t">Type of the target</param>
    /// <param name="c">Collider of the target</param>
    /// <param name="p">Position of the target</param>
    /// <param name="d">Distance to the target</param>
    /// <param name="s">Stopping distance</param>
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s)
    {
        // Set the target Data
        _target.Set(t, c, p, d);

        // Configure and enable the target trigger at the correct
        // position and with the correct radius
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = s;
            _targetTrigger.transform.position = _target.position;
            _targetTrigger.enabled = true;
        }
    }

    /// <summary>
    /// Sets the current target and configures target trigger
    /// </summary>
    /// <param name="t"></param>
    public void SetTarget(AITarget t)
    {
        // Assign the new target
        _target = t;

        // Configure and enable the target trigger at the correct
        // position and with the correct radius
        if (_targetTrigger != null)
        {
            _targetTrigger.radius = _stoppingDistance;
            _targetTrigger.transform.position = t.position;
            _targetTrigger.enabled = true;
        }
    }

    /// <summary>
    /// Clears the current target
    /// </summary>
    public void ClearTarget()
    {
        _target.Clear();
       
        
        if (_targetTrigger != null)
        {
            _targetTrigger.enabled = false;
        }
        
        
    }

    /// <summary>
    /// Clears the audio and visual threats with each tick of the Physics system
    /// Re-calculates distance to the current target
    /// </summary>
    protected virtual void FixedUpdate()
    {
        // Clear the visual threat
        VisualThreat.Clear();
        // Clear the audio target
        AudioThreat.Clear();

        if (_target.type != AITargetType.None)
        {
            _target.distance = Vector3.Distance(_transform.position, _target.position);
        }

        _isTargetReached = false;
    }


    /// <summary>
    /// Called by Unity each frame, gives the current state a chance to update itself and perform transitions
    /// </summary>
    protected virtual void Update()
    {
        //Debug.Log("Root Position Ref Count "+_rootPositionRefCount);

        if (_currentState == null) return;

        AIStateType newStateType = _currentState.OnUpdate();
        if (newStateType != _currentStateType)
        {
            AIState newState = null;
            if (_states.TryGetValue(newStateType, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }
            else
            if (_states.TryGetValue(AIStateType.Idle, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();
                _currentState = newState;
            }

            _currentStateType = newStateType;
        }
    }

    /// <summary>
    /// Called by Physics system when the AI's main collider enter its trigger
    /// Allows the child state to knwo when it has entered the sphere of influence of a waypoint or last player sighted location
    /// </summary>
    /// <param name="other"></param>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) return;

        _isTargetReached = true;
        // Notify Child State
        if (_currentState)
            _currentState.OnDestinationReached(true);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (_targetTrigger == null || other != _targetTrigger) return;

        _isTargetReached = true;
    }

    /// <summary>
    /// Called by PHysics system when the AI's main collider exits its trigger
    /// Informs the child state that the AI entity is no longer at its destination
    /// Usually occurs when a new target has been set by the child
    /// </summary>
    /// <param name="other"></param>
    protected void OnTriggerExit(Collider other)
    {
        if (_targetTrigger == null || _targetTrigger != other) return;

        _isTargetReached = false;

        if (_currentState != null)
            _currentState.OnDestinationReached(false);
    }


    /// <summary>
    /// Called by the AISensor when an AI aggravator has entered/exited the sensor trigger
    /// </summary>
    /// <param name="type"></param>
    /// <param name="other"></param>
    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
    {
        if (_currentState != null)
            _currentState.OnTriggerEvent(type, other);
    }

    /// <summary>
    /// Called by Unity after root motion has been evaluated, but not yet applied to object
    /// Allows us to determine via code what to do with root motion information
    /// </summary>
    protected virtual void OnAnimatorMove()
    {
        if (_currentState != null)
            _currentState.OnAnimatorUpdated();
    }

    /// <summary>
    /// Called by Unity just before IK system is updated
    /// Gives us a change to setup IK Targets and weights
    /// </summary>
    /// <param name="layerIndex"></param>
    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (_currentState != null)
            _currentState.OnAnimatorIKUpdated();
    }

    /// <summary>
    /// Configures the NavMeshAgent to enable/disable auto updates of position/rotation to our transform
    /// </summary>
    /// <param name="positionUpdate"></param>
    /// <param name="rotationUpdate"></param>
    public void NavAgentControl(bool positionUpdate, bool rotationUpdate)
    {
        if (_navAgent)
        {
            _navAgent.updatePosition = positionUpdate;
            _navAgent.updateRotation = rotationUpdate;
        }
    }

    /// <summary>
    /// Called by the State Machine Behaviours to Enable/Disable root motion
    /// </summary>
    /// <param name="rootPosition"></param>
    /// <param name="rootRotation"></param>
    public void AddRootMotionRequest(int rootPosition, int rootRotation)
    {
        _rootPositionRefCount += rootPosition;
        _rootRotationRefCount += rootRotation;

        //Debug.Log("Adding Root Motion Request "+rootPosition+"   and    "+rootRotation);
    }


    /// <summary>
    /// Called upon the destruction of the object
    /// Unregisters the audio sources
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_layeredAudioSource != null && AudioManager.instance)
        {
            AudioManager.instance.UnregisterLayeredAudioSource(_layeredAudioSource);
        }
    }
}
