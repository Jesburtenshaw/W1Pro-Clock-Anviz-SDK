using ClockTransactionsTransmiter.DesignPaterns;
using ClockTransactionsTransmiter.Logics;
using ClockTransactionsTransmiter.Models;
using ClockTransactionsTransmiter.ViewModels;
using SqlSugar;
using System.Linq.Expressions;
using ClockTransactionsTransmiter.Helper;
using ClockTransactionsTransmiter.Helpers;

namespace PQWorld.BLL.Implement.SqlSugar
{
    public class RecordLogic : Singleton<RecordLogic>//, IRecordLogic
    {
        public async Task<ReturnInfo<PageInfo<RecordViewModel>>> ListAsync(int pageIndex, int pageSize,
            string machineId, string employeeId, string startTime, string endTime)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 20;

            var objRet = new ReturnInfo<PageInfo<RecordViewModel>>();
            objRet.Body = new PageInfo<RecordViewModel>();
            objRet.Body.Items = new List<RecordViewModel>();

            RefAsync<int> totalNum = new RefAsync<int>(0);
            var ms = await InitializerLogic.DB
                .Queryable<RecordViewModel>()
                .WhereIF(!string.IsNullOrEmpty(machineId), item => item.MachineId == machineId)
                .WhereIF(!string.IsNullOrEmpty(employeeId), item => item.EmployeeId == employeeId)
                .WhereIF(!string.IsNullOrEmpty(startTime), item => string.Compare(item.Time, startTime) >= 0)
                .WhereIF(!string.IsNullOrEmpty(endTime), item => string.Compare(item.Time, endTime) <= 0)
                .OrderByDescending(item => item.Time)
                .ToPageListAsync(pageIndex, pageSize, totalNum);
            objRet.Body.Items.AddRange(ms);
            objRet.Body.MaxPage = totalNum.Value % pageSize == 0 ? totalNum.Value / pageSize : (totalNum.Value / pageSize + 1);

            objRet.Code = "OK";
            return objRet;
        }

        public async Task<ReturnInfo<bool>> AddAsync(RecordViewModel record)
        {
            var objRet = new ReturnInfo<bool>();

            var m = await InitializerLogic.DB
                .Queryable<RecordViewModel>()
                .FirstAsync(item => item.MachineId == record.MachineId && item.EmployeeId == record.EmployeeId && item.Time == record.Time);
            if (null == m)
            {
                objRet.Body = true;
                await InitializerLogic.DB.Storageable(record).ExecuteCommandAsync();
            }
        
            objRet.Code = "OK";
            return objRet;
        }
    }
}