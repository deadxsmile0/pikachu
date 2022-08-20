using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScript : MonoBehaviour {
    public GameObject vRetryButton;
    public RaycastHit2D vHit2d;
    void Start() {
        StartCoroutine(ShowRetryButton());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            try {
                vHit2d = Physics2D.Raycast(
                    gameObject.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition),
                    Vector3.forward,
                    999f
                );
                if (vHit2d.transform.gameObject.name.CompareTo("RetryButton") == 0) {
                    SceneManager.LoadScene("Main", LoadSceneMode.Single);
                }
            } catch {

            }
        }
    }

    IEnumerator ShowRetryButton() {
        yield return new WaitForSecondsRealtime(3);
        vRetryButton.SetActive(true);
    }
}
