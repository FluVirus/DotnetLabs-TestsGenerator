using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using University.DotnetLabs.Lab3.AssemblyBrowserLibrary.Structures;
using University.DotnetLabs.Lab3.AssemblyBrowserLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace University.DotnetLabs.Lab3.AssemblyBrowserLibrary.Tests;
[TestClass]
public class University_DotnetLabs_Lab3_AssemblyBrowserLibrary_Tests
{
    private AssemblyBrowser _assemblyBrowser;
    [TestInitialize]
    public void TestInitialize()
    {
        _assemblyBrowser = new AssemblyBrowser();
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }
}