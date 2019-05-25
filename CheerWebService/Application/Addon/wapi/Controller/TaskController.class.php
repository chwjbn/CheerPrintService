<?php
namespace Addon\wapi\Controller;
use Common\Controller\CommonController;

class TaskController extends BaseController
{
	public function _initialize()
	{
		parent::_initialize();
	}
	
	public function add_task()
	{
		$data=array(
		    'html_file_url'=>I('post.html_file_url','','strval'),
			'task_callback_url'=>I('post.task_callback_url','','strval'),
			'html_window_width'=>I('post.html_window_width',1440,'intval'),
			'html_window_height'=>I('post.html_window_height',900,'intval'),
			'page_width'=>I('post.page_width',210,'floatval'),
			'page_height'=>I('post.page_height',297,'floatval'),
			'margin_top'=>I('post.margin_top',5,'floatval'),
			'margin_bottom'=>I('post.margin_bottom',10,'floatval'),
			'margin_left'=>I('post.margin_left',5,'floatval'),
			'margin_right'=>I('post.margin_right',5,'floatval'),
			'orientation_flag'=>I('post.orientation_flag',1,'intval'),
		);
		
		$dPrintTask=D('PrintTask');
		
		$ret=$dPrintTask->addPrintTask($data);
		
		if(!$ret)
		{
			$msgData=$dPrintTask->getErrorMsg();
			$this->ajaxCallMsg($msgData['error_code'],$msgData['msg']);
		}
		
		$this->ajaxCallMsg(0,'操作成功!');	
	}
	
	
}