using UnityEngine;

public class Shell : MonoBehaviour
{
    public int Damage = 1;
    public ParticleSystem m_ExplosionParticles;
    public AudioSource m_ExplosionAudio;
    public Rigidbody Rigidbody;

    public string ParentTag;

    public void Initialize(TankController parentTank, Vector3 velocity)
    {
        this.ParentTag = parentTank.tag;
        this.Rigidbody.velocity = velocity;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag(ParentTag))
        {
            return;
        }

        // Play the particle system.
        m_ExplosionParticles.Play();
        // Play the explosion sound effect.
        m_ExplosionAudio.Play();
        Rigidbody.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        GetComponent<Renderer>().enabled = false;
        Destroy(gameObject, 2);
    }
}
