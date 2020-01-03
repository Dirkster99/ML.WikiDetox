namespace DetoxConverter
{
    using Microsoft.Data.Analysis;
    using System;
    using System.Collections.Generic;

	/// <summary>
	/// Converting Data Files from:
	///   https://meta.wikimedia.org/wiki/Research:Detox/Data_Release
	///   https://figshare.com/articles/Wikipedia_Talk_Labels_Toxicity/4563973
	///
	/// to be useful in wikiDetox project (in below solution).
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			string[] srcColumnNames = new string[] { "rev_id", "comment", "year", "logged_in", "ns", "sample", "split"};
			Type[] srcDataTypes = new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) };

			// read main annotations and store each in a seperate in-memory object
			string srcFileName = @".\Data\toxicity_annotated_comments.tsv";
			var df = DataFrame.LoadCsv(srcFileName, '\t', true, srcColumnNames, srcDataTypes);

			var AllComments = new Dictionary<string, DetoxData>();

			// Iterate over unlabeled file to retain textual comment and rev_id
			for (long irow = 0; irow < df.Rows.Count; irow++)
			{
				DataFrameRow dataRow = df.Rows[irow];

				string rev_id = dataRow[0] as string;
				string text = dataRow[1] as string;

				DetoxData fndItem;
				if (AllComments.TryGetValue(rev_id, out fndItem) == true)
				{
					Console.WriteLine();
					Console.WriteLine("Error Item already exists in colleciont:");
					Console.WriteLine(" rev_id: '{0}', Row Idx: '{1}'", fndItem.RevId, fndItem.RowIdx);
					Console.WriteLine("Comment: '{0}'", fndItem.Comment);
					Console.WriteLine();
					Console.WriteLine(" rev_id: '{0}', Row Idx: '{1}'", rev_id, irow.ToString());
					Console.WriteLine("Comment: '{0}'", text);
				}
				else
					AllComments.Add(rev_id, new DetoxData(rev_id, irow.ToString(), text));
			}

			// Iterate over toxicity data and add scores into in memory data items
			string[] toxColumnNames = new string[] { "rev_id", "worker_id", "toxicity", "toxicity_score" };
			Type[] toxDataTypes = new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) };

			string srcToxicityFileName = @".\Data\toxicity_annotations.tsv";
			df = DataFrame.LoadCsv(srcToxicityFileName, '\t', true, toxColumnNames, toxDataTypes);

			// Label each object with its respective scores
			// Iterate through each data row in annotations and store them in their respective in-memory object
			for (long irow = 0; irow < df.Rows.Count; irow++)
			{
				DataFrameRow dataRow = df.Rows[irow];

				string rev_id = dataRow[0] as string;
				string worker_id = dataRow[1] as string;
				string toxicity = dataRow[2] as string;
				string toxicity_score = dataRow[3] as string;

				DetoxData fndItem;
				if (AllComments.TryGetValue(rev_id, out fndItem) == true)
				{
					fndItem.AddToxicity(float.Parse(toxicity_score), int.Parse(toxicity));
				}
			}

			// Sanity check to see if there are any unlabeled data items
			foreach (var item in AllComments)
			{
				if (item.Value.Toxicity.Count == 0)
				{
					Console.WriteLine("Warning item rev_id: '{0}' does not have a label.", item.Key);
				}
				else
				{
					// Get the average score to be used as final score for model building and prediction
					item.Value.ComputeAverageToxicity();

					if (item.Value.AvgToxicity < 0 || item.Value.AvgToxicity > 1)
					{
						Console.WriteLine("Warning item rev_id: '{0}' has an out of range toxicity value of {1}."
							, item.Key, item.Value.AvgToxicity);
					}
				}
			}

			// Write resulting csv file into file system
			var header = new string[] { "label", "rev_id", "comment" };
			var csvOut = new ToCSV(header, '\t');

			foreach (var item in AllComments.Values)
				csvOut.WriteLine(new string[]{ item.AvgToxicity.ToString(), item.RevId, item.Comment });

			csvOut.WriteFile(string.Format("ToxicityJoinedAnnotated.tsv"));
		}
	}
}
