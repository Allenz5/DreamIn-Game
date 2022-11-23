using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


/// <summary>
/// GameData's new Format
/// </summary>
public class ReceiveData
{
    public string status;
    public int code;
    public string game_desc;
    public string game_doc;
}
public class GameData
{
    public string _id;
    public string name;
    public string players_num;

    public List<GameMap> map;
    public List<GameCharacter> character;
}

public class GameMap
{
    public string title;
    public string duration;
    public string end;
    public string question;
    public List<string> answers;
    public string background;
    public string collide_map;
    public List<PlacedObject> map_object; //The average number is 150, but most object messages are empty strings

    public Texture2D mapTexture;
}

public class PlacedObject
{
    public string image_link;
    public string position;
    public string message;

    public Texture2D objTexture;

    public Vector3 GetPosition()
    {
        if (position == null)
        {
            Debug.LogError("The object has no position information!");
            return new Vector3(0, 0, 0);
        }
        string str = position.Substring(1, position.Length - 2);

        string[] pos = str.Split(',');
        Vector3 ret = new Vector3(Convert.ToSingle(pos[0]), Convert.ToSingle(pos[1]), Convert.ToSingle(pos[2]));
        return ret;
    }
}

public class GameCharacter
{
    public string name;
    public string background;

    public Texture2D characterTexture;
}
