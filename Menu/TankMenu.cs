using UnityEngine;

public class TankMenu : MonoBehaviour
{
    //This script is attached to the UI Tank prefab that is rotating in the tank selection menus

	void Update ()
	{
	    Spin(); //Spinning it all the time it shows in selector
	}

    void Spin()
    {
        transform.Rotate(Vector3.up, 60 * Time.deltaTime);
    }

    //void OnDisable()
    //{
    //    transform.localRotation = Quaternion.identity;  //When you select another tank, this tank gets disabled, so set his rotating back so it doesn't get saved each time you deselect this tank
    //}


}
