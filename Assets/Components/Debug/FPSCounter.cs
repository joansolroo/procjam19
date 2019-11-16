using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] bool visible = false;
    float deltaTime = 0.0f;
    List<float> timebuffer = new List<float>();
    List<float> fpsbuffer = new List<float>();
    void Update()
    {
        deltaTime = Time.deltaTime;
        //deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        if(Input.GetKeyDown(KeyCode.P))
        {
            visible = !visible;
        }
    }

    void OnGUI()
    {
        if (visible)
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = Color.white;
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            timebuffer.Add(msec);
            fpsbuffer.Add(fps);
            if (timebuffer.Count > 120)
            {
                timebuffer.RemoveAt(0);
                fpsbuffer.RemoveAt(0);
            }
            float avgt = 0;
            foreach (float t in timebuffer) {
                avgt += t;
            }
            avgt /= timebuffer.Count;
            float avgFps = 0;
            foreach (float t in fpsbuffer)
            {
                avgFps += t;
            }
            avgFps /= timebuffer.Count;

            string text = string.Format("{0:0.0} ms, avg {0:0.0} ms, ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
            Rect rect2 = new Rect(0, 20, w, h * 2 / 100);
            text = string.Format("avg {0:0.0} ms, ({1:0.} fps)", avgt, avgFps);
            GUI.Label(rect2, text, style);

        }
    }

}
