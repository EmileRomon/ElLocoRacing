﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Radio : MonoBehaviour
{
	private AudioSource m_source;
	private List<AudioClip> m_clips = new List<AudioClip>();
	private GameController m_gameController;
	private bool m_pause = false;

	private void Awake()
	{
		AudioSource source = gameObject.AddComponent<AudioSource>();
		m_source = source;
		string[] files = Directory.GetFiles("Assets/Resources/Radio/");
		foreach(string f in files)
		{
			if(f.EndsWith(".mp3")|| f.EndsWith(".wav")||f.EndsWith(".ogg"))
			{
				string[] split = f.Split('/');
				AudioClip audioClip = Resources.Load<AudioClip>("Radio/" + split[split.Length - 1].Split('.')[0]);
				m_clips.Add(audioClip);
			}
		}
		m_gameController = GameObject.Find("Controller").GetComponent<GameController>();
	}

	private void Start()
	{
		RandomMusic();
	}

	private void Update()
	{
		if (m_gameController.Pause)
		{
			m_pause = true;
			m_source.Pause();
		}
		else
		{
			if (!m_source.isPlaying)
			{
				if(m_pause)
				{
					m_source.Play();
					m_pause = false;
				}
				else
				{
					RandomMusic();
				}
			}
		}
	}

	private void RandomMusic()
	{
		m_source.clip = m_clips[Random.Range(0, m_clips.Count)];
		m_source.Play();
	}
}
