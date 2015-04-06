﻿using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.PostProcessors
{
    abstract class PostProcessor
    {

        public static IEnumerable<PostProcessor> BuildAll()
        {
            return new PostProcessor[]{new ArrayFunctionResolver(), new PostfixResolver(), new PrefixResolver(), new InfixResolver(), new FullSymbolPostProcessor()};
        }

        public PostProcessor()
        {
        }

        public virtual void Apply(AbstractSyntaxTree tree)
        {
            tree.Children.ForEach(t =>
            {
                Transform(t);
                Apply(t);
            });
        }

        public abstract void Transform(AbstractSyntaxTree astNode);

    }
}
