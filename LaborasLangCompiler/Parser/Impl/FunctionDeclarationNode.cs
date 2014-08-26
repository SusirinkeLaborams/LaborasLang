﻿using LaborasLangCompiler.ILTools.Methods;
using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Exceptions;
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
    class FunctionDeclarationNode : RValueNode, IFunctionNode, IContainerNode
    {
        public IExpressionNode ObjectInstance { get { return null; } }
        public MethodReference Function { get; private set; }
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public override TypeReference ReturnType { get { return header.ReturnType; } }
        private CodeBlockNode body;
        private MethodEmitter emitter;
        public TypeReference FunctionReturnType { get { return header.FunctorReturnType; } }
        private ClassNode parent;
        public IReadOnlyList<FunctionArgumentNode> Args { get { return header.Args; } }
        private Dictionary<string, ParameterDefinition> symbols;
        private FunctionHeader header;
        public void Emit(bool entry = false)
        {
            emitter.ParseTree(body);
            if(entry)
                emitter.SetAsEntryPoint();
        }
        private FunctionDeclarationNode(IContainerNode parent, SequencePoint point)
            : base(point)
        {
            this.parent = parent.GetClass();
            this.symbols = new Dictionary<string, ParameterDefinition>();
        }
        public FunctionDeclarationNode GetFunction() { return this; }
        public ClassNode GetClass() { return parent.GetClass(); }
        public LValueNode GetSymbol(string name, SequencePoint point)
        {
            if (symbols.ContainsKey(name))
                return new FunctionArgumentNode(symbols[name], true, point);

            return parent.GetSymbol(name, point); 
        }
        public static FunctionDeclarationNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode, string name = null)
        {
            var instance = new FunctionDeclarationNode(parent, parser.GetSequencePoint(lexerNode));
            var header = FunctionHeader.Parse(parser, parent, lexerNode.Children[0]);
            if (name == null)
                name = instance.parent.NewFunctionName();
            instance.header = header;
            instance.symbols = instance.Args.Select(a => a.Param).ToDictionary(arg => arg.Name);
            instance.body = CodeBlockNode.Parse(parser, instance, lexerNode.Children[1]);
            if (instance.FunctionReturnType.FullName != "System.Void" && !instance.body.Returns)
                throw new ParseException(instance.SequencePoint, "Not all control paths return a value");
            instance.emitter = new MethodEmitter(instance.parent.TypeEmitter, "$" + name, header.FunctorReturnType, MethodAttributes.Static | MethodAttributes.Private);
            foreach (var arg in header.Args)
                instance.emitter.AddArgument(arg.Param);
            instance.Function = instance.emitter.Get();
            instance.parent.AddMethod(instance, name);
            return instance;
        }
        public static TypeReference ParseType(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            return FunctionHeader.Parse(parser, parent, lexerNode.Children[0]).ReturnType;
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("(Function: ");
            builder.Append(ReturnType).Append("(");
            string delim = "";
            foreach(var arg in emitter.Get().Parameters)
            {
                builder.Append(String.Format("{0}{1} {2}", delim, arg.ParameterType, arg.Name));
                delim = ", ";
            }
            builder.Append(")").Append(body.ToString()).Append(")");
            return builder.ToString();
        }
    }
}
