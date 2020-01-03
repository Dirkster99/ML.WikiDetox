namespace wikiDetox.Models
{
	using Microsoft.ML.Data;

	/// <summary>
	/// The SentimentPrediction class represents a single sentiment prediction.
	/// </summary>
	public class SentimentPrediction
	{
		[ColumnName("PredictedLabel")] public bool Prediction { get; set; }
		public float Probability { get; set; }

		public float Score { get; set; }
	}
}
