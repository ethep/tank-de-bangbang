using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UniRx;

public class TankController : MonoBehaviour
{
    public Animator Animator;

    public AudioSource SoundSource;
    public AudioClip FireSound;
    public AudioClip IdleSound;

    public Rigidbody ShellRigid;
    public ParticleSystem FireSmoke;
    public Transform GunEnd;
    public Transform Turret;
    public Transform Barrel;

    public float ShellSpeed = 50f;
    public float TankSpeed = 100f;
    public float FireRate = 1.0f;

    private Rigidbody tankRigid;
    private string currentAnim;
    private float lastFireTime;
    private Vector3 initialBarrelPos;
    private Coroutine barrelCoroutine;

    private void Reset()
    {
        Animator = GetComponentInChildren<Animator>();

        SoundSource = GetComponent<AudioSource>();
        FireSound = AssetDatabase.FindAssets("t:audioclip tankShot").ToList()
            .ConvertAll<AudioClip>(x => AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(x), typeof(AudioClip)) as AudioClip).First();
        IdleSound = AssetDatabase.FindAssets("t:audioclip tank_idle").ToList()
            .ConvertAll<AudioClip>(x => AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(x), typeof(AudioClip)) as AudioClip).First();

        ShellRigid = AssetDatabase.FindAssets("t:prefab SCT_Shell").ToList()
            .ConvertAll<Rigidbody>(x => AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(x), typeof(Rigidbody)) as Rigidbody).First();
        FireSmoke = GetComponentsInChildren<ParticleSystem>().First(x => x.name == "smokeBarrel");
        GunEnd = GetComponentsInChildren<Transform>().First(x => x.name == "GunEnd");
        Turret = GetComponentsInChildren<Transform>().First(x => x.name == "BoneTurretTurn");
        Barrel = GetComponentsInChildren<Transform>().First(x => x.name == "BoneBarrel");
    }

    private void Awake()
    {
        tankRigid = GetComponent<Rigidbody>();
        tankRigid.ObserveEveryValueChanged(x => x.velocity)
            .Subscribe(x =>
            {
                if (x.x > 0.1f && currentAnim != "Forward")
                {
                    currentAnim = "Forward";
                    Animator.SetBool("MoveForwStart", true);
                }
                else if (x.x < -0.1f && currentAnim != "Back")
                {
                    currentAnim = "Back";
                    Animator.SetBool("MoveBackStart", true);
                }
                else if (x.magnitude <= 0.1f && currentAnim != "Idle")
                {
                    currentAnim = "Idle";
                    Animator.SetBool("Idle1", true);
                }
            })
            .AddTo(tankRigid);

        initialBarrelPos = Barrel.transform.localPosition;
    }

    private void LateUpdate()
    {
        // Idleのアニメーションで動いてしまうので、強制的に戻す
        Turret.transform.rotation = Quaternion.Euler(
            transform.eulerAngles.x - 90,
            transform.eulerAngles.y,
            transform.eulerAngles.z - 90);
    }

    public void Move(Vector3 vec)
    {
        Vector3 movement = transform.forward * vec.normalized.x * TankSpeed * Time.deltaTime;
        tankRigid.velocity = tankRigid.transform.forward * movement.x;
    }

    public void Fire()
    {
        if (lastFireTime + FireRate > Time.time)
        {
            return;
        }

        Rigidbody shellInstance = Instantiate(ShellRigid, GunEnd.position, Turret.rotation) as Rigidbody;
        shellInstance.velocity = ShellSpeed * -Turret.transform.up;
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
}
