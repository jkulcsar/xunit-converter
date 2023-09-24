using System.Text.RegularExpressions;
using System;
using System.IO;

string path = string.Empty;

if (Args.Count() == 1)
{
	path = Args[0];
}
else
{
	Console.WriteLine("File name argument not found");
	//return;
}

var source = ReadMsTestFile(path);
var result = Transform(source);
WriteFile(path,result);

Console.WriteLine("Conversion completed");

private static string ReadMsTestFile(string file)
{
	using(var reader = new StreamReader(file))
	{
	  Console.WriteLine("Reading file: " + file);
		return reader.ReadToEnd();
	}
}

private static void WriteFile(string pathToFile, string content)
{
    using(var writer = new StreamWriter(pathToFile))
    {
        Console.WriteLine("Writing file: " + pathToFile);
        writer.Write(content);
    }
}
private static string Transform(string source)
{
    source = source.Replace("[TestMethod]", "[Fact]")
    .Replace("[TestClass]", "")
    .Replace("Assert.AreEqual", "Assert.Equal")
    .Replace("Assert.AreNotEqual", "Assert.NotEqual")
    .Replace("Assert.IsTrue", "Assert.True")
    .Replace("Assert.IsFalse", "Assert.False")
    .Replace("Assert.IsNotNull", "Assert.NotNull")
    .Replace("Assert.IsNull", "Assert.Null")
    .Replace("Assert.AreNotSame", "Assert.NotSame")
    .Replace("Assert.AreSame", "Assert.Same")
    .Replace("using Microsoft.VisualStudio.TestTools.UnitTesting", "using Xunit");
    source = HandleIsInstanceOfType(source);
    source = HandleIsNotInstanceOfType(source);
//    source = HandleStringAssertContains(source);
    return source;
}

private static string HandleIsInstanceOfType(string source)
{
    Regex regex = new Regex(@"(Assert.IsInstanceOfType)\((.*),\s(typeof.*)\)");
    return regex.Replace(source, "Assert.IsType($3, $2)");
}

private static string HandleIsNotInstanceOfType(string source)
{
    Regex regex = new Regex(@"(Assert.IsNotInstanceOfType)\((.*),\s(typeof.*)\)");
    return regex.Replace(source, "Assert.IsNotType($3, $2)");
}

private static string HandleStringAssertContains(string source)
{
    Regex regex = new Regex(@"(StringAssert.Contains)((.?), (.))");
    return regex.Replace(source, "Assert.True($3.Contains($2))");
}
