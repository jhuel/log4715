﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RaceManager : MonoBehaviour 
{


	[SerializeField]
	private GameObject _carContainer;

	[SerializeField]
	private GUIText _announcement;

	[SerializeField]
	private int _timeToStart;

	[SerializeField]
	private int _endCountdown;

    public List<CarController> cars;
    public CarController[] allCars;
    public CarController[] carOrder { get; set;}

	// Use this for initialization
	void Awake () 
	{
		CarActivation(false);

	}
	
	void Start()
	{
		StartCoroutine(StartCountdown());
        cars = new List<CarController>();
        foreach (CarController car in _carContainer.GetComponentsInChildren<CarController>(true))
        {
            cars.Add(car);
        }
        allCars = cars.ToArray();
        carOrder = new CarController[cars.Count];
	}

    public void Update()
    {
        foreach (CarController car in allCars)
        {
            carOrder[car.GetCarPosition(allCars) - 1] = car; //placer les voitures en ordre
            car.ApplyRubberBand(getRubberBandMult(car.GetCarPosition(allCars) - 1)); //appliquer le rubberbanding
        }

    }

    float getRubberBandMult(int position)
    {
        float milieu = (allCars.Length - 1) / 2 + 1;
        return ((float)position - milieu) / (allCars.Length - 1);

    }

	IEnumerator StartCountdown()
	{
		int count = _timeToStart;
		do 
		{
			_announcement.text = count.ToString();
			yield return new WaitForSeconds(1.0f);
			count--;
		}
		while (count > 0);
		_announcement.text = "Partez!";
		CarActivation(true);
		yield return new WaitForSeconds(1.0f);
		_announcement.text = "";
	}

	public void EndRace(string winner)
	{
		StartCoroutine(EndRaceImpl(winner));
	}

	IEnumerator EndRaceImpl(string winner)
	{
		CarActivation(false);
		_announcement.fontSize = 20;
		int count = _endCountdown;
		do 
		{
			_announcement.text = "Victoire: " + winner + " en premiere place. Retour au titre dans " + count.ToString();
			yield return new WaitForSeconds(1.0f);
			count--;
		}
		while (count > 0);

		Application.LoadLevel("boot");
	}

	public void Announce(string announcement, float duration = 2.0f)
	{
		StartCoroutine(AnnounceImpl(announcement,duration));
	}

	IEnumerator AnnounceImpl(string announcement, float duration)
	{
		_announcement.text = announcement;
		yield return new WaitForSeconds(duration);
		_announcement.text = "";
	}

	public void CarActivation(bool activate)
	{
		foreach (CarAIControl car in _carContainer.GetComponentsInChildren<CarAIControl>(true))
		{
			car.enabled = activate;
		}
		
		foreach (CarUserControlMP car in _carContainer.GetComponentsInChildren<CarUserControlMP>(true))
		{
			car.enabled = activate;
		}

	}
}
