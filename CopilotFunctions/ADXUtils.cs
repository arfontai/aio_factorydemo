using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Func
{
    internal class ADXUtils
    {
        public static DataTable ConvertDecimal(DataTable dt)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var column = dt.Columns[i];

                if (column.DataType == typeof(System.Data.SqlTypes.SqlDecimal))
                {
                    var newColumn = new DataColumn(column.ColumnName, typeof(decimal));
                    newColumn.ColumnName = "_" + column.ColumnName;
                    dt.Columns.Add(newColumn);
                    var colIndex = column.Ordinal;

                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        if (!dt.Rows[j][colIndex].Equals(System.Data.SqlTypes.SqlDecimal.Null))
                        {
                            dt.Rows[j][newColumn] = ((System.Data.SqlTypes.SqlDecimal)dt.Rows[j][column]).Value;
                        }
                    }

                    dt.Columns.Remove(column);
                    newColumn.SetOrdinal(colIndex);
                    dt.Columns[colIndex].ColumnName = dt.Columns[colIndex].ColumnName.Substring(1);
                }
            }
            return dt;
        }
    }
}
