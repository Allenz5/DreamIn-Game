using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;

public class playerScript : MonoBehaviourPun
{
    public Text nameText;
    internal string playerName;

    private List<GameCharacter> gameCharacters;
    private int playerIndex;

    Rigidbody2D body;
    public float runSpeed = 20.0f;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        playerName = "Default Player Name";
        SetPlayerName(playerName);
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(h, v);
        dir *= runSpeed;

        body.velocity = dir;
    }
    /// <summary>
    /// ������Ϣ����player
    /// </summary>
    /// <param name="characters"></param>
    /// <param name="index"></param>
    public void SetPlayerInfo(List<GameCharacter> characters, int index)
    {
        gameCharacters = characters;
        playerIndex = index;
        SetPlayerName(gameCharacters[playerIndex].name);

        //TODO::����������ͼ
    }
    public void SetPlayerName(string name)
    {
        photonView.RPC("RPCSetPlayerName", RpcTarget.All, name);
    }
    [PunRPC]
    void RPCSetPlayerName(string name)
    {
        playerName = name;
        nameText.text = name;
    }


}
