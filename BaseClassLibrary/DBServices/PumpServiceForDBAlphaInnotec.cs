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
    internal class PumpServiceForDBAlphaInotec
    {
        private readonly LeaveRepository _leaveRepository;
        private readonly NodeRepository _nodeRepository;
        private readonly TextRepository _textRepository;
        public PumpServiceForDBAlphaInotec(string pathDB)
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
               .UseSqlite("Data Source=" + pathDB + ";")
               .Options;
            _leaveRepository = new LeaveRepository(new ApplicationDBContext(options));
            _nodeRepository = new NodeRepository(new ApplicationDBContext(options));
            _textRepository = new TextRepository(new ApplicationDBContext(options));
        }
        public async Task ChangeLeistungsdatenInDbByExcelData(Pump pump)
        {            
            var textIdForWp = _textRepository.FindTextIdByGerName(pump.Name);            
            var wpList = await _leaveRepository.FindLeaveByTextId(textIdForWp);
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
                            WPleistHeiz.value_as_int = (int)(newData.MidHC * 100);
                            await _leaveRepository.UpdateLeaves(WPleistHeiz);
                            //Finding the COP and Update
                            var WPleistCOP = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1221);
                            WPleistCOP.value_as_int = (int)(newData.MidCOP * 100);
                            await _leaveRepository.UpdateLeaves(WPleistCOP);
                            //Finding the Leistungsaufnahme and Update
                            var WPleistAuf = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1014);
                            WPleistAuf.value_as_int = (int)(newData.MidHC / newData.MidCOP * 100);
                            await _leaveRepository.UpdateLeaves(WPleistAuf);
                            //Finding the Kealteleistung and Update
                            var WPleistKaelte = leavesForUpdate.FirstOrDefault(x => x.objectid_fk_properties_objectid == 1013);
                            WPleistKaelte.value_as_int = (int)((newData.MidHC - 0.96 * (newData.MidHC / newData.MidCOP)) * 100);
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
                                new Leave() { objectid_fk_properties_objectid = 1012, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = (int)(newData.MidHC * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1013, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = (int)((newData.MidHC - 0.96 * (newData.MidHC / newData.MidCOP)) * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1014, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = (int)(newData.MidHC / newData.MidCOP * 100) },
                                new Leave() { objectid_fk_properties_objectid = 1015, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = newData.MaxVorlauftemperatur },
                                new Leave() { objectid_fk_properties_objectid = 1221, nodeid_fk_nodes_nodeid = node.nodeid, value = "", value_as_int = (int)(newData.MidCOP * 100) }
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




        //Update in DB this data  EN 14825 LG
        public async Task ChangeDataenEN14825LGInDbByExcelData(StandartPump pump)
        {
            var wpList = await _leaveRepository.FindLeaveByNamePump(pump.Name);
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
                                            _leaveRepository.UpdateLeaves(WPleistHeiz);
                                            _leaveRepository.UpdateLeaves(WPleistCOP);
                                            //form a hash and update
                                            var str = WPleistATemp + "#" + WPleistHeiz.value_as_int + "#" + WPleistCOP.value_as_int;
                                            int hash = GetHashCode(str);
                                            Gui14825Hashcode.value = hash.ToString();
                                            _leaveRepository.UpdateLeaves(Gui14825Hashcode);
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

                    Console.WriteLine("Pump -" + wp.value + "  Update!");
                    Console.WriteLine();
                    Console.WriteLine();
                    Task.Delay(1000).Wait();
                }
            }
        }
        //Method for updating a long hash and switching to a different climate and temperature
        private async Task<(int, int, string)> UpdateBigHash(int leavesIdCount, int actuelIndexLeaveIdInList, int wpId, int gradInseide, int typeClimat, string hash, string bigHash, int gradInseideInLeave, int typeClimatInLeaves)
        {
            if (leavesIdCount - 1 == actuelIndexLeaveIdInList)
                bigHash += hash + "#";

            var bigHashDB = await GetBigHashDB(wpId, gradInseide, typeClimat);
            if (bigHash.Count() >= 150 && bigHashDB != null)
            {
                bigHashDB.value = bigHash;
                if (await _leaveRepository.UpdateLeaves(bigHashDB))
                {
                    Console.WriteLine($"------Up Big Hash For {gradInseide} Grad And {(typeClimat == 1 ? "Cold" : "Mid")}");
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
        private async Task<Leave> GetBigHashDB(int wpId, int gradInseide, int typeClimat)
        {
            switch (gradInseide)
            {
                case 35:
                    return typeClimat == 1 ? await _leaveRepository.GetBigHashFor35GradForKaltesKlimaByWpId(wpId)   //if the climate is cold
                                            : await _leaveRepository.GetBigHashFor35GradForMittelKlimaByWpId(wpId);  //if the climate is average
                case 55:
                    return typeClimat == 1 ? await _leaveRepository.GetBigHashFor55GradForKaltesKlimaByWpId(wpId)   //if the climate is cold
                                            : await _leaveRepository.GetBigHashFor55GradForMittelKlimaByWpId(wpId);  //if the climate is average
                default:
                    return null;
            }
        }

        //Method for changing data in the model before sending it to the database
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
                    WPleistHeiz.value_as_int = (int)(dataPumpForThisData.MidHC * 100);
                    WPleistCOP.value_as_int = (int)(dataPumpForThisData.MidCOP * 100);
                    typeData = 0;
                    break;
                default:
                    break;
            }
        }
        //Method for hashing a string with a carry of 5 bits
        private int GetHashCode(string s)
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
    }
}
