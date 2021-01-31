using System;
using System.IO;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    [Serializable]
    public class Timer
    {
        private float milliseconds = 0;
        private float minutes = 0;
        private float seconds = 0;

        [SerializeField] private GameObject minutesUI = null;
        [SerializeField] private GameObject secondsUI = null;
        [SerializeField] private GameObject millisecondsUI = null;
        private TextMeshProUGUI minutesText;
        private TextMeshProUGUI secondsText;
        private TextMeshProUGUI millisecondsText;

        public float Minutes { get => minutes; set => minutes = value; }
        public float Seconds { get => seconds; set => seconds = value; }
        public float Milliseconds { get => milliseconds; set => milliseconds = value; }

        public void Start()
        {
            minutesText = minutesUI.GetComponent<TextMeshProUGUI>();
            secondsText = secondsUI.GetComponent<TextMeshProUGUI>();
            millisecondsText = millisecondsUI.GetComponent<TextMeshProUGUI>();
        }

        public void Update(bool ui)
        {
            milliseconds += Time.deltaTime * 100;
            if (milliseconds > 99)
            {
                milliseconds = 0;
                ++seconds;
                if (seconds >= 60)
                {
                    seconds = 0;
                    ++minutes;
                    if (ui)
                    {
                        minutesText.text = (minutes >= 10) ? minutes.ToString("F0") + ':' : '0' + minutes.ToString("F0") + ':';
                    }
                }
                if (ui)
                {
                    secondsText.text = (seconds >= 10) ? seconds.ToString("F0") + '.' : '0' + seconds.ToString("F0") + '.';
                }
            }
            if (ui)
            {
                millisecondsText.text = (milliseconds > 9) ? milliseconds.ToString("F0") : '0' + milliseconds.ToString("F0");
            }
        }

        public float Value()
        {
            return minutes * 60000 + seconds * 1000 + milliseconds;
        }

        public void Reset()
        {
            minutes = seconds = milliseconds = 0;
            minutesUI.GetComponent<TextMeshProUGUI>().text = "00:";
            secondsUI.GetComponent<TextMeshProUGUI>().text = "00.";
            millisecondsUI.GetComponent<TextMeshProUGUI>().text = "00";
        }

        public override string ToString()
        {
            minutesText.text = (minutes >= 10) ? minutes.ToString("F0") + ':' : '0' + minutes.ToString("F0") + ':';
            secondsText.text = (seconds >= 10) ? seconds.ToString("F0") + '.' : '0' + seconds.ToString("F0") + '.';
            millisecondsText.text = (milliseconds > 9) ? milliseconds.ToString("F0") : '0' + milliseconds.ToString("F0");
            return minutesText.text + secondsText.text + millisecondsText.text;
        }
    }



    [SerializeField] private GameObject m_nbLapsUI = null;
    [SerializeField] private GameObject m_currentLapUI = null;
    [SerializeField] private GameObject m_nbPositionsUI = null;
    [SerializeField] private GameObject m_currentPositionUI = null;
    [SerializeField] private Timer m_raceTimer = null;
    [SerializeField] private Timer m_lapTimer = null;
    [SerializeField] private GameObject player = null;
    [SerializeField] private GameObject m_endInfo = null;
    [SerializeField] private GameObject m_finishText = null;
    [SerializeField] private TextMeshProUGUI m_bestLapMin = null;
    [SerializeField] private TextMeshProUGUI m_bestLapSec = null;
    [SerializeField] private TextMeshProUGUI m_bestLapMil = null;
    private TextMeshProUGUI m_currentLapText;
    private TextMeshProUGUI m_currentPositionText;
    private int m_nbLaps;
    private CarInfo m_playerInfo;
    private GameController m_gameController;
    private float m_bestValue = 0;

    private void Awake()
    {
        m_gameController = GameObject.Find("Controller").GetComponent<GameController>();
    }

    private void Start()
    {
        m_raceTimer.Start();
        m_lapTimer.Start();
        m_nbLaps = RaceParameters.nbLaps;
        m_playerInfo = player.GetComponent<CarInfo>();
        TextMeshProUGUI lapsText = m_nbLapsUI.GetComponent<TextMeshProUGUI>();
        if (m_nbLaps > 10)
        {
            lapsText.text = '/' + m_nbLaps.ToString();
        }
        else if (m_nbLaps > 0)
        {
            lapsText.text = "/0" + m_nbLaps.ToString();
        }
        else
        {
            lapsText.text = "";
        }
        m_currentLapText = m_currentLapUI.GetComponent<TextMeshProUGUI>();
        m_currentLapText.text = '0' + m_playerInfo.Laps.ToString();

        TextMeshProUGUI positionsText = m_nbPositionsUI.GetComponent<TextMeshProUGUI>();
        if (RaceParameters.AI == 1)
        {
            positionsText.text = "";
        }
        else
        {
            positionsText.text = "/0" + (RaceParameters.AI + 1);
        }
        m_currentPositionText = m_currentPositionUI.GetComponent<TextMeshProUGUI>();
        m_currentPositionText.text = "01";


        if (Application.isEditor && !Application.isPlaying)
        {
            string path = "Assets/Resources/" + SceneManager.GetActiveScene().name + "_best.txt";
            using (BinaryReader file = new BinaryReader(File.Open(path, FileMode.OpenOrCreate)))
            {
                string line;
                if (file.PeekChar() != -1)
                {
                    line = file.ReadString();
                    if (line.Length > 0)
                    {
                        float mil = float.Parse(line);
                        m_bestValue = mil;
                        float min = Mathf.Floor((mil / 60000));
                        mil -= min * 60000;
                        float sec = Mathf.Floor((mil / 1000));
                        mil -= sec * 1000;
                        mil = Mathf.Floor(mil) % 100;
                        m_bestLapMin.text = (min > 9) ? min.ToString("F0") + ':' : '0' + min.ToString("F0");
                        m_bestLapSec.text = (sec > 9) ? sec.ToString("F0") + '.' : '0' + sec.ToString("F0");
                        m_bestLapMil.text = (mil > 9) ? mil.ToString("F0") : '0' + mil.ToString("F0");
                    }
                }
                else
                {
                    m_bestLapMin.text = "00:";
                    m_bestLapSec.text = "00.";
                    m_bestLapMil.text = "00";
                }
            }
        }
    }

    void Update()
    {
        if (!m_gameController.Pause)
        {
            m_raceTimer.Update(!m_gameController.Finished);
            m_lapTimer.Update(!m_gameController.Finished);
        }
    }

    public void UpdatePosition(int position)
    {
        if (m_currentPositionText != null)
        {
            m_currentPositionText.text = '0' + position.ToString();
        }
    }

    public void UpdateLaps(CarInfo carInfo, bool player)
    {
        carInfo.IncreaseLaps();
        if (carInfo.Laps > m_nbLaps)
        {
            carInfo.Time = m_raceTimer.ToString();
            if (player)
            {
                m_gameController.Finish();
            }
        }
        else
        {
            if (player)
            {
                m_currentLapText.text = (carInfo.Laps >= 10) ? carInfo.Laps.ToString() : '0' + carInfo.Laps.ToString();
                m_lapTimer.Reset();
            }
        }
    }

    private void DisplayEndScreen(CarInfo[] cars)
    {
        m_finishText.SetActive(false);
        m_endInfo.transform.parent.gameObject.SetActive(true);
        for (int i = 0; i < cars.Length; ++i)
        {
            GameObject line = m_endInfo.transform.GetChild(i).gameObject;
            line.SetActive(true);
            line.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = cars[i].Name;
            if (cars[i].Time == "")
            {
                cars[i].PredictTime(m_nbLaps, (int)m_raceTimer.Minutes, (int)m_raceTimer.Seconds, (int)m_raceTimer.Milliseconds);
            }
            line.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = cars[i].Time;
        }
        this.gameObject.SetActive(false);
    }

    IEnumerator WaitEndScreen(CarInfo[] cars)
    {
        yield return new WaitForSeconds(4);
        DisplayEndScreen(cars);
    }

    public void SaveBest(float best)
    {
        string path = "Assets/Resources/" + SceneManager.GetActiveScene().name + "_best.txt";
        using (BinaryWriter file = new BinaryWriter(File.Open(path, FileMode.Open)))
        {
            file.Write(best.ToString());
        }
    }

    public void EndRace(CarInfo[] cars)
    {
        m_finishText.SetActive(true);
        m_playerInfo.IA = true;
        float totalTime = m_raceTimer.Value();
        if (Application.isEditor && !Application.isPlaying)
        {
            if (totalTime > m_bestValue)
            {
                SaveBest(totalTime);
            }
        }
        StartCoroutine(WaitEndScreen(cars));
    }

    public long GetRaceTimeValue()
    {
        return Convert.ToInt64(m_raceTimer.Value());
    }

}
