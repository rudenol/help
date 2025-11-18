using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ms
{
    class Item: IComparable
    {
        protected string name;

        public Item(string _name)
        {
            name = _name;
        }

        public int CompareTo(object item)
        {
            if (item is Item item1)
            {
                if (this > item1) return 1;
                else if (this < item1) return -1;
                else return 0;
            }
            return 0;
        }

        public static bool operator >(Item i1, Item i2)
        {
            if (i1 is Detail d1)
                if (i2 is Detail d2)
                    if ((d1.Number.CompareTo(d2.Number) == 0))
                    {
                        return (d1.Cost > d2.Cost);
                    }
                    else 
                    { 
                        return (d1.Number.CompareTo(d2.Number) > 0); 
                    }
                else if (i2 is Group g2)
                    return true;
                else return false;
            else if (i1 is Group g1)
                if (i2 is Group g2)
                    return g1.Name.CompareTo(g2.Name) > 0;
            else return false;
            return false;
        }

        public static bool operator <(Item i1, Item i2)
        {
            return !(i1 > i2) && !(i1.Equals(i2));
        }

        public override bool Equals(object obj)
        {
            if (obj is Item it)
                return (it.name.CompareTo(name) == 0);
            return false;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
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
            int _cost;
            bool res = (param.Length == 4);
            if (res == false) return false;
            _number = param[0];
            _named = param[1];
            _namep = param[2];
            _cost = Convert.ToInt32(param[3].Replace(" ", ""));

            if (res)
            {
                dt = new Detail(_number, _named, _namep, _cost);
            }
            return res;
        }
    }

    class Group: Item //группа и контейнер
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

        public Group(string _named, int _count = 0, int _level = 0) : base(_named)
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
            string s = "";

            s += Name + "\n";
            for (int i = 0; i < items.Count - 1; i++)
            {
                s += ots + "    " + items[i].ToString() + "\n";
            }
            s += ots + "    " + items[items.Count-1].ToString();
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

        public void Sort()
        {
            items.Sort();
            foreach (var item in items)
            {
                if (item is Group g)
                {
                    g.Sort();
                }
            }
        }

        public class CompItems : IComparer
        {
            public int Compare(object o1, object o2)
            {
                if (o1 is Item i1)
                    if (o2 is Item i2)
                        if (i1 > i2)
                            return 1;
                        else if (i1 < i2)
                            return -1;
                        else return 0;
                return 0;
            }
        }

        public void LoadGroup(StreamReader sr, int count)
        {
            for (int i = 0; i < count; i++)
            {
                string line = sr.ReadLine();
                if (line != "" && Detail.TryParse(line, out Item dt)) Add(dt);
                if (line != "" && Group.TryParse(line, out Group grp))
                {
                    Group g = new Group(grp.Name, grp.Count, level+1);
                    g.LoadGroup(sr, grp.Count);
                    Add(g);
                }
            }
        }

        public void LoadFromFile(StreamReader sr)
        {
            string line = sr.ReadLine();
            while (line != "" && !sr.EndOfStream)
            {
                if (line != "" && Detail.TryParse(line, out Item dt)) Add(dt);
                if (line != "" && Group.TryParse(line, out Group grp))
                {
                    Group g = new Group(grp.Name, grp.Count, level+1);
                    g.LoadGroup(sr, grp.Count);
                    Add(g);
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

        public IEnumerator GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public static Detail FindWithNum(Group g, string num)
        {
            Detail mind = null;
            for (int i = 0; i < g.Length; i++)
            {
                if (g[i] is Group group)
                {
                    Detail min = FindWithNum(group, num);
                    if (min != null && mind != null && min.Cost < mind.Cost) mind = min;
                    else if (mind == null && min != null) mind = min;
                }
                else if (g[i] is Detail det)
                {
                    if (det.Number == num)
                    {
                        if (mind != null && det.Cost < mind.Cost) mind = det;
                        else if (mind == null && det != null) mind = det;
                    }
                }
            }
            return mind;
        }

        public static string[] FindUniqueProizv(Group g)
        {
            string[] proizv = new string[0];
            for (int i = 0; i < g.Length; i++)
            {
                //рекурсивно ищем уникальных производителей внутри групп,
                //а после этого проверяем наличие этих производителей в изначальном массиве
                if (g[i] is Group group)
                {
                    string[] p = FindUniqueProizv(group);
                    foreach (string pr in p)
                    {
                        bool nc = true;
                        for (int j = 0; j < proizv.Length; j++)
                        {
                            if (pr == proizv[j]) { nc = false; }
                        }
                        if (nc == true)
                        {
                            Array.Resize(ref proizv, proizv.Length + 1);
                            proizv[proizv.Length - 1] = pr;
                        }
                    }
                }
                //проверяем производителя детали на уникальность
                else if (g[i] is Detail det)
                {
                    bool notcontains = true;
                    for (int k = 0; k < proizv.Length; k++)
                    {
                        if (det.NameP.Trim() == proizv[k].Trim()) notcontains = false;
                    }
                    if (notcontains)
                    {
                        Array.Resize(ref proizv, proizv.Length + 1);
                        proizv[proizv.Length - 1] = det.NameP;
                    }
                }
            }
            return proizv;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Group group = new Group("Каталог запчастей");
            group.LoadFromFile(@"C:\Users\Asura\OneDrive\Desktop\запчасти.txt");
            
            Console.WriteLine("Задание 1\n" + group);
            Console.WriteLine("\nЗадание 2,3 (отсортированная группа): ");

            group.Sort();
            Console.WriteLine(group);

            Console.Write("\nЗадание 4\nВведите номер искомого товара: ");
            string num = Console.ReadLine();
            Console.WriteLine(Group.FindWithNum(group, num));
            Console.Write("\nЗадание 5\nУникальные производители: ");
            string[] unique = Group.FindUniqueProizv(group);
            for (int i = 0; i < unique.Length; i++)
            {
                Console.Write(unique[i] + "; ");
            }

            Console.ReadLine();
        }
    }
}