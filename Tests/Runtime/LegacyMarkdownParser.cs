using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WhiteSparrow.Shared.Logging.Core;

namespace WhiteSparrow.Shared.Logging.Tests
{
	/// <summary>
	/// Frozen copy of the LoggingMarkdownUtil parsing algorithm as of v0.12.1, kept as the
	/// byte-for-byte reference for MarkdownGoldenTests. Do not optimise or otherwise modify
	/// this file — its whole purpose is to preserve the original output.
	/// </summary>
	internal static class LegacyMarkdownParser
	{
		private static readonly Regex s_CombinedRegex = new Regex(@"(^(?'h'#+)(?'header'.+))|(?'blockW'(?<!`)```(?!`)(?'blockTitle'.+)?[\r\n](?'block'(?:(?!```)[\s\S])+)[\r\n](?<!`)```(?!`))|(?'inlineW'(?<!`)`(?!`)(?'inline'.+?)(?<!`)`(?!`))|(?'ibW'(?:(?<!\*)\*\*\*(?!\*)(?'ib'.+?)(?<!\*)\*\*\*(?!\*)|(?<!_)___(?!_)(?'ib'.+?)(?<!_)___(?!_)))|(?'bW'(?:(?<!\*)\*\*(?!\*)(?'b'.+?)(?<!\*)\*\*(?!\*)|(?<!_)__(?!_)(?'b'.+?)(?<!_)__(?!_)))|(?'iW'(?:(?<!\*)\*(?!\*)(?'i'.+?)(?<!\*)\*(?!\*)|(?<!_)_(?!_)(?'i'.+?)(?<!_)_(?!_)))|(\[c:(?'colorS'[a-zA-Z0-9#]+)\])|(?'colorE'\[\/c\])|(\[s:(?'sizeS'[a-zA-Z0-9#]+)\])|(?'sizeE'\[\/s\])", RegexOptions.Compiled | RegexOptions.Multiline);

		public static string Parse(string input, ChirpStyle style) => RecursiveParse(input, style);

		public static string RecursiveParse(string input, ChirpStyle style)
		{
			var matches = s_CombinedRegex.Matches(input);
			if (matches.Count == 0)
				return input;

			var sb = new StringBuilder();
			sb.Append(input);
			int offset = 0;

			for (int i = 0; i < matches.Count; i++)
			{
				var match = matches[i];
				if (match.Groups["inlineW"].Success)
				{
					ProcessInlineElement(sb, match, style, ref offset);
					continue;
				}
				if (match.Groups["blockW"].Success)
				{
					ProcessBlockElement(sb, match, style, ref offset);
					continue;
				}
				if (match.Groups["ibW"].Success)
				{
					ProcessItalicBold(sb, match, style, ref offset);
					continue;
				}
				if (match.Groups["iW"].Success)
				{
					ProcessItalic(sb, match, style, ref offset);
					continue;
				}
				if (match.Groups["bW"].Success)
				{
					ProcessBold(sb, match, style, ref offset);
					continue;
				}

				if (match.Groups["h"].Success)
				{
					ProcessHeader(sb, match, style, ref offset);
					continue;
				}

				if (match.Groups["colorS"].Success)
				{
					ProcessColorStart(sb, match, style, ref offset);
					continue;
				}
				if (match.Groups["colorE"].Success)
				{
					ProcessColorEnd(sb, match, style, ref offset);
					continue;
				}

				if (match.Groups["sizeS"].Success)
				{
					ProcessSizeStart(sb, match, style, ref offset);
					continue;
				}
				if (match.Groups["sizeE"].Success)
				{
					ProcessSizeEnd(sb, match, style, ref offset);
					continue;
				}
			}

			return sb.ToString();
		}

		private static void ProcessHeader(StringBuilder sb, Match match, ChirpStyle style, ref int offset)
		{
			var h = match.Groups["h"];
			var header = RecursiveParse(match.Groups["header"].Value, style);

			int hashCount = h.Length;
			int size = Mathf.CeilToInt(Mathf.Clamp(5 - hashCount, 0, 4) * 2 + 13);

			string replace = $"<size={size}><b>{header}</b></size>";

			sb.Remove(match.Index + offset, match.Length);
			sb.Insert(match.Index + offset, replace);

			offset += replace.Length - match.Length;
		}

		private static void ProcessBlockElement(StringBuilder sb, Match match, ChirpStyle style, ref int offset)
		{
			var capture = match.Groups["block"];

			string value = capture.Value.Trim('\r', '\n');

			try
			{
				var json = JValue.Parse(value).ToString(Formatting.Indented);
				value = json;
			}
			catch (Exception)
			{
				// ignored
			}

			var replace = string.Empty;
			var color = style.MarkdownBlockCodeColorHtml;
			if (color != null)
				replace += $"<color=#{color}>";

			string title = match.Groups["blockTitle"].Success ? match.Groups["blockTitle"].Value.Trim() : null;
			if (!string.IsNullOrWhiteSpace(title))
			{
				replace += "<b>" + title + "</b>\r\n";
			}

			replace += value;
			if (color != null)
				replace += "</color>";

			sb.Remove(match.Index + offset, match.Length);
			sb.Insert(match.Index + offset, replace);

			offset += replace.Length - match.Length;
		}

		private static void ProcessInlineElement(StringBuilder sb, Match match, ChirpStyle style, ref int offset)
		{
			var capture = match.Groups["inline"];
			var replace = string.Empty;
			var color = style.MarkdownInlineCodeColorHtml;
			if (color != null)
				replace += $"<color=#{color}>";
			replace += capture.Value;
			if (color != null)
				replace += "</color>";

			sb.Remove(match.Index + offset, match.Length);
			sb.Insert(match.Index + offset, replace);

			offset += replace.Length - match.Length;
		}

		private static void ProcessItalicBold(StringBuilder sb, Match match, ChirpStyle style, ref int offset)
		{
			var capture = RecursiveParse(match.Groups["ib"].Value, style);
			var replace = $"<i><b>{capture}</b></i>";
			sb.Remove(match.Index + offset, match.Length);
			sb.Insert(match.Index + offset, replace);
			offset += replace.Length - match.Length;
		}

		private static void ProcessItalic(StringBuilder sb, Match match, ChirpStyle style, ref int offset)
		{
			var capture = RecursiveParse(match.Groups["i"].Value, style);
			var replace = $"<i>{capture}</i>";
			sb.Remove(match.Index + offset, match.Length);
			sb.Insert(match.Index + offset, replace);
			offset += replace.Length - match.Length;
		}

		private static void ProcessBold(StringBuilder sb, Match match, ChirpStyle style, ref int offset)
		{
			var capture = RecursiveParse(match.Groups["b"].Value, style);
			var replace = $"<b>{capture}</b>";
			sb.Remove(match.Index + offset, match.Length);
			sb.Insert(match.Index + offset, replace);
			offset += replace.Length - match.Length;
		}

		private static void ProcessSizeStart(StringBuilder sb, Match match, ChirpStyle style, ref int offset)
		{
			var size = match.Groups["sizeS"];
			var replace = $"<size={size.Value}>";
			sb.Remove(match.Index + offset, match.Length);
			sb.Insert(match.Index + offset, replace);
			offset += replace.Length - match.Length;
		}

		private const string c_SizeEndTag = "</size>";

		private static void ProcessSizeEnd(StringBuilder sb, Match match, ChirpStyle style, ref int offset)
		{
			sb.Remove(match.Index + offset, match.Length);
			sb.Insert(match.Index + offset, c_SizeEndTag);
			offset += c_SizeEndTag.Length - match.Length;
		}

		private static void ProcessColorStart(StringBuilder sb, Match match, ChirpStyle style, ref int offset)
		{
			var color = match.Groups["colorS"];
			var replace = $"<color={color.Value}>";
			sb.Remove(match.Index + offset, match.Length);
			sb.Insert(match.Index + offset, replace);
			offset += replace.Length - match.Length;
		}

		private const string c_ColorEndTag = "</color>";

		private static void ProcessColorEnd(StringBuilder sb, Match match, ChirpStyle style, ref int offset)
		{
			sb.Remove(match.Index + offset, match.Length);
			sb.Insert(match.Index + offset, c_ColorEndTag);
			offset += c_ColorEndTag.Length - match.Length;
		}
	}
}
