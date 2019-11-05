using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : HarvestableController
{
    private float logSpawnHeight = 1.0f;
    private const int fruitCapacity = 4;  //Trees have room for four fruits
    public GameObject[] fruits = new GameObject[fruitCapacity];

    public GameObject mango;
    public static int mangoYield = 1;
    public float mangoDistance = 1.3f;
    private float mangoHeight = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
      //Debug.Log(fruits[0]);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GrowMangoes()
    {
      int numMangoes = GetNumMangoes();
      for (int i = 0; i < mangoYield - numMangoes; i++)
      {
        AddMango();
      }
    }

    void AddMango()
    {
      GameObject newMango = Instantiate(mango, gameObject.transform);
      //mangoes[mangoIndex] = newMango;
      //TODO rotate mango
      //TODO put rotation and translation in Instantiate call

      int positionIndex = GetOpenFruitIndex();
      fruits[positionIndex] = newMango;

      Vector3 mangoPos = new Vector3(mangoDistance, mangoHeight, 0);
      for (int i = 0; i < positionIndex; i++)
      {
        mangoPos = Quaternion.Euler(0, 90, 0) * mangoPos;
      }
      newMango.transform.Translate(mangoPos);
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

    int GetNumMangoes()
    {
      //Almost certainly a more better way to do this
      int numMangoes = 0;
      for (int i = 0; i < fruitCapacity; i++)
      {
        if (fruits[i] != null)
        {
          numMangoes++;
        }
      }
      return numMangoes;
    }

    public override GameObject HandleHarvest()
    {
      //GameObject log = Instantiate(log, tree.transform);
      //return log;
      Vector3 logSpawnPos = new Vector3(gameObject.transform.position.x, logSpawnHeight,
                                                        gameObject.transform.position.z);
      GameObject logPrefab = gameObject.GetComponentInParent<ForestManager>().wood;
      GameObject log = Instantiate(logPrefab, logSpawnPos, logPrefab.transform.rotation);

      //TODO make this work for more than or less than one fruit
      for (int i = 0; i < fruits.Length; i++)
      {
        if (fruits[i] != null)
        {
          fruits[i].transform.parent = null;
          Rigidbody fruitRb = fruits[i].GetComponent<Rigidbody>();//gameObject.GetComponentInChildren<Rigidbody>();
          fruitRb.useGravity = true;
          fruits[i] = null;
        }
      }


      harvested = true;
      gameObject.SetActive(false);

      return log;
    }
}
