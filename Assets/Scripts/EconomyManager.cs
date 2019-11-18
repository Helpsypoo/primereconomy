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

    public List<Vector3> treeLocs = new List<Vector3> {
      new Vector3 (-1.3f, 0.0f, -5.4f),
      new Vector3 (6.1f, 0.0f, -9.6f),
      new Vector3 (-8.7f, 0.0f, 8.5f),
      new Vector3 (-3.1f, 0.0f, -9.9f),
      new Vector3 (-10.0f, 0.0f, 0.3f),
      new Vector3 (2.4f, 0.0f, 4.6f),
      new Vector3 (-4.7f, 0.0f, -4.4f),
      new Vector3 (1.4f, 0.0f, 5.6f),
      new Vector3 (-7.7f, 0.0f, 3.0f),
      new Vector3 (3.3f, 0.0f, -4.6f)
    };

    public List<Quaternion> treeRots = new List<Quaternion> {
      new Quaternion (0.0f, -0.7f, 0.0f, 0.7f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, -1.0f, 0.0f, 0.0f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, -0.7f, 0.0f, 0.7f),
      new Quaternion (0.0f, -1.0f, 0.0f, 0.0f),
      new Quaternion (0.0f, -0.7f, 0.0f, -0.7f),
      new Quaternion (0.0f, -0.7f, 0.0f, -0.7f),
      new Quaternion (0.0f, -1.0f, 0.0f, 0.0f),
      new Quaternion (0.0f, -0.7f, 0.0f, 0.7f)
    };

    public List<Vector3> fruitLocs = new List<Vector3> {
      new Vector3 (1.3f, 1.5f, 0.0f),
      new Vector3 (0.0f, 1.5f, 1.3f),
      new Vector3 (1.3f, 1.5f, 0.0f),
      new Vector3 (0.0f, 1.5f, -1.3f),
      new Vector3 (1.3f, 1.5f, 0.0f),
      new Vector3 (1.3f, 1.5f, 0.0f),
      new Vector3 (0.0f, 1.5f, 1.3f),
      new Vector3 (0.0f, 1.5f, 1.3f),
      new Vector3 (0.0f, 1.5f, -1.3f),
      new Vector3 (0.0f, 1.5f, 1.3f)
    };

    public List<Quaternion> fruitRots = new List<Quaternion> {
      new Quaternion (0.0f, 0.7f, 0.0f, -0.7f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, 1.0f, 0.0f, 0.0f),
      new Quaternion (0.0f, 0.7f, 0.0f, -0.7f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, 0.7f, 0.0f, 0.7f)
    };

    /*
    */


    public int date;

    private bool forestIsReady;
    public ForestManager forest;
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

      GameObject home = new GameObject("Home");
      home.transform.position = homePosition;
      home.transform.parent = transform;
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

      //Print summary to Unity console
      AgentDay lastLog = ac.activityLog.Last();
      int lastDate = lastLog.date; //Should be same as date
      float lastRatio = lastLog.timeAllocationRatio;
      int trees = lastLog.numsHarvested[GoalType.Wood];
      int mangoes = lastLog.numsHarvested[GoalType.Fruit];
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
