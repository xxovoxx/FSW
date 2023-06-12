using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "MobsData", menuName = "ScriptableObjectData/MobsData", order = 1)]
public class MobsScriptableObject : ScriptableObject
{
    [Header("Collision box")]
    public float width;
    public float standHeight;
    public float crouchHeight;

    public float cameraHeight;

    [Header("Interaction with the environment")]
    public float groundMoveAcceleration;
    public float airMoveAcceleration;
    public float groundDrag;
    public float airDrag;
    public LayerMask ground;

    [Header ("Mob attribute")]
    public float moveSpeed;
    public float crouchSpeed;
    public float sprintSpeed;
    public float jumpAcceleration;
}