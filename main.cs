using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ms
{
    class Item
    {
        protected string name;

        public Item(string _name)
        {
            name = _name;
        }

        //здесь будут операторы сравнения
    }

    class Detail: Item //деталь
    {
        private string number;
        public string namep;
        public int cost;

        public string Number
        {
            get { return number; }
        }

        public string NameP
        {
            get { return namep; }
        }

        public int Cost
        {
            get { return cost; }
        }

        public Detail(string _number, string _named, string _namep, int _cost) : base(_named)
        {
            number = _number;
            namep = _namep;
            cost = _cost;
        }

        public override string ToString()
        {
            return $"{number}; {name}; {namep}; {cost};";
        }

        public static bool TryParse(string s, out Item dt)
        {
            dt = null;
            var param = s.Split(';');
            string _number, _named, _namep;
            int _cost = 0;
            bool res = (param.Length == 4) && int.TryParse(param[3], out _cost);
            _number = param[0];
            _named = param[1];
            _namep = param[2];

            if (res)
            {
                dt = new Detail(_number, _named, _namep, _cost);
            }
            return res;
        }
    }

    class Group: Item //контейнер
    {
        private readonly int count;
        protected int level;
        List<Item> items = new List<Item>();

        public string Name
        {
            get { return name; }
        }

        public int Count
        {
            get { return count; }
        }

        public Group(string _named, int _count, int _level = 0) : base(_named)
        {
            count = _count;
            level = _level;
        }

        public void Add(Item ob)
        {
            items.Add(ob);
        }

        public override string ToString()
        {
            string ots = new string(' ', level * 4);
            string s = ots + name + "\n";

            for (int i = 0; i < items.Count; i++)
            {
                s += ots + items[i].ToString() + "\n";
            }
            return s;
        }

        public int Length => items.Count;
        public Item this[int i] => i >= 0 && i < Length ? items[i] : null;

        public static bool TryParse(string s, out Group gr)
        {
            gr = null;
            var param = s.Split(';');
            string _name;
            int _count = 0;
            bool res = (param.Length == 2) && int.TryParse(param[1], out _count);
            _name = param[0];

            if (res)
            {
                gr = new Group(_name, _count);
            }
            return res;
        }
        public void LoadFromFile(StreamReader sr)
        {
            string line = sr.ReadLine();
            while (line != "" && !sr.EndOfStream)
            {
                if (line != "" && Group.TryParse(line, out Group grp))
                {
                    for (int i = 0; i < grp.Count; i++)
                    {
                        Group g = new Group(grp.Name, grp.Count, ++level);
                        g.LoadFromFile(sr);
                        level--;
                        Add(g);
                    }
                }
                line = sr.ReadLine();
            }
        }
        public void LoadFromFile(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                LoadFromFile(reader);
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Group group = new Group("",0);
            group.LoadFromFile(@"C:\Users\Asura\OneDrive\Desktop\запчасти.txt");

            Console.WriteLine(group);

            Console.WriteLine("Введите номер искомого товара: ");
            string num = Console.ReadLine();
            string[] proizv = new string[0];
            Detail[] d = new Detail[0];
            Detail mind = null;
            for (int i = 0; i < group.Length; i++)
            {
                if (group[i] is Detail det)
                {
                    if (det.Number == num)
                    {
                        Array.Resize(ref d, d.Length + 1);
                        d[d.Length - 1] = det;
                    }
                    bool notcontains = true;
                    for (int k = 0; k < proizv.Length; k++)
                    {
                        if ( det.NameP.Trim() == proizv[k].Trim()) notcontains = false;
                    }
                    if (notcontains)
                    {
                        Array.Resize(ref proizv, proizv.Length + 1);
                        proizv[proizv.Length - 1] = det.NameP;
                    }
                }
                int mincost = 1000000000;
                for (int j = 0; j < d.Length; j++)
                {
                    if (d[j].Cost < mincost)
                    {
                        mind = d[j];
                        mincost = d[j].Cost;
                    }
                }
            }
            Console.WriteLine(mind);
            Console.WriteLine();
            for (int i = 0; i < proizv.Length; i++)
            {
                Console.WriteLine(proizv[i]);
            }

            Console.ReadLine();
        }
    }
}