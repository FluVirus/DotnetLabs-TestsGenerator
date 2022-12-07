using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Interfaces;
using University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Services;

namespace University.DotnetLabs.Lab4.TestClassGeneratorLibrary.Tests;


[TestClass]
public class TestClassGeneratorTests
{
    ITestClassGenerator _generator = new SophisticatedTestClassGenerator();
    string testCase1InputDirectory = "C:\\Users\\User\\source\\repos\\University\\DotnetLabs\\Lab4\\TestClassGeneratorLibrary.Tests\\TestCase1\\Input";
    string testCase1OutputDirectory = "C:\\Users\\User\\source\\repos\\University\\DotnetLabs\\Lab4\\TestClassGeneratorLibrary.Tests\\TestCase1\\Output";
    string testCase2InputDirectory = "C:\\Users\\User\\source\\repos\\University\\DotnetLabs\\Lab4\\TestClassGeneratorLibrary.Tests\\TestCase2\\Input";
    string testCase2OutputDirectory = "C:\\Users\\User\\source\\repos\\University\\DotnetLabs\\Lab4\\TestClassGeneratorLibrary.Tests\\TestCase2\\Output";
    string testCase3InputDirectory = "C:\\Users\\User\\source\\repos\\University\\DotnetLabs\\Lab4\\TestClassGeneratorLibrary.Tests\\TestCase3\\Input";
    string testCase3OutputDirectory = "C:\\Users\\User\\source\\repos\\University\\DotnetLabs\\Lab4\\TestClassGeneratorLibrary.Tests\\TestCase3\\Output";

    private string _emptyClass = @"using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Tests;
[TestClass]
public class Tests
{
    [TestInitialize]
    public void TestInitialize()
    {
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }
}";

    [TestMethod]
    public void Test_Correct()
    {
        string file = Directory.GetFiles(testCase1InputDirectory)[0];
        string code = _generator.GenerateTestClassCode(File.ReadAllText(file));
        File.WriteAllText(Path.Combine(testCase1OutputDirectory, "Output.txt"), code);
        Assert.AreNotEqual(code, _emptyClass);
    }

    [TestMethod]
    public void Test_Random()
    {
        string file = Directory.GetFiles(testCase2InputDirectory)[0];
        string code = _generator.GenerateTestClassCode(File.ReadAllText(file));
        File.WriteAllText(Path.Combine(testCase2OutputDirectory, "Output.txt"), code);
        Assert.AreEqual(code, _emptyClass);
    }

    [TestMethod]
    public void Test_Incorrect()
    {
        string file = Directory.GetFiles(testCase3InputDirectory)[0];
        string code = _generator.GenerateTestClassCode(File.ReadAllText(file));
        File.WriteAllText(Path.Combine(testCase3OutputDirectory, "Output.txt"), code);
        Assert.AreNotEqual(code, _emptyClass);
    }
}