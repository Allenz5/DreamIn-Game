using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum Target{
    self,
    opponent
}

public class AdvAbility : MonoBehaviour
{
    public int damage;
    public int maxPlay;
    public int remainPlay;
    public string desc;
    public Target target;
    public string effect;

    public TMP_Text damageText;
    public TMP_Text playText;
    public TMP_Text descText;
    public void reset(){
        remainPlay = maxPlay;
        UpdateInfo();
    }
    // Start is called before the first frame update
    void Start()
    {

        
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
        UpdateInfo();
    }

    public void UpdateInfo(){
        damageText.text = effect + ": " + damage;
        descText.text = desc;
        playText.text = "PP: " + remainPlay + "/" + maxPlay;

    }

    public void use(){
        if(remainPlay == 0){
            return;
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
        UpdateInfo();

    }
}
