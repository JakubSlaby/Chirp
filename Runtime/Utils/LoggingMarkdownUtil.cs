using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Profiling;
using UnityEngine;
using WhiteSparrow.Shared.Logging.Core;

namespace WhiteSparrow.Shared.Logging
{
    public static class LoggingMarkdownUtil
    {
#if UNITY_EDITOR
        
        private const string s_PatternItalic =  @"(?'iW'(?:(?<!\*)\*(?!\*)(?'i'.+?)(?<!\*)\*(?!\*)|(?<!_)_(?!_)(?'i'.+?)(?<!_)_(?!_)))";
        private const string s_PatternBold =  @"(?'bW'(?:(?<!\*)\*\*(?!\*)(?'b'.+?)(?<!\*)\*\*(?!\*)|(?<!_)__(?!_)(?'b'.+?)(?<!_)__(?!_)))";
        private const string s_PatternItalicBold =  @"(?'ibW'(?:(?<!\*)\*\*\*(?!\*)(?'ib'.+?)(?<!\*)\*\*\*(?!\*)|(?<!_)___(?!_)(?'ib'.+?)(?<!_)___(?!_)))";
        private const string s_PatternCodeBlock =  @"(?'blockW'(?<!`)```(?!`)(?'blockTitle'.+)?[\r\n](?'block'(?:(?!```)[\s\S])+)[\r\n](?<!`)```(?!`))";
        private const string s_PatternCodeInline =  @"(?'inlineW'(?<!`)`(?!`)(?'inline'.+?)(?<!`)`(?!`))";
        private const string s_PatternHeader = @"(^(?'h'#+)(?'header'.+))";
        private const string s_PatternColor = @"(\[c:(?'colorS'[a-zA-Z0-9#]+)\])|(?'colorE'\[\/c\])";
        private const string s_PatternSize = @"(\[s:(?'sizeS'[a-zA-Z0-9#]+)\])|(?'sizeE'\[\/s\])";
        
        // [UnityEditor.MenuItem("Tools/White Sparrow/Chirp Logger/Development/Markdown RegEx")]
        private static void LogFullRegEx()
        {
            Debug.Log($"{s_PatternHeader}|{s_PatternCodeBlock}|{s_PatternCodeInline}|{s_PatternItalicBold}|{s_PatternBold}|{s_PatternItalic}|{s_PatternColor}|{s_PatternSize}");
        }

        // [UnityEditor.MenuItem("Tools/White Sparrow/Chirp Logger/Development/Markdown Test")]
        private static void TestMarkdown()
        {
            string s = @"# Testing Headers\n## And Sub Headers
incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum

```
Some other text about anything else
sdakdhakj
```

```Some Json
{""fruit"":""Apple"",""size"":""Large"",""color"":""Red""}
```
";
            string output = LoggingMarkdownUtil.Parse(s);
            Debug.Log(output);
        }
#endif

        [ThreadStatic] private static Stack<StringBuilder> s_StringBuilderStack;

        private static Regex s_CombinedRegex = new Regex(@"(^(?'h'#+)(?'header'.+))|(?'blockW'(?<!`)```(?!`)(?'blockTitle'.+)?[\r\n](?'block'(?:(?!```)[\s\S])+)[\r\n](?<!`)```(?!`))|(?'inlineW'(?<!`)`(?!`)(?'inline'.+?)(?<!`)`(?!`))|(?'ibW'(?:(?<!\*)\*\*\*(?!\*)(?'ib'.+?)(?<!\*)\*\*\*(?!\*)|(?<!_)___(?!_)(?'ib'.+?)(?<!_)___(?!_)))|(?'bW'(?:(?<!\*)\*\*(?!\*)(?'b'.+?)(?<!\*)\*\*(?!\*)|(?<!_)__(?!_)(?'b'.+?)(?<!_)__(?!_)))|(?'iW'(?:(?<!\*)\*(?!\*)(?'i'.+?)(?<!\*)\*(?!\*)|(?<!_)_(?!_)(?'i'.+?)(?<!_)_(?!_)))|(\[c:(?'colorS'[a-zA-Z0-9#]+)\])|(?'colorE'\[\/c\])|(\[s:(?'sizeS'[a-zA-Z0-9#]+)\])|(?'sizeE'\[\/s\])", RegexOptions.Compiled | RegexOptions.Multiline);

        // On the public entry only — nested recursion stays inside the outer sample.
        private static readonly ProfilerMarker s_ParseMarker = new ProfilerMarker("LoggingMarkdownUtil.Parse");

        public static string Parse(string input) => Parse(input, Chirp.Style);
        public static string Parse(string input, ChirpStyle style)
        {
            using var _ = s_ParseMarker.Auto();
            return RecursiveParse(input, style);
        }

        public static string RecursiveParse(string input, ChirpStyle style)
        {
            // Every markdown element requires at least one of the hint characters; a plain
            // message skips the regex entirely and returns without allocating.
            if (string.IsNullOrEmpty(input) || !ContainsMarkdownHint(input, 0, input.Length))
                return input;

            var matches = s_CombinedRegex.Matches(input);
            if (matches.Count == 0)
                return input;

            if (s_StringBuilderStack == null)
                s_StringBuilderStack = new Stack<StringBuilder>();

            if (!s_StringBuilderStack.TryPop(out var sb))
                sb = new StringBuilder();
            else
                sb.Clear();

            // Matches are ordered and non-overlapping: walk the input once, appending the gap
            // text between matches verbatim and each matched element in its transformed form.
            int cursor = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                sb.Append(input, cursor, match.Index - cursor);
                EmitElement(sb, input, match, style);
                cursor = match.Index + match.Length;
            }
            sb.Append(input, cursor, input.Length - cursor);

            string output = sb.ToString();
            s_StringBuilderStack.Push(sb);

            return output;
        }

        private static void EmitElement(StringBuilder sb, string input, Match match, ChirpStyle style)
        {
            if (match.Groups["inlineW"].Success)
            {
                EmitInlineElement(sb, input, match, style);
                return;
            }
            if (match.Groups["blockW"].Success)
            {
                EmitBlockElement(sb, input, match, style);
                return;
            }
            if (match.Groups["ibW"].Success)
            {
                EmitWrapped(sb, input, match.Groups["ib"], style, "<i><b>", "</b></i>");
                return;
            }
            if (match.Groups["iW"].Success)
            {
                EmitWrapped(sb, input, match.Groups["i"], style, "<i>", "</i>");
                return;
            }
            if (match.Groups["bW"].Success)
            {
                EmitWrapped(sb, input, match.Groups["b"], style, "<b>", "</b>");
                return;
            }
            if (match.Groups["h"].Success)
            {
                EmitHeader(sb, input, match, style);
                return;
            }
            if (match.Groups["colorS"].Success)
            {
                EmitTagWithValue(sb, input, match.Groups["colorS"], "<color=");
                return;
            }
            if (match.Groups["colorE"].Success)
            {
                sb.Append(c_ColorEndTag);
                return;
            }
            if (match.Groups["sizeS"].Success)
            {
                EmitTagWithValue(sb, input, match.Groups["sizeS"], "<size=");
                return;
            }
            if (match.Groups["sizeE"].Success)
            {
                sb.Append(c_SizeEndTag);
                return;
            }

            // No recognised group — emit the raw match unchanged.
            sb.Append(input, match.Index, match.Length);
        }

        private static bool ContainsMarkdownHint(string input, int start, int length)
        {
            int end = start + length;
            for (int i = start; i < end; i++)
            {
                switch (input[i])
                {
                    case '#':
                    case '`':
                    case '*':
                    case '_':
                    case '[':
                        return true;
                }
            }

            return false;
        }

        private static void EmitNested(StringBuilder sb, string input, Group content, ChirpStyle style)
        {
            if (!ContainsMarkdownHint(input, content.Index, content.Length))
            {
                sb.Append(input, content.Index, content.Length);
                return;
            }

            // Rare nested-markdown case: parse the captured substring standalone so anchors
            // ('^' on headers) behave exactly as they always have for nested content.
            sb.Append(RecursiveParse(content.Value, style));
        }

        private static void EmitWrapped(StringBuilder sb, string input, Group content, ChirpStyle style, string openTag, string closeTag)
        {
            sb.Append(openTag);
            EmitNested(sb, input, content, style);
            sb.Append(closeTag);
        }

        private static void EmitTagWithValue(StringBuilder sb, string input, Group value, string openTag)
        {
            sb.Append(openTag);
            sb.Append(input, value.Index, value.Length);
            sb.Append('>');
        }

        private static void EmitHeader(StringBuilder sb, string input, Match match, ChirpStyle style)
        {
            var h = match.Groups["h"];
            int hashCount = h.Length;
            int size = Mathf.CeilToInt(Mathf.Clamp(5 - hashCount, 0, 4) * 2 + 13);

            sb.Append("<size=");
            StringBuilderUtil.AppendInt(sb, size);
            sb.Append("><b>");
            EmitNested(sb, input, match.Groups["header"], style);
            sb.Append("</b></size>");
        }

        private static void EmitBlockElement(StringBuilder sb, string input, Match match, ChirpStyle style)
        {
            var capture = match.Groups["block"];
            int valueStart = capture.Index;
            int valueEnd = capture.Index + capture.Length;
            while (valueStart < valueEnd && (input[valueStart] == '\r' || input[valueStart] == '\n'))
                valueStart++;
            while (valueEnd > valueStart && (input[valueEnd - 1] == '\r' || input[valueEnd - 1] == '\n'))
                valueEnd--;

            // JValue.Parse throws for arbitrary text and a thrown exception per code block is
            // far more expensive than any allocation — only attempt the pretty-print when the
            // content can plausibly be a JSON object or array.
            string json = null;
            int probe = valueStart;
            while (probe < valueEnd && char.IsWhiteSpace(input[probe]))
                probe++;
            if (probe < valueEnd && (input[probe] == '{' || input[probe] == '['))
            {
                try
                {
                    json = JValue.Parse(input.Substring(valueStart, valueEnd - valueStart)).ToString(Formatting.Indented);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            var color = style.MarkdownBlockCodeColorHtml;
            if (color != null)
            {
                sb.Append("<color=#");
                sb.Append(color);
                sb.Append('>');
            }

            var titleGroup = match.Groups["blockTitle"];
            if (titleGroup.Success)
            {
                int titleStart = titleGroup.Index;
                int titleEnd = titleGroup.Index + titleGroup.Length;
                while (titleStart < titleEnd && char.IsWhiteSpace(input[titleStart]))
                    titleStart++;
                while (titleEnd > titleStart && char.IsWhiteSpace(input[titleEnd - 1]))
                    titleEnd--;
                if (titleEnd > titleStart)
                {
                    sb.Append("<b>");
                    sb.Append(input, titleStart, titleEnd - titleStart);
                    sb.Append("</b>\r\n");
                }
            }

            if (json != null)
                sb.Append(json);
            else
                sb.Append(input, valueStart, valueEnd - valueStart);

            if (color != null)
                sb.Append("</color>");
        }

        private static void EmitInlineElement(StringBuilder sb, string input, Match match, ChirpStyle style)
        {
            var capture = match.Groups["inline"];
            var color = style.MarkdownInlineCodeColorHtml;
            if (color != null)
            {
                sb.Append("<color=#");
                sb.Append(color);
                sb.Append('>');
            }

            sb.Append(input, capture.Index, capture.Length);

            if (color != null)
                sb.Append("</color>");
        }

        private const string c_SizeEndTag = "</size>";
        private const string c_ColorEndTag = "</color>";
    }
} 
