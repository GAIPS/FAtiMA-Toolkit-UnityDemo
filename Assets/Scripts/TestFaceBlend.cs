using System;

using UnityEngine;
using System.Collections;

public class TestFaceBlend : MonoBehaviour
{

	int blendShapeCount;
	public SkinnedMeshRenderer skinnedMeshRenderer;
	public Mesh skinnedMesh;
	float blendOne = 0f;
	float blendTwo = 0f;
	public int [] blended ;
	float blendSpeed = 1f;
	bool blendOneFinished = false;

	void Awake ()
	{
		//skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer> ();
		//skinnedMesh = GetComponent<SkinnedMeshRenderer> ().sharedMesh;
	}

	void Start ()
	{
		blendShapeCount = skinnedMesh.blendShapeCount; 
	}

	void Update ()
	{
		if (blendShapeCount > 2) {
			foreach( int bed in blended ){
				if (blendOne < 100f) {
					skinnedMeshRenderer.SetBlendShapeWeight (bed, blendOne);
					blendOne += blendSpeed;
				} else {
					blendOneFinished = true;
				}

			}


		}
	}
}