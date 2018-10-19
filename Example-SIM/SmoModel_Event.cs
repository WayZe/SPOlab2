using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonModel.Kernel;
using System.IO;

namespace Model_Lab
{
    public partial class SmoModel : Model
    {
        /* Intreface declaring general functions for FIFO and Working Set */
        public interface IPage
        {
            /* Adding page in RAM */
            void AddPage();

            /* Printing tracing */
            void PrintTracing();

            /* Moving on to the next cycle */
            void GoToNextCycle();
        }

        // Event class: FIFO
        public class FIFO : TimeModelEvent<SmoModel>, IPage
        {
            protected override void HandleEvent(ModelEventArgs args)
            {
                AddPage();

                Model.cycleNumber++;

                GoToNextCycle();
            }

            public void AddPage()
            {
                if (Model.inputPagesFifo[0].callTime == Model.cycleNumber)
                {
                    Model.QFIFO.Add(new Page(Model.inputPagesFifo[0].number, Model.inputPagesFifo[0].callTime, Model.inputPagesFifo[0].isPageChange));

                    Model.inputPagesFifo.RemoveAt(0);

                    if (Model.QFIFO.Count == Model.activePageAmount + 1)
                    {
                        Model.pageFaultsAmountFifo++;
                        Model.QFIFO.RemoveAt(0);
                    }
                    else if (Model.QFIFO.Count > Model.activePageAmount + 1)
                    {
                        Model.Tracer.AnyTrace("Something went wrong");
                    }

                    PrintTracing();
                }
            }

            public void PrintTracing()
            {
                String outString = Model.cycleNumber.ToString() + "\t";

                for (int i = Model.QFIFO.Count - 1; i >= 0; i--)
                {
                    outString += " " + Model.QFIFO[i].number.ToString();
                }

                Model.Tracer.AnyTrace(outString);
            }

            public void GoToNextCycle()
            {
                if (Model.inputPagesFifo.Count != 0)
                {
                    var ev = new FIFO();
                    Model.PlanEvent(ev, 1.0);
                }
                else
                {
                    Model.cycleNumber = 1;
                    var ev = new WorkingSet();
                    Model.PlanEvent(ev, 1.0);
                }
            }
        }

        // Event class: Working Set
        public class WorkingSet : TimeModelEvent<SmoModel>, IPage
        {
            protected override void HandleEvent(ModelEventArgs args)
            {
                ExecutePagePreProcessing();

                AddPage();

                Model.cycleNumber++;

                GoToNextCycle();
            }

            /* Updating pages that are in RAM */
            private void ExecutePagePreProcessing()
            {
                if (Model.workPagesWS.Count > 0)
                {
                    for (int i = 0; i < Model.workPagesWS.Count; i++)
                    {
                        if (Model.cycleNumber % 2 == 1)
                        {
                            if (Model.workPagesWS[i].callBit == true)
                            {
                                Model.workPagesWS[i].callBit = false;
                                Model.workPagesWS[i].callTime = Model.cycleNumber - 1;
                            }
                        }
                        //Model.workPagesWS[i].callTime = Model.tickNumber - 1;
                        Model.workPagesWS[i].timeDifference = Model.cycleNumber - Model.workPagesWS[i].callTime;
                    }
                }
            }

            /* Searching page with current number in RAM */
            private bool SearchPageWithCurrentNumber(out int processNumber)
            {
                bool isProcessWithCurrentNumber = false;
                processNumber = -1;

                for (int i = 0; i < Model.workPagesWS.Count; i++)
                {
                    if (Model.workPagesWS[i].number == Model.inputPagesWS[0].number)
                    {
                        isProcessWithCurrentNumber = true;
                        processNumber = i;
                        break;
                    }
                }

                return isProcessWithCurrentNumber;
            }

            public void AddPage()
            {
                if (Model.inputPagesWS[0].callTime == Model.cycleNumber)
                {
                    if (SearchPageWithCurrentNumber(out int processNumber))
                    {
                        ProcessRamPage(processNumber);
                    }
                    else
                    {
                        ProcessNotRamPage();
                    }

                    PrintTracing();

                    Model.inputPagesWS.RemoveAt(0);
                }
            }

            /* Processing addition of page that is in RAM */
            private void ProcessRamPage(int processNumber)
            {
                if (Model.workPagesWS[processNumber].callBit == true && Model.cycleNumber % 2 == 1)
                {
                    Model.workPagesWS[processNumber].callTime = Model.cycleNumber - 1;
                    Model.workPagesWS[processNumber].timeDifference = Model.cycleNumber - Model.workPagesWS[processNumber].callTime;
                }
                else
                {
                    Model.workPagesWS[processNumber].callBit = true;
                }
            }

            /* Processing addition of page that is not in RAM */
            private void ProcessNotRamPage()
            {
                if (Model.workPagesWS.Count < Model.activePageAmount)
                {
                    Model.workPagesWS.Add(new WorkPage(Model.inputPagesWS[0].number, true, 0, Model.cycleNumber));
                }
                else
                {
                    int worstPageNumber = 0;
                    for (int i = 1; i < Model.workPagesWS.Count; i++)
                    {
                        if (Model.workPagesWS[worstPageNumber].timeDifference * Convert.ToInt32(!Model.workPagesWS[worstPageNumber].callBit)
                            <= Model.workPagesWS[i].timeDifference * Convert.ToInt32(!Model.workPagesWS[i].callBit))
                        {
                            worstPageNumber = i;
                        }
                    }

                    Model.workPagesWS[worstPageNumber] = new WorkPage(Model.inputPagesWS[0].number, true, 0, Model.cycleNumber);
                    Model.pageFaultsAmountWS++;
                }
            }

            public void PrintTracing()
            {
                String outString = Model.inputPagesWS[0].number.ToString() + "\t" + Model.cycleNumber.ToString() + "\t";

                for (int i = 0; i < Model.workPagesWS.Count; i++)
                {
                    outString += Model.workPagesWS[i].number.ToString() + " " +
                                      (Convert.ToInt32(Model.workPagesWS[i].callBit)).ToString() + " " +
                                      Model.workPagesWS[i].callTime.ToString() + " " +
                                      Model.workPagesWS[i].timeDifference.ToString() + "  ";
                }

                Model.Tracer.AnyTrace(outString);
            }

            public void GoToNextCycle()
            {
                if (Model.inputPagesWS.Count != 0)
                {
                    var ev = new WorkingSet();
                    Model.PlanEvent(ev, 1.0);
                }
                else
                {
                    Model.isFinish = true;
                }
            }
        }
    }
}
