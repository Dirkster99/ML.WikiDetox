namespace wikiDetox.Models
{
	using Microsoft.ML.Data;

	/// <summary>
	/// The SentimentIssue class represents a single sentiment record.
	/// </summary>
	public class SentimentIssue
	{
		[LoadColumn(0)] public bool Label { get; set; }

		[LoadColumn(2)] public string Text { get; set; }
	}
}
