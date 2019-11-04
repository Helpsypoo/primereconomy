using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public GameObject agent;
    public GameObject forest;

    private bool forestIsReady;

    void Awake()
    {
      forest.GetComponent<ForestManager>().PrepForest();
      forestIsReady = true;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      if (Input.GetKeyDown(KeyCode.Space) && forestIsReady == true)
        {
          agent.GetComponent<AgentController>().StartWorkDay();
          forestIsReady = false;
        }
    }

    public void AgentIsDone(GameObject agent)
    {
      //If there were multiple agents, track doneness here

      //ReplenishForest
      forest.GetComponent<ForestManager>().ReplenishForest();
      forestIsReady = true;
    }
}
