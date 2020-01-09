﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Vehicles.Car;

public class GameController : MonoBehaviour
{
	[SerializeField] private GameObject m_player = null;
	[SerializeField] private HUDController m_hud = null;
	[SerializeField] private GameObject m_pauseMenu;
	private CarInfo[] m_cars;
	private bool m_pause = true;
	private bool m_canUnpause = false;
	private bool m_finished = false;

	public bool Pause { get => m_pause; set => m_pause = value; }
	public bool Unpause { get => m_canUnpause; set => m_canUnpause = value; }
	public bool Finished { get => m_finished; set => m_finished = value; }

	void Awake()
	{
		GameObject AIContainer = GameObject.Find("AI Container");
		int i;
		int nbAI = (RaceParameters.AI < AIContainer.transform.childCount) ? RaceParameters.AI : AIContainer.transform.childCount;
		m_cars = new CarInfo[nbAI + 1];
		CarInfo carInfo = m_player.GetComponent<CarInfo>();
		carInfo.Name = "Player";
		m_cars[0] = carInfo;
		
		// Activates the AI that were indicated by the player
		for (i = 0; i < nbAI; ++i)
		{
			GameObject ai = AIContainer.transform.GetChild(i).gameObject;
			ai.SetActive(true);
			CarInfo ccarInfo = ai.transform.GetChild(0).GetComponent<CarInfo>();
			ccarInfo.Name = "CPU#" + (i + 1);
			m_cars[i + 1] = ccarInfo;
		}

		// Desactivates other AI
		for (; i < AIContainer.transform.childCount; ++i)
		{
			GameObject ai = AIContainer.transform.GetChild(i).gameObject;
			ai.SetActive(false);
		}
	}

	private int CarsComparison(CarInfo c1, CarInfo c2)
	{
		if (c1.Laps > c2.Laps)
		{
			return -1;
		}
		if (c2.Laps > c1.Laps)
		{
			return 1;
		}
		if (c1.Checkpoints > c2.Checkpoints)
		{
			return -1;
		}
		if (c2.Checkpoints > c1.Checkpoints)
		{
			return 1;
		}
		float d2 = c1.DistanceNextCP();
		float d1 = c2.DistanceNextCP();
		if (d1 > d2)
		{
			return -1;
		}
		if (d2 > d1)
		{
			return 1;
		}
		return 0;
	}

	void Update()
	{
		if (!m_pause && !m_finished)
		{
			Array.Sort(m_cars, CarsComparison);
			for (int i = 0; i < m_cars.Length; ++i)
			{
				if (!m_cars[i].IA)
				{
					m_hud.UpdatePosition(i + 1);
					break;
				}
			}
		}
	}

	public void PauseGame()
	{
		if (m_canUnpause)
		{
			m_pause = !m_pause;
			m_pauseMenu.SetActive(m_pause);
			Time.timeScale = (m_pause == true) ? 0 : 1;
		}
	}

	public void Restart()
	{
		SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
	}

	public void Finish()
	{
		m_finished = true;
		m_hud.EndRace(m_cars);
		m_player.GetComponent<CarAIControl>().enabled = true;
		m_player.GetComponent<CarUserControl>().enabled = false;
	}

	public void LoadMenu()
	{
		SceneManager.LoadSceneAsync("Main Screen");
	}

	public void ExitGame()
	{
		Application.Quit();
	}
}
