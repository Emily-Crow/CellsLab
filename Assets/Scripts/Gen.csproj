using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class Gen
{
    public Gens name;
    public object value;
    public string valueForDebug;

    public Gen(Gens _name, object _value)
    {
        name = _name;
        value = _value;
        valueForDebug = value.ToString();
    }
}


public enum Gens
{
    Тип,
    Вероятность_создания_аксона,
    Коэфициент_затухания_передаваемого_сигнала,
    Минимальная_магнитуда_сигнала_для_получения,
    Коэфициент_затухания_внутреннего_сигнала,
    Радиус_притяжения,
    Размерность_инвентаря
}
