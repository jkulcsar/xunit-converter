

namespace XUnitConverter.Regex;
using System.Text.RegularExpressions;

internal static class Program
{

    internal static async Task Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("xunitconverter.regex <directory_path> <script_path>");
            return;
        }

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += delegate { cts.Cancel(); };
        await RunAsync(args[0], args[1], cts.Token);
    }

    private static Task RunAsync(string directoryPath, string scriptPath, CancellationToken cancellationToken)
    {
        ProcessFilesRecursively(directoryPath, scriptPath);
        return Task.CompletedTask;
    }

    private static void ProcessFilesRecursively(string directoryPath, string scriptPath)
    {
        // Get all files in the directory and its subdirectories
        var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

        // Call the script on each file
        foreach (var file in files)
        {
            // Call the script using the dotnet CLI
            // var process = new Process();
            // process.StartInfo.FileName = "dotnet";
            // process.StartInfo.Arguments = $"script {scriptPath} {file}";
            // process.Start();
            // process.WaitForExit();

            var source = ReadMsTestFile(file);
            var result = Transform(source);
            WriteFile(file, result);
        }
    }

    private static string ReadMsTestFile(string file)
    {
        using (var reader = new StreamReader(file))
        {
            Console.WriteLine("Reading file: " + file);
            return reader.ReadToEnd();
        }
    }

    private static void WriteFile(string pathToFile, string content)
    {
        using (var writer = new StreamWriter(pathToFile))
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
        source = HandleStringAssertContains(source);
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
}
