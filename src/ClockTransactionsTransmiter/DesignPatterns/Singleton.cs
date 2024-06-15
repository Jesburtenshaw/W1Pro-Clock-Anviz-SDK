
namespace ClockTransactionsTransmiter.DesignPaterns
{
    /// <summary>
    /// 单例模式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : class, new()
    {
        /// <summary>
        /// 同步锁
        /// </summary>
        private static object locker = new object();

        /// <summary>
        /// 单例
        /// </summary>
        private static T instance;

        /// <summary>
        /// 获取单例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (null == instance)
                {
                    lock (locker)
                    {
                        if (null == instance)
                        {
                            instance = new T();
                        }
                    }
                }
                return instance;
            }
        }
    }
}