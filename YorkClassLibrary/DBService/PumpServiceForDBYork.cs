using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.DBConnection;
using TestExel.DBModels;
using TestExel.Models;
using TestExel.Repository;
using TestExel.ServicesForDB;
using TestExel.StandartModels;

namespace YorkClassLibrary.DBService
{
    public class PumpServiceForDBYork : PumpServiceForDB
    {
        public PumpServiceForDBYork(string pathDB) : base(pathDB)
        {
            
        }

        //Method for adding Leistungdaten to the database (Especially for York, it is made so that not Max Data is taken, but Min Data)
        public override async Task ChangeLeistungsdatenInDbByExcelData(Pump pump, string typePump, int idCompany)
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
                            WPleistHeiz.value_as_int = newData.MidHC == 0 ? 0 : (int)(newData.MidHC * 100);
                            await _leaveRepository.UpdateLeaves(WPleistHeiz);
                            //Finding the COP and Update
                            var WPleistCOP = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1221);
                            WPleistCOP.value_as_int = newData.MidCOP == 0 ? 0 : (int)(newData.MidCOP * 100);
                            await _leaveRepository.UpdateLeaves(WPleistCOP);
                            //Finding the Leistungsaufnahme and Update
                            var WPleistAuf = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1014);
                            WPleistAuf.value_as_int = newData.MidCOP == 0 || newData.MidHC == 0 ? 0 : (int)(newData.MidHC / newData.MidCOP * 100);
                            await _leaveRepository.UpdateLeaves(WPleistAuf);
                            //Finding the Kealteleistung and Update
                            var WPleistKaelte = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1013);
                            WPleistKaelte.value_as_int = newData.MidCOP == 0 || newData.MidHC == 0 ? 0 : (int)((newData.MidHC - 0.96 * (newData.MidHC / newData.MidCOP)) * 100);
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
                                new Leave() { objectid_fk_properties_objectid = 1012, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MidHC == 0 ? 0 :(int)(newData.MidHC * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1013, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MidCOP == 0 || newData.MidHC == 0 ? 0 : (int)((newData.MidHC - 0.96 * (newData.MidHC / newData.MidCOP)) * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1014, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MidCOP == 0 || newData.MidHC == 0 ? 0 :(int)(newData.MidHC / newData.MidCOP * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1015, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MaxVorlauftemperatur },
                                new Leave() { objectid_fk_properties_objectid = 1221, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MidCOP == 0 ? 0 : (int)(newData.MidCOP * 100) }
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

        //Method for changing data in the model before sending it to the database (Especially for York it was made so that Max Data == Min Data)
        protected override void ChangeDataForSendToDB(ref int typeData, Leave WPleistHeiz, Leave WPleistCOP, StandartDataPump dataPumpForThisData)
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
                    WPleistHeiz.value_as_int = (int)(dataPumpForThisData.MidHC * 100);
                    WPleistCOP.value_as_int = (int)(dataPumpForThisData.MidCOP * 100);
                    typeData = 0;
                    break;
                default:
                    break;
            }
        }
        //Method for creating 14825 data (Especially for York it was made so that Max Data == Min Data)
        protected override async Task CreateNew14825Data(Dictionary<int, List<StandartDataPump>> dataDictionary, int typeClimat, int wpId)
        {
            string bigHash = "";
            int forTemp = dataDictionary.Values.First().First().ForTemp;
            foreach (var data in dataDictionary)
            {
                foreach (var dataValue in data.Value)
                {
                    bigHash += await Create14825ForSelectedData(wpId, data.Key, typeClimat, dataValue.ForTemp, dataValue.MinHC, dataValue.MinCOP);
                    bigHash += await Create14825ForSelectedData(wpId, data.Key, typeClimat, dataValue.ForTemp, dataValue.MidHC, dataValue.MidCOP);
                    bigHash += await Create14825ForSelectedData(wpId, data.Key, typeClimat, dataValue.ForTemp, dataValue.MidHC, dataValue.MidCOP);
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
    }
}
