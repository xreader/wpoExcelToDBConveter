﻿using ClosedXML.Excel;
using DocumentFormat.OpenXml.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.Services;
using TestExel.StandartModels;

namespace AlphaInnotecClassLibrary.Services
{
    class PumpServiceForAlphaInnotec : PumpService
    {
        private readonly XLWorkbook workbook;

        public PumpServiceForAlphaInnotec(string excelFilePath)
        {
            workbook = new XLWorkbook(excelFilePath);
        }
        public List<StandartPump> GetDataInListStandartPumpsAlpha(List<StandartPump> standartPumps, List<Pump> oldPumps, int[] outTemps, int[] flowTemps, int forTemp, string climat, string typeFile)
        {
            foreach (var oldPump in oldPumps)
            {
                //Get the pump data dictionary
                Dictionary<int, List<DataPump>> oldDictionary = oldPump.Data;
                if (standartPumps.Any(x => x.Name == oldPump.Name))
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = standartPumps.FirstOrDefault(x => x.Name == oldPump.Name).Data;
                    ChooseMethodForConvert(typeFile, outTemps, flowTemps, forTemp, climat, newDictionary, oldPump);

                }
                else
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = new Dictionary<int, List<StandartDataPump>>();
                    ChooseMethodForConvert(typeFile,outTemps, flowTemps, forTemp, climat, newDictionary, oldPump);
                    var standartPump = new StandartPump()
                    {
                        Name = oldPump.Name,
                        Data = newDictionary
                    };
                    standartPumps.Add(standartPump);
                }
            }

            return standartPumps;


        }
        private void ChooseMethodForConvert(string typeFile, int[] outTemps, int[] flowTemps, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary, Pump oldPump)
        {
            switch(typeFile)
            {
                case "Wasser":
                    GetConvertDataForWasser(outTemps, flowTemps, forTemp, climat, newDictionary, oldPump);
                    break;
                case "Luft":
                    GetConvertDataForLuft(outTemps, flowTemps, forTemp, climat, newDictionary, oldPump);
                    break;
                case "Sole":
                    GetConvertDataForSole(outTemps, flowTemps, forTemp, climat, newDictionary, oldPump);
                    break;
            }
        }
        //Get already converted data(get first value where count == 2)
        private void GetConvertDataForWasser(int[] outTemps, int[] flowTemp, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary, Pump oldPump)
        {
            Dictionary<int, List<DataPump>> oldDictionary = oldPump.Data;
            for (int i = 0; i < outTemps.Length; i++)
            {
                var firstDataForEachKey = oldDictionary.Values.Where(x=>x.Count == 2).FirstOrDefault();
                //Convert values
                ConvertDataInStandart(firstDataForEachKey, flowTemp[i], outTemps[i], forTemp, climat, newDictionary, oldPump);
            }
        }
        //Get already converted data(get first value where count == 2)
        private void GetConvertDataForSole(int[] outTemps, int[] flowTemp, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary, Pump oldPump)
        {
            Dictionary<int, List<DataPump>> oldDictionary = oldPump.Data;
            for (int i = 0; i < outTemps.Length; i++)
            {
                var firstDataForEachKey = oldDictionary.Values.Where(x => x.Count == 2).FirstOrDefault();
                //Convert values
                ConvertDataInStandart(firstDataForEachKey, flowTemp[i], outTemps[i], forTemp, climat, newDictionary, oldPump);
            }
        }
        //Get already converted data
        private void GetConvertDataForLuft(int[] outTemps, int[] flowTemp, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary, Pump oldPump)
        {
            Dictionary<int, List<DataPump>> oldDictionary = oldPump.Data;
            for (int i = 0; i < outTemps.Length; i++)
            {

                if (oldDictionary.ContainsKey(outTemps[i]))
                {
                    //Сode if there is a value for this temperature outside
                    oldDictionary.TryGetValue(outTemps[i], out List<DataPump> oldDataPump);
                    //Convert values
                    ConvertDataInStandart(oldDataPump, flowTemp[i], outTemps[i], forTemp, climat, newDictionary, oldPump);

                }
                else
                {
                    //Code if there is no such temperature outside in the table
                    //Search for data for a temperature outside when there is none
                    var oldDataPump = oldDictionary.FirstOrDefault(pair => pair.Key > outTemps[i]).Value;// FindDataWhenNoDatainThisOutTemp(oldDictionary, outTemps[i]);
                    
                    //Convert values
                    ConvertDataInStandart(oldDataPump, flowTemp[i], outTemps[i], forTemp, climat, newDictionary, oldPump);
                }
            }
        }
        //Get all pumps from Exel
        public List<Pump> GetAllPumpsFromExel(int numFirstDataLineFor35Grad, int numFirstDataLineFor55Grad, string letterColumnWithOutsideTemp, string letterColumnWithBeginningData, string letterColumnWithEndData)
        {
            List<Pump> pumps = new List<Pump>();
            var sheetsCount = workbook.Worksheets.Count;
            for (int i = 1; i <= sheetsCount; i++)
            {
                var worksheet = workbook.Worksheet(i);
                var pump = new Pump(worksheet);
                pump.Name = worksheet.Name;
                pump.GetData(numFirstDataLineFor35Grad, letterColumnWithOutsideTemp, letterColumnWithBeginningData, letterColumnWithEndData, 35);
                pump.GetData(numFirstDataLineFor55Grad, letterColumnWithOutsideTemp, letterColumnWithBeginningData, letterColumnWithEndData, 55);
                if (pump != null && pump.Name != "")
                    pumps.Add(pump);

            }
            RoundCOPAndP(pumps);
            return pumps;
        }
        

    }

}

