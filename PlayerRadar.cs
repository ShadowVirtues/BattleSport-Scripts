using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;

public class PlayerRadar : MonoBehaviour
{
    [SerializeField] private RectTransform ballRadar;
    [SerializeField] private RectTransform goalRadar;
    [SerializeField] private RectTransform enemyRadar;   
    private Transform player;

    private Transform enemy;
    private Transform ball;
    private Transform goal;

    private float arenaDimension;
    private float radarDimension = 120;

    public static bool ballPossession;

    public static void HideBallFromRadars(bool state)
    {
        GameController.Controller.PlayerOne.playerRadar.ballRadar.gameObject.SetActive(!state);
        GameController.Controller.PlayerTwo.playerRadar.ballRadar.gameObject.SetActive(!state);
    }

    void Awake ()
    {
        Player playerPlayer = GetComponent<Player>();

        player = GetComponent<Transform>();

        if (playerPlayer.PlayerNumber == PlayerID.One)
        {
            enemy = GameController.Controller.PlayerTwo.transform;
        }
        else if (playerPlayer.PlayerNumber == PlayerID.Two)
        {
            enemy = GameController.Controller.PlayerOne.transform;
        }

        ball = GameController.Controller.ball.transform;
        goal = GameController.Controller.goal.transform;

    }

    void Start()
    {
        arenaDimension = GameController.Controller.arenaSize;
    }


    //RectTransform.AnchoredX/Y = 120 on the very edge of the radar


    private Vector2 RadarPosition(Transform item)
    {
        float ballX = (item.position.x - player.position.x) / arenaDimension * radarDimension;
        float ballZ = (item.position.z - player.position.z) / arenaDimension * radarDimension;

        Vector3 relative = player.TransformDirection(ballX, 0, -ballZ);

        return new Vector2(relative.x, -relative.z);
    }

	void Update ()
	{        
	    if (ballPossession == false)
	    {
	        ballRadar.anchoredPosition = RadarPosition(ball);
        }	    
	    goalRadar.anchoredPosition = RadarPosition(goal);
	    enemyRadar.anchoredPosition = RadarPosition(enemy);
    }
}
