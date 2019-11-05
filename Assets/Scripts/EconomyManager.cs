using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public GameObject agent;
    private AgentController ac;
    public GameObject forest;
    public int date = 0;

    private bool forestIsReady;

    void Awake()
    {
      ac = agent.GetComponent<AgentController>();

      forest.GetComponent<ForestManager>().PrepForest();
      forestIsReady = true;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      //if (Input.GetKeyDown(KeyCode.Space) && forestIsReady == true)
      if (forestIsReady == true)
        {
          ac.StartWorkDay(date);
          forestIsReady = false;
        }
    }

    public void AgentIsDone(GameObject agent)
    {
      //If there were multiple agents, track doneness here
      //When all are done, increment day and replenish forest

      //Increment date
      date++;

      AgentDay lastLog = ac.activityLog.Last();
      int lastDate = lastLog.date; //Should be same as date
      float lastRatio = lastLog.timeAllocationRatio;
      int trees = lastLog.numTreesHarvested;
      int mangoes = lastLog.numMangoesHarvested;
      Debug.Log(
        "On day " + lastDate.ToString() + ", agent harvested " +
        trees.ToString() + " trees and " + mangoes.ToString() +
        " mangoes. By spending " + lastRatio.ToString() +
        " of its time on wood collection."
      );


      //ReplenishForest
      forest.GetComponent<ForestManager>().ReplenishForest();
      forestIsReady = true;
    }
}
