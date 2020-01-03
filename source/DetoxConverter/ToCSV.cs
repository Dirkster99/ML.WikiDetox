namespace DetoxConverter
{
	using System.IO;
	using System.Text;

	/// <summary>
	/// Small simple utility class to write a CSV file with a delimiter as parameter.
	/// 
	/// The class makes no formating or error checking for you so all strings must be checked
	/// (eg.: strings should not contain pipe symbole '|' if pipe was used as delimiter)
	/// and be csv conform before calling methods in this class.
	/// </summary>
	public class ToCSV
	{
		#region fields
		private readonly char _deli;
		private readonly string[] _headers;
		private readonly StringBuilder _CSVcontent;
		private bool _headerWritten;
		#endregion fields

		#region ctors
		/// <summary>
		/// class constructor
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="deli"></param>
		public ToCSV(string[] headers, char deli = '|')
			: this()
		{
			_headers = headers;
			_deli = deli;
		}

		/// <summary>
		/// Hidden class constructor
		/// </summary>
		protected ToCSV()
		{
			_headerWritten = false;
			_CSVcontent = new StringBuilder();
		}
		#endregion ctors

		#region methods
		/// <summary>
		/// Writes the content of the in memory representation into the filesystem.
		/// </summary>
		/// <param name="filename"></param>
		public void WriteFile(string filename)
		{
			File.WriteAllText(filename, _CSVcontent.ToString());
		}

		/// <summary>
		/// Writes each line in turn into the in memory
		/// representation of the CSV file.
		/// </summary>
		/// <param name="csvLine"></param>
		public void WriteLine(string[] csvLine)
		{
			if (_headerWritten == false)
			{
				CSVHeader(_headers, _CSVcontent);
				_headerWritten = true;
			}

			_CSVcontent.Append(csvLine[0]);
			for (int i = 1; i < csvLine.Length; i++)
				_CSVcontent.Append(_deli + csvLine[i]);

			_CSVcontent.Append('\n');
		}

		/// <summary>
		/// Builds a CSV header from the given strings and returns it
		/// or returns an empty string if given input collection was null or empty.
		/// </summary>
		/// <param name="headers"></param>
		/// <param name="deli"></param>
		/// <returns></returns>
		protected void CSVHeader(string[] headers, StringBuilder sb)
		{
			if (headers != null)
			{
				if (headers.Length > 0)
				{
					sb.Append(headers[0]);

					for (int i = 1; i < headers.Length; i++)
						sb.Append(_deli + headers[i]);

					sb.Append('\n');
				}
			}
		}
		#endregion methods
	}
}
