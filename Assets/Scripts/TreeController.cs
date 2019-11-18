using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeController : HarvestableController
{
    private float logSpawnHeight = 1.0f;
    private const int fruitCapacity = 4;  //Trees have room for four fruits

    //This works kind of strangely, with indices also determining which branch
    //this fruit is on. I do like having the fixed-length array fruits, though.
    public HarvestableController[] fruits = new HarvestableController[fruitCapacity];

    private int mangoYield = 1;
    private float mangoDistance = 1.3f;
    private float mangoHeight = 1.5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RegrowTree()
    {
      gameObject.SetActive(true);
      harvested = false;
    }

    public void GrowMangoes()
    {
      int numMangoes = fruits.Count(x => x != null);
      for (int i = 0; i < mangoYield - numMangoes; i++)
      {
        AddMango();
      }
    }

    void AddMango()
    {
      GameObject newMango = Instantiate(EconomyManager.instance.mangoPrefab, gameObject.transform);
      //TODO put rotation and translation in Instantiate call

      int positionIndex = GetOpenFruitIndex();

      //I've been told to avoid GetComponent when I can, but given that this
      //mango was just instantiated as a new game object, this seems like a
      //reasonable place.
      fruits[positionIndex] = newMango.GetComponent<HarvestableController>();

      Vector3 mangoPos = new Vector3(mangoDistance, mangoHeight, 0);
      for (int i = 0; i < positionIndex; i++)
      {
        mangoPos = Quaternion.Euler(0, 90, 0) * mangoPos;
      }
      newMango.transform.Translate(mangoPos);

      //TODO rotate mango
      Quaternion mangoRot = newMango.transform.rotation;
      int numRots = Random.Range(0, 4);
      for (int i = 0; i < numRots; i++)
      {
        mangoRot = Quaternion.Euler(0, 90, 0) * mangoRot;
      }
      newMango.transform.rotation = mangoRot;
    }

    int GetOpenFruitIndex()
    {
      //Almost certainly a more compact way to do this
      List<int> openSpaces = new List<int>();
      for (int i = 0; i < fruitCapacity; i++)
      {
        if (fruits[i] == null)
        {
          openSpaces.Add(i);
        }
      }
      int openSpaceIndex = Random.Range(0, openSpaces.Count);
      return openSpaces[openSpaceIndex];
    }

    public override GameObject HandleHarvest()
    {
      Vector3 logSpawnPos = new Vector3(gameObject.transform.position.x, logSpawnHeight,
                                                        gameObject.transform.position.z);

      GameObject logPrefab = EconomyManager.instance.logPrefab;
      GameObject log = Instantiate(logPrefab, logSpawnPos, logPrefab.transform.rotation);

      //TODO make this work for more than or less than one fruit
      for (int i = 0; i < fruits.Length; i++)
      {
        if (fruits[i] != null)
        {
          fruits[i].transform.parent = EconomyManager.instance.transform;
          Rigidbody fruitRb = fruits[i].GetComponent<Rigidbody>();
          fruitRb.useGravity = true;
        }
      }

      harvested = true;
      gameObject.SetActive(false);

      return log;
    }
}
