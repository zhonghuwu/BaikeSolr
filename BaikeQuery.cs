using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Net;
using System.IO;

/*用户solr4.1的客户端,只需要修改IP即可。*/

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
        //定义类的成员变量：IP；

        BaikeQparameter bqparameter;
        string QueryPrefix = "";//获取用户输入的查询字符，用于自动补全。
        public List<string> Resultlist=new List<string>();//记录查询结果的词条id列表
        public int QueryCount=0;//返回查询的结果总数。
        string Termid="";//用于获得词条对应的相关词条
        string Typeid="";//用于获得词条对应的相关词条

        public int RelativeCount = 0;

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
        //用于提示词的url,
        private string GetSuggestUrl() 
        {
            string url = IP + "solr/terms?terms.fl=terminfo&terms.limit=10&terms.sort=index&terms.prefix=" +QueryPrefix;
            return url;
        }
     
        //用于获取一个词条的相关词条 
        private string GetRelativeUrl()
        {
            string url = IP + "solr/mlt?q=id:" + Termid + "&mlt.fl=terminfo&rows=6&fl=id&fq=typeid:"+Typeid;
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
            QueryCount = Convert.ToInt32(result.Attributes["numFound"].Value);
            //Console.WriteLine("查询结果数：" + QueryCount);


        }
        //分析自动补全返回的xml文件  
        private void AnalysisXmlSuggest(XmlDocument doc)
        {
            XmlNode termsnode = doc.SelectSingleNode("/response/lst[@name='terms']");
            XmlNodeList nodelist = termsnode.SelectNodes("./lst/int");
            foreach(XmlNode node in nodelist)
            {    
                 string termprefix = node.Attributes[0].Value;
                 Resultlist.Add(termprefix);
              //   Console.WriteLine(termprefix);
            }
           
        }

        //分析相关词条返回的xml文件，
        private void AnaylysisXmlRelative(XmlDocument doc)
        {
            XmlNode tempnode = doc.SelectSingleNode("/response/result[@name='response']");
            XmlNodeList nodelist = tempnode.SelectNodes("./doc");
            foreach (XmlNode node in nodelist)
            {
                string termid = node.InnerText;
                Resultlist.Add(termid);
             //  Console.WriteLine(termid);
            }

            //提取总的个数
            string tempcount = tempnode.Attributes[1].Value;
            RelativeCount = Convert.ToInt32(tempcount);
          // Console.WriteLine(RelativeCount);
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
        //用于提示词
        public void ExcuteSuggest(string queryprefix)
        {
            QueryPrefix = queryprefix;
            string purl = GetSuggestUrl();
            XmlDocument doc = DownloadXmlResult(purl);
            AnalysisXmlSuggest(doc);


        }
       
         //用于相关词条
        public void ExcuteRelativeTerms(string termid,string typeid)
        {
            Termid = termid;
            Typeid = typeid;
            string mrurl = GetRelativeUrl();
            XmlDocument doc = DownloadXmlResult(mrurl);
            AnaylysisXmlRelative(doc);
            
        }

        //用于相关查询词
        //  public void ExcuteRelativeQuery()
        // {

        // }

        
    }
}
