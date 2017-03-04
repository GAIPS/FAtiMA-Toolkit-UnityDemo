#pragma strict

// Demonstrates use of the RandomEyes3D public methods
public class CM_RandomEyes3D_Functions extends MonoBehaviour {
	public var randomEyes3D : CrazyMinnow.SALSA.RandomEyes3D; // Reference to the RandomEyes3D class
	public var mainCam : Camera; // Game camera
	public var target : SpriteRenderer; // SpriteRenderer target used to demonstrate eye target tracking

	private var random : boolean = true; // Toggle [Random Eyes] checkbox on the RandomEyes inspector
	private var affinity : boolean = false; // Toggle [Target Affinity] for the target
	private var affinitySet : boolean = true; // Used to make sure SetTargetAffinity only fires once
	private var track : boolean = false; // Set/clear the [Look Target] on the RandomEyes inspector
	private var trackSet : boolean = true; // Used to make sure SetLookTarget only fires once
	private var targetPosHome : Vector3; // Home position
	private var targetPos : Vector3; // Target sprite mapped to cursor position to demonstrate [Look Target] eye tracking

	// These private variables are used to position buttons in the OnGUI method
	private var xPos : int = 0; // The Z position of a GUI button
	private var yPos : int = 0; // The Y position of a GUI button
	private var yGap : int = 5; // The vertical spacing between GUI buttons
	private var xWidth : int = 150; // The X width of GUI buttons and labels
	private var yHeight : int = 30; // The Y height of GUI buttons and labels

	// On start, try to get a local reference to the RandomEyes3D class and the scene  camera
	function Start() {
		if (!randomEyes3D) // randomEyes3D is null
			randomEyes3D = FindObjectOfType(CrazyMinnow.SALSA.RandomEyes3D); // Try to get a local reference to RandomEyes3D
		
		if (!mainCam) // mainCam is null
			mainCam = FindObjectOfType(Camera); // Try to get a local reference to the scene camera

		targetPosHome = target.transform.position;
	}

	// Draw the GUI buttons
	function OnGUI() {
		xPos = Screen.width - 20 - xWidth; // X position for right side GUI controls
		yPos = 0; // Reset the button Y position
		
		// Turn random blink on or off
		yPos += yGap;
		if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Toggle Blink")) {
			if (randomEyes3D.blink) {
				randomEyes3D.SetBlink (false);
			} 
			else {
				randomEyes3D.SetBlink (true);
			}
		}
		if (randomEyes3D.blink) {
			GUI.Label(Rect(xPos - 120, yPos, xWidth, yHeight), "Random Blink On");
		} 
		else {
			GUI.Label(Rect(xPos - 120, yPos, xWidth, yHeight), "Random Blink Off");
		}
		// When random blink is off, demonstrate programmatic blinking
		if (!randomEyes3D.blink) {
			yPos += (yGap + yHeight);
			if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Blink")) {
				randomEyes3D.Blink (0.075f);
			}
		}
		
		// Toggle target affinity to the target
		yPos += (yGap + yHeight);
		if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Toggle Affinity")) {
			if (affinity) {
				affinity = false;
			}
			else {
				affinity = true;
			}
			affinitySet = true;
		}
		if (affinity) {
			GUI.Label (Rect (xPos - 120, yPos, xWidth, yHeight), "Affinity On: " + randomEyes3D.targetAffinityPercentage + "%");
		} 
		else {
			GUI.Label (Rect (xPos - 120, yPos, xWidth, yHeight), "Affinity Off");
		}
		if (affinitySet) {
			if (affinity) {
				randomEyes3D.SetTargetAffinity(true);
				randomEyes3D.SetLookTarget(target.gameObject);
			}
			else {
				randomEyes3D.SetTargetAffinity(false);
			}
			affinitySet = false;
		}		

		// Turn [Look Target] tracking on or off
		yPos += (yGap + yHeight);	
		if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Toggle Tracking")) {
			if (track) {
				track = false;
			} 
			else {
				track = true;
			}
			trackSet = true;
		}
		if (track) {
			GUI.Label (new Rect (xPos - 120, yPos, xWidth, yHeight), "Tracking On");
		} 
		else {
			GUI.Label (new Rect (xPos - 120, yPos, xWidth, yHeight), "Tracking Off");
		}

		// Turn random eye movement on or off
		yPos += (yGap + yHeight);	
		if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Toggle RandomEyes")) {
			if (random) {
				randomEyes3D.SetRandomEyes (false);
				random = false;
			} 
			else {
				randomEyes3D.SetRandomEyes (true);
				random = true;
			}
		}
		// Display the on/off status of random eye movement
		if (random) {
			GUI.Label(Rect(xPos - 120, yPos, xWidth, yHeight), "Random Eyes On");
		}
		else {
			GUI.Label(Rect(xPos - 120, yPos, xWidth, yHeight), "Random Eyes Off");
		}
		
		// Display the on/off status, set target position to cursor position, and set the randomEyes3D.lookTarget
		if (track) {
			targetPos = Input.mousePosition;
			targetPos.z = -mainCam.transform.position.z - -target.transform.position.z;
			target.transform.position = Vector3(
				mainCam.ScreenToWorldPoint (targetPos).x,
				mainCam.ScreenToWorldPoint (targetPos).y, -2f);
		} 
		else {
			target.transform.position = targetPosHome;
		}
		if (trackSet) {
			if (track) {
				randomEyes3D.SetLookTarget(target.gameObject);
			}
			else {
				randomEyes3D.SetLookTarget(null);
			}
			trackSet = false;
		}
		
		// When random eye movement is off, demonstrate programmatic eye control
		if (!random) {		
			// Set programmatic directional look controls
			yPos += (yGap + yHeight);
			if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Look Up Right")) {
				randomEyes3D.Look (CrazyMinnow.SALSA.RandomEyesLook.Position.UpRight);
			}
			
			yPos += (yGap + yHeight);
			if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Look Up")) {
				randomEyes3D.Look (CrazyMinnow.SALSA.RandomEyesLook.Position.Up);
			}
			
			yPos += (yGap + yHeight);
			if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Look Up Left")) {
				randomEyes3D.Look (CrazyMinnow.SALSA.RandomEyesLook.Position.UpLeft);
			}
			
			yPos += (yGap + yHeight);
			if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Look Right")) {
				randomEyes3D.Look (CrazyMinnow.SALSA.RandomEyesLook.Position.Right);
			}
			
			yPos += (yGap + yHeight);
			if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Look Forward")) {
				randomEyes3D.Look (CrazyMinnow.SALSA.RandomEyesLook.Position.Forward);
			}
			
			yPos += (yGap + yHeight);
			if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Look Left")) {
				randomEyes3D.Look (CrazyMinnow.SALSA.RandomEyesLook.Position.Left);
			}
			
			yPos += (yGap + yHeight);
			if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Look Down Right")) {
				randomEyes3D.Look (CrazyMinnow.SALSA.RandomEyesLook.Position.DownRight);
			}
			
			yPos += (yGap + yHeight);
			if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Look Down")) {
				randomEyes3D.Look (CrazyMinnow.SALSA.RandomEyesLook.Position.Down);
			}
			
			yPos += (yGap + yHeight);
			if (GUI.Button(Rect(xPos, yPos, xWidth, yHeight), "Look Down Left")) {
				randomEyes3D.Look (CrazyMinnow.SALSA.RandomEyesLook.Position.DownLeft);
			}
		}
		
		if (!affinity && !track) randomEyes3D.SetLookTarget(null);
	}
}