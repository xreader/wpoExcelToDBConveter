using BaseClassLibrary.Models;
using BaseClassLibrary.StandartModels;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SQLitePCL;
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

        public virtual async Task ChangeDataenEN14825LGInDbByExcelData(StandartPump pump, string typePump, int idCompany, int numClimat)
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

                        foreach (var idLeave in leavesIdWithOldDataList)
                        {
                            var leaves = await _leaveRepository.GetLeavesById(idLeave);
                            foreach (var leave in leaves)
                            {
                                await _leaveRepository.DeleteLeave(leave);
                            }
                            var node = await _nodeRepository.GetNodeByIdAsync(idLeave);
                            await _nodeRepository.DeleteNode(node);
                        }

                        var leavesWithBigHash = new List<Leave>
                        {
                            await _leaveRepository.GetBigHashFor35GradForKaltesKlimaByWpId(wpId),
                            await _leaveRepository.GetBigHashFor55GradForKaltesKlimaByWpId(wpId),
                            await _leaveRepository.GetBigHashFor35GradForMittelKlimaByWpId(wpId),
                            await _leaveRepository.GetBigHashFor55GradForMittelKlimaByWpId(wpId),
                            await _leaveRepository.GetBigHashFor35GradForWarmKlimaByWpId(wpId),
                            await _leaveRepository.GetBigHashFor55GradForWarmKlimaByWpId(wpId)
                        };

                        foreach (var leaveWithBigHash in leavesWithBigHash)
                        {
                            if (leaveWithBigHash != null)
                            {
                                await _leaveRepository.DeleteLeave(leaveWithBigHash);
                            }
                        }
                    }
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
                        if (dataForActuelClimat35Grad.Count != dataForActuelClimat55Grad.Count)
                        {

                        }
                        await CreateNew14825Data(dataForActuelClimat35Grad, typeClimat, wpId);
                        await CreateNew14825Data(dataForActuelClimat55Grad, typeClimat, wpId);
                        typeClimat++;
                    }
                    typeClimat = 1;
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

                //We sort through the data we received from Excel q
                foreach (var newDataDictionary in pump.Data)
                {
                    foreach (var newData in newDataDictionary.Value)
                    {
                        //We are looking for a list of records where there is data that needs to be changed and their quantity, if the number is more than 1, then we change the first one and delete the rest
                        var listWithLeistungDaten = listWithleavesWithListOldLeistungdaten
                                     .Where(list => list.Any(leave => leave.value_as_int == newDataDictionary.Key && leave.objectid_fk_properties_objectid == 1010))
                                     .Where(list => list.Any(leave => leave.value_as_int == newData.Temp && leave.objectid_fk_properties_objectid == 1011))
                                     .Where(list => list.Any(leave => leave.value_as_int == newData.MaxVorlauftemperatur && leave.objectid_fk_properties_objectid == 1015))
                                     .ToList();
                        //If there are such records, we simply update them and delete duplicates
                        if (listWithLeistungDaten.Count > 0)
                        {
                            foreach (var leavesForDelete in listWithLeistungDaten)
                            {
                                foreach (var leaveForDelete in leavesForDelete)
                                    await _leaveRepository.DeleteLeave(leaveForDelete);
                            }

                            foreach (var idNodeForDelete in leavesIdWithOldLeistungdatenList)
                            {
                                var nodeForDelete = await _nodeRepository.GetNodeByIdAsync(idNodeForDelete);
                                if (nodeForDelete != null)
                                    await _nodeRepository.DeleteNode(nodeForDelete);
                            }

                        }
                        if (newData.MaxCOP > 0 && newData.MaxHC > 0)
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



                Console.WriteLine("Pump -" + wp.value + " Leistungdata Update!");

                // Update backup heater properties (1031 and 1258)
                await UpdateBackupHeater(wpId, pump.BackupHeaterKW);

                // Update BAFA COPs (1204, 1205, 1650)
                await UpdateBafaCOPs(wpId, pump.BafaCOPs);
            }

        }

        // Update Heizstab properties: 1031 (hat Heizstab j/n) and 1258 (Leistung in kW)
        protected async Task UpdateBackupHeater(int wpId, double backupHeaterKW)
        {
            try
            {
                // Find only the specific leaves for 1031 and 1258
                var leave1031 = await _leaveRepository.FindLeaveByNodeIdAndPropertyId(wpId, 1031);
                if (leave1031 != null)
                {
                    leave1031.value_as_int = backupHeaterKW > 0 ? 1 : 0;
                    await _leaveRepository.UpdateLeaves(leave1031);
                }

                var leave1258 = await _leaveRepository.FindLeaveByNodeIdAndPropertyId(wpId, 1258);
                if (leave1258 != null)
                {
                    leave1258.value_as_int = (int)Math.Round(backupHeaterKW * 10);
                    await _leaveRepository.UpdateLeaves(leave1258);
                }

                if (backupHeaterKW > 0)
                    Console.WriteLine($"  Heizstab: {backupHeaterKW} kW gesetzt.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Heizstab-Update für Node {wpId} fehlgeschlagen: {ex.Message}");
            }
        }

        // Update BAFA COPs: 1204 (-7°C), 1205 (2°C), 1650 (7°C) - value_as_int = Round(COP * 100)
        protected async Task UpdateBafaCOPs(int wpId, Dictionary<int, double> bafaCOPs)
        {
            if (bafaCOPs == null || bafaCOPs.Count == 0) return;

            // Mapping: outdoor temp -> property ID
            var tempToProperty = new Dictionary<int, int>
            {
                { -7, 1204 },
                {  2, 1205 },
                {  7, 1650 }
            };

            try
            {
                foreach (var mapping in tempToProperty)
                {
                    if (bafaCOPs.TryGetValue(mapping.Key, out double copValue) && copValue > 0)
                    {
                        int intValue = (int)Math.Round(copValue * 100);
                        var leave = await _leaveRepository.FindLeaveByNodeIdAndPropertyId(wpId, mapping.Value);
                        if (leave != null)
                        {
                            leave.value_as_int = intValue;
                            await _leaveRepository.UpdateLeaves(leave);
                        }
                        else
                        {
                            // Create new leave if it doesn't exist
                            var newLeave = new Leave()
                            {
                                objectid_fk_properties_objectid = mapping.Value,
                                nodeid_fk_nodes_nodeid = wpId,
                                value = "",
                                value_as_int = intValue
                            };
                            await _leaveRepository.CreateLeave(newLeave);
                        }
                    }
                }
                Console.WriteLine($"  BAFA COPs gesetzt: {string.Join(", ", bafaCOPs.Select(x => $"A{x.Key}={Math.Round(x.Value * 100)}"))}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  BAFA COP-Update für Node {wpId} fehlgeschlagen: {ex.Message}");
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
                    if (newDataDictionary.Value.Count == 1)
                    {
                        var data = pump.Data.FirstOrDefault(x => x.Value.Count == 2);
                        var oneData = newDataDictionary.Value[0];
                        var dataWichEqualsTempOneData = data.Value.FirstOrDefault(x => x.Temp == oneData.Temp);
                        var NOdataWichEqualsTempOneData = data.Value.FirstOrDefault(x => x.Temp != oneData.Temp);
                        newDataDictionary.Value.Add(new UnregulatedDataPump()
                        {
                            Temp = NOdataWichEqualsTempOneData.Temp,
                            HC = Math.Round(oneData.HC * NOdataWichEqualsTempOneData.HC / dataWichEqualsTempOneData.HC, 2),
                            COP = Math.Round(oneData.COP * NOdataWichEqualsTempOneData.COP / dataWichEqualsTempOneData.COP, 2),
                            MaxVorlauftemperatur = NOdataWichEqualsTempOneData.MaxVorlauftemperatur
                        });
                    }

                    foreach (var newData in newDataDictionary.Value)
                    {
                        //We are looking for a list of records where there is data that needs to be changed and their quantity, if the number is more than 1, then we change the first one and delete the rest
                        var listWithLeistungDaten = listWithleavesWithListOldLeistungdaten
                                     .Where(list => list.Any(leave => leave.value_as_int == newDataDictionary.Key && leave.objectid_fk_properties_objectid == 1010))
                                     .Where(list => list.Any(leave => leave.value_as_int == newData.Temp && leave.objectid_fk_properties_objectid == 1011))
                                     .Where(list => list.Any(leave => leave.value_as_int == newData.MaxVorlauftemperatur && leave.objectid_fk_properties_objectid == 1015))
                                     .ToList();
                        //If there are such records, we simply update them and delete duplicates
                        if (listWithLeistungDaten.Count > 0)
                        {
                            foreach (var leavesForDelete in listWithLeistungDaten)
                            {
                                foreach (var leaveForDelete in leavesForDelete)
                                    await _leaveRepository.DeleteLeave(leaveForDelete);
                            }

                            foreach (var idNodeForDelete in leavesIdWithOldLeistungdatenList)
                            {
                                var nodeForDelete = await _nodeRepository.GetNodeByIdAsync(idNodeForDelete);
                                if (nodeForDelete != null)
                                    await _nodeRepository.DeleteNode(nodeForDelete);
                            }

                        }
                        if (newData.COP > 0 && newData.HC > 0)
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



                Console.WriteLine("Pump -" + wp.value + " Leistungdata Update!");
            }

        }

        protected virtual async Task CreateNew14825Data(Dictionary<int, List<StandartDataPump>> dataDictionary, int typeClimat, int wpId)
        {
            int[] coldClimate = { -25, -22, -15, -7, 2, 7, 12 };
            int[] midClimate = { -15, -10, -7, 2, 7, 12 };
            int[] warmClimate = { -7, 2, 2, 7, 12 };

            bool correctOutTemp = typeClimat == 1 && dataDictionary.Count > 6 ? true
                                     : typeClimat == 2 && dataDictionary.Count > 5 ? true
                                     : typeClimat == 3 && dataDictionary.Count > 4 ? true : false;

            int minKey = dataDictionary.Keys.Min();
            string bigHash = "";
            int forTemp = dataDictionary.Values.First().First().ForTemp;
            foreach (var data in dataDictionary)
            {

                foreach (var dataValue in data.Value)
                {

                    bigHash += (dataValue.MinHC == 0 && dataValue.MinCOP == 0) ? (data.Key + "#") : await Create14825ForSelectedData(wpId, data.Key, typeClimat, dataValue.ForTemp, dataValue.MinHC, dataValue.MinCOP);
                    bigHash += (dataValue.MidHC == 0 && dataValue.MidCOP == 0) ? (data.Key + "#") : await Create14825ForSelectedData(wpId, data.Key, typeClimat, dataValue.ForTemp, dataValue.MidHC, dataValue.MidCOP);
                    bigHash += (dataValue.MaxHC == 0 && dataValue.MaxCOP == 0) ? (data.Key + "#") : await Create14825ForSelectedData(wpId, data.Key, typeClimat, dataValue.ForTemp, dataValue.MaxHC, dataValue.MaxCOP);
                    if (!correctOutTemp)
                    {
                        int correctOutTempCount = typeClimat == 1 ? 6 - dataDictionary.Count
                                     : typeClimat == 2 ? 5 - dataDictionary.Count
                                     : typeClimat == 3 ? 4 - dataDictionary.Count : 0;
                        for (int i = 0; i < correctOutTempCount; i++)
                        {
                            var minKeyForAddWhenNotHaveNumber = typeClimat == 1 ? coldClimate.Where(x => x < minKey).DefaultIfEmpty(int.MinValue).Max()
                                               : typeClimat == 2 ? midClimate.Where(x => x < minKey).DefaultIfEmpty(int.MinValue).Max()
                                               : typeClimat == 3 ? warmClimate.Where(x => x < minKey).DefaultIfEmpty(int.MinValue).Max() : -20;

                            bigHash += minKeyForAddWhenNotHaveNumber + "#";
                            bigHash += minKeyForAddWhenNotHaveNumber + "#";
                            bigHash += minKeyForAddWhenNotHaveNumber + "#";
                            minKey = minKeyForAddWhenNotHaveNumber;
                        }

                        correctOutTemp = true;
                    }
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
                         : await _leaveRepository.GetBigHashFor35GradForWarmKlimaByWpId(wpId);  //if the climate is average
                case 55:
                    return typeClimat == 1 ? await _leaveRepository.GetBigHashFor55GradForKaltesKlimaByWpId(wpId)   //if the climate is cold
                         : typeClimat == 2 ? await _leaveRepository.GetBigHashFor55GradForMittelKlimaByWpId(wpId)
                         : await _leaveRepository.GetBigHashFor55GradForWarmKlimaByWpId(wpId);  //if the climate is average
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
                    var leave = await _leaveRepository.FindLeaveByTextId(textForWp.textid);
                    if (leave != null)
                    {
                        wpList.Add(leave);
                    }
                }
            }
            // If no leaves found via text, try direct name search
            if (wpList.Count == 0)
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
                    var leave = await _leaveRepository.FindLeaveByTextId(textForWp.textid);
                    if (leave != null)
                    {
                        wpList.Add(leave);
                    }
                }
            }
            // If no leaves found via text, try direct name search
            if (wpList.Count == 0)
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