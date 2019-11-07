using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentDay
{
    //Each day will have the time division ratio and number collected of each good
    public int date;
    public float timeAllocationRatio;
    public Dictionary<GoalType, int> numsHarvested;
    public float utility;

    public AgentDay(int date, float timeAllocationRatio)
    {
      this.date = date;
      this.timeAllocationRatio = timeAllocationRatio;
    }
}
