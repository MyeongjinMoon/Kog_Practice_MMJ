using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

public interface IProduct
{
    public void Setting();
}

public class ProductA : IProduct
{
    public void Setting()
    {
        Debug.Log("Setting A");
    }
}
public class ProductB : IProduct
{
    public void Setting()
    {
        Debug.Log("Setting B");
    }
}