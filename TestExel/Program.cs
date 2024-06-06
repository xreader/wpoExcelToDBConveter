
using AlphaInnotecClassLibrary;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using HovalClassLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using TestExel;
using TestExel.DBConnection;
using TestExel.DBModels;
using TestExel.Models;
using TestExel.Services;
using TestExel.ServicesForDB;
using TestExel.StandartModels;
using YorkClassLibrary;
using RemehaClassLibrary;

class Program
{
    static async Task Main()
    { 
        //Console.WriteLine("Write full path to Data Base:");
        string dataBasePath = Console.ReadLine(); //"D:\\Work\\wpopt-server\\wpoServer\\bin\\Debug\\wpov5_referenz_change.db";//

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Choose Company: ");
            Console.WriteLine("1. York");
            Console.WriteLine("2. Alpha Innotec");
            Console.WriteLine("3. Hoval");
            Console.WriteLine("4. Remeha");
            var company = Console.ReadLine();
            switch (company)
            {
                case "1":
                    var york = new LogicYork(dataBasePath);
                    await york.GoalLogicYourk();
                    break;
                case "2":
                    var alphaInnotec = new LogicAlphaInnotec(dataBasePath);
                    await alphaInnotec.GoalLogicAlphaInnotec();
                    break;
                case "3":
                    var hoval = new LogicHoval(dataBasePath);
                    await hoval.GoalLogicHoval();
                    break;
                case "4":
                    var remeha = new LogicRemeha(dataBasePath);
                    await remeha.GoalLogicRemeha();
                    break;
                default:
                    Console.WriteLine("Error input");
                    break;
            }
        }
    }
}
    

