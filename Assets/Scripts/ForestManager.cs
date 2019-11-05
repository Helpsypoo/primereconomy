using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestManager : MonoBehaviour
{
    public GameObject tree;
    public static int numTrees = 10;
    public float radius = 10f;
    public GameObject wood;

    public GameObject[] trees = new GameObject[numTrees];
    //public GameObject[] mangoes = new GameObject[numTrees * mangoYield];

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
      for (int i = 0; i < numTrees; i++)
      {
        TreeController tc = trees[i].GetComponent<TreeController>();
        //Regrow harvested trees
        if (trees[i].activeSelf == false)
        {
          trees[i].SetActive(true);
          tc.harvested = false;
        }
        else {
          //Make sure harvested fruits are out of tree's fruit array
          //before replenishing fruit
          //Might be cleaner to do this on harvest but would have to make
          //a MangoController, I think.
          for (int j = 0; j < tc.fruits.Length; j++)
          {
            if (tc.fruits[j] != null &&
                tc.fruits[j].GetComponent<HarvestableController>().harvested == true)
              {
                tc.fruits[j] = null;
              }
          }
        }
        tc.GrowMangoes();
        //TODO: Have unharvested mangoes from harvested trees rot and disappear.
      }
    }

    void AddTree(int treeIndex)
    {
      //Create tree
      GameObject newTree = Instantiate(tree, gameObject.transform);
      trees[treeIndex] = newTree;

      //Translate and rotate tree
      Transform treeTransform = newTree.transform;
      Vector3 pos = new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
      treeTransform.Translate(pos);
      int numTurns = Random.Range(0, 4);
      treeTransform.Rotate(0, 90 * numTurns, 0, Space.Self);

      TreeController tc = newTree.GetComponent<TreeController>();
      tc.GrowMangoes();
    }
}
