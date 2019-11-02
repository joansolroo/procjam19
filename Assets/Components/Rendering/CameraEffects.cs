using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [SerializeField] Kino.AnalogGlitch analog;
    [SerializeField] Kino.DigitalGlitch digital;
    [SerializeField] AnimationCurve flashCurve;

    [SerializeField] AudioSource glitchSound;
    public delegate void EffectEvent();
    public EffectEvent OnEffectStart;
    public EffectEvent OnEffectDone;
    bool animating = false;

    public static CameraEffects main;

    private void Awake()
    {
        main = this;
    }
    /*
    public void Fade(float time)
    {
        if (!animating)
        {
            animating = true;
            StartCoroutine(DoFade(time));
        }

    }
    public void Blink(float time)
    {
        if (!animating)
        {
            animating = true;
            StartCoroutine(DoBlink(time));
        }
    }*/
    public void Flash(float time, float intensity)
    {
        if (!animating)
        {
            StartCoroutine(DoFlash(time,intensity));
        }

    }
    /*
    public bool IsAnimating()
    {
        return animating;
    }
    IEnumerator DoFade(float time)
    {
        //if (!animating)
        {
            OnEffectStart?.Invoke();

            animating = true;
            Color initialColor = renderer.color;
            Color targetColor = new Color(0, 0, 0, 0);
            float t = 0;
            while (t < time)
            {
                renderer.color = Interpolate(initialColor, targetColor, t / time);
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }
            renderer.color = targetColor;

            animating = false;
            OnEffectDone?.Invoke();
        }
    }
    IEnumerator DoBlink(float time)
    {
        //if (!animating)
        {
            OnEffectStart?.Invoke();

            animating = true;
            Color initialColor = renderer.color;
            Color currentColor = initialColor;
            float t = 0;
            while (animating && t < time)
            {
                currentColor.a = ((int)((Mathf.Cos(t * 360 * 5f) * 0.5f + 0.5f) * 10)) / 10f;
                renderer.color = currentColor;
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }
            renderer.color = initialColor;
            animating = false;

            OnEffectDone?.Invoke();
        }
    }*/
    IEnumerator DoFlash(float time, float intensity)
    {
        if (!animating)
        {
            OnEffectStart?.Invoke();

            animating = true;
            //Color initialColor = renderer.color;
           // Color currentColor = initialColor;
            //Color targetColor = color;
            float t = 0;

            float volume = glitchSound.volume;
            glitchSound.pitch = Random.Range(0.5f, 0.8f);
            glitchSound.Play();
            float curve;
            while (animating && t < time)
            {
                float nTime = t / time;
                curve = flashCurve.Evaluate(nTime);
                glitchSound.volume = curve * volume;
                analog.colorDrift = curve*intensity;
                digital.intensity = curve * intensity/4;
                //renderer.color = currentColor;
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
            }
            glitchSound.volume = volume;
            //renderer.color = initialColor;
            animating = false;
            analog.colorDrift = 0;
            digital.intensity = 0;
            OnEffectDone?.Invoke();
        }
    }
    /*
    Color Interpolate(Color a, Color b, float t)
    {
        float tPos = (Mathf.Round(t * posterize)) / posterize;
        return Color.Lerp(a, b, tPos);
    }*/
}
