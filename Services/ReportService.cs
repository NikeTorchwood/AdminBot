using AdminBot.Entities;
using AdminBot.Services.ServiceInterfaces;
using Aspose.Cells;
using System.Diagnostics;

namespace AdminBot.Services;

public class ReportService
{
    private readonly Worksheet _dataList;
    private readonly IStoreService _storeService;
    public List<string> ErrorStoreCodeList { get; }

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
        _dataList = workbook.GetSheet("Данные Магазина");
        ErrorStoreCodeList = new List<string>();
    }

    public async Task StartUpdate()
    {
        Console.WriteLine("Подготовка к загрузке в БД");
        var sw = new Stopwatch();
        sw.Restart();
        var storeNames = await _storeService.GetStoreNameList();
        var storeList = new List<Store>();
        foreach (var storeName in storeNames)
        {
            var store = GetStoreFromReport(storeName, _directionNames);
            if (store != null)
            {
                storeList.Add(store);
            }
            else
            {
                ErrorStoreCodeList.Add(storeName);
            }
        }
        sw.Stop();
        Console.WriteLine($"Подготовка к загрузке в БД : {sw.Elapsed}");
        await _storeService.UpdateStores(storeList);
    }
    public EconomicDirection GetDirectionFromReport(string storeCode, string directionName)
    {
        int row;
        if (_dataList.Cells.Find(storeCode, _dataList.Cells.FirstCell) != null)
        {
            row = _dataList.Cells.Find(storeCode, _dataList.Cells.FirstCell).Row;
        }
        else
        {
            return null;
        }
        var planColumn = _dataList.FindColumnByName(directionName, "План");
        var factColumn = _dataList.FindColumnByName(directionName, "Факт");
        var planValue = _dataList.Cells[row, planColumn].Type != CellValueType.IsString
            ? _dataList.Cells[row, planColumn].IntValue
            : 0;
        var factValue = _dataList.Cells[row, factColumn].Type != CellValueType.IsString
            ? _dataList.Cells[row, factColumn].IntValue
            : 0;
        return new EconomicDirection(directionName, planValue, factValue);
    }
    public Store GetStoreFromReport(string storeCode, List<string> directionNameList)
    {
        var economicDirections = new List<EconomicDirection>();
        var countLP = GetCountLPFromReport(storeCode);
        if (countLP == default)
        {
            return null;
        }
        foreach (var directionName in directionNameList)
        {
            var economicDirection = GetDirectionFromReport(storeCode, directionName);
            if (economicDirection != null)
            {
                economicDirections.Add(economicDirection);
            }
            else
            {
                return null;
            }
        }
        return new Store(storeCode, economicDirections, countLP);
    }

    private int GetCountLPFromReport(string storeCode)
    {
        int row;
        if (_dataList.Cells.Find(storeCode, _dataList.Cells.FirstCell) != null)
        {
            row = _dataList.Cells.Find(storeCode, _dataList.Cells.FirstCell).Row;
        }
        else
        {
            return default;
        }
        var countColumn = _dataList.FindColumnByName("Количество", "ЛП");
        var countLP = _dataList.Cells[row, countColumn].Type != CellValueType.IsString
            ? _dataList.Cells[row, countColumn].IntValue
            : 0;
        return countLP;
    }
}