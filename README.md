# CheerPrintService
- 基于Firefox云打印服务,采用Firefox60版本内核渲染HTML，与Chrome现有版本渲染效果几乎一致
- 支持页面边距、页码...(支持二开)
- 支持最多10个打印任务同步进行，可以横向扩容
- 打印程序运行方式:
```
CheerPrintWorker.exe 配置xml文件
CheerPrintWorker.exe cheerprint.xml
```
- 整体服务架构图:
![image](https://github.com/chwjbn/CheerPrintService/blob/master/arc.png)

- 运行过程图:
![image](https://github.com/chwjbn/CheerPrintService/blob/master/Exp/10.png)
![image](https://github.com/chwjbn/CheerPrintService/blob/master/Exp/11.png)

![image](https://github.com/chwjbn/CheerPrintService/blob/master/Exp/30.png)
![image](https://github.com/chwjbn/CheerPrintService/blob/master/Exp/31.png)

![image](https://github.com/chwjbn/CheerPrintService/blob/master/Exp/40.png)
![image](https://github.com/chwjbn/CheerPrintService/blob/master/Exp/41.png)
