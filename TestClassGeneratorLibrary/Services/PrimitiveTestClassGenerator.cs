using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Interfaces;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Records;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.SyntaxWalkers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Services;

public class PrimitiveTestClassGenerator : ITestClassGenerator
{
    public string GenerateTestClassCode(string source)
    {
        CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();
        ClassSelector classSelector = new ClassSelector();
        classSelector.Visit(root);
        ClassDeclarationInfo[] selectedClasses = classSelector.Classes;

        //Clear TestInitialize
        MethodDeclarationSyntax testInitialize = 
        MethodDeclaration(
            PredefinedType(
                Token(SyntaxKind.VoidKeyword)),
                Identifier("TestInitialize"))                                            
        .WithAttributeLists(
            SingletonList(
                AttributeList(
                    SingletonSeparatedList(
                        Attribute(                                            
                            IdentifierName("TestInitialize"))))))
        .WithModifiers(
            TokenList(
                Token(SyntaxKind.PublicKeyword)))
        .WithBody(
            Block());

        //Clear TestCleanup
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

        MethodDeclarationSyntax[] testMethods = GenerateTestMethods(selectedClasses);

        UsingDirectiveSyntax[] additionalUnsings = SelectAdditionalUsings(selectedClasses);

        ISet<UsingDirectiveSyntax> allUsings = classSelector.Usings.Union(additionalUnsings).ToHashSet();
        allUsings.Add(UsingDirective(ParseName("Microsoft.VisualStudio.TestTools.UnitTesting")));

        FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclaration;
        string testClassName;
        if (classSelector.FileScopesNamespaceDeclaration is null)
        {
            fileScopedNamespaceDeclaration = FileScopedNamespaceDeclaration(ParseName("Tests"));
            testClassName = "Tests";
        }
        else   
        {
            fileScopedNamespaceDeclaration = classSelector.FileScopesNamespaceDeclaration;
            testClassName = fileScopedNamespaceDeclaration.Name.ToString().Replace('.', '_') + "_Tests";
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
                .WithMembers(
                    new SyntaxList<MemberDeclarationSyntax>(testMethods.Prepend(testCleanup).Prepend(testInitialize)))))
       .NormalizeWhitespace()
       .ToFullString();
    }

    //------------------------------------------------------------------------
    private MethodDeclarationSyntax[] GenerateTestMethods(ClassDeclarationInfo[] classes)
    { 
        ICollection<MethodDeclarationSyntax> testMethods = new List<MethodDeclarationSyntax>();
        foreach (ClassDeclarationInfo @class in classes)
        {
            MethodDeclarationSyntax[] classPublicMethods =  @class.ClassDeclarationSyntax.ChildNodes().
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
