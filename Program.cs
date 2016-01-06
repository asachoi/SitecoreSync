using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

using Newtonsoft.Json.Linq;

namespace ConsoleApplication4
{
    class Program
    {
        static void Main(string[] args)
        {
            Sitecore a = new Sitecore();
            a.getCountries();
            
        }
    }

    class Sitecore
    {
        System.IO.StreamWriter r;

        public Sitecore()
        {
            r = System.IO.File.AppendText("./actionlog.log");
        }

        public void log(string msg)
        {
            r.WriteLine(msg + ", " + DateTime.Now.ToString());
            Console.WriteLine(msg );
        }
        public void getCountries()
        {

            string SQL = "";

            var countries =
                from p in getNode("/sitecore/content/Home/*")
                where (string)p["Template"] == "DICE/Item Types/Country"
                select p;

            foreach (var c in countries)
            {
                //    log(c["Fields"]["{397FD354-09A1-4AF8-9551-AA0B95457DB2}"]["Value"]);      
                var countryPath = c["Path"];
                var CountryCode = c["Fields"]["{397FD354-09A1-4AF8-9551-AA0B95457DB2}"]["Value"];
                var CountryName = c["Fields"]["{C7A1A5F9-EA92-4F9D-99E0-BF0DE61712F7}"]["Value"];
                //countryText += CountryCode + "," + CountryName + "\n";

                SQL += "insert into tbl_country values ('" + CountryCode + "','" + CountryName + "');\n";

                Dictionary<String, String> States = new Dictionary<string, string>();
                Dictionary<String, String> TVMarkets = new Dictionary<string, string>();
                log((string)CountryName);
                log(">Sync Master Tables");
                #region master sync

                var states = getNode(countryPath + "/States/*");
                foreach (var s in states)
                {
                    var StateName = s["Fields"]["{C7A1A5F9-EA92-4F9D-99E0-BF0DE61712F7}"]["Value"];
                    var StateCode = s["Fields"]["{CCC945AE-4A79-401D-8CD2-A0C44FA7F231}"]["Value"];
                    States.Add((string)StateName, (string)StateCode);
                    log(">>State: " + StateName);
                    SQL += "insert into tbl_state values ('" + StateCode + "','" + StateName + "','" + CountryCode + "','');\n";

                }


                var tvmarkets = getNode(countryPath + "/TVMarkets/*");
                foreach (var t in states)
                {
                    var Name = t["Fields"]["{C7A1A5F9-EA92-4F9D-99E0-BF0DE61712F7}"]["Value"];
                    var Code = t["Fields"]["{CCC945AE-4A79-401D-8CD2-A0C44FA7F231}"]["Value"];
                    TVMarkets.Add((string)Name, (string)Code);
                    log(">>TVMarket: " + Name);
                    SQL += "insert into tbl_tvmarket values ('" + Code + "','" + Name + "','" + CountryCode + "');\n";

                }

                var vouchers = getNode(countryPath + "/Offers/*");
                foreach (var v in vouchers)
                {
                    var vouchercode = v["Fields"]["{FBD70EE5-EDB8-4409-92B0-CFD9D5E7AEE8}"]["Value"];
                    var header = v["Fields"]["{A82920AB-DFA0-4960-BDC1-FDD326B02073}"]["Value"];
                    var title = v["Fields"]["{F6F1392D-2AF3-4DD1-A532-9B41D535B731}"]["Value"];
                    var subtitle = v["Fields"]["{425B4C87-3390-41DC-B54F-B6FC6397FD16}"]["Value"];
                    var from = v["Fields"]["{4629E3B3-8741-4322-876A-B5F518C0CEA2}"]["Value"];
                    var price = v["Fields"]["{43863D6B-7ACE-4151-B178-387ECE482BA2}"]["Value"];
                    var pricelabel = v["Fields"]["{057F0C70-3EAB-456A-BE93-33BFF19E0AF0}"]["Value"];
                    var terms = v["Fields"]["{BD06C4AA-C373-496B-BCD6-2BCB506AE181}"]["Value"];
                    var startdate = v["Fields"]["{94D32B0D-4EB1-490D-A263-249E415A02CD}"]["Value"];
                    var enddate = v["Fields"]["{29AFF084-03F5-4677-9C23-7C537C1B1ABE}"]["Value"];
                    var pickup = v["Fields"]["{798FA8F9-2FF7-4197-BA49-2FB85BB1BE9C}"]["Value"];
                    var delivered = v["Fields"]["{20FA4D41-1843-40BA-A3A8-39A239DBEB28}"]["Value"];
                    var destinationurl = v["Fields"]["{E8EAC96D-3111-4BAF-AB9B-979BE01B3C35}"]["Value"];
                    var product = v["Fields"]["{B257E2E4-C90B-4AF5-89BA-CA7D8D63E9EF}"]["Value"];
                    var tilecreated = "";
                    var active = "1";
                    string SQLText =
                        "insert into tbl_voucher (voucherid, vouchercode, header, title, subtitle, [from], price, pricelabel, terms, startdate, enddate, pickup, delivered, destinationurl, producttypeid, countrycode, tilecreated, active) values ('{0}', '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}', '{14}', '{15}', '{16}');";

                    SQLText = string.Format(SQLText,
                        vouchercode, header, title.ToString().Replace("'", "''"), subtitle, from,
                        price, pricelabel, terms.ToString().Replace("'", "''"), startdate, enddate,
                        pickup, delivered, destinationurl, product, CountryCode, tilecreated, active);
                    log(">>voucher: " + vouchercode);
                    SQL += SQLText + "\n";

                }



                #endregion

                log(">Sync Store Tables");
                #region store sync
                var stores = getNode(countryPath + "/Stores/*");
                foreach (var s in stores)
                {
                    var storeNumber = s["Fields"]["{CFA348EF-AA46-48D7-918F-0C4A0CF43C35}"]["Value"];
                    var storeName = s["Fields"]["{C7A1A5F9-EA92-4F9D-99E0-BF0DE61712F7}"]["Value"];
                    var address = s["Fields"]["{A3CB3A41-05A5-4C5E-A61A-B0D9809AE698}"]["Value"];
                    var Suburb = s["Fields"]["{75180E49-21AF-42A9-95F9-A755D4B2A451}"]["Value"];
                    var State = s["Fields"]["{46C66CCD-9FC5-42F1-89BC-E059A66F43BC}"]["Value"];
                    var PostCode = s["Fields"]["{9E97CED1-1ABA-4326-9FC9-D6AC6A3C561D}"]["Value"];
                    var TVMarket = s["Fields"]["{70DDFF11-1505-40C8-A622-51250DC0E1D7}"]["Value"];
                    var Phone = s["Fields"]["{1F1ACF5D-76A7-4063-AFE7-E44B8F3616D7}"]["Value"];
                    var cityid = 0;
                    var corporatestore = s["Fields"]["{0382B2E6-4DA0-493C-9159-63E7F49FBE99}"]["Value"];
                    var defaultoptout = s["Fields"]["{29831EB7-62F2-4E80-BA0F-81C0842B9CDD}"]["Value"];
                    var defaultchoice = 0;// s["Fields"]["{83AC6A23-7FE0-42EF-9A55-2C6FA00A267D}"]["Value"];
                    var active = s["Fields"]["{DC33632A-59B3-49BE-BE85-CBEC706B0EDC}"]["Value"];
                    var island = 0;
                    var subscribercount = 0;

                    if ((string)active == "Active")
                    {
                        active = 1;
                    }
                    else
                    {
                        active = 0;
                    }

                    if ((string)corporatestore == "")
                    {
                        corporatestore = 0;
                    }


                    try
                    {
                        State = ((string)State == "") ? "" : States[(string)State];
                    }
                    catch (Exception e)
                    {
                        State = "NA";
                    }

                    string storeText = String.Format(
                        @"'{0}','{0}', '{1}','{2}','{3}', '{4}', '{5}','{6}', '{7}', '{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}'",
                            storeNumber,
                            storeName,
                            address,
                            Suburb,
                            State,
                            PostCode,
                            CountryCode,
                            TVMarket,
                            Phone,
                            cityid,
                            corporatestore,
                            defaultoptout,
                            defaultchoice,
                            active,
                            island,
                            subscribercount
                        ) + "\n";

                    log(">>store: " + storeName);
                    string Temp = "insert into tbl_store values (" + storeText + ");\n";
                    SQL += Temp;
                }

                #endregion

                log(">Sync Campaign Masters");
                #region campaign sync
                var CMs = getNode(countryPath + "/CampaignMasters/*");

                foreach (var CM in CMs)
                {

                    string status = (string)CM["Fields"]["{955F5513-E636-4C5C-9748-F2A49099E307}"]["Value"];
                    if (status == "Report")
                    {
                        var defaultOffers = new List<string>();
                        //var campaignName = CM["Fields"]["{20AB2AAC-0F20-4667-9CC3-8CDE278421B3}"]["Value"];
                        string campaignName = (string)CM["DisplayName"];
                        log(">> " + campaignName);
                        var campaignDesc = CM["Fields"]["{2A5B7055-BCE8-4DAB-B773-4CBF89B6C9F5}"]["Value"];
                        var campaignType = CM["Fields"]["{30D19E29-F045-44A5-9E8F-F8BD2181999C}"]["Value"];
                        // var automaticMessagetypeID = CM["Fields"]["{CFA348EF-AA46-48D7-918F-0C4A0CF43C35}"]["Value"];

                        var deployment = CM["Fields"]["{DD9E0D76-F835-4207-9739-9D0C7B7E7CDC}"]["Value"];
                        var lockdays = CM["Fields"]["{56670F56-DF20-4396-9DAD-0D710DA3B9A2}"]["Value"];
                        var includenostoreid = CM["Fields"]["{737F0C88-58D7-49D2-935E-22A7CC9731B7}"]["Value"];
                        var localisedelivery = CM["Fields"]["{CFEE2DA1-6D89-48D8-A09E-238E8B25F156}"]["Value"];
                        var singleusevouchers = CM["Fields"]["{56670F56-DF20-4396-9DAD-0D710DA3B9A2}"]["Value"];
                        var targetcorporateonly = CM["Fields"]["{54CEB1CB-0A29-4504-A7B7-B70F92CD3083}"]["Value"];
                        var optoutnoncorporate = CM["Fields"]["{56670F56-DF20-4396-9DAD-0D710DA3B9A2}"]["Value"];
                        var masterchoice = CM["Fields"]["{2780DB4C-56CB-4B51-B517-CB549583188E}"]["Value"];
                        var advancedtargeting = CM["Fields"]["{56670F56-DF20-4396-9DAD-0D710DA3B9A2}"]["Value"];

                        string campaignSQL = @"insert into tbl_campaign (
                            [campaignid]  ,[campaignname]  ,[campaigndescription]   ,[campaigntypeid]    ,[automaticmessagetypeid] ,
                            [statusid]   ,[deploymentdatetime]    ,[lockdays]    ,[active]   ,[includenostoreid]    ,
                            [localisedelivery]   ,[singleusevouchers]    ,[targetcorporateonly]    ,[optoutnoncorporate]   ,[masterchoice] ,
                            [advancedtargeting]  ,[countrycode]  ,[proofversions]   ,[proofemails], [offers]) values 
                            (   '{0}','{0}','{1}','{2}','{3}',
                                '{4}','{5}','{6}','{7}','{8}',
                                '{9}','{10}','{11}','{12}','{13}',
                                '{14}','{15}','{16}','{17}', '{18}');";
                        campaignSQL = string.Format(campaignSQL,
                                    campaignName, campaignDesc, campaignType, campaignType,
                                    status, deployment, lockdays, status, includenostoreid,
                                    localisedelivery, singleusevouchers, targetcorporateonly, optoutnoncorporate, masterchoice,
                                    advancedtargeting, CountryCode, 0, "", "") + "\n";

                        SQL += campaignSQL;

                        string path = (string)CM["Path"];

                        path = path.Replace(campaignName, "%23" + campaignName + "%23");

                        var choices = getNode(path + "/*");

                        if (choices != null)
                        {
                            var offerNodes =
                                     from p in choices
                                     where (string)p["Template"] == "DICE/Item Types/OfferCampaign"
                                     select p;

                            foreach (var n in offerNodes)
                            {
                                var offer = n["Fields"]["{2622BBD8-AB86-41BB-B1C0-D9EBC94EFFF7}"]["Value"];
                                if (offer != null)
                                {
                                    var offerNode = getNode((string)offer, true);
                                    if (offerNode.HasValues)
                                    {
                                        var offerCode = offerNode[0]["Fields"]["{FBD70EE5-EDB8-4409-92B0-CFD9D5E7AEE8}"]["Value"];//OfferCode
                                        defaultOffers.Add((string)offerCode);
                                        //need to generate customized offers for store
                                        Console.Write(">>>Default Offer:" + offerCode);
                                    }
                                }
                            }

                            var masterChoices =
                                     from p in choices
                                     where (string)p["Template"] == "DICE/Item Types/CampaignMasterVersio"
                                     select p;

                            foreach (var n in masterChoices)
                            {
                                    
                                        //log(choice["Fields"]);
                                        string Subjectline = (string)n["Fields"]["{7408FE06-C59D-4E89-9B22-B081F4E2A03E}"]["Value"];
                                        var HTMLTemplate = "";//choice["Fields"]["{C69C9078-5AE0-4F56-A81C-2D39CDCFAF15}"]["Value"];
                                        var VersionType = n["Fields"]["{CA490C85-0F9D-48D2-B636-6E0D15ECF59D}"]["Value"];

                                        Subjectline = Subjectline.Replace("'", "''");
                                        string vSQL = @"insert into tbl_campaignchoice (
                                                       [campaignchoiceid]
                                                      ,[campaignid]
                                                      ,[defaultchoice]
                                                      ,[subjectline]
                                                      ,[htmltemplate]) values ('{0}','{1}','{2}','{3}','{4}');";
                                        vSQL = String.Format(vSQL, VersionType, campaignName, "", Subjectline, HTMLTemplate);
                                        SQL += vSQL + "\n";
                                        log(">>>CampaignMasterVersion:" + Subjectline);
                            }

                            var FranchiseStores =
                                     from p in choices
                                     where (string)p["Template"] == "DICE/Item Types/FranchiseStore"
                                     select p;

                            foreach (var choice in FranchiseStores)
                            {
                                string fpath = (string)choice["Path"];

                                string choiceName = (string)choice["DisplayName"];

                                fpath = fpath.Replace(campaignName, "%23" + campaignName + "%23").Replace(choiceName, "%23" + choiceName + "%23");

                                log(">>>FranchiseStores:" + choiceName);
                                var vstores = getNode(fpath + "/*");

                                if (vstores != null)
                                {

                                    foreach (var sc in vstores)
                                    {
                                        string versionid = (string)sc["Fields"]["{9D9BDE7A-8419-46A7-A9BC-80632D855897}"]["Value"];
                                        string storeid = (string)sc["Fields"]["{B12CE01C-2632-4C7F-9FDA-7DE3A71A8493}"]["Value"];
                                        
                                        var version = getNode(versionid, true);
                                        var store = getNode(storeid, true);
                                        //tbl_campaignstore

                                        versionid = (string)version[0]["Name"];
                                        storeid = (string)store[0]["Fields"]["{CFA348EF-AA46-48D7-918F-0C4A0CF43C35}"]["Value"];

                                        string scSQL = "insert into tbl_campaignstore values ('{0}', '{1}', '{2}', '{3}', '{4}');";
                                        scSQL = string.Format(scSQL, campaignName + "_" + versionid + "_" + storeid, campaignName, versionid, storeid, "1");

                                        SQL += scSQL + "\n";
                                        log(">>>>FranchiseStores:" + storeid);

                                        string oPath = sc["Path"] + "/*";

                                        var offs = getNode(oPath);

                                        if (offs != null)
                                        {
                                            foreach (var off in offs)
                                            {
                                                var OfferCode = off["Fields"]["{2622BBD8-AB86-41BB-B1C0-D9EBC94EFFF7}"]["Value"];
                                                var OfferNode = getNode((string)OfferCode, true);
                                                var voucherCode = OfferNode[0]["Fields"]["{0D4F3F52-6143-474C-A56C-CF903D5D0367}"]["Value"];
                                                string oSQL = "insert into tbl_campaignstoreoffers values ('{0}', '{1}', '{2}');";
                                                oSQL = string.Format(oSQL, campaignName, storeid, voucherCode);
                                                SQL += oSQL + "\n";
                                                log(">>>>>Custom Offer:" + voucherCode);

                                            }
                                        }
                                        else
                                        {
                                            foreach (string off in defaultOffers)
                                            {
                                                string oSQL = "insert into tbl_campaignstoreoffers values ('{0}', '{1}', '{2}');";
                                                oSQL = string.Format(oSQL, campaignName, storeid, off);
                                                SQL += oSQL + "\n";
                                                log(">>>>>Default Offer:" + off);
                                            }
                                        }
                                    }
                                }


                            }

 

                        }
                    }

                }
                #endregion

                //System.IO.File.WriteAllText("C://db/text.txt", SQL);
            }
            saveToDB(SQL);
        }
        public JToken getNode(string path, bool byGUID = false)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            string sitecoreURL = System.Configuration.ConfigurationManager.AppSettings["sitecoreURL"].ToString(); //"";

            
            string s;
            if (byGUID)
            {
                s = wc.DownloadString(sitecoreURL + "/-/item/v1/?sc_itemid=" + path);
            }
            else
            {
                s = wc.DownloadString(sitecoreURL + "/-/item/v1/?query=" + path);
            }

            try
            {
                JObject jo = JObject.Parse(s);
                return jo["result"]["items"];
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public void saveToDB(string SQL)
        {
            try
            {



                string connectionStr = System.Configuration.ConfigurationManager.ConnectionStrings["sitecore"].ConnectionString;
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "use DICE_Statistics;\nexec sp_remove;\n" + SQL;
                SqlConnection cnn = new SqlConnection();
                cnn.ConnectionString = connectionStr;
                cnn.Open();
                cmd.Connection = cnn;
                cmd.ExecuteNonQuery();
                cnn.Close();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("./error.log", ex.Message);

            }
        }

    }
}
