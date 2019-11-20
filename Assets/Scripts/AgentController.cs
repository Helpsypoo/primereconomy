using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public GameObject home;
    public GameObject box;
    public ForestManager forest;

    private GameObject target = null;
    private GoalType mode = GoalType.Wood;
    private GameObject heldObject = null;

    private float walkSpeed = 10.0f;
    private float turnSpeed = 10.0f;
    private float harvestDistance = 1.0f;
    private float deliverDistance = 2.0f;

    private Dictionary<GoalType, int> numsHarvested =
              new Dictionary<GoalType, int>();

    //half hour increments if the work day is eight hours
    private const float timeAllocationRatioIncrement = 0.0625f;
    private float defaultAlloc = 5 * timeAllocationRatioIncrement;
    private float woodCollectionTimeRatio;

    private float collectionTime = 20.0f; //seconds
    //public float accelerationFactor = 1.0f; //For speeding up sims later on.
    private float startTime;
    private bool dayOver = true;

    //Memory of day outcomes
    public List<AgentDay> activityLog = new List<AgentDay>();

    void Awake()
    {

    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      if (Time.time - startTime > collectionTime)
      {
        dayOver = true;
      }
    }

    float Utility(Dictionary<GoalType, int> nums)
    {
      int wood = nums[GoalType.Wood];
      int fruit = nums[GoalType.Fruit];
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

      //Reset state
      target = null;
      dayOver = false;
      startTime = Time.time;
      foreach (GoalType type in (GoalType[]) System.Enum.GetValues(typeof(GoalType)))
      {
        numsHarvested[type] = 0;
      }

      StartCoroutine("GoHarvest");
    }

    IEnumerator GoHarvest()
    {
      //Check time allocation
      if (Time.time - startTime < woodCollectionTimeRatio * collectionTime)
      {
        mode = GoalType.Wood;
      }
      else
      {
        mode = GoalType.Fruit;
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
      //heldObject.GetComponent<Rigidbody>().useGravity = false;
      Destroy(heldObject.GetComponent<Rigidbody>());

      IEnumerator coroutine = AdjustGrip(heldObject);
      StartCoroutine(coroutine);

      numsHarvested[mode]++;

      StartCoroutine("GoDeliver");
    }

    IEnumerator GoDeliver()
    {
      target = box;
      while (GoToTargetIfNotThere(deliverDistance))
      {
        yield return null;
      }
      if (heldObject == null) {
        Debug.LogWarning("Agent is trying to deliver null in GoDeliver()");
      }
      //Destroy(heldObject); //If you don't like clutter
      heldObject.transform.parent = EconomyManager.instance.transform;
      IEnumerator coroutine = DeliveryAnimation(heldObject);
      StartCoroutine(coroutine);
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

    IEnumerator DeliveryAnimation(GameObject good)
    {
      //Perhaps a bit hacky, trying to get a demo out

      //Set trigger to open box, stop closing it if you were.
      Animator boxAnimator = box.GetComponent<Animator>();
      boxAnimator.ResetTrigger("close");
      boxAnimator.SetTrigger("open");

      //Put above box
      Vector3 aboveBox = box.transform.position + new Vector3 (0f, 2f, 0f);
      Vector3 heading = aboveBox - good.transform.position;
      float sqrDistance = heading.sqrMagnitude;

      while (sqrDistance > 0) {
        good.transform.position = Vector3.MoveTowards(
          good.transform.position,
          aboveBox,
          Time.deltaTime * walkSpeed
        );
        heading = aboveBox - good.transform.position;
        sqrDistance = heading.sqrMagnitude;

        yield return null;
      }

      //Lower into box
      while (good.transform.position.y > box.transform.position.y) {
        good.transform.position = Vector3.MoveTowards(
          good.transform.position,
          box.transform.position,
          Time.deltaTime * walkSpeed
        );

        yield return null;
      }

      boxAnimator.SetTrigger("close");

      Destroy(good);
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
      day.numsHarvested = numsHarvested;
      day.utility = Utility(numsHarvested);

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
            day.numsHarvested[GoalType.Wood].ToString() + " trees and " + day.numsHarvested[GoalType.Fruit].ToString() +
            " mangoes with a ratio of " + day.timeAllocationRatio.ToString() +
            ".@ Day " + otherDay.date.ToString() + " yielded " +
            otherDay.numsHarvested[GoalType.Wood].ToString() + " trees and " + otherDay.numsHarvested[GoalType.Fruit].ToString() +
            " mangoes with a ratio of " + otherDay.timeAllocationRatio.ToString() +
            ".";
          warning = warning.Replace("@", System.Environment.NewLine);

          Debug.LogWarning(warning);
        }
      }

      //Tell manager we're done
      EconomyManager.instance.AgentIsDone(gameObject);
    }

    IEnumerator AdjustGrip(GameObject good)
    {
      //Very similar to other functions. Could likely make a general coroutine
      //for moving over time.
      Vector3 goalPos = new Vector3 (0, 1, 1); 
      Vector3 heading = goalPos - good.transform.localPosition;
      float sqrDistance = heading.sqrMagnitude;

      while (sqrDistance > 0) {
        good.transform.localPosition = Vector3.MoveTowards(
          good.transform.localPosition,
          goalPos,
          Time.deltaTime * walkSpeed
        );
        heading = goalPos - good.transform.localPosition;
        sqrDistance = heading.sqrMagnitude;

        yield return null;
      }
    }

    bool GoToTargetIfNotThere(float goalDistance = 0)
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

    GameObject FindClosestTarget(GoalType mode)
    {
      List<HarvestableController> listToSearch = forest.allHarvestables[mode];

      HarvestableController closest = null;
      float distance = Mathf.Infinity;
      Vector3 position = transform.position;
      foreach (HarvestableController harvestable in listToSearch)
      {
          Vector3 diff = harvestable.transform.position - position;
          float curDistance = diff.sqrMagnitude;
          if (curDistance < distance)
          {
            if (harvestable.harvested == false)
            {
              closest = harvestable;
              distance = curDistance;
            }
          }
      }

      return closest.gameObject;

      //TODO: Handle the case where no more potential targets remain
    }
}
