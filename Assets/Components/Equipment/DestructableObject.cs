using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Weapon;

public class DestructableObject : MonoBehaviour,IDamageable {
    
    [SerializeField] int hp;
    [SerializeField] PickableObject drop;
    [SerializeField] LayerMask damagingMask;

    void Start()
    {
        
    }
    void LateUpdate()
    {
        triggered = false;
    }
    bool triggered = false;
    private void OnCollisionEnter(Collision collision)
    {
        
        //Debug.Log("hitted by " + collision.collider.name+"::"+ collision.gameObject.layer);
        if (damagingMask.Contains(collision.gameObject.layer))
        {
            IDamaging damaging = collision.gameObject.GetComponent<IDamaging>();
            if (damaging != null)
            {
                if (!triggered && CanBeDamaged(damaging))
                {
                    triggered = true;
                    GetDamaged(damaging);
                }
            }
        }
    }  

    void Destroy()
    {
        if (drop != null)
        {
            PickableObject newDrop = GameObject.Instantiate<PickableObject>(drop);
            newDrop.transform.position = this.transform.position;
            newDrop.gameObject.SetActive(true);
            newDrop.transform.parent = drop.transform.parent;
            newDrop.transform.localScale = drop.transform.localScale;
        }
        EffectManager.main.Explode(this.transform.position);
        Destroy(this.gameObject);
    }

    public Layer GetLayer()
    {
        throw new System.NotImplementedException();
    }

    public bool CanBeDamaged(IDamaging source)
    {
        return (damagingMask.Contains(((Component)source).gameObject.layer));
    }

    public bool GetDamaged(IDamaging source)
    {
        throw new System.NotImplementedException();
    }
    public bool GetDamaged(int amount)
    {
        throw new System.NotImplementedException();
    }
}
