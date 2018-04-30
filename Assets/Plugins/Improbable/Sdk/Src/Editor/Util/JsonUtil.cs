// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Improbable.Unity.EditorTools.Util
{
    internal static class JsonUtil
    {
        /// <summary>
        ///     Return a JSON string representation of an IDictionary.
        /// </summary>
        internal static string ToJson(this IDictionary<string, string> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                return "{}";
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append('{');
            stringBuilder.Append(string.Join(",", dict.Select(pair => string.Format("\"{0}\":\"{1}\"", Escape(pair.Key), Escape(pair.Value))).ToArray()));
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }

        /// <summary>
        ///     Escapes the given text string for use in JSON.
        /// </summary>
        /// <remarks>
        ///     Escapes characters based on the ECMA-404 Standard (JSON.org)
        ///     http://www.ecma-international.org/publications/files/ECMA-ST/ECMA-404.pdf
        /// </remarks>
        internal static string Escape(string text)
        {
            StringWriter stringWriter = new StringWriter();
            foreach (var c in text)
            {
                switch (c)
                {
                    case '"':
                        stringWriter.Write("\\\"");
                        break;
                    case '\\':
                        stringWriter.Write("\\\\");
                        break;
                    case '\b':
                        stringWriter.Write("\\b");
                        break;
                    case '\f':
                        stringWriter.Write("\\f");
                        break;
                    case '\n':
                        stringWriter.Write("\\n");
                        break;
                    case '\r':
                        stringWriter.Write("\\r");
                        break;
                    case '\t':
                        stringWriter.Write("\\t");
                        break;
                    default:
                        if ('\x00' <= c && c <= '\x1f')
                        {
                            stringWriter.Write("\\u" + string.Format("{0:X}", (int) c).PadLeft(4, '0'));
                        }
                        else
                        {
                            stringWriter.Write(c);
                        }

                        break;
                }
            }

            return stringWriter.ToString();
        }
    }
}
