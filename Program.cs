using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VoyagerTaskLab
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]

        public static T Min<T>(this T[] arr, T ignore) where T : IComparable
        {
            bool minSet = false;
            T min = default(T);
            for (int i = 0; i < arr.GetLength(0); i++)
                if (arr[i].CompareTo(ignore) != 0)
                    if (!minSet)
                    {
                        minSet = true;
                        min = arr[i];
                    }
                    else if (arr[i].CompareTo(min) < 0)
                        min = arr[i];
            return (minSet) ? min : ignore;
        }

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
