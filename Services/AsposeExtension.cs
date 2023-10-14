using Aspose.Cells;

namespace AdminBot.Services;

public static class AsposeExtension
{
    public static Worksheet? GetSheet(this Workbook workbook, string nameSheet)
    {
        return workbook.Worksheets.FirstOrDefault(worksheet => worksheet.Name == nameSheet);
    }
    public static int FindColumnByName(this Worksheet sheet, string columnName, string columnCategory)
    {
        var firstOption = (columnName + " " + columnCategory.Trim()).ToLower();
        var secondOption = (columnCategory.Trim() + " " + columnName).ToLower();
        for (var i = 1; i <= sheet.Cells.MaxDataColumn; i++)
            if (firstOption == sheet.Cells[0, i].Value.ToString()?.ToLower())
                return i;
            else if (secondOption == sheet.Cells[0, i].Value.ToString()?.ToLower())
                return i;
        return 0;
    }
}