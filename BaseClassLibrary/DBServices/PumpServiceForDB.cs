using BaseClassLibrary.Models;
using BaseClassLibrary.StandartModels;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.DBConnection;
using TestExel.DBModels;
using TestExel.Models;
using TestExel.Repository;
using TestExel.StandartModels;

namespace TestExel.ServicesForDB
{
    public abstract class PumpServiceForDB
    {
        protected readonly LeaveRepository _leaveRepository;
        protected readonly NodeRepository _nodeRepository;
        protected readonly TextRepository _textRepository;
        public PumpServiceForDB(string pathDB)
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseSqlite("Data Source=" + pathDB + ";")
                .Options;
            _leaveRepository = new LeaveRepository(new ApplicationDBContext(options));
            _nodeRepository = new NodeRepository(new ApplicationDBContext(options));
            _textRepository = new TextRepository(new ApplicationDBContext(options));
        }

        public async Task ChangeDataenEN14825LGInDbByExcelData(StandartPump pump, string typePump, int idCompany, int numClimat)
        {
            var wpList = await GetWPList(pump.Name, typePump, idCompany);
            foreach (var wp in wpList)
            {
                var typeData = 0;
                if (wp != null)
                {
                    int typeClimat = 1;
                    int gradInseide = 35;
                    string bigHash = "";
                    var wpId = wp.nodeid_fk_nodes_nodeid;
                    var leavesIdWithOldDataList = await _nodeRepository.GetIdLeavesWithDataByPumpId(wpId);//list of IdLeaves that need to be changed
                    if (leavesIdWithOldDataList.Count > 0)
                    {
                        var actuelIndexLeaveIdInList = 0;
                        foreach (var leaveIdWithOldData in leavesIdWithOldDataList)
                        {
                            var dataWp = await _leaveRepository.GetLeavesById(leaveIdWithOldData);
                            var WPleistATemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1351).value_as_int;              //Finding the temperature outside
                            var WPleistVTemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1011).value_as_int;              //Finding the temperature inside
                            var RefKlimazone14825 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1356).value_as_int;         //Finding the climate type value
                            var Gui14825Hashcode = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1368);                       //Find leave with hashcode
                            if (WPleistATemp != null)
                            {
                                //If there is data with such an outdoor temperature in the model that we received after conversion and standardization
                                if (pump.Data.TryGetValue((int)WPleistATemp, out var myPumpData))
                                {
                                    if (WPleistVTemp != null && RefKlimazone14825 != null)
                                    {
                                        //we obtain data from a standardized model with the desired climate and temperature
                                        var dataPumpForThisData = myPumpData.FirstOrDefault(x => x.ForTemp == WPleistVTemp && x.Climate == RefKlimazone14825.ToString());
                                        if (dataPumpForThisData != null)
                                        {
                                            var WPleistHeiz = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1012); //leave with data for P
                                            var WPleistCOP = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1221);  //leave with data for COP
                                            if (WPleistHeiz != null && WPleistCOP != null && Gui14825Hashcode != null)
                                            { //Changing data for P and COP
                                                if (WPleistVTemp != gradInseide && RefKlimazone14825 != typeClimat)
                                                    typeData = 0;
                                                ChangeDataForSendToDB(ref typeData, WPleistHeiz, WPleistCOP, dataPumpForThisData);
                                                await _leaveRepository.UpdateLeaves(WPleistHeiz);
                                                await _leaveRepository.UpdateLeaves(WPleistCOP);
                                                //form a hash and update
                                                var str = WPleistATemp + "#" + WPleistHeiz.value_as_int + "#" + WPleistCOP.value_as_int;
                                                int hash = GetHashCode(str);
                                                Gui14825Hashcode.value = hash.ToString();
                                                await _leaveRepository.UpdateLeaves(Gui14825Hashcode);
                                            }
                                        }
                                        else
                                            Console.WriteLine("Data for " + WPleistVTemp + " And " + RefKlimazone14825 + " for pump " + pump.Name + " DONT UPDATE, BECOUSE DONT HAVE DATA!");
                                    }
                                }
                                else
                                    Console.WriteLine("Data for " + WPleistVTemp + " And " + RefKlimazone14825 + " for pump " + pump.Name + " DONT UPDATE, BECOUSE DONT HAVE DATA!");
                                //Create a long hash and send it when filled
                                if (WPleistVTemp == gradInseide && RefKlimazone14825 == typeClimat && leavesIdWithOldDataList.Count - 1 != actuelIndexLeaveIdInList)
                                    bigHash += Gui14825Hashcode.value + "#";
                                else
                                {
                                    var changeValue = await UpdateBigHash(leavesIdWithOldDataList.Count, actuelIndexLeaveIdInList, wpId, gradInseide, typeClimat, Gui14825Hashcode.value, bigHash, (int)WPleistVTemp, (int)RefKlimazone14825);
                                    gradInseide = changeValue.Item1;
                                    typeClimat = changeValue.Item2;
                                    bigHash = changeValue.Item3;
                                }

                            }
                            actuelIndexLeaveIdInList++;
                        }

                    }
                    else
                    {
                        while (typeClimat <= numClimat)
                        {
                            var dataForActuelClimat35Grad = pump.Data
                                                                .Where(pair => pair.Value.Any(data => data.Climate == typeClimat.ToString() && data.ForTemp == 35))
                                                                .OrderBy(pair => pair.Key)
                                                                .ToDictionary(pair => pair.Key, pair => pair.Value.Where(data => data.Climate == typeClimat.ToString() && data.ForTemp == 35).ToList());

                            var dataForActuelClimat55Grad = pump.Data
                                                                .Where(pair => pair.Value.Any(data => data.Climate == typeClimat.ToString() && data.ForTemp == 55))
                                                                .OrderBy(pair => pair.Key)
                                                                .ToDictionary(pair => pair.Key, pair => pair.Value.Where(data => data.Climate == typeClimat.ToString() && data.ForTemp == 55).ToList());

                            await CreateNew14825Data(dataForActuelClimat35Grad, typeClimat, wpId);
                            await CreateNew14825Data(dataForActuelClimat55Grad, typeClimat, wpId);
                            typeClimat++;
                        }
                        typeClimat = 1;
                    }
                    Console.WriteLine("Pump -" + wp.value + "  Update!");
                    Console.WriteLine();
                    Console.WriteLine();
                    Task.Delay(1000).Wait();
                }
            }
        }
        public async Task UnregulatedChangeDataenEN14825LGInDbByExcelData(UnregulatedStandartPump pump, string typePump, int idCompany, int numClimat)
        {
            var wpList = await UnregulatedGetWPList(pump.Name, typePump, idCompany);
            foreach (var wp in wpList)
            {
                var typeData = 0;
                if (wp != null)
                {
                    int typeClimat = 1;
                    int gradInseide = 35;
                    string bigHash = "";
                    var wpId = wp.nodeid_fk_nodes_nodeid;
                    var leavesIdWithOldDataList = await _nodeRepository.UnregularedGetIdLeavesWithDataByPumpId(wpId);//list of IdLeaves that need to be changed
                    if (leavesIdWithOldDataList.Count > 0)
                    {
                        var actuelIndexLeaveIdInList = 0;
                        foreach (var leaveIdWithOldData in leavesIdWithOldDataList)
                        {
                            var dataWp = await _leaveRepository.GetLeavesById(leaveIdWithOldData);
                            var WPleistATemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1351).value_as_int;              //Finding the temperature outside
                            var WPleistVTemp = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1011).value_as_int;              //Finding the temperature inside
                            var RefKlimazone14825 = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1356).value_as_int;         //Finding the climate type value
                            if (WPleistATemp != null)
                            {
                                //If there is data with such an outdoor temperature in the model that we received after conversion and standardization
                                if (pump.Data.TryGetValue((int)WPleistATemp, out var myPumpData))
                                {
                                    if (WPleistVTemp != null && RefKlimazone14825 != null)
                                    {
                                        //we obtain data from a standardized model with the desired climate and temperature
                                        var dataPumpForThisData = myPumpData.FirstOrDefault(x => x.ForTemp == WPleistVTemp && x.Climate == RefKlimazone14825.ToString());
                                        if (dataPumpForThisData != null)
                                        {
                                            var WPleistHeiz = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1012); //leave with data for P
                                            var WPleistCOP = dataWp.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1221);  //leave with data for COP
                                            if (WPleistHeiz != null && WPleistCOP != null)
                                            { //Changing data for P and COP
                                                
                                                UnregulatedChangeDataForSendToDB(WPleistHeiz, WPleistCOP, dataPumpForThisData);
                                                await _leaveRepository.UpdateLeaves(WPleistHeiz);
                                                await _leaveRepository.UpdateLeaves(WPleistCOP);
                                            }
                                        }
                                        else
                                            Console.WriteLine("Data for " + WPleistVTemp + " And " + RefKlimazone14825 + " for pump " + pump.Name + " DONT UPDATE, BECOUSE DONT HAVE DATA!");
                                    }
                                }
                                else
                                    Console.WriteLine("Data for " + WPleistVTemp + " And " + RefKlimazone14825 + " for pump " + pump.Name + " DONT UPDATE, BECOUSE DONT HAVE DATA!");
                            }
                            actuelIndexLeaveIdInList++;
                        }

                    }
                    else
                    {
                        while (typeClimat <= numClimat)
                        {
                            var dataForActuelClimat35Grad = pump.Data
                                                                .Where(pair => pair.Value.Any(data => data.Climate == typeClimat.ToString() && data.ForTemp == 35))
                                                                .OrderBy(pair => pair.Key)
                                                                .ToDictionary(pair => pair.Key, pair => pair.Value.Where(data => data.Climate == typeClimat.ToString() && data.ForTemp == 35).ToList());

                            var dataForActuelClimat55Grad = pump.Data
                                                                .Where(pair => pair.Value.Any(data => data.Climate == typeClimat.ToString() && data.ForTemp == 55))
                                                                .OrderBy(pair => pair.Key)
                                                                .ToDictionary(pair => pair.Key, pair => pair.Value.Where(data => data.Climate == typeClimat.ToString() && data.ForTemp == 55).ToList());

                            await UnregulatedCreateNew14825Data(dataForActuelClimat35Grad, typeClimat, wpId);
                            await UnregulatedCreateNew14825Data(dataForActuelClimat55Grad, typeClimat, wpId);
                            typeClimat++;
                        }
                        typeClimat = 1;
                    }
                    Console.WriteLine("Pump -" + wp.value + "  Update!");
                    Console.WriteLine();
                    Console.WriteLine();
                    Task.Delay(1000).Wait();
                }
            }
        }

        //Update/Create in DB this data  Leistung
        public virtual async Task ChangeLeistungsdatenInDbByExcelData(Pump pump, string typePump, int idCompany)
        {
            var wpList = await GetWPList(pump.Name, typePump, idCompany);
            foreach (var wp in wpList)
            {
                var wpId = wp.nodeid_fk_nodes_nodeid;
                //Get all leave Id in db for this WP 
                var leavesIdWithOldLeistungdatenList = await _nodeRepository.GetIdLeavesWithLeistungsdatenByPumpId(wpId);//list of IdLeaves that need to be changed
                                                                                                                         //Get all leave in db for this WP                
                var listWithleavesWithListOldLeistungdaten = await _leaveRepository.GetLeavesByIdList(leavesIdWithOldLeistungdatenList);

                //We sort through the data we received from Excel
                foreach (var newDataDictionary in pump.Data)
                {
                    foreach (var newData in newDataDictionary.Value)
                    {
                        //We are looking for a list of records where there is data that needs to be changed and their quantity, if the number is more than 1, then we change the first one and delete the rest
                        var listWithLeavesForUpdate = listWithleavesWithListOldLeistungdaten
                                     .Where(list => list.Any(leave => leave.value_as_int == newDataDictionary.Key && leave.objectid_fk_properties_objectid == 1010))
                                     .Where(list => list.Any(leave => leave.value_as_int == newData.Temp && leave.objectid_fk_properties_objectid == 1011))
                                     .Where(list => list.Any(leave => leave.value_as_int == newData.MaxVorlauftemperatur && leave.objectid_fk_properties_objectid == 1015))
                                     .ToList();
                        //If there are such records, we simply update them and delete duplicates
                        if (listWithLeavesForUpdate.Count > 0)
                        {
                            //We take the first entry for updating; subsequent repeated ones must be deleted! and must be removed both from the database and from the list
                            var leavesForUpdate = listWithLeavesForUpdate[0];
                            //Finding the Heizleistung - P and Update
                            var WPleistHeiz = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1012);
                            WPleistHeiz.value_as_int = newData.MaxHC == 0 ? 0 : (int)(newData.MaxHC * 100);
                            await _leaveRepository.UpdateLeaves(WPleistHeiz);
                            //Finding the COP and Update
                            var WPleistCOP = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1221);
                            WPleistCOP.value_as_int = newData.MaxCOP == 0 ? 0 : (int)(newData.MaxCOP * 100);
                            await _leaveRepository.UpdateLeaves(WPleistCOP);
                            //Finding the Leistungsaufnahme and Update
                            var WPleistAuf = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1014);
                            WPleistAuf.value_as_int = newData.MaxCOP == 0 || newData.MaxHC == 0 ? 0 : (int)(newData.MaxHC / newData.MaxCOP * 100);
                            await _leaveRepository.UpdateLeaves(WPleistAuf);
                            //Finding the Kealteleistung and Update
                            var WPleistKaelte = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1013);
                            WPleistKaelte.value_as_int = newData.MaxCOP == 0 || newData.MaxHC == 0 ? 0 : (int)((newData.MaxHC - 0.96 * (newData.MaxHC / newData.MaxCOP)) * 100);
                            await _leaveRepository.UpdateLeaves(WPleistKaelte);


                            //We remove from the list with our data what we updated
                            listWithLeavesForUpdate.Remove(leavesForUpdate);
                            //Now the list contains only duplicate entries that should be removed from the database
                            await _leaveRepository.DeleteLeaves(listWithLeavesForUpdate);

                            //We remove from the list with all entries for this pump those entries that have just been updated 
                            listWithleavesWithListOldLeistungdaten.Remove(leavesForUpdate);

                            //We remove from the list with all the records for this pump those records that were deleted from the database, and also delete the connecting records from the database, that is, Node
                            foreach (var item in listWithLeavesForUpdate)
                            {
                                listWithleavesWithListOldLeistungdaten.Remove(item);
                                var node = await _nodeRepository.GetNodeByIdAsync(item[0].nodeid_fk_nodes_nodeid);
                                await _nodeRepository.DeleteNode(node);
                            }

                        }
                        //If there are no records with this temperature inside and the maximum temperature inside (you need to create a record)
                        else
                        {
                            //Create a linking record
                            Node node = new Node() { typeid_fk_types_typeid = 8, parentid_fk_nodes_nodeid = wpId, licence = 0 };
                            await _nodeRepository.CreateNode(node);
                            //We create all the necessary records containing new data
                            List<Leave> leaves = new List<Leave>
                            {
                                new Leave() { objectid_fk_properties_objectid = 1010, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newDataDictionary.Key },
                                new Leave() { objectid_fk_properties_objectid = 1011, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.Temp },
                                new Leave() { objectid_fk_properties_objectid = 1012, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MaxHC == 0 ? 0 : (int)(newData.MaxHC * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1013, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MaxCOP == 0 || newData.MaxHC == 0 ? 0 : (int)((newData.MaxHC - 0.96 * (newData.MaxHC / newData.MaxCOP)) * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1014, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MaxCOP == 0 || newData.MaxHC == 0 ? 0 : (int)(newData.MaxHC / newData.MaxCOP * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1015, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MaxVorlauftemperatur },
                                new Leave() { objectid_fk_properties_objectid = 1221, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MaxCOP == 0 ? 0 :(int)(newData.MaxCOP * 100) }
                            };
                            //Add them to the database
                            foreach (var leave in leaves)
                            {
                                await _leaveRepository.CreateLeave(leave);
                            }

                        }
                    }


                }
                //Deletes data that is not in the Excel file
                //{
                //    await _leaveRepository.DeleteLeaves(listWithleavesWithListOldLeistungdaten);
                //    foreach (var item in listWithleavesWithListOldLeistungdaten)
                //    {
                //        var node = await _nodeRepository.GetNodeByIdAsync(item[0].nodeid_fk_nodes_nodeid);
                //        await _nodeRepository.DeleteNode(node);
                //    }

                //}


                Console.WriteLine("Pump -" + wp.value + " Leistungdata Update!");
            }

        }
        //Update/Create in DB this data  Leistung
        public virtual async Task UnregulatedChangeLeistungsdatenInDbByExcelData(UnregulatedPump pump, string typePump, int idCompany)
        {
            var wpList = await UnregulatedGetWPList(pump.Name, typePump, idCompany);
            foreach (var wp in wpList)
            {
                var wpId = wp.nodeid_fk_nodes_nodeid;
                //Get all leave Id in db for this WP 
                var leavesIdWithOldLeistungdatenList = await _nodeRepository.GetIdLeavesWithLeistungsdatenByPumpId(wpId);//list of IdLeaves that need to be changed
                                                                                                                         //Get all leave in db for this WP                
                var listWithleavesWithListOldLeistungdaten = await _leaveRepository.GetLeavesByIdList(leavesIdWithOldLeistungdatenList);

                //We sort through the data we received from Excel
                foreach (var newDataDictionary in pump.Data)
                {
                    foreach (var newData in newDataDictionary.Value)
                    {
                        //We are looking for a list of records where there is data that needs to be changed and their quantity, if the number is more than 1, then we change the first one and delete the rest
                        var listWithLeavesForUpdate = listWithleavesWithListOldLeistungdaten
                                     .Where(list => list.Any(leave => leave.value_as_int == newDataDictionary.Key && leave.objectid_fk_properties_objectid == 1010))
                                     .Where(list => list.Any(leave => leave.value_as_int == newData.Temp && leave.objectid_fk_properties_objectid == 1011))
                                     .Where(list => list.Any(leave => leave.value_as_int == newData.MaxVorlauftemperatur && leave.objectid_fk_properties_objectid == 1015))
                                     .ToList();
                        //If there are such records, we simply update them and delete duplicates
                        if (listWithLeavesForUpdate.Count > 0)
                        {
                            Console.WriteLine("Update Leistungdaten");
                            //We take the first entry for updating; subsequent repeated ones must be deleted! and must be removed both from the database and from the list
                            var leavesForUpdate = listWithLeavesForUpdate[0];
                            //Finding the Heizleistung - P and Update
                            var WPleistHeiz = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1012);
                            WPleistHeiz.value_as_int = newData.HC == 0 ? 0 : (int)(newData.HC * 100);
                            await _leaveRepository.UpdateLeaves(WPleistHeiz);
                            //Finding the COP and Update
                            var WPleistCOP = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1221);
                            WPleistCOP.value_as_int = newData.HC == 0 ? 0 : (int)(newData.HC * 100);
                            await _leaveRepository.UpdateLeaves(WPleistCOP);
                            //Finding the Leistungsaufnahme and Update
                            var WPleistAuf = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1014);
                            WPleistAuf.value_as_int = newData.COP == 0 || newData.HC == 0 ? 0 : (int)(newData.HC / newData.COP * 100);
                            await _leaveRepository.UpdateLeaves(WPleistAuf);
                            //Finding the Kealteleistung and Update
                            var WPleistKaelte = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1013);
                            WPleistKaelte.value_as_int = newData.COP == 0 || newData.HC == 0 ? 0 : (int)((newData.HC - 0.96 * (newData.HC / newData.COP)) * 100);
                            await _leaveRepository.UpdateLeaves(WPleistKaelte);


                            //We remove from the list with our data what we updated
                            listWithLeavesForUpdate.Remove(leavesForUpdate);
                            //Now the list contains only duplicate entries that should be removed from the database
                            await _leaveRepository.DeleteLeaves(listWithLeavesForUpdate);

                            //We remove from the list with all entries for this pump those entries that have just been updated 
                            listWithleavesWithListOldLeistungdaten.Remove(leavesForUpdate);

                            //We remove from the list with all the records for this pump those records that were deleted from the database, and also delete the connecting records from the database, that is, Node
                            foreach (var item in listWithLeavesForUpdate)
                            {
                                listWithleavesWithListOldLeistungdaten.Remove(item);
                                var node = await _nodeRepository.GetNodeByIdAsync(item[0].nodeid_fk_nodes_nodeid);
                                await _nodeRepository.DeleteNode(node);
                            }

                        }
                        //If there are no records with this temperature inside and the maximum temperature inside (you need to create a record)
                        else
                        {
                            //Create a linking record
                            Node node = new Node() { typeid_fk_types_typeid = 8, parentid_fk_nodes_nodeid = wpId, licence = 0 };
                            await _nodeRepository.CreateNode(node);
                            //We create all the necessary records containing new data
                            List<Leave> leaves = new List<Leave>
                            {
                                new Leave() { objectid_fk_properties_objectid = 1010, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newDataDictionary.Key },
                                new Leave() { objectid_fk_properties_objectid = 1011, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.Temp },
                                new Leave() { objectid_fk_properties_objectid = 1012, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.HC == 0 ? 0 : (int)(newData.HC * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1013, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.COP == 0 || newData.HC == 0 ? 0 : (int)((newData.HC - 0.96 *(newData.HC / newData.COP)) * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1014, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.COP == 0 || newData.HC == 0 ? 0 : (int)(newData.HC / newData.COP * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1015, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MaxVorlauftemperatur },
                                new Leave() { objectid_fk_properties_objectid = 1221, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.COP == 0 ? 0 :(int)(newData.COP * 100) }
                            };
                            //Add them to the database
                            foreach (var leave in leaves)
                            {
                                await _leaveRepository.CreateLeave(leave);
                            }

                        }
                    }


                }
                //Deletes data that is not in the Excel file
                //{
                //    await _leaveRepository.DeleteLeaves(listWithleavesWithListOldLeistungdaten);
                //    foreach (var item in listWithleavesWithListOldLeistungdaten)
                //    {
                //        var node = await _nodeRepository.GetNodeByIdAsync(item[0].nodeid_fk_nodes_nodeid);
                //        await _nodeRepository.DeleteNode(node);
                //    }

                //}


                Console.WriteLine("Pump -" + wp.value + " Leistungdata Update!");
            }

        }

        protected virtual async Task CreateNew14825Data(Dictionary<int, List<StandartDataPump>> dataDictionary, int typeClimat, int wpId)
        {
            string bigHash = "";
            int forTemp = dataDictionary.Values.First().First().ForTemp;
            foreach (var data in dataDictionary)
            {
                foreach (var dataValue in data.Value)
                {
                    bigHash += await Create14825ForSelectedData(wpId, data.Key, typeClimat, dataValue.ForTemp, dataValue.MinHC, dataValue.MinCOP);
                    bigHash += await Create14825ForSelectedData(wpId, data.Key, typeClimat, dataValue.ForTemp, dataValue.MidHC, dataValue.MidCOP);
                    bigHash += await Create14825ForSelectedData(wpId, data.Key, typeClimat, dataValue.ForTemp, dataValue.MaxHC, dataValue.MaxCOP);
                }

            }
            switch (typeClimat)
            {
                case 1:
                    var neuLeaveCold = new Leave()
                    {
                        objectid_fk_properties_objectid = forTemp == 35 ? 1464 : 1466,
                        nodeid_fk_nodes_nodeid = wpId,
                        value = bigHash,
                        value_as_int = 0
                    };
                    await _leaveRepository.CreateLeave(neuLeaveCold);
                    break;
                case 2:
                    var neuLeaveMid = new Leave()
                    {
                        objectid_fk_properties_objectid = forTemp == 35 ? 1364 : 1366,
                        nodeid_fk_nodes_nodeid = wpId,
                        value = bigHash,
                        value_as_int = 0
                    };
                    await _leaveRepository.CreateLeave(neuLeaveMid);

                    break;
                case 3:
                    var neuLeaveWarm = new Leave()
                    {
                        objectid_fk_properties_objectid = forTemp == 35 ? 1468 : 1470,
                        nodeid_fk_nodes_nodeid = wpId,
                        value = bigHash,
                        value_as_int = 0
                    };
                    await _leaveRepository.CreateLeave(neuLeaveWarm);

                    break;

            }

        }
        protected virtual async Task UnregulatedCreateNew14825Data(Dictionary<int, List<UnregulatedStandartDataPump>> dataDictionary, int typeClimat, int wpId)
        {
           
            int forTemp = dataDictionary.Values.First().First().ForTemp;
            foreach (var data in dataDictionary)
            {
                foreach (var dataValue in data.Value)
                {
                    await UnregulatedCreate14825ForSelectedData(wpId, data.Key, typeClimat, dataValue.ForTemp, dataValue.HC, dataValue.COP);                   
                }
            }            
        }

        protected async Task<string> Create14825ForSelectedData(int wpId, int tempOut, int typeClimat, int forTemp, double HC, double COP)
        {
            //form a hash and update
            var str = tempOut + "#" + HC + "#" + COP;
            int hash = GetHashCode(str);

            var nodeForThisData = new Node()
            {
                typeid_fk_types_typeid = 25,
                parentid_fk_nodes_nodeid = wpId,
                licence = 0
            };
            await _nodeRepository.CreateNode(nodeForThisData);
            var leavesForCreate = new List<Leave>()
                                {
                                    new Leave(){objectid_fk_properties_objectid = 1011, nodeid_fk_nodes_nodeid = nodeForThisData.nodeid, value ="", value_as_int = forTemp },
                                    new Leave(){objectid_fk_properties_objectid = 1012, nodeid_fk_nodes_nodeid = nodeForThisData.nodeid, value ="", value_as_int = (int)(HC * 100) },
                                    new Leave(){objectid_fk_properties_objectid = 1221, nodeid_fk_nodes_nodeid = nodeForThisData.nodeid, value ="", value_as_int = (int)(COP * 100)},
                                    new Leave(){objectid_fk_properties_objectid = 1351, nodeid_fk_nodes_nodeid = nodeForThisData.nodeid, value ="", value_as_int = tempOut },
                                    new Leave(){objectid_fk_properties_objectid = 1356, nodeid_fk_nodes_nodeid = nodeForThisData.nodeid, value ="", value_as_int = typeClimat },
                                    new Leave(){objectid_fk_properties_objectid = 1368, nodeid_fk_nodes_nodeid = nodeForThisData.nodeid, value = hash.ToString(), value_as_int = 0},

                                };
            //Add them to the database
            foreach (var leave in leavesForCreate)
            {
                await _leaveRepository.CreateLeave(leave);
            }
            return hash + "#";
        }
        protected async Task UnregulatedCreate14825ForSelectedData(int wpId, int tempOut, int typeClimat, int forTemp, double HC, double COP)
        {
            //form a hash and update            

            var nodeForThisData = new Node()
            {
                typeid_fk_types_typeid = 21,
                parentid_fk_nodes_nodeid = wpId,
                licence = 0
            };
            await _nodeRepository.CreateNode(nodeForThisData);
            var leavesForCreate = new List<Leave>()
                                {
                                    new Leave(){objectid_fk_properties_objectid = 1011, nodeid_fk_nodes_nodeid = nodeForThisData.nodeid, value ="", value_as_int = forTemp },
                                    new Leave(){objectid_fk_properties_objectid = 1012, nodeid_fk_nodes_nodeid = nodeForThisData.nodeid, value ="", value_as_int = (int)(HC * 100) },
                                    new Leave(){objectid_fk_properties_objectid = 1221, nodeid_fk_nodes_nodeid = nodeForThisData.nodeid, value ="", value_as_int = (int)(COP * 100)},
                                    new Leave(){objectid_fk_properties_objectid = 1351, nodeid_fk_nodes_nodeid = nodeForThisData.nodeid, value ="", value_as_int = tempOut },
                                    new Leave(){objectid_fk_properties_objectid = 1356, nodeid_fk_nodes_nodeid = nodeForThisData.nodeid, value ="", value_as_int = typeClimat },

                                };
            //Add them to the database
            foreach (var leave in leavesForCreate)
            {
                await _leaveRepository.CreateLeave(leave);
            }            
        }

        //Method for updating a long hash and switching to a different climate and temperature
        protected async Task<(int, int, string)> UpdateBigHash(int leavesIdCount, int actuelIndexLeaveIdInList, int wpId, int gradInseide, int typeClimat, string hash, string bigHash, int gradInseideInLeave, int typeClimatInLeaves)
        {
            if (leavesIdCount - 1 == actuelIndexLeaveIdInList)
                bigHash += hash + "#";

            var bigHashDB = await GetBigHashDB(wpId, gradInseide, typeClimat);
            if (bigHash.Count() >= 150 && bigHashDB != null)
            {
                bigHashDB.value = bigHash;
                if (await _leaveRepository.UpdateLeaves(bigHashDB))
                {
                    Console.WriteLine($"------Up Big Hash For {gradInseide} Grad And {(typeClimat == 1 ? "Cold"
                                                                                     : typeClimat == 2 ? "Mid"
                                                                                     : "Warm")}");
                }
                else
                {
                    Console.WriteLine("------Dont have node in DB, BigHash == null");
                }
            }

            gradInseide = gradInseideInLeave;
            typeClimat = typeClimatInLeaves;
            bigHash = "" + hash + "#";
            return (gradInseide, typeClimat, bigHash);
        }

        //Method for getting a long hash from the database
        protected async Task<Leave> GetBigHashDB(int wpId, int gradInseide, int typeClimat)
        {
            switch (gradInseide)
            {
                case 35:
                    return typeClimat == 1 ? await _leaveRepository.GetBigHashFor35GradForKaltesKlimaByWpId(wpId)   //if the climate is cold
                         : typeClimat == 2 ? await _leaveRepository.GetBigHashFor35GradForMittelKlimaByWpId(wpId)
                         :                   await _leaveRepository.GetBigHashFor35GradForWarmKlimaByWpId(wpId);  //if the climate is average
                case 55:
                    return typeClimat == 1 ? await _leaveRepository.GetBigHashFor55GradForKaltesKlimaByWpId(wpId)   //if the climate is cold
                         : typeClimat == 2 ? await _leaveRepository.GetBigHashFor55GradForMittelKlimaByWpId(wpId)
                         :                   await _leaveRepository.GetBigHashFor55GradForWarmKlimaByWpId(wpId);  //if the climate is average
                default:
                    return null;
            }
        }

        //Method for changing data in the model before sending it to the database
        protected virtual void ChangeDataForSendToDB(ref int typeData, Leave WPleistHeiz, Leave WPleistCOP, StandartDataPump dataPumpForThisData)
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
        //Method for changing data in the model before sending it to the database
        protected virtual void UnregulatedChangeDataForSendToDB(Leave WPleistHeiz, Leave WPleistCOP, UnregulatedStandartDataPump dataPumpForThisData)
        {
            WPleistHeiz.value_as_int = (int)(dataPumpForThisData.HC * 100);
            WPleistCOP.value_as_int = (int)(dataPumpForThisData.COP * 100);            
        }

        //Method for hashing a string with a carry of 5 bits
        protected int GetHashCode(string s)
        {
            int hash = 0;
            int len = s.Length;

            if (len == 0)
                return hash;

            for (int i = 0; i < len; i++)
            {
                char chr = s[i];
                hash = (hash << 5) - hash + chr;
                hash |= 0; // Convert to 32-bit integer
            }

            return hash;
        }
        //We get a list of pumps with the desired name or create a pump if it doesn’t exist 
        protected async Task<List<Leave>> GetWPList(string pumpName, string typePump, int idCompany)
        {
            List<Leave> wpList = new List<Leave>();
            var textForWpList = await _textRepository.FindTextIdByGerName(pumpName);
            if (textForWpList.Count > 0)
            {
                foreach (var textForWp in textForWpList)
                {
                    wpList.Add(await _leaveRepository.FindLeaveByTextId(textForWp.textid));
                }
            }
            else
            {
                wpList = await _leaveRepository.FindLeaveByNamePump(pumpName);
            }
            if (wpList.Count == 0)
            {
                switch (typePump)
                {
                    case "Wasser":
                        wpList.Add(await CreateNewPumpWasser(pumpName, idCompany));
                        break;
                    case "Luft":
                        wpList.Add(await CreateNewPumpLuft(pumpName, idCompany));
                        break;
                    case "Sole":
                        wpList.Add(await CreateNewPumpSole(pumpName, idCompany));
                        break;
                }
            }
            return wpList;
        }
        //We get a list of pumps with the desired name or create a pump if it doesn’t exist 
        protected async Task<List<Leave>> UnregulatedGetWPList(string pumpName, string typePump, int idCompany)
        {
            List<Leave> wpList = new List<Leave>();
            var textForWpList = await _textRepository.FindTextIdByGerName(pumpName);
            if (textForWpList.Count > 0)
            {
                foreach (var textForWp in textForWpList)
                {
                    wpList.Add(await _leaveRepository.FindLeaveByTextId(textForWp.textid));
                }
            }
            else
            {
                wpList = await _leaveRepository.FindLeaveByNamePump(pumpName);
            }
            if (wpList.Count == 0)
            {
                switch (typePump)
                {
                    case "Wasser":
                        wpList.Add(await UnregulatedCreateNewPumpWasser(pumpName, idCompany));
                        break;
                    case "Luft":
                        wpList.Add(await UnregulatedCreateNewPumpLuft(pumpName, idCompany));
                        break;
                    case "Sole":
                        wpList.Add(await UnregulatedCreateNewPumpSole(pumpName, idCompany));
                        break;
                }
            }
            return wpList;
        }
        
        //Creation of different types of pumps
        protected async Task<Leave> CreateNewPumpLuft(string namePump, int idCompany)
        {
            var node = new Node()
            {
                typeid_fk_types_typeid = 6,
                parentid_fk_nodes_nodeid = idCompany,
                licence = 0
            };
            await _nodeRepository.CreateNode(node);
            var wpId = node.nodeid;

            var leavesList = new List<Leave>()
            {
                new Leave(){ objectid_fk_properties_objectid = 1006, nodeid_fk_nodes_nodeid = wpId, value = namePump, value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1001, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 2},
                new Leave(){ objectid_fk_properties_objectid = 1002, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1007, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1019, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1023, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1031, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1245, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1258, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1542, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1543, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1699, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 60}
            };
            foreach (var item in leavesList)
            {
                await _leaveRepository.CreateLeave(item);
            }
            return leavesList[0];
        }
        protected async Task<Leave> CreateNewPumpWasser(string namePump, int idCompany)
        {
            var node = new Node()
            {
                typeid_fk_types_typeid = 6,
                parentid_fk_nodes_nodeid = idCompany,
                licence = 0
            };
            await _nodeRepository.CreateNode(node);
            var wpId = node.nodeid;

            var leavesList = new List<Leave>()
            {
                new Leave(){ objectid_fk_properties_objectid = 1006, nodeid_fk_nodes_nodeid = wpId, value = namePump, value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1001, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 3},
                new Leave(){ objectid_fk_properties_objectid = 1002, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1007, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1019, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1022, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 150},
                new Leave(){ objectid_fk_properties_objectid = 1023, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1031, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1245, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1258, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1699, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 60},
            };
            foreach (var item in leavesList)
            {
                await _leaveRepository.CreateLeave(item);
            }
            return leavesList[0];
        }
        protected async Task<Leave> CreateNewPumpSole(string namePump, int idCompany)
        {
            var node = new Node()
            {
                typeid_fk_types_typeid = 6,
                parentid_fk_nodes_nodeid = idCompany,
                licence = 0
            };
            await _nodeRepository.CreateNode(node);
            var wpId = node.nodeid;

            var leavesList = new List<Leave>()
            {
                new Leave(){ objectid_fk_properties_objectid = 1006, nodeid_fk_nodes_nodeid = wpId, value = namePump, value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1001, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1002, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1007, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1019, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1022, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 150},
                new Leave(){ objectid_fk_properties_objectid = 1023, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1031, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1245, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1258, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1699, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 60},
            };
            foreach (var item in leavesList)
            {
                await _leaveRepository.CreateLeave(item);
            }
            return leavesList[0];
        }

        //Creation of different types of pumps
        protected async Task<Leave> UnregulatedCreateNewPumpLuft(string namePump, int idCompany)
        {
            var node = new Node()
            {
                typeid_fk_types_typeid = 6,
                parentid_fk_nodes_nodeid = idCompany,
                licence = 0
            };
            await _nodeRepository.CreateNode(node);
            var wpId = node.nodeid;

            var leavesList = new List<Leave>()
            {
                new Leave(){ objectid_fk_properties_objectid = 1006, nodeid_fk_nodes_nodeid = wpId, value = namePump, value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1001, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 2},
                new Leave(){ objectid_fk_properties_objectid = 1002, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1007, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1019, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1023, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1031, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1245, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1258, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1542, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1543, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1699, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 60}
            };
            foreach (var item in leavesList)
            {
                await _leaveRepository.CreateLeave(item);
            }
            return leavesList[0];
        }
        protected async Task<Leave> UnregulatedCreateNewPumpWasser(string namePump, int idCompany)
        {
            var node = new Node()
            {
                typeid_fk_types_typeid = 6,
                parentid_fk_nodes_nodeid = idCompany,
                licence = 0
            };
            await _nodeRepository.CreateNode(node);
            var wpId = node.nodeid;

            var leavesList = new List<Leave>()
            {
                new Leave(){ objectid_fk_properties_objectid = 1006, nodeid_fk_nodes_nodeid = wpId, value = namePump, value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1001, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 3},
                new Leave(){ objectid_fk_properties_objectid = 1002, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1007, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1019, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1022, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 150},
                new Leave(){ objectid_fk_properties_objectid = 1023, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1031, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1245, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1258, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1699, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 60},
            };
            foreach (var item in leavesList)
            {
                await _leaveRepository.CreateLeave(item);
            }
            return leavesList[0];
        }
        protected async Task<Leave> UnregulatedCreateNewPumpSole(string namePump, int idCompany)
        {
            var node = new Node()
            {
                typeid_fk_types_typeid = 6,
                parentid_fk_nodes_nodeid = idCompany,
                licence = 0
            };
            await _nodeRepository.CreateNode(node);
            var wpId = node.nodeid;

            var leavesList = new List<Leave>()
            {
                new Leave(){ objectid_fk_properties_objectid = 1006, nodeid_fk_nodes_nodeid = wpId, value = namePump, value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1001, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1002, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1007, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 1},
                new Leave(){ objectid_fk_properties_objectid = 1019, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1022, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 150},
                new Leave(){ objectid_fk_properties_objectid = 1023, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 50},
                new Leave(){ objectid_fk_properties_objectid = 1031, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1245, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1258, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 0},
                new Leave(){ objectid_fk_properties_objectid = 1699, nodeid_fk_nodes_nodeid = wpId, value = "", value_as_int = 60},
            };
            foreach (var item in leavesList)
            {
                await _leaveRepository.CreateLeave(item);
            }
            return leavesList[0];
        }

    }
}
