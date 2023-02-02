using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class NewNetworkLauncher : MonoBehaviourPunCallbacks
{
    public string roomName;
    public string region;

    public void GetRoomName(string r)
    {
        roomName = r;
    }

    public void GetRegion(string r)
    {
        region = r;
    }


    void Start()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = region;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 10 };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, default);
    }

    public override void OnJoinedRoom()
    {
        //start
    }
}
