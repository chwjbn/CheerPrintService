<?php
namespace Addon\wapi\Controller;
use Common\Controller\CommonController;

class ClientController extends BaseController
{
	public function _initialize()
	{
		parent::_initialize();
	}
	
	public function report_task()
	{
		$check=I('post.check',1,'intval');
		$error_code=I('post.error_code',1,'intval');
		$msg=I('post.msg','','trim');
		$data=I('post.data','','trim');
		
		if($check==0&&$error_code=200)
		{
			$data=base64_decode($data);
			
			$fileName=sprintf('%sdata/pdf.pdf',APP_WEB_ROOT);
			
			file_put_contents($fileName,$data);
		}
		
		$this->ajaxCallMsg('200','succ.');	
	}
	
	
}