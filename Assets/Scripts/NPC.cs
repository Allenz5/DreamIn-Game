using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Ability{
    public int damage;
    public int maxPlay;
    public int remainPlay;
    public string desc;
    public Target target;
    public string effect;

    public void reset(){
        remainPlay = maxPlay;
    }
    // Update is called once per frame
    public void SetPlayerData(GameAbility data){
        desc = data.desc;
        damage = data.dmg;
        maxPlay = data.maxPlay;
        reset();
        if(data.target == "self"){
            target = Target.self;
        } else{
            target = Target.opponent;
        }
        effect = data.effect;
    }
    public bool use(){
        if(remainPlay == 0){
            return false;
        }
        GameObject targetPlayer;
        int index;
        if(target == Target.self){
            index = BattleManager.instance.round;
        } else{
            index = (BattleManager.instance.round + 1) % 2;
        }
        BattleManager.instance.Display(this);
        BattleManager.instance.Animate(index);
        if(effect =="damage"){
            BattleManager.instance.ChangePlayerHP(index, damage);
        } else{
            BattleManager.instance.ChangePlayerHP(index, -damage);
        }
        remainPlay -= 1;
        return true;

    }
}


public class NPC : MonoBehaviour
{
    public TMP_Text nameText;
    public string name;
    public int health;
    public int curHealth;
    public Slider HP;
    public List<Ability> abilities;

    public void SetName(string data){
        name = data;
        nameText.text = name;
    }

    public void ChangeHP(int amount){
        curHealth -= amount;
        curHealth = System.Math.Max(curHealth, 0);
        curHealth = System.Math.Min(curHealth, health);
        StartCoroutine(AnimateHP((float)curHealth / (float)health));
    }
    public IEnumerator AnimateHP(float target){
        float diff= target - HP.value;
        float time = 0.2f;
        float deltaTime = 0.01f;
        float step = diff / (time/deltaTime);
        for(float i=0 ; i < time; i += deltaTime){
            HP.value += step;
            yield return new WaitForSeconds(deltaTime);
        }
    }

    public void SetPlayerData(GameNPC data){
        if(data.health == null){
            health = 1;
        } else{
            health = data.health;
        }
        curHealth = health;
        abilities = new List<Ability>();
        Debug.Log(data.abilities.Count);
        if(data.abilities != null){
            for (int i = 0;  i < data.abilities.Count; i++){
                Ability abi = new Ability();
                abi.SetPlayerData(data.abilities[i]);
                abilities.Add(abi);
            }
        }
        

    }

    public void Reset(){
        curHealth = health;
        HP.value = 1;
        foreach(Ability abi in abilities){
            abi.reset();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            BattleManager.instance.StartBattle();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
