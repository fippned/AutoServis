using System;
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

        // Табулювання з фіксованими межами
        static void ShowCostTable()
        {
            PrintCostTable(0.5, 5.0, 0.5);
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
            int rows, cols;

            if (!ReadSize("Кількість рядків (видів робіт)", WorkTypes.Length, out rows))
                return;
            if (!ReadSize("Кількість стовпців (днів тижня)", Days.Length, out cols))
                return;

            Console.WriteLine();
            Console.WriteLine("Виторг за видами робіт і днями тижня, грн");

            // Шапка
            Console.Write($"{"",-16}");
            for (int c = 0; c < cols; c++)
            {
                Console.Write($"{Days[c],9}");
            }
            Console.WriteLine($"{"Разом",12}");

            double[] colTotals = new double[cols];
            double grandTotal = 0;

            for (int r = 0; r < rows; r++)
            {
                Console.Write($"{WorkTypes[r],-16}");
                double rowTotal = 0; // підсумок рядка — свій для кожного рядка

                for (int c = 0; c < cols; c++)
                {
                    int orders = ((r + 2) * (c + 3) + c) % 5 + 1; // кількість замовлень
                    double sum = orders * WorkPrices[r];

                    Console.Write($"{sum,9:F0}");
                    rowTotal += sum;
                    colTotals[c] += sum;
                }

                Console.WriteLine($"{rowTotal,12:F0}");
                grandTotal += rowTotal;
            }

            // Рядок підсумків
            Console.Write($"{"Разом",-16}");
            for (int c = 0; c < cols; c++)
            {
                Console.Write($"{colTotals[c],9:F0}");
            }
            Console.WriteLine($"{grandTotal,12:F0}");
        }

        // Читає розмір таблиці; false, якщо ввід неправильний
        static bool ReadSize(string prompt, int max, out int value)
        {
            Console.Write($"{prompt} (1–{max}): ");
            string text = Console.ReadLine();

            if (!int.TryParse(text, out value))
            {
                Console.WriteLine("Помилка: потрібно ввести ціле число.");
                return false;
            }
            if (value <= 0)
            {
                Console.WriteLine("Помилка: число має бути більшим за нуль.");
                return false;
            }
            if (value > max)
            {
                Console.WriteLine($"Помилка: максимум — {max}.");
                return false;
            }
            return true;
        }
    }
}