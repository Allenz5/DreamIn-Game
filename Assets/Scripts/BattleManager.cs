using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    public GameObject abilityPanel;
    public GameObject abilities;
    public GameObject desc;

    public AdvPlayer localPlayer;
    public NPC npc;
    System.Random random;

    public int round=0;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != null)
        {
            Destroy(gameObject);
        }
        abilityPanel.SetActive(false);
        random = new System.Random();
    }

    public string GetCurName(){
        if(round == 0){
            return localPlayer.name;
        } else{
            return npc.name;
        }
    }
    public void ChangePlayerHP(int index, int amount){
        if(index == 0){
            localPlayer.ChangeHP(amount);
        } else{
            npc.ChangeHP(amount);
        }

    }

    public void SetPlayers(GameObject player, GameObject boss){
        localPlayer = player.GetComponent<AdvPlayer>();
        npc = boss.GetComponent<NPC>();
    }

    public void Animate(int target){
        GameObject obj; 
        if(target == 0){
            obj = localPlayer.gameObject;
        } else{
            obj = npc.gameObject;
        }
        StartCoroutine(Animate_Corountine(obj));

    }
    public IEnumerator Animate_Corountine(GameObject target){
        Vector3 shakeRate = new Vector3(2.5f, 2.5f, 2.5f);
        float shakeTime = 0.2f;
        float shakeDeltaTime = 0.01f;
        var oriPosition = target.transform.position;
        for(float i = 0; i < shakeTime; i += shakeDeltaTime) {
            target.transform.position = oriPosition +
                Random.Range(-shakeRate.x, shakeRate.x) * Vector3.right +
                Random.Range(-shakeRate.y, shakeRate.y) * Vector3.up +
                Random.Range(-shakeRate.z, shakeRate.z) * Vector3.forward;
            yield return new WaitForSeconds(shakeDeltaTime);
        }
        target.transform.position = oriPosition;
    }

    public void Display(AdvAbility abi){
        abilities.SetActive(false);
        desc.GetComponent<TMP_Text>().text = GetCurName() + " used\n" + abi.desc;
        desc.SetActive(true);

    }

    public void Display(Ability abi){
        abilities.SetActive(false);
        desc.GetComponent<TMP_Text>().text = GetCurName() + " used\n" + abi.desc;
        desc.SetActive(true);

    }

    public void NextTurn(){
        if(CheckGame()){
            return ;
        }
        round = (round + 1) % 2;
        if(round == 0){
            abilities.SetActive(true);
            desc.SetActive(false);
        } else{
            int index = random.Next(0,4);
            bool used = npc.abilities[index].use();
            while(!used){
                used = npc.abilities[index].use();
            }
        }

        
    }

    public bool CheckGame(){
        if(localPlayer.curHealth <= 0){
            abilityPanel.SetActive(false);
            localPlayer.HP.gameObject.SetActive(false);
            localPlayer.gameObject.GetComponent<Collider2D>().enabled = true;
            npc.gameObject.GetComponent<Collider2D>().enabled = true;
            Reset();
            localPlayer.gameObject.GetComponent<PlayerScript>().UnFreeze();
            return true;
        } else if(npc.curHealth <= 0){
            abilityPanel.SetActive(false);
            localPlayer.gameObject.GetComponent<Collider2D>().enabled = true;
            npc.gameObject.GetComponent<Collider2D>().enabled = true;
            GameManager.instance.LevelCompelete();
            localPlayer.gameObject.GetComponent<PlayerScript>().UnFreeze();
            return true;
        }
        return false;
    }

    public void Reset(){
        localPlayer.Reset();
        npc.Reset();
        localPlayer.transform.localPosition = Vector2.zero;
    }

    public void StartBattle(){
        round = 0;
        abilityPanel.SetActive(true);
        abilities.SetActive(true);
        desc.SetActive(false);
        localPlayer.gameObject.GetComponent<PlayerScript>().Freeze();
        localPlayer.HP.gameObject.SetActive(true);
        localPlayer.gameObject.GetComponent<Collider2D>().enabled = false;
        npc.gameObject.GetComponent<Collider2D>().enabled = false;

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
