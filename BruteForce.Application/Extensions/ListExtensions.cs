
using System.Data;
using ClosedXML.Excel;
using System.Reflection;

namespace BruteForce.Application.Extensions;

public static class ListExtensions
{
    public static byte[] ExportToExcel<T>(this IEnumerable<T> list)
    {
        var dtTable = list.ToDataTable();
        using var workbook = new XLWorkbook();
        workbook.Worksheets.Add(dtTable, typeof(T).Name);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static byte[] ExportToExcel<T>(this IEnumerable<T> list, string filePath, string tableName)
    {
        var dtTable = list.ToDataTable();
        using var workbook = new XLWorkbook(Path.Combine(filePath));
        var candidateTable = workbook.Table(tableName);
        candidateTable.ReplaceData(dtTable, propagateExtraColumns: true);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static byte[] ExportToExcelSheets(this IEnumerable<TableData> list, string filePath, IEnumerable<NamedData>? namedData = null)
    {
        using var workbook = new XLWorkbook(Path.Combine(filePath));
        foreach (var tabledata in list.Where(a => a.Data.Rows.Count > 0))
        {
            var dtTable = tabledata.Data;
            var candidateTable = workbook.Worksheet(tabledata.WorksheetName).Table(tabledata.TableName);
            candidateTable.ReplaceData(dtTable, propagateExtraColumns: true);
            candidateTable.Style.Alignment.WrapText = false;
        }

        if (namedData != null)
            foreach (var namedRange in namedData)
                foreach (var rangedata in namedRange.NamedRangeData)
                    workbook.Worksheet(namedRange.WorksheetName).Range(rangedata.Key).Value = rangedata.Value;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static DataTable ToDataTable<T>(this IEnumerable<T> items)
    {
        DataTable dataTable = new (typeof(T).Name);
        PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (PropertyInfo prop in Props)
        {
            dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(
            prop.PropertyType) ?? prop.PropertyType);
        }
        foreach (T item in items)
        {
            var values = new object[Props.Length];
            for (int i = 0; i < Props.Length; i++)
                values[i] = Props[i].GetValue(item, null);
            dataTable.Rows.Add(values);
        }
        return dataTable;
    }

    public static int Percentage<T>(this IEnumerable<T> items, int total) where T : class
    {
        if (total == 0) return 0;
        return items.Count() * 100 / total;
    }
}
/// <summary>
/// Use this class only with exportToExcel extension
/// </summary>
/// <typeparam name="T"></typeparam>
public class TableData
{
    public DataTable Data { get; set; }
    public string TableName { get; set; }
    public string WorksheetName { get; set; }

}
public class NamedData
{
    public string WorksheetName { get; set; }
    public Dictionary<string, string> NamedRangeData { get; set; } = [];
}