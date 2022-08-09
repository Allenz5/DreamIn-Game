using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine;
using TMPro;

/// <summary>
/// Add in Player to controll animations, move, data sync
/// </summary>
public class PlayerScript : MonoBehaviour
{
    public TMP_Text nameText;
    public GameObject nameTextObj;

    internal string playerName;
    internal bool canMove=false;
    private int playerIndex;
    private GameData gameData;

    private Rigidbody2D body;
    private float runSpeed = 200.0f;

    private Animator animator;
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        //Intiate playername Panel and let it follow the player
        GameObject t = GameObject.Instantiate(nameTextObj, transform.position + new Vector3(0, 40, 0), Quaternion.identity);
        GameObject canvas = GameObject.Find("GameCanvas");
        t.transform.SetParent(canvas.transform);
        t.transform.localScale = new Vector3(1, 1, 1);
        t.GetComponent<TextFollow>().SetTarget(gameObject);
        nameText = t.GetComponent<TMP_Text>();
    }
    /// <summary>
    /// player aniamtions control
    /// </summary>
    private void FixedUpdate()
    {
        if (!canMove) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        h *= Mathf.Sqrt(1 - (v * v) / 2.0f);
        v *= Mathf.Sqrt(1 - (h * h) / 2.0f);

        Vector2 dir = new Vector2(h, v);
        body.velocity = dir * runSpeed;

        if (v > 0)
        {
            animator.SetBool("up", true);
            animator.SetBool("down", false);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
        }
        else if (v < 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", true);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
        }
        if (h > 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("left", false);
            animator.SetBool("right", true);
        }
        else if (h < 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("left", true);
            animator.SetBool("right", false);
        }

        if (h == 0 && v == 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
        }
    }
    /// <summary>
    /// Configure the player according to game data
    /// </summary>
    /// <param name="characters"></param>
    /// <param name="index"></param>
    public void SetPlayerData(int index)
    {
        gameData = GameObject.Find("GameManager").GetComponent<GameManager>().gameData;

        playerIndex = index;
        SetPlayerName(gameData.character[playerIndex].name);
    }

    void SetPlayerName(string name)
    {
        playerName = name;
        nameText.text = name;
    }
    void SetPlayerTag(string tag)
    {
        gameObject.tag = tag;
    }

    public string GetPlayerName()
    {
        return playerName;
    }
    public string GetPlayerInfo()
    {
        return gameData.character[playerIndex].background;
    }
}
