﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    interface IContainerNode
    {
        FunctionDeclarationNode GetFunction();
        ClassNode GetClass();
        LValueNode GetSymbol(string name);
    }
}
