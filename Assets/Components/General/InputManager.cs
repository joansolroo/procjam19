using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Input
{
    public static InputManager active;

    [Header("Properties")]
    [SerializeField] public string target = "Webgl-QWERTY";

    [Header("Controls")]
    [SerializeField] public KeyCode key_cancel = KeyCode.Escape;
    [SerializeField] public KeyCode key_pause = KeyCode.P;

    /** TODO: Override all methods relevant to language / target build here **/ 
}
