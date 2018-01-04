using UnityEngine;
using UnityEngine.UI;

public class GameStats : MonoBehaviour
{
    //GameStats script is attached to GameStats prefab panel that gets instantiated into GameUI when the game ends
    //Script has all the fillable fields of GameStats window

    [SerializeField] private Text playerOneName;
    [SerializeField] private Text playerTwoName;   
    [SerializeField] private Text gameResult;       

    //Player One Fields

    [SerializeField] private Text P1Score;
    [SerializeField] private Text P1ShotsOnGoal;
    [SerializeField] private Text P1ShotAccuracy;
    [SerializeField] private Text P1MissileHits;
    [SerializeField] private Text P1MissilesFired;
    [SerializeField] private Text P1MissileAccuracy;
    [SerializeField] private Text P1Kills;
    [SerializeField] private Text P1Fumbles;
    [SerializeField] private Text P1Interceptions;
    [SerializeField] private Text P1Possession;
    [SerializeField] private Text P1GameTime;

    //Player Two Fields

    [SerializeField] private Text P2Score;
    [SerializeField] private Text P2ShotsOnGoal;
    [SerializeField] private Text P2ShotAccuracy;
    [SerializeField] private Text P2MissileHits;
    [SerializeField] private Text P2MissilesFired;
    [SerializeField] private Text P2MissileAccuracy;
    [SerializeField] private Text P2Kills;
    [SerializeField] private Text P2Fumbles;
    [SerializeField] private Text P2Interceptions;
    [SerializeField] private Text P2Possession;
    [SerializeField] private Text P2GameTime;

    //===================

    private PlayerStats P1Stats;    //Shorter reference
    private PlayerStats P2Stats;

    public string gameResultString;

    void Awake()
    {
        P1Stats = GameController.Controller.PlayerOne.playerStats;
        P2Stats = GameController.Controller.PlayerTwo.playerStats;

        playerOneName.text = GameController.Controller.PlayerOne.PlayerName;    //Set player names when GameStats get instantiated
        playerTwoName.text = GameController.Controller.PlayerTwo.PlayerName;
    }

    void Start()
    {       
        gameResult.text = gameResultString;

        //Filling all fields mostly from PlayerStats

        //Player One Stats
        P1Score.text = P1Stats.Score.ToString();
        P1ShotsOnGoal.text = P1Stats.ShotsOnGoal.ToString();
        P1ShotAccuracy.text = P1Stats.ShotsOnGoal == 0 ? $"{0:P1}" : $"{(float)P1Stats.Score / P1Stats.ShotsOnGoal:P1}";    //If there was no shots on goal, set 0%, so we don't divide by 0
        P1MissileHits.text = P1Stats.MissilesHit.ToString();
        P1MissilesFired.text = P1Stats.MissilesFired.ToString();
        P1MissileAccuracy.text = P1Stats.MissilesFired == 0 ? $"{0:P1}" : $"{(float)P1Stats.MissilesHit / P1Stats.MissilesFired:P1}";   //Same with no missiles fired
        P1Kills.text = P2Stats.Deaths.ToString();   //Getting the value from Player Two
        P1Fumbles.text = P1Stats.Fumbles.ToString();
        P1Interceptions.text = P1Stats.Interceptions.ToString();
        int P1PossessionTime = (int) Mathf.Round(P1Stats.PossessionTime);   //Round float value and convert to int. Because "D2" formatter doesn't accept float
        P1Possession.text = $"{P1PossessionTime / 60:D2}:{P1PossessionTime % 60:D2}";   //Format the possession time in seconds to ##:##

        //Player Two Stats
        P2Score.text = P2Stats.Score.ToString();
        P2ShotsOnGoal.text = P2Stats.ShotsOnGoal.ToString();
        P2ShotAccuracy.text = P2Stats.ShotsOnGoal == 0 ? $"{0:P1}" : $"{(float)P2Stats.Score / P2Stats.ShotsOnGoal:P1}";
        P2MissileHits.text = P2Stats.MissilesHit.ToString();
        P2MissilesFired.text = P2Stats.MissilesFired.ToString();
        P2MissileAccuracy.text = P2Stats.MissilesFired == 0 ? $"{0:P1}" : $"{(float)P2Stats.MissilesHit / P2Stats.MissilesFired:P1}";
        P2Kills.text = P1Stats.Deaths.ToString();   //Getting from Player One
        P2Fumbles.text = P2Stats.Fumbles.ToString();
        P2Interceptions.text = P2Stats.Interceptions.ToString();
        int P2PossessionTime = (int)Mathf.Round(P2Stats.PossessionTime);
        P2Possession.text = $"{P2PossessionTime / 60:D2}:{P2PossessionTime % 60:D2}";

        //Get total play time
        int totalTime = GameController.Controller.TotalGameTime;
        P1GameTime.text = $"{totalTime / 60:D2}:{totalTime % 60:D2}"; //Format it to ##:## and output to both player fields
        P2GameTime.text = $"{totalTime / 60:D2}:{totalTime % 60:D2}";

    }

    //public void SetGameResult(string cause)
    //{
    //    gameResultString
    //}










}
