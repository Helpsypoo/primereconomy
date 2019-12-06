using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestableController : MonoBehaviour
{
    public bool harvested {get; protected set;}
    private float defaultTransitionDuration = 0.5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject Create(GameObject prefab)
    {
      GameObject obj = Instantiate(prefab);
      //"Bounce In" animation state is triggered on awake.

      return obj;
    }

    public IEnumerator Disappear()
    {
      Animator anim = gameObject.GetComponent<Animator>();
      anim.SetTrigger("shrinkAway");

      Debug.Log("Disappearing");

      //TODO: figure out how to trigger code on animation state exit
      //Truly silly
      //Wait to see that the animation has started
      //Then wait to see that the animation has finished
      //Then set inactive

      bool animating;
      do
      {
        animating = false;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Shrink Away"))
        {
          animating = true;
        }
        yield return null;
      } while (animating == false);

      animating = true;
      do
      {
        animating = false;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Shrink Away"))
        {
          animating = true;
        }
        yield return null;
      } while (animating == true);

      OnDisappear();
    }

    protected virtual void OnDisappear()
    {
      Destroy(gameObject);
    }

    public virtual GameObject HandleHarvest()
    {
      harvested = true;
      return gameObject;
      //Returns gameObject rather than this because subclasses may return new
      //game objects. The tree returns a wood object, for example.
    }
}
