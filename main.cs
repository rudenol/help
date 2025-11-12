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
    class Objects //этот класс должен включать в себя детали, группы деталей, и группы групп деталей
    {
        protected string name;

        public Objects (string _name)
        {
            name = _name;
        }

        public override string ToString()
        {
            return name;
        }

        //здесь будут прописаны операторы сравнения любых типов элементов
    }

    class Obj: Objects //базовый класс для деталей и групп деталей
    {
        public string Name
        {
            get { return name; }
        }

        public Obj(string _name): base(_name)
        { }

        public override string ToString()
        {
            return name;
        }
    }

    class Detail: Obj //деталь
    {
        private string number;
        public string namep;
        public int cost;

        //свойства класса
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

        //конструктор
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

        public static bool TryParse(string s, out Obj dt)
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

    class Group: Obj //группа деталей
    {
        private readonly int count;

        //свойство
        public int Count
        {
            get { return count; }
        }

        //конструктор
        public Group(string _named, int _count) : base(_named)
        {
            count = _count;
        }

        public override string ToString()
        {
            return $"{name}; {count};";
        }

        public static bool TryParse(string s, out Obj gr)
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
    }

    class Item : Objects //это должно быть контейнером
    {
        protected int level;
        List<Objects> objs = new List<Objects>();

        //конструктор
        public Item(string _name, int _level = 0) : base(_name)
        {
            level = _level;
        }

        public void Add(Objects ob)
        {
            objs.Add(ob);
        }

        public override string ToString()
        {
            string ots = new string(' ', level * 4);
            string s = ots + name + "\n";

            for (int i = 0; i < objs.Count; i++)
            {
                s += ots + objs[i].ToString() + "\n";
            }
            return s;
        }

        public int Length => objs.Count;
        public Objects this[int i] => i >= 0 && i < Length ? objs[i] : null;

        public static Item Parse(string s)
        {
            Item it = new Item("");
            var sa = s.Split('\n');
            for (int i = 0; i < sa.Length; i++)
            {
                if (Group.TryParse(sa[i], out var ob) || Detail.TryParse(sa[i], out ob)) it.Add(ob);
            }

            return it;
        }

        public void LoadFromFile(StreamReader sr)
        {
            string line = sr.ReadLine();
            while (line != "" && !sr.EndOfStream)
            {
                //если просто деталь, добавляем
                if (line.Split(';').Length == 4 && Detail.TryParse(line, out var dt)) Add(dt);
                //иначе это будет группой деталей
                else
                {
                    if (line != "" && Group.TryParse(line, out var grp))
                    {
                        if (grp is Group group1)
                        {
                            //столько раз, сколько элементов в группе, что-то делаем
                            for (int i = 0; i < group1.Count; i++)
                            {
                                Item g = new Item(line, level + 1);
                                g.LoadFromFile(sr);
                                Add(g);
                            }
                        }
                    }
                }
                line = sr.ReadLine();
            }
        }

        //функция из Main, которая вызывает фунцию, написанную выше
        public void LoadFromFile(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                LoadFromFile(reader);
            }
        }

        //public void LoadFromFile(string filename)
        //{
        //    using (StreamReader reader = new StreamReader(filename))
        //    {
        //        while (!reader.EndOfStream)
        //        {
        //            string line = reader.ReadLine();
        //            if (group.TryParse(line, out var pr) || detail.TryParse(line, out pr)) Add(pr);
        //        }
        //    }
        //}
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Item item;
            item = new Item("");
            item.LoadFromFile(@"C:\Users\Asura\OneDrive\Desktop\запчасти.txt");

            Console.WriteLine(item);

            Console.WriteLine("Введите номер искомого товара: ");
            string num = Console.ReadLine();
            string[] proizv = new string[0];
            Detail[] d = new Detail[0];
            Detail mind = null;
            for (int i = 0; i < item.Length; i++)
            {
                if (item[i] is Detail det)
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