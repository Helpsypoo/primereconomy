using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestManager : MonoBehaviour
{
    private static int numTrees = 10;
    private float radius = 11f;
    public GameObject wood;

    public List<HarvestableController> treelessFruits {
      get {
        var fruits = new List<HarvestableController>();
        foreach (Transform transform in EconomyManager.instance.transform) {
          if (transform.tag == "fruit") {
            fruits.Add(transform.GetComponent<HarvestableController>());
          }
        }
        return fruits;
      }
    }

    public List<HarvestableController> allTrees = new List<HarvestableController>();
    public List<HarvestableController> allFruit {
      get {
        var fruit = new List<HarvestableController>();
        foreach (TreeController t in allTrees) {
          foreach (var f in t.fruits)
          {
            if (f != null) {
              fruit.Add(f);
            }
          }
        }
        foreach (HarvestableController f in treelessFruits)
        {
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


    //Temporary manual assignment for considstent locations until I set up
    //actual recordings
    public List<Vector3> treeLocs = new List<Vector3> {
      new Vector3 (-6.4f, 0.0f, -7.2f),
      new Vector3 (-7.8f, 0.0f, -5.5f),
      new Vector3 (-11.0f, 0.0f, 1.6f),
      new Vector3 (7.9f, 0.0f, -8.0f),
      new Vector3 (-10.2f, 0.0f, 10.7f),
      new Vector3 (-9.5f, 0.0f, -9.2f),
      new Vector3 (-3.1f, 0.0f, 2.6f),
      new Vector3 (0.9f, 0.0f, -3.6f),
      new Vector3 (-2.7f, 0.0f, -8.3f),
      new Vector3 (3.1f, 0.0f, -0.3f)
    };

    public List<Quaternion> treeRots = new List<Quaternion> {
      new Quaternion (0.0f, -0.7f, 0.0f, -0.7f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, -0.7f, 0.0f, -0.7f),
      new Quaternion (0.0f, -0.7f, 0.0f, 0.7f),
      new Quaternion (0.0f, -0.7f, 0.0f, -0.7f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, -0.7f, 0.0f, -0.7f),
      new Quaternion (0.0f, -1.0f, 0.0f, 0.0f),
      new Quaternion (0.0f, -1.0f, 0.0f, 0.0f),
      new Quaternion (0.0f, -1.0f, 0.0f, 0.0f)
    };

    public List<Vector3> fruitLocs = new List<Vector3> {
      new Vector3 (0.0f, 1.5f, 1.3f),
      new Vector3 (-1.3f, 1.5f, 0.0f),
      new Vector3 (-1.3f, 1.5f, 0.0f),
      new Vector3 (-1.3f, 1.5f, 0.0f),
      new Vector3 (0.0f, 1.5f, -1.3f),
      new Vector3 (0.0f, 1.5f, 1.3f),
      new Vector3 (0.0f, 1.5f, -1.3f),
      new Vector3 (0.0f, 1.5f, 1.3f),
      new Vector3 (0.0f, 1.5f, 1.3f),
      new Vector3 (-1.3f, 1.5f, 0.0f)
    };

    public List<Quaternion> fruitRots = new List<Quaternion> {
      new Quaternion (0.0f, -1.0f, 0.0f, 0.0f),
      new Quaternion (0.0f, 0.7f, 0.0f, -0.7f),
      new Quaternion (0.0f, -0.7f, 0.0f, 0.7f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, 1.0f, 0.0f, 0.0f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, 0.0f, 0.0f, 1.0f),
      new Quaternion (0.0f, -0.7f, 0.0f, 0.7f),
      new Quaternion (0.0f, 0.7f, 0.0f, 0.7f)
    };

    /*
    //Uncomment for randomized forest
    public List<Vector3> treeLocs;
    public List<Quaternion> treeRots;
    public List<Vector3> fruitLocs;
    public List<Quaternion> fruitRots;
    */

    // Awake is called before Start
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator PrepForest(float duration)
    {
      float delay = (float) duration / numTrees;

      for (int i = 0; i < numTrees; i++)
      {
        AddTree();
        yield return new WaitForSeconds(delay);
      }

      //Wait until animations are done.
      //TODO: Figure out how to run code on animate state exit.
      bool animating = true;
      do
      {
        animating = false;
        foreach (TreeController tc in allTrees)
        {
          Animator anim = tc.GetComponent<Animator>();
          if (anim.GetCurrentAnimatorStateInfo(0).IsName("Bounce In"))
          {
            animating = true;
            break;
          }
        }
        yield return null;
      } while (animating == true);

      //TODO: Make forests aware of own ready state, prepping for multi-forest
      //situations
      EconomyManager.instance.forestIsReady = true;



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

    public IEnumerator ReplenishForest(float duration)
    //Not really duration, more of a start window.
    {
      float delay = (float) duration / numTrees;

      foreach (TreeController tc in allTrees)
      {
        //Regrow harvested trees and mangoes, handle mangoes on ground
        if (tc.harvested == true)
        {
          tc.RegrowTree();
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
        StartCoroutine(tc.GrowMangoes());

        yield return new WaitForSeconds(delay);
      }
      foreach (HarvestableController f in treelessFruits)
      {
        StartCoroutine(f.Disappear());
      }

      //Wait until animations are done.
      //TODO: Figure out how to run code on animate state exit.
      bool animating = true;
      do
      {
        animating = false;
        foreach (TreeController tc in allTrees)
        {
          Animator anim = tc.GetComponent<Animator>();
          if (anim.GetCurrentAnimatorStateInfo(0).IsName("Bounce In"))
          {
            animating = true;
            break;
          }
        }
        yield return null;
      } while (animating == true);

      //TODO: Make forests aware of own ready state, prepping for multi-forest
      //situations
      EconomyManager.instance.forestIsReady = true;
    }

    void AddTree()
    {
      //Create tree
      GameObject newTree = Instantiate(EconomyManager.instance.treePrefab);//, gameObject.transform);

      //Translate and rotate tree GameObject
      Transform treeTransform = newTree.transform;
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
      StartCoroutine(tc.GrowMangoes());
    }
}
