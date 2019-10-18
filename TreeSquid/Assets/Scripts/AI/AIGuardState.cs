using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIGuardState : AIState
{
    // Private
    protected int _playerLayerMask = 18;
    protected int _bodyPartLayer = 16;
    protected int _visualLayerMask = 14;
    protected AIGuardStateMachine _guardStateMachine = null;
    private bool playerComponentSpotted;

    void Awake()
    {
        // Get a mask for line of sight testing with the player
        _playerLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Walkable") + 1;
        _visualLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Visual Aggravator") + 1;

        // Get the layer index of the AI Body Part layer
        _bodyPartLayer = LayerMask.NameToLayer("AI Body Part");
    }

    /// <summary>
    /// Checks for type compliance and stores the reference as the derived type
    /// </summary>
    /// <param name="stateMachine"></param>
    public override void SetStateMachine(AIStateMachine stateMachine)
    {
        if (stateMachine.GetType() == typeof(AIGuardStateMachine))
        {

            base.SetStateMachine(stateMachine);
            _guardStateMachine = (AIGuardStateMachine) stateMachine;

        }
    }

    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other)
    {
        // If we don't have a parent state machine then bail
        if (_guardStateMachine == null)
            return;

        // Check to see if we are in stunned state
        if (_guardStateMachine.currentStateType != AIStateType.Stunned)
        {
            // We are not interested in exit events so only step in and process if its anenter or stay
            if (eventType != AITriggerEventType.Exit)
            {
                // What is the type of the current visual threat we have stored
                AITargetType curType = _guardStateMachine.VisualThreat.type;
                // Is the collider that has entered our sensor a player
                if (other.CompareTag("PlayerMainCollider") && CoreGameManager.Instance.playerManager.playerHealthControl.IsDead == false)
                {
                    // Get distance from the sensor origin to the collider
                    float distance = Vector3.Distance(_guardStateMachine.sensorPosition, other.transform.position);

                    // If the currently stored threat is not a player or if this player is closer than a player
                    // previously stored as the visual threat...this could be more important
                    if (curType != AITargetType.Visual_Player ||
                        (curType == AITargetType.Visual_Player && distance < _guardStateMachine.VisualThreat.distance))
                    {
                        // Is the collider within our view cone and do we have line or sight
                        RaycastHit hitInfo;
                        if (ColliderIsVisible(other, out hitInfo, _playerLayerMask))
                        {
                            _guardStateMachine.playerIsVisible = true;
                            // Yep...it's close and in our FOV and we have line of sight so store as the current most dangerous threat
                            _guardStateMachine.VisualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distance);
                        }
                        else
                        {
                            _guardStateMachine.playerIsVisible = false;
                        }
                    }
                    /*
                    // Get distance from the sensor origin to the collider
                    float distance = Vector3.Distance(_swordHuskStateMachine.sensorPosition, CoreGameManager.Instance.playerManager.PlayerModel.transform.position);

                    // If the currently stored threat is not a player or if this player is closer than a player
                    // previously stored as the visual threat...this could be more important
                    if (curType != AITargetType.Visual_Player && CoreGameManager.Instance.playerManager.playerHealthControl.IsDead == false ||
                        (curType == AITargetType.Visual_Player && distance < _swordHuskStateMachine.VisualThreat.distance && CoreGameManager.Instance.playerManager.playerHealthControl.IsDead == false))
                    {
                        // Is the collider within our view cone and do we have line or sight
                        RaycastHit hitInfo;
                        if (ColliderIsVisible(other, out hitInfo, _playerLayerMask))
                        {
                            Debug.Log("Spotted Player!");
                            // Yep...it's close and in our FOV and we have line of sight so store as the current most dangerous threat
                            _swordHuskStateMachine.VisualThreat.Set(AITargetType.Visual_Player, CoreGameManager.Instance.playerManager.PlayerModel.GetComponent<CapsuleCollider>(), CoreGameManager.Instance.playerManager.PlayerModel.transform.position, distance);
                        }
                    }
                    */
                }
                else
                if (other.CompareTag("AI Sound Emitter"))
                {
                    SphereCollider soundTrigger = (SphereCollider)other;
                    if (soundTrigger == null) return;

                    // Get the position of the Agent Sensor 
                    Vector3 agentSensorPosition = _guardStateMachine.sensorPosition;

                    Vector3 soundPos;
                    float soundRadius;
                    AIState.ConvertSphereColliderToWorldSpace(soundTrigger, out soundPos, out soundRadius);
                    // How far inside the sound's radius are we
                    float distanceToThreat = (soundPos - agentSensorPosition).magnitude;
                    //Debug.Log("Distance to threat: " + distanceToThreat);
                    // Calculate a distance factor such that it is 1.0 when at sound radius 0 when at center
                    float distanceFactor = (distanceToThreat / soundRadius);
                    //Debug.Log("Distance factor: " + distanceFactor);

                    // Bias the factor based on hearing ability of Agent.
                    distanceFactor += distanceFactor * (1.0f - _guardStateMachine.hearing);
                    //Debug.Log("Distance Factor after hearing bias: " + distanceFactor);
                    float test = distanceFactor * 0.1f;
                    // Too far away
                    /*
                    if (distanceFactor> 1.0f)
                    {
                        //Debug.Log("Distance Factor: " + distanceFactor);
                        Debug.Log("Sound was too far away!");
                        return;
                    }
                    */


                    // if We can hear it and is it closer then what we previously have stored
                    if (distanceToThreat < _guardStateMachine.AudioThreat.distance)
                    {
                        //Debug.Log("Skeleton can hear audio threat!");
                        // Most dangerous Audio Threat so far
                        _guardStateMachine.AudioThreat.Set(AITargetType.Audio, other, soundPos, distanceToThreat);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Tests the passed collider against the Sword Husk's FOV and uses the passed layer mask for line of sight testing
    /// </summary>
    /// <param name="other"></param>
    /// <param name="hitInfo"></param>
    /// <param name="layerMask"></param>
    /// <returns></returns>
    protected virtual bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask = -1)
    {
        // Let's make sure we have something to return
        hitInfo = new RaycastHit();

        // We need the state machine to be an AISkeletonStateMachine
        if (_guardStateMachine == null) return false;

        // Calculate the angle between the sensor origin and the direction of the collider
        //Vector3 head = _swordHuskStateMachine.eyeTransform.position;
        Vector3 head = _stateMachine.sensorPosition;
        Vector3 direction = other.transform.position - head;
        Debug.DrawLine(head, other.transform.position, Color.magenta);
        //Debug.DrawRay(head, _swordHuskStateMachine.eyeTransform.forward, Color.yellow, 0f, true);
        //float angle = Vector3.Angle(direction, _swordHuskStateMachine.eyeTransform.forward);
        float angle = Vector3.Angle(direction, transform.forward);
        // If the angle is greater than half our FOV then it is outside the view cone so
        // return false - no visibility
        if (angle > _guardStateMachine.fov * 0.5f)
        {
            // Return false
            return false;
        }


        // Now we need to test line of sight. Perform a ray cast from our sensor origin in the direction of the collider for distance
        // of our sensor radius scaled by the zombie's sight ability. This will return ALL hits.
        RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized, _guardStateMachine.sensorRadius * _guardStateMachine.sight, layerMask);

        // Find the closest collider that is NOT the AIs own body part. If its not the target then the target is obstructed
        float closestColliderDistance = float.MaxValue;
        Collider closestCollider = null;

        // Examine each hit
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            // Is this hit closer than any we previously have found and stored
            if (hit.distance < closestColliderDistance)
            {
                // If the hit is on the body part layer
                if (hit.transform.gameObject.layer == _bodyPartLayer)
                {
                    // And assuming it is not our own body part
                    if (_stateMachine != CoreGameManager.Instance.aiManager.GetAIStateMachine(hit.rigidbody.GetInstanceID()))
                    {
                        // Store the collider, distance and hit info.
                        closestColliderDistance = hit.distance;
                        closestCollider = hit.collider;
                        hitInfo = hit;
                    }
                }
                else
                {
                    // Its not a body part so simply store this as the new closest hit we have found
                    closestColliderDistance = hit.distance;
                    closestCollider = hit.collider;
                    hitInfo = hit;
                }
            }
        }

        // If the closest hit is the collider we are testing against, it means we have line-of-sight
        // so return true.
        if (closestCollider && closestCollider.gameObject == other.gameObject)
        {
            return true;
        }


        // otherwise, something else is closer to us than the collider so line-of-sight is blocked
        return false;
    }
}
