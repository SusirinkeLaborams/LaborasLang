﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer.PostProcessors
{
    class PrefixResolver : PostProcessor
    {
        public override void Transform(AbstractSyntaxTree astNode)
        {
            if (astNode.Type == TokenType.PrefixNode)
            {
                if (astNode.Children.Count == 1)
                {
                    astNode.Collapse();
                    return;
                }
                else
                {
                    var source = astNode.Children;
                    var prefix = new AbstractSyntaxTree(new Node(TokenType.PrefixNode), source.GetRange(1, source.Count - 1));

                    astNode.Children = new List<AbstractSyntaxTree>() { source.First(), prefix };
                }
            }

        }
    }
}
