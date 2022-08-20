using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreScript : MonoBehaviour {
    public TextMeshPro vTmp;
    void Start() {
        vTmp = gameObject.GetComponent<TextMeshPro>();
    }

    public void ChangeText(string _text) {
        vTmp.text = _text;
    }
}
