using System;
using CommonModel.Kernel;
using CommonModel.RandomStreamProducing;
using System.Collections.Generic;
using System.IO;

namespace Model_Lab
{

    public partial class SmoModel : Model
    {
        //Условие завершения прогона модели True - завершить прогон. По умолчанию false. </summary>
        public override bool MustStopRun(int variantCount, int runCount)
        {
            return (isFinish);
        }

        //установка метода перебора вариантов модели
        public override bool MustPerformNextVariant(int variantCount)
        {
            //используем один вариант модели
            return variantCount < 1;
        }

        //true - продолжить выполнение ПРОГОНОВ модели;
        //false - прекратить выполнение ПРОГОНОВ модели. по умолчению false.
        public override bool MustPerformNextRun(int variantCount, int runCount)
        {
            return runCount < 1; //выполняем 1 прогон модели
        }

        //Задание начального состояния модели для нового варианта модели
        public override void SetNextVariant(int variantCount)
        {
            #region Параметры модели

            /* If true then program end */
            isFinish = false;

            /* Pages in RAM */
            activePageAmount = -1;

            /* Number of current cycle */
            cycleNumber = 1;

            inputPageAmount = -1;

            /* Amount of page faults for FIFO */
            pageFaultsAmountFifo = 0;

            /* Amount of page faults for Working Set */
            pageFaultsAmountWS = 0;

            /* If time diffrence less than [maxTimeDifference] then page is in working set */
            maxTimeDifference = -1;

            /* Time when call bit will be reset */
            resetCallBitTime = -1;

            /* Amount of unique input page */
            uniquePageAmount = -1;

            #endregion
        }

        public override void StartModelling(int variantCount, int runCount)
        {
            /* Reading start parametrs from screen */
            if (Convert.ToBoolean(ReadFromScreen()))
            {
                GenerateInputFile();
                /* Reading input file */
                ReadFile(@"inputg.txt");
            }
            else
            {
                /* Reading input file */
                ReadFile(@"input.txt");
            }

            //Печать заголовка строки состояния модели
            TraceModelHeader();

            #region Планирование начальных событий      
            
            Tracer.AnyTrace("");
            Tracer.AnyTrace("FIFO");
            Tracer.AnyTrace("");
            /* Planning startd event */
            var ev = new FIFO();
            PlanEvent(ev, 0.0);

            #endregion
        }

        /// <summary>
        /// Generating file with the given parameters
        /// </summary>
        private void GenerateInputFile()
        {
            StreamWriter streamWriter;

            if (Environment.OSVersion.Platform.ToString() == "Win32NT")
            {
                streamWriter = new StreamWriter(@"inputg.txt");

            }
            else
            {
                streamWriter = new StreamWriter(@"inputg.txt");
            }

            Random rnd = new Random();
            int i = 1;
            while (i <= inputPageAmount)
            { 
                streamWriter.WriteLine(rnd.Next(0, uniquePageAmount).ToString() + " " + i.ToString() + " " + 0.ToString());
                i++;
            }
            streamWriter.Flush();

            streamWriter.Close();
        }

        /// <summary>
        /// Reading amount of input pages from screen 
        /// </summary>
        /// <returns> Amount of input pages </returns>
        private int ReadFromScreen()
        {
            if (Environment.OSVersion.Platform.ToString() == "Win32NT")
            {
                inputPageAmount = ReadValueFromScreen("Введите количество обращений \r\n(если желаете оставить базовые настройки введите 0): ");

                if (inputPageAmount != 0)
                {
                    activePageAmount = ReadValueFromScreen("Введите количество страниц в памяти ");

                    resetCallBitTime = ReadValueFromScreen("Введите время сброса бита обращения ");

                    maxTimeDifference = ReadValueFromScreen("Введите время хранения в рабочем множестве ");

                    uniquePageAmount = ReadValueFromScreen("Введите количество уникальных страниц ");
                }
                else
                {
                    /* Base settings */
                    activePageAmount = 3;
                    resetCallBitTime = 2;
                    maxTimeDifference = 5;
                    uniquePageAmount = 5;
                }

                return inputPageAmount;
            }

            return 0;
        }

        private int ReadValueFromScreen(String message)
        {
            int value = -1;

            while (value < 0)
            {
                try
                {
                    Console.Write(message);
                    value = Convert.ToInt32(Console.ReadLine());
                }
                catch
                {
                }
            }

            return value;
        }

        /// <summary>
        /// Reading information about pages from file
        /// </summary>
        /// <param name="path"> Path to file from which is reading </param>
        private void ReadFile(String path)
        {
            try
            {
                StreamReader streamReader;

                if (Environment.OSVersion.Platform.ToString() == "Win32NT")
                {
                    streamReader = new StreamReader(path);

                }
                else
                {
                    streamReader = new StreamReader(@"/Users/andreymakarov/Downloads/SPOlab2/input.txt");
                }

                while (!streamReader.EndOfStream)
                {
                    String[] tmp = streamReader.ReadLine().Split(' ');
                    inputPagesFifo.Add(new Page(Convert.ToInt32(tmp[0]), Convert.ToInt32(tmp[1]), Convert.ToBoolean(Convert.ToInt32(tmp[2]))));
                    inputPagesWS.Add(new Page(Convert.ToInt32(tmp[0]), Convert.ToInt32(tmp[1]), Convert.ToBoolean(Convert.ToInt32(tmp[2]))));
                }

                inputPageAmount = inputPagesFifo.Count;
            }
            catch
            {
                Console.WriteLine("\r\nНе найден файл input.txt. \r\nСоздайте этот файл и положите его рядом с исполняемым. \r\nИли выберите не базовый вариант.\r\n");
                Console.ReadKey();
                Environment.Exit(-1);
            }
        }

        //Действия по окончанию прогона
        public override void FinishModelling(int variantCount, int runCount)
        {
            Tracer.AnyTrace("");
            Tracer.TraceOut("==============================================================");
            Tracer.TraceOut("============Статистические результаты моделирования===========");
            Tracer.TraceOut("==============================================================");
            Tracer.AnyTrace("");

            Tracer.AnyTrace("FIFO page faults = " + pageFaultsAmountFifo);
            Tracer.AnyTrace("WorkingSet page faults = " + pageFaultsAmountWS);

            Console.WriteLine("\n*Трассировка также сохранена в файл trace.txt");

            Console.ReadKey();
        }

        //Печать заголовка
        void TraceModelHeader()
        {
            Tracer.TraceOut("==============================================================");
            Tracer.TraceOut("======================= Запущена модель ======================");
            Tracer.TraceOut("==============================================================");
            //вывод заголовка трассировки
            //Tracer.AnyTrace("");
            //Tracer.AnyTrace("Параметры модели:");
            //Tracer.AnyTrace("");
        }

        //Печать строки состояния
        void TraceModel()
        {
        }

    }
}

