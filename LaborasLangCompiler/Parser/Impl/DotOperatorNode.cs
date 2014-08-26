﻿using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class DotOperatorNode
    {
        IExpressionNode builtNode;
        Parser parser;
        ClassNode cls;
        IContainerNode parent;

        private DotOperatorNode(Parser parser, IContainerNode parent)
        {
            this.parser = parser;
            this.parent = parent;
            this.cls = parent.GetClass();
        }
        public static DotOperatorNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var instance = new DotOperatorNode(parser, parent);
            foreach(var node in lexerNode.Children)
            {
                instance.Append(ExpressionNode.Parse(parser, parent, node));
            }
            return instance;
        }
        private void Append(IExpressionNode node)
        {
            if(node is ExpressionNode)
            {
                if (!AppendExpression((ExpressionNode)node))
                    throw new ParseException(node.SequencePoint, "Expressions only allowed on left of dot operator");
            }
            else if (node is SymbolCallNode)
            {
                if (!AppendCall((SymbolCallNode)node))
                    throw new SymbolNotFoundException(node.SequencePoint, "Symbol {0} not found", ((SymbolCallNode)node).Value);
            }
            else if(node is SymbolNode)
            {
                var nod = (SymbolNode)node;
                if (AppendLValue(nod))
                    return;
                if (AppendMethod(nod))
                    return;
                if (AppendType(nod))
                    return;
                if (AppendNamespace(nod))
                    return;
                throw new SymbolNotFoundException(node.SequencePoint, "Symbol {0} not found", nod.Value);
            }
        }
        private bool AppendCall(SymbolCallNode node)
        {
            var types = node.Arguments.Select(arg => arg.ReturnType).ToList();
            if (AppendLValue(node))
            {
                if (!builtNode.ReturnType.IsFunctorType())
                    return false;

                var returnType = ILTools.ILHelpers.GetFunctorReturnType(parser.Assembly, builtNode.ReturnType);
                if(ILHelpers.MatchesArgumentList(builtNode.ReturnType, types))
                {
                    builtNode = new MethodCallNode(builtNode, returnType, node.Arguments, node.SequencePoint);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (AppendMethod(node))
            {
                var method = ((AmbiguousMethodNode)builtNode).RemoveAmbiguity(parser, types);
                builtNode = new MethodCallNode(method, method.Function.ReturnType, node.Arguments, node.SequencePoint);
                return true;
            }
            return false;
        }
        private bool AppendMethod(SymbolNode node)
        {
            if(builtNode == null)
            {
                //metodu kaip ir neturim dar
                return false;
            }
            else
            {
                if (builtNode is NamespaceNode)
                    return false;
                
                if(builtNode is TypeNode)
                {
                    //static methods
                    var methods = AssemblyRegistry.GetMethods(parser.Assembly, ((TypeNode)builtNode).ParsedType, node.Value);
                    methods = methods.Where(m => m.Resolve().IsStatic).ToList();
                    if (methods != null && methods.Count != 0)
                    {
                        builtNode = new AmbiguousMethodNode(methods, null, builtNode.SequencePoint);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    //non-static methods
                    var methods = AssemblyRegistry.GetMethods(parser.Assembly, builtNode.ReturnType, node.Value);
                    methods = methods.Where(m => !m.Resolve().IsStatic).ToList();
                    if (methods != null && methods.Count != 0)
                    {
                        builtNode = new AmbiguousMethodNode(methods, builtNode, builtNode.SequencePoint);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        private bool AppendType(SymbolNode node)
        {
            if(builtNode == null)
            {
                builtNode = cls.FindType(node.Value, node.SequencePoint);
                return builtNode != null;
            }
            else
            {
                if(builtNode is NamespaceNode)
                {
                    var found = cls.FindType((NamespaceNode) builtNode, node.Value, node.SequencePoint);
                    if(found != null)
                    {
                        builtNode = found;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if(builtNode is TypeNode)
                {
                    var found = cls.FindType((TypeNode)builtNode, node.Value, node.SequencePoint);
                    if (found != null)
                    {
                        builtNode = found;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
        }
        private bool AppendNamespace(SymbolNode node)
        {
            if (builtNode == null)
            {
                builtNode = cls.FindNamespace(node.Value, node.SequencePoint);
                return builtNode != null;
            }
            else
            {
                if(builtNode is NamespaceNode)
                {
                    var found = cls.FindNamespace((NamespaceNode)builtNode, node.Value, node.SequencePoint);
                    if(found != null)
                    {
                        builtNode = found;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        private bool AppendLValue(SymbolNode node)
        {
            string name = node.Value;
            if(builtNode == null)
            {
                return (builtNode = parent.GetSymbol(name, node.SequencePoint)) != null;
            }
            else
            {
                if(builtNode is NamespaceNode)
                    return false;
                FieldReference field = null;
                if(builtNode is TypeNode)
                {
                    var type = ((TypeNode)builtNode).ParsedType;
                    field = AssemblyRegistry.GetField(parser.Assembly, type, name);
                    if (field != null && !field.Resolve().IsStatic)
                        field = null;
                }
                else
                {
                    var type = builtNode.ReturnType;
                    field = AssemblyRegistry.GetField(parser.Assembly, type, name);
                    if (field != null && field.Resolve().IsStatic)
                        field = null;
                }

                if(field != null)
                {
                    builtNode = new FieldNode(builtNode, new ExternalField(field), builtNode.SequencePoint);
                    return true;
                }
                return false;
            }
        }
        private bool AppendExpression(ExpressionNode node)
        {
            if(builtNode == null)
            {
                builtNode = node;
                return true;
            }
            else
            {
                return false;
            }
        }
        public ExpressionNode ExtractExpression()
        {
            if (builtNode is NamespaceNode || builtNode is TypeNode)
                throw new ParseException(builtNode.SequencePoint, "Expression expected");
            else
                return (ExpressionNode)builtNode;
        }
        public string ExtractNamespace()
        {
            if (!(builtNode is NamespaceNode))
                throw new ParseException(builtNode.SequencePoint, "Namespace expected");
            else
                return ((NamespaceNode)builtNode).Value;
        }
        public TypeReference ExtractType()
        {
            if (!(builtNode is TypeNode))
                throw new ParseException(builtNode.SequencePoint, "Type expected");
            else
                return ((TypeNode)builtNode).ParsedType;
        }
        public LValueNode ExtractLValue()
        {
            if (!(builtNode is LValueNode))
                throw new ParseException(builtNode.SequencePoint, "LValue expected");
            else
                return (LValueNode)builtNode;
        }
        public IExpressionNode ExtractMethod(List<TypeReference> types)
        {
            if (builtNode is LValueNode)
            {
                if (builtNode.ReturnType.IsFunctorType())
                {
                    if (ILHelpers.MatchesArgumentList(builtNode.ReturnType, types))
                    {
                        return (IExpressionNode)builtNode;
                    }
                }
            }
            if (builtNode is AmbiguousMethodNode)
            {
                var method = ((AmbiguousMethodNode)builtNode).RemoveAmbiguity(parser, types);
                return method;
            }
            throw new ParseException(builtNode.SequencePoint, "Method expected");
        }
    }
}
