using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffGameObjectWhenGoOutsiteCamViewScript : MonoBehaviour {
    private void OnBecameInvisible() {
        gameObject.SetActive(false);
    }
}
