using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Records;

internal record class ClassDeclarationInfo 
    (ClassDeclarationSyntax ClassDeclarationSyntax, string Namespace, string FullName);
