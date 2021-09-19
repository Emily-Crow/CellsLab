using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MainController : MonoBehaviour
{
    public Elemental[] elemental = new Elemental[1];

    public GameObject StartPoint;
    public GameObject EndPoint;

    public float SubstractAxonWeightValue = 0.0001f;
    public float AddAxonWeightValue = 0.01f;

    public List<GameObject> gameObjects;

    System.Random r = new System.Random();
    //public int cellCount;
    void Start()
    {
        InitData();
        AddElementalToScene();
    }

    private void AddElementalToScene()
    {
        foreach (var el in elemental)
        {
            for (int i = 0; i < el.count; i++)
            {
                var cell = Instantiate(
                    el.obj,
                    new Vector3(
                        UnityEngine.Random.Range(StartPoint.transform.position.x, EndPoint.transform.position.x),
                        UnityEngine.Random.Range(StartPoint.transform.position.y, EndPoint.transform.position.y)),
                    new Quaternion());
                switch (el.type)
                {
                    case objectTypes.SimpleCell:
                        {
                            cell.transform.parent = GameObject.Find("SimpleCells").transform;
                            cell.GetComponent<ObjectBehaviourScript>()
                                .dna = GetBasicDna(objectTypes.SimpleCell);
                            break;
                        }
                    case objectTypes.MotorCell:
                        {
                            cell.transform.parent = GameObject.Find("MotorCells").transform;
                            cell.GetComponent<ObjectBehaviourScript>()
                                .dna = GetBasicDna(objectTypes.MotorCell);
                            break;
                        }
                    case objectTypes.SensorCell:
                        {
                            cell.transform.parent = GameObject.Find("SensorCells").transform;
                            cell.GetComponent<ObjectBehaviourScript>()
                                .dna = GetBasicDna(objectTypes.SensorCell);

                            break;
                        }
                    case objectTypes.Organic:
                        {
                            cell.transform.parent = GameObject.Find("Elements").transform;
                            break;
                        }
                }
            }
        }
    }

    private void InitData()
    {
    }


    public List<Gen> GetBasicDna(objectTypes type)
    {
        return new List<Gen>() {
                new Gen(Gens.���, type),
                new Gen(Gens.�����������_��������_������, 0.3f),
                new Gen(Gens.����������_���������_�������������_�������, 0.8f),
                new Gen(Gens.�����������_���������_�������_���_���������, 0.0001f),
                new Gen(Gens.����������_���������_�����������_�������, 0.9f),
                new Gen(Gens.������_����������, Mathf.Clamp((float) r.NextDouble() * (float) r.NextDouble(), 0.2f, 1)),
                new Gen(Gens.�����������_���������, 5)}; 
    }

    public void GetOutOfPool(string gameObjectName, Transform parent)
    {
        var obj = GameObject.Find("Pool").transform.Find(gameObjectName);
        obj.transform.parent = parent;
        obj.gameObject.SetActive(true);
    }

    public void PushToPool(GameObject obj)
    {
        obj.transform.parent = GameObject.Find("Pool").transform;
        obj.SetActive(false);
    }
}

[Serializable] 
public struct Elemental //start init
{
    public GameObject obj;
    public int count;
    public objectTypes type;
}

public enum objectTypes
{
    SimpleCell,
    MotorCell,
    SensorCell,
    Organic
}