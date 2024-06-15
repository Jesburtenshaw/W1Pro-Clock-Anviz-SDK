using ClockTransactionsTransmiter.ViewModels;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockTransactionsTransmiter.ViewModels
{
    [SugarTable("Employees")]
    public class EmployeeViewModel : BaseViewModel
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

        private int index;
        [SugarColumn(IsIgnore = true)]
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

        private string employeeName;
        [SugarColumn(ColumnName = "EmployeeName")]
        public string EmployeeName
        {
            get
            {
                return employeeName;
            }
            set
            {
                employeeName = value;
                OnPropertyChanged(nameof(EmployeeName));
            }
        }
    }
}
