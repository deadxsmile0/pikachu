using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonkScript : MonoBehaviour {
    public SpriteRenderer vPkmbonkRender;
    public Animator vAnims;
    public void HidePkmSprite() {
        vPkmbonkRender.sprite = null;
    }
    public void TurnOffBonking() {
        vAnims.SetBool("Bonking", false);
    }
}
