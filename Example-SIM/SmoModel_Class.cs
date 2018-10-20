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

        public bool isFinish;

        public List<Page> inputPagesFifo = new List<Page>();

        public List<Page> inputPagesWS = new List<Page>();

        public List<WorkPage> workPagesWS = new List<WorkPage>();

        public int activePageAmount;

        public int inputPageAmount;

        public int cycleNumber;

        public int pageFaultsAmountFifo;

        public int pageFaultsAmountWS;

        #endregion

        #region Дополнительные структуры

        /* Page */
        public class Page : QueueRecord
        {
            /* Page number */
            public int number;
            /* Page call time */
            public int callTime;
            /* True -  if page has been changed*/
            public bool isPageChange;

            public Page(int _number, int _callTime, bool _isPageChange)
            {
                number = _number;
                callTime = _callTime;
                isPageChange = _isPageChange;
            }
        }

        /* Page IN RAM */
        public class WorkPage : QueueRecord
        {
            /* Page number */
            public int number;
            /* Call bit */
            public bool callBit;
            /* Call time */
            public int callTime;
            /* Time difference */
            public int timeDifference;

            public WorkPage(int _number, bool _callBit, int _callTime, int _timeDifference)
            {
                number = _number;
                callBit = _callBit;
                callTime = _callTime;
                timeDifference = _timeDifference;
            }
        }

        /* Queue for FIFO */
        SimpleModelList<Page> QFIFO;

        #endregion

        #region Инициализация объектов модели

        public SmoModel(Model parent, string name) : base(parent, name)
        {
            QFIFO = InitModelObject<SimpleModelList<Page>>();
        }

        #endregion
    }
}
