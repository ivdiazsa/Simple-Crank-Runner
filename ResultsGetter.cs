using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using ResultsDictionary = System.Collections.Generic.Dictionary<string,
    System.Collections.Generic.List<
        System.Collections.Generic.Dictionary<string, string>>>;

using IterationsList = System.Collections.Generic.List<
    System.Collections.Generic.Dictionary<string, string>>;

internal class ResultsGetter
{
    private ResultsDictionary _results;

    public ResultsGetter(string jsonFile)
    {
        string rawJson = File.ReadAllText(jsonFile);
        _results = JsonSerializer.Deserialize<ResultsDictionary>(rawJson)!;
    }

    public void DisplayAverageResults(params string[] fields)
    {
        decimal[,] averageResults = GetAverageValues(fields);
        var outputSb = new StringBuilder();

        int rows = averageResults.GetLength(0);
        int columns = averageResults.GetLength(1);

        int[] cellLengthsPerRow = new int[columns + 1];
        cellLengthsPerRow[0] = "Scenario".Length + 2;

        for (int i = 1; i < cellLengthsPerRow.Length; i++)
        {
            cellLengthsPerRow[i] = fields[i - 1].Length + 2;
        }

        int longestScCellLength = _results.Keys.Max(k => k.Length) + 2;

        if (cellLengthsPerRow[0] < longestScCellLength)
            cellLengthsPerRow[0] = longestScCellLength;

        outputSb.AppendFormat("|{0}|", PadMiddle("Scenario", cellLengthsPerRow[0]));
        for (int j = 0; j < fields.Length; j++)
        {
            outputSb.AppendFormat("{0}|", PadMiddle(fields[j], cellLengthsPerRow[j+1]));
        }
        outputSb.AppendLine();

        for (int row = 0; row < rows; row++)
        {
            outputSb.AppendFormat("|{0}|", PadMiddle(_results.Keys.ElementAt(row),
                                                     cellLengthsPerRow[0]));
            for (int col = 0; col < columns; col++)
            {
                outputSb.AppendFormat("{0}|", PadMiddle(averageResults[row, col].ToString(),
                                                        cellLengthsPerRow[col+1]));
            }
            outputSb.AppendLine();
        }

        System.Console.WriteLine(outputSb.ToString());
    }

    private decimal[,] GetAverageValues(string[] fields)
    {
        int rows = _results.Keys.Count;
        int columns = fields.Length;
        int scenarioNum = 0;

        decimal[,] averagesTable = new decimal[rows, columns];

        foreach (KeyValuePair<string, IterationsList> scenarioRun in _results)
        {
            string scenario = scenarioRun.Key;
            IterationsList iterList = scenarioRun.Value;

            foreach (Dictionary<string, string> iter in iterList)
            for (int fi = 0; fi < fields.Length; fi++)
            {
                string metric = fields[fi];
                decimal metricValue = decimal.Parse(iter[metric].Replace(",", ""));
                averagesTable[scenarioNum, fi] += decimal.Round(metricValue, 2);
            }

            for (int ci = 0; ci < fields.Length; ci++)
            {
                averagesTable[scenarioNum, ci] /= iterList.Count;
            }
            scenarioNum++;
        }
        return averagesTable;
    }

    private string PadMiddle(string str, int chars)
    {
        int blanks = chars - str.Length;
        int left, right;

        left = right = blanks / 2;
        if (blanks % 2 != 0) right += 1;

        return string.Concat(Enumerable.Repeat(" ", left))
            + str
            + string.Concat(Enumerable.Repeat(" ", right));
    }
}
