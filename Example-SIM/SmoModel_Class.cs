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

        public int activePageAmount;

        public int tickNumber;

        public int pageFaultsAmountFifo;

        #endregion

        #region Дополнительные структуры

        /* Process */
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
