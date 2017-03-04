#pragma strict

/*
	Script usage instructions

	This class demonstrates the use of the Salsa [Broadcast Receivers] property to catch broadcast talk status changes.

	1. Create a GameObject and attach this script to it.
	2. Set this GameObject as a [Broadcast Receiver] in Salsa.
*/

/* This class demonstrates the use of the Salsa [Broadcast Receivers] 
 property to catch broadcast talk status changes.
*/
public class CM_SalsaBroadcastEventTester extends MonoBehaviour {
	function Salsa_OnTalkStatusChanged(status : CrazyMinnow.SALSA.SalsaStatus) {
		Debug.Log("Salsa_OnTalkStatusChanged:" +
			" instance(" + status.instance.GetType() + ")," +
			" talkerName(" + status.talkerName + ")," +
			((status.isTalking) ? "started" : "finished") + " saying " + status.clipName);
	}
}