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

            isFinish = false;
            
            activePageAmount = 3;

            tickNumber = 1;
            
            pageFaultsAmountFifo = 0;

            pageFaultsAmountWS = 0;

            #endregion
        }

        public override void StartModelling(int variantCount, int runCount)
        {
            //Печать заголовка строки состояния модели
            TraceModelHeader();

            ReadFile();

            #region Планирование начальных событий      

            var ev = new FIFO();
            PlanEvent(ev, 0.0);

            #endregion
        }

        public void ReadFile()
        {
            StreamReader streamReader;

            if (Environment.OSVersion.Platform.ToString() == "Win32NT")
            {
                streamReader = new StreamReader(@"D:\Langs\C#\SPOlab2\input.txt");

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
        }

        //Печать заголовка
        void TraceModelHeader()
        {
            Tracer.TraceOut("==============================================================");
            Tracer.TraceOut("======================= Запущена модель ======================");
            Tracer.TraceOut("==============================================================");
            //вывод заголовка трассировки
            Tracer.AnyTrace("");
            Tracer.AnyTrace("Параметры модели:");
            Tracer.AnyTrace("");
        }

        //Печать строки состояния
        void TraceModel()
        {
        }

    }
}

