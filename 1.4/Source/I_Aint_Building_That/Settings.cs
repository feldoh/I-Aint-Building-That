﻿using System;
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
		private Vector2 _scrollPosition = Vector2.zero;
		private string _searchQuery = string.Empty;
		private Dictionary<string, Lazy<BuildableDef>> cachedDefs = new();

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

			if (options.ButtonText("Restore All Hidden Defs"))
			{
				HiddenBuildables.Clear();
			}

			options.Gap();

			// Add a TextField for the search query
			string lastSearch = _searchQuery;
			float searchBarY = wrect.y;
			Widgets.Label(new Rect(0, searchBarY, 120, 30f), "Taggerung_IAintBuildingThat_SearchLabel".Translate());
			_searchQuery = Widgets.TextField(new Rect(130, searchBarY, wrect.width - 130, 30f), _searchQuery);
			if (lastSearch != _searchQuery) _scrollPosition = Vector2.zero;

			options.Gap();

			// Filter the current list
			List<string> filteredBuildables = HiddenBuildables.Where(MatchesFilter).ToList();

			// Create a scrollable list
			Rect viewRect = new(0, 0, wrect.width - 16f, filteredBuildables.Count * RowHeight);
			Rect scrollRect = new(0, 80f, wrect.width, wrect.height - 80f);
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

			options.End();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref HiddenBuildables, "hiddenBuildables", LookMode.Value);
		}
	}
}
