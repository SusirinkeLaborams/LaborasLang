﻿using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    //[Obsolete]
    class SymbolNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override TypeReference ExpressionReturnType { get { throw new InvalidOperationException(); } }
        public string Name { get; private set; }
        public ContextNode Scope { get; private set; }
        public override bool IsGettable { get { return false; } }
        public override bool IsSettable { get { return false; } }

        internal SymbolNode(string value, ContextNode scope, SequencePoint point)
            : base(point)
        {
            Name = value;
            Scope = scope;
        }

        public static SymbolNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            return new SymbolNode(lexerNode.Content.ToString(), context, context.Parser.GetSequencePoint(lexerNode));
        }

        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }

    class NamespaceNode : ExpressionNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ParserInternal; } }
        public override TypeReference ExpressionReturnType { get { throw new InvalidOperationException(); } }
        public Namespace Namespace { get; private set; }
        public override bool IsGettable { get { return false; } }
        public override bool IsSettable { get { return false; } }
        public NamespaceNode(Namespace namespaze, SequencePoint point)
            : base(point)
        {
            this.Namespace = namespaze;
        }
        public override string ToString(int indent)
        {
            throw new InvalidOperationException();
        }
    }
}
