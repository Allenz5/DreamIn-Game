using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public class AdvPlayer : MonoBehaviour
{
    public string name;
    public int health;
    public int curHealth;
    public AdvAbility[] abilities;
    public Slider HP;
    // Start is called before the first frame update
    void Start()
    {
        abilities = BattleManager.instance.abilities.GetComponentsInChildren<AdvAbility>();
        HP.gameObject.SetActive(false);
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
    public void SetPlayerData(GameCharacter data){
        name = data.name;
        if(data.health == null){
            health = 1;
        } else{
            health = data.health;

        }
        Debug.Log(health);
        curHealth = health;
        if(data.abilities != null){
            for (int i = 0;  i < abilities.Length; i++){
                abilities[i].SetPlayerData(data.abilities[i]);
            }
        }
        

    }

    public void Reset(){
        curHealth = health;
        HP.value = 1;
        foreach(AdvAbility abi in abilities){
            abi.reset();
        }
    }
}
