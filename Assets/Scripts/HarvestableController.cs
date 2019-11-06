using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestableController : MonoBehaviour
{
    public bool harvested {get; protected set;}

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual GameObject HandleHarvest()
    {
      harvested = true;
      return gameObject;
      //Returns gameObject rather than this because subclasses may return new
      //game objects. The tree returns a wood object, for example.
    }
}
