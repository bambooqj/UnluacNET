using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnluacNET;

namespace Unluac
{
    class Program
    {
        static string version = "1.2.2";

        static void PrintError(string message, int exitCode)
        {
            Console.Error.WriteLine("  error: {0}", message);
            if (exitCode > 0)
            {
                Console.ReadLine();
                Environment.Exit(exitCode);
            }
        }
        static void PrintError(string message)
        {
            Console.Error.WriteLine("  error: {0}", message);

        }
        static void PrintError(string message, params object[] args)
        {
            PrintError(String.Format(message, args));
        }

        static void PrintUsage(bool error)
        {
            var output = (error) ? Console.Error : Console.Out;
            output.WriteLine("  usage: unluac <inFile> <:outFile>");
            output.WriteLine("  usage: unluac <inFolder> <:outFolder>");
        }

        static void PrintVersion(bool error)
        {
            var output = (error) ? Console.Error : Console.Out;
            output.WriteLine("unluac v{0}", version);
        }

        private static LFunction FileToFunction(string fn)
        {
            using (var fs = File.Open(fn, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var header = new BHeader(fs);

                return header.Function.Parse(fs, header);
            }
        }
        /// <summary>
        /// Decompiles a singe .luac file and creates output to give output param
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        static void SaveSingleLua(string input, string output)
        {
            LFunction lMain = null;

            try
            {
                lMain = FileToFunction(input);
            }
            catch (Exception e)
            {
                PrintVersion(true);
                PrintError(e.Message, 1);
            }

            var d = new Decompiler(lMain);
            d.Decompile();
            var writeLog = true;
            try
            {
                using (var writer = new StreamWriter(output))
                {
                    d.Print(new Output(writer));
                    writer.Flush();
                    if (writeLog)
                        Console.WriteLine("successfully decompiled to '{0}'", output);
                    return;
                }
            }
            catch (Exception e)
            {
                PrintVersion(true);
                PrintError(e.Message, 2);
            }
            d.Print();
            if (System.Diagnostics.Debugger.IsAttached)
                Console.ReadKey();
        }
        /// <summary>
        /// Decompiles a directory of .luac files and creates output to give output param, it will also work with a single file
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        static void SaveLuaCommon(string input, string output)
        {
            if (Directory.Exists(input))
            {
                //Folder and all LUAC inside
                var files = Directory.GetFiles(input, "*.luac", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    PrintError("No .luac files found in " + input, 3);
                    return;
                }
                if (!Directory.Exists(output)) Directory.CreateDirectory(output);
                foreach (var file in files)
                {
                    try
                    {
                        var parsed = file.Replace(input, "");
                        var filename = Path.GetFileNameWithoutExtension(file);
                        var parentDir = parsed.Replace(filename + ".luac", "");
                        var dirToCreate = output + parentDir;
                        if (!Directory.Exists(dirToCreate)) Directory.CreateDirectory(dirToCreate);
                        var outputPath = dirToCreate + filename + ".lua";
                        SaveSingleLua(file, outputPath);
                    }
                    catch (Exception ex)
                    {
                        PrintError(ex.Message + "\tFile: " + file);
                    }
                }
                Console.WriteLine("Done!");
                Console.ReadLine();
                Environment.Exit(0);
            }
            else if (!File.Exists(input))
            {
                PrintVersion(true);
                PrintUsage(true);
                PrintError("input file does not exist", 1); //kills the app
            }
            SaveSingleLua(input, output); //File to File
            Console.ReadLine();
            Environment.Exit(0);
        }
        static void Main(string[] args)
        {

            if (args.Length != 2)
            {
                PrintVersion(true);
                PrintUsage(true);
                PrintError("missing args", 1);
            }
            else
            {
                //Console.BufferHeight = 2500;
                var input = args[0];
                var output = args[1];
                SaveLuaCommon(input, output);
            }
        }
    }
}

