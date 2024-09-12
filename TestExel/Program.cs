﻿using AlphaInnotecClassLibrary;
using HovalClassLibrary;
using YorkClassLibrary;
using RemehaClassLibrary;
using EcoforestClassLibrary;
using BrötjeClassLibrary;
using System.Text.RegularExpressions;
using PanasonicClassLibrary;

class Program
{
    static async Task Main()
    { 
        Console.WriteLine("Write full path to Data Base:");
        string dataBasePath = Console.ReadLine(); //"E:\\Work\\wpopt-server\\wpoServer\\bin\\Debug\\wpov5_referenz_change.db";


        if (dataBasePath != null)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Choose Company: ");
                Console.WriteLine("1. York");
                Console.WriteLine("2. Alpha Innotec");
                Console.WriteLine("3. Hoval");
                Console.WriteLine("4. Remeha");
                Console.WriteLine("5. Ecofortest");
                Console.WriteLine("6. Panasonic");
                Console.WriteLine("7. Brötje");
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
                    case "5":
                        var ecoforest = new LogicEcoforest(dataBasePath);
                        await ecoforest.GoalLogicEcoforest();
                        break;
                    case "6":
                        var panasonic = new LogicPanasonic(dataBasePath);
                        await panasonic.GoalLogicPanasonic();
                        break;
                    case "7":
                        var Brötje = new LogicBrötje(dataBasePath);
                        await Brötje.GoalLogicBrötje();
                        break;
                    default:
                        Console.WriteLine("Error input");
                        break;
                }
            }

        }
        
    }
    
}
    

