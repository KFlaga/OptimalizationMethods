using System;

namespace Algorithm.Parser
{
    public class Input
    {
        public int Position { get; private set; }
        public char Current => input[Position];
        public bool IsEnd => Position >= input.Length;

        private readonly string input;

        public Input(string x)
        {
            input = x;
            Position = 0;
        }

        public Input(string x, int startFrom)
        {
            input = x;
            Position = startFrom;
        }

        public void MoveNext()
        {
            if (IsEnd)
            {
                throw new IndexOutOfRangeException("End of input");
            }
            Position++;
        }

        public void SkipWhile(Func<char, bool> condition, bool includeCorner = false)
        {
            while (!IsEnd && condition(Current))
            {
                MoveNext();
            }
            if (!IsEnd && includeCorner)
            {
                MoveNext();
            }
        }

        public string ReadWhile(Func<char, bool> condition, bool includeCorner = false)
        {
            int startPos = Position;
            SkipWhile(condition, includeCorner);
            return input.Substring(startPos, Position - startPos);
        }

        public void SkipSpace()
        {
            while (!IsEnd && char.IsWhiteSpace(Current))
            {
                MoveNext();
            }
        }

        public void SkipN(int n)
        {
            Position += n;
        }

        public bool StartsWith(string x)
        {
            if(input.Length - Position < x.Length)
            {
                return false;
            }
            return string.Compare(input, Position, x, 0, x.Length) == 0;
        }

        public bool Contains(char c)
        {
            return input.LastIndexOf(c) >= Position;
        }
    }
}
