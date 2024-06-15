using ClockTransactionsTransmiter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockTransactionsTransmiter.ViewModels
{
    public class RecordSearchViewModel : BaseViewModel
    {
        private int pageIndex = 1;
        public int PageIndex
        {
            get
            {
                return pageIndex;
            }
            set
            {
                pageIndex = value;
                OnPropertyChanged(nameof(PageIndex));
            }
        }

        private int maxPage = 1;
        public int MaxPage
        {
            get
            {
                return maxPage;
            }
            set
            {
                maxPage = value;
                OnPropertyChanged(nameof(MaxPage));
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

        private string startTime;
        public string StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }

        private string endTime;
        public string EndTime
        {
            get
            {
                return endTime;
            }
            set
            {
                endTime = value;
                OnPropertyChanged(nameof(EndTime));
            }
        }
    }
}
