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
        private readonly LeaveRepository _leaveRepository;
        private readonly NodeRepository _nodeRepository;       
        protected async Task CreateNew14825Data(Dictionary<int, List<StandartDataPump>> dataDictionary, int typeClimat, int wpId)
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
        protected void ChangeDataForSendToDB(ref int typeData, Leave WPleistHeiz, Leave WPleistCOP, StandartDataPump dataPumpForThisData)
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
    }
}
