﻿using System;
using System.Collections.Generic;
using System.Linq;
using IAintBuildingThat.HarmonyPatches;
using RimWorld;
using UnityEngine;
using Verse;

namespace IAintBuildingThat
{
	public class Settings : ModSettings
	{
		private const float RowHeight = 60f;
		public HashSet<string> HiddenBuildables = [];
		private Vector2 _scrollPosition = Vector2.zero;
		private string _searchQuery = string.Empty;
		private Dictionary<string, Lazy<BuildableDef>> cachedDefs = new();
		internal List<CompAbilityHide> AllAbilityHideComponents = new List<CompAbilityHide>(32);

		private enum Page : byte
		{
			Buildings, Abilities
		}
		private Page _page = Page.Buildings;

		public void HideBuildable(BuildableDef buildable)
		{
			PatchInHideMenuOptionToDesignatorProcessInput.IsRightClicking = false;
			if (buildable == null) return;
			HiddenBuildables.Add(buildable.defName);
			cachedDefs.SetOrAdd(buildable.defName, new Lazy<BuildableDef>(() => buildable));
			Write();
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

			if (_page == Page.Buildings && options.ButtonText("Switch to Abilities Page"))
				_page = Page.Abilities;
			else if (_page == Page.Abilities && options.ButtonText("Switch to Buildings Page"))
				_page = Page.Buildings;

			switch (_page)
			{
				case Page.Buildings:
					{
						if (options.ButtonText("Restore All Hidden Defs"))
						{
							HiddenBuildables.Clear();
						}

						options.Gap();

						// Add a TextField for the search query
						string lastSearch = _searchQuery;
						float searchBarY = options.CurHeight;
						Widgets.Label(new Rect(0, searchBarY, 120, 30f), "Taggerung_IAintBuildingThat_SearchLabel".Translate());
						_searchQuery = Widgets.TextField(new Rect(130, searchBarY, wrect.width - 130, 30f), _searchQuery);
						if (lastSearch != _searchQuery) _scrollPosition = Vector2.zero;

						options.Gap();

						// Filter the current list
						List<string> filteredBuildables = HiddenBuildables.Where(MatchesFilter).ToList();

						// Create a scrollable list
						Rect viewRect = new(0, options.CurHeight, wrect.width - 16f, filteredBuildables.Count * RowHeight);
						Rect scrollRect = new(0, options.CurHeight + 30f, wrect.width, wrect.height - 80);
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
							if (Widgets.ButtonText(new Rect(rowRect.x + 10, rowRect.y, rowRect.width - 20f, rowRect.height), $"{"Taggerung_IAintBuildingThat_RestoreText".Translate()} {buildable}"))
							{
								HiddenBuildables.Remove(buildable);
								break; // break to avoid collection modified exception
							}

							num++;
						}

						Widgets.EndScrollView();
					} break ;
				case Page.Abilities:
					{
						if (options.ButtonText("Restore All Hidden Defs"))
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

						var query = _searchQuery.Length < 1 ? AllAbilityHideComponents.Where(c => c.hidden) : 
							AllAbilityHideComponents.Where(c => c.hidden && (c.parent.def?.LabelCap.Resolve()?.ToLowerInvariant()?.Contains(_searchQuery.ToLowerInvariant()) == true ||
								c.parent.pawn?.Name?.ToStringFull?.ToLowerInvariant()?.Contains(_searchQuery.ToLowerInvariant()) == true));

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

							Rect rowRect = new Rect(rowSectionWidth*2 + 10, num * RowHeight, rowSectionWidth, RowHeight - 5f);
							if (Widgets.ButtonText(new Rect(rowRect.x + 10, rowRect.y, rowRect.width - 20f, rowRect.height), $"{"Taggerung_IAintBuildingThat_RestoreText".Translate()} {ability.def.label}"))
							{
								abilityComp.hidden = false;
								break;
							}

							num++;
						}

						Widgets.EndScrollView();

					} break;
			}

			options.End();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref HiddenBuildables, "hiddenBuildables", LookMode.Value);
		}
	}
}
