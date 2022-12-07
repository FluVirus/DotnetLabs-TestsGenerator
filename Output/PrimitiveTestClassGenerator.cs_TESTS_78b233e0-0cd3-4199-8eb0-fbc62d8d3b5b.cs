using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Interfaces;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Records;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.SyntaxWalkers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Services.Tests;
[TestClass]
public class University_DotnetLabs_Lab4_TestClassGeneratorLibrary_Services_Tests
{
    private PrimitiveTestClassGenerator _primitiveTestClassGenerator;
    [TestInitialize]
    public void TestInitialize()
    {
        _primitiveTestClassGenerator = new PrimitiveTestClassGenerator();
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    [TestMethod]
    public void PrimitiveTestClassGenerator_GenerateTestClassCode_Test()
    {
        string source = default;
        var actual = PrimitiveTestClassGenerator.GenerateTestClassCode(source);
        string expected = default;
        Assert.AreEqual(actual, expected);
        Assert.Fail("autogenerated");
    }
}