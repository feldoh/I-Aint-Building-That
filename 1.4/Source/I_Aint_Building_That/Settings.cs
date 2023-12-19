using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace IAintBuildingThat
{
	public class Settings : ModSettings
	{
		public HashSet<string> HiddenBuildables = new();
		private Vector2 _scrollPosition = Vector2.zero;
		private string _searchQuery = string.Empty;

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
			_searchQuery = Widgets.TextField(new Rect(0, wrect.y, wrect.width, 30f), _searchQuery);
			if (lastSearch != _searchQuery) _scrollPosition = Vector2.zero;

			options.Gap();

			// Filter the current list
			List<string> filteredBuildables = HiddenBuildables.Where(buildable => _searchQuery.Length < 1 || buildable.ToLowerInvariant().Contains(_searchQuery.ToLowerInvariant()))
				.ToList();

			// Create a scrollable list
			Rect viewRect = new(0, 0, wrect.width - 16f, filteredBuildables.Count * 35f);
			Rect scrollRect = new(0, 80f, wrect.width, wrect.height - 80f);
			Widgets.BeginScrollView(scrollRect, ref _scrollPosition, viewRect);

			float num = 0f;
			foreach (string buildable in filteredBuildables)
			{
				if (_searchQuery.Length >= 1 && !buildable.ToLowerInvariant().Contains(_searchQuery.ToLowerInvariant())) continue;
				Rect rowRect = new(0f, num * 35f, viewRect.width, 30f);
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
