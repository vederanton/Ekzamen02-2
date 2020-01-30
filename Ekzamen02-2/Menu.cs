using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ekzamen02_2
{
    public enum Language
    {
        Unknown = 0,
        English,
        Numbers
    }

    public class Menu
    {
        public Account MyAccount;
        public AllQuizzez MyAllQuizzez;
        public static string PathAccs;
        public Dictionary<string, string> Accs;
        public static string PathQuizzez;

        static Menu()
        {
            PathAccs = "Accs-admin.txt";
            PathQuizzez = "quizes.xml";
        }

        public Menu()
        {
            MyAllQuizzez = new AllQuizzez();
            Accs = new Dictionary<string, string>();
            using (FileStream fileStream = new FileStream(PathAccs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream, Encoding.Default))
                {
                    Regex regexAccs = new Regex(@"[A-z](\w*)");
                    Regex regexPasswords = new Regex(@"[0-9](\w*)");

                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        Match matchAccs = regexAccs.Match(currentLine);
                        Match matchPasswords = regexPasswords.Match(currentLine);
                        Accs.Add(matchAccs.Value, matchPasswords.Value);
                    }
                }
            }

            ShowLoginOrRegister(); // внутри будет инициализирован MyAccount
        }

        public bool EnterLogin(out string login) // проверяет на латиницу, на существование такого же и иниц. логин
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Введите ваш логин: ");
                login = Console.ReadLine();
                if (Account.IsEnglishLanguage(login) == false)
                    continue;
                if (Accs.ContainsKey(login) == false)
                {
                    Console.Clear();
                    Console.WriteLine("Данный логин не зарегистрирован!");
                    Console.ReadKey();
                    return false;
                }
                break;
            }
            while (true);
            return true;
        }

        public static void EnterPassword(out string pass)
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Введите пароль цифрами от 0 до 9: ");
                pass = Console.ReadLine();
                if (Account.IsNumbers(pass) == false)
                    continue;
                break;
            }
            while (true);
        }

        public void Register()
        {
            string login;
            do
            {
                Console.Clear();
                Console.WriteLine("Введите Логин латинскими символами верхнего и(или) нижнего регистра: ");
                login = Console.ReadLine();
                if (Account.IsEnglishLanguage(login) == false)
                    continue;
                if (Accs.ContainsKey(login) == true)
                {
                    Console.Clear();
                    Console.WriteLine("Данный логин уже зарегистрирован!");
                    Console.ReadKey();
                    continue;
                }
                break;
            }
            while (true);

            string password;
            EnterPassword(out password);
            Accs.Add(login, password);

            SaveNewAccInFile();

            MyAccount = new Account(login, password);

            Console.Clear();
            Console.WriteLine("Регистрация прошла успешно!");
            Console.ReadKey();
        }

        public void SaveNewAccInFile()
        {
            using (StreamWriter writer = new StreamWriter(PathAccs, false, Encoding.Default))
            {
                foreach (var item in Accs)
                    writer.WriteLine($"{item.Key} - {item.Value}");
            }
        }

        public bool IsTruePassword(string login, string pass)
        {
            if (Accs[login] == pass)
                return true;
            else
            {
                Console.Clear();
                Console.WriteLine("Введен неверный пароль!");
                Console.ReadKey();
                return false;
            }
        }

        public void ShowLoginOrRegister() // первый вызываемый метод меню.
        {
            char choice;
            do
            {
                Console.Clear();
                Console.WriteLine("Викторина для Админа v1.0");
                Console.WriteLine("\n1 - Регистрация нового Администратора");
                Console.WriteLine("2 - Вход в аккаунт");
                Console.WriteLine("\n3 - Выход из программы");
                choice = Console.ReadKey().KeyChar;
                switch (choice)
                {
                    case '1':
                        Register();
                        continue;
                    case '2':
                        string login;
                        if (EnterLogin(out login) == false)
                        {
                            choice = 'q';
                            continue;
                        }
                        else
                        {
                            string pass;
                            EnterPassword(out pass);
                            if (IsTruePassword(login, pass) == false)
                            {
                                choice = 'q';
                                continue;
                            }
                            MyAccount = new Account(login, pass);
                        }
                        break;
                    case '3':
                        break;
                    default:
                        continue;
                }
            }
            while (choice != '1' && choice != '2' && choice != '3');

            if (choice != '3')
                ShowMainMenu();
            else
                Console.Clear();
        }

        public void ShowMainMenu()
        {
            char choice;
            do
            {
                Console.Clear();
                Console.WriteLine($"Вход выполнен, {MyAccount.Login}!");
                Console.WriteLine("\n1 - Создать новую викторину");
                Console.WriteLine("2 - Редактировать существующую викторину");
                Console.WriteLine("\n3 - Поменять пароль");
                Console.WriteLine("\n4 - Выход из аккаунта");
                Console.WriteLine("5 - Выход из программы");
                choice = Console.ReadKey().KeyChar;
                switch (choice)
                {
                    case '1':
                        CreateNewQuiz();
                        continue;
                    case '2':
                        EditQuiz();
                        continue;
                    case '3':
                        MyAccount.ChangePassword();
                        continue;
                    case '4':
                        MyAccount.Exit();
                        continue;
                    case '5':
                        break;
                    default:
                        continue;
                }
            }
            while (choice != '4' && choice != '5');
            Console.Clear();
        }

        public void CreateNewQuiz()
        {
            string quizName;
            do
            {
                Console.Clear();
                Console.WriteLine("Начинаем создание новой викторины!");
                Console.Write("\nВведите название викторины: ");
                quizName = Console.ReadLine();
                if (quizName.Length == 0)
                {
                    Console.Clear();
                    Console.WriteLine("Вы ничего не ввели!");
                    Console.ReadKey();
                    continue;
                }
                break;
            }
            while (true);

            int quizId = MyAllQuizzez.AllQuizzezList.Count;

            List<XmlQuestionData> questionsList = new List<XmlQuestionData>();
            string question;
            int questionId;
            List<string> options;
            List<int> answers;
            for (int i = 0; i < 20; i++)
            {
                questionId = i;
                Console.Clear();
                Console.WriteLine($"Введите {i + 1} вопрос: ");
                question = Console.ReadLine();
                if (question.Length == 0)
                {
                    Console.Clear();
                    Console.WriteLine("Вы ничего не ввели!");
                    Console.ReadKey();
                    i--;
                    continue;
                }

                options = new List<string>();
                int numberOptions = default;
                do
                {
                    Console.Clear();
                    Console.WriteLine($"Ваш вопрос: \"{question}\"");
                    Console.Write("\nВведите кол-во вариантов ответа (от 2 до 10): ");
                    try
                    {
                        numberOptions = Convert.ToInt32(Console.ReadLine());
                        if (numberOptions < 2 || numberOptions > 10)
                            throw new Exception("Неверный ввод!");
                    }
                    catch
                    {
                        Console.Clear();
                        Console.WriteLine("Неверный ввод!");
                        Console.ReadKey();
                    }
                }
                while (numberOptions < 2 || numberOptions > 10);

                answers = new List<int>();
                for (int j = 0; j < numberOptions; j++)
                {
                    Console.Clear();
                    Console.Write($"Введите вариант ответа №{j + 1}: ");
                    string option = Console.ReadLine();

                    if (option.Length == 0)
                    {
                        Console.Clear();
                        Console.WriteLine("Вы ничего не ввели!");
                        Console.ReadKey();
                        j--;
                        continue;
                    }

                    options.Add(option);
                    char choice;
                    do
                    {
                        Console.Clear();
                        Console.WriteLine(option);
                        Console.WriteLine("\nЯвляется ли данный вариант ответа правильным ответом (или одним из правильных) на данный вопрос?");
                        Console.WriteLine("1 - Да");
                        Console.WriteLine("0 - Нет");
                        choice = Console.ReadKey().KeyChar;
                        switch (choice)
                        {
                            case '1':
                                answers.Add(j);
                                break;
                            case '0':
                                if (j == (numberOptions - 1) && (answers.Count == 0 || answers == null))
                                {
                                    answers.Add(j);
                                    Console.Clear();
                                    Console.WriteLine("Так как это был последний вариант ответа, а хотя бы одного правильного ответа указано не было, " +
                                        "этот вариант ответа помечается как правильный.");
                                    Console.WriteLine("\nЕсли это не соответствует действительности, вы можете отредактировать данный вопрос в меню редактирования.");
                                    Console.ReadKey();
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    while (choice != '1' && choice != '0');
                }
                questionsList.Add(new XmlQuestionData(questionId, question, options, answers));
            }

            MyAllQuizzez.AllQuizzezList.Add(new Quiz(quizId, quizName, questionsList));

            SaveQuizzezInFile();

            Console.Clear();
            Console.WriteLine($"Викторина \"{quizName}\" успешно создана!");
            Console.ReadKey();
        }

        public void EditQuiz()
        {
            int choiceQuiz;
            do
            {
                Console.Clear();
                Console.WriteLine("Выберите викторину для редактирования, нажав соответствующий ей номер: ");
                Console.WriteLine();
                List<int> quizIds = new List<int>();
                foreach (Quiz quiz in MyAllQuizzez.AllQuizzezList)
                {
                    Console.WriteLine($"{quiz.QuizId} - {quiz.QuizName}");
                    quizIds.Add(Convert.ToInt32(quiz.QuizId));
                }

                Console.Write("\nВвод: ");
                try
                {
                    string choice = Console.ReadLine();
                    choiceQuiz = Convert.ToInt32(choice);
                }
                catch
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                if (quizIds.Contains(choiceQuiz) == false)
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                break;
            }
            while (true);

            char myChoice;
            do
            {
                Console.Clear();
                Console.WriteLine($"Выберите вариант редактирования викторины \"{MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizName}\":");
                Console.WriteLine("\n1 - Изменить название викторины");
                Console.WriteLine("2 - Изменить один из вопросов викторины");

                Console.WriteLine("\n0 - Сохранить изменения и выйти из меню редактирования");
                myChoice = Console.ReadKey().KeyChar;
                switch (myChoice)
                {
                    case '1':
                        Console.Clear();
                        Console.WriteLine($"Текущее название викторины: \"{MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizName}\".");
                        Console.Write("\nВведите новое название: ");
                        string newName = Console.ReadLine();
                        if (newName.Length == 0)
                        {
                            Console.Clear();
                            Console.WriteLine("Вы ничего не ввели!");
                            Console.ReadKey();
                            break;
                        }
                        MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizName = newName;
                        Console.Clear();
                        Console.WriteLine("Название викторины успешно изменено!");
                        Console.ReadKey();
                        break;

                    case '2':
                        EditQuestion(choiceQuiz);
                        break;

                    case '0':
                        SaveQuizzezInFile();
                        break;

                    default:
                        break;
                }
            }
            while (myChoice != '0');
        }

        public void EditQuestion(int choiceQuiz)
        {
            int choiceQuestion;
            do
            {
                Console.Clear();
                Console.WriteLine("Выберите вопрос для редактирования, нажав соответствующий ему номер: ");
                Console.WriteLine();
                List<int> questionsIds = new List<int>();
                foreach (XmlQuestionData question in MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList)
                {
                    Console.WriteLine($"{question.ItemId} - {question.Question}");
                    questionsIds.Add(Convert.ToInt32(question.ItemId));
                }

                Console.Write("\nВвод: ");
                try
                {
                    string choice = Console.ReadLine();
                    choiceQuestion = Convert.ToInt32(choice);
                }
                catch
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                if (questionsIds.Contains(choiceQuestion) == false)
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                break;
            }
            while (true);

            char choiceHowEditQuestion;
            do
            {
                Console.Clear();
                Console.WriteLine($"Редактирование вопроса \"{MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].Question}\":");
                Console.WriteLine("\n1 - Изменить название вопроса");
                Console.WriteLine("2 - Изменить вариант ответа");
                Console.WriteLine("\n0 - Выход из меню редактирования вопроса");
                choiceHowEditQuestion = Console.ReadKey().KeyChar;
                switch (choiceHowEditQuestion)
                {
                    case '1':
                        Console.Clear();
                        Console.WriteLine($"Текущее название вопроса: \"{MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].Question}\".");
                        Console.Write("\nВведите новое название: ");
                        string newName = Console.ReadLine();
                        if (newName.Length == 0)
                        {
                            Console.Clear();
                            Console.WriteLine("Вы ничего не ввели!");
                            Console.ReadKey();
                            break;
                        }
                        MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].Question = newName;
                        Console.Clear();
                        Console.WriteLine("Название вопроса успешно изменено!");
                        Console.ReadKey();
                        break;

                    case '2':
                        int temp = 0;
                        do
                        {
                            int choiceOption;
                            do
                            {
                                Console.Clear();
                                Console.WriteLine($"Какой вариант ответа вопроса \"{MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].Question}\" редактировать?");
                                Console.WriteLine();
                                for (int i = 0; i < MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].PossibleOptionsList.Count; i++)
                                    Console.WriteLine($"{i} - \"{MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].PossibleOptionsList[i]}\"");
                                Console.Write("\nВвод: ");
                                try
                                {
                                    string choice = Console.ReadLine();
                                    choiceOption = Convert.ToInt32(choice);
                                }
                                catch
                                {
                                    Console.WriteLine("\nНеверный ввод!");
                                    Console.ReadKey();
                                    continue;
                                }
                                if (MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].PossibleOptionsList.Count < (choiceOption + 1) || choiceOption < 0)
                                {
                                    Console.WriteLine("\nНеверный ввод!");
                                    Console.ReadKey();
                                    continue;
                                }
                                break;
                            }
                            while (true);

                            do
                            {
                                Console.Clear();
                                Console.Write("\nВведите новый вариант ответа вместо выбранного: ");
                                string newOption = Console.ReadLine();
                                if (newOption.Length == 0)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Вы ничего не ввели!");
                                    Console.ReadKey();
                                    break;
                                }
                                MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].PossibleOptionsList[choiceOption] = newOption;
                                break;
                            }
                            while (true);

                            char choiceTryOrNot;
                            do
                            {
                                Console.Clear();
                                Console.WriteLine($"Является ли данный вариант ответа правильным либо одним из правильных на вопрос \"{MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].Question}\"?");
                                Console.WriteLine("\n1 - Да");
                                Console.WriteLine("0 - Нет");
                                choiceTryOrNot = Console.ReadKey().KeyChar;
                                switch (choiceTryOrNot)
                                {
                                    case '1':
                                        if (MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].AnswersList.Contains(choiceOption) == false)
                                            MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].AnswersList.Add(choiceOption);
                                        break;
                                    case '0':
                                        break;
                                }
                            }
                            while (choiceTryOrNot != '1' && choiceTryOrNot != '0');
                            Console.Clear();
                            Console.WriteLine("Вариант ответа успешно изменен!");
                            Console.ReadKey();

                            if (MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[choiceQuestion].AnswersList.Count < 1)
                            {
                                Console.WriteLine("\nТребуется дальнейшее редактирование вариантов ответов, так как ни один из вариантов не помечен как правильный!");
                                Console.ReadKey();
                                temp = 1;
                            }
                            else
                                temp = 0;
                        }
                        while (temp == 1);
                        break;

                    case '0':
                        break;
                }
            }
            while (choiceHowEditQuestion != '0');
        }

        public void SaveQuizzezInFile()
        {
            using (FileStream fileStream = new FileStream(PathQuizzez, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var root = new XElement("quizes");
                foreach (var quiz in MyAllQuizzez.AllQuizzezList)
                {
                    var XmlQuiz = new XElement("quiz", new XAttribute("id", quiz.QuizId), new XAttribute("name", quiz.QuizName));
                    foreach (var item in quiz.QuizList)
                    {
                        var XmlItem = new XElement("item", new XAttribute("id", item.ItemId));
                        var XmlQuestion = new XElement("question", item.Question);
                        XmlItem.Add(XmlQuestion);
                        foreach (var possible in item.PossibleOptionsList)
                        {
                            var XmlPossible = new XElement("possible", possible);
                            XmlItem.Add(XmlPossible);
                        }
                        foreach (var answer in item.AnswersList)
                        {
                            var XmlAnswer = new XElement("answer", answer);
                            XmlItem.Add(XmlAnswer);
                        }
                        XmlQuiz.Add(XmlItem);
                    }
                    root.Add(XmlQuiz);
                }
                XDocument doc = new XDocument(root);
                doc.Save(fileStream);

                Console.Clear();
                Console.WriteLine("Сохранение данных прошло успешно!");
                Console.ReadKey();
            }
        }
    }
}
