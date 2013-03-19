using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaikeSolr
{
    //用于测试
    class Test
    {
        static void Main(string[] args)
        {

            BaikeQuery bquery=new BaikeQuery();

            //一般查询
          //  Console.WriteLine("一般查询:");
          //  bquery.ExcuteQuery("足球",0);
          //  Console.WriteLine("查询的结果为：" + bquery.QueryCount);


            //分类查询
           // Console.WriteLine("分类查询:");
           // bquery.ExcuteQuery("规则",0,"1");


            //自动提示词
           // Console.WriteLine("自动提示:");
           // bquery.ExcuteSuggest("羽");

            //相关视频
            Console.WriteLine("获取词条的相关词条");
            bquery.ExcuteRelativeTerms("761","5");
           
            Console.ReadLine();
        }
    }
}
