using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheeLibExt.Cache
{

    public class RedisConfigNode
    {
        public string host = "127.0.0.1";
        public int port = 6379;
        public int db = 0;
        public string user = string.Empty;
        public string password = string.Empty;

        public void parseFromConfig(string configName)
        {
            try
            {
                var configVal = ConfigurationManager.AppSettings.Get(configName);
                if (string.IsNullOrEmpty(configVal))
                {
                    return;
                }

                string[] itemList = configVal.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string item in itemList)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }

                    var idex = item.IndexOf("=");

                    if (idex < 1)
                    {
                        continue;
                    }

                    if (idex >= (item.Length - 1))
                    {
                        continue;
                    }

                    var itemKey = item.Substring(0, idex).Trim().ToLower();
                    var itemVal = item.Substring(idex + 1).Trim();

                    if (itemKey == "host")
                    {
                        this.host = itemVal;
                        continue;
                    }

                    if (itemKey == "port")
                    {
                        int.TryParse(itemVal, out this.port);
                        continue;
                    }

                    if (itemKey == "db")
                    {
                        int.TryParse(itemVal, out this.db);
                        continue;
                    }

                    if (itemKey == "user")
                    {
                        this.user = itemVal;
                        continue;
                    }

                    if (itemKey == "password")
                    {
                        this.password = itemVal;
                        continue;
                    }

                }

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }
    }

    public abstract class RedisAbstractService
    {
        protected string mRedisHost = string.Empty;
        protected int mRedisPort = 6379;
        protected int mRedisDb = 0;
        protected string mRedisPassword = string.Empty;

        private ConnectionMultiplexer mLiveRedisClient = null;
        private ReaderWriterLockSlim mRedisClientLock = new ReaderWriterLockSlim();

        public RedisAbstractService()
        {
            CheerLib.LogWriter.Info("{0}.initRedis Begin.", this.GetType().Name);
            initRedis();
            CheerLib.LogWriter.Info("{0}.initRedis End.", this.GetType().Name);
        }

        protected IDatabase getRedisActiveDb()
        {
            var redisClient = this.getRedisClient();
            return redisClient.GetDatabase(this.mRedisDb);
        }

        protected IServer getRedisActiveServer()
        {
            var redisClient = this.getRedisClient();
            var ipList = redisClient.GetEndPoints();
            return redisClient.GetServer(ipList[0]);
        }

        protected ConnectionMultiplexer getRedisClient()
        {
            ConnectionMultiplexer xRedisClient = null;

            this.mRedisClientLock.EnterReadLock();

            try
            {
                xRedisClient = mLiveRedisClient;
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            this.mRedisClientLock.ExitReadLock();

            return xRedisClient;
        }

        /// <summary>
        /// 重置redis客户端
        /// </summary>
        protected void resetRedisClient()
        {
            this.mRedisClientLock.EnterWriteLock();

            initRedisClient();

            while (true)
            {
                if (checkRedisClient())
                {
                    break;
                }

                CheerLib.LogWriter.Info("{0}.checkRedisClient Faild,try initRedisClient 2s later...", this.GetType().Name);

                Thread.Sleep(1000 * 2);

                initRedisClient();
            }

            this.mRedisClientLock.ExitWriteLock();
        }

        /// <summary>
        /// 初始化redis客户端
        /// </summary>
        private void initRedisClient()
        {
            try
            {
                if (this.checkRedisClient())
                {
                    return;
                }

                destoryRedisClient();

                string connStr = string.Format(
                    "{0}:{1},defaultDatabase={2},password={3},keepAlive=10",
                    this.mRedisHost,
                    this.mRedisPort,
                    this.mRedisDb,
                    this.mRedisPassword
                    );

                this.mLiveRedisClient = ConnectionMultiplexer.Connect(connStr);

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }

        /// <summary>
        /// 检查redis客户端
        /// </summary>
        /// <returns></returns>
        private bool checkRedisClient()
        {
            bool bRet = false;
            try
            {
                if (this.mLiveRedisClient == null)
                {
                    return bRet;
                }

                var statusData = this.mLiveRedisClient.GetStatus();

                if (!string.IsNullOrEmpty(statusData))
                {
                    bRet = true;
                }
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return bRet;
        }

        /// <summary>
        /// 销毁redis客户端
        /// </summary>
        private void destoryRedisClient()
        {
            try
            {
                if (this.mLiveRedisClient == null)
                {
                    return;
                }

                this.mLiveRedisClient.Dispose();
                this.mLiveRedisClient = null;
            }
            catch (Exception)
            {
            }
        }



        private void initRedis()
        {

            var configName = this.getRedisConfigName();

            var configNode = new RedisConfigNode();
            configNode.parseFromConfig(configName);

            this.mRedisHost = configNode.host;
            this.mRedisPort = configNode.port;
            this.mRedisDb = configNode.db;
            this.mRedisPassword = configNode.password;

            this.resetRedisClient();
        }


        protected abstract string getRedisConfigName();

        protected string ObjectToJson(Object data)
        {
            string sRet = string.Empty;

            try
            {
                sRet = JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                sRet = string.Empty;
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return sRet;
        }

        protected T JsonToObject<T>(string data) where T : new()
        {
            T retData = new T();

            try
            {
                retData = JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception ex)
            {
                retData = new T();
                CheerLib.LogWriter.Log(ex.ToString());

            }

            return retData;
        }

    }
}
