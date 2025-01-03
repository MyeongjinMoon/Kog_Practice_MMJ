using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PrefabFactory
{
    public IPrefab GeneratePrefab()
    {
        IPrefab prefab = CreatePrefab();
        prefab.Generate();
        return prefab;
    }
    protected abstract IPrefab CreatePrefab();
}
public class PropFactory : PrefabFactory
{
    protected override IPrefab CreatePrefab()
    {
        return new Prop();
    }
}
public class CharacterFactory : PrefabFactory
{
    protected override IPrefab CreatePrefab()
    {
        return new Character();
    }
}
public class CarFactory : PrefabFactory
{
    protected override IPrefab CreatePrefab()
    {
        return new Car();
    }
}