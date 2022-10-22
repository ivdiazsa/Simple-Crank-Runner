using System;
using System.Collections.Generic;
using System.Diagnostics;

internal class SimpleCrankRunner
{
    private static readonly string s_timestamp = DateTime.Now.ToString("MMdd-HHmm");

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        if (args[0].Equals("results-only"))
        {
            var startFinale = new ResultsGetter(args[1]);
            Console.Write("\n");
            startFinale.DisplayAverageResults("Build Time (ms)", "Start Time (ms)");
            Environment.Exit(0);
        }

        string[] scenarios = args[0].Split(',');
        int iterations = int.Parse(args[1]);
        string pathToBenchmarkerRepo = args[2];

        var logger = new MultiIOLogger($"log-{s_timestamp}.txt");
        var outputKeep = new List<string>();
        var resultsHandler = new ResultsHandler($"results-{s_timestamp}.json");

        for (int i = 0; i < scenarios.Length; i++)
        {
            string scenario = scenarios[i];
            string crankArgs = $"--config {pathToBenchmarkerRepo}/scenarios/containers.benchmarks.yml"
                             + $" --scenario {scenario}"
                             + " --profile aspnet-citrine-lin";

            resultsHandler.ConfigName = scenario;

            logger.Write($"\nRunning scenario {scenario} ({i+1}/{scenarios.Length})...\n");
            for (int j = 1; j <= iterations; j++)
            {
                outputKeep.Clear();
                logger.Write($"\nIteration {j}/{iterations}...\n");
                logger.Write($"\ncrank {crankArgs}\n\n");

                using (Process p = new Process())
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "crank",
                        Arguments = crankArgs,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        UseShellExecute = false,
                    };

                    p.StartInfo = startInfo;
                    p.Start();

                    while (!p.StandardOutput.EndOfStream)
                    {
                        string line = p.StandardOutput.ReadLine()!;
                        logger.Write($"{line}\n");
                        outputKeep.Add(line);
                    }
                    p.WaitForExit();
                }
                resultsHandler.ParseAndStoreIterationResults(j, outputKeep);
            }
            resultsHandler.StoreConfigRunResults();
        }
        resultsHandler.SerializeToJSON();

        var finale = new ResultsGetter($"results-{s_timestamp}.json");
        Console.Write("\n");
        finale.DisplayAverageResults("Build Time (ms)", "Start Time (ms)");
    }
}
