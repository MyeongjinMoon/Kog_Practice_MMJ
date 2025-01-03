using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPrefab
{
	public void Generate();
}
public class Prop : IPrefab
{
	public void Generate()
	{
        Debug.Log("Generate Prop");
	}
}
public class Character : IPrefab
{
    public void Generate()
    {
        Debug.Log("Generate Character");
    }
}
public class Car : IPrefab
{
    public void Generate()
    {
        Debug.Log("Generate Car");
    }
}