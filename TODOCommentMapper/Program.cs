using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using CsvHelper;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TODOCommentMapper
{
	public static class Program
	{
		public static void Main(string[] args) =>
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(Parse);

		private static readonly List<RecordItem> RecordItems = new List<RecordItem>();
		
		private static void Parse(Options options)
		{
			var solutionFilePath = options.SolutionFilePath;

			var todoCommentIdentifier = new ToDoCommentIdentifier();
			foreach (var csharpCompileFile in GetProjectFilesForSolution(new FileInfo(solutionFilePath)).SelectMany(projectFile => GetCSharpCompileItemFilesForProject(projectFile)))
			{
				foreach (var todoComment in todoCommentIdentifier.GetToDoComments(csharpCompileFile.OpenText().ReadToEnd()))
				{
					string methodOrProperty = string.Empty;
					
					Console.WriteLine(todoComment.Content);
					Console.WriteLine();
					if (todoComment.NamespaceIfAny == null)
						Console.WriteLine("Not in any namespace");
					else
					{
						Console.WriteLine("Namespace: " + todoComment.NamespaceIfAny.Name);
						if (todoComment.TypeIfAny != null)
							Console.WriteLine("Type: " + todoComment.TypeIfAny.Identifier);
						if (todoComment.MethodOrPropertyIfAny != null)
						{
							Console.Write("Method/Property: ");
							if (todoComment.MethodOrPropertyIfAny is ConstructorDeclarationSyntax)
								methodOrProperty = ".ctor";
							else if (todoComment.MethodOrPropertyIfAny is MethodDeclarationSyntax)
								methodOrProperty = ((MethodDeclarationSyntax) todoComment.MethodOrPropertyIfAny).Identifier.ToString();
							else if (todoComment.MethodOrPropertyIfAny is IndexerDeclarationSyntax)
								methodOrProperty = "[Indexer]";
							else if (todoComment.MethodOrPropertyIfAny is PropertyDeclarationSyntax)
								methodOrProperty = ((PropertyDeclarationSyntax) todoComment.MethodOrPropertyIfAny)
									.Identifier.ToString();
							else
								methodOrProperty = "?";

							Console.Write(methodOrProperty);
							Console.WriteLine();
						}
					}
					Console.WriteLine(csharpCompileFile.FullName + ":" + todoComment.LineNumber);
					Console.WriteLine();
					
					RecordItems.Add(
						new RecordItem(comment: todoComment.Content,
							ns: todoComment.NamespaceIfAny.Name.ToString(),
							type: todoComment.TypeIfAny.Identifier.ToString(),
							methodOrProperty: methodOrProperty,
							path: csharpCompileFile.FullName + ":" + todoComment.LineNumber));
				}
			}

			WriteCsv(options.ReportFilePath);
			
			Console.WriteLine("Success! Press [Enter] to continue..");
			Console.ReadLine();
		}

		private static IEnumerable<FileInfo> GetProjectFilesForSolution(FileInfo solutionFile)
		{
			if (solutionFile == null)
				throw new ArgumentNullException("solutionFile");

			var projectFileMatcher = new Regex(
				@"Project\(""\{\w{8}-\w{4}-\w{4}-\w{4}-\w{12}\}""\) = ""(.*?)"", ""(?<projectFile>(.*?\.csproj))"", ""\{\w{8}-\w{4}-\w{4}-\w{4}-\w{12}\}"""
			);
			foreach (Match match in projectFileMatcher.Matches(solutionFile.OpenText().ReadToEnd()))
				yield return new FileInfo(Path.Combine(solutionFile.Directory.FullName, match.Groups["projectFile"].Value));
		}

		private static IEnumerable<FileInfo> GetCSharpCompileItemFilesForProject(FileInfo projectFile)
		{
			if (projectFile == null)
				throw new ArgumentNullException("projectFile");

			return (new ProjectCollection()).LoadProject(projectFile.FullName).AllEvaluatedItems
				.Where(item => item.ItemType == "Compile")
				.Select(item => item.EvaluatedInclude)
				.Where(include => include.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
				.Select(include => new FileInfo(Path.Combine(projectFile.Directory.FullName, include)));
		}

		private static void WriteCsv(string reportPath)
		{
			using (var writer = new StreamWriter(reportPath))
			using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
			{
				csv.WriteRecords(RecordItems);
			}
			
			Console.WriteLine($"Written report to {reportPath}");
		}
	}
}
