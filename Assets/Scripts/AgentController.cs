using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public GameObject home;

    private GameObject target = null;
    private int mode; //0 for wood, 1 for fruit
    private GameObject heldObject = null;

    public float walkSpeed = 15.0f;
    public float turnSpeed = 15.0f;
    public float harvestDistance = 3.0f;
    public float deliverDistance = 0.1f;

    public float collectionTime = 10.0f; //seconds
    //public float accelerationFactor = 1.0f; //For speeding up sims later on.
    public float woodCollectionTimeRatio = 0.5f;
    private bool dayOver = false;

    // Start is called before the first frame update
    void Start()
    {
      StartCoroutine("GoHarvest");
    }

    // Update is called once per frame
    void Update()
    {
      if (Time.time > collectionTime)
      {
        dayOver = true;
      }
    }

    IEnumerator GoHarvest()
    {
      //Check time allocation
      if (Time.time < woodCollectionTimeRatio * collectionTime)
      {
        mode = 0; //Collect wood
      }
      else
      {
        mode = 1; //Collect fruit
      }

      //Pick target
      if (target == null)
      {
        switch(mode)
        {
          case 0:
            target = FindClosestHarvestableWithTag("wood");
            break;
          case 1:
            target = FindClosestHarvestableWithTag("fruit");
            break;
        }
        //TODO: Handle the case where no more potential targets remain
      }

      while (GoToTargetIfNotThere(harvestDistance))
      {
        yield return null;
      }
      //Harvest
      heldObject = target.GetComponent<HarvestableController>().HandleHarvest();
      heldObject.transform.parent = gameObject.transform;
      //TODO: Animate movement of heldObject

      StartCoroutine("GoDeliver");
    }

    IEnumerator GoDeliver()
    {
      target = home;
      while (GoToTargetIfNotThere(deliverDistance))
      {
        yield return null;
      }
      if (heldObject == null) {
        Debug.LogWarning("Agent is trying to deliver null in GoDeliver()");
      }
      heldObject.transform.parent = null;
      heldObject = null;
      target = null;
      //TODO: Animate movement of heldObject

      if (dayOver)
      {
        StartCoroutine("GoHome");
      }
      else
      {
        StartCoroutine("GoHarvest");
      }
    }

    IEnumerator GoHome()
    {
      //Keeping this separate from GoDeliver since I'm planning to have separate
      //home and storage area objects.
      target = home;
      while (GoToTargetIfNotThere())
      {
        yield return null;
      }
    }

    private bool GoToTargetIfNotThere(float goalDistance = 0)
    {
      //This seems like it might break some best practices
      Vector3 heading = target.transform.position - transform.position;
      heading = new Vector3 (heading.x, 0, heading.z); //project to xz plane
      float distance = heading.magnitude;
      if (distance > goalDistance)
      {
          Vector3 groundTarget = new Vector3(
            target.transform.position.x,
            0,
            target.transform.position.z
          );

          //Translate
          gameObject.transform.position = Vector3.MoveTowards(
            gameObject.transform.position,
            groundTarget,
            Time.deltaTime * walkSpeed
          );

          //Rotate
          Vector3 targetDir = groundTarget - transform.position;
          float step = turnSpeed * Time.deltaTime;
          Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
          transform.rotation = Quaternion.LookRotation(newDir);

          return true;
      }
      else
      {
        return false;
      }
    }

    public GameObject FindClosestHarvestableWithTag(string tag)
    {
        GameObject[] goods;
        goods = GameObject.FindGameObjectsWithTag(tag);

        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in goods)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                if (go.GetComponent<HarvestableController>().harvested == false)
                {
                  closest = go;
                  distance = curDistance;
                }
            }
        }
        return closest;
    }
}
