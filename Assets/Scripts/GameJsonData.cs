using System.Collections.Generic;
using UnityEngine;

public class GameJsonData
{
    public string status;
    public ReceiveResult result;
}
public class ReceiveResult
{
    public int id;
    public GameData info;
}

public class GameData
{
    public string name;
    public string end;
    public int length;
    public List<Character> character; //���11��
    public List<GameMap> Map;
}

public class Character
{
    public string name;
    public int identity;
    public string background;
}

public class GameMap
{
    public string background;
    public string collide_map;
    public List<PlacedObject> Map_Object; //ƽ����150�������󲿷�object��messageΪ���ַ���
}

public class PlacedObject
{
    public string image_link;
    public string message;
    public string position;

    public Transform transform;

    public void GetTransformPosition()
    {
        if (position == null) return;
        string pos = position.Substring(1, position.Length - 2);
            
    }
}
