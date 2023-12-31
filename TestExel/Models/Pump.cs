
using ClosedXML.Excel;
using System.Diagnostics.Metrics;

class Pump
{
    public string Name { get; set;}
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
    public void GetTypePumpInExel(string cell)
    {
        Type = _sheet.Cell(cell).GetString();
    }
    public void GetData(int numFirstDataLine, string letterColumnWithOutsideTemp, string letterColumnWithBeginningData, string letterColumnWithEndData)
    {
        int[] tempOut = GetAllTZ(letterColumnWithOutsideTemp, numFirstDataLine);              
        
        var data = new Dictionary<int, List<DataPump>>();       
        foreach( var item in tempOut )
        {
            
            List<double> getLineData = ReadExcelRangeToDoubleArray(letterColumnWithBeginningData + numFirstDataLine + ":" + letterColumnWithEndData + numFirstDataLine);
            //Remove null cell, it is empty in the file because it is merged with another cell
            getLineData.RemoveAt(4);
            //Remove PI values, because we calculate PI
            RemovePIValues(getLineData);
            //Add Values in dictionary 
            AddValuesInDictionary(data, getLineData, item);
            numFirstDataLine++;
        }
        Data = data;
    }
    //We get all the temperatures outside
    public int[] GetAllTZ(string cellLetter, int firstNum)
    {
        List<int> dataArray = new List<int>();
        var tz = _sheet.Cell(cellLetter + firstNum).GetString();
        while (!string.IsNullOrWhiteSpace(tz) && tz != "Obciążenie częściowe:  100%")
        {
            dataArray.Add(int.Parse(tz));
            firstNum++;
            tz = _sheet.Cell(cellLetter + firstNum).GetString();
        }
        return dataArray.ToArray();
    }
    public List<double> ReadExcelRangeToDoubleArray(string cellRange)
    {
        // Выбор ячеек по диапазону
        var range = _sheet.Range(cellRange);

        // Преобразование данных в массив double
        var dataArray = range.Cells().Select(cell =>
        {
            string cellValue = cell.GetString();
            return string.IsNullOrWhiteSpace(cellValue) ? 0.0 : double.Parse(cellValue);
        }).ToList();

        return dataArray;
    }

    public static void RemovePIValues(List<double> allData)
    {
        int indexRemovePI = 1;
        const int step = 2; 
        for (int i = 0; i < 9; i++)
        {
            allData.RemoveAt(indexRemovePI);
            indexRemovePI += step;
        }
    }
    public static void AddValuesInDictionary(Dictionary<int, List<DataPump>> dictionary, List<double> allData,int tempOut)
    {
        var datasPump = new List<DataPump>();
        int tempWaterIn = 25;
        for (int i = 0; i < allData.Count; i = i + 2)
        {
            datasPump.Add(new DataPump
            {
                Temp = tempWaterIn,
                HC = allData[i],
                COP = allData[i + 1]
            });
            tempWaterIn += 5;
        }
        dictionary.Add(tempOut, datasPump);
    }
}
