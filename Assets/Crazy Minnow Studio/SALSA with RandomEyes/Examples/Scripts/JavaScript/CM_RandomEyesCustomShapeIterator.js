#pragma strict

/*
	Script usage instructions

	This class demonstrates the use of the RandomEyes 2D/3D [Broadcast Eye Events], 
	and the RandomEyes 3D [Broadcast Custom Shape Events], to catch broadcasted
	RandomEyes_OnLookStatusChanged and RandomEyes_OnCustomShapeChanged events.

	1. Create a GameObject and attach this script to it.
	2. Set this GameObject as a [Broadcast Receiver] in RandomEyes.
*/

public class CM_RandomEyesCustomShapeIterator extends MonoBehaviour {
	public var randomEyes3D : CrazyMinnow.SALSA.RandomEyes3D;

	private var customIndex : int = 0;

	function Start() {
		if (!randomEyes3D) {
			randomEyes3D = GetComponent(CrazyMinnow.SALSA.RandomEyes3D);
		}
	}

	function RandomEyes_OnCustomShapeChanged(customShape : CrazyMinnow.SALSA.RandomEyesCustomShapeStatus) {
		if (customShape.isOn == true) {
			if (customIndex < randomEyes3D.customShapes.Length-1) {
				customIndex++;
			}
			else {
				customIndex = 0;
			}
			
			randomEyes3D.SetCustomShape(randomEyes3D.customShapes[customIndex].shapeName);
		}
	}
}