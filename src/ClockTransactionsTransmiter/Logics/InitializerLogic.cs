using ClockTransactionsTransmiter.DesignPaterns;
using ClockTransactionsTransmiter.Interfaces;
using ClockTransactionsTransmiter.Models;
using SqlSugar;

namespace ClockTransactionsTransmiter.Logics
{
    public class InitializerLogic : Singleton<InitializerLogic>, IInitializerLogic
    {
        private static readonly object locker = new object();
        private static SqlSugarScope db = null;
        public static SqlSugarScope DB { get { return db; } }

        public ReturnInfo Init(string connString)
        {
            var objRet = new ReturnInfo();
            if (null != db)
            {
                objRet.Tip = "Already conencted";
                return objRet;
            }
            lock (locker)
            {
                if (null != db)
                {
                    objRet.Tip = "Already conencted";
                    return objRet;
                }
                db = new SqlSugarScope(new ConnectionConfig()
                {
                    ConnectionString = connString,//Connection String
                    DbType = DbType.Sqlite,//Database Type                
                    IsAutoCloseConnection = true //Auto Close Connection
                },
                    client =>
                   {
                       client.Aop.OnError = (ex) =>
                           {

                           };
                       client.Aop.OnLogExecuting = (sql, pars) =>
                           {
                               //UtilMethods.GetSqlString(DbType.SqlServer, sql, pars);
                               //Console.WriteLine("SQL：" + sql);                      
                           };
                   });
            }
            objRet.Code = "OK";
            return objRet;
        }
    }
}