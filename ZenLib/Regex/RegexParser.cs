// <copyright file="RegexParser.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A parser for a subset of C# regex expressions.
    /// </summary>
    public class RegexParser
    {
        /// <summary>
        /// All valid characters.
        /// </summary>
        private static ISet<string> characters = new HashSet<string>
        {
            "!", "\"", "#", "$", "%", "&", "'", "(", ")", "*", "+", ",", "-", ".", "/", ":",
            ";", "<", "=", ">", "?", "@", "[", "\\", "]", "^", "_", "`", "{", "|", "}", "~",
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
        };

        /// <summary>
        /// Special characters.
        /// </summary>
        private static ISet<string> specialCharacters = new HashSet<string>
        {
            "\\", "^", "$", "*", "+", "?", ".", "(", ")", "[", "]", "{", "}", "|",
        };

        /// <summary>
        /// Unary operation characters.
        /// </summary>
        private static ISet<string> unaryOperationCharacters = new HashSet<string>
        {
            "*", "+", "?",
        };

        /// <summary>
        /// The input string to match.
        /// </summary>
        private string inputString;

        /// <summary>
        /// The current symbol.
        /// </summary>
        private string symbol;

        /// <summary>
        /// The current index into the input string.
        /// </summary>
        private int currentIndex;

        /// <summary>
        /// Creates a new instance of the <see cref="RegexParser"/> class.
        /// </summary>
        /// <param name="inputString">The input string to parse.</param>
        public RegexParser(string inputString)
        {
            this.inputString = inputString;
            this.currentIndex = -1;
            NextSymbol();
        }

        /// <summary>
        /// Parse the regex.
        /// </summary>
        /// <returns>The Zen regex value.</returns>
        public Regex<byte> Parse()
        {
            return ParseRegex();
        }

        private Regex<byte> ParseRegex()
        {
            Console.WriteLine($"Parse Regex: {symbol}");
            var r = ParseTerm();
            while (symbol == "|")
            {
                NextSymbol();
                r = Regex.Union(r, ParseTerm());
            }

            return r;
        }

        private Regex<byte> ParseTerm()
        {
            Console.WriteLine($"Parse Term: {symbol}");
            var r = ParseFactor();
            while (IsCharacter() || symbol == "." || symbol == "(" || symbol == "[" || symbol == "\\")
            {
                r = Regex.Concat(r, ParseFactor());
            }

            return r;
        }

        private Regex<byte> ParseFactor()
        {
            Console.WriteLine($"Parse Factor: {symbol}");
            var r = ParseAtom();
            if (AcceptUnaryOperation(out var operation))
            {
                if (operation == "*")
                {
                    return Regex.Star(r);
                }
                else if (operation == "+")
                {
                    return Regex.Concat(r, Regex.Star(r));
                }
                else
                {
                    Contract.Assert(operation == "?");
                    return Regex.Opt(r);
                }
            }

            return r;
        }

        private Regex<byte> ParseAtom()
        {
            Console.WriteLine($"Parse Atom: {symbol}");
            if (AcceptNonSpecialCharacter(out var character))
            {
                return Regex.Char((byte)character[0]);
            }
            else if (Accept("\\"))
            {
                character = ExpectAnyCharacter();
                return Regex.Char((byte)character[0]);
            }
            else if (Accept("."))
            {
                return Regex.Dot<byte>();
            }
            else if (Accept("("))
            {
                var r = ParseRegex();
                Expect(")");
                return r;
            }
            else if (Accept("["))
            {
                var r = ParseCharacterClass();
                Expect("]");
                return r;
            }
            else
            {
                throw new ZenException($"Unexpected symbol: {symbol}");
            }
        }

        private Regex<byte> ParseCharacterClass()
        {
            var r = ParseCharacterRange();
            while (IsCharacter())
            {
                r = Regex.Union(r, ParseCharacterRange());
            }

            return r;
        }

        private Regex<byte> ParseCharacterRange()
        {
            var c1 = ParseCharacter();

            if (Accept("-"))
            {
                var c2 = (byte)ExpectNonSpecialCharacter()[0];
                return Regex.Range(c1, c2);
            }

            return Regex.Char(c1);
        }

        private byte ParseCharacter()
        {
            if (AcceptNonSpecialCharacter(out var character))
            {
                return (byte)character[0];
            }
            else if (Accept("\\"))
            {
                character = ExpectAnyCharacter();
                return (byte)character[0];
            }
            else
            {
                throw new ZenException($"Unexpected symbol: {symbol}, expected character or \\");
            }
        }

        private bool IsCharacter()
        {
            return characters.Contains(symbol) && !specialCharacters.Contains(symbol);
        }

        private bool IsUnaryOperation()
        {
            return unaryOperationCharacters.Contains(symbol);
        }

        private void Expect(string s)
        {
            if (!Accept(s))
            {
                throw new ZenException($"Unexpected symbol {symbol}, expected: {s}");
            }
        }

        private string ExpectNonSpecialCharacter()
        {
            if (!AcceptNonSpecialCharacter(out var character))
            {
                throw new ZenException($"Unexpected symbol {symbol}, expected non-special character");
            }

            return character;
        }

        private string ExpectAnyCharacter()
        {
            if (!AcceptAnyCharacter(out var character))
            {
                throw new ZenException($"Unexpected symbol {symbol}, expected character");
            }

            return character;
        }

        private bool Accept(string s)
        {
            if (symbol == s)
            {
                NextSymbol();
                return true;
            }

            return false;
        }

        private bool AcceptNonSpecialCharacter(out string character)
        {
            character = symbol;
            if (IsCharacter())
            {
                NextSymbol();
                return true;
            }

            return false;
        }

        private bool AcceptAnyCharacter(out string character)
        {
            character = symbol;
            if (characters.Contains(symbol))
            {
                NextSymbol();
                return true;
            }

            return false;
        }

        private bool AcceptUnaryOperation(out string character)
        {
            character = symbol;
            if (IsUnaryOperation())
            {
                NextSymbol();
                return true;
            }

            return false;
        }

        private void NextSymbol()
        {
            currentIndex++;
            if (currentIndex >= inputString.Length)
            {
                this.symbol = string.Empty;
            }
            else
            {
                this.symbol = inputString.Substring(currentIndex, 1);
            }
        }
    }
}
