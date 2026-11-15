using System;
using System.Globalization;
using System.Text;

namespace AutoService
{
    internal class Program
    {
        // Довідник видів робіт (для таблиці з двома входами)
        static readonly string[] WorkTypes =
        {
            "Діагностика", "Заміна мастила", "Гальма",
            "Ходова", "Двигун", "Електрика"
        };

        // Середня вартість одного замовлення за видом робіт, грн
        static readonly double[] WorkPrices = { 300, 450, 800, 1200, 3500, 900 };

        static readonly string[] Days = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд" };

        const double HourRate = 450;     // вартість нормо-години, грн
        const double PartsShare = 0.15;  // частка запчастин від вартості робіт
        const double PartsFixed = 200;   // фіксована частина запчастин, грн
        const double MaxHours = 24;      // найбільша тривалість ремонту, год

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            bool running = true;

            while (running)
            {
                ShowMenu();
                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1": ShowAbout(); Pause(); break;
                    case "2": ShowCostTable(); Pause(); break;
                    case "3": ShowWorkloadTable(); Pause(); break;
                    case "0": running = false; break;
                    default: ShowUnknownChoice(); Pause(); break;
                }
            }

            Console.WriteLine("До побачення.");
        }

        // Виводить головне меню
        static void ShowMenu()
        {
            Console.WriteLine("=== Автосервіс ===");
            Console.WriteLine("1 — довідка про програму");
            Console.WriteLine("2 — таблиця вартості ремонту за годинами");
            Console.WriteLine("3 — звіт: виторг за видами робіт і днями тижня");
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
        static void ShowWorkloadTable()
        {
            int rows = ReadInt("Кількість рядків (видів робіт)", 1, WorkTypes.Length);
            int cols = ReadInt("Кількість стовпців (днів тижня)", 1, Days.Length);

            Console.WriteLine();
            Console.WriteLine("Виторг за видами робіт і днями тижня, грн");

            PrintWorkloadHeader(cols);
            for (int r = 0; r < rows; r++)
            {
                PrintWorkloadRow(r, cols);
            }
            PrintWorkloadFooter(rows, cols);
        }

        // Виводить шапку: дні тижня й стовпець підсумку
        static void PrintWorkloadHeader(int cols)
        {
            Console.Write($"{"",-16}");
            for (int c = 0; c < cols; c++)
            {
                Console.Write($"{Days[c],9}");
            }
            Console.WriteLine($"{"Разом",12}");
        }

        // Виводить один рядок таблиці: вид робіт, виторг по днях і підсумок рядка
        static void PrintWorkloadRow(int r, int cols)
        {
            Console.Write($"{WorkTypes[r],-16}");
            for (int c = 0; c < cols; c++)
            {
                Console.Write($"{CalculateRevenue(r, c),9:F0}");
            }
            Console.WriteLine($"{CalculateRowTotal(r, cols),12:F0}");
        }

        // Виводить рядок підсумків по стовпцях і загальний підсумок
        static void PrintWorkloadFooter(int rows, int cols)
        {
            Console.Write($"{"Разом",-16}");
            double grandTotal = 0;
            for (int c = 0; c < cols; c++)
            {
                double columnTotal = CalculateColumnTotal(c, rows);
                Console.Write($"{columnTotal,9:F0}");
                grandTotal += columnTotal;
            }
            Console.WriteLine($"{grandTotal,12:F0}");
        }

        // Повертає виторг виду робіт r у день c (кількість замовлень × ціна)
        static double CalculateRevenue(int r, int c)
        {
            int orders = ((r + 2) * (c + 3) + c) % 5 + 1; // умовна кількість замовлень
            return orders * WorkPrices[r];
        }

        // Повертає суму виторгу виду робіт r за перші cols днів
        static double CalculateRowTotal(int r, int cols)
        {
            double total = 0;
            for (int c = 0; c < cols; c++)
            {
                total += CalculateRevenue(r, c);
            }
            return total;
        }

        // Повертає суму виторгу дня c за перші rows видів робіт
        static double CalculateColumnTotal(int c, int rows)
        {
            double total = 0;
            for (int r = 0; r < rows; r++)
            {
                total += CalculateRevenue(r, c);
            }
            return total;
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
