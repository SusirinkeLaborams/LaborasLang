﻿using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class MethodCallNode : RValueNode, IFunctionCallNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.Call; } }
        public override TypeWrapper TypeWrapper { get { return type; } }
        public IReadOnlyList<IExpressionNode> Args { get { return args; } }
        public IExpressionNode Function { get { return function; } }

        private TypeWrapper type;
        private List<ExpressionNode> args;
        private ExpressionNode function;
        public MethodCallNode(ExpressionNode function, TypeWrapper returnType, List<ExpressionNode> args, SequencePoint point)
            : base(point)
        {
            this.function = function;
            this.args = args;
            this.type = returnType;
        }
        public static new ExpressionNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            if (lexerNode.Children.Count(x => x.Type == Lexer.TokenType.FunctionArgumentsList) > 1)
                throw new NotImplementedException("Calling returned functions not supported");
            var function = ExpressionNode.Parse(parser, parent, lexerNode.Children[0]);
            var args = new List<ExpressionNode>();
            var point = parser.GetSequencePoint(lexerNode);
            foreach (var node in lexerNode.Children[1].Children)
            {
                switch(node.Type)
                {
                    case Lexer.TokenType.LeftParenthesis:
                    case Lexer.TokenType.RightParenthesis:
                    case Lexer.TokenType.Comma:
                        break;
                    case Lexer.TokenType.Value:
                        args.Add(ExpressionNode.Parse(parser, parent, node));
                        break;
                    default:
                        throw new ParseException(parser.GetSequencePoint(node), "Unexpected node type {0} in call", node.Type);
                }
                
            }

            var method = AsObjectCreation(parser, function, args, point);
            if (method != null)
                return method;

            method = AsMethod(parser, function, args, point);
            if (method != null)
                return method;

            method = AsFunctor(function, args, point);
            if (method != null)
                return method;

            if (method == null)
                throw new TypeException(point, "Unable to call {0} as a method or constructor", lexerNode.FullContent);

            return method;
        }
        private static ExpressionNode AsFunctor(ExpressionNode node, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            if (node.TypeWrapper == null)
                return null;
            if(node.TypeWrapper.IsFunctorType())
            {
                if (node.TypeWrapper.MatchesArgumentList(args.Select(a => a.TypeWrapper)))
                    return new MethodCallNode(node, node.TypeWrapper.FunctorReturnType, args.ToList(), point);
                else
                    return null;
            }
            else
            {
                return null;
            }
        }
        private static ExpressionNode AsMethod(Parser parser, ExpressionNode node, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            if (node is MethodNode)
                return new MethodCallNode(node, node.TypeWrapper.FunctorReturnType, args.ToList(), point);

            var ambiguous = node as AmbiguousMethodNode;
            if (ambiguous == null)
                return null;

            var method = ambiguous.RemoveAmbiguity(parser, new FunctorTypeWrapper(parser.Assembly, null, args.Select(a => a.TypeWrapper)));
            return new MethodCallNode(method, method.TypeWrapper.FunctorReturnType, args.ToList(), point);
        }
        private static ExpressionNode AsObjectCreation(Parser parser, ExpressionNode node, IEnumerable<ExpressionNode> args, SequencePoint point)
        {
            var type = node as TypeNode;
            if (type == null)
                return null;

            var method = AssemblyRegistry.GetCompatibleConstructor(parser.Assembly, type.ParsedType.TypeReference, args.Select(a => a.ExpressionReturnType).ToList());
            if (method == null)
                return null;
            return new ObjectCreationNode(type.ParsedType, args.ToList(), new ExternalMethod(parser.Assembly, method), point);
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("MethodCall:");
            builder.Indent(indent + 1).AppendFormat("ReturnType: {0}", TypeWrapper);
            builder.Indent(indent + 1).Append("Args:");
            foreach(var arg in args)
            {
                builder.AppendLine(arg.ToString(indent + 2));
            }
            builder.Indent(indent + 1).AppendLine("Function:");
            builder.AppendLine(function.ToString(indent + 2));
            return builder.ToString();
        }
    }
}
