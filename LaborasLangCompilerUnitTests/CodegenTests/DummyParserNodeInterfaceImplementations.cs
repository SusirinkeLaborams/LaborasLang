﻿using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LaborasLangCompilerUnitTests.CodegenTests
{
    class ThisNode : IExpressionNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.This; } }

        public TypeReference ExpressionReturnType { get; set; }
    }
    class NullNode : IExpressionNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Null; } }
        public TypeReference ExpressionReturnType { get { return NullType.Instance; } }
    }

    class LiteralNode : ILiteralNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Literal; } }

        public TypeReference ExpressionReturnType { get; set; }
        public Literal Value { get; set; }

        public LiteralNode()
        {
        }

        public LiteralNode(TypeReference literalType, IConvertible value)
        {
            ExpressionReturnType = literalType;
            Value = new Literal(value);
        }
    }

    class FunctionNode : IMethodNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Function; } }

        public TypeReference ExpressionReturnType { get; set; }
        public IExpressionNode ObjectInstance { get; set; }
        public MethodReference Method { get; set; }
    }

    class MethodCallNode : IFunctionCallNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        private List<IExpressionNode> arguments;

        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Call; } }

        private TypeReference expressionReturnType;
        public TypeReference ExpressionReturnType
        {
            get
            {
                if (expressionReturnType != null)
                    return expressionReturnType;

                var methodNode = Function as IMethodNode;
                if (methodNode != null)
                    return methodNode.Method.GetReturnType();

                var functorType = Function.ExpressionReturnType;

                if (functorType != null)
                    return functorType.Resolve().Methods.SingleOrDefault(method => method.Name == "Invoke").GetReturnType();

                return null;
            }
            set
            {
                expressionReturnType = value;
            }
        }
        public IExpressionNode Function { get; set; }

        public IReadOnlyList<IExpressionNode> Args
        {
            get { return arguments; }
            set
            {
                if (value is List<IExpressionNode>)
                {
                    arguments = (List<IExpressionNode>)value;
                }
                else
                {
                    arguments = value.ToList();
                }
            }
        }
    }

    class ValueCreationNode : IExpressionNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ValueCreation; } }
        public TypeReference ExpressionReturnType { get; set; }

        public ValueCreationNode()
        {
        }

        public ValueCreationNode(TypeReference type)
        {
            ExpressionReturnType = type;
        }
    }

    class ObjectCreationNode : IObjectCreationNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        private List<IExpressionNode> arguments;

        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ObjectCreation; } }

        public TypeReference ExpressionReturnType { get; set; }
        public MethodReference Constructor { get; set; }

        public IReadOnlyList<IExpressionNode> Args
        {
            get { return arguments; }
            set
            {
                if (value is List<IExpressionNode>)
                {
                    arguments = (List<IExpressionNode>)value;
                }
                else
                {
                    arguments = value.ToList();
                }
            }
        }
    }

    class ArrayCreationNode : IArrayCreationNode
    {
        public NodeType Type { get { return NodeType.Expression; } }
        public SequencePoint SequencePoint { get { return null; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ArrayCreation; } }

        public TypeReference ExpressionReturnType { get; set; }
        public IReadOnlyList<IExpressionNode> Dimensions { get; set; }
        public IReadOnlyList<IExpressionNode> Initializer { get; set; }
    }

    class ArrayAccessNode : IArrayAccessNode
    {
        public NodeType Type { get { return NodeType.Expression; } }
        public SequencePoint SequencePoint { get { return null; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ArrayAccess; } }

        public TypeReference ExpressionReturnType { get; set; }
        public IExpressionNode ObjectInstance { get; set; }
        public IReadOnlyList<IExpressionNode> Indices { get; set; }
    }

    class IndexOperatorNode : IIndexOperatorNode
    {
        public NodeType Type { get { return NodeType.Expression; } }
        public SequencePoint SequencePoint { get { return null; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.IndexOperator; } }
        public TypeReference ExpressionReturnType { get { return Property.PropertyType; } }

        public IExpressionNode ObjectInstance { get; set; }
        public PropertyReference Property { get; set; }
        public IReadOnlyList<IExpressionNode> Indices { get; set; }
    }

    class LocalVariableNode : ILocalVariableNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.LocalVariable; } }

        public TypeReference ExpressionReturnType { get { return LocalVariable.VariableType; } }
        public VariableDefinition LocalVariable { get; set; }

        public LocalVariableNode(VariableDefinition variable)
        {
            LocalVariable = variable;
        }
    }

    class FieldNode : IFieldNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Field; } }

        public TypeReference ExpressionReturnType { get { return Field.FieldType; } }
        public IExpressionNode ObjectInstance { get; set; }
        public FieldReference Field { get; set; }

        public FieldNode()
        {
        }

        public FieldNode(FieldReference field)
        {
            Field = field;
        }
    }

    class PropertyNode : IPropertyNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Property; } }

        public TypeReference ExpressionReturnType { get; private set; }
        public IExpressionNode ObjectInstance { get; set; }
        public PropertyReference Property { get; set; }
        
        public PropertyNode(AssemblyEmitter assembly, PropertyReference property)
        {
            Property = property;
            ExpressionReturnType = AssemblyRegistry.GetPropertyType(assembly, property);
        }
    }

    class ParameterNode : IParameterNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.FunctionArgument; } }

        public TypeReference ExpressionReturnType { get { return Parameter.ParameterType; } }
        public ParameterDefinition Parameter { get; set; }

        public ParameterNode(ParameterDefinition parameter)
        {
            Parameter = parameter;
        }
    }

    class BinaryOperatorNode : IBinaryOperatorNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.BinaryOperator; } }

        public TypeReference ExpressionReturnType { get; set; }
        public BinaryOperatorNodeType BinaryOperatorType { get; set; }
        public IExpressionNode LeftOperand { get; set; }
        public IExpressionNode RightOperand { get; set; }
    }

    class UnaryOperatorNode : IUnaryOperatorNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.UnaryOperator; } }

        public TypeReference ExpressionReturnType { get; set; }
        public UnaryOperatorNodeType UnaryOperatorType { get; set; }
        public IExpressionNode Operand { get; set; }
    }

    class IncrementDecrementOperatorNode : IIncrementDecrementOperatorNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.IncrementDecrementOperator; } }

        public TypeReference ExpressionReturnType { get { return Operand.ExpressionReturnType; } }
        public IncrementDecrementOperatorType IncrementDecrementType { get; set; }
        public IExpressionNode Operand { get; set; }
        public MethodReference OverloadedOperatorMethod { get; set; }
    }

    class AssignmentOperatorNode : IAssignmentOperatorNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.AssignmentOperator; } }

        public TypeReference ExpressionReturnType { get { return LeftOperand.ExpressionReturnType; } }
        public IExpressionNode LeftOperand { get; set; }
        public IExpressionNode RightOperand { get; set; }
    }

    class SymbolDeclarationNode : ISymbolDeclarationNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.SymbolDeclaration; } }

        public TypeReference ReturnType { get { return Variable.VariableType; } }
        public VariableDefinition Variable { get; set; }
        public IExpressionNode Initializer { get; set; }
    }

    class CodeBlockNode : ICodeBlockNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        private List<IParserNode> nodes;

        public NodeType Type { get { return NodeType.CodeBlockNode; } }
        public IReadOnlyList<IParserNode> Nodes
        {
            get { return nodes; }
            set
            {
                if (value is List<IParserNode>)
                {
                    nodes = (List<IParserNode>)value;
                }
                else
                {
                    nodes = value.ToList();
                }
            }
        }
    }

    class ConditionBlockNode : IConditionBlock
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.ConditionBlock; } }

        public IExpressionNode Condition { get; set; }
        public ICodeBlockNode TrueBlock { get; set; }
        public ICodeBlockNode FalseBlock { get; set; }
    }

    class WhileBlockNode : IWhileBlockNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.WhileBlock; } }

        public IExpressionNode Condition { get; set; }
        public ICodeBlockNode ExecutedBlock { get; set; }
    }

    class ForLoopNode : IForLoopNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.ForLoop; } }

        public ICodeBlockNode InitializationBlock { get; set; }
        public IExpressionNode ConditionBlock { get; set; }
        public ICodeBlockNode IncrementBlock { get; set; }
        public ICodeBlockNode Body { get; set; }
    }

    class ForEachLoop : IForEachLoopNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.ForEachLoop; } }

        public IExpressionNode Collection { get; set; }
        public ISymbolDeclarationNode LoopVariable { get; set; }
        public ICodeBlockNode Body { get; set; }
    }

    class ReturnNode : IReturnNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.ReturnNode; } }

        public IExpressionNode Expression { get; set; }
    }

    class CastNode : ICastNode
    {
        public SequencePoint SequencePoint { get { return null; } }
        public NodeType Type { get { return NodeType.Expression; } }
        public ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Cast; } }

        public TypeReference ExpressionReturnType { get; set; }
        public IExpressionNode TargetExpression { get; set; }

        public CastNode()
        {
        }

        public CastNode(IExpressionNode expression, TypeReference targetType)
        {
            ExpressionReturnType = targetType;
            TargetExpression = expression;
        }
    }
}
