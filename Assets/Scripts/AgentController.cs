using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    private static System.Random rand = new System.Random();

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

    private Dictionary<GoalType, float> numsHarvested =
              new Dictionary<GoalType, float>();

    private int defaultGoal = 3;
    private int goalFruitHarvest;

    private double explorationChance = 0.2;

    private float collectionTime = 20.0f; //seconds
    //public float accelerationFactor = 1.0f; //For speeding up sims later on.
    private float startTime;
    private bool dayOver = true;

    //Memory of day outcomes
    public AgentLog activityLog = new AgentLog();

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

    //float Utility(Dictionary<GoalType, float> nums)
    public float Utility(float fruit, float wood)
    {
      //float wood = nums[GoalType.Wood];
      //float fruit = nums[GoalType.Fruit];
      float scalerY = 0.25f;
      float scalerX = 10f;
      return scalerY * (1.5f * Mathf.Log(scalerX * fruit + 1, 10) + 1 * Mathf.Log(scalerX * wood + 1, 10));
    }

    void DetermineFruitHarvestGoal()
    {
      //If no experience, just go with default
      if (activityLog.IsLogEmpty())
      {
        goalFruitHarvest = defaultGoal;
        return;
      }

      //Find goal with the best result so far
      float maxUtility = 0f;
      int bestGoal = 0;
      int lowestGoal = int.MaxValue;
      int highestGoal = 0;
      //TODO: Find a way to clean up the messiness caused by the fact Utility()
      //takes a different type of dictionary than how allocationCandidates are
      //stored
      foreach (KeyValuePair<int, float> entry in
                                      activityLog.summary.allocationCandidates)
      {

        Dictionary<GoalType, float> fakeDay = new Dictionary<GoalType, float>();
        fakeDay.Add(GoalType.Fruit, (float)entry.Key);
        fakeDay.Add(GoalType.Wood, (float)entry.Value);
        //float util = Utility(fakeDay);


        float util = Utility(entry.Key, entry.Value);
        if (util > maxUtility)
        {
          maxUtility = util;
          bestGoal = (int)fakeDay[GoalType.Fruit];
        }

        highestGoal = (int)Mathf.Max(fakeDay[GoalType.Fruit], highestGoal);
        lowestGoal = (int)Mathf.Min(fakeDay[GoalType.Fruit], lowestGoal);
      }
      Debug.Log(bestGoal.ToString() +
                ", " + highestGoal.ToString() +
                ", " + lowestGoal.ToString());

      //If the best result so far comes from the highest or lowest goal, keep
      //exploring. Otherwise, use the middle goal.
      if (bestGoal == highestGoal)
      {
        goalFruitHarvest = activityLog.summary.highestGoalTried + 1;
        return;
      }
      else if (bestGoal == lowestGoal)
      {
        goalFruitHarvest = activityLog.summary.lowestGoalTried - 1;
        return;
      }
      else
      {
        /*
        Debug.Log("Best goal is " + bestGoal.ToString() +
                  ". Highest goal is " + highestGoal.ToString() +
                  ". Lowest goal is " + lowestGoal.ToString() +
                  ", sitting tight."
        );
        */

        //Chance of exploring anyway, since nearby strategies could be better
        //with the non-deterministic nature.
        double variationRoll = rand.NextDouble();
        if (variationRoll < explorationChance / 2) //Over two, because half chance
        {
          //Go higher
          int nextHigher = int.MaxValue;
          foreach (KeyValuePair<int, float> entry in activityLog.summary.allocationCandidates)
          {
            int possibleGoal = entry.Key;
            if (possibleGoal >= bestGoal && possibleGoal <= nextHigher)
            {
              nextHigher = possibleGoal;
            }
          }
          bestGoal = nextHigher;
        }
        else if (variationRoll < explorationChance)
        {
          //Go lower
          int nextLower = 0;
          foreach (KeyValuePair<int, float> entry in activityLog.summary.allocationCandidates)
          {
            int possibleGoal = entry.Key;
            if (possibleGoal <= bestGoal && possibleGoal >= nextLower)
            {
              nextLower = possibleGoal;
            }
          }
          bestGoal = nextLower;
        }

        goalFruitHarvest = bestGoal;
        return;
      }
      //TODO: make it so the goal doesn't shoot past the harvestable number of
      //fruit, likely by assigning a negative wood yield in cases where the fruit
      //goal is not met.
      //If it becomes a problem. /shrug
    }

    public void StartWorkDay(int date)
    {
      DetermineFruitHarvestGoal();
      activityLog.AddDay(new AgentDay(date, goalFruitHarvest));

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
      if (numsHarvested[GoalType.Fruit] < goalFruitHarvest)
      {
        mode = GoalType.Fruit;
      }
      else
      {
        mode = GoalType.Wood;
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

      Animator agentAnimator = gameObject.GetComponent<Animator>();
      if (agentAnimator != null)
      {
        agentAnimator.ResetTrigger("drop");
        agentAnimator.SetTrigger("grab");
      }

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

      Animator agentAnimator = gameObject.GetComponent<Animator>();
      if (agentAnimator != null)
      {
        agentAnimator.ResetTrigger("grab");
        agentAnimator.SetTrigger("drop");
      }

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

      //TODO: remove the waving below after demo
      /*
      Animator agentAnimator = gameObject.GetComponent<Animator>();
      if (agentAnimator != null)
      {
        agentAnimator.ResetTrigger("drop");
        agentAnimator.SetTrigger("Wave");
      }
      */

      //Log day's outcome
      activityLog.AddLatestHarvest(numsHarvested);

      //Tell manager we're done
      EconomyManager.instance.AgentIsDone(gameObject);

      //TODO Delete string builder after testing
      var avgsListString = new System.Text.StringBuilder();
      avgsListString.AppendLine("Average utilities by allocation ");

      foreach (KeyValuePair<int, float> entry in activityLog.summary.avgWoods)
      {
        /*
        Dictionary<GoalType, float> fakeDay = new Dictionary<GoalType, float>();
        fakeDay.Add(GoalType.Wood, entry.Value);
        fakeDay.Add(GoalType.Fruit, entry.Key);
        float util = Utility(fakeDay);
        */

        float util = Utility(entry.Key, entry.Value);
        avgsListString.AppendLine(
          entry.Key.ToString() + ", " + util.ToString()
        );
      }
      Debug.Log(avgsListString);
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
