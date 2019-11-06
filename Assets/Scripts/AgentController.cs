using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public GameObject home;
    //private EconomyManager econManager;
    //TODO make EconomyManager spawn both forest and agent and keep track of
    //relationship
    private ForestManager forest;

    private GameObject target = null;
    //private int mode; //0 for wood, 1 for fruit
    private GoalType mode = GoalType.Wood;
    private GameObject heldObject = null;

    public float walkSpeed = 15.0f;
    public float turnSpeed = 15.0f;
    public float harvestDistance = 3.0f;
    public float deliverDistance = 0.1f;
    public int numTreesHarvested = 0;
    public int numMangoesHarvested = 0;

    //half hour increments if the work day is eight hours
    private const float timeAllocationRatioIncrement = 0.0625f;
    private float defaultAlloc = 5 * timeAllocationRatioIncrement;
    public float woodCollectionTimeRatio;


    public float collectionTime = 20.0f; //seconds
    //public float accelerationFactor = 1.0f; //For speeding up sims later on.
    private float startTime;
    public bool dayOver = true;

    //Memory of day outcomes
    public List<AgentDay> activityLog = new List<AgentDay>();

    void Awake()
    {
      //I'm certain there's a preferred way to do this,
      //though it doesn't seem preferable to have to assign it in the UI.
      //GameObject econManagerObject = GameObject.Find("EconomyManager");
      //econManager = GameObject.Find("EconomyManager").GetComponent<EconomyManager>();
      forest = GameObject.Find("Forest").GetComponent<ForestManager>();
    }

    // Update is called once per frame
    void Update()
    {
      if (Time.time - startTime > collectionTime)
      {
        dayOver = true;
      }
    }

    float Utility(int fruit, int wood)
    {
      return Mathf.Log(fruit + 1, 2) + Mathf.Log(wood + 1, 2);
    }

    void DetermineTimeAllocation()
    {
      if (activityLog.Count() == 0)
      {
        woodCollectionTimeRatio = defaultAlloc;
        return;
      }

      //TODO: Maybe refactor this
      float bestUtility = 0f;
      //float bestAlloc = -1.0f; //just something out of normal range to be replace
      AgentDay bestDay = null;
      float lowestAlloc = 1.0f; //To be replaced
      float highestAlloc = 0.0f; //To be replaced

      foreach (AgentDay day in activityLog)
      {
        //lowestAlloc?
        if (day.timeAllocationRatio < lowestAlloc)
        {
          lowestAlloc = day.timeAllocationRatio;
        }
        //highestAlloc?
        if (day.timeAllocationRatio > highestAlloc)
        {
          highestAlloc = day.timeAllocationRatio;
        }
        //bestDay?
        if (day.utility > bestUtility)
        {
          bestUtility = day.utility;
          bestDay = day;
        }
        //If equal to best day, take extreme allocs for exploration.
        else if (day.utility == bestUtility &&
                 (day.timeAllocationRatio == lowestAlloc ||
                  day.timeAllocationRatio == highestAlloc)
                )
        {
          bestDay = day;
        }
      }
      //Check whether bestDay is at an allocation extreme
      bool exploreUp = true;
      bool exploreDown = true;

      if (bestDay.timeAllocationRatio != highestAlloc) {exploreUp = false;}
      if (bestDay.timeAllocationRatio != lowestAlloc) {exploreDown = false;}

      float bestAlloc = bestDay.timeAllocationRatio;
      //Decide where to explore
      if (exploreUp && exploreDown)
      {
        woodCollectionTimeRatio = bestAlloc +
          (Random.Range(0, 2) * 2 - 1) * timeAllocationRatioIncrement;
      }
      else if (exploreUp)
      {
        woodCollectionTimeRatio = bestAlloc + timeAllocationRatioIncrement;
      }
      else if (exploreDown)
      {
        woodCollectionTimeRatio = bestAlloc - timeAllocationRatioIncrement;
      }
      else
      {
        woodCollectionTimeRatio = bestAlloc;
      }
    }

    public void StartWorkDay(int date)
    {
      DetermineTimeAllocation();
      activityLog.Add(new AgentDay(date, woodCollectionTimeRatio));

      startTime = Time.time;
      dayOver = false;
      StartCoroutine("GoHarvest");
    }

    IEnumerator GoHarvest()
    {
      //Check time allocation
      if (Time.time - startTime < woodCollectionTimeRatio * collectionTime)
      {
        mode = GoalType.Wood; //Collect wood
      }
      else
      {
        mode = GoalType.Fruit; //Collect fruit
      }

      //Pick target
      if (target == null)
      {
        target = FindClosestTarget(mode);
      }
      else {
          Debug.LogWarning("GoHarvest was called when agent already had target.");
      }

      while (GoToTargetIfNotThere(harvestDistance))
      {
        yield return null;
      }
      //Harvest
      heldObject = target.GetComponent<HarvestableController>().HandleHarvest();
      heldObject.transform.parent = transform;
      //TODO: Animate movement of heldObject

      StartCoroutine("GoDeliver");
    }

    IEnumerator GoDeliver()
    {
      target = home;
      while (GoToTargetIfNotThere(deliverDistance))
      {
        yield return null;
      }
      if (heldObject == null) {
        Debug.LogWarning("Agent is trying to deliver null in GoDeliver()");
      }
      //Destroy(heldObject);
      heldObject.transform.parent = null;
      heldObject = null;
      target = null;
      //TODO: Animate movement of heldObject to container, or something

      if (dayOver)
      {
        StartCoroutine("GoHome");
      }
      else
      {
        StartCoroutine("GoHarvest");
      }
    }

    IEnumerator GoHome()
    {
      //Keeping this separate from GoDeliver since I'm planning to have separate
      //home and storage area objects.
      target = home;
      while (GoToTargetIfNotThere())
      {
        yield return null;
      }

      //Log day's outcome
      AgentDay day = activityLog.Last();
      day.numTreesHarvested = numTreesHarvested;
      day.numMangoesHarvested = numMangoesHarvested;
      day.utility = Utility(numMangoesHarvested, numTreesHarvested);

      //Testing
      //Agent behavior assumes deterministic harvesting. Check that the new day
      //is not evidence of variation. Warn if necessary.
      //Will mostly function as a reminder to update agent behavior if harvesting
      //is changed to something nondeterministic for visual/freshness reasons.
      foreach (AgentDay otherDay in activityLog){
        if (day.timeAllocationRatio == otherDay.timeAllocationRatio &&
            day.utility != otherDay.utility)
        {
          string warning = "Day " + day.date.ToString() + " yielded " +
            day.numTreesHarvested.ToString() + " trees and " + day.numMangoesHarvested.ToString() +
            " mangoes with a ratio of " + day.timeAllocationRatio.ToString() +
            ".@ Day " + otherDay.date.ToString() + " yielded " +
            otherDay.numTreesHarvested.ToString() + " trees and " + otherDay.numMangoesHarvested.ToString() +
            " mangoes with a ratio of " + otherDay.timeAllocationRatio.ToString() +
            ".";
          warning = warning.Replace("@", System.Environment.NewLine);

          Debug.LogWarning(warning);
        }
      }

      //Reset state
      target = null;
      mode = 0;
      numTreesHarvested = 0;
      numMangoesHarvested = 0;

      //Tell manager we're done
      EconomyManager.instance.AgentIsDone(gameObject);
    }

    private bool GoToTargetIfNotThere(float goalDistance = 0)
    {
      //This seems like it might break some best practices
      Vector3 heading = target.transform.position - transform.position;
      heading = new Vector3 (heading.x, 0, heading.z); //project to xz plane
      float distance = heading.magnitude;
      if (distance > goalDistance)
      {
          Vector3 groundTarget = new Vector3(
            target.transform.position.x,
            0,
            target.transform.position.z
          );

          //Translate
          gameObject.transform.position = Vector3.MoveTowards(
            gameObject.transform.position,
            groundTarget,
            Time.deltaTime * walkSpeed
          );

          //Rotate
          Vector3 targetDir = groundTarget - transform.position;
          float step = turnSpeed * Time.deltaTime;
          Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
          transform.rotation = Quaternion.LookRotation(newDir);

          return true;
      }
      else
      {
        return false;
      }
    }

    private GameObject FindClosestTarget(GoalType mode)
    {
      //This whole method is messy in part because TreeController inherits from
      //HarvestableController, and I'm unsure how to declare a list that can
      //handle either type, so I'm using GameObject as a common denominator.
      //Ideally it would just consider TreeControllers to be HarvestableControllers
      //letting me treat everything as a HarvestableController in cases such as
      //this, where the specifics don't matter, which was the whole point of
      //setting up inheritance in the first place. ¯\_(ツ)_/¯

      //But hey, it doesn't FindGameObject anywhere!

      List<GameObject> listToSearch = new List<GameObject>();

      switch(mode)
      {
        case GoalType.Wood:
          var tlist = forest.allTrees;
          foreach (TreeController tc in tlist)
          {
            listToSearch.Add(tc.gameObject);
          }
          //TODO: Increment num harvested after harvest
          numTreesHarvested++;
          break;
        case GoalType.Fruit:
          var flist = forest.allFruit;
          foreach (HarvestableController hc in flist)
          {
            listToSearch.Add(hc.gameObject);
          }
          numMangoesHarvested++;
          break;
        default:
          Debug.LogWarning("Unknown GoalType");
          break;
      }

      GameObject closest = null;
      float distance = Mathf.Infinity;
      Vector3 position = transform.position;
      foreach (var go in listToSearch)
      {
          //go = hc.gameObject;
          Vector3 diff = go.transform.position - position;
          float curDistance = diff.sqrMagnitude;
          if (curDistance < distance)
          {
            if (go.GetComponent<HarvestableController>().harvested == false)
            {
              closest = go;
              distance = curDistance;
            }
          }
      }

      return closest;

      //TODO: Handle the case where no more potential targets remain
    }
}
