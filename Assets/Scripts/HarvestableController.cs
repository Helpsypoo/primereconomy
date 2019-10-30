using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestableController : MonoBehaviour
{
    //I would prefer to hide this from the editor, but it needs to be writable
    //by subclasses and readable by AgentController
    public bool harvested = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
