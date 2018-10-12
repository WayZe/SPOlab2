using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommonModel.StatisticsCollecting;
using CommonModel.RandomStreamProducing;
using CommonModel.Collections;
using CommonModel.Kernel;

namespace Model_Lab
{

    public partial class SmoModel : Model
    {

        #region Параметры модель

        bool isFinish;

        List<int> inputPages = new List<int>();

        #endregion

        #region Дополнительные структуры

        /* Process */
        public class Process : QueueRecord
        {
            /* Number of process*/
            public int number;
            /* Required number of ticks for execution */
            public int requiredAmount;
            /* Priority */
            public int priority;
            /* Readiness time */
            public int readinessTime;

            public Process(int _number, int _readinessTime, int _requiredAmount, int _priority)
            {
                number = _number;
                requiredAmount = _requiredAmount;
                priority = _priority;
                readinessTime = _readinessTime;
            }
        }

        /* Queue for FIFO */
        SimpleModelList<Process> QFIFO;

        #endregion

        #region Cборщики статистики

        #endregion

        #region Инициализация объектов модели

        public SmoModel(Model parent, string name) : base(parent, name)
        {
            QFIFO = InitModelObject<SimpleModelList<Process>>();
        }

        #endregion
    }
}
