using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.DBConnection;
using TestExel.DBModels;
using TestExel.Repository;
using TestExel.StandartModels;

namespace TestExel.Services
{
    internal class PumpServiceForDB
    {
        private readonly PumpRepositoryForDB _pumpRepositoryForDB;
        public PumpServiceForDB(string pathDB)
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
               .UseSqlite("Data Source=" + pathDB + ";")
               .Options;
            _pumpRepositoryForDB = new PumpRepositoryForDB(new ApplicationDBContext(options));
        }
        public void GoalLogic(StandartPump pump)
        {
            var wpList = _pumpRepositoryForDB.FindLeaveByNamePump(pump.Name); // находим насос
            //var numForHash = 74892;// Для 35 при холод климат = 74892
            foreach(var wp in wpList)
            {
                var typeData = 0;
                if (wp != null)
                {
                    int typeClimat = 1;
                    int Grad = 35;
                    string bigHash = "";
                    var wpId = wp.nodeid_fk_nodes_nodeid; //находим его айди
                    var Idnid = wpId + 1;
                    while (_pumpRepositoryForDB.GetCountLeavesById(Idnid) != 6)
                    {
                        Idnid++;
                    }
                    
                    while (_pumpRepositoryForDB.GetCountLeavesById(Idnid) == 6 || _pumpRepositoryForDB.GetCountLeavesById(Idnid+1) == 6 || _pumpRepositoryForDB.GetCountLeavesById(Idnid)==0) // Всегда 6 записей в которых храняться данные 
                    {
                        var dataWp = _pumpRepositoryForDB.GetLeavesById(Idnid);
                        var WPleistATemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1351); // берем температуру на улице
                        if (WPleistATemp != null)
                        {
                            var WPleistATempValue = WPleistATemp.value_as_int;
                            if (pump.Data.TryGetValue((int)WPleistATempValue, out var myPumpData)) // проеверяем есть ли данные при такой температуре на улице
                            {
                                var WPleistVTemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1011);  //Находим температуру внутри
                                var RefKlimazone14825 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1356); //находим тип климата
                                if (WPleistVTemp != null && RefKlimazone14825 != null)
                                {
                                    var dataPumpForThisData = myPumpData.FirstOrDefault(x => x.ForTemp == WPleistVTemp.value_as_int && x.Climate == RefKlimazone14825.value_as_int.ToString());
                                    if (dataPumpForThisData != null)
                                    {
                                        var WPleistHeiz = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1012);
                                        var WPleistCOP = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1221);
                                        var Gui14825Hashcode = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1368);
                                        if (WPleistHeiz != null && WPleistCOP != null && Gui14825Hashcode != null)
                                        { //Меняем даныне для P и COP 
                                            ChangeDataForSendToDB(ref typeData, WPleistHeiz, WPleistCOP, dataPumpForThisData);
                                            _pumpRepositoryForDB.UpdateLeaves(WPleistHeiz);
                                            _pumpRepositoryForDB.UpdateLeaves(WPleistCOP);
                                            var str = WPleistATempValue + "#" + WPleistHeiz.value_as_int + "#" + WPleistCOP.value_as_int;
                                            //str = numForHash+ "#" + str;
                                            int hash = GetHashCode(str);
                                            Gui14825Hashcode.value = hash.ToString();
                                            _pumpRepositoryForDB.UpdateLeaves(Gui14825Hashcode);
                                            if (WPleistVTemp.value_as_int == Grad && RefKlimazone14825.value_as_int == typeClimat && _pumpRepositoryForDB.GetCountLeavesById(Idnid + 1) == 6)
                                            {
                                                bigHash += hash + "#";
                                            }
                                            else
                                            {
                                                UpdateBigHash(Idnid, wpId, ref Grad, ref typeClimat, hash.ToString(), ref bigHash);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var WPleistVTemp2 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1011);  //Находим температуру внутри
                                        var RefKlimazone148252 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1356); //находим тип климата
                                        var Gui14825Hashcode2 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1368);
                                        if (WPleistVTemp2.value_as_int == Grad && RefKlimazone148252.value_as_int == typeClimat && _pumpRepositoryForDB.GetCountLeavesById(Idnid + 1) == 6)
                                        {
                                            bigHash += Gui14825Hashcode2.value + "#";
                                        }
                                        else
                                        {
                                            UpdateBigHash(Idnid, wpId, ref Grad, ref typeClimat, Gui14825Hashcode2.value, ref bigHash);
                                        }
                                    }
                                }
                            }
                            //если такого значения нет в данных то данные из бд не меняются а только хэш добовляется в строку хэшей
                            else
                            {
                                var WPleistVTemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1011);  //Находим температуру внутри
                                var RefKlimazone14825 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1356); //находим тип климата
                                var Gui14825Hashcode = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1368);
                                if (WPleistVTemp.value_as_int == Grad && RefKlimazone14825.value_as_int == typeClimat && _pumpRepositoryForDB.GetCountLeavesById(Idnid + 1) == 6)
                                {
                                    bigHash += Gui14825Hashcode.value + "#";
                                }
                                else
                                {
                                    UpdateBigHash(Idnid, wpId, ref Grad, ref typeClimat, Gui14825Hashcode.value, ref bigHash);
                                }
                            }
                        }
                        Idnid++;
                        //numForHash++;
                    }
                    Console.WriteLine("Pump -" + wp.value + "  Update!");
                    Console.WriteLine();
                    Console.WriteLine();
                    Task.Delay(5000).Wait();
                }
            }
            
        }

        private void ChangeDataForSendToDB(ref int typeData, Leaves WPleistHeiz, Leaves WPleistCOP, StandartDataPump dataPumpForThisData)
        {
            switch (typeData)
            {
                case 0:
                    WPleistHeiz.value_as_int = (int)(dataPumpForThisData.MinHC * 100);
                    WPleistCOP.value_as_int = (int)(dataPumpForThisData.MinCOP * 100);
                    typeData++;
                    break;
                case 1:
                    WPleistHeiz.value_as_int = (int)(dataPumpForThisData.MidHC * 100);
                    WPleistCOP.value_as_int = (int)(dataPumpForThisData.MidCOP * 100);
                    typeData++;
                    break;
                case 2:
                    WPleistHeiz.value_as_int = (int)(dataPumpForThisData.MaxHC * 100);
                    WPleistCOP.value_as_int = (int)(dataPumpForThisData.MaxCOP * 100);
                    typeData = 0;
                    break;
                default:
                    break;
            }
        }

        //Обновляем большой хэш и переключаемся на следущую температуру и климат
        private void UpdateBigHash(int Idnid, int wpId, ref int Grad, ref int typeClimat, string hash, ref string bigHash)
        {
            if (_pumpRepositoryForDB.GetCountLeavesById(Idnid + 1) == 7)
            {
                bigHash += hash + "#";

            }
            if (Grad == 35 && typeClimat == 1)
            {   if(bigHash.Count() >= 150)
                {
                    var bigHashDB = _pumpRepositoryForDB.GetBigHashFor35GradForKaltesKlimaByWpId(wpId);
                    if(bigHashDB != null)
                    {
                        bigHashDB.value = bigHash;
                        if (_pumpRepositoryForDB.UpdateLeaves(bigHashDB))
                            Console.WriteLine("------Up Big Hash For 35 Grad And Cold");
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }
                   
                }                
                Grad = 55;
            }
            else if (Grad == 55 && typeClimat == 1)
            {
                if (bigHash.Count() >= 150)
                {
                    var bigHashDB = _pumpRepositoryForDB.GetBigHashFor55GradForKaltesKlimaByWpId(wpId);
                    if (bigHashDB != null)
                    {
                        bigHashDB.value = bigHash;
                        if (_pumpRepositoryForDB.UpdateLeaves(bigHashDB))
                            Console.WriteLine("------Up Big Hash For 55 Grad And Cold");
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }
                }
                Grad = 35;
                typeClimat = 2;
            }
            else if (Grad == 35 && typeClimat == 2)
            {
                if (bigHash.Count() >= 150)
                {
                    var bigHashDB = _pumpRepositoryForDB.GetBigHashFor35GradForMittelKlimaByWpId(wpId);
                    if (bigHashDB != null)
                    {
                        bigHashDB.value = bigHash;
                        if (_pumpRepositoryForDB.UpdateLeaves(bigHashDB))
                            Console.WriteLine("------Up Big Hash For 35 Grad And Mid");
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }
                }
                Grad = 55;
            }
            else if (Grad == 55 && typeClimat == 2)
            {
                if (bigHash.Count() >= 150)
                {
                    var bigHashDB = _pumpRepositoryForDB.GetBigHashFor55GradForMittelKlimaByWpId(wpId);
                    if (bigHashDB != null)
                    {
                        bigHashDB.value = bigHash;
                        if (_pumpRepositoryForDB.UpdateLeaves(bigHashDB))
                            Console.WriteLine("------Up Big Hash For 55 Grad And Mid");
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }
                }
                Grad = 35;
                typeClimat = 1;
            }
            if (_pumpRepositoryForDB.GetCountLeavesById(Idnid + 1) == 6)
            {
                bigHash = "" + hash + "#";

            }
            else
            {
                bigHash = "";
            }
        }
        private int GetHashCode(string s)
        {
            int hash = 0;
            int len = s.Length;

            if (len == 0)
                return hash;

            for (int i = 0; i < len; i++)
            {
                char chr = s[i];
                hash = ((hash << 5) - hash) + chr;
                hash |= 0; // Convert to 32-bit integer
            }

            return hash;
        }
    }
}
