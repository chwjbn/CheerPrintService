<?php
	
	$returnConfig = array();
	
	$offlineConfig=array(

		//默认库
		'DB_MYSQL_DEFAULT'=>array(
			'DB_TYPE'               =>  'mysql',     // 数据库类型
			'DB_HOST'               =>  '127.0.0.1', // 服务器地址
			'DB_NAME'               =>  'db_print',          // 数据库名
			'DB_USER'               =>  'root',      // 用户名
			'DB_PWD'                =>  'root',          // 密码
			'DB_PORT'               =>  '3306',        // 端口
			'DB_PREFIX'             =>  't_',    // 数据库表前缀
		),
	
	
		//默认redis
		'REDIS_DEFAULT'=>array(
			'REDIS_HOST'=>'127.0.0.1',
			'REDIS_PORT'=>6379,
			'REDIS_DB'=>4,
		    'REDIS_PASSWORD'=>''
		),

	);

	$onlineConfig=array(

		//默认库
		'DB_MYSQL_DEFAULT'=>array(
			'DB_TYPE'               =>  'mysql',     // 数据库类型
			'DB_HOST'               =>  '127.0.0.1', // 服务器地址
			'DB_NAME'               =>  'db_print',          // 数据库名
			'DB_USER'               =>  'root',      // 用户名
			'DB_PWD'                =>  'root',          // 密码
			'DB_PORT'               =>  '3306',        // 端口
			'DB_PREFIX'             =>  't_',    // 数据库表前缀
		),
	
	
		//默认redis
		'REDIS_DEFAULT'=>array(
			'REDIS_HOST'=>'172.25.10.98',
			'REDIS_PORT'=>16740,
			'REDIS_DB'=>4,
		    'REDIS_PASSWORD'=>''
		),

	);
	
	
	
	if(is_local_mode())
	{
		$returnConfig=$offlineConfig;
	}
	else
	{
		$returnConfig=$onlineConfig;
	}

	return $returnConfig;