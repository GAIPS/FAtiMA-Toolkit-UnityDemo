#pragma strict


/*
 Script usage instructions 

 CM_DialogSystem is a basic dialog system that demonstrates one approach to implementing Salsa into your game project. 
 
 1. Attach this script to an empty GameObject called [Game Manager], or whatever game object you want to manage your dialog.
 2. Set the [Npc Dialog] [Size] property to the number of NPC lines you wish to have on one or more NPC's.
 3. Link the following:
 	Npc: NPC GameObject that has the Salsa component attached to it.
 	Npc Text: The text script of the NPC's line.
 	Npc Audio: The audio clip that Salsa will process for this NPC.
 	Player Response: If you want the player to have response options to this line, set the 
 		[Player Response] [Size] property to the number of Player line options you wish to have.
 		Each player response consists of the following:
 			Player: Player GameObject that has the Salsa component attached to it.
 			Player Text: The text script of the Players's line.
 			Player Audio: The audio clip that Salsa will process for the Player.
 			NPC Dialog Index: The next [NPC Dialog] [Element] you wish to player 
 				after this player response is selected. (Allows basic conversation branching)
 			End Dialog: When checked, selecting this [Player Response] will end the 
 				dialog after this [Player Audio] file finishes playing.
		End Dialog: When checked, the dialog will end after this [Npc Dialog] [Element]'s
 			[Npc Audio] file finishes playing.
 4. Be sure to Set the GameObject, with this script attached to it, as a [BroadCast Receiver] of all Salsa enabled game actors
 	so this script will recieve [Salsa_OnTalkStatusChanged] events from Salsa's talk status changed events.
*/

/* A properties class that defines a Salsa GameObject and a Salsa type.
 It's used in the GetSalsaType function of the CM_DialogSystem class
 to store the GameObject and Salsa type (Salsa2D or ) that was
 detected in the GetSalsaType function. This class assits in allowing the
 CM_DialogSystem class to work on Salsa2D and Salsa3D powered characters.
*/
public class CM_SalsaTypeAndObject {
	public var salsaGameObject : GameObject;
	public enum SalsaTypeOf { Salsa2D, Salsa3D }
	public var salsaType : SalsaTypeOf = SalsaTypeOf.Salsa2D;
}

/* A properties class that defines player dialog response details.
 It's used in the GetSalsaType function of the CM_NPCDialog class.
*/
public class CM_PlayerResponse {
	public var player : GameObject; // Player GameObject that has the Salsa component attached
	public var playerText : String; // Player dialog text to display in the GUI
	public var playerAudio : AudioClip; // Player audio dialog to play
	public var npcDialogIndex : int; // The NPC dialog index triggered by this player response
	public var endDialog : boolean = false; // Will this response end all dialog
}

/* A properties class that defines NPC dialog, and stores player dialog responses.
 It's used in the GetSalsaType function of the CM_DialogSystem class.
*/
public class CM_NPCDialog {
	public var npc : GameObject; // NPC GameObject that has the Salsa component attached
	public var npcText : String; // NPC dialog text to display in the GUI
	public var npcAudio : AudioClip; // NPC audio dialog to play
	public var playerResponse : CM_PlayerResponse[]; // Array of player dialog responses
	public var endDialog : boolean = false; // Will this response end all dialog
}

// A basic dialog system that demonstrates one approach to implementing Salsa into your game project. 
public class CM_DialogSystem extends MonoBehaviour {
	public var npcDialog : CM_NPCDialog[]; // Array of NPC dialog

	private var salsa2D : CrazyMinnow.SALSA.Salsa2D; // If the detected character using Salsa2D, this variable is used
	private var salsa3D : CrazyMinnow.SALSA.Salsa3D; // If the detected character using Salsa3D, this variable is used
	private var npcDialogIndexTracker : int = 0; // Tracks the current NPC dialog index
	private var showNPCDialog : boolean = true; // Tracks the visible status of the NPC dialog text
	private var showPlayerResponses : boolean = false; // Tracks the visible status of the player dialog text
	private var endDialogPlayer : boolean = false; // Tracks when the player ends the dialog
	private var endDialogNpc : boolean = false; // Tracks whe the NPC ends the dialog
	private var salsaTypObj : CM_SalsaTypeAndObject; // See comments for the CM_SalsaTypeAndObject class listed above

	/* Determines if the NPC is using Salsa2D or Salsa3D, gets reference to 
	 the component, sets the first NPC audio clip, and plays the audio clip
	*/
	function Start() {
		salsaTypObj = GetSalsaType(npcDialog[npcDialogIndexTracker].npc);

		if (salsaTypObj.salsaType == CM_SalsaTypeAndObject.SalsaTypeOf.Salsa3D) {
			salsa3D = salsaTypObj.salsaGameObject.GetComponent(CrazyMinnow.SALSA.Salsa3D);
			salsa3D.SetAudioClip(npcDialog[npcDialogIndexTracker].npcAudio);
			salsa3D.Play();
		}

		if (salsaTypObj.salsaType == CM_SalsaTypeAndObject.SalsaTypeOf.Salsa2D) {
			salsa2D = salsaTypObj.salsaGameObject.GetComponent(CrazyMinnow.SALSA.Salsa2D);
			salsa2D.SetAudioClip(npcDialog[npcDialogIndexTracker].npcAudio);
			salsa2D.Play();
		}
	}

	// Method is called by SALSA broadcast when the talk status has changed
	function Salsa_OnTalkStatusChanged(status : CrazyMinnow.SALSA.SalsaStatus) {
		// Npc has stopped talking
		if (!status.isTalking && status.talkerName == npcDialog[npcDialogIndexTracker].npc.name) {
			// NPC says end dialog
			if (npcDialog[npcDialogIndexTracker].endDialog) {
				EndDialog();
			}

			if (!endDialogNpc) {
				// There are no player responses to this NPC dialog
				if (npcDialog[npcDialogIndexTracker].playerResponse.Length == 0) {
					// We're not at the end of the [Npc Dialog] array
					if (npcDialogIndexTracker < npcDialog.Length - 1) {
						npcDialogIndexTracker++; // Increment to the Npc dialog
						showNPCDialog = true; // Show NCP dialog
						Start(); // Get Salsa type, set audio clip, and play
					}
				}
				else { // There are player responses to this NPC dialog
					showPlayerResponses = true;
				}
			}
		}

		// Player has stopped talking
		if (!status.isTalking && status.talkerName != npcDialog[npcDialogIndexTracker].npc.name) {
			if (!endDialogNpc || !endDialogPlayer) {
				showNPCDialog = true; // Show NCP dialog
				Start(); // Get Salsa type, set audio clip, and play
			}
		}
	}

	// NPC dialog text and player dialog response text GUI
	function OnGUI() {
		var yPos : int = 0;
		var yStart : int = 20;
		var yIncrement : int = 40;

		// No end dialog flags are set
		if (!endDialogNpc || !endDialogPlayer) {
			if (showNPCDialog && !endDialogPlayer) {
				GUI.Label(Rect(20, yStart, 300, 35), npcDialog[npcDialogIndexTracker].npcText);
			}
		}

		if (showPlayerResponses) {
			yPos = yStart;
			// Loop through all player responses to the current NPC dialog
			for (var i : int = 0; i < npcDialog[npcDialogIndexTracker].playerResponse.Length; i++) {
				// Show response dialog text buttons
				if (GUI.Button(Rect(Screen.width - 320, yPos, 300, 35), npcDialog[npcDialogIndexTracker].playerResponse[i].playerText)) {
					// If this button was selected, get the Salsa type and GameObject
					salsaTypObj = GetSalsaType(npcDialog[npcDialogIndexTracker].playerResponse[i].player);

					// If Salsa3D
					if (salsaTypObj.salsaType == CM_SalsaTypeAndObject.SalsaTypeOf.Salsa3D) {
						salsa3D = salsaTypObj.salsaGameObject.GetComponent(CrazyMinnow.SALSA.Salsa3D);
						salsa3D.SetAudioClip(npcDialog[npcDialogIndexTracker].playerResponse[i].playerAudio);
						salsa3D.Play();
					}

					// If Salsa2D
					if (salsaTypObj.salsaType == CM_SalsaTypeAndObject.SalsaTypeOf.Salsa2D) {
						salsa2D = salsaTypObj.salsaGameObject.GetComponent(CrazyMinnow.SALSA.Salsa2D);
						salsa2D.SetAudioClip(npcDialog[npcDialogIndexTracker].playerResponse[i].playerAudio);
						salsa2D.Play();
					}

					// Check/Set the player end dialog flag
					endDialogPlayer = npcDialog[npcDialogIndexTracker].playerResponse[i].endDialog;
					// Set the next NPC dialog index
					npcDialogIndexTracker = npcDialog[npcDialogIndexTracker].playerResponse[i].npcDialogIndex;
					showNPCDialog = false; // Hide the NPC dialog
					showPlayerResponses = false; // Hide the player responses
				}
				yPos += yIncrement;
			}
		}
	}
	
	/* Gets the game object of the character with Salsa3D or Salsa2D attached, 
	 and returns an instance of the CM_SalsaTypeAndObject properties class 
	 with the Salsa GameObject and SalsaType
	*/
	private function GetSalsaType(character : GameObject) {
		var salsaTypObj = CM_SalsaTypeAndObject();

		if (character.GetComponent(CrazyMinnow.SALSA.Salsa2D) != null) { 
			salsaTypObj.salsaGameObject = character.GetComponent(CrazyMinnow.SALSA.Salsa2D).gameObject;
			salsaTypObj.salsaType = CM_SalsaTypeAndObject.SalsaTypeOf.Salsa2D;
		}
		else if (character.GetComponent(CrazyMinnow.SALSA.Salsa3D) != null) { 
			salsaTypObj.salsaGameObject = character.GetComponent(CrazyMinnow.SALSA.Salsa3D).gameObject;
			salsaTypObj.salsaType = CM_SalsaTypeAndObject.SalsaTypeOf.Salsa3D;
		}

		return salsaTypObj;
	}
	
	/* Ends the dialog by setting the NPC and Player end dialog flags to true,
	 and setting their respective show dialog flags to false.
	*/
	private function EndDialog()
	{
		endDialogNpc = true;
		endDialogPlayer = true;
		showNPCDialog = false;
		showPlayerResponses = false;
	}
	
	// Reset the dialog system at runtime
	public function ResetDialog() {
		npcDialogIndexTracker = 0;
		endDialogNpc = false;
		endDialogPlayer = false;
		showNPCDialog = true;
		showPlayerResponses = false;

		salsaTypObj = GetSalsaType(npcDialog[npcDialogIndexTracker].npc);
		
		if (salsaTypObj.salsaType == CM_SalsaTypeAndObject.SalsaTypeOf.Salsa3D) {
			salsa3D = salsaTypObj.salsaGameObject.GetComponent(CrazyMinnow.SALSA.Salsa3D);
			salsa3D.Stop();
		}
		
		if (salsaTypObj.salsaType == CM_SalsaTypeAndObject.SalsaTypeOf.Salsa2D) {
			salsa2D = salsaTypObj.salsaGameObject.GetComponent(CrazyMinnow.SALSA.Salsa2D);
			salsa2D.Stop();
		}

		if (npcDialog[npcDialogIndexTracker].playerResponse.Length > 0) {
			salsaTypObj = GetSalsaType(npcDialog[npcDialogIndexTracker].playerResponse[0].player);
			
			if (salsaTypObj.salsaType == CM_SalsaTypeAndObject.SalsaTypeOf.Salsa3D) {
				salsa3D = salsaTypObj.salsaGameObject.GetComponent(CrazyMinnow.SALSA.Salsa3D);
				salsa3D.Stop();
			}
			
			if (salsaTypObj.salsaType == CM_SalsaTypeAndObject.SalsaTypeOf.Salsa2D) {
				salsa2D = salsaTypObj.salsaGameObject.GetComponent(CrazyMinnow.SALSA.Salsa2D);
				salsa2D.Stop();
			}
		}

		Start();
	}
}