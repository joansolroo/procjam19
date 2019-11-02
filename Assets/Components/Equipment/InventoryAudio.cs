using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment
{
    public class InventoryAudio<T> : MonoBehaviour where T: InventoryData
    {
        [SerializeField] protected AudioSource audioSource;

        protected virtual void Reset()
        {
            audioSource = GetComponent<AudioSource>();
            if (!audioSource)
            {
                audioSource = this.gameObject.AddComponent<AudioSource>();
            }
        }
    }
}