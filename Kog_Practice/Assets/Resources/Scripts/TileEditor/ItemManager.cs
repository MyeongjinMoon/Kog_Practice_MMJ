using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
	PrefabFactory factory;
	private void OnEnable()
	{
		EventManager.OnClicked += GeneratePrefab;
		EventManager.SetProp += SetProp;
		EventManager.SetCharacter += SetCharacter;
		EventManager.SetCar += SetCar;
	}
	private void OnDisable()
	{
		EventManager.OnClicked -= GeneratePrefab;
		EventManager.SetProp -= SetProp;
		EventManager.SetCharacter -= SetCharacter;
		EventManager.SetCar -= SetCar;
	}
	private void GeneratePrefab()
	{
		if (factory != null) factory.GeneratePrefab();
	}
	private void SetProp()
	{
        Debug.Log("Setting Prop");
		factory = new PropFactory();
    }
	private void SetCharacter()
	{
        Debug.Log("Setting Character");
		factory = new CharacterFactory();
    }
	private void SetCar()
	{
        Debug.Log("Setting Car");
		factory = new CarFactory();
    }
}
