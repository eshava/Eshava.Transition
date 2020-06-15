using System.Collections.Generic;

namespace Eshava.Transition.Models
{
	public class DataProperty
	{
		public IEnumerable<string> MappingProperties { get; set; }
		public IEnumerable<DataProperty> DataProperties { get; set; }

		public string PropertyTarget { get; set; }
		public string PropertySource { get; set; }

		/// <summary>
		/// Configured value, whether is used if PropertySource is not set
		/// HasMapping has to be true
		/// </summary>
		public string MappedValue { get; set; }
		public bool HasMapping { get; set; }
		/// <summary>
		/// Configured key/value pair list to translate incoming values
		/// HasMapping has to be false
		/// </summary>
		public IEnumerable<MappingPair> ValueMappings { get; set; }

		/// <summary>
		/// Optional
		/// Fallback: InvariantCulture
		/// </summary>
		public string CultureCode { get; set; }
		public bool IsSameDataRecord { get; set; }              /* Export */
		public string ConditionalPropertyName { get; set; }     /* Export */
		public string ConditionalPropertyValue { get; set; }    /* Export */
		public bool SplitExportResult { get; set; }             /* Export */

		#region json
		public bool ExportAsString { get; set; }                /* Export */
		#endregion

		#region edi
		public int PositionEDI { get; set; }
		public int LengthEDI { get; set; }
		public int LineIndexEDI { get; set; }
		public bool CanRepeatEDI { get; set; }
		#endregion

		#region csv
		public bool HasSurroundingQuotationMarksCSV { get; set; }
		public int StartRowIndexCSV { get; set; }
		public bool HasColumnNamesCSV { get; set; }
		public char SeparatorCSVColumn { get; set; }
		public int PropertySourceIndexCSV { get; set; }            /* Export */
		#endregion

		#region xml
		public IEnumerable<AdditionalPropertyData> AdditionalPropertyData { get; set; }            /* Export */
		#endregion
	}
}