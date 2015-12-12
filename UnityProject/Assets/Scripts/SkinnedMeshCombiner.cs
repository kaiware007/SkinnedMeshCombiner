using UnityEngine;
using System.Collections.Generic;

public class SkinnedMeshCombiner : MonoBehaviour {
	// 身体の部位
	public enum MAIN_PARTS {
		HEAD,
		BODY,
		LEG,
		MAX,
	};

	// 素体となるボーン
	public Transform rootBoneObject = null; 
	private Dictionary<string, int> rootBoneList = new Dictionary<string, int>();

	// 装備品のオブジェクトのリスト
	public GameObject[] equipPartsObjectList = new GameObject[(int)MAIN_PARTS.MAX];

	/// <summary>
	/// Combine
	/// </summary>
	public void Combine() {
		
		SkinnedMeshRenderer[] smRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		List<Transform> bones = new List<Transform>();        
		List<BoneWeight> boneWeights = new List<BoneWeight>();        
		List<CombineInstance> combineInstances = new List<CombineInstance>();
		List<Texture2D> textures = new List<Texture2D>();
		Dictionary<string, int> boneIndexDic = new Dictionary<string, int> ();
		List<Material> materials = new List<Material>();
		
		int numSubs = 0;
		int boneOffset = 0;

		// 素体のボーンを先に登録する
		Transform[] rootBoneTransforms = this.rootBoneObject.GetComponentsInChildren<Transform> ();
		for (int i = 0; i < rootBoneTransforms.Length; i++) {
			bones.Add( rootBoneTransforms[i] );
			this.rootBoneList.Add(rootBoneTransforms[i].name, boneOffset++);
		}

        // Parts Mesh Add
        for( int p = 0; p < (int)MAIN_PARTS.MAX; p++ ) {
			
			if ( this.equipPartsObjectList[p] == null )
				continue;

			GameObject parts = this.equipPartsObjectList[p];
			
			Dictionary<int,int> boneIndexRplaceDic = new Dictionary<int, int>(4);
			
			SkinnedMeshRenderer[] smRenderersParts = parts.GetComponentsInChildren<SkinnedMeshRenderer>();
            
			for( int s = 0; s < smRenderersParts.Length; s++ ) {
				SkinnedMeshRenderer smr = smRenderersParts[s];
				materials.AddRange(smr.materials);
				
				Transform[] meshBones = smr.bones;
				BoneWeight[] meshBoneweight = smr.sharedMesh.boneWeights;
				bool isDuplication = false;

				boneIndexRplaceDic.Clear();
				int duplicationMeshNo = -1;
				for( int i = 0; i < meshBones.Length; i++ ) {
					// 素体のボーンと重複するボーンを調べる
					if(this.rootBoneList.ContainsKey(meshBones[i].name) ) {
						boneIndexRplaceDic.Add(i, this.rootBoneList[meshBones[i].name]);
					}
				}

				// 部位側のインデックスを素体側のボーンインデックスに置き換える
				foreach( BoneWeight bw in meshBoneweight ) {
					BoneWeight bWeight = bw;
					
					bWeight.boneIndex0 = boneIndexRplaceDic[bWeight.boneIndex0];
					bWeight.boneIndex1 = boneIndexRplaceDic[bWeight.boneIndex1];
					bWeight.boneIndex2 = boneIndexRplaceDic[bWeight.boneIndex2];
					bWeight.boneIndex3 = boneIndexRplaceDic[bWeight.boneIndex3];
					
					boneWeights.Add( bWeight );
				}

				// CombineInstance登録
				CombineInstance ci = new CombineInstance();
				ci.mesh = smr.sharedMesh;
                ci.transform = smr.transform.localToWorldMatrix;
				combineInstances.Add( ci );

				// 部位のSkinnedMeshRendererオブジェクトは削除する
				Object.Destroy( smr.gameObject );
			}
		}
		
		List<Matrix4x4> bindposes = new List<Matrix4x4>();
		
		for( int b = 0; b < bones.Count; b++ ) {
			bindposes.Add( bones[b].worldToLocalMatrix );
		}

		// メッシュ結合
		SkinnedMeshRenderer r = gameObject.AddComponent<SkinnedMeshRenderer>();
		r.sharedMesh = new Mesh();
		r.sharedMesh.CombineMeshes( combineInstances.ToArray(), false, true );
		r.sharedMaterials = materials.ToArray();
		r.bones = bones.ToArray();
		r.sharedMesh.boneWeights = boneWeights.ToArray();
		r.sharedMesh.bindposes = bindposes.ToArray();
		r.sharedMesh.RecalculateBounds();

		// 結合前の装備品のノードは不要なので削除
		for( int p = 0; p < (int)MAIN_PARTS.MAX; p++ ) {
			if ( this.equipPartsObjectList[p] == null )
				continue;
			Destroy(this.equipPartsObjectList[p]);	
		}
		
	}
    
}