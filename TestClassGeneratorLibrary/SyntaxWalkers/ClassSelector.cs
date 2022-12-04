using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Records;

namespace University.DotnetLabs.Lab4.TestClassGeneratorLibrary.SyntaxWalkers;
internal class ClassSelector : CSharpSyntaxWalker
{
    private ICollection<UsingDirectiveSyntax> _usings = new List<UsingDirectiveSyntax>();
    private ICollection<ClassDeclarationInfo> _classes = new HashSet<ClassDeclarationInfo>();
    public FileScopedNamespaceDeclarationSyntax? FileScopesNamespaceDeclaration { get; private set; } = null;
    public ClassDeclarationInfo[] Classes
    {
        get => _classes.ToArray();
    }

    public UsingDirectiveSyntax[] Usings
    {
        get => _usings.ToArray();
    }

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        _usings.Add(node);
        base.VisitUsingDirective(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        string @namespace;
        if (FileScopesNamespaceDeclaration is not null)
        {
            @namespace = FileScopesNamespaceDeclaration.Name.ToString();
        }
        else
        {
            @namespace = GetClassNormalNamespace(node);
        }
        _classes.Add(new ClassDeclarationInfo(node, @namespace, GetFullClassName(node)));
        base.VisitClassDeclaration(node);
    }

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    { 
        FileScopesNamespaceDeclaration = node;
        base.VisitFileScopedNamespaceDeclaration(node);
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    private string GetClassNormalNamespace(ClassDeclarationSyntax node)
    {
        StringBuilder builder = new();
        SyntaxNode current = node;

        while (current.Parent is NamespaceDeclarationSyntax || current.Parent is ClassDeclarationSyntax)
        {
            if (current.Parent is NamespaceDeclarationSyntax ns)
            {
                builder.Insert(0, $"{ns.Name}.");
            }
            current = current.Parent;
        }

        if (builder.Length > 0)
        {
            builder.Remove(builder.Length - 1, 1);
        }

        return builder.ToString();
    }

    private string GetFullClassName(ClassDeclarationSyntax node)
    {
        StringBuilder builder = new();
        SyntaxNode current = node;

        builder.Append(node.Identifier.Text);

        while (current.Parent is ClassDeclarationSyntax parentClassDelcaration)
        {
            builder.Insert(0, $"{parentClassDelcaration.Identifier.Text}.");
            current = current.Parent;
        }

        return builder.ToString();
    }
}
