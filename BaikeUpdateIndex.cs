using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Xml;
using System.Net;


namespace BaikeSolrUpdateIndex
{
    
    class BaikeUpdateIndex
    {

        string IP = "http://192.168.0.117:8080/";
        string csvfilepath = "D:\\yumaoqiu\\badmintainnet\\BaikeSolrUpdateIndex\\baikeinfo.csv";
        string curlcommandpath = "D:\\yumaoqiu\\badmintainnet\\BaikeSolrUpdateIndex\\curl";
        dbAccess db = new dbAccess();
        public BaikeUpdateIndex()
        {


        }


        //输入查询语句，导出为csv文件
       private bool outputcsv(string sqlcommand)
        {
            
            FileStream fs = new FileStream(csvfilepath, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter strwriter = new StreamWriter(fs, new UTF8Encoding(false));
            strwriter.WriteLine("\"id\",\"typeid\",\"termname\",\"terminfo\"");


            DataSet ds = db.ReadDS(sqlcommand);
            int count = 0;
            //遍历ds，分条写入文件中
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {

               //Console.WriteLine("termid:"+ds.Tables[0].Rows[i][0]+"typeid"+ds.Tables[0].Rows[i][1]+"termname"+ds.Tables[0].Rows[i][2]+"terminfo"+ds.Tables[0].Rows[i][3]);
               string termname = ds.Tables[0].Rows[i][2].ToString();
               string terminfo = ds.Tables[0].Rows[i][3].ToString();
    
               termname = termname.Replace("\"", "\"\"");
               terminfo = terminfo.Replace("\"", "\"\"");

               strwriter.WriteLine("\"" + ds.Tables[0].Rows[i][0].ToString() + "\",\"" + ds.Tables[0].Rows[i][1].ToString() + "\",\"" + termname + "\",\"" + terminfo + "\"");


               ++count;
            }

            ds.Clear();
            strwriter.Close();
            fs.Close();

            if (count == 0)
            {
                return false;
            }

            return true;

        
        }

       //执行curlcommand命令
       private bool excutecurlcommand(string curlpath, string filepath)
       {

           string command =curlpath+ " " + IP + "solr/update/csv?commit=true --data-binary @" + filepath + " -H \"Content-type:text/plain;charset=utf-8\"";

           Process copyprocess = new Process();
           copyprocess.StartInfo.FileName = "cmd.exe";
           copyprocess.StartInfo.Arguments = "/c" + command;
           copyprocess.Start();
           copyprocess.Close();

           return true;
       }



/////////////////////////////////////////////////////////第1 部分  删除索引////////////////////////////////////////////////////////

        // 一共四种创建索引功能，基本步骤都是相同的：（1）第一步 设置sqll语句；（2）第二步 创建csv文件；（3）第3步 将csv文件上传到solr数据库中。
        // 四种不同功能就体现在（1）第一步，所以将（2）第二步和（3）第三步整合到函数CreateIndex（string sqlcommand）中。
        
        //创建索引1  对ID集合重新建立索引
        public bool CreateAllIndex()
       { 
            //第一步 设置sql语句
            Console.WriteLine("1 设置sql语句");
            string sqlcmd = "select termid ,typeid,termname,terminfo from baike_term";


            //第二步和第三步
            bool result = CreateIndex(sqlcmd);

            return result;
        }


        //创建索引2   每天更新当天创建的词条
        public bool IncreateIndex()
        {

            //第一步 设置sql语句
            Console.WriteLine("1 设置sql语句");
            string currenttime = System.DateTime.Now.ToString("yyyy-MM-dd");
            currenttime = currenttime + " 00:00:00";
            string sqlcmd = "select termid ,typeid,termname,terminfo from baike_term where createtime>to_date('"+currenttime+"','yy-mm-dd hh24:mi:ss')";

            //第二步和第三步
            bool result = CreateIndex(sqlcmd);

            return result; 
     
        }


        //创建索引3   对Termid集合进行创建索引
        public bool CreateOneIndex(string termid)
        { 
            //第一步 设置sql语句
            string sqlcmd = "select termid ,typeid,termname,terminfo from baike_term where termid='"+termid+"'";

            //第二步和第三步
            bool result = CreateIndex(sqlcmd);

            return result; 
        
        }

        //创建索引4   对某个termid进行创建索引
        public bool CreateMoreIndex(ArrayList termidset)
        {
            string tset = "('";
            for (int i = 0; i <termidset.Count-1 ; i++)
            {

                tset = tset + termidset[i] + "','";
            }
            tset = tset + termidset[termidset.Count - 1] + "')";
            string sqlcmd = "select termid ,typeid,termname,terminfo from baike_term where termid in " + tset + "";

            //第二步和第三步
            bool result = CreateIndex(sqlcmd);

            return result; 
        
        }


        //创建索引共有操作
        private bool CreateIndex(string sqlcmd)
        {
            //第二步 创建csv文件
            Console.WriteLine("2.创建csv文件");
            bool csvresult = outputcsv(sqlcmd);
            if (csvresult == false)
            {
                return false;
            }


            //第三步 上传solr服务器,执行curl命令
            Console.WriteLine("3. 上传csv文件到solr服务器");
            excutecurlcommand(curlcommandpath, csvfilepath);


            return true; 
        
        
        }
       



 //////////////////////////////////////////////////////////第2部分  删除索引//////////////////////////////////////////////////////
        //删除索引：就是通过url向solr发送删除的请求，所以只需要设置url参数即可，其他操作都是共有的，封装在DeleteIndex（string query）中。
        
        
        //删除索引1   删除所有的索引
        public bool DeleteAllIndex()
        {
            Console.WriteLine("删除所有的索引");
            string query = "*:*";
            return DeleteIndex(query);

        }
        

        //删除索引2   删除solr中某个id对应的索引
        public bool DeleteOneIndex(string termid)
        {
            Console.WriteLine("删除索引：termid=" + termid);
            string query = "id:" + termid;
            return DeleteIndex(query);
        }


        //删除索引共有操作
        public bool DeleteIndex(string query)
        {
            string deleteurl =IP+ "solr/update/?stream.body=<delete><query>" + query + "</query></delete>&stream.contentType=text/xml;charset=utf-8&commit=true";
            
            bool flag = false;
            try
            {
                XmlDocument response_xml = DownloadXmlResult(deleteurl);
                XmlNode status_node = response_xml.SelectSingleNode("/response/lst/int[@name='status']");
                int status = Convert.ToInt32(status_node.InnerText);
                if (status == 0)
                    flag = true;
                else
                    flag = false;
            }
            catch (Exception e)
            {
                flag = false;
            }
            if (flag == true)
                Console.WriteLine("成功删除");
            else
                Console.WriteLine("删除失败");
            return flag;
        
        }

        private XmlDocument DownloadXmlResult(string QueryUrl)
        {
            HttpWebRequest baiekrequest = (HttpWebRequest)WebRequest.Create(QueryUrl);
            HttpWebResponse baikeresponse = (HttpWebResponse)baiekrequest.GetResponse();
            Stream baikestream = baikeresponse.GetResponseStream();
            XmlDocument queryxml = new XmlDocument();
            queryxml.Load(baikestream);
            return queryxml;
        }



    }
}
