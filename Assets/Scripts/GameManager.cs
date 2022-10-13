using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using LitJson;
using TMPro;
using System.Text;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject readyButton;
    public GameObject watchButton;
    public GameObject startButton;
    public GameObject finishButton;
    public GameObject cluesButton;
    public GameObject gameCanvas;
    public GameObject objects;
    public GameObject colliders;
    public GameObject objectPrefab;
    public GameObject mapPrefab;
    public GameObject colliderPrefab;
    public GameObject questionPanel;
    public GameObject objectInfoPanel;
    public GameObject timer;
    public GameObject currentMap;
    public GameObject cluePanel;
    public GameObject LastTipPanel;

    public TMP_Text PlayerInfoText;
    public TMP_Text PlayerNameText;
    public TMP_Text EndText;
    public TMP_Text TimerText;

    private PhotonView GM_PhotonView;
    public GameData gameData;
    private GameObject localPlayer;

    private int countTime = 0;
    private bool isDownloadCompelete=false;
    private int ColliderSize = 32;
    public int mapIndex = 0;

    public void Start()
    {
        GM_PhotonView = GetComponent<PhotonView>();
        //check whether to join the game midway
        Invoke("TestIfGameStart", 1);
    }

    /// <summary>
    /// check whether to join the game midway
    /// </summary>
    /// 
    void TestIfGameStart()
    {
        GameObject testflag = GameObject.FindGameObjectWithTag("GameStartFlag");
        if (testflag != null)//if so, only watch the game after downloading game data
        {
            readyButton.SetActive(false);
            watchButton.SetActive(true);
            InitializeScene();
        }
        else
        {
            readyButton.SetActive(true);
            watchButton.SetActive(true);
        }
    }
    
    
    [PunRPC]
    void RPCInitializedGame()
    {
        //initailize player panel
        if (localPlayer != null)
        {
            PlayerNameText.text = "Your name is " + localPlayer.GetComponent<PlayerScript>().GetPlayerName();
           // PlayerIdentityText.text = "You are a " + localPlayer.GetComponent<PlayerScript>().GetPlayerIdentity();
            PlayerInfoText.text = localPlayer.GetComponent<PlayerScript>().GetPlayerInfo();
            PlayerInfoText.transform.parent.parent.parent.gameObject.SetActive(true);
        }
        cluesButton.SetActive(true);
    }
    
    public void InitializeScene()
    {
        //set Player
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject it in players)
                it.transform.SetSiblingIndex(it.transform.parent.childCount - 1);

            if (localPlayer != null)
                localPlayer.transform.SetSiblingIndex(localPlayer.transform.parent.childCount - 1);

            GameObject[] watchers = GameObject.FindGameObjectsWithTag("Watcher");
            foreach (GameObject it in watchers)
                it.transform.SetSiblingIndex(it.transform.parent.childCount - 1);
        }

        //Set Map
        {
            mapIndex = 0;
            UpdateMap(mapIndex);
        }
    }

    void UpdateMap(int index)
    {
        //delete previous map and object
        if(currentMap!=null)
        {
            Destroy(currentMap);
            foreach (Transform child in colliders.transform)
                GameObject.Destroy(child.gameObject);

            foreach (Transform child in objects.transform)
                GameObject.Destroy(child.gameObject);
        }

        //update map
        {
            GameObject map = Instantiate(mapPrefab, new Vector2(0, 0), Quaternion.identity, gameCanvas.transform);

            map.transform.SetParent(gameCanvas.transform);
            map.transform.localScale = new Vector3(1, 1, 1);

            float w = gameData.map[index].mapTexture.width;
            float h = gameData.map[index].mapTexture.height;

            map.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            map.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            map.transform.SetSiblingIndex(0);
            map.GetComponent<Image>().sprite = Sprite.Create(gameData.map[index].mapTexture, new Rect(0, 0, w, h), new Vector2(0, 0));

            currentMap = map;
        }

        //update map object
        {
            for (int i = 0; i < gameData.map[index].map_object.Count; i++)
            {
                GameObject obj = Instantiate(objectPrefab, new Vector2(0, 0), Quaternion.identity, objects.transform);

                obj.transform.SetParent(gameCanvas.transform);
                obj.transform.localScale = new Vector3(1, 1, 1);

                float w = gameData.map[index].map_object[i].objTexture.width;
                float h = gameData.map[index].map_object[i].objTexture.height;
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                obj.GetComponent<Image>().sprite = Sprite.Create(gameData.map[index].map_object[i].objTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
                obj.GetComponent<Object>().objectInfoPanel = objectInfoPanel;
                obj.GetComponent<Object>().SetInfo(gameData.map[index].map_object[i].message);
                obj.GetComponent<Object>().GM = this;
                obj.transform.localPosition = gameData.map[index].map_object[i].GetPosition();

                obj.transform.SetParent(objects.transform);
            }
        }

        //update collision
        {
            float w = gameData.map[index].mapTexture.width / 2;
            float h = gameData.map[index].mapTexture.height / 2;
            string[] rows = gameData.map[index].collide_map.Split(';');
            for (int i = 0; i < rows.Length; i++)
            {
                string[] cols = rows[i].Split(',');
                for (int j = 0; j < cols.Length - 1; j++)
                {
                    GameObject obj = Instantiate(colliderPrefab, new Vector2(0, 0), Quaternion.identity, colliders.transform);
                    obj.transform.localPosition = new Vector3(-w + int.Parse(cols[j]) * ColliderSize + ColliderSize / 2, h - i * ColliderSize - ColliderSize / 2, 0);

                    obj.transform.SetParent(colliders.transform);
                }
            }
        }

        //set count time
        countTime = int.Parse(gameData.map[index].duration);
    }

    /// <summary>
    /// Call this method after question part
    /// </summary>
    void LevelCompelete()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GM_PhotonView.RPC("RPCLevelCompelete", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPCLevelCompelete()
    {
        mapIndex++;
        //if this is the last map
        if (mapIndex >= gameData.map.Count)
        {
            //Set and show end text
            EndText.text = gameData.map[mapIndex-1].end;
            EndText.transform.parent.parent.gameObject.SetActive(true);

            mapIndex = -1;
        }
        else
        {
            //Set and show end text
            EndText.text = gameData.map[mapIndex-1].end;
            EndText.transform.parent.parent.gameObject.SetActive(true);

            UpdateMap(mapIndex);
            StartCountTime(countTime);//restart count time

            //reset player's position
            localPlayer.transform.localPosition = Vector2.zero;
        }
    }


    /// <summary>
    /// show inf opanel according to the input obj's info
    /// </summary>
    /// <param name="obj"></param>
    public void ShowInfoPanel(Object obj)
    {
        objectInfoPanel.GetComponent<ObjectPanel>().SetText(obj.objectInfo);

        //check whether the info is in the clue panel
        bool flag = false;
        foreach (string clue in cluePanel.GetComponent<CluePanel>().clueList)
        {
            if (obj.objectInfo == clue)
            {
                flag = true;
                break;
            }
        }
        //if exist, set share button in object panel to non interactive
        if (flag)
        {
            objectInfoPanel.GetComponent<ObjectPanel>().SetSharedButton(false, "Shared");
        }
        else
        {
            objectInfoPanel.GetComponent<ObjectPanel>().SetSharedButton(true,"Share");
        }
        objectInfoPanel.SetActive(true);
    }

    [PunRPC]
    void RPCSetMapIndex(int index)
    {
        mapIndex = index;
    }
    [PunRPC]
    void RPCShowSelectPanel()
    {
        if (localPlayer == null || localPlayer.tag == "Watcher") return;
        questionPanel.SetActive(true);
        questionPanel.GetComponent<QuestionPanel>().SetQuestion(gameData.map[mapIndex].question);
        questionPanel.GetComponent<QuestionPanel>().CreateAnswerItem(gameData.map[mapIndex].answers);
    }

    [PunRPC]
    void SetPlayerMove(bool canMove)
    {
        localPlayer.GetComponent<PlayerScript>().canMove = canMove;
    }

    #region Button
    public void WatchButton()
    {
        string playerName = "Player " + (int)Random.Range(1, 7);
        localPlayer = PhotonNetwork.Instantiate(playerName, gameCanvas.transform.position, Quaternion.identity, 0);
        localPlayer.transform.localPosition = new Vector2(0, 0);
        localPlayer.GetComponent<PlayerScript>().SetPlayerTag("Watcher");
        localPlayer.GetComponent<PlayerScript>().SetPlayerName("Watcher");
        Camera.main.GetComponent<CameraFollow>().SetTarget(localPlayer);

        //now all players can move
        GM_PhotonView.RPC("SetPlayerMove", RpcTarget.All, true);

        if (PhotonNetwork.IsMasterClient)
            startButton.SetActive(true);

        readyButton.SetActive(false);
        watchButton.SetActive(false);
    }
    public void ReadyButton()
    {
        string playerName = "Player " + (int)Random.Range(1, 7);
        localPlayer = PhotonNetwork.Instantiate(playerName, gameCanvas.transform.position, Quaternion.identity, 0);
        localPlayer.transform.localPosition = new Vector2(0, 0);
        Camera.main.GetComponent<CameraFollow>().SetTarget(localPlayer);

        //now all players can move
        GM_PhotonView.RPC("SetPlayerMove", RpcTarget.All, true);

        if (PhotonNetwork.IsMasterClient)
            startButton.SetActive(true);

        readyButton.SetActive(false);
        watchButton.SetActive(false);
    }
    public void StartButton()
    {
        if (gameData == null)
        {
            Debug.Log("game data has not been download!");
            return;
        }
        if(isDownloadCompelete == false)
        {
            Debug.Log("game data downloading");
            return;
        }

        //instantiate players
        List<GameCharacter> characters = new List<GameCharacter>(gameData.character);
        GameObject[] playerObj = GameObject.FindGameObjectsWithTag("Player");
        if (playerObj.Length != characters.Count)
        {
            Debug.LogError("Incorrect Number of Players!");
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            finishButton.SetActive(true);
        }

        startButton.SetActive(false);
        InitializeScene();

        List<int> selectedIndex=new List<int>();
        for(int i=0;i<playerObj.Length;i++)
        {
            int index = Random.Range(0, characters.Count);
            while(selectedIndex.Contains(index))
                index = Random.Range(0, characters.Count);

            selectedIndex.Add(index);
            playerObj[i].GetComponent<PlayerScript>().SetPlayerData(index);
        }

        GM_PhotonView.RPC("RPCInitializedGame", RpcTarget.All);

        //instantiate GameStartFlag
        GameObject flag = PhotonNetwork.Instantiate("GameStartFlag", gameCanvas.transform.position, Quaternion.identity, 0);

        //start count time
        timer.SetActive(true);
        StartCountTime(countTime);
    }
    public void EndLevelButton()
    {
        if (mapIndex == -1)
        {
            Debug.Log("This is the last Level");
            LastTipPanel.SetActive(true);
            return;
        }
        EndCountTime();
    }
    public void ShareButton()
    {
        cluePanel.GetComponent<CluePanel>().AddClue(objectInfoPanel.GetComponent<ObjectPanel>().GetText());
    }
    public void SelectResultButton()
    {
        questionPanel.GetComponent<QuestionPanel>().ResetPanel();
        LevelCompelete();
    }



    #endregion

    #region Download Data
    string GetImageLink(string addr)
    {
        string imageLink = "https://raw.githubusercontent.com/hanxuan5/DreamIn-Assets/master/";
        imageLink += addr;
        imageLink = imageLink.Replace(" ", "%20");
        imageLink += ".png";
        return imageLink;
    }

    public void DownloadData(string ID)
    {
        StartCoroutine(GetGameData(ID));
    }
    IEnumerator GetGameData(string ID)
    {
        string url = "https://api.dreamin.land/get_game_doc/";
        UnityWebRequest webRequest = new UnityWebRequest(url, "POST");

        Encoding encoding = Encoding.UTF8;
        byte[] buffer = encoding.GetBytes("{\"id\":" + ID + "}");
        webRequest.uploadHandler = new UploadHandlerRaw(buffer);
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(webRequest.error + "\n" + webRequest.downloadHandler.text);
        }
        else
        {
            Debug.Log("get game data succcess!");
#if UNITY_EDITOR
            //Save a gamedata backup for debug
            string savePath = "Assets/JsonData/GameData.json";
            File.WriteAllText(savePath, Regex.Unescape(webRequest.downloadHandler.text));
#endif
        }

        //read and store in gameData
        ReceiveData d = JsonMapper.ToObject<ReceiveData>(webRequest.downloadHandler.text);
        gameData = JsonMapper.ToObject<GameData>(d.game_doc);

        for (int i = 0; i < gameData.map.Count; i++)
        {
            string addr = gameData.map[i].background;
            StartCoroutine(GetMapTexture(addr, i));

            for (int j = 0; j < gameData.map[i].map_object.Count; j++)
            {
                string objAddr = gameData.map[i].map_object[j].image_link;
                StartCoroutine(GetObjectTexture(objAddr, i, j));
            }
        }
        StartCoroutine(WaitForDownloadCompelete());
    }

    IEnumerator GetMapTexture(string addr, int i)
    {
        string imageLink = GetImageLink(addr);

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageLink);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error + "imageLink: " + imageLink);
        }
        else
        {
            Texture2D t= ((DownloadHandlerTexture)www.downloadHandler).texture;
            t.filterMode = FilterMode.Point;
            gameData.map[i].mapTexture = t;
        }
    }

    IEnumerator GetObjectTexture(string addr, int i, int j)
    {
        string imageLink = GetImageLink(addr);

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageLink);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error + "imageLink: " + imageLink);
        }
        else
        {
            Texture2D t = ((DownloadHandlerTexture)www.downloadHandler).texture;
            t.filterMode = FilterMode.Point;
            gameData.map[i].map_object[j].objTexture = t;
        }
    }
    IEnumerator WaitForDownloadCompelete()
    {
        while (true)
        {
            bool isCompelete = true;
            foreach (GameMap gm in gameData.map)
            {
                if (gm.mapTexture == null) isCompelete = false;
                foreach (PlacedObject po in gm.map_object)
                {
                    if (po.objTexture == null) isCompelete = false;
                }

            }
            if (isCompelete == true) break;
            yield return null;
        }
        Debug.Log("Download Compelete!");
        isDownloadCompelete = true;
    }
#endregion

    #region Count Time
    private IEnumerator IECountTime;
    void StartCountTime(int t)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            countTime = t * 60;
            IECountTime = CountTime();
            StartCoroutine(IECountTime);
        }
        GM_PhotonView.RPC("RPCShowTimerText", RpcTarget.All);
    }

    void EndCountTime()
    {
        StopCoroutine(IECountTime);
        countTime = 0;

        GM_PhotonView.RPC("RPCSetTimerText", RpcTarget.All, countTime);
        GM_PhotonView.RPC("RPCShowSelectPanel", RpcTarget.All);
    }

    IEnumerator CountTime()
    {
        while(true)
        {
            countTime -= 1;
            GM_PhotonView.RPC("RPCSetTimerText", RpcTarget.All, countTime);
            if(countTime==0)
            {
                EndCountTime();
                break;
            }
            yield return new WaitForSeconds(1);
        }
    }
    [PunRPC]
    void RPCShowTimerText()
    {
        timer.gameObject.SetActive(true);
    }
    [PunRPC]
    void RPCSetTimerText(int t)
    {
        string s = "";
        if(t>=3600)
        {
            int hour = t / 3600;
            t = t % 3600;
            int min = t / 60;
            int sec = t % 60;
            if(min>=10)
            {
                if(sec>=10)
                    s = "" + hour + ":" + min + ":" + sec;
                else
                    s = "" + hour + ":" + min + ":0" + sec;
            }
            else
            {
                if (sec >= 10)
                    s = "" + hour + ":0" + min + ":" + sec;
                else
                    s = "" + hour + ":0" + min + ":0" + sec;
            }
            TimerText.text = s;
        }
        else if(t>60)
        {
            int min = t / 60;
            int sec = t % 60;

            if (min >= 10)
            {
                if (sec >= 10)
                    s =""+ min + ":" + sec;
                else
                    s = "" + min + ":0" + sec;
            }
            else
            {
                if (sec >= 10)
                    s = "0" + min + ":" + sec;
                else
                    s = "0" + min + ":0" + sec;
            }
            TimerText.text = s;
        }
        else
        {
            TimerText.text = "" + t;
        }
    }


    #endregion

}


