using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : TerrainElement
{
    public GameObject blocTemplate;
    public GameObject windowTemplate;
    public Gradient windowColorGradient;

    public Texture[] textureList;
    public Vector3[] textureWindowSizeList;


    public void Resize(Vector3 newsize)
    {
        this.transform.localScale = Vector3.one;
        size = newsize;
    }


    public void Init(int b = 3)
    {
        float roundy = 10f;
        //float f = 0.5f / roundy - 0.005f;
        float epsilon = 0.0001f;
        int textureIndex = Random.Range(0, textureList.Length);

        List<GameObject> blocs = new List<GameObject>();
        for (int i=0; i<b; i++)
        {
            GameObject go = Instantiate(blocTemplate);
            go.transform.parent = transform;
            go.SetActive(true);

            float sidesize = Random.Range(0.6f, 0.9f) * size.x;
            Vector3 s = new Vector3(sidesize, (i==0?1:Random.Range(0.5f, 0.9f)) * size.y, sidesize);
                        go.transform.localScale = new Vector3((int)(s.x* roundy) / roundy, (int)(s.y * roundy +1) / roundy, (int)(s.z * roundy) / roundy);
            
            Vector3 maxDisplacement = size - go.transform.localScale;
            Vector3 p = new Vector3(Random.Range(-maxDisplacement.x / 2, maxDisplacement.x / 2), go.transform.localScale.y/2, Random.Range(-maxDisplacement.z / 2, maxDisplacement.z / 2));
            go.transform.localPosition = new Vector3((int)(p.x * roundy) / roundy, p.y, (int)(p.z * roundy) / roundy);
            go.name = this.transform.position.ToString() + "." + i;

            go.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", textureList[textureIndex]);
            go.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(2*go.transform.localScale.x * roundy, 2*go.transform.localScale.y * roundy);

            blocs.Add(go);
        }

        foreach (GameObject go in blocs)
        {
            //Face +z
            for(float k = 0; k < go.transform.localScale.x; k += 0.5f/ roundy)
                for (float l = 0.25f/roundy; l < go.transform.localScale.y; l += 0.5f/roundy)
                {
                    Vector3 p = new Vector3( go.transform.localPosition.x - go.transform.localScale.x / 2 + k + 0.25f / roundy,  l, go.transform.localPosition.z + go.transform.localScale.z / 2 + epsilon);

                    if(IsEmptySpace(blocs, p) && Random.Range(0f, 1f) > 0.8f)
                    {
                        PlaceWindow(p, textureWindowSizeList[textureIndex], new Vector3(0, 180, 0), true);
                    }
                }

            //Face -z
            for (float k = 0; k < go.transform.localScale.x; k += 0.5f / roundy)
                for (float l = 0.25f / roundy; l < go.transform.localScale.y; l += 0.5f / roundy)
                {
                    Vector3 p = new Vector3(go.transform.localPosition.x - go.transform.localScale.x / 2 + k + 0.25f / roundy, l, go.transform.localPosition.z - go.transform.localScale.z / 2 - epsilon);

                    if (IsEmptySpace(blocs, p) && Random.Range(0f, 1f) > 0.8f)
                    {
                        PlaceWindow(p, textureWindowSizeList[textureIndex], new Vector3(0, 0, 0), true);
                    }
                }

            //Face +x
            for (float k = 0; k < go.transform.localScale.z; k += 0.5f / roundy)
                for (float l = 0.25f / roundy; l < go.transform.localScale.y; l += 0.5f / roundy)
                {
                    Vector3 p = new Vector3(go.transform.localPosition.x + go.transform.localScale.x / 2 + epsilon, l, go.transform.localPosition.z - go.transform.localScale.z / 2 + k + 0.25f / roundy);

                    if (IsEmptySpace(blocs, p) && Random.Range(0f, 1f) > 0.8f)
                    {
                        PlaceWindow(p, textureWindowSizeList[textureIndex], new Vector3(0, -90, 0), true);
                    }
                }

            //Face -x
            for (float k = 0; k < go.transform.localScale.z; k += 0.5f / roundy)
                for (float l = 0.25f / roundy; l < go.transform.localScale.y; l += 0.5f / roundy)
                {
                    Vector3 p = new Vector3(go.transform.localPosition.x - go.transform.localScale.x / 2 - epsilon, l, go.transform.localPosition.z - go.transform.localScale.z / 2 + k + 0.25f / roundy);

                    if (IsEmptySpace(blocs, p) && Random.Range(0f, 1f) > 0.8f)
                    {
                        PlaceWindow(p, textureWindowSizeList[textureIndex], new Vector3(0, 90, 0), true);
                    }
                }
        }
    }

    private bool IsEmptySpace(List<GameObject> blocs, Vector3 p)
    {
        foreach (GameObject g2 in blocs)
        {
            if (p.x < g2.transform.localPosition.x - g2.transform.localScale.x / 2) continue;
            else if (p.x > g2.transform.localPosition.x + g2.transform.localScale.x / 2) continue;
            else if (p.y < g2.transform.localPosition.y - g2.transform.localScale.y / 2) continue;
            else if (p.y > g2.transform.localPosition.y + g2.transform.localScale.y / 2) continue;
            else if (p.z < g2.transform.localPosition.z - g2.transform.localScale.z / 2) continue;
            else if (p.z > g2.transform.localPosition.z + g2.transform.localScale.z / 2) continue;

            return false;
        }
        return true;
    }
    private void PlaceWindow(Vector3 p, Vector3 s, Vector3 r, bool light = false)
    {
        GameObject w = Instantiate(windowTemplate);
        w.transform.parent = transform;
        w.SetActive(true);
        w.transform.localScale = s;
        w.transform.localPosition = p;
        w.transform.localEulerAngles = r;
        /*Material m = w.GetComponent<MeshRenderer>().material;
        if (light)
            m.SetColor("_EmissionColor", windowColorGradient.Evaluate(Random.Range(0f, 1f)));
        else
            m.SetColor("_EmissionColor", m.color * Mathf.LinearToGammaSpace(0));*/
        
    }
}
