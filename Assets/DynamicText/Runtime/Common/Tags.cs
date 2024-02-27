using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityEngine.UI
{
    public partial class DynamicText
    {
        internal enum TagType
        {
            /// <summary>图片 </summary>
            Sprite,
            /// <summary>预制体 </summary>
            Prefab,
            /// <summary>下划线 </summary>
            Underline,
            /// <summary>超链接 </summary>
            Hyperlink,
        }
        internal class TagData
        {
            public TagType Type { get; set; }
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public int StartPadding { get; set; }
            public int EndPadding { get; set; }
            /// <summary>字段附加信息</summary>
            public string Extra { get; set; }
            /// <summary>内联颜色信息</summary>
            public string Color { get; set; }

            public bool IsQuadTag => Type == TagType.Sprite || Type == TagType.Prefab;

            public readonly List<Rect> bounds = new List<Rect>();
        }

        internal static class TagUtils
        {
            static readonly string matchColorTag = "<color=([#\\w]+)>([\\s\\S]+?)</color>";
            static readonly string matchSizeTag = "<size=([\\w]+)>([\\s\\S]+?)</size>";
            static readonly string matchBoldTag = "<b>([\\s\\S]+?)</b>";
            static readonly string matchItalicTag = "<i>([\\s\\S]+?)</i>";

            static readonly string matchUnderlineTag = "<u>([\\s\\S]+?)</u>";
            static readonly string matchLinkTag = "<link=\"([^\"\n<>]+?)\">([\\s\\S]+?)</link>";
            static readonly string matchSpriteTag = "<sprite=\"([^\"\n<>]+?)\"( size=([\\d\\.]+?))?( width=([\\d\\.]+?))?/?>";
            static readonly string matchPrefabTag = "<prefab=\"([^\"\n<>]+?)\"( size=([\\d\\.]+?))?( width=([\\d\\.]+?))?/?>";

            static Dictionary<string, Regex> _tagRegexs;
            public static Regex GetTagRegex(string pattern)
            {
                if (string.IsNullOrEmpty(pattern))
                {
                    pattern = "*";
                }
                if (_tagRegexs == null) _tagRegexs = new Dictionary<string, Regex>();

                Regex regex;
                if (!_tagRegexs.TryGetValue(pattern, out regex))
                {
                    if (pattern == "*")
                    {
                        string _pattern = "(" + string.Join("|", new string[]{
                            matchColorTag,
                            matchSizeTag,
                            matchBoldTag,
                            matchItalicTag,
                            matchUnderlineTag,
                            matchLinkTag,
                            matchSpriteTag,
                            matchPrefabTag,
                        }.Select(tag => $"({tag})")) + ")";
                        regex = new Regex(_pattern, RegexOptions.ExplicitCapture);
                    }
                    else
                    {
                        regex = new Regex(pattern, RegexOptions.Singleline);
                    }
                    _tagRegexs.Add(pattern, regex);
                }
                return regex;

            }

            static readonly HashSet<char> emptyCharacters = new HashSet<char>(new char[]{
                ' ',
                '\n',
            });
            static int GetStringCountWithoutEmpty(string text)
            {
                int count = 0, empty = 0;
                GetStringCount(text, ref count, ref empty);
                return count;
            }
            static void GetStringCount(string text, ref int count, ref int empty)
            {
                if (text == null)
                    return;
                for (int i = 0; i < text.Length; i++)
                {
                    if (emptyCharacters.Contains(text[i]))
                        empty++;
                    else
                        count++;
                }
            }
            static float GetQuadWidth(string name, string widthText)
            {
                float width = 1f;
                if (string.IsNullOrEmpty(widthText) || !float.TryParse(widthText, out width))
                {
                    width = 1f;
                }
                return width;
            }
            static float GetQuadSize(string sizeText)
            {
                float size = 1f;
                if (string.IsNullOrEmpty(sizeText) || !float.TryParse(sizeText, out size))
                {
                    size = 1f;
                }
                return size;
            }

            public static string ParseText(string sourceText, ref List<TagData> tags, int fontSize)
            {
                tags?.Clear();
                if (string.IsNullOrEmpty(sourceText))
                    return sourceText;

                //匹配文本标签
                var matchs = GetTagRegex(null).Matches(sourceText);
                int count = matchs.Count;
                if (count <= 0)
                    return sourceText;
                if (tags == null)
                {
                    tags = new List<TagData>();
                }
                StringBuilder builder = new StringBuilder();
                ParseText(sourceText, matchs, new ParseParameters(builder, tags, fontSize));
                return builder.ToString();
            }
            static void ParseText(string text, ParseParameters parameters)
            {
                if (string.IsNullOrEmpty(text))
                    return;
                var matchs = GetTagRegex(null).Matches(text);
                ParseText(text, matchs, parameters);
            }
            static void ParseText(string text, MatchCollection matchs, ParseParameters parameters)
            {
                if (matchs.Count <= 0)
                {
                    parameters.Builder.Append(text);
                    GetStringCount(text, ref parameters.characterVisible, ref parameters.padding);
                    return;
                }
                int lastMatchEndIndex = 0;
                foreach (Match match in matchs)
                {
                    if (match.Groups.Count <= 0)
                        continue;
                    int before = match.Index - lastMatchEndIndex;
                    if (before > 0)
                    {
                        string _text = text.Substring(lastMatchEndIndex, before);
                        parameters.Builder.Append(_text);
                        GetStringCount(_text, ref parameters.characterVisible, ref parameters.padding);
                    }
                    lastMatchEndIndex = match.Index + match.Length;

                    string value = match.Groups[0].Value;
                    if (value.StartsWith("<color="))
                        ParseTextWithColorTag(value, parameters);
                    else if (value.StartsWith("<size="))
                        ParseTextWithSizeTag(value, parameters);
                    else if (value.StartsWith("<b>"))
                        ParseTextWithBoldTag(value, parameters);
                    else if (value.StartsWith("<i>"))
                        ParseTextWithItalicTag(value, parameters);
                    else if (value.StartsWith("<u>"))
                        ParseTextWithUnderlineTag(value, parameters);
                    else if (value.StartsWith("<link="))
                        ParseTextWithLinkTag(value, parameters);
                    else if (value.StartsWith("<sprite="))
                        ParseTextWithSpriteTag(value, parameters);
                    else if (value.StartsWith("<prefab="))
                        ParseTextWithPrefabTag(value, parameters);
                    else
                    {
                        parameters.Builder.Append(value);
                        GetStringCount(value, ref parameters.characterVisible, ref parameters.padding);
                    }
                }

                if (lastMatchEndIndex < text.Length)
                {
                    string _text = text.Substring(lastMatchEndIndex);
                    parameters.Builder.Append(_text);
                    GetStringCount(_text, ref parameters.characterVisible, ref parameters.padding);
                }
            }
            static void ParseTextWithColorTag(string text, ParseParameters parameters)
            {
                var match = GetTagRegex(matchColorTag).Match(text);
                if (match == null || match.Groups.Count <= 0)
                {
                    parameters.Builder.Append(text);
                    GetStringCount(text, ref parameters.characterVisible, ref parameters.padding);
                    return;
                }

                string preColor = parameters.Color,
                    color = match.Groups[1].Value;

                parameters.Builder.AppendFormat("<color={0}>", color);
                parameters.padding += (8 + match.Groups[1].Value.Length);
                parameters.Color = color;
                ParseText(match.Groups[2].Value, parameters);
                parameters.Builder.Append("</color>");
                parameters.padding += 8;
                parameters.Color = preColor;
            }
            static void ParseTextWithSizeTag(string text, ParseParameters parameters)
            {
                var match = GetTagRegex(matchSizeTag).Match(text);
                if (match == null || match.Groups.Count <= 0)
                {
                    parameters.Builder.Append(text);
                    GetStringCount(text, ref parameters.characterVisible, ref parameters.padding);
                    return;
                }

                parameters.Builder.AppendFormat("<size={0}>", match.Groups[1].Value);
                parameters.padding += (7 + match.Groups[1].Value.Length);
                ParseText(match.Groups[2].Value, parameters);
                parameters.Builder.Append("</size>");
                parameters.padding += 7;
            }
            static void ParseTextWithBoldTag(string text, ParseParameters parameters)
            {
                var match = GetTagRegex(matchBoldTag).Match(text);
                if (match == null || match.Groups.Count <= 0)
                {
                    parameters.Builder.Append(text);
                    GetStringCount(text, ref parameters.characterVisible, ref parameters.padding);
                    return;
                }
                int idx = 0;
                Debug.Log("===================================:ParseTextWithBoldTag:");
                Debug.Log(text);
                Debug.Log(string.Join($"\n", match.Groups.Select(g => $"{++idx}.{g.Value}")));

                parameters.Builder.AppendFormat("<b>");
                parameters.padding += 3;
                ParseText(match.Groups[1].Value, parameters);
                parameters.Builder.Append("</b>");
                parameters.padding += 4;
            }
            static void ParseTextWithItalicTag(string text, ParseParameters parameters)
            {
                var match = GetTagRegex(matchItalicTag).Match(text);
                if (match == null || match.Groups.Count <= 0)
                {
                    parameters.Builder.Append(text);
                    GetStringCount(text, ref parameters.characterVisible, ref parameters.padding);
                    return;
                }

                parameters.Builder.AppendFormat("<i>");
                parameters.padding += 3;
                ParseText(match.Groups[1].Value, parameters);
                parameters.Builder.Append("</i>");
                parameters.padding += 4;
            }
            static void ParseTextWithUnderlineTag(string text, ParseParameters parameters)
            {
                var match = GetTagRegex(matchUnderlineTag).Match(text);
                if (match == null || match.Groups.Count <= 0)
                {
                    parameters.Builder.Append(text);
                    GetStringCount(text, ref parameters.characterVisible, ref parameters.padding);
                    return;
                }

                TagData tag = new TagData()
                {
                    Type = TagType.Underline,
                    StartIndex = parameters.characterVisible,
                    StartPadding = parameters.padding,
                    Color = parameters.Color,
                };
                parameters.Tags.Add(tag);
                ParseText(match.Groups[1].Value, parameters);
                tag.EndIndex = parameters.characterVisible;
                tag.EndPadding = parameters.padding;
            }
            static void ParseTextWithLinkTag(string text, ParseParameters parameters)
            {
                var match = GetTagRegex(matchLinkTag).Match(text);
                if (match == null || match.Groups.Count <= 0)
                {
                    parameters.Builder.Append(text);
                    GetStringCount(text, ref parameters.characterVisible, ref parameters.padding);
                    return;
                }

                TagData tag = new TagData()
                {
                    Type = TagType.Hyperlink,
                    StartIndex = parameters.characterVisible,
                    StartPadding = parameters.padding,
                    Extra = match.Groups[1].Value,
                    Color = parameters.Color,
                };
                parameters.Tags.Add(tag);
                ParseText(match.Groups[2].Value, parameters);
                tag.EndIndex = parameters.characterVisible;
                tag.EndPadding = parameters.padding;
            }
            static void ParseTextWithSpriteTag(string text, ParseParameters parameters)
            {
                Match match = GetTagRegex(matchSpriteTag).Match(text);
                if (match == null || match.Groups.Count <= 0)
                {
                    parameters.Builder.Append(text);
                    GetStringCount(text, ref parameters.characterVisible, ref parameters.padding);
                    return;
                }

                /* int idx = 0;
                Debug.Log("=================================================:ParseTextWithSpriteTag:");
                Debug.Log(text);
                Debug.Log(string.Join("\n", match.Groups.Select(g => $"{++idx}.{g.Value}"))); */

                TagData tag = new TagData()
                {
                    Type = TagType.Sprite,
                    StartIndex = parameters.characterVisible,
                    EndIndex = parameters.characterVisible,
                    StartPadding = parameters.padding,
                    EndPadding = parameters.padding,
                    Extra = match.Groups[1].Value,
                    Color = parameters.Color,
                };
                parameters.Tags.Add(tag);

                int size = (int)(parameters.FontSize * GetQuadSize(match.Groups[3].Value));
                float width = GetQuadWidth(tag.Extra, match.Groups[5].Value);
                string quadText = string.Format("<quad size={0}, width={1}/>", size, width);
                parameters.Builder.Append(quadText);
                parameters.characterVisible++;                      //<quad>标签占用一个顶点序号
                parameters.padding += quadText.Length - 1;          //计算标签占用符号(减去<quad>本身占用的字符1)
            }
            static void ParseTextWithPrefabTag(string text, ParseParameters parameters)
            {
                Match match = GetTagRegex(matchPrefabTag).Match(text);
                if (match == null || match.Groups.Count <= 0)
                {
                    parameters.Builder.Append(text);
                    GetStringCount(text, ref parameters.characterVisible, ref parameters.padding);
                    return;
                }

                TagData tag = new TagData()
                {
                    Type = TagType.Prefab,
                    StartIndex = parameters.characterVisible,
                    EndIndex = parameters.characterVisible,
                    StartPadding = parameters.padding,
                    EndPadding = parameters.padding,
                    Extra = match.Groups[1].Value,
                    Color = parameters.Color,
                };
                parameters.Tags.Add(tag);

                int size = (int)(parameters.FontSize * GetQuadSize(match.Groups[3].Value));
                float width = GetQuadWidth(tag.Extra, match.Groups[5].Value);
                string quadText = string.Format("<quad size={0}, width={1}/>", size, width);
                parameters.Builder.Append(quadText);
                parameters.characterVisible++;                      //<quad>标签占用一个顶点序号
                parameters.padding += quadText.Length - 1;          //计算标签占用符号(减去<quad>本身占用的字符1)
            }

            class ParseParameters
            {
                public int padding;
                public int characterVisible;
                public string Color { get; set; }
                public StringBuilder Builder { get; }
                public List<TagData> Tags { get; }
                public int FontSize { get; }
                public ParseParameters(StringBuilder builder, List<TagData> tags, int fontSize)
                {
                    this.Builder = builder;
                    this.Tags = tags;
                    this.FontSize = fontSize;
                }
            }
        }
    }
}