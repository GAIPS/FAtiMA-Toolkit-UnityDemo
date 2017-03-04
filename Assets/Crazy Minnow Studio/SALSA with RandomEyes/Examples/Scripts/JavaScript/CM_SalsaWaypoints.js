#pragma strict

/*
	Script usage instructions

	A simple waypoints script to allow waypoint movement triggered by Salsa_OnTalkStatusChanged events.
	The script lets you use the Start and End of [AudioClip]'s used by Salsa to trigger waypoint
	destination and movement speed changes. When set as a [Broadcast Receiver] in Salsa, it listens
	for the start and end of audio clips from Salsa's [Salsa_OnTalkStatusChanged] events. When a Salsa
	[Audio Clip] matches and CM_SalsaWaypoints [Audio Clip], the waypoint details are updated.

	1. Attach this script to an empty GameObject, or whatever game object you want to manage
		[Salsa_OnTalkStatusChanged] event-based waypoint movements.
	2. Create [Empty] GameObjects in your scene, these are used as waypoints. It makes things
		easier to manage if you name them waypoint0, waypoint1, etc., and parent them to the 
		[CM_SalsaWaypoints] GameObject.
	3. Set the [Waypoints] [Size] property to match the number of waypoint you created, and link
		waypoint0 to [Waypoints][Element0], waypoint1 to [Waypoints][Element1], etc.
	4. Set the [Triggers] [Size] property to the number of audio clip-based waypoint changes
		you want to make.
	5. Inside each [Triggers] element, you will find the following:
		Trigger: Trigger this waypoint at the [Start] or [End] of an audio clip.
		Audio Clip: The audio clip you want to trigger this waypoint change.
		Movement Speed: How fast you want the target to move.
		Waypoint Index: The waypoint [Element], inside the [Waypoints] array, you want to move towards.
	6. Set the [Target] you want to animate through the waypoints.
	7. The [Starting Waypoint] is a [Waypoints] [Element] index value (Which waypoint to start at)
	8. [Current Waypoint] lets you know, in the inspector, what the current waypoint index is set to.
	9. [Match Waypoint Ration] lets your [Target] move towards the same rotation as the waypoints rotation.
*/

// Properties class for storing waypoint and waypoint trigger information
public class CM_SalsaWaypointTriggers {
	public enum Trigger { Start, End }
	public var trigger : Trigger = Trigger.Start;
	public var audioClip : AudioClip;
	public var movementSpeed : float = 10; // Movement speed
	public var waypointIndex : int;
}

/* A simple waypoints script to allow waypoint movement 
 triggered by Salsa_OnTalkStatusChanged events
*/
public class CM_SalsaWaypoints extends MonoBehaviour {
	public var target : GameObject; // The object you wish to move
	public var startingWaypoint : int; // Current waypoint index
	public var currentWaypoint : int; // Current waypoint index
	public var matchWaypointRotation : boolean;
	public var triggers : CM_SalsaWaypointTriggers[];
	public var waypoints : GameObject[]; // Array of waypoints

	private var movementSpeed : float = 10f; // Movement speed

	/* On start, move the tartet to the first waypoint position, 
	 then set the currentWaypoint to index 1 to being moving 
	 towards the next waypoint
	*/
	function Start() {
		target.transform.position = waypoints[currentWaypoint].transform.position;
		currentWaypoint = startingWaypoint;
	}

	// Move the target towards the current waypoint index
	function Update () {
		target.transform.position = Vector3.MoveTowards(
			target.transform.position, waypoints[currentWaypoint].transform.position, Time.deltaTime * movementSpeed);

		if (matchWaypointRotation) {
			target.transform.rotation = Quaternion.RotateTowards( 
		        target.transform.rotation, waypoints[currentWaypoint].transform.rotation, Time.deltaTime * movementSpeed);
		}
	}

	// Method is called by SALSA broadcast when the talk status has changed
	function Salsa_OnTalkStatusChanged(status : CrazyMinnow.SALSA.SalsaStatus) {
		for (var i : int = 0; i < triggers.Length; i++) {
			if (triggers[i].trigger == CM_SalsaWaypointTriggers.Trigger.Start && status.isTalking) {
				if (triggers[i].audioClip.name == status.clipName) {
					movementSpeed = triggers[i].movementSpeed;
					SetSpeed(movementSpeed);
					SetWaypoint(triggers[i].waypointIndex);
				}
			}

			if (triggers[i].trigger == CM_SalsaWaypointTriggers.Trigger.End && !status.isTalking) {
				if (triggers[i].audioClip.name == status.clipName) {
					movementSpeed = triggers[i].movementSpeed;
					SetSpeed(movementSpeed);
					SetWaypoint(triggers[i].waypointIndex);
				}
			}
		}
	}

	// Set the waypoint index to a valid index in the waypoints array
	public function SetWaypoint(index : int) {
		if (index < this.waypoints.Length) {
			this.currentWaypoint = index;
		}
	}

	// Set the movement speed
	public function SetSpeed(speed : float) {
		this.movementSpeed = speed;
	}

	public function ResetSalsaWaypoints() {
		currentWaypoint = startingWaypoint;
		target.transform.position = waypoints[0].transform.position;
	}
}