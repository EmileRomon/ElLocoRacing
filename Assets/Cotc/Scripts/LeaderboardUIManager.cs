using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardUIManager : MonoBehaviour
{
    #region XtralifeLeaderboard

    public CustomLoginManager customLoginManager = null;

    [SerializeField] private GameObject LeaderboardUI = null;

    [SerializeField] private GameObject m_BoardElmPrefab = null;

    [SerializeField] private HUDController m_HUDController = null;

    [SerializeField] private Text m_LeaderboardTitle = null;

    private void Awake()
    {
        customLoginManager = FindObjectOfType<CustomLoginManager>();
    }

    public void AddScore()
    {
        string boardName = SceneManager.GetActiveScene().name /*+ " " + MenuController.modes[RaceParameters.AI] + " " + RaceParameters.nbLaps + " lap(s)"*/;
        m_LeaderboardTitle.text = "Leaderboard - " + boardName;
        customLoginManager.AddToLeaderboard(m_HUDController.GetRaceTimeValue(), boardName);
        customLoginManager.BestHighScores(boardName);
    }

    public void DisplayLeaderboard(CotcSdk.PagedList<CotcSdk.Score> bestHighScoresRes)
    {
        LeaderboardUI.SetActive(true);
        foreach (var score in bestHighScoresRes)
        {
            GameObject LeaderboardElm = Instantiate(m_BoardElmPrefab, LeaderboardUI.transform);
            ScoreUIController scoreUI = LeaderboardElm.GetComponent<ScoreUIController>();
            scoreUI.SetValues(score.Rank, score.GamerInfo["profile"]["displayName"].ToString(), score.Value);
            Debug.Log(score.Rank + ". " + score.GamerInfo["profile"]["displayName"] + ": " + score.Value);
        }
    }

    #endregion
}
