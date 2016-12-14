using UnityEngine;

namespace Assets.Scripts
{
	[ExecuteInEditMode]
	public class BillboardBehaviour : MonoBehaviour
	{
		private void LateUpdate()
		{
			transform.LookAt(Camera.main.transform,Vector3.up);
		}
	}
}