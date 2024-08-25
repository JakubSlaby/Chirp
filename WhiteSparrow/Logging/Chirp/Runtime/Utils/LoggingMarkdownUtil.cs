using System;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
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
        private const string s_PatternCodeBlock =  @"(?'blockW'(?<!`)```(?!`)(?'blockTitle'[\s\S]+)?(\r\n)(?'block'.+?)(\r\n)?(?<!`)```(?!`))";
        private const string s_PatternCodeInline =  @"(?'inlineW'(?<!`)`(?!`)(?'inline'.+?)(?<!`)`(?!`))";
        
        [UnityEditor.MenuItem("Tools/Chirp Logger/Development/Markdown RegEx")]
        private static void LogFullRegEx()
        {
            Debug.Log($"{s_PatternCodeBlock}|{s_PatternCodeInline}|{s_PatternItalicBold}|{s_PatternBold}|{s_PatternItalic}");
        }

        [UnityEditor.MenuItem("Tools/Chirp Logger/Development/Markdown Test")]
        private static void TestMarkdown()
        {
            string s = @"*Lorem ipsum dolor sit amet*, consectetur adipiscing elit, `sed do eiusmod tempor incididunt ut` labore et dolore 
magna aliqua. **Ut enim ad minim veniam**, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea 
commodo consequat. ***Duis aute irure dolor in reprehenderit in*** voluptate velit esse 
cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, 
sunt in culpa qui officia deserunt mollit anim id est laborum.

```
Some other text about anything else
```

```Object title
{""employee"":{""name"":""sonoo"",""salary"":56000,""married"":true}}
```
";

            string output = Parse(s);
            Debug.Log(output);
        }
        #endif
        
        [ThreadStatic] 
        private static StringBuilder s_StringBuilder;


        private static Regex s_CombinedRegex = new Regex(@"(?'blockW'(?<!`)```(?!`)(?'blockTitle'.+)?(\r\n)(?'block'[\s\S]+?)(\r\n)?(?<!`)```(?!`))|(?'inlineW'(?<!`)`(?!`)(?'inline'.+?)(?<!`)`(?!`))|(?'ibW'(?:(?<!\*)\*\*\*(?!\*)(?'ib'.+?)(?<!\*)\*\*\*(?!\*)|(?<!_)___(?!_)(?'ib'.+?)(?<!_)___(?!_)))|(?'bW'(?:(?<!\*)\*\*(?!\*)(?'b'.+?)(?<!\*)\*\*(?!\*)|(?<!_)__(?!_)(?'b'.+?)(?<!_)__(?!_)))|(?'iW'(?:(?<!\*)\*(?!\*)(?'i'.+?)(?<!\*)\*(?!\*)|(?<!_)_(?!_)(?'i'.+?)(?<!_)_(?!_)))", RegexOptions.Compiled | RegexOptions.Multiline);

        public static string Parse(string input) => Parse(input, Chirp.Style);
        public static string Parse(string input, ChirpStyle style)
        {
            if (s_StringBuilder == null)
                s_StringBuilder = new StringBuilder();
            else
                s_StringBuilder.Clear();

            s_StringBuilder.Append(input);
            
            var matches = s_CombinedRegex.Matches(input);
            if (matches.Count == 0)
                return input;
            
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];
                if (match.Groups["inlineW"].Success)
                {
                    ProcessInlineElement(s_StringBuilder, match, style);
                    continue;
                }
                if (match.Groups["blockW"].Success)
                {
                    ProcessBlockElement(s_StringBuilder, match, style);
                    continue;
                }
                if (match.Groups["ibW"].Success)
                {
                    ProcessItalicBold(s_StringBuilder, match, style);
                    continue;
                }
                if (match.Groups["iW"].Success)
                {
                    ProcessItalic(s_StringBuilder, match, style);
                    continue;
                }
                if (match.Groups["bW"].Success)
                {
                    ProcessBold(s_StringBuilder, match, style);
                    continue;
                }
            }
            



            return s_StringBuilder.ToString();
        }

        private static void ProcessBlockElement(StringBuilder sb, Match match, ChirpStyle style)
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
            var start = string.Empty;
            var color = style.MarkdownBlockCodeColorHtml;
            if (color != null)
                start += $"<color=#{color}>";

            if (match.Groups["blockTitle"].Success)
            {
                start += "<b>" + match.Groups["blockTitle"] + "</b>\r\n";
            }

            sb.Remove(match.Index, match.Length);
            sb.Insert(match.Index, start);
            sb.Insert(match.Index + start.Length, value);
            if(color != null)
                sb.Insert(match.Index + start.Length + value.Length, "</color>");
        }

        private static void ProcessInlineElement(StringBuilder sb, Match match, ChirpStyle style)
        {
            var capture = match.Groups["inline"];
            var start = string.Empty;
            var color = style.MarkdownInlineCodeColorHtml;
            if (color != null)
                start += $"<color=#{color}>";
            sb.Remove(match.Index, match.Length);
            sb.Insert(match.Index, start);
            sb.Insert(match.Index + start.Length, capture.Value);
            if (color != null)
                sb.Insert(match.Index + start.Length + capture.Length, "</color>");
        }
        
        private static void ProcessItalicBold(StringBuilder sb, Match match, ChirpStyle style)
        {
            var capture = match.Groups["ib"];
            var start = "<i><b>";
            sb.Remove(match.Index, match.Length);
            sb.Insert(match.Index, start);
            sb.Insert(match.Index + start.Length, capture.Value);
            sb.Insert(match.Index + start.Length + capture.Length, "</b></i>");
        }
        private static void ProcessItalic(StringBuilder sb, Match match, ChirpStyle style)
        {
            var capture = match.Groups["i"];
            var start = "<i>";
            sb.Remove(match.Index, match.Length);
            sb.Insert(match.Index, start);
            sb.Insert(match.Index + start.Length, capture.Value);
            sb.Insert(match.Index + start.Length + capture.Length, "</i>");
        }
        private static void ProcessBold(StringBuilder sb, Match match, ChirpStyle style)
        {
            var capture = match.Groups["b"];
            var start = "<b>";
            sb.Remove(match.Index, match.Length);
            sb.Insert(match.Index, start);
            sb.Insert(match.Index + start.Length, capture.Value);
            sb.Insert(match.Index + start.Length + capture.Length, "</b>");
        }
    }
} 