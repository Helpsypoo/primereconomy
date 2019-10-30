using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public GameObject home;

    private GameObject target = null;
    private int mode = 0; //0 for wood, 1 for food
    private GameObject heldObject = null;

    public float walkSpeed = 15.0f;
    public float turnSpeed = 15.0f;
    public float harvestDistance = 3.0f;
    public float homeDistance = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
      //target = gameObject; //Target self to avoid null
    }

    // Update is called once per frame
    void Update()
    {
        if (heldObject == null) {
            //Pick target
            if (target == null)
            {
              switch(mode)
              {
                case 0:
                  Debug.Log("Finding wood");
                  target = FindClosestGoodWithTag("wood");
                  break;
                case 1:
                  Debug.Log("Finding food");
                  target = FindClosestGoodWithTag("food");
                  break;
              }
              Debug.Log(target.name);

              //TODO: Handle the case where no more potential targets remain
            }

            //Move toward target
            /*Vector3 heading = target.transform.position - transform.position;
            heading = new Vector3 (heading.x, 0, heading.z); //project to xz plane
            float distance = heading.magnitude;
            if (distance > harvestDistance)
            {
                heading = heading / distance; //Normalize heading
                transform.Translate(heading * Time.deltaTime * walkSpeed); //, Space.World
            }
            */
            if (GoToTargetIfNotThere(harvestDistance))
            {}
            else
            {
              //Harvest
              switch(mode)
              {
                case 0:
                  heldObject = target.GetComponent<TreeController>().HandleHarvest();
                  break;
                case 1:
                  //TODO: FoodController
                  //heldObject = target.GetComponent<FoodController>().HandleHarvest();
                  break;
              }
              heldObject.transform.parent = gameObject.transform;
              //TODO: Animate movement of heldObject
            }
        }
        else {
          target = home;
          /*
          Vector3 heading = target.transform.position - transform.position;
          heading = new Vector3 (heading.x, 0, heading.z); //project to xz plane
          float distance = heading.magnitude;
          if (distance > 0.1) //TODO: Handle this better
          {
              heading = heading / distance; //Normalize heading
              transform.Translate(heading * Time.deltaTime * walkSpeed); //, Space.World
          }
          */
          if (GoToTargetIfNotThere(homeDistance))
          {}
          else
          {
            heldObject.transform.parent = null;
            heldObject = null;
            target = null;
          }
        }





        /*else if (heldObject == null)
        {
          //Harvest
          //GameObject toHarvest = null; //apparently assigning to null avoids
                                       //compiling errors
          //Copied from above switch, could surely make more compact
          switch(mode)
          {
            case 1:
              heldObject = target.GetComponent<TreeController>().HandleHarvest();
              break;
            case 2:
              //toHarvest = target.GetComponent<FoodController>().HandleHarvest();
              break;
          }
          Debug.Log("Grabbing " + heldObject.name);

          heldObject.transform.parent = gameObject.transform;

          //TODO: Animate movement of heldObject
        }
        */
    }

    private bool GoToTargetIfNotThere(float goalDistance)
    {
      //This seems like it might break some best practices

      Vector3 heading = target.transform.position - transform.position;
      heading = new Vector3 (heading.x, 0, heading.z); //project to xz plane
      float distance = heading.magnitude;
      if (distance > goalDistance)
      {
          //heading = heading / distance; //Normalize heading

          //transform.Translate(heading * Time.deltaTime * walkSpeed); //, Space.World
          gameObject.transform.position = Vector3.MoveTowards(
              gameObject.transform.position,
              target.transform.position,
              Time.deltaTime * walkSpeed
          );

          //Can't be the easiest way to rotate to face something over time
          float yGoalAngle = -Mathf.Atan2(heading.z, heading.x);
          float yAngleDiff = yGoalAngle - transform.eulerAngles.y * Mathf.Deg2Rad;
          //Debug.Log(yGoalAngle * Mathf.Rad2Deg);
          Debug.Log(transform.eulerAngles.y);
          //Debug.Log(yAngleDiff * Mathf.Rad2Deg);
          while (yAngleDiff > Mathf.PI) {yAngleDiff -= 2 * Mathf.PI;}
          while (yAngleDiff < -Mathf.PI) {yAngleDiff += 2 * Mathf.PI;}
          if (Mathf.Abs(yAngleDiff) > Time.deltaTime * turnSpeed)
          {
            yAngleDiff = Time.deltaTime * turnSpeed * Mathf.Sign(yAngleDiff);
          }
          yGoalAngle = transform.eulerAngles.y * Mathf.Deg2Rad + yAngleDiff;
          Vector3 newEulerAngle = new Vector3 (0, Mathf.Rad2Deg * yGoalAngle, 0);
          transform.eulerAngles = newEulerAngle;


          return true;
      }
      else
      {
        return false;
      }

    }

    public GameObject FindClosestGoodWithTag(string tag)
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
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }
}
