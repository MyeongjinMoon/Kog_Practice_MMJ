using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Factory
{
	public IProduct CreateOperation()
	{
		IProduct product = createProduct();
		product.Setting();
		return product;
	}
	abstract protected IProduct createProduct();
}

public class FactoryA : Factory
{
    protected override IProduct createProduct()
	{
		return new ProductA();
	}
}

public class FactoryB : Factory
{
    protected override IProduct createProduct()
    {
        return new ProductB();
    }
}