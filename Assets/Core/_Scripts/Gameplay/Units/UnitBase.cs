using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UnitBase : MonoBehaviour
{

    //variables visible in the inspector
    public float healthFloat;
    public float damage;
    public float minAttackDistance;
    public float castleStoppingDistance;
    public int addGold;
    public string attackTag;
    public string attackBaseTag;
    public ParticleSystem dieParticles;

    //variables not visible in the inspector

    public bool selected;
    public Transform currentTarget;
    [HideInInspector]
    public Vector3 castleAttackPosition;

    [Space(10)]
    public bool transport;
    public ParticleSystem spawnEffect;
    public GameObject transportedUnit;
    public int transportedUnitAmount;
    public float newtransportedUnitsWaitTime;

    private UnityEngine.AI.NavMeshAgent agent;
    private GameObject[] enemies;
    private GameObject healthGO;
    private GameObject healthbar;
    private GameObject selectedObject;
    private bool goingToClickedPos;
    private float startLives;
    private float defaultStoppingDistance;
    private GameObject castle;
    private bool transportSpawns;
    private Animator[] animators;
    private Vector3 targetPosition;

    private ParticleSystem dustEffect;

    void Start()
    {
        //character is not selected
        selected = false;
        //selected character is not moving to clicked position
        goingToClickedPos = false;
        //find navmesh agent component
        agent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        animators = gameObject.GetComponentsInChildren<Animator>();

        //find objects attached to this character
        healthGO = transform.Find("Health").gameObject;
        healthbar = healthGO.transform.Find("Healthbar").gameObject;
        healthGO.SetActive(false);
        selectedObject = transform.Find("selected object").gameObject;
        selectedObject.SetActive(false);

        //set healtbar value
        healthbar.GetComponent<Slider>().maxValue = healthFloat;
        startLives = healthFloat;
        //get default stopping distance
        defaultStoppingDistance = agent.stoppingDistance;
        //find the castle closest to this character
        findClosestBase();

        //if there's a dust effect (cavalry characters), find and assign it
        if (transform.Find("dust"))
            dustEffect = transform.Find("dust").gameObject.GetComponent<ParticleSystem>();

        //if this is a transport, start the spawning loop
        if (transport)
            StartCoroutine(spawntransportedUnits());

    }

    void Update()
    {
        //find closest castle
        if (castle == null && GameObject.FindGameObjectsWithTag(attackBaseTag) != null)
        {
            findClosestBase();
        }

        if (healthFloat != startLives)
        {
            //only use the healthbar when the character lost some lives
            healthGO.SetActive(true);
            healthGO.transform.LookAt(2 * transform.position - Camera.main.transform.position);
            healthbar.GetComponent<Slider>().value = healthFloat;
        }

      
            if (selected)
            {
                selectedObject.SetActive(true);
            }
            else
            {
                selectedObject.SetActive(false);
            }
    

        //find closest enemy
        findCurrentTarget();

        //first check if character is not selected and moving to a clicked position
        if (!goingToClickedPos)
        {
            //If there's a currentTarget and its within the attack range, move agent to currenttarget
            if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) < minAttackDistance)
            {
                if (!transportSpawns)
                {
                    agent.Resume();
                }
                agent.SetDestination(currentTarget.position);



                if (GetComponent<projectileActor>() != null)
                {
                    agent.stoppingDistance = 10;
                }




                //check if character has reached its target and than rotate towards target and attack it
                if (Vector3.Distance(currentTarget.position, transform.position) <= agent.stoppingDistance)
                {
                    var currentTargetPosition = currentTarget.position;
                    currentTargetPosition.y = transform.position.y;
                    transform.LookAt(currentTargetPosition);
                    agent.Stop();

                    foreach (Animator animator in animators)
                    {
                        animator.SetBool("Attacking", true);
                    }

                    currentTarget.gameObject.GetComponent<UnitBase>().healthFloat -= Time.deltaTime * damage;
                }
                //if its still traveling to the target, play running animation
                if (Vector3.Distance(currentTarget.position, transform.position) > agent.stoppingDistance && !transportSpawns)
                {
                    foreach (Animator animator in animators)
                    {
                        animator.SetBool("Attacking", false);
                    }
                }
            }
            //if currentTarget is out of range...
            else
            {
                //if character is not moving to clicked position attack the castle
                if (!goingToClickedPos)
                {
                    if (!transportSpawns)
                    {
                        agent.Resume();
                    }
                    agent.SetDestination(castleAttackPosition);
                }
                //if character is close enough to castle, attack castle

                if (castle != null && Vector3.Distance(transform.position, castleAttackPosition) <= castleStoppingDistance + castle.GetComponent<Base>().size)
                {

                    transform.LookAt(castle.transform);

                    agent.Stop();
                    foreach (Animator animator in animators)
                    {
                        animator.SetBool("Attacking", true);
                    }
                    if (castle != null)
                    {
                        castle.GetComponent<Base>().lives -= Time.deltaTime * damage;
                    }
                }
                //if character is traveling to castle play running animation
                else if (!transportSpawns)
                {
                    agent.Resume();
                    foreach (Animator animator in animators)
                    {
                        animator.SetBool("Attacking", false);
                    }
                }
            }
        }
        //if character is going to clicked position...
        else
        {
            //if character is close enough to clicked position, let it attack enemies again
            if (Vector3.Distance(transform.position, targetPosition) < agent.stoppingDistance + 1)
            {
                goingToClickedPos = false;
                agent.stoppingDistance = defaultStoppingDistance;
                if (!transportSpawns)
                {
                    agent.Resume();
                }
            }
        }

        //if character ran out of lives add blood particles, add gold and destroy character
        if (healthFloat < 1)
        {
            Vector3 position = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
            ParticleSystem particles = Instantiate(dieParticles, position, transform.rotation) as ParticleSystem;
            particles.transform.parent = GameObject.Find("Manager").transform;
            UnitManager.gold += addGold;
            if (gameObject.tag == "Enemy")
            {
                Manager.enemiesKilled++;
            }
            Destroy(gameObject);
        }

        //play dusteffect when running and stop it when the character is not running
        if (dustEffect && animators[0].GetBool("Attacking") == false && !dustEffect.isPlaying)
        {
            dustEffect.Play();
        }
        if (dustEffect && dustEffect.isPlaying && animators[0].GetBool("Attacking") == true)
        {
            dustEffect.Stop();
        }

        //check if character must go to a clicked position
        checkForClickedPosition();
    }

    void findClosestBase()
    {
        //find the castles that should be attacked by this character
        GameObject[] castles = GameObject.FindGameObjectsWithTag(attackBaseTag);

        //distance between character and its nearest castle
        float closestBase = Mathf.Infinity;

        foreach (GameObject potentialBase in castles)
        {
            //check if there are castles left to attack and check per castle if its closest to this character
            if (Vector3.Distance(transform.position, potentialBase.transform.position) < closestBase && potentialBase != null)
            {
                //if this castle is closest to character, set closest distance to distance between character and this castle
                closestBase = Vector3.Distance(transform.position, potentialBase.transform.position);
                //also set current target to closest target (this castle)
                castle = potentialBase;
            }
        }

        //Define a position to attack the castles(to spread characters when they are attacking the castle)
        if (castle != null)
        {
            castleAttackPosition = castle.transform.position;
        }
    }

    void findCurrentTarget()
    {
        //find all potential targets (enemies of this character)
        enemies = GameObject.FindGameObjectsWithTag(attackTag);

        //distance between character and its nearest enemy
        float closestDistance = Mathf.Infinity;

        foreach (GameObject potentialTarget in enemies)
        {
            //check if there are enemies left to attack and check per enemy if its closest to this character
            if (Vector3.Distance(transform.position, potentialTarget.transform.position) < closestDistance && potentialTarget != null)
            {
                //if this enemy is closest to character, set closest distance to distance between character and enemy
                closestDistance = Vector3.Distance(transform.position, potentialTarget.transform.position);
                //also set current target to closest target (this enemy)
                if (!currentTarget || (currentTarget && Vector3.Distance(transform.position, currentTarget.position) > 2))
                {
                    currentTarget = potentialTarget.transform;
                }
            }
        }
    }

    void checkForClickedPosition()
    {
        RaycastHit hit;

        //if character is selected and right mouse button gets clicked...
        if (selected && ((Input.GetMouseButtonDown(1) && !GameObject.Find("Mobile")) || (Input.GetMouseButtonDown(0) && GameObject.Find("Mobile") && Mobile.selectionModeMove)))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
                //if you clicked battle ground, move character to clicked point and play running animation
                if (hit.collider.gameObject.CompareTag("Battle ground"))
                {
                    agent.Resume();
                    agent.SetDestination(hit.point);
                    UnitManager.clickedPos = hit.point;
                    targetPosition = hit.point;
                    goingToClickedPos = true;
                    if (GetComponent<projectileActor>() != null)
                    {
                        agent.stoppingDistance = 2;
                    }
                    foreach (Animator animator in animators)
                    {
                        animator.SetBool("Attacking", false);
                    }
                    //set the yellow target position
                    UnitManager.target.transform.position = hit.point;
                    UnitManager.target.SetActive(true);
                }
        }
    }

    //if this is a transport:
    IEnumerator spawntransportedUnits()
    {

        //stop the spawneffect
        spawnEffect.Stop();

        //create an infinite loop
        while (true)
        {

            //if character is not attacking and there's no target in range...
            if (animators[0].GetBool("Attacking") == false && ((currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) >= minAttackDistance) || !currentTarget))
            {
                //transport is now spawning
                transportSpawns = true;

                //stop the navmesh agent
                agent.Stop();

                //start spawning animation
                animators[0].SetBool("Spawning", true);
                //wait 0.5 seconds
                yield return new WaitForSeconds(0.5f);
                //play the spawn effect
                spawnEffect.Play();

                //spawn the correct amount of transportedUnits with some delay
                for (int i = 0; i < transportedUnitAmount; i++)
                {
                    spawntransportedUnit();
                    yield return new WaitForSeconds(2f / transportedUnitAmount);
                }

                //stop playing the spawneffect
                spawnEffect.Stop();

                //wait for 0.5 seconds again
                yield return new WaitForSeconds(0.5f);

                //transport is not spawning transportedUnits anymore
                transportSpawns = false;

                //stop the spawning animation
                animators[0].SetBool("Spawning", false);
            }

            //short delay before the loop starts again
            yield return new WaitForSeconds(newtransportedUnitsWaitTime);
        }
    }

    void spawntransportedUnit()
    {
        //instantiate a transportedUnit character in front of the transport
        Vector3 position = new Vector3(transform.position.x + Random.Range(1f, 2f), transform.position.y, transform.position.z + Random.Range(-0.5f, 0.5f));
        Instantiate(transportedUnit, position, Quaternion.identity);
    }
}
