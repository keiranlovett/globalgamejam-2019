using UnityEngine;
using System.Collections;

public class ExplodingProjectile : MonoBehaviour
{
    public GameObject impactPrefab;
    public GameObject explosionPrefab;
  //  public float thrust;

    public Rigidbody thisRigidbody;

    public GameObject particleKillGroup;
    private Collider thisCollider;

    public bool LookRotation = true;
    public bool Missile = false;
    public Transform missileTarget;
    public float projectileSpeed;
    public float projectileSpeedMultiplier;

    public bool ignorePrevRotation = false;

    public bool explodeOnTimer = false;
    public float explosionTimer;
    float timer;

    private Vector3 previousPosition;

    public Vector3 _direction;

    // Use this for initialization
    void Start()
    {
        thisRigidbody = GetComponent<Rigidbody>();

        thisCollider = GetComponent<Collider>();
        previousPosition = transform.position;

        if (Missile)
        {
            missileTarget = GameObject.FindWithTag("Target").transform;
        }

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= explosionTimer && explodeOnTimer == true)
        {
            Explode();
        }

    }

    void FixedUpdate()
    {
        if (Missile)
        {
           // projectileSpeed += projectileSpeed * projectileSpeedMultiplier;

            transform.position = Vector3.MoveTowards(transform.position, missileTarget.transform.position, 0);

          //  transform.LookAt(missileTarget);

            thisRigidbody.AddForce(transform.forward * projectileSpeed);




            Vector3 targetDelta = missileTarget.transform.position - transform.position;

            //get the angle between transform.forward and target delta
            float angleDiff = Vector3.Angle(transform.forward, targetDelta);

            // get its cross product, which is the axis of rotation to
            // get from one vector to the other
            Vector3 cross = Vector3.Cross(transform.forward, targetDelta);

            // apply torque along that axis according to the magnitude of the angle.
            thisRigidbody.AddTorque(cross * angleDiff * 0.1f);



        }

        if (LookRotation && timer >= 0.05f)
        {

           // _direction = (missileTarget.transform.position - transform.position).normalized;
         //   var sss = Quaternion.LookRotation(_direction).eulerAngles;










            transform.rotation = Quaternion.LookRotation(thisRigidbody.velocity);
        }

        CheckCollision(previousPosition);

        previousPosition = transform.position;
    }

    void CheckCollision(Vector3 prevPos)
    {
        RaycastHit hit;
        Vector3 direction = transform.position - prevPos;
        Ray ray = new Ray(prevPos, direction);
        float dist = Vector3.Distance(transform.position, prevPos);
        if (Physics.Raycast(ray, out hit, dist))
        {
            transform.position = hit.point;
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            Vector3 pos = hit.point;
            Instantiate(impactPrefab, pos, rot);
            if (!explodeOnTimer && Missile == false)
            {
                Destroy(gameObject);
            }
            else if (Missile == true)
            {
                Explode();
            }

        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "FX")
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, contact.normal);
            if (ignorePrevRotation)
            {
                rot = Quaternion.Euler(0, 0, 0);
            }
            Vector3 pos = contact.point;
            Instantiate(impactPrefab, pos, rot);
            if (!explodeOnTimer && Missile == false)
            {
                Destroy(gameObject);
            }
            else if (Missile == true)
            {

                Explode();

            }
        }
    }

    void Explode()
    {
        Instantiate(impactPrefab, gameObject.transform.position, Quaternion.Euler(0, 0, 0));
        Destroy(gameObject);
    }

}