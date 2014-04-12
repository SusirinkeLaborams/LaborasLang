﻿using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Tree;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    abstract class LValueNode : ExpressionNode, ILValueNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.LValue; } }
        public abstract LValueNodeType LValueType { get; }

        public static new LValueNode Parse(Parser parser, CodeBlockNode parent, AstNode lexerNode)
        {
            if(lexerNode.Token.Name == "Symbol")
            {
                var value = parser.GetNodeValue(lexerNode);
                var instance = parent.GetSymbol(value);
                if(instance == null)
                    parser.Root.GetSymbol(value);
                if (instance != null)
                    return instance;
                else
                    throw new SymbolNotFoundException("Symbol " + value + " not found");
            }
            throw new NotImplementedException();
        }
    }

    class LocalVariableNode : LValueNode, ILocalVariableNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.LocalVariable; } }
        public VariableDefinition LocalVariable { get; private set; }
        public override TypeReference ReturnType { get { return LocalVariable.VariableType; } }
        public LocalVariableNode(VariableDefinition variable)
        {
            LocalVariable = variable;
        }
    }

    class FunctionArgumentNode : LValueNode, IFunctionArgumentNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.FunctionArgument; } }
        public ParameterDefinition Param { get; private set; }
        public override TypeReference ReturnType { get { return Param.ParameterType; } }
        public FunctionArgumentNode(ParameterDefinition param)
        {
            Param = param;
        }
    }

    class FieldNode : LValueNode, IFieldNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Field; } }
        public IExpressionNode ObjectInstance { get; private set; }
        public FieldDefinition Field { get; private set; }
        public override TypeReference ReturnType { get { return Field.FieldType; } }
        public FieldNode(ExpressionNode instance, FieldDefinition field)
        {
            ObjectInstance = instance;
            Field = field;
        }
    }

    /*class PropertyNode : LValueNode, IPropertyNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Property; } }
        public IExpressionNode ObjectInstance { get; }
        public PropertyDefinition Property { get; }
    }*/
}
