using ClockTransactionsTransmiter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockTransactionsTransmiter.ViewModels
{
    public class DeviceViewModel: BaseViewModel
    {
        private int index;
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
                OnPropertyChanged(nameof(Index));
            }
        }

        private string machineId;
        public string MachineId
        {
            get
            {
                return machineId;
            }
            set
            {
                machineId = value;
                OnPropertyChanged(nameof(MachineId));
            }
        }

        private bool connected;
        public bool Conencted
        {
            get
            {
                return connected;
            }
            set
            {
                connected = value;
                OnPropertyChanged(nameof(Conencted));
            }
        }

        private string devSerialNum;
        public string DevSerialNum
        {
            get
            {
                return devSerialNum;
            }
            set
            {
                devSerialNum = value;
                OnPropertyChanged(nameof(DevSerialNum));
            }
        }

        private string ipAddr;
        public string IpAddr
        {
            get
            {
                return ipAddr;
            }
            set
            {
                ipAddr = value;
                OnPropertyChanged(nameof(IpAddr));
            }
        }

        private string macAddr;
        public string MacAddr
        {
            get
            {
                return macAddr;
            }
            set
            {
                macAddr = value;
                OnPropertyChanged(nameof(MacAddr));
            }
        }
    }
}
