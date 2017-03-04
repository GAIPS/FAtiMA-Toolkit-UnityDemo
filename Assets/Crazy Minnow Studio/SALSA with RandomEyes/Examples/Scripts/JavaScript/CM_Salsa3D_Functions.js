#pragma strict

// Demonstrates use of the Salsa3D public methods
public class CM_Salsa3D_Functions extends MonoBehaviour {
	public var salsa3D : CrazyMinnow.SALSA.Salsa3D; // Reference to the Salsa3D class
	public var audioClips : AudioClip[]; // An array of example sound to play

	private var clipIndex : int = 0; // Track audioClips index

	// These private variables are used to position buttons in the OnGUI method
	private var yPos : int = 0; // The Y position of a GUI button
	private var yGap : int = 10; // The vertical spacing between GUI buttons
	private var xWidth : int = 150; // The X width of GUI buttons and labels
	private var yHeight : int = 30; // The Y height of GUI buttons and labels

	// On start, try to get a local reference to Salsa3D
	function Start () {
		if (!salsa3D) { // salsa3D is null
			salsa3D = FindObjectOfType(CrazyMinnow.SALSA.Salsa3D); // Try to get a local reference to Salsa3D
		}
		
		if (audioClips.Length > 0) {
			salsa3D.SetAudioClip(audioClips[clipIndex]);
		}
	}

	// Draw the GUI buttons
	function OnGUI() {
		yPos = 0; // Reset the button Y position
		
		// Salsa3D Play, Pause, and Stop controls
		yPos += yGap;
		if (GUI.Button(Rect(20, yPos, xWidth, yHeight), "Play")) {
			salsa3D.Play(); // Salsa3D Play method
		}
		
		yPos += (yGap + yHeight);
		if (GUI.Button(Rect(20, yPos, xWidth, yHeight), "Pause")) {
			salsa3D.Pause(); // Salsa3D Pause method
		}
		
		yPos += (yGap + yHeight);
		if (GUI.Button(Rect(20, yPos, xWidth, yHeight), "Stop")) {
			salsa3D.Stop(); // Salsa3D Stop method
		}
		
		// Toggle which audio clip is set on Salsa3D
		yPos += (yGap + yHeight);
		if (GUI.Button(Rect(20, yPos, xWidth, yHeight), "Set audio clip")) {
			if (clipIndex < audioClips.Length - 1) {
				clipIndex++;
				salsa3D.SetAudioClip(audioClips[clipIndex]);
			}
			else {
				clipIndex = 0;
				salsa3D.SetAudioClip(audioClips[clipIndex]);
			}
		}
		// Display the currently selected audio clip
		GUI.Label(Rect(30 + xWidth, yPos, xWidth, yHeight), "Clip " + audioClips[clipIndex].name);
	}
}