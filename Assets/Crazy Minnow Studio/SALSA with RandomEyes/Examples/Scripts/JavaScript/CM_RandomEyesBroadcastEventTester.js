#pragma strict

/*
	Script usage instructions

	This class demonstrates the use of the RandomEyes 2D/3D [Broadcast Eye Events], 
	and the RandomEyes 3D [Broadcast Custom Shape Events], to catch broadcasted
	RandomEyes_OnLookStatusChanged and RandomEyes_OnCustomShapeChanged events.

	1. Create a GameObject and attach this script to it.
	2. Set this GameObject as a [Broadcast Receiver] in RandomEyes.
*/

/* This class demonstrates the use of the RandomEyes [Broadcast Eye Events] 
 and [Broadcast Custom Shape Events] [Broadcast Receivers]
*/
public class CM_RandomEyesBroadcastEventTester extends MonoBehaviour {
	function RandomEyes_OnLookStatusChanged(status : CrazyMinnow.SALSA.RandomEyesLookStatus) {
		Debug.Log("RandomEyes_OnLookStatusChanged:" +
			" instance(" + status.instance.GetType() + ")," +
			" name(" + status.instance.name + ")," +
			" lookPosition(" + status.lookPosition + ")," +
			" blendSpeed(" + status.blendSpeed + ")," +
			" rangeOfMotion(" + status.rangeOfMotion + ")");
	}

	function RandomEyes_OnCustomShapeChanged(status : CrazyMinnow.SALSA.RandomEyesCustomShapeStatus) {
		Debug.Log("RandomEyes_OnCustomShapeChanged:" +
			" instance(" + status.instance.GetType() + ")," +
			" name(" + status.instance.name + ")," +
			" shapeIndex(" + status.shapeIndex + ")," +
			" shapeName(" + status.shapeName + ")," +
			" overrideOn(" + status.overrideOn + ")," +
			" isOn(" + status.isOn + ")," +
			" blendSpeed(" + status.blendSpeed + ")," +
			" rangeOfMotion(" + status.rangeOfMotion + ")");
	}
}