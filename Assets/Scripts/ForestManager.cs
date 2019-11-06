using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestManager : MonoBehaviour
{
    public GameObject tree;
    public static int numTrees = 20;
    public float radius = 15f;
    public GameObject wood;

    public TreeController[] trees = new TreeController[numTrees];

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
        AddTree(i);
      }
    }

    public void ReplenishForest()
    {
      foreach (TreeController tc in trees) //(int i = 0; i < numTrees; i++)
      {
        //TreeController tc = trees[i];
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

    void AddTree(int treeIndex)
    {
      //Create tree
      GameObject newTree = Instantiate(tree, gameObject.transform);

      //Translate and rotate tree GameObject
      Transform treeTransform = newTree.transform;
      Vector3 pos = new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
      treeTransform.Translate(pos);
      int numTurns = Random.Range(0, 4);
      treeTransform.Rotate(0, 90 * numTurns, 0, Space.Self);

      //Manage TreeController
      TreeController tc = newTree.GetComponent<TreeController>();
      trees[treeIndex] = tc;
      tc.forest = this;
      tc.GrowMangoes();
    }
}
