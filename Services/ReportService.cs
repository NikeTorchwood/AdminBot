using System.Diagnostics;
using System.Text;
using AdminBot.Entities;
using AdminBot.Services.ServiceInterfaces;
using Aspose.Cells;

namespace AdminBot.Services;

public class ReportService
{
    public Worksheet DataList { get; }
    private readonly IStoreService _storeService;

    private readonly List<string> _directionNames = new()
    {
        "KPI1",
        "KPI2",
        "KPI3",
        "KPI4",
        "KPI5",
        "KPI6",
        "KPI7",
    };
    public ReportService(IStoreService storeService, Workbook workbook)
    {
        _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
        if (workbook == null)
        {
            throw new ArgumentNullException(nameof(workbook));
        }
        DataList = workbook.GetSheet("Данные Магазина");
    }

    public async Task StartUpdate()
    {
        Console.WriteLine("Подготовка к загрузке в БД");
        var sw = new Stopwatch();
        sw.Restart();
        var storeNames =await _storeService.GetStoreNameList();
        var storeList =  storeNames.Select(storeName => 
            GetStoreFromReport(storeName, _directionNames, this)).ToList();
        sw.Stop();
        Console.WriteLine($"Подготовка к загрузке в БД : {sw.Elapsed}");
        await _storeService.UpdateStores(storeList);
    }
    public static EconomicDirection GetDirectionFromReport(string storeCode, string directionName, Worksheet dataList)
    {
        var row = dataList.Cells.Find(storeCode, dataList.Cells.FirstCell).Row;
        var planColumn = dataList.FindColumnByName(directionName, "План");
        var factColumn = dataList.FindColumnByName(directionName, "Факт");
        var planValue = dataList.Cells[row, planColumn].Type != CellValueType.IsString
            ? dataList.Cells[row, planColumn].IntValue
            : 0;
        var factValue = dataList.Cells[row, factColumn].Type != CellValueType.IsString
            ? dataList.Cells[row, factColumn].IntValue
            : 0;
        return new EconomicDirection(directionName, planValue, factValue);
    }
    public Store GetStoreFromReport(string storeCode, List<string> directionNameList, ReportService reportService)
    {
        var economicDirections = directionNameList.Select(directionName
            => GetDirectionFromReport(storeCode, directionName, reportService.DataList)).ToList();
        return new Store(storeCode, economicDirections);
    }
}