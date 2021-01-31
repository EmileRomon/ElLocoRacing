using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public struct Track
{
	private string name;
	private string path;
}

public class MenuController : MonoBehaviour
{
	public static readonly int[] lapsValues = new int[] {1,2,3,5,-1};
	private int[] modesValues = new int[] {0,1,2,3,4,5,6,7,8};
	public static readonly string[] laps = new string[] {"1 lap", "2 laps", "3 laps", "5 laps", "Infinite"};
	private List<string> tracks = new List<string>();
	public static readonly string[] modes = new string[] { "Time Attack", "1 CPU", "2 CPU", "3 CPU", "4 CPU", "5 CPU", "6 CPU", "7 CPU", "8 CPU"};
	private int tracksIndex = 0;
	private int modesIndex = 0;
	private int lapsIndex = 0;
	[SerializeField] private TextMeshProUGUI tracksText = null;
	[SerializeField] private TextMeshProUGUI modesText = null;
	[SerializeField] private TextMeshProUGUI lapsText = null;

	private void Awake()
	{
		//need to retrieve all existing tracks in a list of tracks
		tracks.Add("Limousin");
		tracks.Add("Savoie");
		if (tracksText != null)
		{
			tracksText.text = tracks[0];
		}
	}

	public void OnDisplayRace()
	{
		SceneManager.LoadSceneAsync("Track Selection");
	}

	public void OnDisplayOptions()
	{
		Debug.Log("options");
	}

	public void OnQuitGame()
	{
		Application.Quit();
	}

	public void OnLeftTrack()
	{
		tracksIndex = (tracksIndex - 1 + tracks.Count) % tracks.Count;
		tracksText.text = tracks[tracksIndex];
	}

	public void OnRightTrack()
	{
		tracksIndex = (tracksIndex + 1 + tracks.Count) % tracks.Count;
		tracksText.text = tracks[tracksIndex];
	}

	public void OnLeftMode()
	{
		modesIndex = (modesIndex - 1 + modes.Length) % modes.Length;
		modesText.text = modes[modesIndex];
	}

	public void OnRightMode()
	{
		modesIndex = (modesIndex + 1 + modes.Length) % modes.Length;
		modesText.text = modes[modesIndex];
	}

	public void OnLeftLaps()
	{
		lapsIndex = (lapsIndex - 1 + laps.Length) % laps.Length;
		lapsText.text = laps[lapsIndex];
	}

	public void OnRightLaps()
	{
		lapsIndex = (lapsIndex + 1 + laps.Length) % laps.Length;
		lapsText.text = laps[lapsIndex];
	}

	public void OnStartRace()
	{
		RaceParameters.AI = modesValues[modesIndex];
		RaceParameters.nbLaps = lapsValues[lapsIndex];
		SceneManager.LoadSceneAsync(tracks[tracksIndex]);
	}

	public void OnBack()
	{
		SceneManager.LoadSceneAsync("Main Screen");
	}
}
