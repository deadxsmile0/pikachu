using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioControllerScript : MonoBehaviour {
    public List<AudioClip> vAuClipList;
    public List<string> vAuClipName;
    public List<AudioSource> vAuSrc;
    void Start() {
        vAuSrc.Add(gameObject.GetComponent<AudioSource>());
        vAuClipName = new List<string>();
        for (int a = 0; a < vAuClipList.Count; a++) {
            vAuClipName.Add(vAuClipList[a].name);
        }
        LoadAudio(vAuClipName[0]);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            vAuSrc[0].Play();
        }
    }

    public AudioSource PlayAudio(string _clipName) {
        for (int a = 0; a < vAuSrc.Count; a++) {
            if (!vAuSrc[a].isPlaying) {
                vAuSrc[a].clip = LoadAudio(_clipName);
                vAuSrc[a].Play();
                return vAuSrc[a];
            }
        }
        return null;
    }

    public AudioClip LoadAudio(string _clipName) {
        for (int a = 0; a < vAuClipList.Count; a++) {
            if (vAuClipList[a].name == _clipName) {
                return vAuClipList[a];
            }
        }
        return null;
    }
}
