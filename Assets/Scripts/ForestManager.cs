using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestManager : MonoBehaviour
{
    private static int numTrees = 20;
    private float radius = 15f;
    public GameObject wood;

    public List<HarvestableController> allTrees = new List<HarvestableController>();
    public List<HarvestableController> allFruit {
      get {
        var fruit = new List<HarvestableController>();
        foreach (TreeController t in allTrees) {
          foreach (var f in t.fruits)
            if (f != null) {
              fruit.Add(f);
            }
          }
        return fruit;
      }
    }

    public Dictionary<GoalType, List<HarvestableController>> allHarvestables {
      //Dict needs getter to trigger getter/update of allFruit
      get {
        var all = new Dictionary<GoalType, List<HarvestableController>>();
        all.Add(GoalType.Wood, allTrees);
        all.Add(GoalType.Fruit, allFruit);
        return all;
      }
    }

    // Awake is called before Start
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PrepForest()
    {
      for (int i = 0; i < numTrees; i++)
      {
        AddTree();
      }
    }

    public void ReplenishForest()
    {
      foreach (TreeController tc in allTrees)
      {
        //Regrow harvested trees and mangoes, handle mangoes on ground
        if (tc.harvested == true)
        {
          tc.RegrowTree();

          //Set all of harvested tree's fruit to null, destroy unharvested fruit
          for (int j = 0; j < tc.fruits.Length; j++)
          {
            if (tc.fruits[j] != null &&
                tc.fruits[j].harvested == false)
              {
                Destroy(tc.fruits[j]);
              }
            tc.fruits[j] = null;
          }
        }
        else {
          //Make sure harvested fruits are out of tree's fruit array
          //before replenishing fruit
          //Might be cleaner to do this on harvest.
          for (int j = 0; j < tc.fruits.Length; j++)
          {
            if (tc.fruits[j] != null &&
                tc.fruits[j].harvested == true)
              {
                tc.fruits[j] = null;
              }
          }
        }
        tc.GrowMangoes();
      }
    }

    void AddTree()
    {
      //Create tree
      GameObject newTree = Instantiate(EconomyManager.instance.treePrefab, gameObject.transform);

      //Translate and rotate tree GameObject
      Transform treeTransform = newTree.transform;
      Vector3 pos = new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
      treeTransform.Translate(pos);
      int numTurns = Random.Range(0, 4);
      treeTransform.Rotate(0, 90 * numTurns, 0, Space.Self);

      //Manage TreeController
      TreeController tc = newTree.GetComponent<TreeController>();
      allTrees.Add(tc);
      tc.GrowMangoes();
    }
}
