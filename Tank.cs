using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
        
[System.Serializable]
public class TankProperty       //To be able to make Property Drawers (cool "Progress Bars" showing tank stats) we make each tank property a class member, so that this member gets modified inspector field
{
    public float value; //Every tank property has just it's value
}

public class Tank : MonoBehaviour
{
    public TankProperty acceleration;
    public TankProperty topSpeed;
    public TankProperty firePower;  //All the properties as TankProperty
    public TankProperty armor;
    public TankProperty ballHandling;

    public float Acceleration => acceleration.value;
    public float TopSpeed => topSpeed.value;
    public float FirePower => firePower.value;          //This is how they get used in the other code via 'get' properties
    public float Armor => armor.value;
    public float BallHandling => ballHandling.value;
    
    public int RocketCount;                         //The amount of rockets tank can shoot at a time

    public Transform[] RocketSpawnPoints;   //Transform points of rocket and laser spawn points, on them the weapons spawn when shot
    public Transform[] LaserSpawnPoints;
}

[CustomPropertyDrawer(typeof(TankProperty))]
public class TankPropertyDrawer : PropertyDrawer    
{   
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        SerializedProperty value = prop.FindPropertyRelative("value");
        
        float barWidth;
        if (pos.width < 340)    //So this kinda makes the ProgressBar shot on the right of the text field and those numbers make it scale appropriately with inspector window width
        {
            barWidth = 200 - (355 - pos.width);
        }        
        else
        {
            barWidth = 200 - (370 - pos.width) * 0.5f;
        }

        Rect fieldRect = new Rect(pos.x, pos.y, pos.width - barWidth - 3, pos.height);
        EditorGUI.PropertyField(fieldRect, value, label);                               //This is the text field where tank property value is input

        Rect barRect = new Rect(pos.width - barWidth + 14, pos.y, barWidth, pos.height);
        EditorGUI.ProgressBar(barRect, value.floatValue / 100.0f, "");                  //This is the progress bar on the right of it
        
    }
}




////OLD CLASS:

//public class Tank : MonoBehaviour
//{
//    public int Acceleration;
//    public int TopSpeed;
//    public int FirePower;
//    public int Armor;
//    public int BallHandling;

//    public int RocketCount;

//    public Transform[] RocketSpawnPoints;
//    public Transform[] LaserSpawnPoints;
//}
