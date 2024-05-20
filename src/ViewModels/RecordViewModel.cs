using ClockTransactionsTransmiter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockTransactionsTransmiter.ViewModels
{
    public class RecordViewModel : BaseViewModel
    {
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

        private uint index;
        public uint Index
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

        private string employeeId;
        public string EmployeeId
        {
            get
            {
                return employeeId;
            }
            set
            {
                employeeId = value;
                OnPropertyChanged(nameof(EmployeeId));
            }
        }

        private string time;
        public string Time
        {
            get
            {
                return time;
            }
            set
            {
                time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        private string workType;
        public string WorkType
        {
            get
            {
                return workType;
            }
            set
            {
                workType = value;
                OnPropertyChanged(nameof(WorkType));
            }
        }

        private string recordType;
        public string RecordType
        {
            get
            {
                return recordType;
            }
            set
            {
                recordType = value;
                OnPropertyChanged(nameof(RecordType));
            }
        }
    }
}
