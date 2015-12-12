using UnityEngine;
using System.Collections;

public class AvatarControll : MonoBehaviour {

	public SkinnedMeshCombiner smc = null;

	// Use this for initialization
	void Start () {
        Application.targetFrameRate = 60; // ターゲットフレームレートを60に設定
        Combine();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Combine() {
		// スキンメッシュ結合
		this.smc.Combine();
	}
}
