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

            #endregion

            #region Установка параметров законов распределения

            //(generator.BPN as GeneratedBaseRandomStream).Seed = 1;
            //generator.A = 0;
            //generator.B = 2;

            #endregion
        }

        public override void StartModelling(int variantCount, int runCount)
        {


            //Печать заголовка строки состояния модели
            TraceModelHeader();

            #region Планирование начальных событий      

            #endregion
        }

        public void ReadFile()
        {
            StreamReader streamReader = new StreamReader(@"D:\Langs\C#\SPOlab2\input.txt");

            String[] tmp = streamReader.ReadLine().Split();

            for (int i = 0; i < tmp.Length; i++)
            {
                inputPages.Add(Convert.ToInt32(tmp[i]));
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

