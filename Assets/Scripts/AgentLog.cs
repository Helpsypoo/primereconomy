using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentLog
{
  public List<AgentDay> daysLog = new List<AgentDay>();
  public ProductionPossibilitiesSummary summary =
                                          new ProductionPossibilitiesSummary();

  public void AddDay(AgentDay day)
  {
    daysLog.Add(day);
  }

  public void AddLatestHarvest(Dictionary<GoalType, float> numsHarvested)
  {
    daysLog.Last().numsHarvested = numsHarvested;
    summary.IncorporateNewDay(daysLog.Last());
  }

  public bool IsLogEmpty()
  {
    return daysLog.Count() == 0;
  }
}

public class ProductionPossibilitiesSummary
{
  //Key for both dictionaries is the number of fruits gathered
  //Agents always start the day with fruit, at least for now.
  public Dictionary<int, float> avgWoods = new Dictionary<int, float>();
  public Dictionary<int, int> numTries = new Dictionary<int, int>();

  public int highestGoalTried;
  public int lowestGoalTried = int.MaxValue;

  //Kind of silly, but because the sim results are small integers and forests
  //are static over a sim, getting an additional fruit will not always have
  //a cost in wood, meaning some fruit goals are strictly better than others.
  //For improving decision making and quantifying marginal costs later,
  //discard strictly dominated allocations.
  public Dictionary<int, float> allocationCandidates;

  public void IncorporateNewDay(AgentDay day)
  {
    int key = day.goalFruitHarvest;
    if (avgWoods.ContainsKey(key))
    {
      //recalculate total
      //would be int but for rounding, could maybe keep track of totals as int
      float totalWoodSoFar = avgWoods[key] * numTries[key];
      totalWoodSoFar += day.numsHarvested[GoalType.Wood];

      numTries[key] += 1;
      avgWoods[key] = totalWoodSoFar / numTries[key];
    }
    else
    {
      //Add entry into both dictionaries
      numTries.Add(key, 1);
      avgWoods.Add(key, day.numsHarvested[GoalType.Wood]);
    }

    //Update highest/lowest
    highestGoalTried = (int)Mathf.Max(day.numsHarvested[GoalType.Fruit], highestGoalTried);
    lowestGoalTried = (int)Mathf.Min(day.numsHarvested[GoalType.Fruit], lowestGoalTried);

    determineAllocationCandidates();
  }

  private void determineAllocationCandidates()
  {
    //Could probably be more efficient than just reconstructing.
    allocationCandidates = new Dictionary<int, float>();

    foreach (KeyValuePair<int, float> entry in avgWoods)
    {
      bool isDominated = false;
      foreach (KeyValuePair<int, float> other in avgWoods)
      {
        //TODO: If decision making is ever not based on fruit goals, this
        //condition should change.
        if (entry.Value <= other.Value &&
            entry.Key < other.Key &&
            entry.Key != 0 //The min is always significant, since none lie beyond
        )
        {
          isDominated = true;
        }
      }
      if (!isDominated)
      {
        allocationCandidates.Add(entry.Key, entry.Value);
      }

    }
  }
}

public class AgentDay
{
    //Each day will have the time division ratio and number collected of each good
    public int date;
    public int goalFruitHarvest;
    public Dictionary<GoalType, float> numsHarvested;
    //public float utility;

    public AgentDay(int date, int goalFruitHarvest)
    {
      this.date = date;
      this.goalFruitHarvest = goalFruitHarvest;
    }
}
