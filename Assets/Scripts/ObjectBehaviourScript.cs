using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class ObjectBehaviourScript : MonoBehaviour
{

    //гены
    //переменные
    //генная структура
    //значения генов
    //набор действий
    //инвентарь
    //мозг   

    //метод размножения
    //public int id;
    SpriteRenderer sr;

    Rigidbody2D rb;

    private System.Random r = new System.Random();

    public List<Gen> dna;

    public List<Axon> axons = new List<Axon>();

    public Signal currentSignal = new Signal() { magnitude = 0 };

    //public float Food;
    //public float Minerals;
    public float HP = 10;

    public object[] inventory;
    private MainController controller;

    public int proceessDeley = 1;
    
    public int fadeDeley = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>(); 
        controller = GameObject.Find("MainController").GetComponent<MainController>();
        var coll = GetComponents<CircleCollider2D>().First(coll => coll.isTrigger);
        coll.radius = (float)dna.FirstOrDefault(gen => gen.name == Gens.Радиус_притяжения).value;
        inventory = new object[(int)dna.FirstOrDefault(gen => gen.name == Gens.Размерность_инвентаря).value];
    }

    //void Awake()
    //{
    //    SetId();
    //}

    void FixedUpdate()
    {

    }

    void Update()
    {
        SubstractAxonWeight();
        FadeCurrentSignal();
        ReproductElements();
        RemoveDeadAxons();//надо делать реже??
        foreach (var line in GameObject.FindGameObjectsWithTag("Axon"))
        {
            Destroy(line);
        }
    }

    private void ReproductElements()
    {
        //if (Food > 0) HP++; Food--;
        //if (Minerals > 0)
        //    //Изменять что-то
        //    ;
        for(int i = 0; i< inventory.Length; i++)
        {
            var obj = (GameObject)inventory[i];
            if (obj.transform.parent.name == "Organics")
            {
                HP += obj.GetComponent<cfgOrganic>().sustenance;
                inventory[i] = null;
            }
        }
    }

    void LateUpdate()
    {
        VisualizeCurrentSignal();
        VisualizeAxons();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var obj = collision.collider.gameObject;
        switch (obj.tag)
        {
            case "Cell":
                {
                    SortingCells(obj); 
                    RunSignal(new Signal() { value = "Collision", magnitude = 1 });
                    break;
                }
            default: AddToInventory(obj); break;
        }
        
        
    }

    private void AddToInventory(GameObject obj)
    {
        foreach(var invs in inventory.First(inv => inv != null))
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = obj;
                controller.PushToPool(obj);
                break;
            }
        }
    }


    private void SortingCells(GameObject cell)
    {
        CreateAxon(cell);
    }
    private void CreateAxon(GameObject cell)
    {
        if (axons.Count(ax => ax.cell == cell) == 0)
        {

            var p = (float)dna.FirstOrDefault(gen => gen.name == Gens.Вероятность_создания_аксона).value;

            if (p < r.NextDouble())
            {

                AddAxon(cell, Random.Range(0, 1f));
            }
        }
    }

    private void FadeCurrentSignal()
    {
        //StartCoroutine(Pause(fadeDeley));
        currentSignal = new Signal()
        {
            magnitude = (float)Math.Round(currentSignal.magnitude * (float)dna.First(gen => gen.name == Gens.Коэфициент_затухания_внутреннего_сигнала).value, 4),
            value = currentSignal.value
        };
    }
    private void VisualizeCurrentSignal()
    {
        //sr.color = new Color(1, 1 - Mathf.Clamp(currentSignal.magnitude, 0, 1), 1 - Mathf.Clamp(currentSignal.magnitude, 0, 1));
        sr.color = new Color(1, 1 / (currentSignal.magnitude*10 + 1), 1 / (currentSignal.magnitude*10 + 1));
    }
    private void VisualizeAxons()
    {
        foreach (var ax in axons)
        {
            var line = Instantiate(controller
                    .gameObjects
                    .First(obj => obj.name == "AxonLine"));
            line.transform.parent = this.gameObject.transform.Find("Axons");
            var renderer = line.GetComponent<LineRenderer>();

            var vec = this.transform.position - ax.cell.transform.position;
            var a = vec.x;
            var b = vec.y;
            vec.x = b;
            vec.y = -a;
            renderer.SetPosition(0, Vector3.zero + Vector3.Normalize(vec));
            renderer.SetPosition(1, this.transform.position - ax.cell.transform.position + Vector3.Normalize(vec));

            //renderer.SetPosition(0, Vector3.zero);
            //renderer.SetPosition(1, (this.transform.position - ax.cell.transform.position)/2 + Vector3.Normalize(vec) / 2);
            //renderer.SetPosition(2, this.transform.position - ax.cell.transform.position);

            renderer.widthMultiplier = Mathf.Clamp(ax.weight, 0.3f, 1);
            line.transform.position = ax.cell.transform.position;
        }
    }

    private void RemoveDeadAxons() //надо вызывать иногда
    {
        for (int i = 0; i < axons.Count; i++)
        {
            if (axons[i].cell == null)
            {
                axons.RemoveAt(i);
                i--;
            }
        }
    }

    //private void SetId()
    //{
    //    var controller = GameObject.Find("MainController").GetComponent<MainController>();
    //    id = controller.cellCount;
    //    controller.cellCount++;
    //}

    public void AddAxon(GameObject cell, float weight)
    {
        axons.Add(new Axon() { cell = cell, weight = weight });
    }

    public void RunSignal(Signal signal)
    {
        currentSignal = signal;//было
        SendCurrentSignal();
    }

    public async Task ReceiveSignal(GameObject sender, Signal signal)
    {
        if (signal.magnitude < ((float)dna.First(gen => gen.name == Gens.Минимальная_магнитуда_сигнала_для_получения).value))
           return;
        Methods.DoSmthByType(this, signal);
        currentSignal = Methods.ProcessSignalBySignal(this, signal);
        //currentSignal.magnitude = (float)Math.Round(Math.Tanh(currentSignal.magnitude+signal.magnitude), 2); // (1...2 * 1...2) / 4 = ~0.5 максимальное значение
        //a++;
        //await SendCurrentSignal();
        await Task.Run(new Action(SendCurrentSignal));
        return;

    }


    public async void SendCurrentSignal()
    {
        for (var i = 0; i < axons.Count; i++)
        {
            await axons[i].cell
                .GetComponent<ObjectBehaviourScript>()
                .ReceiveSignal(
                    this.gameObject,
                    new Signal()
                    {
                        value = currentSignal.value,
                            //magnitude = Methods.NewMagnitudeByWeight(this, ax.cell.GetComponent<ObjectBehaviourScript>(), currentSignal.magnitude)
                            magnitude = (float)Math.Round(currentSignal.magnitude * axons[i].weight, 4)
                });
                axons[i] = new Axon()
                {
                    weight = Mathf.Clamp(axons[i].weight + controller.AddAxonWeightValue, 0, 1),
                    cell = axons[i].cell
                };
        }
    }


    private void SubstractAxonWeight()
    {
        for (var i = 0; i < axons.Count; i++)
        {
            if (axons[i].weight < 0.0001)
            {
                axons.RemoveAt(i);
                i--;
                continue;
            }
            

            axons[i] = new Axon()
            {
                weight = (float)(axons[i].weight - controller.SubstractAxonWeightValue * Time.deltaTime),
                cell = axons[i].cell
            };
        }
    }
}

public static class Methods
{
    static MainController controller = GameObject.Find("MainController").GetComponent<MainController>();    
    public static Signal ProcessSignalBySignal(ObjectBehaviourScript currentCell, Signal signal)
    {
        Signal result = new Signal();
        switch (currentCell.dna.First(gen => gen.name == Gens.Тип).value.ToString())
        {
            case "SimpleCell":
                {
                    result.value = signal.value;
                    result.magnitude =
                        (float)(Math.Tanh(currentCell.currentSignal.magnitude + signal.magnitude) 
                        * ((float)currentCell
                            .dna
                            .First(gen => gen.name == Gens.Коэфициент_затухания_передаваемого_сигнала).value));
                    break;
                }
            case "MotorCell":
                {
                    result.value = null;
                    result.magnitude = 0;
                    break;
                }
            default:
                {
                    result = signal;
                    break;
                }
        }
        return result;
    }

    public static float NewMagnitudeByWeight(ObjectBehaviourScript fromCell, ObjectBehaviourScript toCell, float magnitude)
    {
        var weight = fromCell.axons.First(ax => ax.cell == toCell.gameObject).weight;
        switch (fromCell.dna.First(gen => gen.name == Gens.Тип).value.ToString())
        {
            case "SimpleCell":
                {
                    return magnitude * weight;//какая-то функция
                }
            default:
                {
                    return magnitude
                        * (weight);//какая-то функция
                }
        }
    }

    public static void DoSmthByType(ObjectBehaviourScript currentCell, Signal signal)
    {
        var cellGO = currentCell.gameObject;
        var rb = cellGO.GetComponent<Rigidbody2D>();
        switch (currentCell.dna.First(gen => gen.name == Gens.Тип).value.ToString())
        {
            case "MotorCell":
                {
                    //rb.AddForce(new Vector2(currentCell.transform.forward.x, currentCell.transform.forward.y)
                    //    * signal.magnitude
                    //    * 100000);
                    rb.AddForce(new Vector2((float)Math.Cos(currentCell.transform.rotation.z),
                       (float)Math.Sin(currentCell.transform.rotation.z))*signal.magnitude, ForceMode2D.Impulse);
                    //rb.velocity = new Vector2(100, 100);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

}

[Serializable]
public struct Axon
{
    public GameObject cell;
    public float weight;
}

[Serializable]
public class Signal
{
    public object value;
    public float magnitude;//какая угодно? потом всё равно множится на вейты и коэф. затухания
}

