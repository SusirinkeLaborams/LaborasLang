﻿using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    interface FieldWrapper
    {
        FieldReference FieldReference { get; }
        TypeReference ReturnType { get; }
        string Name { get; }
    }
}