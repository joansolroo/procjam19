using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DamagingObject3D : MonoBehaviour, IDamaging
{
    [SerializeField] int damage = 1;
    [SerializeField] float tickTime=1;
    [SerializeField] LayerMask damageMask;

    void Start()
    {
       // position = transform.localPosition;
      //  randomStartWiggling = Random.value * 10;
    }
    void LateUpdate()
    {
      //  this.transform.localPosition = position + new Vector3(0, Mathf.Sin(Time.time * 6 + randomStartWiggling) * 0.05f, 0);
    }

    float t;
    void OnTriggerEnter(Collider other)
    {
        t = 0;
        //Debug.Log("collision");
        if (damageMask.Contains(other.gameObject.layer))
        {
            IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                TryDamage(damageable);
                //controller.Push(-(this.transform.position - controller.transform.position));
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
       

        if(damageMask.Contains(other.gameObject.layer))
        {
            IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                t += Time.deltaTime;
                if (t > tickTime)
                {
                    t -= tickTime;
                    TryDamage(damageable);
                    //controller.Push(-(this.transform.position - controller.transform.position));
                }
            }
        }
        
    }

    public bool TryDamage(IDamageable damageable)
    {

        if (damageMask.Contains(((Component)damageable).gameObject.layer))
            if (damageable.CanBeDamaged(this))
            {
                damageable.GetDamaged(this);
                return true;
            }
        return false;
    }
}
