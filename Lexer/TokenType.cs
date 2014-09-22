﻿namespace Lexer
{ 
    public enum TokenType
    {
        Unknown = 0,

        // Terminals
        EndOfLine,
        Comma,
        Period,
        Comment,
        BitwiseAnd,
        BitwiseAndEqual,
        Plus,
        PlusPlus,
        Minus,
        MinusMinus,
        MinusEqual,
        NotEqual,
        Not,
        Whitespace,
        PlusEqual,
        StringLiteral,
        BitwiseComplement,
        BitwiseXor,
        BitwiseXorEqual,
        BitwiseOr,
        BitwiseOrEqual,
        LeftShiftEqual,
        LeftShift,
        LessOrEqual,
        Less,
        LogicalAnd,
        LogicalAndEqual,
        LogicalOr,
        LogicalOrEqual,
        More,
        RightShift,
        RightShiftEqual,
        MoreOrEqual,
        Divide,
        DivideEqual,
        Multiply,
        MultiplyEqual,
        Remainder,
        RemainderEqual,
        Assignment,
        Equal,
        LeftCurlyBrace,
        RightCurlyBrace,
        LeftParenthesis,
        RightParenthesis,
        Integer,
        Float,
        Long,
        Double,
        MalformedToken,
        Symbol,
        Abstract,
        As,
        Base,
        Break,
        Case,
        Catch,
        Class,
        Const,
        Continue,
        Default,
        Do,
        Extern,
        Else,
        Enum,
        False,
        Finally,
        For,
        Goto,
        If,
        Interface,
        Internal,
        Is,
        New,
        Null,
        Namespace,
        Out,
        Override,
        Protected,
        Ref,
        Return,
        Switch,
        Sealed,
        This,
        Throw,
        Struct,
        True,
        Try,
        Use,
        Virtual,
        While,


        Static,
        Private,
        Public,
        
        //Non terminals
        NonTerminalToken,
        
        CodeBlockNode,
        DeclarationNode,
        ReturnNode,
        Value,

        RootNode,
        
        FullSymbol,
        Type,
        UseNode,

        WhileLoop,
        Function,
        ConditionalSentence,

        AssignmentOperatorNode,
        OrNode,
        AndNode,
        BitwiseOrNode,
        BitwiseXorNode,
        BitwiseAndNode,
        PeriodNode,
        PrefixNode, 
        PostfixNode,
        InlineFunctionCallNode,
        FunctionCallNode,
        FunctionArgumentsList,
        EqualityOperatorNode,
        RelationalOperatorNode,
        ShiftOperatorNode,
        AdditiveOperatorNode,
        MultiplicativeOperatorNode,
        ParenthesesNode,
        LiteralNode,

        LexerInternalTokens,    // Lexer internal-only tokens start from here

        StatementNode,
        DeclarationSubnode,
        ValueStatementNode,

        TypeSubnode,
        TypeAndSymbolSubnode,
        CommaAndValue,
        SubSymbol,

        Operand,
        VariableModifier,

        PeriodSubnode,
        MultiplicativeOperatorSubnode,
        AdditiveOperatorSubnode,
        ShiftOperatorSubnode,
        RelationalOperatorSubnode,
        EqualityOperatorSubnode,
        BitwiseAndSubnode,
        BitwiseXorSubnode,
        BitwiseOrSubnode,
        AndSubnode,
        OrSubnode,

        PrefixOperator,
        PostfixOperator,
        MultiplicativeOperator,
        AdditiveOperator,
        ShiftOperator,
        RelationalOperator,
        EqualityOperator,
        AssignmentOperator,

        TokenTypeCount
    }

    public static class TokenInfo
    {
        public static bool IsTerminal(this TokenType token)
        {
            // PERF: CompareTo is expensive
            return (int)token < (int)TokenType.NonTerminalToken;
        }
    }
    
}
