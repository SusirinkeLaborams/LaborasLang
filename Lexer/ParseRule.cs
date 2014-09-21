﻿using System.Collections.Generic;
using System.Diagnostics;

namespace Lexer
{
    public enum ConditionType
    {
        OptionalFromThis,
        One,
        OneOrMore,
        ZeroOrMore
    }

    [DebuggerDisplay("Condition, {Type} {Token}")]
    struct Condition
    {
        public TokenType Token;
        public ConditionType Type;

        public Condition(Condition token, ConditionType type)
        {
            Token = token.Token;
            Type = type;
        }

        public Condition(TokenType token, ConditionType type)
        {
            Token = token;
            Type = type;
        }

        public static implicit operator Condition(TokenType token)
        {
            return new Condition(token, ConditionType.One);
        }

        public static implicit operator ConditionList(Condition token)
        {
            return new ConditionList(8) { token };
        }

        public static ConditionList operator +(Condition a, Condition b)
        {
            return new ConditionList(8) { a, b };
        }

        public static ConditionList operator +(ConditionList list, Condition token)
        {
            list.Add(token);
            return list;
        }
    }

    class ConditionList : List<Condition>
    {
        public ConditionList()
        {
        }

        public ConditionList(int capacity) :
            base(capacity)
        {
        }
        
        public static ConditionList operator +(ConditionList list, ConditionList tokens)
        {
            foreach (var token in tokens)
            {
                list.Add(token);
            }

            return list;
        }
    }

    struct ParseRule
    {
        public TokenType Result;
        public Condition[][] RequiredTokens { get; private set; }
        public int CollapsableLevel { get; private set; }

        public ParseRule(Condition result, int collapsableLevel, params List<Condition>[] requiredTokens) : this()
        {
            Result = result.Token;
            CollapsableLevel = collapsableLevel;
            RequiredTokens = new Condition[requiredTokens.Length][];

            for (int i = 0; i < requiredTokens.Length; i++)
            {
                RequiredTokens[i] = new Condition[requiredTokens[i].Count];

                for (int j = 0; j < requiredTokens[i].Count; j++)
                {
                    RequiredTokens[i][j] = requiredTokens[i][j];
                }
            }
        }
    }
}
