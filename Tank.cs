using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
        
[System.Serializable]
public class TankProperty       //COMM EVERYTHING
{
    public float value;
}

public class Tank : MonoBehaviour
{
    public TankProperty acceleration;
    public TankProperty topSpeed;
    public TankProperty firePower;
    public TankProperty armor;
    public TankProperty ballHandling;

    public float Acceleration => acceleration.value;
    public float TopSpeed => topSpeed.value;
    public float FirePower => firePower.value;
    public float Armor => armor.value;
    public float BallHandling => ballHandling.value;
    
    public int RocketCount;

    public Transform[] RocketSpawnPoints;
    public Transform[] LaserSpawnPoints;
}

[CustomPropertyDrawer(typeof(TankProperty))]
public class TankPropertyDrawer : PropertyDrawer
{
    
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        SerializedProperty value = prop.FindPropertyRelative("value");
        
        float barWidth;
        if (pos.width < 340)
        {
            barWidth = 200 - (355 - pos.width);
        }        
        else
        {
            barWidth = 200 - (370 - pos.width) * 0.5f;
        }

        Rect fieldRect = new Rect(pos.x, pos.y, pos.width - barWidth - 3, pos.height);
        EditorGUI.PropertyField(fieldRect, value, label);

        Rect barRect = new Rect(pos.width - barWidth + 14, pos.y, barWidth, pos.height);
        EditorGUI.ProgressBar(barRect, value.floatValue / 100.0f, "");
        
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
