using System;
using System.Collections;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;

public class TankController : VehicleController
{
    public AudioClip FireSound;
    public AudioClip IdleSound;
    public AudioClip TankDead;

    public Shell ShellPrefab;
    public ParticleSystem FireSmoke;
    public ParticleSystem DeadSmoke;
    public ParticleSystem Explosion;

    public Transform GunEnd;
    public Transform Turret;
    public Transform Barrel;

    public float ShellSpeed = LevelDesign.Player.ShellSpeedMin;
    public float FireRate = LevelDesign.Player.FireRateMin;

    protected Rigidbody tankRigid;
    [SerializeField]
    protected string currentAnim;
    protected float lastFireTime;
    protected Vector3 initialBarrelPos;
    protected Coroutine barrelCoroutine;

    protected new void Reset()
    {
        base.Reset();
        FireSound = AssetDatabase.FindAssets("t:audioclip tankShot").ToList()
            .ConvertAll<AudioClip>(x => AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(x), typeof(AudioClip)) as AudioClip).First();
        IdleSound = AssetDatabase.FindAssets("t:audioclip tank_idle").ToList()
            .ConvertAll<AudioClip>(x => AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(x), typeof(AudioClip)) as AudioClip).First();
        TankDead = AssetDatabase.FindAssets("t:audioclip tankDead").ToList()
            .ConvertAll<AudioClip>(x => AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(x), typeof(AudioClip)) as AudioClip).First();

        ShellPrefab = AssetDatabase.FindAssets("t:prefab SCT_Shell").ToList()
            .ConvertAll<Shell>(x => AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(x), typeof(Shell)) as Shell).First();

        FireSmoke = GetComponentsInChildren<ParticleSystem>().First(x => x.name == "smokeBarrel");
        DeadSmoke = GetComponentsInChildren<ParticleSystem>().First(x => x.name == "SmokeEffect");
        Explosion = GetComponentsInChildren<ParticleSystem>().First(x => x.name == "TankExplosion");

        GunEnd = GetComponentsInChildren<Transform>().First(x => x.name == "GunEnd");
        Turret = GetComponentsInChildren<Transform>().First(x => x.name == "BoneTurretTurn");
        Barrel = GetComponentsInChildren<Transform>().First(x => x.name == "BoneBarrel");
    }

    private void Awake()
    {
        initialBarrelPos = Barrel.transform.localPosition;
        SetMoveAnimation();
    }

    void SetMoveAnimation()
    {
        tankRigid = GetComponent<Rigidbody>();
        tankRigid.ObserveEveryValueChanged(x => x.velocity)
            .Where(x => !IsDead)
            .Subscribe(x =>
            {
                if (currentAnim == "Hit")
                {
                    return;
                }

                var velocity = Vector3.Dot(transform.forward, x);
                if (x.magnitude < 0.01f)
                {
                    currentAnim = "Idle";
                    Animator.SetBool("Idle1", true);
                    return;
                }

                if (velocity > 0 && currentAnim != "Forward")
                {
                    currentAnim = "Forward";
                    Animator.SetBool("MoveForwStart", true);
                }
                else if (velocity < 0 && currentAnim != "Back")
                {
                    currentAnim = "Back";
                    Animator.SetBool("MoveBackStart", true);
                }
            })
            .AddTo(tankRigid);
    }

    public void Fire()
    {
        if (IsDead)
        {
            return;
        }
        if (lastFireTime + FireRate > Time.time)
        {
            return;
        }

        var shellInstance = Instantiate(ShellPrefab.gameObject, GunEnd.position, Turret.rotation * Quaternion.Euler(180f, 0f, 0f)) as GameObject;
        shellInstance.GetComponent<Shell>().Initialize(this, ShellSpeed * -Turret.transform.up);

        FireSmoke.Play();
        SoundSource.PlayOneShot(FireSound);

        lastFireTime = Time.time;

        if (barrelCoroutine != null)
        {
            StopCoroutine(barrelCoroutine);
            barrelCoroutine = null;
        }

        barrelCoroutine = StartCoroutine(BarrelPiston());
        IEnumerator BarrelPiston()
        {
            var remain = (lastFireTime + FireRate - Time.time) / 2; // 次の発射までの半分の時間で
            while ((remain -= Time.deltaTime) > 0)
            {
                Barrel.transform.localPosition = new Vector3(
                    Barrel.transform.localPosition.x,
                    initialBarrelPos.y + remain / 2,
                    Barrel.transform.localPosition.z);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    protected override void Damage(int damage)
    {
        if (IsDead)
        {
            return;
        }

        HitPoint -= damage;
        if (HitPoint <= 0)
        {
            OnDead.OnNext(Unit.Default);

            Animator.SetBool("Dead" + (int)UnityEngine.Random.Range(1, 5), true);
            currentAnim = "Dead";

            SoundSource.loop = false;
            SoundSource.pitch = 1;
            SoundSource.clip = TankDead;
            SoundSource.Play();
            DeadSmoke.Play();
            Explosion.Play();

            Collider.enabled = false;
            tankRigid.isKinematic = true;

            StartCoroutine(DeadWipe());
            IEnumerator DeadWipe()
            {
                yield return new WaitForSeconds(2f);
                Destroy(this.gameObject);
            }
        }
        else
        {
            Animator.SetBool("HitForw", true);
            currentAnim = "Hit";
            if (!DeadSmoke.isPlaying)
            {
                DeadSmoke.Play();
            }
            StartCoroutine(HitEnd());
            IEnumerator HitEnd()
            {
                yield return new WaitForSeconds(1);
                if (!IsDead)
                {
                    // 一旦Idleに戻す
                    Animator.SetBool("Idle", true);
                    currentAnim = "Idle";
                }
            }
        }
    }
}
