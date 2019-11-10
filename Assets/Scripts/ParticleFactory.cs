using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleFactory : MonoBehaviour
{
    // Start is called before the first frame update
    public virtual Particle GetParticle()
    {
        return null;
    }
}
