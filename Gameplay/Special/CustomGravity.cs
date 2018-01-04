using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    //[SerializeField] private float gravity;

    //void Awake()
    //{
    //    Physics.gravity = Vector3.down * gravity;
    //}

    //void OnDestroy()
    //{
    //    Physics.gravity = Vector3.down * 20;
    //}

    //void Start()
    //{
    //    GameController.Controller.ball.additionalGravity = Vector3Int.zero;
    //    GameController.Controller.ball.rigidbody.mass = 0.75f;
    //    GameController.Controller.ball.rigidbody.drag = 0;

    //}



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ScreenCapture.CaptureScreenshot("ASDF.png", 2);


        }


    }
}
