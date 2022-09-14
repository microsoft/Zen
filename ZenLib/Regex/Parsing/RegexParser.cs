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
    internal class RegexParser<T>
    {
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
            "*", "+", "?", "{",
        };

        /// <summary>
        /// Digit characters.
        /// </summary>
        private static ISet<string> digitCharacters = new HashSet<string>
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
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
        /// Converter from a character to a base value.
        /// </summary>
        private Func<char, T> characterConverter;

        /// <summary>
        /// The current index into the input string.
        /// </summary>
        private int currentIndex;

        /// <summary>
        /// Creates a new instance of the <see cref="RegexParser{T}"/> class.
        /// </summary>
        /// <param name="inputString">The input string to parse.</param>
        /// <param name="characterConverter">A character converter.</param>
        public RegexParser(string inputString, Func<char, T> characterConverter)
        {
            this.inputString = inputString;
            this.currentIndex = -1;
            this.characterConverter = characterConverter;
            NextSymbol();
        }

        /// <summary>
        /// Parse the regex.
        /// </summary>
        /// <returns>The Zen regex value.</returns>
        public Regex<T> Parse()
        {
            if (symbol == string.Empty)
            {
                return Regex.Epsilon<T>();
            }

            var ret = ParseRegex();

            if (symbol != string.Empty)
            {
                var remaining = inputString.Substring(currentIndex, inputString.Length - currentIndex - 1);
                throw new ZenException($"Unable to parse trailing: {remaining}");
            }

            return new RegexRemoveAnchorVisitor<T>().Compute(ret);
        }

        /// <summary>
        /// Parse a regex from the current location.
        /// </summary>
        /// <returns>A regex.</returns>
        private Regex<T> ParseRegex()
        {
            var r = ParseTerm();
            while (symbol == "|")
            {
                NextSymbol();
                r = Regex.Union(r, ParseTerm());
            }

            return r;
        }

        /// <summary>
        /// Parse a regex term (part of a union) from the current location.
        /// </summary>
        /// <returns>A regex.</returns>
        private Regex<T> ParseTerm()
        {
            var r = ParseFactor();
            while (IsNonSpecialCharacter() || symbol == "$" || symbol == "^" || symbol == "." || symbol == "(" || symbol == "[" || symbol == "\\")
            {
                r = Regex.Concat(r, ParseFactor());
            }

            return r;
        }

        /// <summary>
        /// Parse a factor (part of a concatentation) from the current location.
        /// </summary>
        /// <returns>A regex.</returns>
        private Regex<T> ParseFactor()
        {
            var r = ParseAtom();
            while (Accept(unaryOperationCharacters, out var operation))
            {
                if (operation == "*")
                {
                    r = Regex.Star(r);
                }
                else if (operation == "+")
                {
                    r = Regex.Concat(r, Regex.Star(r));
                }
                else if (operation == "{")
                {
                    var lo = ParseInteger();

                    if (Accept(","))
                    {
                        if (Accept("}"))
                        {
                            r = Regex.Concat(Regex.Repeat(r, lo), Regex.Star(r));
                        }
                        else
                        {
                            var hi = ParseInteger();
                            r = Regex.Repeat(r, lo, hi);
                            Expect("}");
                        }
                    }
                    else
                    {
                        r = Regex.Repeat(r, lo);
                        Expect("}");
                    }
                }
                else
                {
                    Contract.Assert(operation == "?");
                    r = Regex.Opt(r);
                }
            }

            return r;
        }

        /// <summary>
        /// Parse a positive integer.
        /// </summary>
        /// <returns>The integer.</returns>
        private int ParseInteger()
        {
            var d = Expect(digitCharacters);
            while (Accept(digitCharacters, out var digit))
            {
                d += digit;
            }

            return int.Parse(d);
        }

        /// <summary>
        /// Parse an atom (a base element) from the current location.
        /// </summary>
        /// <returns>A regex.</returns>
        private Regex<T> ParseAtom()
        {
            if (AcceptNonSpecialCharacter(out var character))
            {
                return Regex.Char(this.characterConverter(character[0]));
            }
            else if (Accept("\\"))
            {
                if (Accept("e"))
                {
                    return Regex.Epsilon<T>();
                }
                else
                {
                    character = ExpectAnyCharacter();
                    return Regex.Char(this.characterConverter(character[0]));
                }
            }
            else if (Accept("$"))
            {
                return RegexAnchorExpr<T>.EndInstance;
            }
            else if (Accept("^"))
            {
                return RegexAnchorExpr<T>.BeginInstance;
            }
            else if (Accept("."))
            {
                return Regex.Dot<T>();
            }
            else if (Accept("("))
            {
                var r = ParseRegex();
                Expect(")");
                return r;
            }
            else if (Accept("["))
            {
                var isNegated = Accept("^");
                var r = ParseCharacterClass();
                Expect("]");

                return isNegated ? Regex.Intersect(Regex.Negation(r), Regex.Dot<T>()) : r;
            }
            else
            {
                throw new ZenException($"Unexpected symbol: {symbol}");
            }
        }

        /// <summary>
        /// Parse a character class from the current location.
        /// </summary>
        /// <returns>A regex.</returns>
        private Regex<T> ParseCharacterClass()
        {
            var r = ParseCharacterRange();
            while (symbol != "]")
            {
                r = Regex.Union(r, ParseCharacterRange());
            }

            return r;
        }

        /// <summary>
        /// Parse a character range from the current location.
        /// </summary>
        /// <returns>A regex.</returns>
        private Regex<T> ParseCharacterRange()
        {
            var c1 = ParseClassCharacter();

            if (Accept("-"))
            {
                // if the - is the last character, then it is treated as a literal.
                if (Peek(1) == "]")
                {
                    return Regex.Char(this.characterConverter('-'));
                }
                else
                {
                    var c2 = ParseClassCharacter();
                    if (((dynamic)c2).CompareTo((dynamic)c1) < 0)
                    {
                        throw new ZenException($"Invalid character range given.");
                    }

                    return Regex.Range(c1, c2);
                }
            }

            return Regex.Char(c1);
        }

        /// <summary>
        /// Parse a character or escaped sequence from the current location.
        /// </summary>
        /// <returns>A character value.</returns>
        private T ParseClassCharacter()
        {
            if (AcceptClassCharacter(out var character))
            {
                return this.characterConverter(character[0]);
            }
            else if (Accept("\\"))
            {
                character = ExpectAnyCharacter();
                return this.characterConverter(character[0]);
            }
            else
            {
                throw new ZenException($"Unexpected symbol: {symbol}, expected character or \\");
            }
        }

        /// <summary>
        /// Determine if the current symbol is a character.
        /// </summary>
        /// <returns>True or false.</returns>
        private bool IsNonSpecialCharacter()
        {
            return symbol != "" && !specialCharacters.Contains(symbol);
        }

        /// <summary>
        /// Determine if the current symbol is a class character.
        /// </summary>
        /// <returns>True or false.</returns>
        private bool IsClassCharacter()
        {
            return symbol != "" && symbol != "\\" && symbol != "]";
        }

        /// <summary>
        /// Consume a symbol and expect it to match the one provided.
        /// </summary>
        /// <param name="s">The expected symbol.</param>
        private void Expect(string s)
        {
            if (!Accept(s))
            {
                throw new ZenException($"Unexpected symbol {symbol}, expected: {s}");
            }
        }

        /// <summary>
        /// Consume a symbol and expect it to match the one provided.
        /// </summary>
        /// <param name="characters">The characters to test against.</param>
        /// <returns>True if there was a match.</returns>
        private string Expect(ISet<string> characters)
        {
            if (!Accept(characters, out var character))
            {
                throw new ZenException($"Unexpected symbol {symbol}");
            }

            return character;
        }

        /// <summary>
        /// Consume a symbol and expect it to be any valid character.
        /// </summary>
        /// <returns>The symbol consumed.</returns>
        private string ExpectAnyCharacter()
        {
            if (symbol == string.Empty)
            {
                throw new ZenException("Expected any character");
            }

            var character = symbol;
            NextSymbol();
            return character;
        }

        /// <summary>
        /// Check if the next symbol matches the one tested and if so advance.
        /// </summary>
        /// <param name="s">The symbol tested.</param>
        /// <returns>True if there was a match.</returns>
        private bool Accept(string s)
        {
            if (symbol == s)
            {
                NextSymbol();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the next symbol matches one of the set provided, and if so advance.
        /// </summary>
        /// <param name="characters">The characters to test against.</param>
        /// <param name="character">The character matched.</param>
        /// <returns>True if there was a match.</returns>
        private bool Accept(ISet<string> characters, out string character)
        {
            character = symbol;
            if (characters.Contains(symbol))
            {
                NextSymbol();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the current symbol matches a class character and if so, advance.
        /// </summary>
        /// <param name="character">The symbol matched.</param>
        /// <returns>True if there was a match.</returns>
        private bool AcceptClassCharacter(out string character)
        {
            character = symbol;
            if (IsClassCharacter())
            {
                NextSymbol();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the current symbol matches a non-special character and if so advance.
        /// </summary>
        /// <param name="character">The symbol matched.</param>
        /// <returns>True if there was a match.</returns>
        private bool AcceptNonSpecialCharacter(out string character)
        {
            character = symbol;
            if (IsNonSpecialCharacter())
            {
                NextSymbol();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Advance to the next symbol, which will be the empty
        /// string where there are no more symbols to consume.
        /// </summary>
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

        /// <summary>
        /// Peek a given number of symbols ahead.
        /// </summary>
        /// <param name="count">The number of symbols ahead to peek.</param>
        /// <returns>The sequence of symbols ahead, or empty if exceeds the length of the string.</returns>
        private string Peek(int count)
        {
            if (currentIndex + count >= inputString.Length)
            {
                return string.Empty;
            }

            return inputString.Substring(currentIndex, count);
        }
    }
}
