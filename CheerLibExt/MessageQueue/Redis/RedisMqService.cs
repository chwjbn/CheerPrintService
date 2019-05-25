using CheeLibExt.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheeLibExt.MessageQueue.Redis
{
    public class RedisMqService : RedisAbstractService
    {

        private static RedisMqService mInstance = new RedisMqService();

        public static RedisMqService getInstance()
        {
            return mInstance;
        }


        /// <summary>
        /// 压入任务消息数据
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="data"></param>
        public void pushTaskMessageData(string channelName, string data)
        {
            try
            {
                var dataKey = string.Format("cheer_mq_{0}", channelName);


                var redisDb = this.getRedisActiveDb();

                redisDb.ListLeftPush(dataKey, data);

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());

                this.resetRedisClient();
            }
        }

        /// <summary>
        /// 弹出任务消息数据
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public string popTaskMessageData(string channelName)
        {
            var data = string.Empty;
            try
            {
                var dataKey = string.Format("cheer_mq_{0}", channelName);

                var redisDb = this.getRedisActiveDb();

                var result = redisDb.ListRightPop(dataKey);

                if ((!result.HasValue) || result.IsNullOrEmpty)
                {
                    return data;
                }

                string tempV = result.ToString();

                if (!string.IsNullOrEmpty(tempV))
                {
                    data = tempV;
                }

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());

                this.resetRedisClient();
            }

            return data;
        }


        protected override string getRedisConfigName()
        {
            return "cheer_redis_mq";
        }


    }
}
