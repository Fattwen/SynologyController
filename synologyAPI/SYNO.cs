using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using RestSharp;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;

namespace synologyAPI
{
    public class SYNO
    {
        //When you login successfullys,this variable will keep your host_ip_address and used in future request.
        private string _server_ip_addr;
        private string _sid;
        public bool _login_error = false;
        public string _login_error_msg = string.Empty;
        //使用者預設路徑字典?
        //路徑變數?

        //error code mapping dict.
        private readonly Dictionary<int, string> _ErrorCodeMapping;

        //__Constructor()
        public SYNO()
        {
            _ErrorCodeMapping = new Dictionary<int, string>();
            _ErrorCodeMapping.Add(100, "Unknown error");
            _ErrorCodeMapping.Add(101, "No parameter of API, method or version");
            _ErrorCodeMapping.Add(102, "The requested API does not exist");
            _ErrorCodeMapping.Add(103, "The requested method does not exist");
            _ErrorCodeMapping.Add(104, "The requested version does not support the functionality");
            _ErrorCodeMapping.Add(105, "The logged in session does not have permission");
            _ErrorCodeMapping.Add(106, "Session timeout");
            _ErrorCodeMapping.Add(107, "Session interrupted by duplicate login");
            _ErrorCodeMapping.Add(400, "Invalid parameter of file operation");
            _ErrorCodeMapping.Add(401, "Unknown error of file operation");
            _ErrorCodeMapping.Add(402, "System is too busy");
            _ErrorCodeMapping.Add(403, "Invalid user does this file operation");
            _ErrorCodeMapping.Add(404, "Invalid group does this file operation");
            _ErrorCodeMapping.Add(405, "Invalid user and group does this file operation");
            _ErrorCodeMapping.Add(406, @"Can’t get user/group information from the account server");
            _ErrorCodeMapping.Add(407, "Operation not permitted");
            _ErrorCodeMapping.Add(408, "No such file or directory");
            _ErrorCodeMapping.Add(409, "Non-supported file system");
            _ErrorCodeMapping.Add(410, "Failed to connect internet-based file system (ex: CIFS)");
            _ErrorCodeMapping.Add(411, "Read-only file system");
            _ErrorCodeMapping.Add(412, "Filename too long in the non-encrypted file system");
            _ErrorCodeMapping.Add(413, "Filename too long in the encrypted file system");
            _ErrorCodeMapping.Add(414, "File already exists");
            _ErrorCodeMapping.Add(415, "Disk quota exceeded");
            _ErrorCodeMapping.Add(416, "No space left on device");
            _ErrorCodeMapping.Add(417, "Input/output error");
            _ErrorCodeMapping.Add(418, "Illegal name or path");
            _ErrorCodeMapping.Add(419, "Illegal file name");
            _ErrorCodeMapping.Add(420, "Illegal file name on FAT file system");
            _ErrorCodeMapping.Add(421, "Device or resource busy");
            _ErrorCodeMapping.Add(599, "No such task of the file operation");
            _ErrorCodeMapping.Add(800, "A folder path of favorite folder is already added to user’s favorites.");
            _ErrorCodeMapping.Add(801, "A name of favorite folder conflicts with an existing folder path in the user’s favorites.");
            _ErrorCodeMapping.Add(802, "There are too many favorites to be added.");
            _ErrorCodeMapping.Add(900, "Failed to delete file(s)/folder(s). More information in <errors> object.");
            _ErrorCodeMapping.Add(1000, "Failed to copy files/folders. More information in <errors> object.");
            _ErrorCodeMapping.Add(1001, "Failed to move files/folders. More information in <errors> object.");
            _ErrorCodeMapping.Add(1002, "An error occurred at the destination. More information in <errors> object.");
            _ErrorCodeMapping.Add(1003, "Cannot overwrite or skip the existing file because no overwrite parameter is given.");
            _ErrorCodeMapping.Add(1004, "File cannot overwrite a folder with the same name, or folder cannot overwrite a file with the same name.");
            _ErrorCodeMapping.Add(1006, "Cannot copy/move file/folder with special characters to a FAT32 file system.");
            _ErrorCodeMapping.Add(1007, "Cannot copy/move a file bigger than 4G to a FAT32 file system.");
            _ErrorCodeMapping.Add(1100, "Failed to create a folder. More information in <errors> object.");
            _ErrorCodeMapping.Add(1101, "The number of folders to the parent folder would exceed the system limitation.");
            _ErrorCodeMapping.Add(1200, "Failed to rename it. More information in <errors> object.");
            _ErrorCodeMapping.Add(1300, "Failed to compress files/folders.");
            _ErrorCodeMapping.Add(1301, "Cannot create the archive because the given archive name is too long.");
            _ErrorCodeMapping.Add(1400, "Failed to extract files.");
            _ErrorCodeMapping.Add(1401, "Cannot open the file as archive.");
            _ErrorCodeMapping.Add(1402, "Failed to read archive data error.");
            _ErrorCodeMapping.Add(1403, "Wrong password.");
            _ErrorCodeMapping.Add(1404, "Failed to get the file and dir list in an archive.");
            _ErrorCodeMapping.Add(1405, "Failed to find the item ID in an archive file.");
            _ErrorCodeMapping.Add(1800, "There is no Content-Length information in the HTTP header or the received size doesn’t match the value of Content-Length information in the HTTP header.");
            _ErrorCodeMapping.Add(1801, "Wait too long, no date can be received from client (Default maximum wait time is 3600 seconds).");
            _ErrorCodeMapping.Add(1802, "No filename information in the last part of file content.");
            _ErrorCodeMapping.Add(1803, "Upload connection is cancelled.");
            _ErrorCodeMapping.Add(1804, "Failed to upload too big file to FAT file system.");
            _ErrorCodeMapping.Add(1805, "Can’t overwrite or skip the existed file, if no overwrite parameter is given.");
            _ErrorCodeMapping.Add(2000, "Sharing link does not exist.");
            _ErrorCodeMapping.Add(2001, "Cannot generate sharing link because too many sharing links exist.");
            _ErrorCodeMapping.Add(2002, "Failed to access sharing links.");

        }

        //error code mapping
        private string Mapping(int ErrorCode) 
        {
            if (_ErrorCodeMapping.ContainsKey(ErrorCode))
            {
                return $"{ErrorCode.ToString()}->{_ErrorCodeMapping[ErrorCode]}";
            }
            else
                return $"[Unknown]{ErrorCode.ToString()}->this Error Code is not defined!";
        }

        //login and return session id or cookie
        public string Login(string server,string acct, string pswd)
        {
            _server_ip_addr = server;
            string reqUrl = $@"http://{server}/webapi/auth.cgi?api=SYNO.API.Auth&version=6&method=login";
            reqUrl += $"&account={acct}";
            reqUrl += $"&passwd={pswd}";
            //or set cookie
            //reqUrl += $"&format=cookie";
            reqUrl += $"&format=sid";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reqUrl);
            request.Method = "GET";
            request.KeepAlive = true;
            request.ContentType = "text/plain";
            WebResponse wr = request.GetResponse();
            var reader = new StreamReader(wr.GetResponseStream());
            var temp = reader.ReadToEnd();
            wr.Close();
            reader.Close();
            //dynamic Deserialize json
            dynamic json = JsonConvert.DeserializeObject<dynamic>(temp);
            //Root json = JsonConvert.DeserializeObject<Root>(temp);
            //success -> return true , else -> false
            if ((bool)json.success)
            {
                _login_error = false;
                _login_error_msg = string.Empty;
                _sid = json.data.sid;
                return json.data.sid;
            }
            else 
            {
                _login_error = true;
                _login_error_msg = Mapping(json.error.code.Value<int>());
                return Mapping(json.error.code.Value<int>());
            }
        }

        //get all folder(name) list
        public ArrayList GetList()
        {
            ArrayList folder = new ArrayList();
            if (_login_error)
            {
                folder.Add("Login Error");
                return folder;
            }
            string reqUrl = $@"http://{_server_ip_addr}/webapi/entry.cgi?api=SYNO.FileStation.List&version=2&method=list_share";
            reqUrl += $"&_sid={_sid}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(reqUrl);
            request.Method = "GET";
            request.KeepAlive = true;
            request.ContentType = "text/plain";
            WebResponse wr = request.GetResponse();
            var reader = new StreamReader(wr.GetResponseStream());
            var temp = reader.ReadToEnd();
            wr.Close();
            reader.Close();
            dynamic json = JsonConvert.DeserializeObject<dynamic>(temp);
            //Root json = JsonConvert.DeserializeObject<Root>(temp);
            
            if ((bool)json.success)
            {

                foreach (var i in json.data.shares)
                {
                    //Console.WriteLine(i.name);
                    folder.Add(i.name.ToString());
                }
                return folder;
            }
            else
            {
                folder.Add(Mapping(json.error.code.Value<int>()));
                return folder;
            }
        }

        //upload local file By Restsharp
        public string UploadFile(string dest_path,string file,bool create_parents=false)
        {
            if (_login_error)
            {
                return "Login Error";
            }
            string reqUrl = $@"http://{_server_ip_addr}/webapi/entry.cgi?api=SYNO.FileStation.Upload&version=3&method=upload";
            reqUrl += $"&_sid={_sid}";
            var client = new RestClient(reqUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddParameter("path", $@"{dest_path}");
            request.AddParameter("create_parents", $"{create_parents.ToString().ToLower()}");
            request.AddFile("filename", $@"{file}");
            try
            {
                IRestResponse response = client.Execute(request);
                dynamic json = JsonConvert.DeserializeObject<dynamic>(response.Content);
                string report = (bool)json.success ? json.success.ToString() : Mapping(json.error.code.Value<int>());
                return report;
            }
            catch (Exception excp)
            {
                return excp.Message;
            }
        }

        //move filestation's file By Restsharp
        public string MoveFile(string file,string dest_path,bool overwrite=false) 
        {
            if (_login_error)
            {
                return "Login Error";
            }
            string reqUrl = $@"http://{_server_ip_addr}/webapi/entry.cgi?api=SYNO.FileStation.CopyMove&version=3&method=start";
            reqUrl += $@"&path={file}&dest_folder_path={dest_path}&remove_src=true&overwrite={overwrite.ToString().ToLower()}&_sid={_sid}";
            //remove_src = true
            var client = new RestClient(reqUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            //request.AddHeader("Cookie", $"smid={sid}");
            IRestResponse response = client.Execute(request);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(response.Content);
            string report = (bool)json.success ? json.success.ToString() : Mapping(json.error.code.Value<int>());
            return report;
        }

        //copy filestation's file By Restsharp
        public string CopyFile(string file, string dest_path, bool overwrite=false) 
        {
            if (_login_error)
            {
                return "Login Error";
            }
            //remove_src = false
            string reqUrl = $@"http://{_server_ip_addr}/webapi/entry.cgi?api=SYNO.FileStation.CopyMove&version=3&method=start";
            reqUrl += $@"&path={file}&dest_folder_path={dest_path}&remove_src=false&overwrite={overwrite.ToString().ToLower()}&_sid={_sid}";
            //remove_src = true
            var client = new RestClient(reqUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            //request.AddHeader("Cookie", $"smid={sid}");
            IRestResponse response = client.Execute(request);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(response.Content);
            string report = (bool)json.success ? json.success.ToString() : Mapping(json.error.code.Value<int>());
            return report;
        }

        //rename file's name
        public string RenameFile(string path,string name)
        {
            if (_login_error)
            {
                return "Login Error";
            }
            string reqUrl = $@"http://{_server_ip_addr}/webapi/entry.cgi?api=SYNO.FileStation.Rename&method=rename&version=2"; 
            reqUrl += $@"&path={path}&name={name}&_sid={_sid}";
            
            var client = new RestClient(reqUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(response.Content);
            string report = (bool)json.success ? json.success.ToString() : Mapping(json.error.code.Value<int>());
            //string report = json.error.code;
            return report;
        }

        public string RemoveFile()
        {
            return "not set yet";
        }
        //logout and release session id
        public bool Logout() 
        {
            var client = new RestClient($@"http://{_server_ip_addr}/webapi/auth.cgi?api=SYNO.API.Auth&version=6&method=logout&_sid={_sid}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            dynamic json = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return (bool)json.success;
        }

        /*
         * SYNO.FileStation.CheckPermission
         * SYNO.FileStation.BackgroundTask
        */
        
    }
}
