using ClockTransactionsTransmiter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockTransactionsTransmiter.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private uint servicePort = 5010u;
        public uint ServicePort
        {
            get
            {
                return servicePort;
            }
            set
            {
                servicePort = value;
                OnPropertyChanged(nameof(ServicePort));
            }
        }

        private string authenticationName = "admin";
        public string AuthenticationName
        {
            get
            {
                return authenticationName;
            }
            set
            {
                authenticationName = value;
                OnPropertyChanged(nameof(AuthenticationName));
            }
        }

        private string authenticationPassword = "12345";
        public string AuthenticationPassword
        {
            get
            {
                return authenticationPassword;
            }
            set
            {
                authenticationPassword = value;
                OnPropertyChanged(nameof(AuthenticationPassword));
            }
        }

        private int minIntervalToGetResults = 1;
        public int MinIntervalToGetResults
        {
            get
            {
                return minIntervalToGetResults;
            }
            set
            {
                minIntervalToGetResults = value;
                OnPropertyChanged(nameof(MinIntervalToGetResults));
            }
        }

        private int minIntervalToUploadRecords = 1;
        public int MinIntervalToUploadRecords
        {
            get
            {
                return minIntervalToUploadRecords;
            }
            set
            {
                minIntervalToUploadRecords = value;
                OnPropertyChanged(nameof(MinIntervalToUploadRecords));
            }
        }

        private string uploadRecordsApiUrl = "http://";
        public string UploadRecordsApiUrl
        {
            get
            {
                return uploadRecordsApiUrl;
            }
            set
            {
                uploadRecordsApiUrl = value;
                OnPropertyChanged(nameof(UploadRecordsApiUrl));
            }
        }

        private string bootargsFilePath = "*.bootargs";
        public string BootargsFilePath
        {
            get
            {
                return bootargsFilePath;
            }
            set
            {
                bootargsFilePath = value;
                OnPropertyChanged(nameof(BootargsFilePath));
            }
        }

        public string Ips
        {
            get
            {
                return string.Join(",", ipList);
            }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                ipList.AddRange(value.Split(','));
            }
        }
        private List<string> ipList = new List<string>();
        public void AddIP(string ip)
        {
            if (!ipList.Contains(ip))
            {
                ipList.Add(ip);
            }
        }
        public List<string> GetIpList()
        {
            return ipList;
        }
        public bool CheckIp(string ip)
        {
            return ipList.Contains(ip);
        }
    }
}