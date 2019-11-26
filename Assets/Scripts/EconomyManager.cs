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
    public GameObject homePrefab;
    public GameObject boxPrefab;

    public int date;

    private bool forestIsReady;
    public ForestManager forest;

    private AgentController ac;
    //private Vector3 homePosition;

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
      forestGO.transform.position = new Vector3(0, 0, 0);
      forestGO.transform.parent = transform;
      forest = forestGO.AddComponent<ForestManager>();
      forest.PrepForest();
      forestIsReady = true;

      //Spawn agent
      //Raymarching toolkit doesn't support spawning of raymarching objects
      //so to have a raymarched agent, need to put it in the scene beforehand.
      //If it doesn't exist, fall back to prefab.
      //(Blob asset not public)
      GameObject agent = GameObject.FindGameObjectWithTag("agent");
      if (agent == null)
      {
        agent = Instantiate(agentPrefab);
      }
      agent.transform.parent = transform;
      ac = agent.AddComponent<AgentController>();
      ac.forest = forest;

      GameObject home = GameObject.Find("home");
      if (home == null) {
        home = Instantiate(homePrefab);
        //home.transform.position = new Vector3(10, 0, 10);
        home.transform.parent = transform;
      }
      //TODO: Changing this for the demo instead of making a whole animation to go
      //in the house.
      //Old, good for now code: ac.home = home;
      //Bad, only for the demo code:
      GameObject tempHome = new GameObject("temp home");
      tempHome.transform.position = ac.transform.position;
      ac.home = tempHome;

      GameObject box = GameObject.Find("box");
      if (box == null) {
        box = Instantiate(boxPrefab);
        //box.transform.position = new Vector3(10, 0, 10);
        //box.transform.parent = home.transform;
        box.transform.SetParent(home.transform, false);
      }
      ac.box = box;
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

      //Print summary to Unity console
      //TODO: Make activityLog and daysLog private and delete code below once this kind of
      //logging is no longer useful.
      AgentDay lastLog = ac.activityLog.daysLog.Last();
      int lastDate = lastLog.date; //Should be same as date
      int lastGoal = lastLog.goalFruitHarvest;
      float trees = lastLog.numsHarvested[GoalType.Wood];
      float mangoes = lastLog.numsHarvested[GoalType.Fruit];
      Debug.Log(
        "On day " + lastDate.ToString() + ", agent harvested " +
        trees.ToString() + " trees and " + mangoes.ToString() +
        " mangoes. By going for " + lastGoal.ToString() +
        " mangoes."
      );



      //ReplenishForest
      forest.ReplenishForest();
      forestIsReady = true;
    }
}
