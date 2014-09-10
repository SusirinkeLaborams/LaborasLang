﻿using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;

namespace LaborasLangCompiler.Parser.Impl
{
    class SymbolDeclarationNode : ParserNode, ISymbolDeclarationNode
    {
        public override NodeType Type { get { return NodeType.SymbolDeclaration; } }
        public ILValueNode DeclaredSymbol { get { return declaredSymbol; } }
        public IExpressionNode Initializer { get { return initializer; } }

        private ExpressionNode initializer;
        private LValueNode declaredSymbol;
        private SymbolDeclarationNode(LValueNode symbol, ExpressionNode init, SequencePoint point)
            : base(point)
        {
            this.declaredSymbol = symbol;
            this.initializer = init;
        }
        public static SymbolDeclarationNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            LValueNode symbol = null;
            var info = DeclarationInfo.Parse(parser, lexerNode);
            var name = info.SymbolName.GetSingleSymbolOrThrow();
            var declaredType = TypeNode.Parse(parser, parent, info.Type);
            ExpressionNode initializer = info.Initializer.IsNull ? null : ExpressionNode.Parse(parser, parent, info.Initializer);

            //temp code
            if (lexerNode.Children.Count > 2)
            {
                initializer = ExpressionNode.Parse(parser, parent, lexerNode.Children[2], true);
            }

            if (declaredType == null && (initializer == null || initializer.TypeWrapper == null))
                throw new TypeException(parser.GetSequencePoint(lexerNode), "Type inference requires initialization");

            if (initializer != null)
            {
                if (declaredType != null && initializer is AmbiguousNode)
                {
                    initializer = ((AmbiguousNode)initializer).RemoveAmbiguity(parser, declaredType);
                    if(initializer.TypeWrapper == null)
                    {
                        throw new ParseException(initializer.SequencePoint, "Ambiguous result, {0}", initializer);
                    }
                }

                if (declaredType == null)
                {
                    declaredType = initializer.TypeWrapper;
                }
                else if (!initializer.TypeWrapper.IsAssignableTo(declaredType))
                {
                    throw new TypeException(parser.GetSequencePoint(lexerNode), "Type mismatch, type " + declaredType.FullName + " initialized with " + initializer.ExpressionReturnType.FullName);
                }
            }

            if (parent is CodeBlockNode)
            {
                symbol = ((CodeBlockNode)parent).AddVariable(declaredType, name, parser.GetSequencePoint(lexerNode));
            }
            else
            {
                throw new ParseException(parser.GetSequencePoint(lexerNode), "SymbolDeclarationNode somehow parsed not in a code block");
            }

            return new SymbolDeclarationNode(symbol, initializer, parser.GetSequencePoint(lexerNode));
        }
        public override string ToString()
        {
            return String.Format("(Declaration: {0} = {1})", DeclaredSymbol.ToString(), Initializer != null ? Initializer.ToString() : "");
        }
    }
}
