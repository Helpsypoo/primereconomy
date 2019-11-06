using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GoalType
{
  Wood,
  Fruit
}

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager instance;
    public GameObject treePrefab;
    public GameObject logPrefab;
    public GameObject mangoPrefab;
    public GameObject agentPrefab;


    public int date;

    private bool forestIsReady;
    private ForestManager forest;
    private Vector3 forestPosition = new Vector3(10, 0, 0);

    private AgentController ac;
    private Vector3 homePosition = new Vector3(-10, 0, 0);

    void Awake()
    {
      if (instance == null) {
        instance = this;
      }
      else {
        Destroy(this.gameObject);
      }

      //TODO: Hand prefabs down to forests, really only when there are other kinds
      //of forest.
      //Spawn forest
      GameObject forestGO = new GameObject("Forest");
      forestGO.transform.position = forestPosition;
      forest = forestGO.AddComponent<ForestManager>();
      forest.PrepForest();
      forestIsReady = true;

      //Spawn agent
      GameObject agent = Instantiate(agentPrefab);
      ac = agent.AddComponent<AgentController>();
      ac.forest = forest;

      GameObject home = new GameObject("Home");
      home.transform.position = homePosition;
      ac.home = home;
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
      forest.ReplenishForest();
      forestIsReady = true;
    }
}
