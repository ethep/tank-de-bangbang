﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//this script is for the  shell
public class Shell : MonoBehaviour
{
    public int shellDamage = 10;
    public ParticleSystem m_ExplosionParticles;
    public AudioSource m_ExplosionAudio;
    private TankController parent;


    public void Initialize(TankController parentTank)
    {
        this.parent = parentTank;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col == parent.Collider)
        {
            return;
        }

        // Play the particle system.
        m_ExplosionParticles.Play();
        // Play the explosion sound effect.
        m_ExplosionAudio.Play();
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        GetComponent<Renderer>().enabled = false;
        Destroy(gameObject, 2);
    }
}
