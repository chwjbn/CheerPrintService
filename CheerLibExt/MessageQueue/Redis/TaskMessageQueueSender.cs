using CheerLibExt.Model;
using System;

namespace CheeLibExt.MessageQueue.Redis
{
    public class TaskMessageQueueSender
    {
        private static TaskMessageQueueSender mInstance = new TaskMessageQueueSender();
        public static TaskMessageQueueSender getInstance()
        {
            return mInstance;
        }

        private RedisMqService mRedisMqService = RedisMqService.getInstance();

        public void sendTaskNotify(NeedRunTaskNode taskNode)
        {
            try
            {
                long taskId = long.Parse(taskNode.id);
                long channelId = taskId % 10;

                var channelName = string.Format("task_group_{0}", channelId);

                this.mRedisMqService.pushTaskMessageData(channelName, taskNode.ToJson());

                CheerLib.LogWriter.Info("{0}.sendTaskNotify channelName={1},taskId={2}", this.GetType().FullName, channelName, taskId);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }

    }
}
