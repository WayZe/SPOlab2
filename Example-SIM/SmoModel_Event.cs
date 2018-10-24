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
        /* General functions for FIFO and Working Set */
        public interface IPage
        {
            /* Adding page in RAM */
            void AddPage();

            /// <summary>
            /// Printing tracing. If [isPageFault] == true, add " P" in end of string
            /// </summary>
            /// <param name="isPageFault"> True if there is page fault </param>
            void PrintTracing(bool isPageFault);

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
                bool isPageFault = false;
                if (Model.inputPagesFifo[0].callTime == Model.cycleNumber)
                {
                    Model.QFIFO.Add(new Page(Model.inputPagesFifo[0].number, Model.inputPagesFifo[0].callTime, Model.inputPagesFifo[0].isPageChange));

                    if (Model.QFIFO.Count == Model.activePageAmount + 1)
                    {
                        Model.pageFaultsAmountFifo++;
                        Model.QFIFO.RemoveAt(0);
                        isPageFault = true;
                    }
                    else if (Model.QFIFO.Count > Model.activePageAmount + 1)
                    {
                        Model.Tracer.AnyTrace("Something went wrong");
                    }

                    PrintTracing(isPageFault);

                    Model.inputPagesFifo.RemoveAt(0);
                }
            }

            public void PrintTracing(bool isPageFault)
            {
                String outString = Model.inputPagesFifo[0].number.ToString() + "\t" + Model.cycleNumber.ToString() + "\t";

                for (int i = Model.QFIFO.Count - 1; i >= 0; i--)
                {
                    outString += Model.QFIFO[i].number.ToString() + " ";
                }

                if (isPageFault)
                {
                    outString += " P";
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
                    Model.Tracer.AnyTrace("");
                    Model.Tracer.AnyTrace("Working Set");
                    Model.Tracer.AnyTrace("");
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
                        Model.workPagesWS[i].timeDifference = Model.cycleNumber - Model.workPagesWS[i].callTime;
                    }
                }
            }

            /// <summary>
            /// Searching page with current number in RAM 
            /// </summary>
            /// <param name="pageNumber">  </param>
            /// <returns> True if there is page with input page number </returns>
            private bool SearchPageWithCurrentNumber(out int pageNumber)
            {
                bool isPageWithCurrentNumber = false;
                pageNumber = -1;

                for (int i = 0; i < Model.workPagesWS.Count; i++)
                {
                    if (Model.workPagesWS[i].number == Model.inputPagesWS[0].number)
                    {
                        isPageWithCurrentNumber = true;
                        pageNumber = i;
                        break;
                    }
                }

                return isPageWithCurrentNumber;
            }

            public void AddPage()
            {
                bool isPageFault = false;
                if (Model.inputPagesWS[0].callTime == Model.cycleNumber)
                {
                    if (SearchPageWithCurrentNumber(out int pageNumber))
                    {
                        ProcessRamPage(pageNumber);
                    }
                    else
                    {
                        ProcessNotRamPage(out isPageFault);
                    }

                    PrintTracing(isPageFault);

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
            private void ProcessNotRamPage(out bool isPageFault)
            {
                isPageFault = false;
                if (Model.workPagesWS.Count < Model.activePageAmount)
                {
                    Model.workPagesWS.Add(new WorkPage(Model.inputPagesWS[0].number, true, 0, Model.cycleNumber));
                }
                else
                {
                    int worstPageNumber = 0;
                    int k = 0;
                    /* Searching first element with [timeDufference] less than [maxTimeDiffernce] */
                    for (int i = k; i < Model.workPagesWS.Count; i++)
                    {
                        if (Model.workPagesWS[i].timeDifference > Model.maxTimeDifference)
                        {
                            worstPageNumber = i;
                            k = ++i;
                            break;
                        }
                    }


                    for (int i = k; i < Model.workPagesWS.Count; i++)
                    {
                        if (Model.workPagesWS[i].timeDifference > Model.maxTimeDifference)
                        {
                            if (Model.workPagesWS[worstPageNumber].timeDifference * Convert.ToInt32(!Model.workPagesWS[worstPageNumber].callBit)
                                <= Model.workPagesWS[i].timeDifference * Convert.ToInt32(!Model.workPagesWS[i].callBit))
                            {
                                worstPageNumber = i;
                                break;
                            }
                        }
                        else
                        {
                            // TODO
                        }
                    }

                    Model.workPagesWS[worstPageNumber] = new WorkPage(Model.inputPagesWS[0].number, true, 0, Model.cycleNumber);
                    Model.pageFaultsAmountWS++;
                    isPageFault = true;
                }
            }

            public void PrintTracing(bool isPageFault)
            {
                String outString = Model.inputPagesWS[0].number.ToString() + "\t" + Model.cycleNumber.ToString() + "\t";

                for (int i = 0; i < Model.workPagesWS.Count; i++)
                {
                    outString += Model.workPagesWS[i].number.ToString() + " " +
                                      (Convert.ToInt32(Model.workPagesWS[i].callBit)).ToString() + " " +
                                      Model.workPagesWS[i].callTime.ToString() + " " +
                                      Model.workPagesWS[i].timeDifference.ToString() + "  ";
                }

                if (isPageFault)
                {
                    outString += " P";
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
