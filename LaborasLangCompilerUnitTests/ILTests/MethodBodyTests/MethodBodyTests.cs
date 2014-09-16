﻿using LaborasLangCompiler.FrontEnd;
using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using LaborasLangCompiler.ILTools.Types;
using LaborasLangCompiler.ILTools.Methods;

namespace LaborasLangCompilerUnitTests.ILTests.MethodBodyTests
{
    [TestClass]
    public class MethodBodyTests : ILTestBase
    {
        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_EmptyMethod()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>()
            };

            ExpectedILFilePath = "TestCanEmit_EmptyMethod.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_HelloWorld()
        {
            BodyCodeBlock = new CodeBlockNode
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Method = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", 
                                new List<string>() { "System.String" })
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(string)),
                                Value = "Hello, world!"
                            }
                        }
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = new MethodCallNode()
                        {
                            Function = new FunctionNode()
                            {
                                Method = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "ReadKey",
                                    new List<TypeReference>())
                            },
                            Args = new List<IExpressionNode>()
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_HelloWorld.il";
            ExecuteAndAssertSuccess();
        }

        #region Load/Store lvalues and load literals tests

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_VariableDeclarationAndInitialization_LoadFloatLiteral()
        {
            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = new VariableDefinition("floatValue", assemblyEmitter.TypeToTypeReference(typeof(float)))                            
                        },
                        Initializer = new LiteralNode()
                        {
                            ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(float)),
                            Value = 2.5
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_VariableDeclarationAndInitialization_LoadFloatLiteral.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_StoreField_LoadIntLiteral()
        {
            var field = new FieldDefinition("intField", FieldAttributes.Static, assemblyEmitter.TypeToTypeReference(typeof(int)));
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new LiteralNode()
                            {
                                ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(int)),
                                Value = 1
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_StoreField_LoadIntLiteral.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_StoreLocalVariable_LoadField()
        {
            var field = new FieldDefinition("intField", FieldAttributes.Static, assemblyEmitter.TypeToTypeReference(typeof(int)));
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = new VariableDefinition("intLocal", assemblyEmitter.TypeToTypeReference(typeof(int)))
                        },
                        Initializer = new FieldNode()
                        {
                            Field = field
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_StoreLocalVariable_LoadField.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_StoreProperty_LoadLocalVariable_LoadArgument_LoadDoubleLiteral()
        {
            var property = new PropertyDefinition("doubleProperty", PropertyAttributes.HasDefault, assemblyEmitter.TypeToTypeReference(typeof(double)));
            var backingField = new FieldDefinition("doubleProperty_backingField", FieldAttributes.Static, assemblyEmitter.TypeToTypeReference(typeof(double)));

            typeEmitter.AddField(backingField);

            var setter = new MethodEmitter(typeEmitter, "set_doubleProperty", assemblyEmitter.TypeToTypeReference(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Private);

            var argument = setter.AddArgument(assemblyEmitter.TypeToTypeReference(typeof(double)), "value");
            setter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = backingField
                            },
                            RightOperand = new FunctionArgumentNode()
                            {
                                Param = argument,
                                IsMethodStatic = true
                            }
                        }
                    }
                }
            });

            property.SetMethod = setter.Get().Resolve();

            typeEmitter.AddProperty(property);

            var localVariable = new VariableDefinition("doubleLocal", assemblyEmitter.TypeToTypeReference(typeof(double)));

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = localVariable,
                        },
                        Initializer = new LiteralNode
                        {
                            ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(double)),
                            Value = 5.5
                        }
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new PropertyNode()
                            {
                                Property = property
                            },
                            RightOperand = new LocalVariableNode()
                            {
                                LocalVariable = localVariable
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_StoreProperty_LoadLocalVariable_LoadArgument_LoadDoubleLiteral.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_StoreArgument_LoadProperty_LoadStringLiteral()
        {
            var property = new PropertyDefinition("stringProperty", PropertyAttributes.HasDefault, assemblyEmitter.TypeToTypeReference(typeof(string)));

            var getter = new MethodEmitter(typeEmitter, "get_stringProperty", assemblyEmitter.TypeToTypeReference(typeof(string)),
                MethodAttributes.Static | MethodAttributes.Private);

            getter.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new LiteralNode()
                    {
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(string)),
                        Value = "Test"
                    }
                }
            });

            property.GetMethod = getter.Get().Resolve();
            typeEmitter.AddProperty(property);

            var methodWithArgument = new MethodEmitter(typeEmitter, "TestMethod", assemblyEmitter.TypeToTypeReference(typeof(void)),
                MethodAttributes.Static | MethodAttributes.Private);

            var argument = methodWithArgument.AddArgument(assemblyEmitter.TypeToTypeReference(typeof(string)), "arg");

            methodWithArgument.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FunctionArgumentNode()
                            {
                                Param = argument
                            },
                            RightOperand = new PropertyNode()
                            {
                                Property = property
                            }
                        }
                    }
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Method = methodWithArgument.Get()
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(string)),
                                Value = "Test"
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_StoreArgument_LoadProperty_LoadStringLiteral.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_CallFunction_PassArgument_LoadBoolLiteral()
        {
            var callableMethod = new MethodEmitter(typeEmitter, "Test", assemblyEmitter.TypeToTypeReference(typeof(void)),
                MethodAttributes.Private | MethodAttributes.Static);

            callableMethod.AddArgument(assemblyEmitter.TypeToTypeReference(typeof(bool)), "isTrue");
            callableMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        Function = new FunctionNode()
                        {
                            Method = callableMethod.Get()
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode
                            {
                                ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(bool)),
                                Value = true
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_CallFunction_PassArgument_LoadBoolLiteral.il";
            ExecuteAndAssertSuccess();
        }

        #endregion

        #region Operator tests

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_MultipleNestedAssignments()
        {
            var assignmentNode = new AssignmentOperatorNode()
            {
                RightOperand = new LiteralNode()
                {
                    ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(int)),
                    Value = 110
                }
            };

            const int count = 10;
            for (int i = 0; i < count; i++)
            {
                var field = new FieldDefinition("intField" + i.ToString(), FieldAttributes.Static | FieldAttributes.Private,
                    assemblyEmitter.TypeToTypeReference(typeof(int)));
                typeEmitter.AddField(field);

                assignmentNode.LeftOperand = new FieldNode()
                {
                    Field = field
                };

                if (i != count - 1)
                {
                    var newNode = new AssignmentOperatorNode()
                    {
                        RightOperand = assignmentNode
                    };

                    assignmentNode = newNode;
                }
            }

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = assemblyEmitter.TypeToTypeReference(typeof(void)),
                        Operand = assignmentNode
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_MultipleNestedAssignments.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_MultipleNestedInstanceFieldAssignments()
        {
            var floatType = assemblyEmitter.TypeToTypeReference(typeof(float));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var testMethod = new MethodEmitter(typeEmitter, "TestNestedInstanceFieldAssignment", voidType);

            var assignmentNode = new AssignmentOperatorNode()
            {
                RightOperand = new LiteralNode()
                {
                    ExpressionReturnType = floatType,
                    Value = 110
                }
            };

            const int count = 10;
            for (int i = 0; i < count; i++)
            {
                var field = new FieldDefinition("intField" + i.ToString(), FieldAttributes.Private, floatType);
                typeEmitter.AddField(field);

                assignmentNode.LeftOperand = new FieldNode()
                {
                    ObjectInstance = new ThisNode
                    {
                        ExpressionReturnType = field.DeclaringType
                    },
                    Field = field
                };

                if (i != count - 1)
                {
                    var newNode = new AssignmentOperatorNode()
                    {
                        RightOperand = assignmentNode
                    };

                    assignmentNode = newNode;
                }
            }

            testMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = voidType,
                        Operand = assignmentNode
                    }
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                }
            };

            ExpectedILFilePath = "TestCanEmit_MultipleNestedInstanceFieldAssignments.il";
            ExecuteAndAssertSuccess();
        }

        #region Binary operators

        #region Arithmetic operators

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_AddIntegers()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var localVariable = new VariableDefinition("myVar", intType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = localVariable
                        }
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = voidType,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new LocalVariableNode()
                            {
                                LocalVariable = localVariable
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = intType,
                                LeftOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = intType,
                                    Value = 2
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = intType,
                                    Value = 3
                                },
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_AddIntegers.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_AddFloatAndInteger()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var floatType = assemblyEmitter.TypeToTypeReference(typeof(float));
            var doubleType = assemblyEmitter.TypeToTypeReference(typeof(double));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var localVariable = new VariableDefinition("myVar", doubleType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = localVariable
                        }
                    },
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = voidType,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new LocalVariableNode()
                            {
                                LocalVariable = localVariable
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = floatType,
                                LeftOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = floatType,
                                    Value = 3.2f
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = intType,
                                    Value = 2
                                },
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_AddFloatAndInteger.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_AddStrings()
        {
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, stringType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        ExpressionReturnType = voidType,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = stringType,
                                LeftOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = stringType,
                                    Value = "testing string addition: "
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = intType,
                                    Value = 22
                                },
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_AddStrings.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_Subtraction()
        {
            var doubleType = assemblyEmitter.TypeToTypeReference(typeof(double));
            var floatType = assemblyEmitter.TypeToTypeReference(typeof(float));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, doubleType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = doubleType,
                                BinaryOperatorType = BinaryOperatorNodeType.Subtraction,
                                LeftOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = doubleType,
                                    Value = 3.4
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = floatType,
                                    Value = 5.5f
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_Subtraction.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_Multiplication()
        {
            var uintType = assemblyEmitter.TypeToTypeReference(typeof(uint));
            var ushortType = assemblyEmitter.TypeToTypeReference(typeof(ushort));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.Multiplication,
                                LeftOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = uintType,
                                    Value = 5
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = uintType,
                                    Value = 3
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_Multiplication.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_SignedDivision()
        {
            var doubleType = assemblyEmitter.TypeToTypeReference(typeof(double));
            var longType = assemblyEmitter.TypeToTypeReference(typeof(long));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, longType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = doubleType,
                                BinaryOperatorType = BinaryOperatorNodeType.Division,
                                LeftOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = doubleType,
                                    Value = 15.4
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = doubleType,
                                    Value = 4.8
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_SignedDivision.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_UnsignedDivision()
        {
            var uintType = assemblyEmitter.TypeToTypeReference(typeof(uint));
            var ushortType = assemblyEmitter.TypeToTypeReference(typeof(ushort));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.Division,
                                LeftOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = uintType,
                                    Value = 17
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = uintType,
                                    Value = 3
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_UnsignedDivision.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_SignedRemainder()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var longType = assemblyEmitter.TypeToTypeReference(typeof(long));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, longType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = intType,
                                BinaryOperatorType = BinaryOperatorNodeType.Modulus,
                                LeftOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = intType,
                                    Value = 94
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = intType,
                                    Value = -17
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_SignedRemainder.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_UnsignedRemainder()
        {
            var uintType = assemblyEmitter.TypeToTypeReference(typeof(uint));
            var ushortType = assemblyEmitter.TypeToTypeReference(typeof(ushort));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.Modulus,
                                LeftOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = uintType,
                                    Value = 41
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = uintType,
                                    Value = 81
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_UnsignedRemainder.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_ShiftLeftAndRight()
        {
            var uintType = assemblyEmitter.TypeToTypeReference(typeof(uint));
            var ushortType = assemblyEmitter.TypeToTypeReference(typeof(ushort));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, ushortType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.ShiftLeft,
                                LeftOperand = new BinaryOperatorNode()
                                {
                                    ExpressionReturnType = uintType,
                                    BinaryOperatorType = BinaryOperatorNodeType.ShiftRight,
                                    LeftOperand = new LiteralNode()
                                    {
                                        ExpressionReturnType = uintType,
                                        Value = 15
                                    },
                                    RightOperand = new LiteralNode()
                                    {
                                        ExpressionReturnType = ushortType,
                                        Value = 2
                                    }
                                },
                                RightOperand = new LiteralNode()
                                {
                                    ExpressionReturnType = uintType,
                                    Value = 3
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_ShiftLeftAndRight.il";
            ExecuteAndAssertSuccess();
        }

        #endregion

        #region Comparison operators

        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(TypeReference literalType, dynamic value1, dynamic value2)
        {
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var outputMethod = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<string>()
            {
                "System.String",
                literalType.FullName,
                literalType.FullName
            });

            var localA = new VariableDefinition("a", literalType);
            var localB = new VariableDefinition("b", literalType);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = localA
                        },
                        Initializer = new LiteralNode()
                        {
                            ExpressionReturnType = literalType,
                            Value = value1
                        }
                    },
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = localB
                        },
                        Initializer = new LiteralNode()
                        {
                            ExpressionReturnType = literalType,
                            Value = value2
                        }
                    },
                    new ConditionBlockNode()
                    {
                        Condition = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = boolType,
                            BinaryOperatorType = BinaryOperatorNodeType.GreaterThan,
                            LeftOperand = new LocalVariableNode()
                            {
                                LocalVariable = localA
                            },
                            RightOperand = new LocalVariableNode()
                            {
                                LocalVariable = localB
                            },
                        },
                        TrueBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                new MethodCallNode()
                                {
                                    ExpressionReturnType = voidType,
                                    Function = new FunctionNode()
                                    {
                                        Method = outputMethod,
                                    },
                                    Args = new List<IExpressionNode>()
                                    {
                                        new LiteralNode()
                                        {
                                            ExpressionReturnType = stringType,
                                            Value = "{0} is greater than {1}."
                                        },
                                        new LocalVariableNode()
                                        {
                                            LocalVariable = localA
                                        },
                                        new LocalVariableNode()
                                        {
                                            LocalVariable = localB
                                        }
                                    }
                                }
                            }
                        },
                        FalseBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                new ConditionBlockNode()
                                {
                                    Condition = new BinaryOperatorNode()
                                    {
                                        ExpressionReturnType = boolType,
                                        BinaryOperatorType = BinaryOperatorNodeType.LessThan,
                                        LeftOperand = new LocalVariableNode()
                                        {
                                            LocalVariable = localA
                                        },
                                        RightOperand = new LocalVariableNode()
                                        {
                                            LocalVariable = localB
                                        },
                                    },
                                    TrueBlock = new CodeBlockNode()
                                    {
                                        Nodes = new List<IParserNode>()
                                        {
                                            new MethodCallNode()
                                            {
                                                ExpressionReturnType = voidType,
                                                Function = new FunctionNode()
                                                {
                                                    Method = outputMethod,
                                                },
                                                Args = new List<IExpressionNode>()
                                                {
                                                    new LiteralNode()
                                                    {
                                                        ExpressionReturnType = stringType,
                                                        Value = "{0} is less than {1}."
                                                    },
                                                    new LocalVariableNode()
                                                    {
                                                        LocalVariable = localA
                                                    },
                                                    new LocalVariableNode()
                                                    {
                                                        LocalVariable = localB
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    FalseBlock = new CodeBlockNode()
                                    {
                                        Nodes = new List<IParserNode>()
                                        {
                                            new ConditionBlockNode()
                                            {
                                                Condition = new BinaryOperatorNode()
                                                {
                                                    ExpressionReturnType = boolType,
                                                    BinaryOperatorType = BinaryOperatorNodeType.Equals,
                                                    LeftOperand = new LocalVariableNode()
                                                    {
                                                        LocalVariable = localA
                                                    },
                                                    RightOperand = new LocalVariableNode()
                                                    {
                                                        LocalVariable = localB
                                                    },
                                                },
                                                TrueBlock = new CodeBlockNode()
                                                {
                                                    Nodes = new List<IParserNode>()
                                                    {
                                                        new MethodCallNode()
                                                        {
                                                            ExpressionReturnType = voidType,
                                                            Function = new FunctionNode()
                                                            {
                                                                Method = outputMethod,
                                                            },
                                                            Args = new List<IExpressionNode>()
                                                            {
                                                                new LiteralNode()
                                                                {
                                                                    ExpressionReturnType = stringType,
                                                                    Value = "{0} and {1} are equal."
                                                                },
                                                                new LocalVariableNode()
                                                                {
                                                                    LocalVariable = localA
                                                                },
                                                                new LocalVariableNode()
                                                                {
                                                                    LocalVariable = localB
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                FalseBlock = new CodeBlockNode()
                                                {
                                                    Nodes = new List<IParserNode>()
                                                    {
                                                        new MethodCallNode()
                                                        {
                                                            ExpressionReturnType = voidType,
                                                            Function = new FunctionNode()
                                                            {
                                                                Method = outputMethod,
                                                            },
                                                            Args = new List<IExpressionNode>()
                                                            {
                                                                new LiteralNode()
                                                                {
                                                                    ExpressionReturnType = stringType,
                                                                    Value = "We're screwed."
                                                                },
                                                                new LocalVariableNode()
                                                                {
                                                                    LocalVariable = localA
                                                                },
                                                                new LocalVariableNode()
                                                                {
                                                                    LocalVariable = localB
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Numerals()
        {
            TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(assemblyEmitter.TypeToTypeReference(typeof(int)), 5, 6);

            ExpectedILFilePath = "TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Numerals.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Strings()
        {
            TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Base(assemblyEmitter.TypeToTypeReference(typeof(string)), "hi", "bye");

            ExpectedILFilePath = "TestCanEmit_ConditionBlock_GreaterThan_LessThan_Equals_Strings.il";
            ExecuteAndAssertSuccess();
        }

        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(TypeReference literalType, dynamic value1, dynamic value2)
        {
            var outputMethod = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<string>()
            {
                "System.String",
                literalType.FullName,
                literalType.FullName,
                "System.Boolean"
            });

            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var booleanType = assemblyEmitter.TypeToTypeReference(typeof(bool));

            var literal1 = new LiteralNode()
            {
                ExpressionReturnType = literalType,
                Value = value1,
            };

            var literal2 = new LiteralNode()
            {
                ExpressionReturnType = literalType,
                Value = value2,
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "Is {0} is greater than or equal to {1}? {2}"
                            },
                            literal1,
                            literal2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.GreaterEqualThan,
                                LeftOperand = literal1,
                                RightOperand = literal2
                            }
                        }                        
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "Is {0} is less than or equal to {1}? {2}"
                            },
                            literal1,
                            literal2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.LessEqualThan,
                                LeftOperand = literal1,
                                RightOperand = literal2
                            }
                        }                        
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "Is {0} is not equal to {1}? {2}"
                            },
                            literal1,
                            literal2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.NotEquals,
                                LeftOperand = literal1,
                                RightOperand = literal2
                            }
                        }                        
                    }
                }
            };
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Numerals()
        {
            TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(assemblyEmitter.TypeToTypeReference(typeof(float)), 3.5, 2.1);

            ExpectedILFilePath = "TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Numerals.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Strings()
        {
            TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Base(assemblyEmitter.TypeToTypeReference(typeof(string)), "hi", "bye");

            ExpectedILFilePath = "TestCanEmit_GreaterEqualThan_LessEqualThan_NotEquals_Strings.il";
            ExecuteAndAssertSuccess();
        }

        #endregion

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_LogicalAnd_LogicalOr()
        {
            var outputMethod = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<string>()
            {
                "System.String",
                "System.Boolean",                
                "System.Boolean",
                "System.Boolean"
            });

            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var booleanType = assemblyEmitter.TypeToTypeReference(typeof(bool));

            var variable1 = new LocalVariableNode()
            {
                LocalVariable = new VariableDefinition("a", booleanType)
            };

            var variable2 = new LocalVariableNode()
            {
                LocalVariable = new VariableDefinition("b", booleanType)
            };

            var literal1 = new LiteralNode()
            {
                ExpressionReturnType = booleanType,
                Value = true
            };

            var literal2 = new LiteralNode()
            {
                ExpressionReturnType = booleanType,
                Value = false
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = variable1,
                        Initializer = literal1
                    },
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = variable2,
                        Initializer = literal2
                    },

                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0} && {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.LogicalAnd,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0} || {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = booleanType,
                                BinaryOperatorType = BinaryOperatorNodeType.LogicalOr,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_LogicalAnd_LogicalOr.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_BinaryAnd_BinaryOr_BinaryXor()
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var uintType = assemblyEmitter.TypeToTypeReference(typeof(uint));

            var outputMethod = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType,
                uintType,
                uintType,
                uintType
            });


            var variable1 = new LocalVariableNode()
            {
                LocalVariable = new VariableDefinition("a", uintType)
            };

            var variable2 = new LocalVariableNode()
            {
                LocalVariable = new VariableDefinition("b", uintType)
            };

            var literal1 = new LiteralNode()
            {
                ExpressionReturnType = uintType,
                Value = 0x156
            };

            var literal2 = new LiteralNode()
            {
                ExpressionReturnType = uintType,
                Value = 0x841
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = variable1,
                        Initializer = literal1
                    },
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = variable2,
                        Initializer = literal2
                    },

                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0} & {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.BinaryAnd,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0} | {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.BinaryOr,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = outputMethod
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0} ^ {1} == {2}."
                            },
                            variable1,
                            variable2,
                            new BinaryOperatorNode()
                            {
                                ExpressionReturnType = uintType,
                                BinaryOperatorType = BinaryOperatorNodeType.BinaryXor,
                                LeftOperand = variable1,
                                RightOperand = variable2
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_BinaryAnd_BinaryOr_BinaryXor.il";
            ExecuteAndAssertSuccess();
        }
        
        #endregion

        #region Unary operators

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_Negation_BinaryNot_Increment_Decrement()
        {
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType,
                intType,
                intType,
                intType,
                intType,
                intType,
                intType,
                intType
            });

            var variables = new List<LocalVariableNode>();

            for (int i = 0; i < 7; i++)
            {
                variables.Add(new LocalVariableNode()
                {
                    LocalVariable = new VariableDefinition(((char)i + 'a').ToString(), intType)
                });
            }

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = variables[0],
                        Initializer = new LiteralNode()
                        {
                            ExpressionReturnType = intType,
                            Value = 5
                        }
                    }
                }
            };

            var nodes = (List<IParserNode>)BodyCodeBlock.Nodes;
            var operators = new UnaryOperatorNodeType[]
            {
                UnaryOperatorNodeType.BinaryNot,
                UnaryOperatorNodeType.Negation,
                UnaryOperatorNodeType.PostDecrement,
                UnaryOperatorNodeType.PostIncrement,
                UnaryOperatorNodeType.PreDecrement,
                UnaryOperatorNodeType.PreIncrement
            };

            for (int i = 1; i < 7; i++)
            {
                nodes.Add(new SymbolDeclarationNode()
                {
                    DeclaredSymbol = variables[i],
                    Initializer = new UnaryOperatorNode()
                    {
                        UnaryOperatorType = operators[i - 1],
                        ExpressionReturnType = intType,
                        Operand = variables[0]
                    }
                });
            }

            nodes.Add(new MethodCallNode()
            {
                ExpressionReturnType = voidType,
                Function = new FunctionNode()
                {
                    Method = consoleWriteLine
                },
                Args = new List<IExpressionNode>()
                {
                    new LiteralNode()
                    {
                        ExpressionReturnType = stringType,
                        Value = "Results: \r\n{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n{5}\r\n{6}\r\n"
                    },
                    variables[0],
                    variables[1],
                    variables[2],
                    variables[3],
                    variables[4],
                    variables[5],
                    variables[6]
                }
            });

            ExpectedILFilePath = "TestCanEmit_Negation_BinaryNot_Increment_Decrement.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_LogicalNot()
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));

            var field = new FieldDefinition("myField", FieldAttributes.Private | FieldAttributes.Static, boolType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>() 
                {
                    new UnaryOperatorNode()
                    {
                        ExpressionReturnType = voidType,
                        UnaryOperatorType = UnaryOperatorNodeType.VoidOperator,
                        Operand = new AssignmentOperatorNode()
                        {
                            LeftOperand = new FieldNode()
                            {
                                Field = field
                            },
                            RightOperand = new UnaryOperatorNode()
                            {
                                ExpressionReturnType = boolType,
                                UnaryOperatorType = UnaryOperatorNodeType.LogicalNot,
                                Operand = new FieldNode()
                                {
                                    Field = field
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_LogicalNot.il";
            ExecuteAndAssertSuccess();
        }

        #endregion

        #endregion

        #region Control Flow Tests

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_WhileLoop()
        {
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));

            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType,
                intType
            });

            var localVariable = new LocalVariableNode()
            {
                LocalVariable = new VariableDefinition("counter", intType)
            };

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = localVariable,
                        Initializer = new LiteralNode()
                        {
                            ExpressionReturnType = intType,
                            Value = 0
                        }
                    },
                    new WhileBlockNode()
                    {
                        Condition = new BinaryOperatorNode()
                        {
                            ExpressionReturnType = boolType,
                            BinaryOperatorType = BinaryOperatorNodeType.LessThan,
                            LeftOperand = localVariable,
                            RightOperand = new LiteralNode()
                            {
                                ExpressionReturnType = intType,
                                Value = 10
                            }
                        },
                        ExecutedBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                new MethodCallNode()
                                {
                                    ExpressionReturnType = voidType,
                                    Function = new FunctionNode()
                                    {
                                        Method = consoleWriteLine
                                    },
                                    Args = new List<IExpressionNode>()
                                    {
                                        new LiteralNode()
                                        {
                                            ExpressionReturnType = stringType,
                                            Value = "The loop has looped {0} time(s)."
                                        },
                                        localVariable
                                    }
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_WhileLoop.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_IfBlockWithoutElse()
        {
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var intType = assemblyEmitter.TypeToTypeReference(typeof(int));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var boolType = assemblyEmitter.TypeToTypeReference(typeof(bool));

            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType
            });

            var field = new FieldDefinition("myField", FieldAttributes.Static | FieldAttributes.Private, boolType);
            typeEmitter.AddField(field);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new ConditionBlockNode()
                    {
                        Condition = new FieldNode()
                        {
                            Field = field,
                        },
                        TrueBlock = new CodeBlockNode()
                        {
                            Nodes = new List<IParserNode>()
                            {
                                new MethodCallNode()
                                {
                                    ExpressionReturnType = voidType,
                                    Function = new FunctionNode()
                                    {
                                        Method = consoleWriteLine
                                    },
                                    Args = new List<IExpressionNode>()
                                    {
                                        new LiteralNode()
                                        {
                                            ExpressionReturnType = stringType,
                                            Value = "myField is true."
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_IfBlockWithoutElse.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_Return()
        {
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));

            var consoleReadLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "ReadLine", new List<TypeReference>());
            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType,
                stringType
            });

            var readInputMethod = new MethodEmitter(typeEmitter, "ReadInput", stringType, MethodAttributes.Static | MethodAttributes.Private);

            readInputMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new ReturnNode()
                    {
                        Expression = new MethodCallNode()
                        {
                            ExpressionReturnType = stringType,
                            Function = new FunctionNode()
                            {
                                Method = consoleReadLine
                            },
                            Args = new List<IExpressionNode>()
                        },
                    }
                }
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = consoleWriteLine
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "Your input is: \"{0}\"."
                            },
                            new MethodCallNode()
                            {
                                ExpressionReturnType = stringType,
                                Function = new FunctionNode()
                                {
                                    Method = readInputMethod.Get()
                                },
                                Args = new List<IExpressionNode>()
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_Return.il";
            ExecuteAndAssertSuccess();
        }

        #endregion

        /* Missing tests:
         * Functors with optional parameters and params parameters
         */

        #region Method Call tests

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_CreateObject()
        {
            typeEmitter.AddDefaultConstructor();
            var myType = typeEmitter.Get(assemblyEmitter);

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new SymbolDeclarationNode()
                    {
                        DeclaredSymbol = new LocalVariableNode()
                        {
                            LocalVariable = new VariableDefinition("myInstance", myType)
                        },
                        Initializer = new ObjectCreationNode()
                        {
                            ExpressionReturnType = myType,
                            Args = new List<IExpressionNode>()
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_CreateObject.il";
            ExecuteAndAssertSuccess();
        }

        [TestMethod, TestCategory("IL Tests")]
        public void TestCanEmit_CallFunctionWithOptionalParameter()
        {
            var voidType = assemblyEmitter.TypeToTypeReference(typeof(void));
            var stringType = assemblyEmitter.TypeToTypeReference(typeof(string));

            var consoleWriteLine = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, "System.Console", "WriteLine", new List<TypeReference>()
            {
                stringType,
                stringType,
                stringType
            });

            var testMethod = new MethodEmitter(typeEmitter, "MethodWithDefaultParameter", voidType, MethodAttributes.Private | MethodAttributes.Static);
            
            var neededParameter = new ParameterDefinition("neededParameter", ParameterAttributes.None, stringType);
            testMethod.AddArgument(neededParameter);

            var optionalParameter = new ParameterDefinition("optionalParameter", ParameterAttributes.Optional | ParameterAttributes.HasDefault, stringType);
            optionalParameter.Constant = "Default value";
            testMethod.AddArgument(optionalParameter);

            testMethod.ParseTree(new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = consoleWriteLine
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "{0}: {1}"
                            },
                            new FunctionArgumentNode()
                            {
                                IsMethodStatic = true,
                                Param = neededParameter,
                            },
                            new FunctionArgumentNode()
                            {
                                IsMethodStatic = true,
                                Param = optionalParameter,
                            }
                        }
                    }
                }
            });

            var callableTestMethod1 = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, typeEmitter.Get(assemblyEmitter),
                "MethodWithDefaultParameter", new List<TypeReference>()
            {
                stringType
            });

            var callableTestMethod2 = AssemblyRegistry.GetCompatibleMethod(assemblyEmitter, typeEmitter.Get(assemblyEmitter),
                "MethodWithDefaultParameter", new List<TypeReference>()
            {
                stringType,
                stringType
            });

            BodyCodeBlock = new CodeBlockNode()
            {
                Nodes = new List<IParserNode>()
                {
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = callableTestMethod1
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "Hi"
                            }
                        }
                    },
                    new MethodCallNode()
                    {
                        ExpressionReturnType = voidType,
                        Function = new FunctionNode()
                        {
                            Method = callableTestMethod2
                        },
                        Args = new List<IExpressionNode>()
                        {
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "Hi"
                            },
                            new LiteralNode()
                            {
                                ExpressionReturnType = stringType,
                                Value = "NonOptional"
                            }
                        }
                    }
                }
            };

            ExpectedILFilePath = "TestCanEmit_CallFunctionWithOptionalParameter.il";
            ExecuteAndAssertSuccess();
        }

        #endregion
    }
}
