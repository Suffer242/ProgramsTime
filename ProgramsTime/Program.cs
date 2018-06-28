using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramsTime
{

    public static class helpers
    {
        public static IEnumerable<DateTime> Dates(DateTime StartDate, DateTime EndDate)
        {
            for (DateTime i = StartDate; i <= EndDate; i=i.AddDays(1))
            {
                yield return i;
            }
        }

        public static IEnumerable<DateTime> Dates2(DateTime StartDate, DateTime EndDate)
        {
            for (; StartDate <= EndDate; StartDate = StartDate.AddDays(1))
            {
                yield return StartDate;
            }
        }

        public static IEnumerable<DateTime> Dates3(DateTime StartDate, DateTime EndDate)
        {
            while (StartDate <= EndDate)
            {
                yield return StartDate;
                StartDate = StartDate.AddDays(1);
            }
        }

        public static IEnumerable<DateTime> Dates4(DateTime StartDate, DateTime EndDate)
        {
            while (StartDate <= EndDate)
            {
                yield return StartDate;
                StartDate = StartDate.AddDays(1);
            }
        }
    }

    class Program
    {
        public static void  WeekStat(DateTime sdate)
    {

        TimeSpan total_info = new TimeSpan();
        TimeSpan total_reclam = new TimeSpan();
        var cnt = 0;

        for (DateTime dt = sdate.AddDays(0); dt < sdate.AddDays(7); dt = dt.AddDays(1))
        {


            var l = new List<String>();
            var items = Common.GetProgsByDay(dt).ToList();

            TimeSpan info = new TimeSpan();
            TimeSpan reclam = new TimeSpan();

            foreach (var itemgroup in items.GroupBy(f => f.ProgHour)) // выводи почасу
            {
                l.Add(itemgroup.Key + " ----");
              //  Console.WriteLine(itemgroup.Key + " ----");

                TimeSpan hour_info = new TimeSpan();
                TimeSpan hour_reclam = new TimeSpan();

                foreach (var item in itemgroup.ToList())
                {
                 //   Console.WriteLine(item.ToString(items));

                    if (item.Prog.Kind == "Информационно-аналитическое") info += item.GetProgDuration(items);
                    if (item.Prog.Kind == "Реклама") reclam += item.GetProgDuration(items);

                    if (item.Prog.Kind == "Информационно-аналитическое") hour_info += item.GetProgDuration(items);
                    if (item.Prog.Kind == "Реклама") hour_reclam += item.GetProgDuration(items);

                    l.Add(item.ToString(items));
                }

                l.Add("--> Информационно-аналитическое: " + hour_info);
                l.Add("--> Реклама: " + hour_reclam);
                l.Add("");

            }

            total_info += info;
            total_reclam += reclam;

            l.Add(""); l.Add("");
            l.Add("----> Информационно-аналитическое: " + info);
            l.Add("----> Реклама: " + reclam);

            File.WriteAllLines(dt.ToShortDateString() + " log.txt", l);
            cnt++;
        }

        Console.WriteLine();
        Console.WriteLine(sdate.ToShortDateString() + " - " + sdate.AddDays(6).ToShortDateString());
        Console.WriteLine("total_info: " + total_info.TotalMinutes);
        Console.WriteLine("total_reclam: " + total_reclam.TotalMinutes);
        Console.WriteLine("procent: " + total_info.TotalMinutes / (cnt * 24 * 60 - total_reclam.TotalMinutes) * 100);

    }

        static void Main(string[] args)
        {

            //foreach ( var d in helpers.Dates4(new DateTime(2018, 2, 12), new DateTime(2018, 2, 18)) )
            //{
            //    Console.WriteLine(d);
            //}

            //Console.ReadKey();
            //return;


            var sdate = new DateTime(2018, 04, 16);
            for (int i = 0; i < 6+4; i++, sdate = sdate.AddDays(7))
            {
                WeekStat(sdate);
              
            }



            Console.ReadKey();

        }
    }
}
