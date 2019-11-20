using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestManager : MonoBehaviour
{
    private static int numTrees = 10;
    private float radius = 12f;
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

    public List<Vector3> treeLocs;
    /* = new List<Vector3> {
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
    };*/

    public List<Quaternion> treeRots;
    /* = new List<Quaternion> {
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
    */

    public List<Vector3> fruitLocs;
    /* = new List<Vector3> {
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
    */

    public List<Quaternion> fruitRots;
    /*
    = new List<Quaternion> {
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
    */

    /*
    */

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

      //Temporary for finding a fixed set of coords
      //In place of actual recordings and recreations

      var coords = new System.Text.StringBuilder();
      coords.AppendLine("Tree coords");

      var treeRots = new System.Text.StringBuilder();
      treeRots.AppendLine("Tree rots");

      var fruitCoords = new System.Text.StringBuilder();
      fruitCoords.AppendLine("Fruit coords");

      var fruitRots = new System.Text.StringBuilder();
      fruitRots.AppendLine("Fruit rots");

      foreach (TreeController tree in allTrees) {
        coords.AppendLine(tree.transform.localPosition.ToString());
        treeRots.AppendLine(tree.transform.localRotation.ToString());

        foreach (HarvestableController fruit in tree.fruits) {
          if (fruit != null) {
            fruitCoords.AppendLine(fruit.transform.localPosition.ToString());
            fruitRots.AppendLine(fruit.transform.localRotation.ToString());
          }
        }

      }

      Debug.Log(coords);
      Debug.Log(treeRots);
      Debug.Log(fruitCoords);
      Debug.Log(fruitRots);
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
      //Debug.Log(treeLocs);
      //Debug.Log(treeLocs.Count);
      //Debug.Log(allTrees);
      //Debug.Log(allTrees.Count);
      if (treeLocs != null && allTrees.Count < treeLocs.Count) {
        treeTransform.localPosition = treeLocs[allTrees.Count];
        treeTransform.localRotation = treeRots[allTrees.Count];
      }
      else {
        //Just some starting values that will be false in the checker
        float x = 10;
        float z = 10;

        //Generate positions away from the blob's home corner.
        while (x + z >= 3) {
          x = Random.Range(-radius, radius);
          z = Random.Range(-radius, radius);
        }

        treeTransform.localPosition = new Vector3(x, 0, z);
        int numTurns = Random.Range(0, 4);
        treeTransform.localRotation = Quaternion.Euler(0, 90 * numTurns, 0) * treeTransform.localRotation;
      }

      //Manage TreeController
      TreeController tc = newTree.GetComponent<TreeController>();
      allTrees.Add(tc);
      tc.GrowMangoes();
    }
}
