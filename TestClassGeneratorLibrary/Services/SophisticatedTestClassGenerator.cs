using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Interfaces;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Records;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.SyntaxWalkers;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Services;

public class SophisticatedTestClassGenerator : ITestClassGenerator
{
    public string GenerateTestClassCode(string source)
    {
        CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();
        ClassSelector classSelector = new ClassSelector();
        classSelector.Visit(root);
        ClassDeclarationInfo[] selectedClasses = classSelector.Classes;

        //TestCleanup
        MethodDeclarationSyntax testCleanup =
        MethodDeclaration(
            PredefinedType(
                Token(SyntaxKind.VoidKeyword)),
                Identifier("TestCleanup"))
        .WithAttributeLists(
            SingletonList(
                AttributeList(
                    SingletonSeparatedList(
                        Attribute(
                            IdentifierName("TestCleanup"))))))
        .WithModifiers(
            TokenList(
                Token(SyntaxKind.PublicKeyword)))
        .WithBody(
            Block());

        MemberDeclarationSyntax[] testInitialize = GenerateTestInitialize(selectedClasses);

        MethodDeclarationSyntax[] testMethods = GenerateTestMethods(selectedClasses);

        UsingDirectiveSyntax[] additionalUnsings = SelectAdditionalUsings(selectedClasses);

        ISet<UsingDirectiveSyntax> allUsings = classSelector.Usings.Union(additionalUnsings).ToHashSet();
        allUsings.Add(UsingDirective(ParseName("Microsoft.VisualStudio.TestTools.UnitTesting")));
        allUsings.Add(UsingDirective(ParseName("Moq")));

        FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclaration;
        string testClassName;
        if (classSelector.FileScopesNamespaceDeclaration is null)
        {
            fileScopedNamespaceDeclaration = FileScopedNamespaceDeclaration(ParseName("Tests"));
            testClassName = "Tests";
        }
        else
        {
            fileScopedNamespaceDeclaration = FileScopedNamespaceDeclaration(ParseName(classSelector.FileScopesNamespaceDeclaration.Name.ToString()+ ".Tests"));  
            testClassName = fileScopedNamespaceDeclaration.Name.ToString().Replace('.', '_');
        }

        return CompilationUnit()
       .WithUsings(
            new SyntaxList<UsingDirectiveSyntax>(allUsings))
       .WithMembers(
            new SyntaxList<MemberDeclarationSyntax>()
            .Add(
                fileScopedNamespaceDeclaration)
            .Add(
                ClassDeclaration(testClassName)
                .WithAttributeLists(
                    SingletonList(
                        AttributeList(
                            SingletonSeparatedList(
                                Attribute(
                                    IdentifierName("TestClass"))))))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithMembers(new SyntaxList<MemberDeclarationSyntax>(testInitialize.Append(testCleanup).Concat(testMethods)))))
       .NormalizeWhitespace()
       .ToFullString();
    }

    //------------------------------------------------------------------------
    MemberDeclarationSyntax[] GenerateTestInitialize(ClassDeclarationInfo[] classes)
    {
        
        List<StatementSyntax> body = new();
        List<MemberDeclarationSyntax> fields = new();

        foreach (ClassDeclarationInfo @class in classes)
        {
            if (@class.ClassDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                continue;
            }

            List<ConstructorDeclarationSyntax> constructors = @class.ClassDeclarationSyntax.ChildNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .OrderBy(ctor => ctor.ParameterList.Parameters.Count)
                .ToList();

            var classIdentifier = $"_{@class.ClassDeclarationSyntax.Identifier.Text.ToCamelCase()}";

            fields.Add(FieldDeclaration(
                                VariableDeclaration(
                                    IdentifierName(@class.FullName))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                            Identifier(classIdentifier)))))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PrivateKeyword))));
            List<SyntaxNodeOrToken> constructorArgs = new();
            
            if(constructors.Count == 0)
            {
                body.Add(ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName($"_{@class.ClassDeclarationSyntax.Identifier.Text.ToCamelCase()}"),
                                            ObjectCreationExpression(
                                                IdentifierName(@class.FullName))
                                            .WithArgumentList(
                                                ArgumentList()))));
            }
            else
            {
                foreach (var parameter in constructors[0].ParameterList.Parameters)
                {
                    var typeName = parameter.Type!.ToString();

                    if (typeName.StartsWith("I"))
                    {
                        var fieldName = $"_{parameter.Identifier.Text.ToCamelCase()}";

                        constructorArgs.Add(Argument(IdentifierName($"{fieldName}.Object")));

                        fields.Add(FieldDeclaration(
                                        VariableDeclaration(
                                            GenericName(
                                                Identifier("Mock"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SingletonSeparatedList<TypeSyntax>(
                                                        IdentifierName(typeName)))))
                                         .WithVariables(
                                            SingletonSeparatedList(
                                                VariableDeclarator(
                                                    Identifier(fieldName)))))
                                    .WithModifiers(
                                        TokenList(
                                            Token(SyntaxKind.PrivateKeyword))));

                        body.Add(ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName(fieldName),
                                            ObjectCreationExpression(
                                                GenericName(
                                                    Identifier("Mock"))
                                                .WithTypeArgumentList(
                                                    TypeArgumentList(
                                                        SingletonSeparatedList<TypeSyntax>(
                                                            IdentifierName(typeName)))))
                                            .WithArgumentList(
                                                ArgumentList()))));
                    }
                    else
                    {
                        constructorArgs.Add(Argument(IdentifierName(parameter.Identifier.Text)));

                        body.Add(LocalDeclarationStatement(
                                        VariableDeclaration(parameter.Type!)
                                        .WithVariables(
                                            SingletonSeparatedList(
                                                VariableDeclarator(
                                                    Identifier(parameter.Identifier.Text))
                                                .WithInitializer(
                                                    EqualsValueClause(
                                                        LiteralExpression(
                                                            SyntaxKind.DefaultLiteralExpression,
                                                            Token(SyntaxKind.DefaultKeyword))))))));
                    }
                    constructorArgs.Add(Token(SyntaxKind.CommaToken));
                }

                if (constructorArgs.Count != 0)
                {
                    constructorArgs.RemoveAt(constructorArgs.Count - 1);
                }

                body.Add(ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName(classIdentifier),
                                            ObjectCreationExpression(
                                                IdentifierName(@class.FullName))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SeparatedList<ArgumentSyntax>(constructorArgs))))));
            }
        }

        fields.Add(MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier("TestInitialize"))
                .WithAttributeLists(
                    SingletonList<AttributeListSyntax>(
                        AttributeList(
                            SingletonSeparatedList<AttributeSyntax>(
                                Attribute(
                                    IdentifierName("TestInitialize"))))))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBody(
                    Block(body)));

        return fields.ToArray();
    }

    private MethodDeclarationSyntax[] GenerateTestMethods(ClassDeclarationInfo[] classes)
    {
        ICollection<MethodDeclarationSyntax> testMethods = new List<MethodDeclarationSyntax>();
        foreach (ClassDeclarationInfo @class in classes)
        {
            MethodDeclarationSyntax[] classPublicMethods = @class.ClassDeclarationSyntax.ChildNodes().
                OfType<MethodDeclarationSyntax>().Where(node => node.Modifiers.Any(SyntaxKind.PublicKeyword)).ToArray();

            //generation of TestMethods
            int methodIndex = 1;
            string previousMethodName = string.Empty;
            foreach (MethodDeclarationSyntax publicMethod in classPublicMethods)
            {
                string currentMethodName = publicMethod.Identifier.Text;
                string currentTestMethodName =
                    $"{@class.ClassDeclarationSyntax.Identifier.Text}_{publicMethod.Identifier.Text}_Test";
                if (currentMethodName == previousMethodName)
                {
                    currentTestMethodName = $"{currentTestMethodName}_{++methodIndex}";
                }
                else
                {
                    methodIndex = 1;
                }

                List<StatementSyntax> body = new();


                //----
                var args = new List<SyntaxNodeOrToken>();
                foreach (var parameter in publicMethod.ParameterList.Parameters)
                {
                    args.Add(Argument(IdentifierName(parameter.Identifier.Text)));
                    args.Add(Token(SyntaxKind.CommaToken));

                    body.Add(LocalDeclarationStatement(
                                        VariableDeclaration(parameter.Type!)
                                        .WithVariables(
                                            SingletonSeparatedList(
                                                VariableDeclarator(
                                                    Identifier(parameter.Identifier.Text))
                                                .WithInitializer(
                                                    EqualsValueClause(
                                                        LiteralExpression(
                                                            SyntaxKind.DefaultLiteralExpression,
                                                            Token(SyntaxKind.DefaultKeyword))))))));
                }

                if (args.Count != 0)
                {
                    args.RemoveAt(args.Count - 1);
                }

                if (publicMethod.ReturnType is PredefinedTypeSyntax typeSyntax
                    && typeSyntax.Keyword.ValueText == Token(SyntaxKind.VoidKeyword).ValueText)
                {
                    body.Add(ExpressionStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(@class.ClassDeclarationSyntax.Identifier.Text),
                                        IdentifierName(publicMethod.Identifier.Text)))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(args)))));
                }
                else
                {
                    body.Add(LocalDeclarationStatement(
                                VariableDeclaration(
                                    IdentifierName(
                                        Identifier(
                                            TriviaList(),
                                            SyntaxKind.VarKeyword,
                                            "var",
                                            "var",
                                            TriviaList())))
                             .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                        Identifier("actual"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            InvocationExpression(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName(@class.ClassDeclarationSyntax.Identifier.Text),
                                                    IdentifierName(publicMethod.Identifier.Text)))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SeparatedList<ArgumentSyntax>(args)))))))));

                    body.Add(LocalDeclarationStatement(
                                        VariableDeclaration(publicMethod.ReturnType)
                                        .WithVariables(
                                            SingletonSeparatedList(
                                                VariableDeclarator(
                                                    Identifier("expected"))
                                                .WithInitializer(
                                                    EqualsValueClause(
                                                        LiteralExpression(
                                                            SyntaxKind.DefaultLiteralExpression,
                                                            Token(SyntaxKind.DefaultKeyword))))))));

                    body.Add(ExpressionStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(
                                            "Assert"),
                                        IdentifierName("AreEqual")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                Argument(
                                                    IdentifierName("actual")),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    IdentifierName("expected")) })))));

                }

                ExpressionStatementSyntax assertFailAutogenerated =
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("Assert"),
                            IdentifierName("Fail")))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal("autogenerated")))))));
                body.Add(assertFailAutogenerated);

                MethodDeclarationSyntax currentTestMethod =
                MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                        Identifier(currentTestMethodName))
                .WithAttributeLists(
                     SingletonList(
                        AttributeList(
                            SingletonSeparatedList(
                                Attribute(
                                    IdentifierName("TestMethod"))))))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBody(
                    Block(body));

                testMethods.Add(currentTestMethod);
                previousMethodName = currentMethodName;
            }
        }
        return testMethods.ToArray();
    }
    private UsingDirectiveSyntax[] SelectAdditionalUsings(ClassDeclarationInfo[] classes)
    {
        HashSet<UsingDirectiveSyntax> usings = new();
        foreach (ClassDeclarationInfo classDeclaration in classes)
        {
            UsingDirectiveSyntax @using =
            UsingDirective(
                ParseName(classDeclaration.Namespace));
            usings.Add(@using);
        }
        return usings.ToArray();
    }
}
