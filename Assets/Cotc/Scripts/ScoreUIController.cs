using UnityEngine;
using UnityEngine.UI;

public class ScoreUIController : MonoBehaviour
{
    [SerializeField] private Text m_Rank = null;
    [SerializeField] private Text m_PlayerName = null;
    [SerializeField] private Text m_Score = null;

    private string LongToTimer(long miliseconds)
    {
        long minutes = miliseconds / 60000;
        miliseconds -= minutes * 60000;
        long seconds = miliseconds / 1000;
        miliseconds -= seconds * 1000;

        return minutes + ":" + seconds + ":" + miliseconds;
    }

    public void SetValues(int rank, string playerName, long miliseconds)
    {
        m_Rank.text = rank.ToString();
        m_PlayerName.text = playerName;
        m_Score.text = LongToTimer(miliseconds);
    }
}
