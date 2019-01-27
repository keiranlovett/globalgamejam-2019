using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UnitTypeTransport : MonoBehaviour
{
    public GameObject setDeployedUnit;

    protected Queue<Unit> buildQueue;

    private UnityEngine.AI.NavMeshAgent agent;
    private float defaultStoppingDistance;

    private Animator[] animators;
    private ParticleSystem dustEffect;


    public Transform homePosition;

    private void Awake()
    {
        buildQueue = new Queue<Unit>();
    }

    void Start()
    {
        //find navmesh agent component
        agent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        animators = gameObject.GetComponentsInChildren<Animator>();

        //if there's a dust effect (cavalry characters), find and assign it
        if (transform.Find("dust"))
            dustEffect = transform.Find("dust").gameObject.GetComponent<ParticleSystem>();
    }

    void Update()
    {

        ProcessBuildQueue();

        //play dusteffect when running and stop it when the character is not running
        if (dustEffect && animators[0].GetBool("Attacking") == false && !dustEffect.isPlaying)
        {
            dustEffect.Play();
        }
        if (dustEffect && dustEffect.isPlaying && animators[0].GetBool("Attacking") == true)
        {
            dustEffect.Stop();
        }

        if (Vector3.Distance(agent.destination, agent.transform.position) < 10)
        {
            agent.baseOffset = Mathf.Clamp(10, 1, Vector3.Distance(agent.transform.position, agent.destination) / 2);

        }
    }

    public void CreateUnit(Unit unitName)
    {

        buildQueue.Enqueue(unitName);

    }


    protected void AddUnit(Unit unitName, Quaternion rotation)
    {
        moveToUnitPlacement(unitName.location);
        StartCoroutine(SpawnWhenNear(unitName));
    }

    //We wait until relatively close to the spawn point.
    IEnumerator SpawnWhenNear(Unit unitName)
    {
        yield return new WaitUntil(() => Vector3.Distance(agent.transform.position, unitName.location) <= 1);
        GameObject newTroop = Instantiate(unitName.go, unitName.location, unitName.go.transform.rotation) as GameObject;

        agent.SetDestination(homePosition.position);


    }


    //Move the agent to the intended position, and animate along the way.
    protected void moveToUnitPlacement(Vector3 hit)
    {
        agent.SetDestination(hit);
        agent.Resume();

        //set the yellow target position
        UnitManager.target.transform.position = hit;
        UnitManager.target.SetActive(true);

        foreach (Animator animator in animators)
        {
            animator.SetBool("Attacking", false);
        }
    }


    protected void ProcessBuildQueue()
    {
        if (buildQueue.Count > 0)
        {

            if (pathComplete())
            {
            
                AddUnit(buildQueue.Dequeue(), transform.rotation);
            }
        }
        else
        {


           


        }
    }


    protected bool pathComplete()
    {
        if (Vector3.Distance(agent.destination, agent.transform.position) <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }

        return false;
    }

}
