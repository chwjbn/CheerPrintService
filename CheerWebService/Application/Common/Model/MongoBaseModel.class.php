<?php

namespace Common\Model;
use Think\Model\MongoModel;

class MongoBaseModel extends MongoModel
{
 
   private $errorCode=0;
 
   public function _initialize()
   {
		$this->connection=C("DB_MONGO_DC");
		$this->tablePrefix=C('DB_MONGO_DC.DB_PREFIX');
		$this->dbName=C('DB_MONGO_DC.DATA_DB_NAME');
	}
	
	protected function setErrorCode($error_code)
	{
		$this->errorCode=$error_code;
	}
	
	protected function setError($msg)
	{
	   $this->error=$msg;
	}
	
	public function  getErrorMsg()
	{
		return array('error_code'=>$this->errorCode,'msg'=>$this->error);
	}

}