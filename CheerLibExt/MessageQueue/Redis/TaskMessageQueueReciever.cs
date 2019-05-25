using System;
using System.Threading;

namespace CheeLibExt.MessageQueue.Redis
{
    public class TaskMessageQueueReciever
    {
        public delegate void OnTaskMessageHandler(string msgContent);
        public event OnTaskMessageHandler OnTaskMessage;

        private string mChannelName = string.Empty;

        private RedisMqService mRedisMqService = RedisMqService.getInstance();

        public TaskMessageQueueReciever(string iChannelName)
        {
            this.mChannelName = iChannelName;
        }

        public void start()
        {
            this.init();
        }

        private void init()
        {
            try
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(msgRecvThreadFunc));
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }


        private void msgRecvThreadFunc(object ob)
        {
            while (true)
            {
                msgRecvTask();
            }
        }

        private void msgRecvTask()
        {
            try
            {
                var msgContent = this.mRedisMqService.popTaskMessageData(this.mChannelName);
                if (string.IsNullOrEmpty(msgContent))
                {
                    Thread.Sleep(1000*3);
                    return;
                }

                this.processTaskMessage(msgContent);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }

        private void processTaskMessage(string msgContent)
        {
            try
            {
                CheerLib.LogWriter.Log(this.GetType().FullName + ".processTaskMessage channelName:" + this.mChannelName + ",msgContent:" + msgContent);
                OnTaskMessage?.Invoke(msgContent);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }
    }
}
