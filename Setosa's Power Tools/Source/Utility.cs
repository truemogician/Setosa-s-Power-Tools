using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace Setosa;

public static class Logger {
	internal static bool Enabled =
#if DEBUG
		true;
#else
		false;
#endif

	private const string LogPrefix = $"[{ThisAssembly.Info.Title}] ";

	public static void Message(string text) => Log(Verse.Log.Message, text);

	public static void Warning(string text, bool once = false) {
		if (once)
			LogOnce(Verse.Log.WarningOnce, text, true);
		else
			Log(Verse.Log.Warning, text, true);
    }

	public static void Error(string text, bool once = false) {
		if (once)
			LogOnce(Verse.Log.ErrorOnce, text, true);
		else
			Log(Verse.Log.Error, text, true);
    }

    private static void Log(Action<string> method, string text, bool important = false) {
		if (Enabled || important)
			method(LogPrefix + text);
    }

	private static void LogOnce(Action<string, int> method, string text, bool important = false) {
		if (Enabled || important)
			method(LogPrefix + text, text.GetHashCode());
	}
}

public class ScrollView {
	private Vector2 _position = Vector2.zero;

	private Listing_Standard? _listing;

	public float ScrollBarWidth { get; init; } = 16f;

	public bool ShowScrollBar { get; init; } = true;

    public Listing_Standard Begin(Rect inRect, float height) {
		var viewRect = new Rect(0f, 0f, inRect.width - ScrollBarWidth, height);
		Widgets.BeginScrollView(inRect, ref _position, viewRect, ShowScrollBar);
		_listing = new Listing_Standard();
		_listing.Begin(viewRect);
		return _listing;
	}

    public void End() {
		if (_listing is null)
			throw new InvalidOperationException("Begin needs to be called before End");
		_listing.End();
		_listing = null;
		Widgets.EndScrollView();
    }
}

internal class TransientStyle: IDisposable {
	private readonly TextAnchor? _oldAnchor;

	private readonly GameFont? _oldFont;

	internal TextAnchor Anchor {
		init {
			_oldAnchor = Text.Anchor;
			Text.Anchor = value;
        }
	}

	internal GameFont Font {
		init {
			_oldFont = Text.Font;
			Text.Font = value;
		}
    }
	
	public void Dispose() {
		if (_oldAnchor is not null)
			Text.Anchor = _oldAnchor.Value;
		if (_oldFont is not null)
			Text.Font = _oldFont.Value;
    }
}

public static class RectExtensions {
    private static readonly Regex FlexPattern = new Regex(
        @"^(?<val>\d+(?:\.\d+)?)\s*(?<unit>fr|px)?$",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
    );

	extension(Rect rect) {
		public List<Rect> FlexBox(params string[] lengths) {
			var results = new List<Rect>(lengths.Length);
			var parsedItems = new (float val, bool fr)[lengths.Length];

			float totalFixedPx = 0f;
			float totalFr = 0f;
			for (int i = 0; i < lengths.Length; i++) {
				var match = FlexPattern.Match(lengths[i]);
				if (!match.Success)
					throw new FormatException($"Invalid FlexBox length format: {lengths[i]}");
				float val = float.Parse(match.Groups["val"].Value);
				string unit = match.Groups["unit"].Value.ToLowerInvariant();
				parsedItems[i] = (val, unit == "fr");
				if (unit == "fr")
					totalFr += val;
				else
					totalFixedPx += val;
			}

			if (totalFixedPx > rect.width) {
				throw new ArgumentOutOfRangeException(
					nameof(lengths),
					$"Total fixed width ({totalFixedPx}px) exceeds the available Rect width ({rect.width}px)."
				);
			}

			float availableSpace = rect.width - totalFixedPx;
			float pxPerFr = totalFr > 0 ? availableSpace / totalFr : 0;
			float currentX = rect.x;
			foreach (var item in parsedItems) {
				float width = item.fr ? item.val * pxPerFr : item.val;
				results.Add(new Rect(currentX, rect.y, width, rect.height));
				currentX += width;
			}

			return results;
		}
    }
}