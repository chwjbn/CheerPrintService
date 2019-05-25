<?php
namespace Common\Model;

class PrintTaskModel extends BaseModel
{
	public function addPrintTask($data)
	{
		
		$printTaskData=$data;
		
		if(!$printTaskData['html_file_url'])
		{
			$this->setErrorMsg(401,'html文件url地址不能为空!');			
			return false;
		}
		
		
		$printTaskData['id']=155;
		$printTaskData['uuid']=md5($printTaskData['id']);
		
		$channelId=$printTaskData['id']%10;
		
		 //写到redis
		$redisClient = \Com\Chw\RedisLib::getInstance('REDIS_DEFAULT');
		$redisClient->select(4);
		
		$dataKey=sprintf('cheer_mq_cheer_print_task_%s',$channelId);
		
		$redisClient->lpush($dataKey,json_encode($printTaskData));
		
		return true;
	}
}