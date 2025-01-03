using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
	private void Start()
	{
		Factory factory = new FactoryA();

		IProduct product = factory.CreateOperation();

		factory = new FactoryB();

		product = factory.CreateOperation();
	}
}
