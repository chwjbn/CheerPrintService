<?php
namespace Common\Controller;
use Think\Controller;

class CommonController extends Controller
{
	public function _initialize()
	{
		$this->initRuntimeVal();
	}
	
    
	public function ajaxCallMsg($error_code=0,$msg='',$data=array())
	{
	   $returnData=array(
	     'error_code'=>$error_code,
		 'msg'=>$msg
	   );
	   
	   if($data)
	   {
	      $returnData['data']=$data;
	   }
	   
	   $this->ajaxReturn($returnData,'json');
	}
	
	private function initRuntimeVal()
	{
	    define('CDN_URL','');
		define('CDN_VER',time());
	}
}