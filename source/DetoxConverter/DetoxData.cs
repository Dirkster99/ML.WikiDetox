namespace DetoxConverter
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Implements an in-memory model of the detox data that combines all data items in one place
	/// to perform join over 2 tsv files and use resulting rev_id, score and comment for later processing.
	/// </summary>
	public class DetoxData
	{
		#region fields
		private readonly string _rev_id;
		private readonly string _row_idx;
		private readonly string _comment;
		private readonly List<float> _toxicity_score;
		private readonly List<int> _toxicity;
		#endregion fields

		#region ctors
		/// <summary>
		/// Class constructor.
		/// </summary>
		/// <param name="rev_id"></param>
		/// <param name="row_idx"></param>
		/// <param name="comment"></param>
		public DetoxData(string rev_id, string row_idx, string comment)
			: this()
		{
			_rev_id = rev_id;
			_row_idx = row_idx;
			_comment = comment;
		}

		/// <summary>
		/// Hidden class constructor.
		/// </summary>
		protected DetoxData()
		{
			_toxicity_score = new List<float>();
			_toxicity = new List<int>();
		}
		#endregion ctors

		#region properties
		/// <summary>
		/// the unique identifier for the comment
		/// </summary>
		public string RevId { get { return _rev_id; } }

		/// <summary>
		/// The index row in the dataframe that was used to read in this data
		/// (a input line indicator useful for debugging).
		/// </summary>
		public string RowIdx { get { return _row_idx; } }

		/// <summary>
		/// the text of the comment.
		/// </summary>
		public string Comment { get { return _comment; } }

		/// <summary>
		/// Categorical variable ranging from very toxic (-2), to neutral (0), to very healthy (2).
		/// https://meta.wikimedia.org/wiki/Research:Detox/Data_Release
		/// </summary>
		public IReadOnlyList<float> ToxicityScore { get { return _toxicity_score; } }

		/// <summary>
		/// Indicator variable for whether the worker thought the comment is toxic.
		/// The annotation takes on the value 1 if the worker considered the comment toxic
		/// (i.e worker gave a toxicity_score less than 0) and value 0 if the worker considered
		/// the comment neutral or healthy (i.e worker gave a toxicity_score greater or equal to 0).
		/// Takes on values in {0, 1}.
		/// https://meta.wikimedia.org/wiki/Research:Detox/Data_Release
		/// </summary>
		public IReadOnlyList<int> Toxicity { get { return _toxicity; } }

		/// <summary>
		/// Invoke <see cref="ComputeAverageToxicity"/> before getting the average score over
		/// <see cref="ToxicityScore"/> from this property.
		/// </summary>
		public float AvgToxicityScore { get; protected set; }

		/// <summary>
		/// Invoke <see cref="ComputeAverageToxicity"/> before getting the average score over
		/// <see cref="Toxicity"/> from this property.
		/// </summary>
		public int AvgToxicity { get; protected set; }
		#endregion properties

		#region methods
		/// <summary>
		/// Add another toxicity score pair for this object.
		/// </summary>
		/// <param name="toxicity_score"></param>
		/// <param name="toxicity"></param>
		public void AddToxicity(float toxicity_score, int toxicity)
		{
			_toxicity_score.Add(toxicity_score);
			_toxicity.Add(toxicity);
		}

		/// <summary>
		/// Computes average scores and makes them available in
		/// <see cref="AvgToxicity"/> and <see cref="AvgToxicityScore"/>.
		/// </summary>
		public void ComputeAverageToxicity()
		{
			float toxicityScore = 0;
			for (int i = 0; i < ToxicityScore.Count; i++)
				toxicityScore += ToxicityScore[i];

			AvgToxicityScore = toxicityScore / ToxicityScore.Count;

			float toxicity = 0;
			for (int i = 0; i < Toxicity.Count; i++)
				toxicity += Toxicity[i];

			AvgToxicity = (int)Math.Round(toxicity / Toxicity.Count);
		}
		#endregion methods
	}
}
