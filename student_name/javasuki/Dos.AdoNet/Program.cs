using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using Mini.Data;

namespace Dos.AdoNet
{
    class Program
    {
        static void Main(string[] args)
        {
            string cnfgName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dos.AdoNet.exe.config");
            DbConfig.Get(cnfgName);

            Console.WriteLine("输入客户名称：");
            execQuery();
            Console.Read();
        }

        static void execQuery()
        {
            string s = Console.ReadLine();
            if (string.IsNullOrEmpty(s))
            {
                Console.WriteLine("无输入，请重试：");
                execQuery();
            }
            s = s.Replace("'", "''");
            string sql = @" select o.*
                            from CustData c inner join OrdData o 
                            on c.CustID = o.CustID where c.CustName='"+ s +"'";
            var dt = DbFactory.DbSelect(sql);
            foreach (DataRow row in dt.Rows)
            {
                sql = "select sum(LPrice) from OrdDetails where OrdID=" + row["OrdID"];
                var totalPrice = DbFactory.DbScalar<double>(sql);
                Console.WriteLine("-------------------------------");
                Console.WriteLine("{0}\t{1}\t{2:c}", row["OrdNO"], row["OrdTime"], totalPrice);

                sql = @"SELECT     PrdData.PrdName, OrdDetails.Price, OrdDetails.QNum, OrdDetails.LPrice
                        FROM         OrdDetails INNER JOIN
                        PrdData ON OrdDetails.PrdID = PrdData.PrdID";
                var dt2 = DbFactory.DbSelect(sql);
                foreach (DataRow d in dt2.Rows)
                {
                    Console.WriteLine("\t{0}\t{1,6}\t{2:c}\t{3:c}", d["PrdName"], d["QNum"], d["Price"], d["LPrice"]);
                }

                Console.WriteLine("");
            }

            Console.WriteLine(s);
        }
    }
}
