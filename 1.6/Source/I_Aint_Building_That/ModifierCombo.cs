using UnityEngine;
using Verse;

namespace IAintBuildingThat;

/// <summary>
/// A user-configurable set of held modifier keys (Shift / Ctrl / Alt).
/// Left and right variants of each modifier are treated as equivalent.
/// </summary>
public class ModifierCombo : IExposable
{
	public bool shift;
	public bool ctrl;
	public bool alt;

	public ModifierCombo()
	{
	}

	public ModifierCombo(bool shift, bool ctrl, bool alt)
	{
		this.shift = shift;
		this.ctrl = ctrl;
		this.alt = alt;
	}

	/// <summary>The legacy reveal combo (Ctrl + Alt) used as the default so existing users are unaffected.</summary>
	public static ModifierCombo DefaultReveal() => new(false, true, true);

	/// <summary>True when at least one modifier is part of this combo.</summary>
	public bool RequiresModifier => shift || ctrl || alt;

	/// <summary>True when every modifier that is part of this combo is currently held down.</summary>
	private bool AllHeld =>
		(!shift || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
		(!ctrl || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
		(!alt || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));

	/// <summary>
	/// Whether the IBT right-click menu should be offered. When no modifier is configured the menu is
	/// always shown (legacy behaviour); once a modifier is configured it is only shown while that
	/// modifier is held, letting a plain right-click fall through to the game or another mod.
	/// </summary>
	public bool ShouldShowMenu => !RequiresModifier || AllHeld;

	/// <summary>
	/// Whether hidden things should be temporarily revealed. Requires an explicit modifier so that
	/// clearing the combo simply disables hold-to-reveal rather than revealing everything.
	/// </summary>
	public bool RevealActive => RequiresModifier && AllHeld;

	public void ExposeData()
	{
		Scribe_Values.Look(ref shift, "shift");
		Scribe_Values.Look(ref ctrl, "ctrl");
		Scribe_Values.Look(ref alt, "alt");
	}
}
