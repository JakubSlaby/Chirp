using NUnit.Framework;
using UnityEngine;
using WhiteSparrow.Shared.Logging.Core;

namespace WhiteSparrow.Shared.Logging.Tests
{
	/// <summary>
	/// Pins LoggingMarkdownUtil.Parse output byte-for-byte against the frozen v0.12.1
	/// implementation (LegacyMarkdownParser). Any intentional output change must be
	/// reflected in the legacy copy and called out in the CHANGELOG.
	/// Known deliberate divergence: code blocks whose content is a bare JSON scalar
	/// (e.g. "1.50" or 'single-quoted') are no longer round-tripped through the JSON
	/// serializer — only blocks starting with '{' or '[' are pretty-printed. Such
	/// scalar blocks are therefore excluded from this corpus.
	/// </summary>
	public class MarkdownGoldenTests
	{
		private static ChirpStyle CreateStyle() => new ChirpStyle
		{
			MarkdownInlineCodeColor = new Color(3 / 255f, 252 / 255f, 132 / 255f),
			MarkdownBlockCodeColor = new Color(3 / 255f, 252 / 255f, 132 / 255f)
		};

		private static readonly string[] s_Corpus =
		{
			"",
			"plain text without special characters",
			"text with * a stray asterisk pair * around words",
			"a_b_c underscores mid-word",
			"**bold**",
			"middle **bold** text",
			"*italic*",
			"_italic_",
			"__bold__",
			"___bolditalic___",
			"***bolditalic***",
			"`inline` code",
			"combo **bold** and *italic* and `code`",
			"# Header",
			"## Sub Header\nWith body text",
			"# One\n## Two\n### Three\n#### Four\n##### Five\n###### Six",
			"**bold with *italic* inside**",
			"*italic with `code` inside*",
			"[c:#ff0000]red[/c] then [s:20]big[/s] end",
			"[c:red]named color[/c]",
			"```\nSome other text about anything else\nsecond line\n```",
			"```My Title\nplain block content\n```",
			"```   \nblock with whitespace-only title\n```",
			"```Some Json\n{\"fruit\":\"Apple\",\"size\":\"Large\",\"color\":\"Red\"}\n```",
			"```\n[1,2,3]\n```",
			"```json\n  {\"indented\": true}\n```",
			"```Broken Json\n{not actually json}\n```",
			"\r\nwindows **line** endings\r\nnext line\r\n",
			"# Header then ```\ncode\n``` after",
			"text before\n```\nblock in the middle\n```\ntext after with `inline`"
		};

		[Test]
		public void Parse_MatchesLegacyImplementation()
		{
			var style = CreateStyle();
			foreach (var input in s_Corpus)
			{
				string expected = LegacyMarkdownParser.Parse(input, style);
				string actual = LoggingMarkdownUtil.Parse(input, style);
				Assert.AreEqual(expected, actual, "Output diverged from the v0.12.1 parser for input:\n{0}", input);
			}
		}

		[Test]
		public void Parse_MatchesLegacyImplementation_DefaultStyle()
		{
			var style = Chirp.Style;
			foreach (var input in s_Corpus)
			{
				string expected = LegacyMarkdownParser.Parse(input, style);
				string actual = LoggingMarkdownUtil.Parse(input, style);
				Assert.AreEqual(expected, actual, "Output diverged from the v0.12.1 parser for input:\n{0}", input);
			}
		}

		// Literal anchors independent of the legacy copy — these lock the actual expected
		// rich-text output for the simple cases, so a bug replicated into both parsers
		// cannot slip through the comparison tests.
		[Test]
		public void Parse_LiteralAnchors()
		{
			var style = CreateStyle();
			Assert.AreEqual("plain text", LoggingMarkdownUtil.Parse("plain text", style));
			Assert.AreEqual("<b>bold</b>", LoggingMarkdownUtil.Parse("**bold**", style));
			Assert.AreEqual("<i>italic</i>", LoggingMarkdownUtil.Parse("*italic*", style));
			Assert.AreEqual("<i><b>both</b></i>", LoggingMarkdownUtil.Parse("***both***", style));
			Assert.AreEqual("<size=21><b> Title</b></size>", LoggingMarkdownUtil.Parse("# Title", style));
			Assert.AreEqual("<size=19><b> Title</b></size>", LoggingMarkdownUtil.Parse("## Title", style));
			Assert.AreEqual("<color=#03FC84>code</color>", LoggingMarkdownUtil.Parse("`code`", style));
			Assert.AreEqual("<color=#ff0000>red</color>", LoggingMarkdownUtil.Parse("[c:#ff0000]red[/c]", style));
			Assert.AreEqual("<size=20>big</size>", LoggingMarkdownUtil.Parse("[s:20]big[/s]", style));
		}
	}
}
