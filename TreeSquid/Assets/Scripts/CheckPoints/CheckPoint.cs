using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    #region Variables & Inspector Options
    [Header("Objects To Reset")]
    public List<ResetObject> resetObjects; //All RigidBodys to reset once the player resets to this checkpoint

    [Header("Gizmo Control")]
    [Space(10)]
    public List<GizmoDrawObject> drawObjects; //0 = Checkpoint Zone | 1 = Spawn Point

    #region Stored Data
    [HideInInspector]
    public int CheckPointID;
    #endregion
    #endregion

    /// <summary>
    /// Calls the OnValidate methods of the Checkpoint & Spawn Point Gizmos
    /// </summary>
    private void OnValidate()
    {
        if (drawObjects != null && drawObjects.Count == 2)
        {
            drawObjects[0].OnValidate();
            drawObjects[1].OnValidate();
        }
    }

    /// <summary>
    /// Calls the OnDrawGizmos methods of the Checkpoint & Spawn Point Gizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        if(drawObjects != null && drawObjects.Count > 0)
        {         
            if(drawObjects.Count > 2)
            {
                drawObjects.RemoveAt(drawObjects.Count - 1);
            }
            else if (drawObjects.Count != 2)
            {
                Debug.LogError("ERROR! Checkpoint " + gameObject.name + " MUST have 2 DrawObjects");
            }
            else
            {
                drawObjects[0].defaultColor = Color.green;
                drawObjects[1].defaultColor = Color.yellow;

                drawObjects[0].OnDrawGizmos();
                drawObjects[1].OnDrawGizmos();
            }
        }
    }

    /// <summary>
    /// Calls the Start methods of the Checkpoint & Spawn Point Gizmos
    /// </summary>
    private void Start()
    {
        foreach(ResetObject resetObject in resetObjects)
        {
            resetObject.Start();
        }
    }

    /// <summary>
    /// If the player has collided with this checkpoint and,
    /// 1)  There is no current checkpoint saved
    /// 2)  This Checkpoint's ID doesn't match the currently saved checkpoint
    /// 2a) This Checkpoint's ID was not already added to the list of reached checkpoints
    /// </summary>
    /// <param name="col"></param>
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == 11) //Squid
        {
            if (PlayerVars.instance.mostRecentCheckPoint == null || PlayerVars.instance.mostRecentCheckPoint.CheckPointID != CheckPointID && !PlayerVars.instance.reachedCheckPoints.Contains(CheckPointID))
            {
                PlayerVars.instance.mostRecentCheckPoint = gameObject.GetComponent<CheckPoint>();
                PlayerVars.instance.reachedCheckPoints.Add(CheckPointID);
            }
        }
    }

    /// <summary>
    /// Resets the transforms of every object specified in the resetObjects list to their default position
    /// </summary>
    public void ResetObjects()
    {
        foreach (ResetObject resetObject in resetObjects)
        {
            //Debug.Log("Resetting " + resetObject.resetObject.name);
            resetObject.ResetTransform();
        }
    }
}

[System.Serializable]
public class ResetObject
{
    #region Variables & Inspector Options
    public GameObject resetObject; //Object To Watch

    #region Stored Values
    Vector3 defaultPosition; //Orignal Transform
    Quaternion defaultRotation; //Orignal Transform
    Vector3 defaultScale; //Orignal Transform
    #endregion
    #endregion

    /// <summary>
    /// Sets the default transform values
    /// </summary>
    public void Start()
    {
        defaultPosition = resetObject.transform.position;
        defaultRotation = resetObject.transform.rotation;
        defaultScale = resetObject.transform.localScale;
    }

    /// <summary>
    /// Resets the current transform values to the default values set in Start
    /// </summary>
    public void ResetTransform()
    {
        Rigidbody resetRigidBody = resetObject.GetComponent<Rigidbody>();
        if (resetRigidBody != null)
        {
            resetRigidBody.velocity = Vector3.zero;
        }
        resetObject.transform.position = defaultPosition;
        resetObject.transform.rotation = defaultRotation;
        resetObject.transform.localScale = defaultScale;
    }
}

[System.Serializable]
public class GizmoDrawObject
{
    #region Variables & Inspector Options
    public bool drawGizmo = true; //Check if this gizmo should draw
    public GizmoDrawMode gizmoDrawMode = GizmoDrawMode.SolidMesh;
    public GizmoMeshMode gizmoMeshMode = GizmoMeshMode.MeshFilter;
    public Color drawColor;  
    [ConditionalHide("FilterMode", true)]
    public MeshFilter filterMesh; //Mesh to draw override
    [ConditionalHide("SkinMode", true)]
    public SkinnedMeshRenderer skinMesh; //Mesh to draw override
    public Transform drawTransform; //Reference transform that should probably be a child object of this object
    public Vector3 drawRotationOverride; //Rotate Override to make sure mesh fits where you expect

    #region Stored Data
    public enum GizmoDrawMode { SolidMesh, WireMesh } //Visual mode this gizmo should draw with
    public enum GizmoMeshMode { MeshFilter, SkinMeshRenderer } //Type of renderer this gizmo should draw
    [HideInInspector]
    public Color defaultColor;
    [HideInInspector]
    public bool FilterMode = true, SkinMode;
    #endregion
    #endregion

    /// <summary>
    /// Update inspector tools based on mode
    /// </summary>
    public void OnValidate()
    {
        if (gizmoMeshMode == GizmoMeshMode.MeshFilter)
        {
            if (!FilterMode)
            {
                FilterMode = true;
            }

            if (SkinMode)
            {
                SkinMode = false;
            }
        }
        else if (gizmoMeshMode == GizmoMeshMode.SkinMeshRenderer)
        {
            if (FilterMode)
            {
                FilterMode = false;
            }

            if (!SkinMode)
            {
                SkinMode = true;
            }
        }
    }

    /// <summary>
    /// Draw gizmo based on variables defined above
    /// </summary>
    public void OnDrawGizmos()
    {
        if (drawGizmo)
        {
            if (drawColor != Color.clear)
            {
                Gizmos.color = drawColor;
            }
            else
            {
                Gizmos.color = defaultColor;
            }

            if (drawTransform != null)
            {
                if (filterMesh != null || skinMesh != null)
                {
                    Quaternion drawRotation = Quaternion.Euler(drawTransform.rotation.x + drawRotationOverride.x, drawTransform.rotation.y + drawRotationOverride.y, drawTransform.rotation.z + drawRotationOverride.z);

                    if (gizmoDrawMode == GizmoDrawMode.SolidMesh)
                    {
                        if (gizmoMeshMode == GizmoMeshMode.MeshFilter)
                        {
                            Gizmos.DrawMesh(filterMesh.sharedMesh, drawTransform.position, drawRotation, drawTransform.localScale);
                        }
                        else if (gizmoMeshMode == GizmoMeshMode.SkinMeshRenderer)
                        {
                            Gizmos.DrawMesh(skinMesh.sharedMesh, drawTransform.position, drawRotation, drawTransform.localScale);
                        }
                    }
                    else if (gizmoDrawMode == GizmoDrawMode.WireMesh)
                    {
                        if (gizmoMeshMode == GizmoMeshMode.MeshFilter)
                        {
                            Gizmos.DrawWireMesh(filterMesh.sharedMesh, drawTransform.position, drawTransform.rotation, drawTransform.localScale);
                        }
                        else if (gizmoMeshMode == GizmoMeshMode.SkinMeshRenderer)
                        {
                            Gizmos.DrawWireMesh(skinMesh.sharedMesh, drawTransform.position, drawTransform.rotation, drawTransform.localScale);
                        }
                    }
                }
                else
                {
                    if (gizmoDrawMode == GizmoDrawMode.SolidMesh)
                    {
                        Gizmos.DrawCube(drawTransform.position, drawTransform.localScale);
                    }
                    else if (gizmoDrawMode == GizmoDrawMode.WireMesh)
                    {
                        Gizmos.DrawWireCube(drawTransform.position, drawTransform.localScale);
                    }
                }

                Gizmos.DrawLine(drawTransform.position, drawTransform.position + drawTransform.forward * 2);
            }
        }
    }
}
