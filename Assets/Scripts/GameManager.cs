using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using LitJson;
using TMPro;
using System.Text;

//single game version
public class GameManager :MonoBehaviour
{
    public GameObject readyButton;
    public GameObject startButton;
    public GameObject finishButton;
    public GameObject cluesButton;
    public GameObject scriptScroll;
    public GameObject gameCanvas;
    public GameObject objects;
    public GameObject colliders;
    public GameObject playerPrefb;
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

    public GameData gameData;
    private string gameDataID;
    private GameObject localPlayer;

    private int countTime = 0;
    private bool isDownloadCompelete=false;
    private int ColliderSize = 24;
    public int mapIndex = 0;

    public void Start()
    {    
        localPlayer = GameObject.Instantiate<GameObject>(playerPrefb, gameCanvas.transform.position, Quaternion.identity);
        Debug.Log(localPlayer);
        localPlayer.transform.parent = gameCanvas.transform;
        //localPlayer=GameObject.Instantiate<GameObject>(playerName, gameCanvas.transform.position, Quaternion.identity);
        localPlayer.transform.localPosition = new Vector2(0, 50);
        Camera.main.GetComponent<CameraFollow>().SetTarget(localPlayer);

        readyButton.SetActive(false);
        scriptScroll.SetActive(true);
        Debug.Log(localPlayer);
    }

void InitializedGame()
    {
        //initailize player panel
        if (localPlayer != null)
        {
            PlayerNameText.text = "Your name is " + localPlayer.GetComponent<PlayerScript>().GetPlayerName();
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

        startButton.SetActive(true);
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

            map.GetComponent<RectTransform>().sizeDelta = new Vector2(w*0.75f, h*0.75f);
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
                obj.GetComponent<Image>().sprite = Sprite.Create(gameData.map[index].map_object[i].objTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(w*0.75f, h*0.75f);
                obj.GetComponent<Object>().objectInfoPanel = objectInfoPanel;
                obj.GetComponent<Object>().SetInfo(gameData.map[index].map_object[i].message);
                obj.GetComponent<Object>().GM = this;
                obj.transform.localPosition = gameData.map[index].map_object[i].GetPosition();

                obj.transform.SetParent(objects.transform);
            }
        }

        //update collision
        {
            float w = gameData.map[index].mapTexture.width * 0.75f/ 2;
            float h = gameData.map[index].mapTexture.height *0.75f / 2;
            string[] rows = gameData.map[index].collide_map.Split(';');
            for (int i = 0; i < rows.Length; i++)
            {
                string[] cols = rows[i].Split(',');
                for (int j = 0; j < cols.Length - 1; j++)
                {
                    GameObject obj = Instantiate(colliderPrefab, new Vector2(0, 0), Quaternion.identity, colliders.transform);
                    obj.GetComponent<RectTransform>().sizeDelta = new Vector2(ColliderSize, ColliderSize);
                    obj.transform.localPosition = new Vector3(-w + int.Parse(cols[j]) * ColliderSize + ColliderSize / 2, h- i * ColliderSize - ColliderSize / 2, 0);
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
    void ShowSelectPanel()
    {
        if (localPlayer == null || localPlayer.tag == "Watcher") return;
        questionPanel.SetActive(true);
        questionPanel.GetComponent<QuestionPanel>().SetQuestion(gameData.map[mapIndex].question);
        questionPanel.GetComponent<QuestionPanel>().CreateAnswerItem(gameData.map[mapIndex].answers);
    }
    void SetPlayerMove(bool canMove)
    {
        localPlayer.GetComponent<PlayerScript>().canMove = canMove;
    }

    #region Button
    public void ReadyButton()
    {
        localPlayer = GameObject.Instantiate<GameObject>(playerPrefb, gameCanvas.transform.position, Quaternion.identity);
        localPlayer.transform.position = gameCanvas.transform.position;
        //localPlayer=GameObject.Instantiate<GameObject>(playerName, gameCanvas.transform.position, Quaternion.identity);
        localPlayer.transform.localPosition = new Vector2(0, 50);
        Camera.main.GetComponent<CameraFollow>().SetTarget(localPlayer);

        readyButton.SetActive(false);
        scriptScroll.SetActive(true);

    }
    public void StartButton()
    {
        if (gameData == null)
        {
            Debug.Log("game data has not been download!");
            return;
        }
        if(isDownloadCompelete==false)
        {
            Debug.Log("game data downloading");
            return;
        }
        finishButton.SetActive(true);

        startButton.SetActive(false);

        //instantiate players
        List<GameCharacter> characters = new List<GameCharacter>(gameData.character);
        GameObject[] playerObj = GameObject.FindGameObjectsWithTag("Player");
        if (playerObj.Length > characters.Count)
        {
            Debug.LogError("too many players!");
            return;
        }
        List<int> selectedIndex=new List<int>();
        for(int i=0;i<playerObj.Length;i++)
        {
            int index = Random.Range(0, characters.Count);
            while(selectedIndex.Contains(index))
                index = Random.Range(0, characters.Count);

            selectedIndex.Add(index);
            playerObj[i].GetComponent<PlayerScript>().SetPlayerData(index);
        }
        //now all players can move
        SetPlayerMove(true);

        InitializedGame();

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

    /// <summary>
    /// if click close button on end panel, call this method
    /// </summary>
    public void CloseEndPanel()
    {

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
    public void DownLoadGameData(string ID)
    {
        gameDataID = ID;
        //StartCoroutine(GetGameData(ID));
        StartCoroutine(SingleGameData(ID));
    }
    string GetGameDataLink(string ID)
    {
        return "https://api.dreamin.land/q_game/?id="+ID;
    }

    //test method, for debug
    IEnumerator SingleGameData(string ID)
    {
        ////Manually remove double quotation marks
        //string gameDocStr = "\"game_doc\":";
        //string text = File.ReadAllText("Assets/JsonData/DebugData.json");
        //int index = text.IndexOf(gameDocStr) + gameDocStr.Length;
        //string substr = text.Substring(index);
        //string gameDataStr = substr.Substring(2, substr.Length - 2);
        var uri = new System.Uri(Path.Combine(Application.streamingAssetsPath, ID + ".json"));
        UnityWebRequest www = UnityWebRequest.Get(uri);
        yield return www.SendWebRequest();

        string gameDataStr = www.downloadHandler.text;
        Debug.Log(www.downloadHandler.text);
        //read and store in gameData
        gameData = JsonMapper.ToObject<GameData>(gameDataStr);
        int playerCount = GameObject.FindGameObjectsWithTag("Player").Length;
        if (playerCount >= int.Parse(gameData.players_num))
        {
            for (int i = 0; i < gameData.map.Count; i++)
            {
                string addr = gameData.map[i].background;
                GetMapTexture(addr, i);

                for (int j = 0; j < gameData.map[i].map_object.Count; j++)
                {
                    string objAddr = gameData.map[i].map_object[j].image_link;
                    GetObjectTexture(objAddr, i, j);
                }
            }
            StartCoroutine(WaitForDownloadCompelete());
            scriptScroll.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("not enough player for this script!\n " + gameData.character.Count);
            scriptScroll.gameObject.SetActive(true);
        }
    }
  

    public void GetMapTexture(string addr, int i)
    {
        /*
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
        */
        Texture2D t= Resources.Load<Texture2D>(addr);
        t.filterMode = FilterMode.Point;
        gameData.map[i].mapTexture = t;
        
    }

    public void GetObjectTexture(string addr, int i, int j)
    {
        /*
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
        */

        Texture2D t= Resources.Load<Texture2D>(addr);
        t.filterMode = FilterMode.Point;
        gameData.map[i].map_object[j].objTexture = t;
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
        InitializeScene();
    }
#endregion

#region Count Time
    private IEnumerator IECountTime;
    void StartCountTime(int t)
    {
        countTime = t * 60;
        IECountTime = CountTime();
        StartCoroutine(IECountTime);
        ShowTimerText();
    }

    void EndCountTime()
    {
        StopCoroutine(IECountTime);
        countTime = 0;

        SetTimerText(countTime);
        ShowSelectPanel();
    }

    IEnumerator CountTime()
    {
        while(true)
        {
            countTime -= 1;
            SetTimerText(countTime);
            if (countTime==0)
            {
                EndCountTime();
                break;
            }
            yield return new WaitForSeconds(1);
        }
    }
    void ShowTimerText()
    {
        timer.gameObject.SetActive(true);
    }
    void SetTimerText(int t)
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


