﻿using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ProjectParser : IDisposable
    {
        public AssemblyEmitter Assembly { get; private set; }
        public bool ShouldEmit { get; private set; }

        private Dictionary<string, TypeReference> aliases;
        private HashSet<TypeReference> primitives;

        public IReadOnlyDictionary<ulong, TypeReference> MaxValues { get; private set; }
        public IReadOnlyDictionary<long, TypeReference> MinValues { get; private set; }

        private List<Parser> parsers;

        #region types

        public TypeReference Bool { get; private set; }
        public TypeReference Char { get; private set; }
        public TypeReference Int8 { get; private set; }
        public TypeReference UInt8 { get; private set; }
        public TypeReference Int16 { get; private set; }
        public TypeReference UInt16 { get; private set; }
        public TypeReference Int32 { get; private set; }
        public TypeReference UInt32 { get; private set; }
        public TypeReference Int64 { get; private set; }
        public TypeReference UInt64 { get; private set; }
        public TypeReference Float { get; private set; }
        public TypeReference Double { get; private set; }
        public TypeReference Decimal { get; private set; }
        public TypeReference String { get; private set; }
        public TypeReference Void { get; private set; }
        public TypeReference Auto { get; private set; }
        public TypeReference Object { get; private set; }

        #endregion types

        private ProjectParser(AssemblyEmitter assembly, bool emit)
        {
            Assembly = assembly;
            ShouldEmit = emit;
            this.aliases = new Dictionary<string, TypeReference>();
            this.parsers = new List<Parser>();

            aliases["bool"] = Bool = assembly.TypeToTypeReference(typeof(bool));

            aliases["char"] = Char = assembly.TypeToTypeReference(typeof(char));
            aliases["int8"] = Int8 = assembly.TypeToTypeReference(typeof(sbyte));
            aliases["uint8"] = UInt8 = assembly.TypeToTypeReference(typeof(byte));

            aliases["int16"] = Int16 = assembly.TypeToTypeReference(typeof(short));
            aliases["uint16"] = UInt16 = assembly.TypeToTypeReference(typeof(ushort));

            aliases["int32"] = aliases["int"] = Int32 = assembly.TypeToTypeReference(typeof(int));
            aliases["uint32"] = aliases["uint"] = UInt32 = assembly.TypeToTypeReference(typeof(uint));

            aliases["int64"] = aliases["long"] = Int64 = assembly.TypeToTypeReference(typeof(long));
            aliases["uint64"] = aliases["ulong"] = UInt64 = assembly.TypeToTypeReference(typeof(ulong));

            aliases["float"] = Float = assembly.TypeToTypeReference(typeof(float));
            aliases["double"] = Double = assembly.TypeToTypeReference(typeof(double));
            aliases["decimal"] = Decimal = assembly.TypeToTypeReference(typeof(decimal));

            aliases["string"] = String = assembly.TypeToTypeReference(typeof(string));
            aliases["object"] = Object = assembly.TypeToTypeReference(typeof(object));

            aliases["void"] = Void = assembly.TypeToTypeReference(typeof(void));
            aliases["auto"] = Auto = AutoType.Instance;

            primitives = new HashSet<TypeReference>(Utils.Utils.Enumerate(Bool, Char, Int8, UInt8, Int16, UInt16, Int32, UInt32, Int64, UInt64, Float, Double, Decimal, String));

            MaxValues = new Dictionary<ulong, TypeReference>()
            {
                {(ulong)sbyte.MaxValue, Int8},
                {byte.MaxValue, UInt8},
                {(ulong)short.MaxValue, Int16},
                {ushort.MaxValue, UInt16},
                {int.MaxValue, Int32},
                {uint.MaxValue, UInt32},
                {long.MaxValue, Int64},
                {ulong.MaxValue, UInt64}
            };

            MinValues = new Dictionary<long, TypeReference>()
            {
                {sbyte.MinValue, Int8},
                {short.MinValue, Int16},
                {int.MinValue, Int32},
                {long.MinValue, Int64},
            };
        }

        public static ProjectParser ParseAll(AssemblyEmitter assembly, string[] sources, string[] names, bool emit)
        {
            
            ProjectParser projectParser = new ProjectParser(assembly, emit);
            for(int i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                var name = names[i];
                projectParser.parsers.Add(new Parser(projectParser, Lexer.Lexer.Lex(source), name));
            }

            projectParser.parsers.ForEach(p => p.Root.ParseDeclarations());
            projectParser.parsers.ForEach(p => p.Root.ParseInitializers());
            projectParser.parsers.ForEach(p => p.Root.Emit());
            return projectParser;
        }

        public static ProjectParser ParseAll(AssemblyEmitter assembly, string[] files, bool emit)
        {
            return ParseAll(assembly, files.Select(f => File.ReadAllText(f)).ToArray(), files, emit);
        }

        public TypeReference FindType(string fullname)
        {
            if (aliases.ContainsKey(fullname))
                return aliases[fullname];
            else
                return AssemblyRegistry.FindType(Assembly, fullname);
        }

        public Namespace FindNamespace(string fullname)
        {
            if (AssemblyRegistry.IsNamespaceKnown(fullname))
                return new Namespace(fullname, Assembly);
            else
                return null;
        }

        public bool IsPrimitive(TypeReference type)
        {
            return primitives.Contains(type);
        }

        public void Dispose()
        {
            foreach(var parser in parsers)
            {
                parser.Dispose();
            }
        }

        public override string ToString()
        {
            return string.Join("\r\n", parsers.Select(p => p.Root.ToString(0)));
        }
    }
}
