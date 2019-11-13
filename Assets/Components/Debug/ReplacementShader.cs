using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ReplacementShader : MonoBehaviour
{

    public Shader shader;
    public Color OverDrawColor;

    new Camera camera;
    private void OnValidate()
    {
        Shader.SetGlobalColor("_OverDrawColor", OverDrawColor);
        camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        if (shader != null)
        {
            camera.SetReplacementShader(shader, "");
        }
    }

    private void OnDisable()
    {
        camera.ResetReplacementShader();
    }
}
