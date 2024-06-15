using ClockTransactionsTransmiter.Models;

namespace ClockTransactionsTransmiter.Interfaces
{
    public interface IInitializerLogic
    {
        ReturnInfo Init(string connString);
    }
}
