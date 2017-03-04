#pragma strict

/* A simple class to provide a GUI reset button that calls 
 the reset functions on the CM_DialogSystem and CM_SalsaWaypoints
 in the demo scene.
*/
public class CM_GameManager extends MonoBehaviour {
	public var dialogSystem : CM_DialogSystem;
	public var spiderWaypoints : CM_SalsaWaypoints;
	public var boxheadWaypoints : CM_SalsaWaypoints;

	// GUI Reset button, resets the dialog sequence in the demo scene
	function OnGUI () {
		if (GUI.Button(Rect(20, Screen.height - 55, 75, 35), "Reset")) {
			spiderWaypoints.ResetSalsaWaypoints();
			boxheadWaypoints.ResetSalsaWaypoints();
			dialogSystem.ResetDialog();
		}
	}
}