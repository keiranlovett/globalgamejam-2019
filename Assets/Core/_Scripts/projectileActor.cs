using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class projectileActor : MonoBehaviour
{

    public Transform spawnLocator;
    public Transform spawnLocatorMuzzleFlare;
    public Transform shellLocator;
    public Animator recoilAnimator;
    public Transform[] shotgunLocator;

    [System.Serializable]
    public class projectile
    {
        public string name;
        public Rigidbody projectilePrefab;
        public GameObject muzzleflare;
        public float forceMin, forceMax;
        public bool rapidFire;
        public float rapidFireCooldown;

        public bool shotgunBehavior;
        public int shotgunPellets;
        public GameObject shellPrefab;
        public bool hasShells;
    }
    public projectile[] projectileList;
    public int projectileTypeSelected = 0;

    public bool canCameraShake = true;
    public CameraShake CameraShakeCaller;
    public float rapidFireDelay;

    public bool fireWithAnimation = false;
    public bool firing;
    float firingTimer;
    private float _lastShotTime = float.MinValue;



    public bool canAddTorque = false;
    public float torqueMin, torqueMax;


    public Transform turretBody;

    public bool rotateUnitOnFire;
    int seq = 0;



    //not visible in the inspector
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

        if (animator.GetBool("Attacking") == true)
        {

            if (fireWithAnimation == true)
            {
                //only shoot when animation is almost done (when the character is shooting)
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 >= 0.95f)
                {
                    Fire();
                }
            }
            else
            {
                if (Time.time > _lastShotTime + (rapidFireDelay))
                {
                    _lastShotTime = Time.time;
                    Fire();
                }
            }
        }
    }

    public void SwitchProjectile(int value)
    {
        projectileTypeSelected += value;
        if (projectileTypeSelected < 0)
        {
            projectileTypeSelected = projectileList.Length;
            projectileTypeSelected--;
        }
        else if (projectileTypeSelected >= projectileList.Length)
        {
            projectileTypeSelected = 0;
        }
    }

    public void Fire()
    {
        recoilAnimator.SetTrigger("recoil_trigger");

        if (canCameraShake)
        {
            CameraShakeCaller.ShakeCamera();
        }
        Instantiate(projectileList[projectileTypeSelected].muzzleflare, spawnLocatorMuzzleFlare.position, spawnLocatorMuzzleFlare.rotation);

        if (projectileList[projectileTypeSelected].hasShells)
        {
            Instantiate(projectileList[projectileTypeSelected].shellPrefab, shellLocator.position, shellLocator.rotation);
        }

        Rigidbody projectileInstance;
        projectileInstance = Instantiate(projectileList[projectileTypeSelected].projectilePrefab, spawnLocator.position, spawnLocator.rotation) as Rigidbody;
        projectileInstance.AddForce(spawnLocator.forward * Random.Range(projectileList[projectileTypeSelected].forceMin, projectileList[projectileTypeSelected].forceMax));

        if (projectileList[projectileTypeSelected].shotgunBehavior)
        {
            for (int i = 0; i < projectileList[projectileTypeSelected].shotgunPellets; i++)
            {
                Rigidbody projectileInstanceShotgun;
                projectileInstanceShotgun = Instantiate(projectileList[projectileTypeSelected].projectilePrefab, shotgunLocator[i].position, shotgunLocator[i].rotation) as Rigidbody;
                projectileInstanceShotgun.AddForce(shotgunLocator[i].forward * Random.Range(projectileList[projectileTypeSelected].forceMin, projectileList[projectileTypeSelected].forceMax));
            }
        }

        if (canAddTorque)
        {
            projectileInstance.AddTorque(spawnLocator.up * Random.Range(torqueMin, torqueMax));
        }
        if (rotateUnitOnFire)
        {
            RandomizeRotation();
        }

    }


    void RandomizeRotation()
    {
        if (seq == 0)
        {
            seq++;
            transform.Rotate(0, 1, 0);
        }
        else if (seq == 1)
        {
            seq++;
            transform.Rotate(1, 1, 0);
        }
        else if (seq == 2)
        {
            seq++;
            transform.Rotate(1, -3, 0);
        }
        else if (seq == 3)
        {
            seq++;
            transform.Rotate(-2, 1, 0);
        }
        else if (seq == 4)
        {
            seq++;
            transform.Rotate(1, 1, 1);
        }
        else if (seq == 5)
        {
            seq = 0;
            transform.Rotate(-1, -1, -1);
        }
    }
}