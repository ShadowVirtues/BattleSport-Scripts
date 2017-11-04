using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.UI;



/*
    ==================GLOBAL CONCEPTS========================
    EVERYTHING PLAYER-PLAYER RELATED HAS TO REVOLVE AROUND WHICH PLAYER NUMBER IS SET UP TO EACH ONE ON TOP OF HIERARCHY IN THIS SCRIPT

    Ideally i should make ONE SINGLE "PlayerX" prefab with everything setup, and then when instantiating, just set its parameters for different players.
    ==========================================================

    Next thing to do: come up with damage system with firepower, armor which affects mass.



    Try to fix this bullshit when the tank moves by itself





    Consider that we have default Physic Material and there needs to be ball bouncing out of everything     
    

    Adjust thrusters lifetime so you don't see it while moving backwards. MAYBE MAKE CULLING SHIT TO CAMERA, SO WITH INSANE FOVS PLAYERS COULDNT SEE THEIR OWN TANK
         
    When putting a chosen tank in a scene's container "PlayerOne/Two", set the layer of the tank object to respective one.






    ADDITIONAL IDEAS:
    When hit, slider slowly going down with red trail like in LOL






*/






public class Player : MonoBehaviour
{
    public PlayerID PlayerNumber; //From TeamUtility InputManager "Add-on". Sets which player it is



    public float Health = 100;

    

    [SerializeField] private Slider healthSlider;








    public void Hit(float damage)
    {
        Health -= damage;

        

        

        if (Health < 0)
        {
            //TODO Destroy tank

        }
        else
        {
            healthSlider.value = Health;
        }
        

    }

}
