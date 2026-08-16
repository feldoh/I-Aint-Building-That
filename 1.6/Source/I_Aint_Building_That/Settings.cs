using System;
using System.Collections.Generic;
using System.Linq;
using IAintBuildingThat.HarmonyPatches;
using UnityEngine;
using Verse;

namespace IAintBuildingThat
{
	public class Settings : ModSettings
	{
		private const float RowHeight = 60f;
		public HashSet<string> HiddenBuildables = [];
		public bool showHiddenButtons = false;

		// Modifier combos. Defaults are chosen so existing users see no behaviour change:
		//  - reveal defaults to the legacy Ctrl+Alt hold
		//  - the menu combos default to unset, meaning the hide menu is always shown on right-click.
		//    Setting a combo opts in to "only show the menu while this combo is held", so a plain
		//    right-click passes through to the game / other mods (e.g. mechanoid turret auto-use toggles).
		public ModifierCombo revealHiddenCombo = ModifierCombo.DefaultReveal();
		public ModifierCombo buildablesMenuCombo = new();
		public ModifierCombo abilitiesMenuCombo = new();

		private Vector2 _scrollPosition = Vector2.zero;
		private string _searchQuery = string.Empty;
		private Dictionary<string, Lazy<BuildableDef>> cachedDefs = new();
		internal List<CompAbilityHide> AllAbilityHideComponents = new(32);

		private enum Page : byte
		{
			Buildings,
			Abilities,
			General
		}

		private Page _page = Page.Buildings;

		public void HideBuildable(BuildableDef buildable)
		{
			PatchInHideMenuOptionToDesignatorProcessInput.IsRightClicking = false;
			if (buildable == null) return;
			HiddenBuildables.Add(buildable.defName);
			cachedDefs.SetOrAdd(buildable.defName, new Lazy<BuildableDef>(() => buildable));
			Write();
			buildable.designationCategory.DirtyCache();
		}

		public void RestoreBuildable(BuildableDef buildable)
		{
			if (buildable == null) return;
			HiddenBuildables.Remove(buildable.defName);
			buildable.designationCategory.DirtyCache();
		}

		public BuildableDef LookupDef(string buildable)
		{
			cachedDefs.TryGetValue(buildable, out Lazy<BuildableDef> cachedDef);
			if (cachedDef != null) return cachedDef.Value;
			Lazy<BuildableDef> lazyDef = new(() => DefDatabase<BuildableDef>.GetNamed(buildable, false));
			cachedDefs.SetOrAdd(buildable, lazyDef);
			return lazyDef.Value;
		}

		public bool MatchesFilter(string buildable) =>
			_searchQuery.Length < 1 ||
			buildable.ToLowerInvariant().Contains(_searchQuery.ToLowerInvariant()) ||
			(LookupDef(buildable) is { } def && def.LabelCap.Resolve().ToLowerInvariant().Contains(_searchQuery.ToLowerInvariant()));

		public void DoWindowContents(Rect wrect)
		{
			Listing_Standard options = new();
			options.Begin(wrect);

			// Define the tabs
			List<TabRecord> tabs =
			[
				new("Taggerung_IAintBuildingThat_BuildingsTabText".Translate(), () => _page = Page.Buildings, _page == Page.Buildings),
				new("Taggerung_IAintBuildingThat_AbilitiesTabText".Translate(), () => _page = Page.Abilities, _page == Page.Abilities),
				new("Taggerung_IAintBuildingThat_GeneralTabText".Translate(), () => _page = Page.General, _page == Page.General)
			];

			// Draw the tabs
			TabDrawer.DrawTabs(wrect, tabs);
			options.Gap(50);

			switch (_page)
			{
				case Page.General:
				{
					options.CheckboxLabeled("Taggerung_IAintBuildingThat_ShowHiddenButtonsLabel".Translate(),
						ref showHiddenButtons, "Taggerung_IAintBuildingThat_ShowHiddenButtonsTooltip".Translate());

					options.GapLine();

					DrawComboRow(options, "Taggerung_IAintBuildingThat_RevealComboLabel".Translate(),
						"Taggerung_IAintBuildingThat_RevealComboTooltip".Translate(), revealHiddenCombo);
					DrawComboRow(options, "Taggerung_IAintBuildingThat_BuildingsComboLabel".Translate(),
						"Taggerung_IAintBuildingThat_MenuComboTooltip".Translate(), buildablesMenuCombo);
					DrawComboRow(options, "Taggerung_IAintBuildingThat_AbilitiesComboLabel".Translate(),
						"Taggerung_IAintBuildingThat_MenuComboTooltip".Translate(), abilitiesMenuCombo);
				}
					break;
				case Page.Buildings:
				{
					if (options.ButtonText("Taggerung_IAintBuildingThat_RestoreAllButtonText".Translate()))
					{
						foreach (var hiddenBuildable in cachedDefs)
						{
							hiddenBuildable.Value?.Value?.designationCategory?.DirtyCache();
						}

						HiddenBuildables.Clear();
					}

					options.Gap();

					// Add a TextField for the search query
					string lastSearch = _searchQuery;
					Rect searchRect = options.GetRect(30f);
					Widgets.Label(searchRect.LeftPart(0.25f), "Taggerung_IAintBuildingThat_SearchLabel".Translate());
					_searchQuery = Widgets.TextField(searchRect.RightPart(0.75f), _searchQuery);
					if (lastSearch != _searchQuery) _scrollPosition = Vector2.zero;

					options.Gap();

					// Filter the current list
					List<string> filteredBuildables = HiddenBuildables.Where(MatchesFilter).ToList();

					// Create a scrollable list
					Rect scrollRect = new(0, options.CurHeight + 10f, wrect.width, wrect.height - 80);
					Rect viewRect = new(0, 0, wrect.width - 10, filteredBuildables.Count * RowHeight);
					Widgets.BeginScrollView(scrollRect, ref _scrollPosition, viewRect);

					float num = 0f;
					foreach (string buildable in filteredBuildables)
					{
						BuildableDef def = LookupDef(buildable);
						float rowSectionWidth = (viewRect.width - 20) / 2;
						if (def != null)
						{
							Widgets.DefLabelWithIcon(new Rect(0f, num * RowHeight, rowSectionWidth, RowHeight - 5f), def);
						}

						Rect rowRect = def == null
							? new Rect(0, num * RowHeight, viewRect.width, RowHeight - 5f)
							: new Rect(rowSectionWidth + 10, num * RowHeight, rowSectionWidth, RowHeight - 5f);
						if (Widgets.ButtonText(new Rect(rowRect.x + 10, rowRect.y, rowRect.width - 20f, rowRect.height),
							    $"{"Taggerung_IAintBuildingThat_RestoreText".Translate()} {buildable}"))
						{
							HiddenBuildables.Remove(buildable);
							if (cachedDefs.TryGetValue(buildable, out var buildableDef)) buildableDef.Value?.designationCategory?.DirtyCache();
							break; // break to avoid collection modified exception
						}

						num++;
					}

					Widgets.EndScrollView();
				}
					break;
				case Page.Abilities:
				{
					if (options.ButtonText("Taggerung_IAintBuildingThat_RestoreAllButtonText".Translate()))
					{
						AllAbilityHideComponents.ForEach(c => c.hidden = false);
					}

					options.Gap();

					if (Current.Game == null || Current.Game.World == null) break;

					// Add a TextField for the search query
					string lastSearch = _searchQuery;
					float searchBarY = options.CurHeight;
					Widgets.Label(new Rect(0, searchBarY, 120, 30f), "Taggerung_IAintBuildingThat_SearchLabel".Translate());
					_searchQuery = Widgets.TextField(new Rect(130, searchBarY, wrect.width - 130, 30f), _searchQuery);
					if (lastSearch != _searchQuery) _scrollPosition = Vector2.zero;

					options.Gap();

					var query = (_searchQuery.Length < 1
						? AllAbilityHideComponents.Where(c => c.hidden)
						: AllAbilityHideComponents.Where(c => c.hidden && (c.parent.def?.LabelCap.Resolve()?.ToLowerInvariant()?.Contains(_searchQuery.ToLowerInvariant()) == true ||
						                                                   c.parent.pawn?.Name?.ToStringFull?.ToLowerInvariant()?.Contains(_searchQuery.ToLowerInvariant()) == true))).ToList();

					if (query.EnumerableNullOrEmpty()) break;

					// Create a scrollable list
					Rect viewRect = new(0, 0, wrect.width - 16f, query.Count() * RowHeight);
					Rect scrollRect = new(0, options.CurHeight + 30f, wrect.width, wrect.height - 80);
					Widgets.BeginScrollView(scrollRect, ref _scrollPosition, viewRect);

					float num = 0f;
					foreach (var abilityComp in query)
					{
						var ability = abilityComp.parent;

						float rowSectionWidth = (viewRect.width - 20) / 3;
						Widgets.ThingIcon(new Rect(0f, num * RowHeight, rowSectionWidth, RowHeight - 5f), ability.pawn);
						Widgets.DefLabelWithIcon(new Rect(rowSectionWidth, num * RowHeight, rowSectionWidth, RowHeight - 5f), ability.def);

						Rect rowRect = new Rect(rowSectionWidth * 2 + 10, num * RowHeight, rowSectionWidth, RowHeight - 5f);
						if (Widgets.ButtonText(new Rect(rowRect.x + 10, rowRect.y, rowRect.width - 20f, rowRect.height),
							    $"{"Taggerung_IAintBuildingThat_RestoreText".Translate()} {ability.def.label}"))
						{
							abilityComp.hidden = false;
							break;
						}

						num++;
					}

					Widgets.EndScrollView();
				}
					break;
			}

			options.End();
		}

		private static void DrawComboRow(Listing_Standard listing, string label, string tooltip, ModifierCombo combo)
		{
			Rect row = listing.GetRect(30f);
			if (Mouse.IsOver(row)) Widgets.DrawHighlight(row);
			if (!tooltip.NullOrEmpty()) TooltipHandler.TipRegion(row, tooltip);

			Widgets.Label(row.LeftPart(0.55f), label);

			const float btnWidth = 64f;
			const float btnGap = 6f;
			const float btnHeight = 28f;
			float x = row.xMax - (btnWidth * 3 + btnGap * 2);
			float y = row.y + (row.height - btnHeight) / 2f;
			combo.shift = DrawModToggle(new Rect(x, y, btnWidth, btnHeight), "Taggerung_IAintBuildingThat_ModShift".Translate(), combo.shift);
			x += btnWidth + btnGap;
			combo.ctrl = DrawModToggle(new Rect(x, y, btnWidth, btnHeight), "Taggerung_IAintBuildingThat_ModCtrl".Translate(), combo.ctrl);
			x += btnWidth + btnGap;
			combo.alt = DrawModToggle(new Rect(x, y, btnWidth, btnHeight), "Taggerung_IAintBuildingThat_ModAlt".Translate(), combo.alt);

			listing.Gap(6f);
		}

		private static bool DrawModToggle(Rect rect, string label, bool value)
		{
			if (value) Widgets.DrawHighlightSelected(rect);
			if (Widgets.ButtonText(rect, label)) value = !value;
			return value;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref HiddenBuildables, "hiddenBuildables", LookMode.Value);
			Scribe_Values.Look(ref showHiddenButtons, "showHiddenButtons", false);
			Scribe_Deep.Look(ref revealHiddenCombo, "revealHiddenCombo");
			Scribe_Deep.Look(ref buildablesMenuCombo, "buildablesMenuCombo");
			Scribe_Deep.Look(ref abilitiesMenuCombo, "abilitiesMenuCombo");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				// Missing nodes (e.g. upgrading from an older config) load as null. Restore the
				// behaviour-preserving defaults: legacy reveal combo, menu combos unset.
				revealHiddenCombo ??= ModifierCombo.DefaultReveal();
				buildablesMenuCombo ??= new ModifierCombo();
				abilitiesMenuCombo ??= new ModifierCombo();
			}
		}
	}
}
