/*
Класс для запуска методов в основном потоке Unity, например когда приходил callback из сокета
Использование: 
	MainRunner.Execute.Enqueue(() => { 
		// наш метод
	});
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MainRunner : MonoBehaviour {
	
	public readonly static Queue<Action> Execute = new Queue<Action>();

	public virtual void Update () {
		while (Execute.Count > 0)
			Execute.Dequeue().Invoke();
	}
}
