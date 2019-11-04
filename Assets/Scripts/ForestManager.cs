using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestManager : MonoBehaviour
{
    public GameObject tree;
    public int numTrees = 10;
    public float radius = 10f;
    public GameObject wood;

    public GameObject mango;
    public int mangoYield = 1;
    public float mangoDistance = 1.3f;
    private float mangoHeight = 1.5f;

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
      
    }

    void AddTree()
    {
      //GameObject newTree = Instantiate(tree, pos, treeTransform.rotation);
      GameObject newTree = Instantiate(tree, gameObject.transform);
      Transform treeTransform = newTree.transform;
      Vector3 pos = new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
      treeTransform.Translate(pos);
      int numTurns = Random.Range(0, 4);
      treeTransform.Rotate(0, 90 * numTurns, 0, Space.Self);
      for (int i = 0; i < mangoYield; i++)
      {
        AddMango(newTree);
      }
    }

    void AddMango(GameObject tree)
    {
      GameObject newMango = Instantiate(mango, tree.transform);
      //TODO rotate mango
      //TODO put rotation and translation in Instantiate call
      //TODO randomize branch
      newMango.transform.Translate(new Vector3(mangoDistance, mangoHeight, 0));
    }
}
