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
    public class EmployeeLogic : Singleton<EmployeeLogic>//, IEmployeeLogic
    {
        public async Task<ReturnInfo<PageInfo<EmployeeViewModel>>> ListAsync(int pageIndex, int pageSize,
            string machineId, string employeeId, string employeeName)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 20;

            var objRet = new ReturnInfo<PageInfo<EmployeeViewModel>>();
            objRet.Body = new PageInfo<EmployeeViewModel>();
            objRet.Body.Items = new List<EmployeeViewModel>();

            RefAsync<int> totalNum = new RefAsync<int>(0);
            var ms = await InitializerLogic.DB
                .Queryable<EmployeeViewModel>()
                .WhereIF(!string.IsNullOrEmpty(machineId), item => item.MachineId == machineId)
                .WhereIF(!string.IsNullOrEmpty(employeeId), item => item.EmployeeId == employeeId)
                .WhereIF(!string.IsNullOrEmpty(employeeName), item => item.EmployeeName == employeeName)
                .OrderByDescending(item=>item.EmployeeId)
                .ToPageListAsync(pageIndex, pageSize, totalNum);
            objRet.Body.Items.AddRange(ms);
            objRet.Body.MaxPage = totalNum.Value % pageSize == 0 ? totalNum.Value / pageSize : (totalNum.Value / pageSize + 1);

            objRet.Code = "OK";
            return objRet;
        }

        public async Task<ReturnInfo<bool>> AddAsync(EmployeeViewModel employee)
        {
            var objRet = new ReturnInfo<bool>();

            var m = await InitializerLogic.DB
                .Queryable<EmployeeViewModel>()
                .FirstAsync(item => item.MachineId == employee.MachineId && item.EmployeeId == employee.EmployeeId);
            if (null == m)
            {
                objRet.Body = true;
                await InitializerLogic.DB.Storageable(employee).ExecuteCommandAsync();
            }
        
            objRet.Code = "OK";
            return objRet;
        }
    }
}