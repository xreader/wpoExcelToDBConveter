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
                    int gradInseide = 35;
                    string bigHash = "";
                    var wpId = wp.nodeid_fk_nodes_nodeid;
                    var leavesIdWithOldDataList = _nodeRepository.GetIdLeavesWithDataByPumpId(wpId);//список IdLeaves которые надо менять
                    var actuelIndexLeaveIdInList = 0;
                    foreach (var leaveIdWithOldData in leavesIdWithOldDataList)
                    {
                        var dataWp = _leaveRepository.GetLeavesById(leaveIdWithOldData);
                        var WPleistATemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1351).value_as_int;              //Находим значение температуры на улице
                        var WPleistVTemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1011).value_as_int;              //Находим значение температуры внутри
                        var RefKlimazone14825 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1356).value_as_int;         //Находим значение типа климата
                        var Gui14825Hashcode = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1368);                       //Находим leave с хєшкодом
                        if (WPleistATemp != null)
                        {
                            //Если есть даные с такой температурой на улице в модели которую мы получили после конвертации и стандартизации
                            if (pump.Data.TryGetValue((int)WPleistATemp, out var myPumpData))
                            {                                
                                if (WPleistVTemp != null && RefKlimazone14825 != null)
                                {
                                    //получаем даныне из стандартизованой модели с нужным климатом и температурой
                                    var dataPumpForThisData = myPumpData.FirstOrDefault(x => x.ForTemp == WPleistVTemp && x.Climate == RefKlimazone14825.ToString());
                                    if (dataPumpForThisData != null)
                                    {
                                        var WPleistHeiz = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1012); //leave с данными для P
                                        var WPleistCOP = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1221); // leave c данными для COP
                                        if (WPleistHeiz != null && WPleistCOP != null && Gui14825Hashcode != null)
                                        { //Меняем даныне для P и COP 
                                            ChangeDataForSendToDB(ref typeData, WPleistHeiz, WPleistCOP, dataPumpForThisData);
                                            _leaveRepository.UpdateLeaves(WPleistHeiz);
                                            _leaveRepository.UpdateLeaves(WPleistCOP);
                                            //формируем хэш и обновляем
                                            var str = WPleistATemp + "#" + WPleistHeiz.value_as_int + "#" + WPleistCOP.value_as_int;
                                            int hash = GetHashCode(str);
                                            Gui14825Hashcode.value = hash.ToString();
                                            _leaveRepository.UpdateLeaves(Gui14825Hashcode);                                            
                                        }
                                    }                                    
                                }
                            }
                            //Создание длиного хєша и его отправка при заполнении
                            if (WPleistVTemp == gradInseide && RefKlimazone14825 == typeClimat && leavesIdWithOldDataList.Count - 1 != actuelIndexLeaveIdInList)
                                bigHash += Gui14825Hashcode.value + "#";
                            else
                                UpdateBigHash(leavesIdWithOldDataList.Count, actuelIndexLeaveIdInList, wpId, ref gradInseide, ref typeClimat, Gui14825Hashcode.value, ref bigHash, (int)WPleistVTemp, (int)RefKlimazone14825);

                        }
                        actuelIndexLeaveIdInList++;
                    }
                    
                    Console.WriteLine("Pump -" + wp.value + "  Update!");
                    Console.WriteLine();
                    Console.WriteLine();
                    Task.Delay(2000).Wait();
                }
            }
        }
        //Метод для обновления длиного хэша и переключения на другой климат и температуру
        private void UpdateBigHash(int leavesIdCount, int actuelIndexLeaveIdInList, int wpId, ref int gradInseide, ref int typeClimat, string hash, ref string bigHash, int gradInseideInLeave, int typeClimatInLeaves)
        {
            if (leavesIdCount-1 == actuelIndexLeaveIdInList)
                bigHash += hash + "#";

            var bigHashDB = GetBigHashDB(wpId, gradInseide, typeClimat);
            if (bigHash.Count() >= 150 && bigHashDB != null)
            {
                bigHashDB.value = bigHash;
                if (_leaveRepository.UpdateLeaves(bigHashDB))
                {
                    Console.WriteLine($"------Up Big Hash For {gradInseide} gradInseide And {(typeClimat == 1 ? "Cold" : "Mid")}");
                }
                else
                {
                    Console.WriteLine("------Dont have node in DB, BigHash == null");
                }
            }

            gradInseide = gradInseideInLeave;
            typeClimat = typeClimatInLeaves;
            bigHash = "" + hash + "#";
        }

        //Метод для получения длиного хэша с базы данных
        private Leave GetBigHashDB(int wpId, int gradInseide, int typeClimat)
        {
            switch (gradInseide)
            {
                case 35:
                    return typeClimat == 1 ? _leaveRepository.GetBigHashFor35GradForKaltesKlimaByWpId(wpId)    //если холодный климат
                                            : _leaveRepository.GetBigHashFor35GradForMittelKlimaByWpId(wpId);  //если средний климат
                case 55:
                    return typeClimat == 1 ? _leaveRepository.GetBigHashFor55GradForKaltesKlimaByWpId(wpId)    //если холодный климат
                                            : _leaveRepository.GetBigHashFor55GradForMittelKlimaByWpId(wpId);  //если средний климат
                default:
                    return null;
            }
        }

        //Метод для изменения данных в модели перед отправкой в базу
        private void ChangeDataForSendToDB(ref int typeData, Leave WPleistHeiz, Leave WPleistCOP, StandartDataPump dataPumpForThisData)
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
        //Метод для хэширования строки с переносом в 5 бит
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
