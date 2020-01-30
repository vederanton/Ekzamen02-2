using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekzamen02_2
{
    public class Account
    {
        public string Login;
        public string Password;

        public Account(string login, string password)
        {
            Login = login;
            Password = password;
        }

        public void ChangePassword()
        {
            string newPassword;
            Menu.EnterPassword(out newPassword);
            string oldPassword = Password;
            Password = newPassword;

            SaveAccauntDataInFile(oldPassword);

            Console.Clear();
            Console.WriteLine("Пароль успешно обновлен!");
            Console.ReadKey();
        }

        private void SaveAccauntDataInFile(string oldPassword)
        {
            List<string> tmp = new List<string>();
            using (FileStream fileStream = new FileStream(Menu.PathAccs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream, Encoding.Default))
                {
                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        tmp.Add(currentLine);
                    }
                }
                int deleteIndex = Array.FindIndex(tmp.ToArray(), s => s == $"{Login} - {oldPassword}");
                tmp.RemoveAt(deleteIndex);
                tmp.Add($"{Login} - {Password}");
            }
            using (StreamWriter writer = new StreamWriter(Menu.PathAccs, false, Encoding.Default))
            {
                foreach (string item in tmp)
                    writer.WriteLine(item);
            }
        }

        public void Exit()
        {
            Menu newMenu = new Menu();
        }

        public static Language CheckSymbolLanguage(int symbol)
        {
            if ((symbol >= 65 && symbol <= 90) || symbol >= 97 && symbol <= 122)
                return Language.English;
            if (symbol >= 48 && symbol <= 57)
                return Language.Numbers;
            return Language.Unknown;
        }

        public static bool IsEnglishLanguage(string word)
        {
            Language current = Language.English;
            Language tmp;
            for (int i = 0; i < word.Length; i++)
            {
                tmp = CheckSymbolLanguage(word[i]);
                if (current != tmp)
                {
                    Console.WriteLine("Допустимый ввод логина - латинские символы верхнего и нижнего регистра!");
                    Console.ReadKey();
                    return false;
                }
            }
            return true;
        }

        public static bool IsNumbers(string password)
        {
            Language current = Language.Numbers;
            Language tmp;
            for (int i = 0; i < password.Length; i++)
            {
                tmp = CheckSymbolLanguage(password[i]);
                if (current != tmp)
                {
                    Console.WriteLine("Допустимый ввод пароля - цифры от 0 до 9!");
                    Console.ReadKey();
                    return false;
                }
            }
            return true;
        }
    }
}
