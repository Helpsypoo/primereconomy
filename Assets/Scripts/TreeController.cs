using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : HarvestableController
{
    //public bool alive = true;
    //public bool harvested = false;
    private float logSpawnHeight = 1.0f;
    //private bool harvested = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject HandleHarvest()
    {
      //GameObject log = Instantiate(log, tree.transform);
      //return log;
      Vector3 logSpawnPos = new Vector3(gameObject.transform.position.x, logSpawnHeight,
                                                        gameObject.transform.position.z);
      GameObject logPrefab = gameObject.GetComponentInParent<ForestManager>().wood;
      GameObject log = Instantiate(logPrefab, logSpawnPos, logPrefab.transform.rotation);

      GameObject fruit = gameObject.transform.GetChild(0).gameObject;
      //TODO make this work for more than one fruit
      fruit.transform.parent = null;
      Rigidbody fruitRb = fruit.GetComponent<Rigidbody>();//gameObject.GetComponentInChildren<Rigidbody>();
      fruitRb.useGravity = true;

      harvested = true;
      gameObject.SetActive(false);

      return log;
    }
}
