using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ProgramsTime.Common;

namespace ProgramsTime
{

    class Common
    {

        public class DigitonFile
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public String Alias { get; set; }
            public String File { get; set; }

            public int DisconnectCount { get; set; }
            public int DisconnectLevel { get; set; }

            public FileTypes Type { get; set; } = FileTypes.Audio;

            public bool isRed()
            {
                return Procent > 110 && (DisconnectCount - DisconnectLevel) >= 3;
            }

            public int Procent
            {
                get
                {
                    return DisconnectLevel > 0 ? (int)Math.Round((float)DisconnectCount / DisconnectLevel * 100.0) : 0;
                }
            }

            public TimeSpan Duration
            {
                get
                {
                    return EndDate - StartDate;
                }
            }

            public override string ToString()
            {
                return (EndDate - StartDate).ToString() + "\t" + StartDate + "\t" + EndDate + "\t" + Alias + "\t" + File;
            }
        }

        public enum FileTypes { Audio = 0, Reclam = 1, News = 2, Program = 3 };


        class dcomparator : IEqualityComparer<Match>
        {
            public bool Equals(Match x, Match y)
            {
                return x.Groups[2].Value == y.Groups[2].Value && x.Groups[3].Value == y.Groups[3].Value && x.Groups[4].Value == y.Groups[4].Value; 

            }

            public int GetHashCode(Match obj)
            {
                return obj.Groups[4].Value.GetHashCode();
                
            }
        }
        public static IEnumerable<DigitonFile> GetDayLog(DateTime dt)
        {
            String DigitonDir = @"\\digiton001\SoundFiles\Плейлистывещания\";
            //DigitonDir = @"c:\newweb\dpm\";

            var regstart = new Regex(@"<\t(.*?)\t(.*?)\t(.*?)\t(.*?)\t(.*?)\t.*", RegexOptions.Compiled | RegexOptions.Multiline);

            var regend = new Regex(@">\t(.*?)\t(.*?)\t(.*?)\t(.*?)\t(.*?)\t.*", RegexOptions.Compiled | RegexOptions.Multiline);

            String CurDigiton = null;
            try
            {
                CurDigiton = System.IO.File.ReadAllText(DigitonDir + $"{dt.Year:d4}{dt.Month:d2}{dt.Day:d2}" + ".dpm", Encoding.GetEncoding(1251));
            }
            catch
            {
                DigitonDir = @"c:\newweb\dpm\";
                CurDigiton = System.IO.File.ReadAllText(DigitonDir + $"{dt.Year:d4}{dt.Month:d2}{dt.Day:d2}" + ".dpm", Encoding.GetEncoding(1251));
            }

            // var matchstart = regstart.Matches(CurDigiton).Cast<Match>().Distinct( new dcomparator());
            // var matchend = regend.Matches(CurDigiton).Cast<Match>().Distinct(new dcomparator());

            var matchstart = regstart.Matches(CurDigiton).Cast<Match>().GroupBy(f => f.Groups[2].Value + f.Groups[4].Value).Select(f => f.First());
            var matchend = regend.Matches(CurDigiton).Cast<Match>().GroupBy(f => f.Groups[2].Value + f.Groups[4].Value).Select(f => f.Last());

            return from start in matchstart
                   join end in matchend
                   on start.Groups[2].Value+ start.Groups[4].Value equals end.Groups[2].Value+ end.Groups[4].Value
                   select new DigitonFile
                   {
                       StartDate = DateTime.Parse(start.Groups[1].Value),
                       EndDate = DateTime.Parse(end.Groups[1].Value),
                       File = start.Groups[4].Value,
                       Alias = start.Groups[3].Value,

                       Type = start.Groups[3].Value == "РЕКЛАМА" ? FileTypes.Reclam
                       : start.Groups[4].Value.ToLower().Contains("новости") ? FileTypes.News
                       : start.Groups[3].Value.ToLower() == "back-jingles programs" ? FileTypes.Program : FileTypes.Audio
                   };
        }

        /*
        public static Dictionary<int, List<Tuple<DateTime, String, String>>> GetDayLog(DateTime dt)
        {
            String DigitonDir = @"\\digiton001\SoundFiles\Плейлистывещания\";
            var reg = new Regex(@"<\t(.*?)\t.*?\t(.*?)\t(.*?)\t(.*?)\t.*", RegexOptions.Compiled | RegexOptions.Multiline);

            var CurDigiton = System.IO.File.ReadAllText(DigitonDir + $"{dt.Year:d4}{dt.Month:d2}{dt.Day:d2}" + ".dpm", Encoding.GetEncoding(1251));

            MatchCollection m = reg.Matches(CurDigiton);

            return m.Cast<Match>().Select(f => Tuple.Create(DateTime.Parse(f.Groups[1].Value), f.Groups[2].Value, f.Groups[3].Value))
                 .GroupBy(f => f.Item1.Hour).ToDictionary(f => f.Key, f => f.ToList());

        }
        */

        public static IEnumerable<radioprogram> GetProgs()
        {

            return System.IO.File.ReadAllLines("progs.txt", Encoding.GetEncoding(1251)).Select(f => f.Split('\t')).Select(f => new radioprogram
            {
                Name = f[0],
                Duration = f[1] == "1 час" ? 60 : 3,
                OtbList = new String[] { f[2] },
                Kind = f[3]
            });

            //var text = System.IO.File.ReadAllText("progs.txt", Encoding.GetEncoding(1251));
            //var reg = new Regex(@"(.*?) \((.*?)\)(.*?)\r\n\r\n", RegexOptions.Compiled | RegexOptions.Singleline);

            //var m = reg.Matches(text);

            //var r = m.Cast<Match>().Select(f => new radioprogram
            //{
            //    Name = f.Groups[1].Value,
            //    Duration = Int32.Parse(f.Groups[2].Value),
            //    OtbList = String.IsNullOrWhiteSpace(f.Groups[3].Value) ? null : f.Groups[3].Value.Trim().Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries),
                
               

            //});

            

            //return r;

        }

        
        public static IEnumerable<ProgEnter> GetProgsByDay(DateTime dt)
        {
            var log = GetDayLog(dt);
            var progs = GetProgs();

            foreach (var hour in log.GroupBy(f=>f.StartDate.Hour))
            {
                var h = hour;

                foreach (var p in progs)
                {
                    foreach (var r in p.HasInHour(h.ToList()))
                     yield return r;

                   //if (pe != null) yield return pe;
                }
            }
        }

    }
    class radioprogram
    {
        public String Name { get; set; }
        public int Duration { get; set; }
        public IEnumerable<String> OtbList { get; set; }

        public String Kind { get; set; }
        public bool IsHourProg { get { return Duration == 60; } }

        public override string ToString()
        {
            return Name + " : " + Duration;
        }


        public IEnumerable<ProgEnter> HasInHour(List<DigitonFile> hourlog)
        {
            if (OtbList == null) yield break;


            foreach (var item in hourlog)
            {
                if (!(Duration == 60 && item.Alias == "РЕКЛАМА"))
                    foreach (var otb in OtbList)
                        if ( (item.File+" "+item.Alias).Contains(otb))
                        {
                           var pe = new ProgEnter { Prog=this,ProgDate=item.StartDate ,ProgHour=item.StartDate.Hour,StartMinute=item.StartDate, EndMinute = item.EndDate, jingle = otb, File=item.File};
                            var ishour = Duration == 60;
                             yield return pe;

                           if (ishour) yield break;
                        } 
                
            }

        }
    }

    class ProgEnter
    {
        public radioprogram Prog { set; get; }
        public DateTime ProgDate { get; set; }
        public int ProgHour { get; set; }
        public DateTime StartMinute { get; set; }

        public DateTime EndMinute { get; set; }

        public String jingle { get; set; }

        public String File { get; set; }

        public override string ToString()
        {
            return Prog.Name + " ("+ Prog.Duration + " мин) : " + ProgDate.ToString() + " - распознаная метка -> " + jingle;
        }

        public string ToString( IEnumerable<ProgEnter> items)
        {
            var d = GetProgDuration(items);
            return (Prog.Duration == 60 ? "#####ЧАС#####>" : "") + Prog.Name + " (" + d + " мин) : " + ProgDate.ToString() + " - " + Prog.Kind +  " en:" + EndMinute.ToLongTimeString()
                + " [" + File+"]";


            //" - распознаная метка -> " + jingle;
        }

        public TimeSpan GetProgDuration(IEnumerable<ProgEnter> plist)
        {
            if (Prog.Duration < 60) return EndMinute - StartMinute;
            else
                return TimeSpan.FromSeconds(Prog.Duration*60 - plist.Where(f => f.ProgHour == ProgHour && !f.Prog.IsHourProg).Sum(f => f.GetProgDuration(plist).TotalSeconds));
        }

    }
}
