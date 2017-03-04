#pragma strict

/* This class provides a simple example of how you can use a collision trigger
 to set the RandomEyes2D or RandomEyes3D lookTarget to enable eye tracking.
*/
public class CM_RandomEyesTriggerTracking extends MonoBehaviour {
	public var lookTarget : GameObject;
	public var emitDebug : boolean = true;

	private var randomEyes2D : CrazyMinnow.SALSA.RandomEyes2D;
	private var randomEyes3D : CrazyMinnow.SALSA.RandomEyes3D;
	private var randomEyes : GameObject;

	// Get reference to a RandomEyes component
	function Start () {
		randomEyes2D = GetComponent(CrazyMinnow.SALSA.RandomEyes2D);
		if (randomEyes2D) {
			randomEyes = randomEyes2D.gameObject;
		}

		randomEyes3D = GetComponent(CrazyMinnow.SALSA.RandomEyes3D);
		if (randomEyes3D) {
			randomEyes = randomEyes3D.gameObject;
		}
	}

	// OnTriggerEnter, set the RandomEyes lookTarget to the collider GameObject
	function OnTriggerEnter(col : Collider) {
		if (randomEyes2D) randomEyes2D.SetLookTarget(col.gameObject);
		if (randomEyes3D) randomEyes3D.SetLookTarget(col.gameObject);
		if (emitDebug) Debug.Log(randomEyes.name + " OnTriggerEnter2D triggered");
	}

	// OnTriggerExit, clear the RandomEyes lookTarget
	function OnTriggerExit(col : Collider) {
		if (randomEyes2D) randomEyes2D.SetLookTarget(null);
		if (randomEyes3D) randomEyes3D.SetLookTarget(null);
		if (emitDebug) Debug.Log(randomEyes.name + " OnTriggerExit2D triggered");
	}
}