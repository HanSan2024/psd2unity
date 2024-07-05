using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PhotoshopFile;
using UnityEngine;
using System.Globalization;
using System.Reflection;

// 数据节点
public class EngineDataNode
{
    private double? number;
    private List<EngineDataNode> array;
    private Dictionary<string, EngineDataNode> dict;
    private string text;
    private bool? boolean;

    public int intValue => number != null ? (int)number.Value : 0;
    public float floatValue => number != null ? (float)number.Value : 0;
    public bool boolValue => boolean != null && boolean.Value;
    public string textValue => text;

    public EngineDataNode(string t) => text = t;
    public EngineDataNode(Dictionary<string, EngineDataNode> d) => dict = d;
    public EngineDataNode(List<EngineDataNode> a) => array = a;
    public EngineDataNode(double? n) => number = n;
    public EngineDataNode(bool? b) => boolean = b;

    public EngineDataNode this[int index]
    {
        get
        {
            if (array != null && index >=0 && index < array.Count)
            {
                return array[index];
            }
            return null;
        }
    }

    public EngineDataNode this[string key]
    {
        get
        {
            if (!string.IsNullOrEmpty(key) && dict != null && dict.TryGetValue(key, out var n))
            {
                return n;
            }
            return null;
        }
    }


    public override string ToString()
    {
        if (number != null)
            return number.Value.ToString(CultureInfo.InvariantCulture);
        else if (text != null)
            return $"\"{text.Replace("\n", "")}\"";
        else if (boolean != null)
            return $"{boolean.Value.ToString().ToLower()}";
        else if (array != null)
        {
            var sb = new StringBuilder();
            sb.Append('[');
            for (int i = 0; i < array.Count; i++)
            {
                sb.Append($"{array[i]}{(i == array.Count - 1 ? "" : ", ")}");
            }
            sb.Append(']');
            return sb.ToString();
        }
        else if (dict != null)
        {
            var sb = new StringBuilder();
            sb.Append('{');
            int i = 0, count = dict.Count;
            foreach (var pair in dict)
            {
                sb.Append($"\"{pair.Key}\" : {pair.Value} {(i == dict.Count - 1 ? "" : ", ")}");
                i++;
            }
            sb.Append('}');
            return sb.ToString();
        }

        return "";
    }
}


public class EngineDataParser
{
    private enum Token
    {
        NONE, // node
        DICT_OPEN, // <<
        DICT_CLOSE, // >>
        SQUA_OPEN, // [
        SQUA_CLOSE, // ]
        STR_START, // (˛ˇ
        STR_END, // \r)
        SLASH, // /
        TRUE, // true
        FALSE, // false
        NUMBER, // number
    }

    private class Reader
    {
        private PsdBinaryReader reader;
        private BinaryReader baseReader;

        public byte ReadByte() => reader.ReadByte();
        public int PeekChar() => baseReader.PeekChar();

        public Reader(PsdBinaryReader r)
        {
            reader = r;
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var info = typeof(PsdBinaryReader).GetField("reader", flags);
            baseReader = info.GetValue(r) as BinaryReader;
        }
    }


    public static EngineDataNode Parse(byte[] bytes)
    {
        using var bs = new MemoryStream(bytes);
        using var br = new PsdBinaryReader(bs, Encoding.ASCII);
        var reader = new Reader(br);
        var token = NextToken(reader);
        return ParseByToken(token, reader);
    }


    const string WORD_BREAK = "{}[],:\"";

    public static bool IsWordBreak(char c)
    {
        return char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
    }


    static char PeekChar(Reader reader)
    {
        try
        {
            return Convert.ToChar(reader.PeekChar());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return default;
        }
    }


    static void EatWhitespace(Reader reader)
    {
        while (char.IsWhiteSpace(PeekChar(reader)))
        {
            reader.ReadByte();

            if (reader.PeekChar() == -1)
            {
                break;
            }
        }
    }


    private static string NextWord(Reader reader)
    {
        var word = new StringBuilder();

        while (!IsWordBreak(PeekChar(reader)))
        {
            word.Append((char)reader.ReadByte());

            if (reader.PeekChar() == -1)
            {
                break;
            }
        }

        return word.ToString();
    }


    static Token NextToken(Reader reader)
    {
        EatWhitespace(reader);

        var peek = reader.PeekChar();
        if (peek == -1)
        {
            return Token.NONE;
        }

        switch ((char)peek)
        {
            case '/':
                reader.ReadByte();
                return Token.SLASH;
            case '[':
                reader.ReadByte();
                return Token.SQUA_OPEN;
            case ']':
                reader.ReadByte();
                return Token.SQUA_CLOSE;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
            case '.':
                return Token.NUMBER;
        }

        if (peek == '<')
        {
            reader.ReadByte();
            var peek1 = reader.PeekChar();
            switch (peek1)
            {
                case -1:
                    return Token.NONE;
                case '<':
                    reader.ReadByte();
                    return Token.DICT_OPEN;
            }
        }
        else if (peek == '>')
        {
            reader.ReadByte();
            var peek1 = reader.PeekChar();
            switch (peek1)
            {
                case -1:
                    return Token.NONE;
                case '>':
                    reader.ReadByte();
                    return Token.DICT_CLOSE;
            }
        }
        else if (peek == '(')
        {
            var f1 = reader.ReadByte();
            var b1 = reader.ReadByte();
            var b2 = reader.ReadByte();
            if (b1 == 0xfe && b2 == 0xff)
            {
                return Token.STR_START;
            }
        }
        else if (peek == '\r')
        {
            reader.ReadByte();
            var peek1 = reader.PeekChar();
            switch (peek1)
            {
                case ')':
                    reader.ReadByte();
                    return Token.STR_END;
            }
        }

        var word = NextWord(reader);
        switch (word)
        {
            case "true":
                return Token.TRUE;
            case "false":
                return Token.FALSE;
        }

        return Token.NONE;
    }


    static EngineDataNode ParseByToken(Token token, Reader reader)
    {
        switch (token)
        {
            case Token.STR_START:
                return ParseString(reader);
            case Token.NUMBER:
                return ParseNumber(reader);
            case Token.DICT_OPEN:
                return ParseObject(reader);
            case Token.SQUA_OPEN:
                return ParseArray(reader);
            case Token.TRUE:
                return new EngineDataNode(true);
            case Token.FALSE:
                return new EngineDataNode(false);
            default:
                return null;
        }
    }


    private static EngineDataNode ParseString(Reader reader)
    {
        var bytes = new List<byte>();
        while (reader.PeekChar() != -1)
        {
            var c = reader.ReadByte();
            if (c == '\\')
            {
                var c2 = reader.ReadByte();
                if (c2 != '\\' && c2 != '(' && c2 != ')')
                {
                    bytes.Add(c);
                }

                bytes.Add(c2);
            }
            else if (c == ')')
            {
                if (reader.ReadByte() == 10)
                {
                    break;
                }

                bytes.Add(c);
            }
            else
            {
                bytes.Add(c);
            }
        }

        var str = Encoding.BigEndianUnicode.GetString(bytes.ToArray());
        str = str.Replace('\r', '\n');
        return new EngineDataNode(str);
    }


    private static EngineDataNode ParseNumber(Reader reader)
    {
        var number = NextWord(reader);
        if (number.IndexOf('.') == -1 && number.IndexOf('E') == -1 && number.IndexOf('e') == -1)
        {
            long.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedInt);
            return new EngineDataNode(parsedInt);
        }

        double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedDouble);
        return new EngineDataNode(parsedDouble);
    }


    private static EngineDataNode ParseObject(Reader reader)
    {
        var table = new Dictionary<string, EngineDataNode>();
        while (true)
        {
            var token1 = NextToken(reader);
            switch (token1)
            {
                case Token.DICT_CLOSE:
                    return new EngineDataNode(table);
                case Token.SLASH:
                    var key = NextWord(reader);
                    var token = NextToken(reader);
                    var obj = ParseByToken(token, reader);
                    table[key] = obj;
                    break;
                case Token.SQUA_OPEN:
                case Token.SQUA_CLOSE:
                case Token.STR_START:
                case Token.STR_END:
                case Token.NONE:
                case Token.DICT_OPEN:
                case Token.TRUE:
                case Token.FALSE:
                case Token.NUMBER:
                default:
                    return null;
            }
        }
    }


    private static EngineDataNode ParseArray(Reader reader)
    {
        var array = new List<EngineDataNode>();
        while (true)
        {
            switch (NextToken(reader))
            {
                case Token.DICT_OPEN:
                    array.Add(ParseObject(reader));
                    break;
                case Token.SQUA_CLOSE:
                    return new EngineDataNode(array);
                case Token.NUMBER:
                    array.Add(ParseNumber(reader));
                    break;
                case Token.NONE:
                case Token.DICT_CLOSE:
                case Token.SQUA_OPEN:
                case Token.STR_START:
                case Token.STR_END:
                case Token.SLASH:
                case Token.TRUE:
                case Token.FALSE:
                default:
                    return null;
            }
        }
    }

}