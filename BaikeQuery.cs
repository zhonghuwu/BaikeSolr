using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Net;
using System.IO;

/*用户solr4.1的客户端*/

namespace BaikeSolr
{
   struct BaikeQparameter
   {
       public string querystr;
       public int start;
       public string typeid;
   };
    class BaikeQuery
    {
        //定义类的成员变量：IP,下载xml的路径；

        BaikeQparameter bqparameter;
        public List<string> Resultlist=new List<string>();//记录查询结果的词条id列表
        public string QueryCount="0";//返回查询的结果总数。

        string IP="http://localhost:8080/";
        
        //一般查询url，接受参数:查询词，start.
        private string GetGeneralUrl()
        {
            string url = IP+"solr/select?q="+bqparameter.querystr+"&start="+bqparameter.start;
            return url;
        }
        //分类查询url，接受参数：查询词，start，fq过滤的球类
        private string GetClassifyUrl()
        {
            string url = IP + "solr/select?q=" + bqparameter.querystr + "&start=" + bqparameter.start + "&fq=typeid:" + bqparameter.typeid;
            return url;
        }

        //根据查询url，使用Curl来下载xml文件
        private XmlDocument DownloadXmlResult(string QueryUrl)
        {
            HttpWebRequest baiekrequest = (HttpWebRequest)WebRequest.Create(QueryUrl);
            HttpWebResponse baikeresponse = (HttpWebResponse)baiekrequest.GetResponse();
            Stream baikestream = baikeresponse.GetResponseStream();
            XmlDocument queryxml = new XmlDocument();
            queryxml.Load(baikestream);
            return queryxml;
        }


        //分析xml文件，然后分析结果,返回dataset
        private void AnalysisXmlResult(XmlDocument doc)
        {
            
            //读取termid
            XmlNodeList nodelist = doc.SelectNodes("/response/result/doc");
            foreach (XmlNode node in nodelist)
            {
                string termid = node.InnerText;
                Resultlist.Add(termid);
               // Console.WriteLine("termid:"+termid);
            }

            //获取总的结果数

            XmlNode result = doc.SelectSingleNode("/response/result");
            QueryCount = result.Attributes["numFound"].Value;
            //Console.WriteLine("查询结果数：" + QueryCount);


        }

        public void ExcuteQuery(string querystr,int start)//用于一般的查询
        {
            bqparameter.querystr = querystr;
            bqparameter.start = start;
            string qurl = GetGeneralUrl();
           XmlDocument doc= DownloadXmlResult(qurl);
           AnalysisXmlResult(doc);

        }
        public void ExcuteQuery(string querystr, int start,string typeid)//用于对于含有某种球类的查询词，可以用于分类查询
        {
            bqparameter.querystr = querystr;
            bqparameter.start = start;
            bqparameter.typeid = typeid;
            string qurl = GetClassifyUrl();
            XmlDocument doc = DownloadXmlResult(qurl);
            AnalysisXmlResult(doc);
        }

    }
}
