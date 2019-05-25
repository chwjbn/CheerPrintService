<?php
// 应用入口文件

// 检测PHP环境
if (version_compare(PHP_VERSION, '5.3.0', '<')) 
{
    die('require PHP > 5.3.0 !');
}

//WEB根目录
define('APP_WEB_ROOT',realpath(dirname(__FILE__)).'/');

//开发显示
function dev_show($data)
{
   print_r($data);
   exit();
}

//是否本地模式
function is_local_mode() 
{
    $debugFile = APP_WEB_ROOT . 'debug.lock';
    if (file_exists($debugFile)) 
	{
        return true;
    }
    return false;
}


if(is_local_mode())
{
	define('APP_DEBUG', true);
}
else
{
	define('APP_DEBUG', false);
}

// 定义应用目录
define('APP_PATH', APP_WEB_ROOT.'../Application/');

// 引入ThinkPHP入口文件
require APP_WEB_ROOT.'../ThinkPHP/ThinkPHP.php';

// 亲^_^ 后面不需要任何代码了 就是如此简单
