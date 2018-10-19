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
        // Event class: processor tick
        public class FIFO : TimeModelEvent<SmoModel>
        {       
            protected override void HandleEvent(ModelEventArgs args)
            {
                if (Model.inputPagesFifo[0].callTime == Model.tickNumber)
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

                    String outString = Model.tickNumber.ToString() + "\t";

                    for (int i = Model.QFIFO.Count - 1; i >= 0; i--)
                    {
                        outString += " " + Model.QFIFO[i].number.ToString();
                    }

                    Model.Tracer.AnyTrace(outString);
                }

                Model.tickNumber++;

                if (Model.inputPagesFifo.Count != 0)
                {
                    var ev = new FIFO();
                    Model.PlanEvent(ev, 1.0);
                }
                else
                {
                    Model.tickNumber = 1;
                    //Model.isFinish = true;
                    var ev = new WorkingSet();
                    Model.PlanEvent(ev, 1.0);
                }
            }
        }

        // Event class: processor tick
        public class WorkingSet : TimeModelEvent<SmoModel>
        {       
            protected override void HandleEvent(ModelEventArgs args)
            {
                if (Model.workPagesWS.Count > 0)
                {
                    for (int i = 0; i < Model.workPagesWS.Count; i++)
                    {
                        if (Model.tickNumber % 2 == 1)
                        {
                            if (Model.workPagesWS[i].callBit == true)
                            {
                                Model.workPagesWS[i].callBit = false;
                                Model.workPagesWS[i].callTime = Model.tickNumber - 1;
                            }
                        }
                        //Model.workPagesWS[i].callTime = Model.tickNumber - 1;
                        Model.workPagesWS[i].timeDifference = Model.tickNumber - Model.workPagesWS[i].callTime;
                    }
                }

                if (Model.inputPagesWS[0].callTime == Model.tickNumber)
                {
                    bool isProcessWithCurrentNumber = false;
                    int processNumber = -1;
                    for (int i = 0; i < Model.workPagesWS.Count; i++)
                    {
                        if (Model.workPagesWS[i].number == Model.inputPagesWS[0].number) 
                        {
                            isProcessWithCurrentNumber = true;
                            processNumber = i;
                            break;
                        }
                    }

                    if (isProcessWithCurrentNumber)
                    {
                        if (Model.workPagesWS[processNumber].callBit == true && Model.tickNumber % 2 == 1)
                        {
                            Model.workPagesWS[processNumber].callTime = Model.tickNumber - 1;
                            Model.workPagesWS[processNumber].timeDifference = Model.tickNumber - Model.workPagesWS[processNumber].callTime;
                        }
                        else
                        {
                            Model.workPagesWS[processNumber].callBit = true;
                        }
                    }
                    else
                    {
                        if (Model.workPagesWS.Count < Model.activePageAmount)
                        {
                            Model.workPagesWS.Add(new WorkPage(Model.inputPagesWS[0].number, true, 0, Model.tickNumber));
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

                            Model.Tracer.AnyTrace(worstPageNumber);
                            Model.workPagesWS[worstPageNumber] = new WorkPage(Model.inputPagesWS[0].number, true, 0, Model.tickNumber);
                            Model.pageFaultsAmountWS++;
                        }
                    }

                    Model.inputPagesWS.RemoveAt(0);

                    String outString = Model.tickNumber.ToString() + "\t";

                    for (int i = 0; i < Model.workPagesWS.Count; i++)
                    {
                        outString += Model.workPagesWS[i].number.ToString() + " " +
                                          (Convert.ToInt32(Model.workPagesWS[i].callBit)).ToString() + " " +
                                          Model.workPagesWS[i].callTime.ToString() + " " +
                                          Model.workPagesWS[i].timeDifference.ToString() + "  ";
                    }

                    Model.Tracer.AnyTrace(outString);
                }

                Model.tickNumber++;

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
