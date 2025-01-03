using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
	public static event Action OnClicked;
	public static event Action SetProp;
	public static event Action SetCharacter;
	public static event Action SetCar;

	private void OnGUI()
	{
		if (GUI.Button(new Rect(
			Screen.width / 2 - 50, 10, 100, 30),
			"Click"))
		{
			if (OnClicked != null)
				OnClicked();
		}
        if (GUI.Button(new Rect(
            Screen.width / 2 - 50, 50, 100, 30),
            "Prop"))
        {
            if (SetProp != null)
                SetProp();
        }
        if (GUI.Button(new Rect(
            Screen.width / 2 - 50, 90, 100, 30),
            "Character"))
        {
            if (SetCharacter != null)
                SetCharacter();
        }
        if (GUI.Button(new Rect(
            Screen.width / 2 - 50, 130, 100, 30),
            "Car"))
        {
            if (SetCar != null)
                SetCar();
        }
    }
}
