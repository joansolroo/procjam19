using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class advertisment1 : MonoBehaviour
{
    public TextMeshPro frontText;
    public TextMeshPro backText;
    public Gradient neonColor;

    public string[] textList;

    public void SetText(string text)
    {
        frontText.text = text;
        backText.text = text;
    }
    public void SetColor(Color c)
    {
        frontText.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, c);
        backText.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, c);
        c.a = 0.1f;
        frontText.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, c);
        backText.fontMaterial.SetColor(ShaderUtilities.ID_FaceColor, c);
    }

    void Start()
    {
        SetText(textList[Random.Range(0, textList.Length)]);
        SetColor(neonColor.Evaluate(Random.Range(0f, 1f)));
    }
}
