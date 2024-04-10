using System;
using System.Collections.Generic;
using System.IO;

namespace Thermo.SampleManager.Library.ObjectModel
{
	/// <summary>
	/// Orderes numbered files.
	/// </summary>
	internal class ImageFileComparer : IComparer<string>
	{
		#region Member Variables

		private readonly string m_FilePrefix;
		private readonly string m_FileExtension;

		#endregion

		#region Construct

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageFileComparer"/> class.
		/// </summary>
		/// <param name="filePrefix">The file prefix.</param>
		/// <param name="fileExtension">The file extension.</param>
		public ImageFileComparer(string filePrefix, string fileExtension)
		{
			m_FilePrefix = filePrefix.ToUpper();
			m_FileExtension = fileExtension.ToUpper();
		}

		#endregion

		#region IComparer<string> Members

		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		/// Value
		/// Condition
		/// Less than zero
		/// <paramref name="x"/> is less than <paramref name="y"/>.
		/// Zero
		/// <paramref name="x"/> equals <paramref name="y"/>.
		/// Greater than zero
		/// <paramref name="x"/> is greater than <paramref name="y"/>.
		/// </returns>
		public int Compare(string x, string y)
		{
			string fileNameX = Path.GetFileName(x).ToUpper().Replace(m_FilePrefix, "");
			int xNum = Convert.ToInt32(fileNameX.Replace(m_FileExtension, ""));
			string fileNameY = Path.GetFileName(y).ToUpper().Replace(m_FilePrefix, "");
			int yNum = Convert.ToInt32(fileNameY.Replace(m_FileExtension, ""));

			return xNum.CompareTo(yNum);
		}

		#endregion
	}
}