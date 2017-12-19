using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filters
{
    /// <summary>
    /// Класс для создания Log файлов при обработке исключений 
    /// </summary>
    public class Logger
    {
        private static string fileName="Log.txt";
        public static string FILE_NAME
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
                FILE = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            }
        }
        private static string FILE;

        /// <summary>
        /// Создание Log файла, располагается в папке "Документы"
        /// </summary>
        /// <param name="message">Сообщение о ошибке</param>
        public static void Log(string message)
        {
            using (StreamWriter writer = new StreamWriter(FILE_NAME= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName), true))
            {
                writer.Write(DateTime.Now + " " + message + "\n");
            }
        }
    }
}
