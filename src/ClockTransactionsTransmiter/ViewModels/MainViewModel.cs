using ClockTransactionsTransmiter.Devices;
using ClockTransactionsTransmiter.Helper;
using CsvHelper;
using PQWorld.BLL.Implement.SqlSugar;
using SqlSugar.Extensions;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows;

namespace ClockTransactionsTransmiter.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private bool set = false;
        private bool exit = false;
        public IntPtr anviz_handle = IntPtr.Zero;
        public CancellationTokenSource cts_get_results = null;
        private const int authenticationLength = 12;
        private const int pageSize = 20;
        private const int bufferLength = 32000;
        private bool employeesInited = false;
        private bool recordsInited = false;
        private string settingsCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.csv");
        private string recordsCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Records.csv");
        private string pendingRecordsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PendingRecords");
        private string employeesCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Employees.csv");     
        private Queue<RecordViewModel> qRecords = new Queue<RecordViewModel>();
        private Queue<EmployeeViewModel> qEmployees = new Queue<EmployeeViewModel>();
        private Dictionary<int, int> dictDevTypeFlag = new Dictionary<int, int>();// 0x10:DR, 0x400:msg content 200 and unicode

        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        private bool success;
        public bool Success
        {
            get
            {
                return success;
            }
            set
            {
                success = value;
                OnPropertyChanged(nameof(Success));
            }
        }

        private bool ready;
        public bool Ready
        {
            get
            {
                return ready;
            }
            set
            {
                ready = value;
                OnPropertyChanged(nameof(Ready));
            }
        }

        private string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private bool showSettings;
        public bool ShowSettings
        {
            get
            {
                return showSettings;
            }
            set
            {
                showSettings = value;
                OnPropertyChanged(nameof(ShowSettings));
            }
        }

        private bool showDevices;
        public bool ShowDevices
        {
            get
            {
                return showDevices;
            }
            set
            {
                showDevices = value;
                OnPropertyChanged(nameof(ShowDevices));
            }
        }

        private bool showRecords;
        public bool ShowRecords
        {
            get
            {
                return showRecords;
            }
            set
            {
                showRecords = value;
                OnPropertyChanged(nameof(ShowRecords));
            }
        }

        private bool showEmployees;
        public bool ShowEmployees
        {
            get
            {
                return showEmployees;
            }
            set
            {
                showEmployees = value;
                OnPropertyChanged(nameof(ShowEmployees));
            }
        }

        private bool showMain;
        public bool ShowMain
        {
            get
            {
                return showMain;
            }
            set
            {
                showMain = value;
                OnPropertyChanged(nameof(ShowMain));
            }
        }

        private bool hasPrev;
        public bool HasPrev
        {
            get
            {
                return hasPrev;
            }
            set
            {
                hasPrev = value;
                OnPropertyChanged(nameof(HasPrev));
            }
        }

        private bool hasNext;
        public bool HasNext
        {
            get
            {
                return hasNext;
            }
            set
            {
                hasNext = value;
                OnPropertyChanged(nameof(HasNext));
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

        private SettingsViewModel editingSettings;
        public SettingsViewModel EditingSettings
        {
            get
            {
                return editingSettings;
            }
            set
            {
                editingSettings = value;
                OnPropertyChanged(nameof(EditingSettings));
            }
        }

        private SettingsViewModel curSettings;
        public SettingsViewModel CurSettings
        {
            get
            {
                return curSettings;
            }
            set
            {
                curSettings = value;
                OnPropertyChanged(nameof(CurSettings));
            }
        }

        private EmployeeSearchViewModel curEmployeeSearch;
        public EmployeeSearchViewModel CurEmployeeSearch
        {
            get
            {
                return curEmployeeSearch;
            }
            set
            {
                curEmployeeSearch = value;
                OnPropertyChanged(nameof(CurEmployeeSearch));
            }
        }

        private RecordSearchViewModel curRecordSearch;
        public RecordSearchViewModel CurRecordSearch
        {
            get
            {
                return curRecordSearch;
            }
            set
            {
                curRecordSearch = value;
                OnPropertyChanged(nameof(CurRecordSearch));
            }
        }

        private ObservableCollection<DeviceViewModel> devices;
        public ObservableCollection<DeviceViewModel> Devices
        {
            get
            {
                return devices;
            }
            set
            {
                devices = value;
                OnPropertyChanged(nameof(Devices));
            }
        }

        private ObservableCollection<EmployeeViewModel> employees;
        public ObservableCollection<EmployeeViewModel> Employees
        {
            get
            {
                return employees;
            }
            set
            {
                employees = value;
                OnPropertyChanged(nameof(Employees));
            }
        }

        private ObservableCollection<RecordViewModel> records;
        public ObservableCollection<RecordViewModel> Records
        {
            get
            {
                return records;
            }
            set
            {
                records = value;
                OnPropertyChanged(nameof(Records));
            }
        }

        public RelayCommand ScanCommand { get; set; }
        public RelayCommand SwitchUICommand { get; set; }
        public RelayCommand SwitchDataCommand { get; set; }
        public RelayCommand SaveSettingsCommand { get; set; }
        
        public IotHubPublisher IothubPublisher { get; set; }

        public MainViewModel()
        {
            ScanCommand = new RelayCommand(Scan);
            SwitchUICommand = new RelayCommand(SwitchUI);
            SwitchDataCommand = new RelayCommand(SwitchData);
            SaveSettingsCommand = new RelayCommand(SaveSettings);
            ShowMain = true;
            EditingSettings = new SettingsViewModel();
            CurSettings = new SettingsViewModel();
            CurEmployeeSearch = new EmployeeSearchViewModel();
            CurRecordSearch = new RecordSearchViewModel();
            Devices = new ObservableCollection<DeviceViewModel>();
            Employees = new ObservableCollection<EmployeeViewModel>();
            Records = new ObservableCollection<RecordViewModel>();
            IothubPublisher = new IotHubPublisher();
        }

        private void SaveSettings(object sender)
        {
            if (EditingSettings.ServicePort < 0u)
            {
                Status = "Please type a valid Service Port.";
                return;
            }
            if (EditingSettings.MinIntervalToGetResults <= 0u)
            {
                Status = "Please type a valid Minutes Interval to Get Records.";
                return;
            }
            if (EditingSettings.MinIntervalToUploadRecords <= 0u)
            {
                Status = "Please type a valid Minutes Interval to Upload Records.";
                return;
            }
            if (string.IsNullOrEmpty(EditingSettings.UploadRecordsApiUrl))
            {
                Status = "Please type a valid Api Url to Upload Records.";
                return;
            }

            Ready = false;
            int ret = 1;
            if (EditingSettings.AuthenticationName != CurSettings.AuthenticationName ||
                EditingSettings.AuthenticationPassword != CurSettings.AuthenticationPassword)
            {
                if (!string.IsNullOrEmpty(EditingSettings.AuthenticationName) && !string.IsNullOrEmpty(EditingSettings.AuthenticationPassword))
                {
                    AnvizNew.CCHEX_CONNECTION_AUTHENTICATION_STRU param;
                    byte[] bytes = BytesStringHelper.Encoding.GetBytes(EditingSettings.AuthenticationName);
                    param.username = new byte[authenticationLength];
                    Array.Copy(bytes, param.username, bytes.Length > authenticationLength ? authenticationLength : bytes.Length);
                    param.password = new byte[authenticationLength];
                    bytes = BytesStringHelper.Encoding.GetBytes(EditingSettings.AuthenticationPassword);
                    Array.Copy(bytes, param.password, bytes.Length > authenticationLength ? authenticationLength : bytes.Length);

                    IntPtr ptrParam = Marshal.AllocHGlobal(Marshal.SizeOf(param));
                    Marshal.StructureToPtr(param, ptrParam, false);

                    ret = AnvizNew.CChex_Set_Connect_Authentication(anviz_handle, ptrParam);
                    Marshal.FreeHGlobal(ptrParam);
                }
                else
                {
                    ret = AnvizNew.CChex_Set_Connect_Authentication(anviz_handle, IntPtr.Zero);
                }
                if (ret != 1)
                {
                    Ready = true;
                    Status = $"Couldn't authenticate, error code: {ret}.";
                    return;
                }
            }

            CurSettings.ServicePort = EditingSettings.ServicePort;
            CurSettings.AuthenticationName = EditingSettings.AuthenticationName;
            CurSettings.AuthenticationPassword = EditingSettings.AuthenticationPassword;
            CurSettings.MinIntervalToGetResults = EditingSettings.MinIntervalToGetResults;
            CurSettings.MinIntervalToUploadRecords = EditingSettings.MinIntervalToUploadRecords;
            CurSettings.UploadRecordsApiUrl = EditingSettings.UploadRecordsApiUrl;

            var curBootargsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(EditingSettings.BootargsFilePath));
            if (EditingSettings.BootargsFilePath != CurSettings.BootargsFilePath &&
                EditingSettings.BootargsFilePath != curBootargsFilePath &&
                EditingSettings.BootargsFilePath.Equals("*.bootargs"))
            {
                EnumerationOptions enumerationOptions = new EnumerationOptions();
                enumerationOptions.MatchCasing = MatchCasing.CaseInsensitive;
                enumerationOptions.RecurseSubdirectories = false;
                var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.bootargs", enumerationOptions);
                foreach (var file in files)
                {
                    File.Move(file, file.Replace(".bootargs", ".bootargs.bak"), true);
                }

                CurSettings.BootargsFilePath = EditingSettings.BootargsFilePath;
                File.Copy(CurSettings.BootargsFilePath, curBootargsFilePath, true);
            }

            Task.Run(() =>
            {
                using (var writer = new StreamWriter(settingsCsvPath))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(new List<SettingsViewModel> { CurSettings });
                    }
                }

                if (!set)
                {
                    set = !set;
                    DoScan(sender);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Ready = true;
                        Status = "Settings saved. Searching required.";
                    });
                }
            });
        }

        private async void SwitchUI(object sender)
        {
            //Status = "";
            var btnCode = sender.ToString();
            Title = btnCode;
            if (btnCode == "Settings")
            {
                ShowSettings = true;
                ShowDevices = false;
                ShowEmployees = false;
                ShowRecords = false;
                ShowMain = false;
            }
            else if (btnCode == "Devices")
            {
                //if (null == Devices || Devices.Count == 0)
                //{
                //    Status = "No devices at the moments.";
                //    return;
                //}
                ShowSettings = false;
                ShowDevices = true;
                ShowEmployees = false;
                ShowRecords = false;
                ShowMain = false;
                PageIndex = 1;
                MaxPage = 1;
                HasPrev = false;
                HasNext = false;
            }
            else if (btnCode == "Employees")
            {
                ShowSettings = false;
                ShowDevices = false;
                ShowEmployees = true;
                ShowRecords = false;
                ShowMain = false;
                SwitchData("");
            }
            else if (btnCode == "Records")
            {
                ShowSettings = false;
                ShowDevices = false;
                ShowEmployees = false;
                ShowRecords = true;
                ShowMain = false;
                SwitchData("");
            }
            else
            {
                ShowSettings = false;
                ShowDevices = false;
                ShowEmployees = false;
                ShowRecords = false;
                ShowMain = true;
            }
        }

        private async void SwitchData(object sender)
        {
            if (!Ready)
            {
                return;
            }
            //Status = "";
            var btnOp = sender.ToString();
            if (Title == "Employees")
            {
                if (!employeesInited)
                {
                    employeesInited = true;
                }
                else
                {
                    if (btnOp == "+" && curEmployeeSearch.PageIndex < curEmployeeSearch.MaxPage)
                    {
                        curEmployeeSearch.PageIndex++;
                    }
                    else if (btnOp == "-" && curEmployeeSearch.PageIndex > 1)
                    {
                        curEmployeeSearch.PageIndex--;
                    }
                }
                var employeeBody = (await EmployeeLogic.Instance.ListAsync(CurEmployeeSearch.PageIndex,
                pageSize,
                CurEmployeeSearch.MachineId,
                CurEmployeeSearch.EmployeeId,
                CurEmployeeSearch.EmployeeName)).Body;
                CurEmployeeSearch.MaxPage = employeeBody.MaxPage;
                Employees.Clear();
                foreach (var employee in employeeBody.Items)
                {
                    Employees.Add(employee);
                }
                PageIndex = curEmployeeSearch.PageIndex;
                MaxPage = curEmployeeSearch.MaxPage;
                HasPrev = curEmployeeSearch.PageIndex > 1;
                HasNext = curEmployeeSearch.PageIndex < curEmployeeSearch.MaxPage;
            }
            else if (Title == "Records")
            {
                if (!recordsInited)
                {
                    recordsInited = true;
                }
                else
                {
                    if (btnOp == "+" && curRecordSearch.PageIndex < curRecordSearch.MaxPage)
                    {
                        curRecordSearch.PageIndex++;
                    }
                    else if (btnOp == "-" && curRecordSearch.PageIndex > 1)
                    {
                        curRecordSearch.PageIndex--;
                    }
                }
                var recordBody = (await RecordLogic.Instance.ListAsync(CurRecordSearch.PageIndex,
                pageSize,
                CurRecordSearch.MachineId,
                CurRecordSearch.EmployeeId,
                CurRecordSearch.StartTime,
                CurRecordSearch.EndTime)).Body;
                CurRecordSearch.MaxPage = recordBody.MaxPage;
                Records.Clear();
                foreach (var record in recordBody.Items)
                {
                    Records.Add(record);
                }
                PageIndex = curRecordSearch.PageIndex;
                MaxPage = curRecordSearch.MaxPage;
                HasPrev = curRecordSearch.PageIndex > 1;
                HasNext = curRecordSearch.PageIndex < curRecordSearch.MaxPage;
            }
        }

        private void DoScan(object sender)
        {
            Task.Run(async () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Ready = false;
                });
                if (!Success)
                {
                    await Start();
                }

                //int ret = AnvizNew.CCHex_Udp_Search_Dev(anviz_handle);
                //if (ret != 1)
                //{
                    await Broadcast();
                //}

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Ready = true;
                    Status = "Searched, please wait for the devices to respond.";
                    //SwitchUI("Devices");
                });
            });
        }
        private void Scan(object sender)
        {
            if (!set)
            {
                Status = "Please modify and save settings first.";
                SwitchUI("Settings");
                return;
            }
            var messageBox = new AdonisUI.Controls.MessageBoxModel
            {
                Text = "Are you sure to search for all the devices in the same LAN again?",
                Caption = "Search",
                Icon = AdonisUI.Controls.MessageBoxImage.Question,
                Buttons = AdonisUI.Controls.MessageBoxButtons.YesNo()
            };
            if (AdonisUI.Controls.MessageBox.Show(messageBox) != AdonisUI.Controls.MessageBoxResult.Yes)
            {
                return;
            }
            DoScan(sender);
        }

        public void ReGenCts()
        {
            cts_get_results?.Cancel();
            cts_get_results = new CancellationTokenSource();
        }
        private void ClearCts()
        {
            cts_get_results?.Cancel();
        }

        public async Task<bool> Start()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Ready = false;
                Success = true;
                Status = "";
            });

            if (!File.Exists(settingsCsvPath))
            {
                File.WriteAllText(settingsCsvPath, null);
            }
            using (var reader = new StreamReader(settingsCsvPath))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var settings = csv.GetRecords<SettingsViewModel>();
                    var setting = settings.FirstOrDefault();
                    if (null != setting)
                    {
                        set = true;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            EditingSettings.AuthenticationName = setting.AuthenticationName;
                            EditingSettings.AuthenticationPassword = setting.AuthenticationPassword;
                            EditingSettings.ServicePort = setting.ServicePort;
                            EditingSettings.MinIntervalToUploadRecords = setting.MinIntervalToUploadRecords;
                            EditingSettings.MinIntervalToUploadRecords = setting.MinIntervalToUploadRecords;
                            EditingSettings.UploadRecordsApiUrl = setting.UploadRecordsApiUrl;
                            EditingSettings.Ips = setting.Ips;
                            EditingSettings.BootargsFilePath = setting.BootargsFilePath;

                            CurSettings.AuthenticationName = setting.AuthenticationName;
                            CurSettings.AuthenticationPassword = setting.AuthenticationPassword;
                            CurSettings.ServicePort = setting.ServicePort;
                            CurSettings.MinIntervalToUploadRecords = setting.MinIntervalToUploadRecords;
                            CurSettings.MinIntervalToUploadRecords = setting.MinIntervalToUploadRecords;
                            CurSettings.UploadRecordsApiUrl = setting.UploadRecordsApiUrl;
                            CurSettings.Ips = setting.Ips;
                        });
                    }
                }
            }

            AnvizNew.CChex_Init();

            anviz_handle = AnvizNew.CChex_Start_With_Param(0u, CurSettings.ServicePort);
            if (anviz_handle == IntPtr.Zero)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Ready = true;
                    Success = false;
                    Status = "Couldn't start Anviz SDK.";
                });
                return false;
            }

            if (set)
            {
                //AnvizNew.CChex_Get_Service_Port(anviz_handle);

                int ret = 1;
                if (!string.IsNullOrEmpty(CurSettings.AuthenticationName) && !string.IsNullOrEmpty(CurSettings.AuthenticationPassword))
                {
                    AnvizNew.CCHEX_CONNECTION_AUTHENTICATION_STRU param;
                    byte[] bytes = BytesStringHelper.Encoding.GetBytes(CurSettings.AuthenticationName);
                    param.username = new byte[authenticationLength];
                    Array.Copy(bytes, param.username, bytes.Length > authenticationLength ? authenticationLength : bytes.Length);
                    param.password = new byte[authenticationLength];
                    bytes = BytesStringHelper.Encoding.GetBytes(CurSettings.AuthenticationPassword);
                    Array.Copy(bytes, param.password, bytes.Length > authenticationLength ? authenticationLength : bytes.Length);

                    IntPtr ptrParam = Marshal.AllocHGlobal(Marshal.SizeOf(param));
                    Marshal.StructureToPtr(param, ptrParam, false);

                    ret = AnvizNew.CChex_Set_Connect_Authentication(anviz_handle, ptrParam);
                    Marshal.FreeHGlobal(ptrParam);
                }
                else
                {
                    ret = AnvizNew.CChex_Set_Connect_Authentication(anviz_handle, IntPtr.Zero);
                }
                if (ret != 1)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Ready = true;
                        Success = false;
                        Status = $"Couldn't authenticate, error code: {ret}.";
                    });
                    return false;
                }

                ret = AnvizNew.CCHex_Udp_Search_Dev(anviz_handle);
                if (ret != 1)
                {
                    await Broadcast();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Ready = true;
                    Success = true;
                    Status = "Initialized successfully, please wait for the devices to respond.";
                    //SwitchUI("Devices");
                });
            }
            else
            {
                Status = "Please modify and save settings first.";
                Ready = true;
                SwitchUI("Settings");
            }

            return true;
        }
        public void Stop(bool exit = true)
        {
            this.exit = exit;
            foreach (var device in Devices)
            {
                if (device.Conencted)
                {
                    AnvizNew.CCHex_ClientDisconnect(anviz_handle, device.Index);
                }
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                Devices.Clear();
                Success = false;
            });
            if (exit)
            {
                ClearCts();
            }
            AnvizNew.CChex_Stop(anviz_handle);
            anviz_handle = IntPtr.Zero;
        }
       
        public async Task GetResults()
        {
            int ret = 1;
            int[] dev_idx = new int[1], type = new int[1];
            IntPtr pBuff = Marshal.AllocHGlobal(bufferLength);
            while (!cts_get_results.Token.IsCancellationRequested)
            {
                if (anviz_handle == IntPtr.Zero)
                {
                    continue;
                }

                ret = AnvizNew.CChex_Update(anviz_handle, dev_idx, type, pBuff, bufferLength);
                if (ret > 0)
                {
                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //  Status = $"Device Index: {dev_idx[0]}, Update Type: {type[0]}.";
                    //});
                    switch (type[0])
                    {
                        case (int)AnvizNew.MsgType.CCHEX_RET_UDP_SEARCH_DEV_TYPE:
                            var lstDevice = UdpSearchDataHelper.ToDevices(pBuff);
                            if (lstDevice.Count > 0)
                            {
                                foreach (var objDevice in lstDevice)
                                {
                                    if (Devices.FirstOrDefault(item => item.Conencted && item.Index == objDevice.Index) != null)
                                    {
                                        continue;
                                    }
                                    AnvizNew.CCHex_ClientConnect(anviz_handle, BytesStringHelper.Encoding.GetBytes(objDevice.IpAddr), (int)CurSettings.ServicePort);
                                    await Task.Delay(10);
                                }
                            }
                            else
                            {
                                await Broadcast();
                            }
                            break;
                        case (int)AnvizNew.MsgType.CCHEX_RET_CONNECTION_AUTHENTICATION_TYPE:
                            AnvizNew.CCHEX_RET_COMMON_STRU result;
                            result = (AnvizNew.CCHEX_RET_COMMON_STRU)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_COMMON_STRU));
                            //Application.Current.Dispatcher.Invoke(() =>
                            //{
                            //    Status = $"MachineId: {result.MachineId}, Authentication: {(result.Result == 0 ? "OK" : "Fail")}.";
                            //});
                            break;
                        case (int)AnvizNew.MsgType.CCHEX_RET_CLINECT_CONNECT_FAIL_TYPE:
                            AnvizNew.CCHEX_RET_CLINECT_CONNECT_FAIL_STRU dev_fail;
                            dev_fail = (AnvizNew.CCHEX_RET_CLINECT_CONNECT_FAIL_STRU)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_CLINECT_CONNECT_FAIL_STRU));
                            //Application.Current.Dispatcher.Invoke(() =>
                            //{
                            //    Devices.Add(new DeviceViewModel
                            //    {
                            //        Conencted = false,
                            //        MachineId = "",
                            //        Index = -1,
                            //        IpAddr = BytesStringHelper.Encoding.GetString(dev_fail.Addr),
                            //        DevSerialNum = "",
                            //        MacAddr = ""
                            //    });
                            //    Status = $"Failed to connect to {BytesStringHelper.Encoding.GetString(dev_fail.Addr)}";
                            //});
                            var ip_port_fail = BytesStringHelper.Encoding.GetString(dev_fail.Addr);
                            var ip_fail = ip_port_fail.Substring(0, ip_port_fail.IndexOf(':'));
                            if (!this.exit && CurSettings.CheckIp(ip_fail))
                            {
                                AnvizNew.CCHex_ClientConnect(anviz_handle, BytesStringHelper.Encoding.GetBytes(ip_fail), (int)CurSettings.ServicePort);
                            }
                            break;
                        case (int)AnvizNew.MsgType.CCHEX_RET_DEV_LOGIN_TYPE:
                            AnvizNew.CCHEX_RET_DEV_LOGIN_STRU dev_add;
                            dev_add = (AnvizNew.CCHEX_RET_DEV_LOGIN_STRU)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_DEV_LOGIN_STRU));
                            if (!dictDevTypeFlag.ContainsKey(dev_add.DevIdx))
                            {
                                dictDevTypeFlag.Add(dev_add.DevIdx, (int)dev_add.DevTypeFlag);
                            }
                            else
                            {
                                dictDevTypeFlag[dev_add.DevIdx] = (int)dev_add.DevTypeFlag;
                            }
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var deviceAdd = Devices.FirstOrDefault(item => item.Index == dev_add.DevIdx);
                                if (null == deviceAdd)
                                {
                                    Devices.Add(new DeviceViewModel
                                    {
                                        Conencted = true,
                                        MachineId = dev_add.MachineId.ToString(),
                                        Index = dev_add.DevIdx,
                                        IpAddr = BytesStringHelper.Encoding.GetString(dev_add.Addr),
                                        DevSerialNum = "",
                                        MacAddr = ""
                                    });
                                    AnvizNew.CChex_ListPersonInfo(anviz_handle, dev_add.DevIdx);
                                    AnvizNew.CChex_DownloadAllRecords(anviz_handle, dev_add.DevIdx);
                                    AnvizNew.CChex_DownloadAllNewRecords(anviz_handle, dev_add.DevIdx);
                                    Status = $"Succeeded to connect to {BytesStringHelper.Encoding.GetString(dev_add.Addr)}";
                                }
                            }); 
                            var ip_port = BytesStringHelper.Encoding.GetString(dev_add.Addr);
                            var ip = ip_port.Substring(0, ip_port.IndexOf(':'));
                            CurSettings.AddIP(ip);
                            await Task.Run(() =>
                            {
                                using (var writer = new StreamWriter(settingsCsvPath))
                                {
                                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                                    {
                                        csv.WriteRecords(new List<SettingsViewModel> { CurSettings });
                                    }
                                }
                            });
                            break;
                        case (int)AnvizNew.MsgType.CCHEX_RET_DEV_LOGIN_CHANGE_TYPE:
                            AnvizNew.CCHEX_RET_DEV_LOGIN_STRU dev_change;
                            dev_change = (AnvizNew.CCHEX_RET_DEV_LOGIN_STRU)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_DEV_LOGIN_STRU));
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var deviceChange = Devices.FirstOrDefault(item => item.Index == dev_change.DevIdx);
                                if (null == deviceChange)
                                {
                                    Devices.Add(new DeviceViewModel
                                    {
                                        Conencted = true,
                                        MachineId = dev_change.MachineId.ToString(),
                                        Index = dev_change.DevIdx,
                                        IpAddr = BytesStringHelper.Encoding.GetString(dev_change.Addr),
                                        DevSerialNum = "",
                                        MacAddr = ""
                                    });
                                }
                                else
                                {
                                    deviceChange.Conencted = true;
                                    deviceChange.MachineId = dev_change.MachineId.ToString();
                                    deviceChange.Index = dev_change.DevIdx;
                                    deviceChange.IpAddr = BytesStringHelper.Encoding.GetString(dev_change.Addr);
                                    deviceChange.DevSerialNum = "";
                                    deviceChange.MacAddr = "";
                                }
                                Status = $"Succeeded to change {BytesStringHelper.Encoding.GetString(dev_change.Addr)}";
                            });
                            break;
                        case (int)AnvizNew.MsgType.CCHEX_RET_DEV_LOGOUT_TYPE:
                            AnvizNew.CCHEX_RET_DEV_LOGOUT_STRU dev_del;
                            dev_del = (AnvizNew.CCHEX_RET_DEV_LOGOUT_STRU)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_DEV_LOGOUT_STRU));
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var deviceDel = Devices.FirstOrDefault(item => item.Index == dev_del.DevIdx);
                                if (null != deviceDel)
                                {
                                    Devices.Remove(deviceDel);
                                }
                            });
                            var ip_port_del = BytesStringHelper.Encoding.GetString(dev_del.Addr);
                            var ip_del = ip_port_del.Substring(0, ip_port_del.IndexOf(':'));
                            if (!this.exit)
                            {
                                AnvizNew.CCHex_ClientConnect(anviz_handle, BytesStringHelper.Encoding.GetBytes(ip_del), (int)CurSettings.ServicePort);
                            }
                            Status = $"{ip_port_del} disconnected.";
                            break;
                        case (int)AnvizNew.MsgType.CCHEX_RET_RECORD_INFO_TYPE:
                        case (int)AnvizNew.MsgType.CCHEX_RET_GET_NEW_RECORD_INFO_TYPE:
                            RecordViewModel recordViewModel = new RecordViewModel();
                            bool isNew = false;
                            if ((dictDevTypeFlag[dev_idx[0]] & 0xFF) == (int)AnvizNew.CustomType.DEV_TYPE_VER_4_NEWID)
                            {
                                AnvizNew.CCHEX_RET_RECORD_INFO_STRU_VER_4_NEWID record_info;
                                record_info = (AnvizNew.CCHEX_RET_RECORD_INFO_STRU_VER_4_NEWID)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_RECORD_INFO_STRU_VER_4_NEWID));
                                recordViewModel.EmployeeId = BytesStringHelper.BytesToUnicodeString(record_info.EmployeeId);
                                recordViewModel.MachineId = record_info.MachineId.ToString();
                                recordViewModel.Time = new DateTime(2000, 1, 2).AddSeconds(BytesStringHelper.SwapInt32(BitConverter.ToUInt32(record_info.Date, 0))).ToString("yyyy-MM-dd HH:mm:ss");
                                recordViewModel.RecordType = record_info.RecordType.ToString();
                                recordViewModel.Index = record_info.CurIdx;
                                isNew = record_info.NewRecordFlag == 1;
                            }
                            else
                            {
                                AnvizNew.CCHEX_RET_RECORD_INFO_STRU record_info;
                                record_info = (AnvizNew.CCHEX_RET_RECORD_INFO_STRU)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_RECORD_INFO_STRU));
                                recordViewModel.EmployeeId = BytesStringHelper.GetEmployeeId(record_info.EmployeeId);
                                recordViewModel.MachineId = record_info.MachineId.ToString();
                                recordViewModel.Time = new DateTime(2000, 1, 2).AddSeconds(BytesStringHelper.SwapInt32(BitConverter.ToUInt32(record_info.Date, 0))).ToString("yyyy-MM-dd HH:mm:ss");
                                recordViewModel.RecordType = record_info.RecordType.ToString();
                                recordViewModel.Index = record_info.CurIdx;
                                isNew = record_info.NewRecordFlag == 1;
                            }
                            qRecords.Enqueue(recordViewModel);
                            //if (isNew)
                            //{
                            //    Application.Current.Dispatcher.Invoke(() =>
                            //    {
                            //        Status = $"Record Index: {recordViewModel.Index}, downloaded.";
                            //    });
                            //}
                            break;
                        case (int)AnvizNew.MsgType.CCHEX_RET_LIST_PERSON_INFO_TYPE:
                            EmployeeViewModel employeeViewModel = new EmployeeViewModel();
                            if ((dictDevTypeFlag[dev_idx[0]] & 0xFF) == (int)AnvizNew.CustomType.DEV_TYPE_FLAG_CARDNO_BYTE_7)
                            {
                                AnvizNew.CCHEX_RET_DLEMPLOYEE_INFO_STRU_EXT_INF_CARD_LEN_7 person_list;
                                person_list = (AnvizNew.CCHEX_RET_DLEMPLOYEE_INFO_STRU_EXT_INF_CARD_LEN_7)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_DLEMPLOYEE_INFO_STRU_EXT_INF_CARD_LEN_7));
                                employeeViewModel.MachineId = person_list.MachineId.ToString();
                                employeeViewModel.Index = person_list.CurIdx;
                                employeeViewModel.EmployeeId = BytesStringHelper.GetEmployeeId(person_list.EmployeeId);
                                employeeViewModel.EmployeeName = BytesStringHelper.BytesToUnicodeString(person_list.EmployeeName);
                            }
                            else
                            {
                                if ((int)AnvizNew.MsgType.CCHEX_RET_GET_ONE_EMPLOYEE_INFO_TYPE == type[0])
                                {
                                    AnvizNew.CCHEX_RET_PERSON_INFO_STRU one_person;
                                    one_person = (AnvizNew.CCHEX_RET_PERSON_INFO_STRU)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_PERSON_INFO_STRU));
                                    if (one_person.TotalCnt == 0)
                                    {
                                        break;
                                    }

                                }
                                if ((dictDevTypeFlag[dev_idx[0]] & 0xFF) == (int)AnvizNew.CustomType.DEV_TYPE_VER_4_NEWID)
                                {
                                    AnvizNew.CCHEX_RET_DLEMPLOYEE_INFO_STRU_EXT_INF_FOR_VER_4 person_list;
                                    person_list = (AnvizNew.CCHEX_RET_DLEMPLOYEE_INFO_STRU_EXT_INF_FOR_VER_4)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_DLEMPLOYEE_INFO_STRU_EXT_INF_FOR_VER_4));
                                    employeeViewModel.MachineId = person_list.MachineId.ToString();
                                    employeeViewModel.Index = person_list.CurIdx;
                                    employeeViewModel.EmployeeId = BytesStringHelper.GetEmployeeId(person_list.EmployeeId);
                                    employeeViewModel.EmployeeName = BytesStringHelper.BytesToUnicodeString(person_list.EmployeeName);
                                }
                                else if ((dictDevTypeFlag[dev_idx[0]] & 0xFF) == (int)AnvizNew.CustomType.ANVIZ_CUSTOM_EMPLOYEE_FOR_W2_ADD_TIME)//this for ANVIZ_CUSTOM_FOR_ComsecITech_W2  SDK
                                {
                                    AnvizNew.CCHEX_EMPLOYEE_INFO_STRU_EXT_INF_FOR_W2 person_list;
                                    person_list = (AnvizNew.CCHEX_EMPLOYEE_INFO_STRU_EXT_INF_FOR_W2)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_EMPLOYEE_INFO_STRU_EXT_INF_FOR_W2));
                                    employeeViewModel.MachineId = "";
                                    employeeViewModel.Index = 0;
                                    employeeViewModel.EmployeeId = BytesStringHelper.GetEmployeeId(person_list.EmployeeId);
                                    employeeViewModel.EmployeeName = BytesStringHelper.BytesToUnicodeString(person_list.EmployeeName);
                                }
                                else if ((dictDevTypeFlag[dev_idx[0]] & 0xFF) == (int)AnvizNew.EmployeeType.DEV_TYPE_FLAG_MSG_ASCII_32)
                                {                                                                                           ////this for NORMAL  SDK
                                    AnvizNew.CCHEX_RET_PERSON_INFO_STRU person_list;
                                    person_list = (AnvizNew.CCHEX_RET_PERSON_INFO_STRU)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_PERSON_INFO_STRU));
                                    employeeViewModel.MachineId = person_list.MachineId.ToString();
                                    employeeViewModel.Index = person_list.CurIdx;
                                    employeeViewModel.EmployeeId = BytesStringHelper.GetEmployeeId(person_list.EmployeeId);
                                    employeeViewModel.EmployeeName = BytesStringHelper.BytesToUnicodeString(person_list.EmployeeName);
                                }
                                else
                                {                                                                                           ////this for NORMAL  SDK
                                    AnvizNew.CCHEX_RET_PERSON_INFO_STRU person_list;
                                    person_list = (AnvizNew.CCHEX_RET_PERSON_INFO_STRU)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_RET_PERSON_INFO_STRU));
                                    employeeViewModel.MachineId = person_list.MachineId.ToString();
                                    employeeViewModel.Index = person_list.CurIdx;
                                    employeeViewModel.EmployeeId = BytesStringHelper.GetEmployeeId(person_list.EmployeeId);
                                    employeeViewModel.EmployeeName = BytesStringHelper.BytesToUnicodeString(person_list.EmployeeName);
                                }
                            }
                            qEmployees.Enqueue(employeeViewModel);
                            //Application.Current.Dispatcher.Invoke(() =>
                            //{
                            //    Status = $"Employee Index: {employeeViewModel.Index}, downloaded.";
                            //});
                            break;
                        default:
                            break;
                    }
                }
                else if (ret == 0)
                {
                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //    // Invalid Return value.
                    //    Status = "Invalid Return value.";
                    //});
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // buffer space is not enough, based on Return value, then re-apply the space.
                        Status = "Buffer space is not enough, based on Return value, then re-apply the space.";
                    });
                }
                await Task.Delay(10);
            }
        }

        public async Task HandleEmployees()
        {     
            while (!cts_get_results.Token.IsCancellationRequested)
            {
                var hasNew = false;
                while (qEmployees.Count > 0)
                {
                    var q = qEmployees.Dequeue();
                    hasNew = (await EmployeeLogic.Instance.AddAsync(q)).Body;
                    if (hasNew)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Status = $"Last employee record: Clock ID: {q.MachineId} Employee ID: {q.EmployeeId} Employee Name: {q.EmployeeName}";
                        });
                    }
                }

                if (qEmployees.Count == 0)
                {
                    await Task.Delay(10);
                }
            }
        }
        public async Task SyncEmployees()
        {
            while (!cts_get_results.Token.IsCancellationRequested)
            {
                foreach(var device in Devices)
                {
                    AnvizNew.CChex_ListPersonInfo(anviz_handle, device.Index);
                }
                await Task.Delay(60000);
            }
        }

        public async Task HandleRecords()
        {
            var newRecords = new List<RecordViewModel>();
            while (!cts_get_results.Token.IsCancellationRequested)
            {
                var hasNew = false;
                newRecords.Clear();
                while (qRecords.Count > 0)
                {
                    var q = qRecords.Dequeue();
                    hasNew = (await RecordLogic.Instance.AddAsync(q)).Body;
                    if (hasNew)
                    {
                        newRecords.Add(q);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Status = $"Last transaction record: Clock ID: {q.MachineId} Employee ID: {q.EmployeeId} DateTime: {q.Time}";
                        });
                    }
                }

                if (hasNew)
                {
                    foreach (var record in newRecords)
                    {
                        try
                        {
                            await IothubPublisher.SendClockInOut(record.EmployeeId, DateTime.Parse(record.Time));
                        }
                        catch
                        {
                            await File.AppendAllTextAsync(Path.Combine(pendingRecordsPath, $"{DateTime.Now.ToString("yyMMddHHmmssfff")}.json"), Newtonsoft.Json.JsonConvert.SerializeObject(record));
                        }
                    }
                }
                if (qRecords.Count == 0)
                {
                    await Task.Delay(10);
                }
            }
        }
        public async Task SyncRecords()
        {
            while (!cts_get_results.Token.IsCancellationRequested)
            {
                foreach (var device in Devices)
                {
                    AnvizNew.CChex_DownloadAllNewRecords(anviz_handle, device.Index);
                }
                await Task.Delay(60000);
            }
        }
        public async Task HandlePendingRecords()
        {
            EnumerationOptions enumerationOptions = new EnumerationOptions();
            enumerationOptions.MatchCasing = MatchCasing.CaseInsensitive;
            enumerationOptions.RecurseSubdirectories = false;
            RecordViewModel record = null;
            while (!cts_get_results.Token.IsCancellationRequested)
            {
                var files = Directory.GetFiles(pendingRecordsPath, "*.json", enumerationOptions);
                if (files.Length == 0)
                {
                    await Task.Delay(60000);
                    continue;
                }
                foreach (var file in files)
                {
                    try
                    {
                        record = Newtonsoft.Json.JsonConvert.DeserializeObject<RecordViewModel>(File.ReadAllText(file));
                        await IothubPublisher.SendClockInOut(record.EmployeeId, DateTime.Parse(record.Time));
                        File.Delete(file);
                    }
                    catch
                    {
                        await Task.Delay(60000);
                        break;
                    }
                }
            }
        }

        private async Task Broadcast()
        {
            var ignoringIpList = new List<string>();
            foreach (var device in Devices)
            {
                if (device.Conencted)
                {
                    ignoringIpList.Add(device.IpAddr.Substring(0, device.IpAddr.IndexOf(':')));
                }
            }
            foreach (var ip in CurSettings.GetIpList())
            {
                if (ignoringIpList.Contains(ip))
                {
                    continue;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Status = $"Ping {ip} ...";
                });
                AnvizNew.CCHex_ClientConnect(anviz_handle, BytesStringHelper.Encoding.GetBytes(ip), (int)CurSettings.ServicePort);
                await Task.Delay(10);
            }
            string hostName = Dns.GetHostName();
            IPAddress[] ipList = Dns.GetHostAddresses(hostName, AddressFamily.InterNetwork);
            foreach (IPAddress ip in ipList)
            {
                string ipAddr = ip.ToString().Substring(0, ip.ToString().LastIndexOf('.') + 1);
                for (var i = 2; i <= 254; i++)
                {
                    if (ignoringIpList.Contains($"{ipAddr}{i}"))
                    {
                        continue;
                    }
                    if (CurSettings.CheckIp($"{ipAddr}{i}"))
                    {
                        continue;
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Status = $"Ping {ipAddr}{i} ...";
                    });
                    AnvizNew.CCHex_ClientConnect(anviz_handle, BytesStringHelper.Encoding.GetBytes($"{ipAddr}{i}"), (int)CurSettings.ServicePort);
                    await Task.Delay(10);
                }
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                Status = $"Pinged all ips.";
            });
        }
    }
}