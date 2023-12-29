
using ClosedXML.Excel;
using System.Diagnostics.Metrics;

class Pump
{
    public string Name {get;set;}
    public string Type{ get;set;}
    public Dictionary<int, List<DataPump>> Data { get; set; }
    private IXLWorksheet _sheet;

    public Pump (IXLWorksheet sheet)
    {
        _sheet = sheet;
    }
    public Pump()
    {

    }

    public void GetNamePumpInExel(string cell)
    {
        Name = _sheet.Cell(cell).GetString();
    }
    public void GetTypePumpInExel( string cell)
    {
        Type = _sheet.Cell(cell).GetString();
    }
    public void GetData()
    {
        int firstNumStr = 6;
        int[] tempOut = GetAllTZ(_sheet,"A", firstNumStr);              
        
        var data = new Dictionary<int, List<DataPump>>();       
        foreach( var item in tempOut )
        {
            var datasPump = new List<DataPump>();
            List<double> getLineData = ReadExcelRangeToDoubleArray(_sheet, "C"+firstNumStr+":AD"+firstNumStr);
            getLineData.RemoveAt(4);
            int tempWaterIn = 25;
            for (int i = 0; i < getLineData.Count; i = i + 3)
            {
                datasPump.Add(new DataPump
                {
                    Temp = tempWaterIn,
                    HC = getLineData[i],
                    PI = getLineData[i + 1],
                    COP = getLineData[i + 2]
                });
                tempWaterIn += 5;
            }
            data.Add(item, datasPump);
            firstNumStr++;
        }
        Data = data;
    }
    public int[] GetAllTZ(IXLWorksheet sheet, string cellLetter, int firstNum)
    {
        List<int> dataArray = new List<int>();
        var tz = sheet.Cell(cellLetter + firstNum).GetString();
        while (!string.IsNullOrWhiteSpace(tz) && tz != "Obciążenie częściowe:  100%")
        {
            dataArray.Add(int.Parse(tz));
            firstNum++;
            tz = sheet.Cell(cellLetter + firstNum).GetString();
        }
        return dataArray.ToArray();
    }
    public List<double> ReadExcelRangeToDoubleArray(IXLWorksheet sheet, string cellRange)
    {
        // Выбор ячеек по диапазону
        var range = sheet.Range(cellRange);

        // Преобразование данных в массив double
        var dataArray = range.Cells().Select(cell =>
        {
            string cellValue = cell.GetString();
            return string.IsNullOrWhiteSpace(cellValue) ? 0.0 : double.Parse(cellValue);
        }).ToList();

        return dataArray;
    }    
}
