using System;
using System.Globalization;
using System.Text;

namespace AutoService
{
    internal class Program
    {
        const double HourRate = 450;     // вартість нормо-години, грн
        const double PartsShare = 0.15;  // частка запчастин від вартості робіт
        const double PartsFixed = 200;   // фіксована частина запчастин, грн
        const double MaxHours = 24;      // найбільша тривалість ремонту, год

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            // Довідники для таблиці виторгу — тепер це локальні змінні, а не поля класу
            string[] workTypes =
            {
                "Діагностика", "Заміна мастила", "Гальма",
                "Ходова", "Двигун", "Електрика"
            };
            double[] workPrices = { 300, 450, 800, 1200, 3500, 900 };
            string[] days = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд" };

            // Сховище вартостей замовлень: масив фіксованої довжини й лічильник заповнених
            double[] costs = new double[100];
            int count = 0;

            bool running = true;
            while (running)
            {
                ShowMenu(count, costs.Length);
                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1": ShowAbout(); Pause(); break;
                    case "2": ShowCostTable(); Pause(); break;
                    case "3": ShowWorkloadTable(workTypes, workPrices, days); Pause(); break;
                    case "4": AddValue(costs, ref count, ReadDouble("Вартість замовлення, грн", 0, 1000000), costs.Length); Pause(); break;
                    case "5": PrintValues(costs, count); Pause(); break;
                    case "6": PrintMax(costs, count); Pause(); break;
                    case "7": RemoveAt(costs, ref count, ReadInt("Номер запису для видалення", 1, costs.Length) - 1); Pause(); break;
                    case "8": PrintStatistics(costs, count); Pause(); break;
                    case "9": ShowPassingDemo(); Pause(); break;
                    case "0": running = false; break;
                    default: ShowUnknownChoice(); Pause(); break;
                }
            }

            Console.WriteLine("До побачення.");
        }

        // Виводить головне меню й скільки записів зараз у масиві
        static void ShowMenu(int count, int capacity)
        {
            Console.WriteLine("=== Автосервіс ===");
            Console.WriteLine($"Записів у масиві: {count} з {capacity}");
            Console.WriteLine("1 — довідка про програму");
            Console.WriteLine("2 — таблиця вартості ремонту за годинами");
            Console.WriteLine("3 — звіт: виторг за видами робіт і днями тижня");
            Console.WriteLine("4 — додати значення (вартість замовлення)");
            Console.WriteLine("5 — показати вміст масиву");
            Console.WriteLine("6 — знайти найбільше значення");
            Console.WriteLine("7 — видалити запис за номером");
            Console.WriteLine("8 — показати статистику");
            Console.WriteLine("9 — демонстрація: значення й посилання");
            Console.WriteLine("0 — вихід");
            Console.Write("Ваш вибір: ");
        }

        // Повідомляє, що такого пункту меню немає
        static void ShowUnknownChoice()
        {
            Console.WriteLine("Немає такого пункту. Спробуйте ще раз.");
        }

        // Пауза й очищення екрана після дії
        static void Pause()
        {
            Console.WriteLine("\nНатисніть будь-яку клавішу...");
            Console.ReadKey(true);
            Console.Clear();
        }

        // Довідка: тема, сутність, поля з типами
        static void ShowAbout()
        {
            Console.WriteLine("Тема: Автосервіс");
            Console.WriteLine("Головна сутність: Замовлення на ремонт");
            Console.WriteLine("Довідник: Вид робіт");
            Console.WriteLine("Полів: 8");
            Console.WriteLine("  int      номер замовлення");
            Console.WriteLine("  string   марка авто");
            Console.WriteLine("  string   держномер");
            Console.WriteLine("  int      код виду робіт (з довідника)");
            Console.WriteLine("  DateTime дата прийому");
            Console.WriteLine("  double   вартість робіт");
            Console.WriteLine("  double   вартість запчастин");
            Console.WriteLine("  string   статус");
            Console.WriteLine("Підсумкова величина: сума по кожному виду робіт за місяць.");
        }

        // Питає межі й крок, потім виводить таблицю вартості
        static void ShowCostTable()
        {
            double start = ReadDouble("Початок, годин", 0, MaxHours);
            double end = ReadDouble("Кінець, годин", start, MaxHours);
            double step = ReadDouble("Крок, годин", 0.1, MaxHours);

            Console.WriteLine();
            PrintCostTable(start, end, step);
        }

        // Повертає вартість ремонту: години × ціна години + запчастини
        static double CalculateCost(double hours, double rate, double parts)
        {
            return hours * rate + parts;
        }

        // Повертає вартість запчастин за вартістю робіт
        static double CalculateParts(double workCost)
        {
            return workCost * PartsShare + PartsFixed;
        }

        // Табулює вартість ремонту від start до end з кроком step
        static void PrintCostTable(double start, double end, double step)
        {
            Console.WriteLine("Вартість ремонту залежно від тривалості робіт");
            Console.WriteLine($"{"Годин",8}{"Роботи, грн",16}{"З запчастинами",18}");

            // Годину рахуємо від номера кроку, а не додаванням, щоб похибка не накопичувалась.
            // 1e-9 — запас, щоб не зникла остання стрічка.
            for (int i = 0; start + i * step <= end + 1e-9; i++)
            {
                double hours = start + i * step;
                double work = CalculateCost(hours, HourRate, 0);
                double total = CalculateCost(hours, HourRate, CalculateParts(work));
                Console.WriteLine($"{hours,8:F2}{work,16:F2}{total,18:F2}");
            }
        }

        // Таблиця з двома входами: рядки — види робіт, стовпці — дні тижня
        static void ShowWorkloadTable(string[] workTypes, double[] workPrices, string[] days)
        {
            int rows = ReadInt("Кількість рядків (видів робіт)", 1, workTypes.Length);
            int cols = ReadInt("Кількість стовпців (днів тижня)", 1, days.Length);

            Console.WriteLine();
            Console.WriteLine("Виторг за видами робіт і днями тижня, грн");

            PrintWorkloadHeader(days, cols);
            for (int r = 0; r < rows; r++)
            {
                PrintWorkloadRow(workTypes, workPrices, r, cols);
            }
            PrintWorkloadFooter(workPrices, rows, cols);
        }

        // Виводить шапку: дні тижня й стовпець підсумку
        static void PrintWorkloadHeader(string[] days, int cols)
        {
            Console.Write($"{"",-16}");
            for (int c = 0; c < cols; c++)
            {
                Console.Write($"{days[c],9}");
            }
            Console.WriteLine($"{"Разом",12}");
        }

        // Виводить один рядок таблиці: вид робіт, виторг по днях і підсумок рядка
        static void PrintWorkloadRow(string[] workTypes, double[] workPrices, int r, int cols)
        {
            Console.Write($"{workTypes[r],-16}");
            for (int c = 0; c < cols; c++)
            {
                Console.Write($"{CalculateRevenue(workPrices, r, c),9:F0}");
            }
            Console.WriteLine($"{CalculateRowTotal(workPrices, r, cols),12:F0}");
        }

        // Виводить рядок підсумків по стовпцях і загальний підсумок
        static void PrintWorkloadFooter(double[] workPrices, int rows, int cols)
        {
            Console.Write($"{"Разом",-16}");
            double grandTotal = 0;
            for (int c = 0; c < cols; c++)
            {
                double columnTotal = CalculateColumnTotal(workPrices, c, rows);
                Console.Write($"{columnTotal,9:F0}");
                grandTotal += columnTotal;
            }
            Console.WriteLine($"{grandTotal,12:F0}");
        }

        // Повертає виторг виду робіт r у день c (кількість замовлень × ціна)
        static double CalculateRevenue(double[] workPrices, int r, int c)
        {
            int orders = ((r + 2) * (c + 3) + c) % 5 + 1; // умовна кількість замовлень
            return orders * workPrices[r];
        }

        // Повертає суму виторгу виду робіт r за перші cols днів
        static double CalculateRowTotal(double[] workPrices, int r, int cols)
        {
            double total = 0;
            for (int c = 0; c < cols; c++)
            {
                total += CalculateRevenue(workPrices, r, c);
            }
            return total;
        }

        // Повертає суму виторгу дня c за перші rows видів робіт
        static double CalculateColumnTotal(double[] workPrices, int c, int rows)
        {
            double total = 0;
            for (int r = 0; r < rows; r++)
            {
                total += CalculateRevenue(workPrices, r, c);
            }
            return total;
        }


        // Додає значення в кінець масиву. Лічильник — ref, бо метод має змінити його в Main.
        // Масив ref не потрібен: він і так передається посиланням.
        static void AddValue(double[] values, ref int count, double newValue, int capacity)
        {
            if (count == capacity)
            {
                Console.WriteLine("Місця більше немає.");
                return;
            }

            values[count] = newValue;
            count++;
            Console.WriteLine($"Значення додано. Записів: {count}.");
        }

        // Виводить вміст масиву до count таблицею з номерами
        static void PrintValues(double[] values, int count)
        {
            if (count == 0)
            {
                Console.WriteLine("Масив порожній.");
                return;
            }

            Console.WriteLine($"{"№",4}{"Вартість, грн",18}");
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine($"{i + 1,4}{values[i],18:F2}");
            }
        }

        // Шукає найбільше значення. Повертає false, якщо набір порожній.
        // max і index — out: метод створює їх, а не змінює наявні.
        // index — позиція в масиві з нуля.
        static bool FindMax(double[] values, int count, out double max, out int index)
        {
            max = 0;
            index = -1;

            if (count == 0)
            {
                return false;
            }

            max = values[0];
            index = 0;
            for (int i = 1; i < count; i++)
            {
                if (values[i] > max)
                {
                    max = values[i];
                    index = i;
                }
            }
            return true;
        }

        // Виводить найбільше значення та його номер у таблиці (з одиниці)
        static void PrintMax(double[] values, int count)
        {
            double max;
            int index;

            if (!FindMax(values, count, out max, out index))
            {
                Console.WriteLine("Масив порожній, максимуму немає.");
                return;
            }

            Console.WriteLine($"Найбільша вартість: {max:F2} грн, запис №{index + 1}.");
        }

        // Видаляє елемент за позицією index (з нуля), зсуваючи решту вліво.
        // Лічильник — ref, бо він зменшується.
        static void RemoveAt(double[] values, ref int count, int index)
        {
            if (index < 0 || index >= count)
            {
                Console.WriteLine("Помилка: запису з таким номером немає.");
                return;
            }

            for (int i = index; i < count - 1; i++)
            {
                values[i] = values[i + 1];
            }

            count--;
            values[count] = 0; // очищаємо зайвий хвіст
            Console.WriteLine($"Запис видалено. Записів: {count}.");
        }

        // Рахує суму й середнє. Для порожнього набору середнє дорівнює нулю,
        // щоб не ділити на нуль.
        static void Statistics(double[] values, int count, out double sum, out double average)
        {
            sum = 0;
            for (int i = 0; i < count; i++)
            {
                sum += values[i];
            }

            average = count > 0 ? sum / count : 0;
        }

        // Виводить кількість записів, суму й середнє
        static void PrintStatistics(double[] values, int count)
        {
            if (count == 0)
            {
                Console.WriteLine("Масив порожній, статистики немає.");
                return;
            }

            double sum;
            double average;
            Statistics(values, count, out sum, out average);

            Console.WriteLine($"Записів: {count}");
            Console.WriteLine($"Сума: {sum:F2} грн");
            Console.WriteLine($"Середнє: {average:F2} грн");
        }

        // Намагається змінити int-параметр без ref: змінюється лише копія
        static void TryIncreaseNumber(int value)
        {
            value = value + 100;
        }

        // Змінює елемент масиву: масив передається посиланням, тому зміну видно назовні
        static void IncreaseFirstElement(int[] prices)
        {
            prices[0] = prices[0] + 100;
        }

        // Демонстрація різниці між значимим і посилальним типом
        static void ShowPassingDemo()
        {
            int price = 50;
            int[] prices = { 50, 70, 90 };

            Console.WriteLine($"До виклику:    price = {price}, prices[0] = {prices[0]}");
            TryIncreaseNumber(price);
            IncreaseFirstElement(prices);
            Console.WriteLine($"Після виклику: price = {price}, prices[0] = {prices[0]}");

            Console.WriteLine("int передався копією, тому price не змінився.");
            Console.WriteLine("Масив передався посиланням, тому prices[0] змінився.");
        }
        // Читає дійсне число з клавіатури в межах [min, max].
        // Питає знову, доки не отримає прийнятне значення.
        static double ReadDouble(string prompt, double min, double max)
        {
            while (true)
            {
                Console.Write($"{prompt} ({min:0.##}–{max:0.##}): ");
                string text = Console.ReadLine();

                // Крапка й кома однаково приймаються як роздільник
                if (text != null)
                {
                    text = text.Trim().Replace(',', '.');
                }

                double value;
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                    || double.IsNaN(value) || double.IsInfinity(value))
                {
                    Console.WriteLine("Помилка: потрібно ввести число.");
                    continue;
                }
                if (value < min || value > max)
                {
                    Console.WriteLine($"Помилка: число має бути від {min:0.##} до {max:0.##}.");
                    continue;
                }
                return value;
            }
        }

        // Читає ціле число з клавіатури в межах [min, max].
        // Питає знову, доки не отримає прийнятне значення.
        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write($"{prompt} ({min}–{max}): ");
                string text = Console.ReadLine();

                int value;
                if (!int.TryParse(text, out value))
                {
                    Console.WriteLine("Помилка: потрібно ввести ціле число.");
                    continue;
                }
                if (value < min || value > max)
                {
                    Console.WriteLine($"Помилка: число має бути від {min} до {max}.");
                    continue;
                }
                return value;
            }
        }
    }
}