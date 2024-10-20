using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;

public class GameHandler : MonoBehaviour {

    [SerializeField] private CharacterWindow characterWindow;

    private void Awake() {
        Character character = new Character();
        character.SetEquippedBodyArmor(Character.BodyArmor.Tier_3);
        characterWindow.SetCharacter(character);

        /*
        CMDebug.ButtonUI(new Vector2(0, -80), "Damage 20", () => {
            character.Damage(20);
        });
        
        CMDebug.ButtonUI(new Vector2(-200, -130), "Heal 25", () => {
            character.HealHealth(25);
        });
        CMDebug.ButtonUI(new Vector2(-100, -130), "Shield 25", () => {
            character.HealShield(25);
        });
        CMDebug.ButtonUI(new Vector2(+100, -130), "Heal 100", () => {
            character.HealHealth(100);
        });
        CMDebug.ButtonUI(new Vector2(+200, -130), "Shield 100", () => {
            character.HealShield(100);
        });
        CMDebug.ButtonUI(new Vector2(0, -200), "MEGA HEAL", () => {
            character.HealHealth(100);
            character.HealShield(100);
        });
        */
    }
}
