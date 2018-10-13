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
                    Model.isFinish = true;
                }
            }
        }

        // Event class: processor tick
        public class WorkingSet : TimeModelEvent<SmoModel>
        {       
            protected override void HandleEvent(ModelEventArgs args)
            {
            }
        }
    }
}
