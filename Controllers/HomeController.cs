using ClientMVC.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Stripe;
using System.Threading.Tasks;
using ClientMVC.Helpers;
using System.Net;
using System.IO;
using System.Xml;
using MySql.Data.MySqlClient;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ClientMVC.Controllers
{
    public class HomeController : Controller
    {
        private string apiKey = "sk_test_zH5WN0BSq1YdwtFYCk8QR5r6";
        private string encCypher = "zH5WN0BSq1YdwtFYCk8QR5r6";
        private string server;
        private string database;
        private string uid;
        private string password;
        private MySql.Data.MySqlClient.MySqlConnection connection;

        // Begin database connectivity methods
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                return false;
            }
        }
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                return false;
            }
        }
        private void initilize()
        {
            server = "bravodbinstance.c7nqnuxewgdw.us-west-2.rds.amazonaws.com";
            database = "demo";
            uid = "root";
            password = "charan92";
            string connectionString;
            connectionString = "Server = " + server + "; Database = " + database + "; Uid = " + uid + "; Pwd = " + password;

            connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
        }
        // End database connectivity methods

        public HomeController()
        {
            // In the constructor, database connection is initialized
            initilize();
        }

        // GET: Transactions
        public ActionResult Transactions()
        {
            // Checking if the session is valid.
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            // Getting the current user from the session
            User currentUser = Session["user"] as User;

            // Fetching a list of all transactions associated with the user
            List<string[]> transactions = getTransactions(currentUser.id);
            // Storing the list of transactions in session
            Session["transactions"] = transactions;
            return View();
        }

        // GET: Home
        public ActionResult Index()
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login","Login");
            }
            User curentUser = Session["user"] as User;
            Session["apiKey"] = this.apiKey;
            DBTools dbtools = new DBTools();
            curentUser.name = dbtools.GetName(curentUser.id);
            Session["user"] = curentUser;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(string country, string fullname, string card, string cvc, string expyear, string expmonth, string billingaddline1, string amount, string billingaddline2, string city, string state, string zip)
        {
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            User currentUser = Session["user"] as User;
            try {
                Session["final"] = "";
                // This method generates a token by calling Stripe endpoint
                string token = await GetTokenId(country, fullname, card, cvc, expyear, expmonth, billingaddline1, amount, billingaddline2, city, state, zip);
                string res = string.Empty;
                Uri baseUri = new Uri("http://mainservice.apphb.com/Service1.svc");
                UriTemplate myTemplate = new UriTemplate("{id}/{token}/{amt}");
                Uri mainUri = myTemplate.BindByPosition(baseUri, "user1@test.com", token, amount);
                string url = mainUri.AbsoluteUri;
                try
                {

                    WebRequest httpRequest = WebRequest.Create(url);    // creating a http request

                    httpRequest.Method = "GET";
                    HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();  // getting http response

                    if (httpResponse.StatusCode == HttpStatusCode.OK)   // Checking if response is OK
                    {
                        Stream responseStream = httpResponse.GetResponseStream();
                        StreamReader streamReaderObj = new StreamReader(responseStream, Encoding.UTF8);
                        XmlDocument xmlDoc = new XmlDocument();     // Creating a new xml document
                        xmlDoc.LoadXml(streamReaderObj.ReadToEnd());    // Loading the xml document

                        XmlNodeList xmlNodeList = xmlDoc.SelectNodes("//*");    // selecting all nodes from xml document

                       
                        XmlNode xmlNode = xmlNodeList[0];
                        string resp = "\n"+xmlNode.InnerText;

                        var jsonReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(resp), new System.Xml.XmlDictionaryReaderQuotas());
                        var root = XElement.Load(jsonReader);
                        string color = "red";
                        string msg = root.XPathSelectElement("//message").Value;
                        string tid = root.XPathSelectElement("//transactionID").Value;
                        if (msg.Equals("succeeded"))
                        {
                            res = "Transaction successful! Please note the transaction ID: " + tid;
                            color = "green";
                        }
                        else
                        {
                            res = "Transaction failed! Please note the transaction ID: " + tid;
                        }
                        Session["final"] = res;
                        Session["color"] = color;

                    }
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                }
                return View();
            }
            catch(Stripe.StripeException se)
            {
                Session["error"] = se.Message;
            }
            catch(Exception ex)
            {
                Session["error"] = ex.Message;
                return View();
            }
            
            
            return View();
        }

        private static async Task<string> GetTokenId(string country, string fullname, string card, string cvc, string expyear, string expmonth, string billingaddline1, string amount, string billingaddline2, string city, string state, string zip)
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                var myToken = new StripeTokenCreateOptions();
               // myToken.CustomerId = id;
                myToken.Card = new StripeCreditCardOptions()
                {
                    AddressZip = zip,
                    AddressCity = city,
                    AddressCountry = country,
                    AddressLine1 = billingaddline1,
                    AddressLine2 = billingaddline2,
                    AddressState = state,
                    Cvc = cvc,
                    ExpirationMonth = expmonth,
                    ExpirationYear = expyear,
                    Name = fullname,
                    Number = card
                };
               
                AppSettingsReader reader = new AppSettingsReader();
                var tokenService = new StripeTokenService(reader.GetValue("StripeApiKey", typeof(string)).ToString());
                var stripeToken = tokenService.Create(myToken);
                
                return stripeToken.Id;
            });
        }

        private string encrypt(string toEncrypt,bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
            // Get the key from config file

            string key = encCypher;
            //System.Windows.Forms.MessageBox.Show(key);
            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //Always release the resources and flush data
                //of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray = cTransform.TransformFinalBlock
                    (toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);


        }

        private List<string[]> getTransactions(string id)
        {
            List<string[]> res = new List<string[]>();
            string query = "SELECT tid,amount,state,date FROM TRANSACTIONS WHERE uid = " + "'" + id + "'";
            if (this.OpenConnection() == true)
            {

                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    string tid = dataReader["tid"].ToString();
                    string amount = dataReader["amount"].ToString();
                    string state = dataReader["state"].ToString();
                    string date = dataReader["date"].ToString();

                    string[] row = { tid, amount, state, date };
                    res.Add(row);
                }
                this.CloseConnection();
            }

            return res;
        }

    }
}