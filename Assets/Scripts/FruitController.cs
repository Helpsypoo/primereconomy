using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitController : HarvestableController
{
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
      harvested = true;
      return gameObject;
    }
}
