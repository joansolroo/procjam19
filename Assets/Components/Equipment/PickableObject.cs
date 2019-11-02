using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Equipment;

public class PickableObject : Container<IPickeable>
{
    [SerializeField] public bool instant = false;
    Vector3 position;
    float randomStartWiggling;
    // Use this for initialization
    void Start()
    {
        Init();
    }
    public void Init()
    {
        position = transform.localPosition;
        randomStartWiggling = Random.value * 10;
    }
    void LateUpdate()
    {
        this.transform.localPosition = position + new Vector3(0, Mathf.Sin(Time.time * 6 + randomStartWiggling) * 0.01f, 0);
        this.transform.Rotate(0, 60 * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleTrigger(other, false);
    }

    private void OnTriggerExit(Collider other)
    {
        HandleTrigger(other, true);
    }

    private void HandleTrigger(Collider other, bool end)
    {
        {
            if (other.gameObject.tag == "Player")
            {
                IPicker picker = other.gameObject.GetComponent<IPicker>();
                if (picker != null)
                {
                    bool success = picker.OfferToPick(this, !end);
                    if (success)
                        GotPicked();
                }
            }
        }
    }
    public void GotPicked()
    {

        Destroy();

    }
    void Destroy()
    {
        Destroy(this.gameObject);
    }
}
