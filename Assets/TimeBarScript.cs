using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeBarScript : MonoBehaviour {
    public float vTimeLeft, vOldTimeLeft;
    public GameObject vHolder;
    public SpriteRenderer vSPRen;
    public GameObject vBlackLayer;
    public SpriteRenderer vBlackLayerSPrender;
    public AudioControllerScript vAuCon;
    public bool vRunAllow;
    public float R, G, B;
    private void Awake() {
        vOldTimeLeft = vTimeLeft;
        vRunAllow = false;
        vBlackLayerSPrender = vBlackLayer.GetComponent<SpriteRenderer>();
    }
    private void FixedUpdate() {
        if (vRunAllow) {
            if (vTimeLeft > 0) {
                vTimeLeft -= Time.deltaTime;
                vHolder.transform.localScale = new Vector3(MyHelperScript.NormalizeNumber(vTimeLeft, 0, vOldTimeLeft), 1, 1);
                vSPRen.color = new Vector4(R, G, B, 1);
            } else {
                Debug.Log("Time is up!!");
                foreach (AudioSource x in vAuCon.vAuSrc) {
                    x.Stop();
                }
                vBlackLayer.SetActive(true);
            }
        }
        if (vBlackLayerSPrender.color.a >= 1) {
            SceneManager.LoadScene("You Lose", LoadSceneMode.Single);
        }
    }
    public void ResetTimeBar() {
        vTimeLeft *= 1.5f;
        if (vTimeLeft > vOldTimeLeft) {
            vTimeLeft = vOldTimeLeft;
        }
    }
    public void TimeBarPenalty() {
        vTimeLeft *= .5f;
    }
    public void BonusTime() {
        vTimeLeft += 1;
        if (vTimeLeft > vOldTimeLeft) {
            vTimeLeft = vOldTimeLeft;
        }
    }
}
