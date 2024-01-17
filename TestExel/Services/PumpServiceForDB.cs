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
        private readonly LeaveRepository _leaveRepository;
        private readonly NodeRepository _nodeRepository;
        public PumpServiceForDB(string pathDB)
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
               .UseSqlite("Data Source=" + pathDB + ";")
               .Options;
            _leaveRepository = new LeaveRepository(new ApplicationDBContext(options));
            _nodeRepository = new NodeRepository(new ApplicationDBContext(options));
        }
        public void MyNewLogic(StandartPump pump)
        {
            var wpList = _leaveRepository.FindLeaveByNamePump(pump.Name);
            foreach (var wp in wpList)
            {
                var typeData = 0;
                if (wp != null)
                {
                    int typeClimat = 1;
                    int Grad = 35;
                    string bigHash = "";
                    var wpId = wp.nodeid_fk_nodes_nodeid; //находим его айди
                    var leavesIdWithOldDataList = _nodeRepository.GetIdLeavesWithDataByPumpId(wpId);//список IdLeaves которые надо менять
                    var actuelIndexLeaveIdInList = 0;
                    foreach (var leaveWithOldData in leavesIdWithOldDataList)
                    {
                        var dataWp = _leaveRepository.GetLeavesById(leaveWithOldData);
                        var WPleistATemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1351); // берем температуру на улице
                        if (WPleistATemp != null)
                        {
                            //Если есть даные с такой температурой на улице в модели которую мы получили после конвертации и стандартизации
                            if (pump.Data.TryGetValue((int)WPleistATemp.value_as_int, out var myPumpData))
                            {
                                var WPleistVTemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1011);  //Находим температуру внутри насоса в записи
                                var RefKlimazone14825 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1356); //находим тип климата в записи
                                if (WPleistVTemp != null && RefKlimazone14825 != null)
                                {
                                    //получаем даныне из стандартизованой модели с нужным климатом и температурой
                                    var dataPumpForThisData = myPumpData.FirstOrDefault(x => x.ForTemp == WPleistVTemp.value_as_int && x.Climate == RefKlimazone14825.value_as_int.ToString());
                                    if (dataPumpForThisData != null)
                                    {
                                        var WPleistHeiz = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1012); //leave с данными для P
                                        var WPleistCOP = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1221); // leave c данными для COP
                                        var Gui14825Hashcode = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1368); //leave с HashCode
                                        if (WPleistHeiz != null && WPleistCOP != null && Gui14825Hashcode != null)
                                        { //Меняем даныне для P и COP 
                                            ChangeDataForSendToDB(ref typeData, WPleistHeiz, WPleistCOP, dataPumpForThisData);
                                            _leaveRepository.UpdateLeaves(WPleistHeiz);
                                            _leaveRepository.UpdateLeaves(WPleistCOP);
                                            //формируем хэш и обновляем
                                            var str = WPleistATemp.value_as_int + "#" + WPleistHeiz.value_as_int + "#" + WPleistCOP.value_as_int;
                                            int hash = GetHashCode(str);
                                            Gui14825Hashcode.value = hash.ToString();
                                            _leaveRepository.UpdateLeaves(Gui14825Hashcode);
                                            if (WPleistVTemp.value_as_int == Grad && RefKlimazone14825.value_as_int == typeClimat && leavesIdWithOldDataList.Count -1 != actuelIndexLeaveIdInList)
                                            {
                                                bigHash += hash + "#";
                                            }
                                            else
                                            {
                                                UpdateBigHash2(leavesIdWithOldDataList.Count, actuelIndexLeaveIdInList, wpId, ref Grad, ref typeClimat, hash.ToString(), ref bigHash, (int)WPleistVTemp.value_as_int, (int)RefKlimazone14825.value_as_int);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var WPleistVTemp2 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1011);  //Находим температуру внутри
                                        var RefKlimazone148252 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1356); //находим тип климата
                                        var Gui14825Hashcode2 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1368);
                                        if (Gui14825Hashcode2 != null)
                                        {
                                            if (WPleistVTemp2.value_as_int == Grad && RefKlimazone148252.value_as_int == typeClimat && leavesIdWithOldDataList.Count -1   != actuelIndexLeaveIdInList)
                                            {
                                                bigHash += Gui14825Hashcode2.value + "#";
                                            }
                                            else
                                            {
                                                UpdateBigHash2(leavesIdWithOldDataList.Count, actuelIndexLeaveIdInList, wpId, ref Grad, ref typeClimat, Gui14825Hashcode2.value, ref bigHash, (int)WPleistVTemp.value_as_int, (int)RefKlimazone14825.value_as_int);
                                            }
                                        }


                                    }

                                }
                            }
                            //Если нет даных с такой температурой на улице в модели которую мы получили после конвертации и стандартизации
                            else
                            {
                                var WPleistVTemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1011);  //Находим температуру внутри
                                var RefKlimazone14825 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1356); //находим тип климата
                                var Gui14825Hashcode = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1368);
                                if (WPleistVTemp.value_as_int == Grad && RefKlimazone14825.value_as_int == typeClimat && leavesIdWithOldDataList.Count -1 != actuelIndexLeaveIdInList)
                                {
                                    bigHash += Gui14825Hashcode.value + "#";
                                }
                                else
                                {
                                    UpdateBigHash2(leavesIdWithOldDataList.Count, actuelIndexLeaveIdInList, wpId, ref Grad, ref typeClimat, Gui14825Hashcode.value, ref bigHash,  (int)WPleistVTemp.value_as_int, (int)RefKlimazone14825.value_as_int);
                                }


                            }
                        }
                        actuelIndexLeaveIdInList++;
                    }
                    
                    Console.WriteLine("Pump -" + wp.value + "  Update!");
                    Console.WriteLine();
                    Console.WriteLine();
                    Task.Delay(5000).Wait();
                }
            }
        }
        //Обновляем большой хэш и переключаемся на следущую температуру и климат
        private void UpdateBigHash2(int leavesIdCount, int actuelIndexLeaveIdInList, int wpId, ref int Grad, ref int typeClimat, string hash, ref string bigHash, int gradInLeave, int typeClimatInLeaves)
        {
            if (leavesIdCount-1 == actuelIndexLeaveIdInList)
            {
                bigHash += hash + "#";

            }
            if (Grad == 35 && typeClimat == 1)
            {
                if (bigHash.Count() >= 150)
                {
                    var bigHashDB = _leaveRepository.GetBigHashFor35GradForKaltesKlimaByWpId(wpId);
                    if (bigHashDB != null)
                    {
                        bigHashDB.value = bigHash;
                        if (_leaveRepository.UpdateLeaves(bigHashDB))
                        {
                            Console.WriteLine("------Up Big Hash For 35 Grad And Cold");
                        }
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }

                }
            }
            else if (Grad == 55 && typeClimat == 1)
            {
                if (bigHash.Count() >= 150)
                {
                    var bigHashDB = _leaveRepository.GetBigHashFor55GradForKaltesKlimaByWpId(wpId);
                    if (bigHashDB != null)
                    {
                        bigHashDB.value = bigHash;
                        if (_leaveRepository.UpdateLeaves(bigHashDB))
                        {
                            Console.WriteLine("------Up Big Hash For 55 Grad And Cold");
                        }
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }
                }
            }
            else if (Grad == 35 && typeClimat == 2)
            {
                if (bigHash.Count() >= 150)
                {
                    var bigHashDB = _leaveRepository.GetBigHashFor35GradForMittelKlimaByWpId(wpId);
                    if (bigHashDB != null)
                    {
                        bigHashDB.value = bigHash;
                        if (_leaveRepository.UpdateLeaves(bigHashDB))
                        {
                            Console.WriteLine("------Up Big Hash For 35 Grad And Mid");
                        }
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }
                }
            }
            else if (Grad == 55 && typeClimat == 2)
            {
                if (bigHash.Count() >= 150)
                {
                    var bigHashDB = _leaveRepository.GetBigHashFor55GradForMittelKlimaByWpId(wpId);
                    if (bigHashDB != null)
                    {
                        bigHashDB.value = bigHash;
                        if (_leaveRepository.UpdateLeaves(bigHashDB))
                        {
                            Console.WriteLine("------Up Big Hash For 55 Grad And Mid");
                        }
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }
                }

            }
            Grad = gradInLeave;
            typeClimat = typeClimatInLeaves;
            bigHash = "" + hash + "#";
        }
        public void GoalLogic(StandartPump pump)
        {
            var wpList = _leaveRepository.FindLeaveByNamePump(pump.Name); // находим  список насосов где есть такое имя            

            foreach(var wp in wpList)
            {
                bool[] upData = { false, false, false, false};
                var typeData = 0;
                if (wp != null)
                {
                    int typeClimat = 1;
                    int Grad = 35;
                    string bigHash = "";
                    var wpId = wp.nodeid_fk_nodes_nodeid; //находим его айди
                    var Idnid = wpId + 1;
                    if (wpId == 139078)
                        Idnid = 140565;
                    else
                        while (_leaveRepository.GetCountLeavesById(Idnid) != 6)
                        {
                            Idnid++;
                        }
                    
                    var a = true;
                    //while ((_leaveRepository.GetCountLeavesById(Idnid) == 6 || _leaveRepository.GetCountLeavesById(Idnid+1) == 6 || _leaveRepository.GetCountLeavesById(Idnid)==0)
                    //    && !upData[0] && !upData[1] && !upData[2] && !upData[3]) // Всегда 6 записей в которых храняться данные 
                    while (a) 
                    {
                        var dataWp = _leaveRepository.GetLeavesById(Idnid);
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
                                            _leaveRepository.UpdateLeaves(WPleistHeiz);
                                            _leaveRepository.UpdateLeaves(WPleistCOP);
                                            var str = WPleistATempValue + "#" + WPleistHeiz.value_as_int + "#" + WPleistCOP.value_as_int;
                                            //str = numForHash+ "#" + str;
                                            int hash = GetHashCode(str);
                                            Gui14825Hashcode.value = hash.ToString();
                                            _leaveRepository.UpdateLeaves(Gui14825Hashcode);
                                            if (WPleistVTemp.value_as_int == Grad && RefKlimazone14825.value_as_int == typeClimat && _leaveRepository.GetCountLeavesById(Idnid + 1) == 6)
                                            {
                                                bigHash += hash + "#";
                                            }
                                            else
                                            {
                                                UpdateBigHash(Idnid, wpId, ref Grad, ref typeClimat, hash.ToString(), ref bigHash, ref upData, (int)WPleistVTemp.value_as_int, (int)RefKlimazone14825.value_as_int);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var WPleistVTemp2 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1011);  //Находим температуру внутри
                                        var RefKlimazone148252 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1356); //находим тип климата
                                        var Gui14825Hashcode2 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1368);
                                        if(Gui14825Hashcode2 != null)
                                        {
                                            if (WPleistVTemp2.value_as_int == Grad && RefKlimazone148252.value_as_int == typeClimat && _leaveRepository.GetCountLeavesById(Idnid + 1) == 6)
                                            {
                                                bigHash += Gui14825Hashcode2.value + "#";
                                            }
                                            else
                                            {
                                                UpdateBigHash(Idnid, wpId, ref Grad, ref typeClimat, Gui14825Hashcode2.value, ref bigHash, ref upData, (int)WPleistVTemp.value_as_int, (int)RefKlimazone14825.value_as_int);
                                            }
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
                                if (WPleistVTemp.value_as_int == Grad && RefKlimazone14825.value_as_int == typeClimat && _leaveRepository.GetCountLeavesById(Idnid + 1) == 6)
                                {
                                    bigHash += Gui14825Hashcode.value + "#";
                                }
                                else
                                {
                                    UpdateBigHash(Idnid, wpId, ref Grad, ref typeClimat, Gui14825Hashcode.value, ref bigHash, ref upData, (int)WPleistVTemp.value_as_int, (int)RefKlimazone14825.value_as_int);
                                }

                                
                            }
                        }
                        Idnid++;
                        if (Idnid == 136713 && wp.value == "YKF07CNC")
                            Idnid = 140643;
                        if (upData[0] && upData[1] && upData[2] && upData[3])
                        {
                            a = false;
                        }
                        //numForHash++;
                    }
                    
                    Console.WriteLine("Pump -" + wp.value + "  Update!");
                    Console.WriteLine();
                    Console.WriteLine();
                    Task.Delay(5000).Wait();
                }
            }
            
        }

        private void ChangeDataForSendToDB(ref int typeData, Leave WPleistHeiz, Leave WPleistCOP, StandartDataPump dataPumpForThisData)
        {
            switch (typeData)
            {
                case 0:
                    WPleistHeiz.value_as_int = 600;//(int)(dataPumpForThisData.MinHC * 100);
                    WPleistCOP.value_as_int = 600;//(int)(dataPumpForThisData.MinCOP * 100);
                    typeData++;
                    break;
                case 1:
                    WPleistHeiz.value_as_int = 600;// (int)(dataPumpForThisData.MidHC * 100);
                    WPleistCOP.value_as_int = 600; //(int)(dataPumpForThisData.MidCOP * 100);
                    typeData++;
                    break;
                case 2:
                    WPleistHeiz.value_as_int = 600; //(int)(dataPumpForThisData.MaxHC * 100);
                    WPleistCOP.value_as_int = 600;//(int)(dataPumpForThisData.MaxCOP * 100);
                    typeData = 0;
                    break;
                default:
                    break;
            }
        }

        //Обновляем большой хэш и переключаемся на следущую температуру и климат
        private void UpdateBigHash(int Idnid, int wpId, ref int Grad, ref int typeClimat, string hash, ref string bigHash, ref bool[] upBigHash, int gradInLeave, int typeClimatInLeaves)
        {
            if (_leaveRepository.GetCountLeavesById(Idnid + 1)!=6)
            {
                bigHash += hash + "#";

            }
            if (Grad == 35 && typeClimat == 1)
            {   if(bigHash.Count() >= 150)
                {
                    var bigHashDB = _leaveRepository.GetBigHashFor35GradForKaltesKlimaByWpId(wpId);
                    if(bigHashDB != null && !upBigHash[0])
                    {
                        bigHashDB.value = bigHash;
                        if (_leaveRepository.UpdateLeaves(bigHashDB))
                        {
                            Console.WriteLine("------Up Big Hash For 35 Grad And Cold");
                            upBigHash[0] = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }
                   
                }                
                //Grad = 55;
            }
            else if (Grad == 55 && typeClimat == 1)
            {
                if (bigHash.Count() >= 150)
                {
                    var bigHashDB = _leaveRepository.GetBigHashFor55GradForKaltesKlimaByWpId(wpId);
                    if (bigHashDB != null && !upBigHash[1])
                    {
                        bigHashDB.value = bigHash;
                        if (_leaveRepository.UpdateLeaves(bigHashDB))
                        {
                            Console.WriteLine("------Up Big Hash For 55 Grad And Cold");
                            upBigHash[1] = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }
                }
                //Grad = 35;
                //typeClimat = 2;
            }
            else if (Grad == 35 && typeClimat == 2)
            {
                if (bigHash.Count() >= 150)
                {
                    var bigHashDB = _leaveRepository.GetBigHashFor35GradForMittelKlimaByWpId(wpId);
                    if (bigHashDB != null && !upBigHash[2])
                    {
                        bigHashDB.value = bigHash;
                        if (_leaveRepository.UpdateLeaves(bigHashDB))
                        {
                            Console.WriteLine("------Up Big Hash For 35 Grad And Mid");
                            upBigHash[2] = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }
                }
                //Grad = 55;
            }
            else if (Grad == 55 && typeClimat == 2)
            {
                if (bigHash.Count() >= 150)
                {
                    var bigHashDB = _leaveRepository.GetBigHashFor55GradForMittelKlimaByWpId(wpId);
                    if (bigHashDB != null && !upBigHash[3])
                    {
                        bigHashDB.value = bigHash;
                        if (_leaveRepository.UpdateLeaves(bigHashDB))
                        {
                            Console.WriteLine("------Up Big Hash For 55 Grad And Mid");
                            upBigHash[3] = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("------Dont have node in DB, BigHash == null");
                    }
                }
                //Grad = 35;
                //typeClimat = 1;
            }
            Grad = gradInLeave;
            typeClimat = typeClimatInLeaves;
            if (_leaveRepository.GetCountLeavesById(Idnid + 1) == 6)
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
