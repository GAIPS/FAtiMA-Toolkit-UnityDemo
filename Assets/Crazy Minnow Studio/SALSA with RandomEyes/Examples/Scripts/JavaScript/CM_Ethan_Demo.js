#pragma strict

/*
	Script usage instructions

	CM_Ethan_Demo is a script that uses the SALSA and RandomEyes event systems to drive
	emotional expressions linked to RandomEyes as [Custom Shapes]. It uses
	Salsa_OnTalkStatusChanged events to choreograph the scene with expression changes,
	dialog changes, and lookTargets. It also uses the RandomEyes_OnLookStatusChanged 
	to trigger additional emotional expressions.
*/

public class CM_Ethan_Demo extends MonoBehaviour {
	public var salsa : CrazyMinnow.SALSA.Salsa3D;
	public var randomEyes : CrazyMinnow.SALSA.RandomEyes3D;
	public var lookTargets : GameObject[];
	public var clips : AudioClip[];
	public var salsaEvents : boolean = false;
	public var randomEyesLookEvents : boolean = false;
	public var randomEyesShapeEvents : boolean = false;

	// Play SALSA with the first audio clip after a 1 second delay
	function Start() {
		StartCoroutine(WaitStart(1f));
	}

	/* Here we use the Salsa on talk status changed event to: 
	 Listen for audio clip starts and stops
	 Call custom shape coroutines
	 Set and play the next dialog clip
	 Look at specific GameObjects
	*/
	function Salsa_OnTalkStatusChanged(status : CrazyMinnow.SALSA.SalsaStatus) {
		if (salsaEvents) {
			Debug.Log("Salsa_OnTalkStatusChanged:" +
				" instance(" + status.instance.GetType() + ")," +
				" talkerName(" + status.talkerName + ")," +
				((status.isTalking) ? "started" : "finished") + " saying " + status.clipName);
		}

		if (status.clipName == clips[0].name && status.isTalking) { // Line 0 start
			StartCoroutine(Look(0f, 2f, lookTargets[0])); // Look at camera
			StartCoroutine(Look(5f, 2f, lookTargets[1])); // Look at door
		}
		if (status.clipName == clips[0].name && !status.isTalking) { // Line 0 stop
			salsa.SetAudioClip(clips[1]);
			salsa.Play();
		}


		if (status.clipName == clips[1].name && status.isTalking) { // Line 1 start
			StartCoroutine(Look(0f, 3f, lookTargets[2])); // // Look at vent
		}
		if (status.clipName == clips[1].name && !status.isTalking) { // Line 1 stop
			salsa.SetAudioClip(clips[2]);
			salsa.Play();
		}


		if (status.clipName == clips[2].name && status.isTalking) { // Line 2 start
			StartCoroutine(Look(6f, 5f, lookTargets[0])); // // Look at camera
		}
		if (status.clipName == clips[2].name && !status.isTalking) { // Line 2 stop
			StartCoroutine(Look(0f, 2.5f, lookTargets[0]));  // Look at camera for 2.5 sec
			randomEyes.SetCustomShapeRandom(false); // Disable random custom shapes
			randomEyes.SetCustomShapeOverride("brows_inner_up", 2f); // Override brows_inner_up
			randomEyes.SetCustomShapeOverride("smile", 2f); // Overrid smile
		}
	}

	/* RandomEyes on look status changed lets us know when 
	 the eye postion has finished moving to the next position.
	 In this simple example scene, we are using the random
	 look positions to trigger custom shape emotions.
	*/
	function RandomEyes_OnLookStatusChanged(status : CrazyMinnow.SALSA.RandomEyesLookStatus) {
		if (randomEyesLookEvents) {
			Debug.Log("RandomEyes_OnLookStatusChanged:" +
				" instance(" + status.instance.GetType() + ")," +
				" name(" + status.instance.name + ")," +
				" lookPosition(" + status.lookPosition + ")," +
				" blendSpeed(" + status.blendSpeed + ")," +
				" rangeOfMotion(" + status.rangeOfMotion + ")");
		}

		// When looking up, raise the brows for a random duration
		if (status.lookPosition == CrazyMinnow.SALSA.RandomEyesLook.Position.Up) {
			//randomEyes.SetCustomShapeOverride("brows-up", Random.Range(1f, 3f));
		}

		// When looking down, lower the brows for a random duration
		if (status.lookPosition == CrazyMinnow.SALSA.RandomEyesLook.Position.Up) {
			//randomEyes.SetCustomShapeOverride("brows-down", Random.Range(1f, 3f));
		}
	}

	// We can also respond to custom shape changes
	function RandomEyes_OnCustomShapeChanged(status : CrazyMinnow.SALSA.RandomEyesCustomShapeStatus) {
		if (randomEyesShapeEvents) {
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

	// A coroutine to track a GameObject with a pre-delay and a track duration
	function Look(preDelay : float, duration : float, lookTarget : GameObject) {
		yield WaitForSeconds(preDelay);

		randomEyes.SetLookTarget(lookTarget);

		yield WaitForSeconds(duration);

		randomEyes.SetLookTarget(null);
	}

	/* A coroutine to add a delay before playing the first clip.
	 This is a hack to sync up the dialog to the mocap data.
	*/	
	function WaitStart(duration : float) {
		yield WaitForSeconds(duration);

		salsa.SetAudioClip(clips[0]);
		salsa.Play();
	}
}