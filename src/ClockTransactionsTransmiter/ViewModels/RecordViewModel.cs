using ClockTransactionsTransmiter.ViewModels;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockTransactionsTransmiter.ViewModels
{
    [SugarTable("Records")]
    public class RecordViewModel : BaseViewModel
    {
        [SugarColumn(ColumnName = "Id", IsPrimaryKey = true)]
        public int? Id { get; set; }

        private string machineId; 
        [SugarColumn(ColumnName = "MachineId")]
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
        [SugarColumn(IsIgnore = true)]
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
        [SugarColumn(ColumnName = "EmployeeId")]
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
        [SugarColumn(ColumnName = "Time")]
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
        [SugarColumn(ColumnName = "WorkType")]
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
        [SugarColumn(ColumnName = "RecordType")]
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
