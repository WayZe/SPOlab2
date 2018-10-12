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
                 /* Adding processes in [QFIFO] */
                while (Model.QFIFO.Count < Model.processes.Count && Model.allProcesses.Count > 0)
                {
                    Model.QFIFO.Add(new Process(Model.allProcesses[0].number,
                                                /*Model.measureNumber*/ Model.allProcesses[0].readinessTime,
                                                Model.allProcesses[0].requiredAmount,
                                                Model.allProcesses[0].priority));
                    Model.allProcesses.RemoveAt(0);
                }

                /* Queue is not empty */
                if (Model.QFIFO.Count != 0)
                {
                    //Model.newProcess = true;
                    /* Printing of base tracing */
                    String outString = Model.tickNumber.ToString();
                    for (int i = 0; i < Model.processes.Count; i++)
                    {
                        bool isProcess = false;
                        for (int j = 0; j < Model.QFIFO.Count; j++)
                        {
                            /* Process is ready */
                            if (Model.QFIFO[j].readinessTime <= Model.tickNumber)
                            {
                                /* Process found in [QFIFO] */
                                if (Model.processes[i].number == Model.QFIFO[j].number)
                                {
                                    /* Process is first in [QFIFO] */
                                    if (j == 0)
                                    {
                                        isProcess = true;
                                        outString += " В";
                                        Model.fifoExecTime++;
                                        break;
                                    }
                                    /* Process is not first in [QFIFO] */
                                    else
                                    {
                                        isProcess = true;
                                        outString += " Г";
                                        Model.fifoWaitTime++;
                                        break;
                                    }
                                }
                            }
                            /* Process is not ready */
                            else
                            {
                                isProcess = true;
                                outString += " Б";
                                break;
                            }
                        }
                        /* Process not found in [QFIFO] */
                        if (!isProcess)
                        {
                            outString += " -";
                        }
                    }

                    Model.Tracer.AnyTrace(outString);
                    String[] elems1 = outString.Split();
                    String[] elems2;
                    if (Model.fifoTrace.Count != 0)
                    {
                        elems2 = Model.fifoTrace[Model.fifoTrace.Count - 1].Split();
                        Model.Tracer.AnyTrace(outString + "\n" + elems2.Length);
                        for (int i = 1; i < elems2.Length; i++)
                        {
                            if (Model.newProcess || Model.fifoTrace.Count == 0 || elems1[i] != elems2[i])
                            {
                                Model.fifoTrace.Add(outString);
                                break;
                            }
                        }
                    }
                    else
                    {
                        Model.fifoTrace.Add(outString);
                    }

                    //if (!Model.newProcess && (outString) != Model.fifoTrace[Model.fifoTrace.Count - 1] || Model.fifoTrace.Count == 0)
                    //{
                    //    Model.fifoTrace.Add(outString);
                    //}
                }
                /* Adding expected process in queue */
                //Model.Tracer.AnyTrace("Такт №" + Model.measureNumber);
                //int j = 0;
                //if (Model.allProcesses.Count > 0)
                //{
                //    while (j < Model.allProcesses.Count)
                //    {
                //        if (Model.allProcesses[j].readinessTime <= Model.measureNumber)
                //        {
                //            Model.Tracer.AnyTrace("Процесс №" + (Model.allProcesses[j].number) + " добавлен в очередь" + 
                //                                  " " + (Model.allProcesses[j].readinessTime) + " " + (Model.allProcesses[j].requiredAmount));

                //            Model.QFIFO.Add(
                //            new Process(Model.allProcesses[j].number,
                //                        Model.allProcesses[j].readinessTime,
                //                        Model.allProcesses[j].requiredAmount,
                //                        Model.allProcesses[j].priority));
                //            Model.allProcesses.RemoveAt(j);

                //            j--;
                //        }
                //        j++;
                //    }
                //}

                /* Process is ready */
                if (Model.QFIFO[0].readinessTime <= Model.tickNumber)
                {
                    Model.QFIFO[0].requiredAmount--;

                    if (Model.QFIFO[0].requiredAmount == 0)
                    {
                        Model.newProcess = true;
                    }
                    else
                    {
                        Model.newProcess = false;
                    }

                    /* First process is completed */
                    if (Model.QFIFO[0].requiredAmount == 0)
                    {
                        Model.QFIFO.RemoveAt(0);
                        Model.NCP++;

                        //if (Model.allProcesses.Count > 0)
                        //{
                        //    Random rand = new Random();
                        //    int index = rand.Next(0, Model.processes.Count);

                        //    //Model.Tracer.AnyTrace("GПроцесс №" + (Model.processes[index].number) + " добавлен в очередь" +
                        //    //" " + (Model.processes[index].readinessTime) + " " + (Model.processes[index].requiredAmount));

                        //    Model.waitProcesses.Add(new Process(Model.allProcesses[index].number,
                        //                                        Model.measureNumber + Model.allProcesses[index].readinessTime,
                        //                                        Model.allProcesses[index].requiredAmount,
                        //                                        Model.allProcesses[index].priority));

                        //    //Model.waitProcesses.Last().readinessTime = Model.measureNumber + Model.processes[index].readinessTime;
                        //}
                    }

                }

                /* Printing queue */
                for (int i = 0; i < Model.QFIFO.Count; i++)
                {
                    Model.Tracer.AnyTrace(Model.QFIFO[i].number + " " + Model.QFIFO[i].readinessTime + " " + Model.QFIFO[i].requiredAmount);
                }

                Model.tickNumber++;

                /* All processes did not end */
                if (Model.NCP < Model.maxNCP)
                {
                    var ev = new FIFO();
                    Model.PlanEvent(ev, 1.0);
                    //Model.Tracer.PlanEventTrace(ev);
                    Model.Tracer.AnyTrace("");
                }
                /* All processes ended; transition to SJF */
                else
                {
                    var ev = new SJF();
                    Model.PlanEvent(ev, 1.0);
                    Model.Tracer.AnyTrace("\n// SJF //\n");

                    /* Cleaning of variables */
                    Model.waitProcesses.Clear();
                    Model.fifoTickNumber = Model.tickNumber;
                    Model.tickNumber = 0;
                    Model.NCP = 0;

                    /* Initializing of processes start list for SJF */
                    foreach (Process process in Model.processes)
                    {
                        Model.waitProcesses.Add(new Process(process.number,
                                                      process.readinessTime,
                                                      process.requiredAmount,
                                                      process.priority));
                    }
                }
            }
        }

        // Event class: processor tick
        public class SJF : TimeModelEvent<SmoModel>
        {       
            protected override void HandleEvent(ModelEventArgs args)
            {
                bool isDelete = false;
                String outString = Model.tickNumber.ToString();
                /* Selecting of first process with the shortest readiness time or equal to it  */
                int i = 0;
                int minLength = -1;
                int minProcNumber = -1;
                int minReadinessTime = -1;
                for (i = 0; i < Model.waitProcesses.Count; i++)
                {
                    if (Model.waitProcesses[i].readinessTime <= Model.tickNumber)
                    {
                        minLength = Model.waitProcesses[i].requiredAmount;
                        minProcNumber = i;
                        minReadinessTime = Model.waitProcesses[i].readinessTime;
                        break;
                    }
                }

                if (minLength != -1)
                {
                    /* Selecting of process with minimal length and equal or less readiness time */
                    for (int j = i + 1; j < Model.waitProcesses.Count; j++)
                    {
                        /* Process is ready */
                        if (Model.waitProcesses[j].readinessTime <= Model.tickNumber)
                        {
                            if (Model.waitProcesses[j].requiredAmount < minLength)
                            {
                                minLength = Model.waitProcesses[j].requiredAmount;
                                minProcNumber = j;
                                minReadinessTime = Model.waitProcesses[j].readinessTime;
                            }
                            else if ((Model.waitProcesses[j].requiredAmount == minLength && Model.waitProcesses[j].readinessTime < minReadinessTime))
                            {
                                minLength = Model.waitProcesses[j].requiredAmount;
                                minProcNumber = j;
                                minReadinessTime = Model.waitProcesses[i].readinessTime;
                            }
                        }
                    }

                    /*  */
                    for (int j = 0; j < Model.waitProcesses.Count; j++)
                    {
                        if (Model.waitProcesses[j].readinessTime <= Model.tickNumber)
                        {
                            if ((Model.waitProcesses[j].requiredAmount == minLength && Model.waitProcesses[j].readinessTime < minReadinessTime))
                            {
                                minLength = Model.waitProcesses[j].requiredAmount;
                                minProcNumber = j;
                                minReadinessTime = Model.waitProcesses[j].readinessTime;
                            }
                        }
                    }

                    /* [waitProcess] is not empty */
                    if (Model.waitProcesses.Count != 0)
                    {
                        /* Printing base tracing */
                        for (int j = 0; j < Model.processes.Count; j++)
                        {
                            bool isProcess = false;
                            bool isBlockedProcess = false;
                            for (int k = 0; k < Model.waitProcesses.Count; k++)
                            {
                                /* There is current process from [waitProcesses] in [processes] */
                                if (Model.processes[j].number == Model.waitProcesses[k].number)
                                {
                                    /* Current process is ready */
                                    if (Model.waitProcesses[k].readinessTime <= Model.tickNumber)
                                    {
                                        /* Current process has minimal length */
                                        if (j == Model.waitProcesses[minProcNumber].number - 1)
                                        {
                                            outString += " В";
                                            isProcess = true;
                                            break;
                                        }
                                        /* Current process does not have minimal length */
                                        else
                                        {
                                            outString += " Г";
                                            isProcess = true;
                                            break;
                                        }
                                    }
                                    /* Current process is not ready */
                                    else
                                    {
                                            isBlockedProcess = true;
                                            isProcess = true;
                                    }
                                }
                            }

                            /* Current process is blocked */
                            if (isBlockedProcess)
                            {
                                outString += " Б";
                            }

                            /* [waitProcesses] does not have current process */
                            if (!isProcess)
                            {
                                outString += " -";
                            }
                        }
                        Model.Tracer.AnyTrace(outString);
                    }

                    Model.waitProcesses[minProcNumber].requiredAmount--;
                    /* One of the processes ended */
                    if (Model.waitProcesses[minProcNumber].requiredAmount == 0)
                    {
                        isDelete = true;
                        Model.waitProcesses.RemoveAt(minProcNumber);
                        Model.NCP++;
                        //Random rand = new Random();
                        //int index = rand.Next(0, Model.processes.Count);

                        /* Adding new process */
                        if (Model.allSjfProcesses.Count > 0)
                        {
                            Model.waitProcesses.Add(new Process(Model.allSjfProcesses[0].number,
                                                                Model.allSjfProcesses[0].readinessTime,
                                                                Model.allSjfProcesses[0].requiredAmount,
                                                                Model.allSjfProcesses[0].priority));
                            Model.allSjfProcesses.RemoveAt(0);
                        }
                    }
                }
                /* [waitProcess] does not have ready process */
                else
                {
                    List<bool> isProcesses = new List<bool>();
                    for (int j = 0; j < Model.waitProcesses.Count; j++)
                    {
                        for (int k = 0; k < Model.waitProcesses.Count; k++)
                        {
                            if (j == Model.waitProcesses[k].number - 1)
                            {
                                isProcesses.Add(true);
                            }
                        }

                        if (isProcesses.Count < j+1)
                        {
                            isProcesses.Add(false);
                        }
                    }

                    for (int j = 0; j < Model.waitProcesses.Count; j++)
                    {
                        if (isProcesses[j])
                        {
                            outString += " Б";
                        }
                        else
                        {
                            outString += " -";
                        }
                    }

                    Model.Tracer.AnyTrace(outString);
                }

                String[] elems1 = outString.Split();
                String[] elems2;
                if (Model.sjfTrace.Count != 0)
                {
                    elems2 = Model.sjfTrace[Model.sjfTrace.Count - 1].Split();
                    Model.Tracer.AnyTrace(outString + "\n" + elems2.Length);
                    for (int j = 1; j < elems2.Length; j++)
                    {
                        if (Model.newProcess || Model.sjfTrace.Count == 0 || elems1[j] != elems2[j])
                        {
                            Model.sjfTrace.Add(outString);
                            break;
                        }
                    }
                }
                else
                {
                    Model.sjfTrace.Add(outString);
                }

                Model.newProcess = isDelete;

                /* Printing queue */
                for (int k = 0; k < Model.waitProcesses.Count; k++)
                {
                    Model.Tracer.AnyTrace(Model.waitProcesses[k].number + " " + Model.waitProcesses[k].readinessTime + " " + Model.waitProcesses[k].requiredAmount);
                }

                Model.tickNumber++;

                /* All processes did not end */
                if (Model.NCP < Model.maxNCP)
                {
                    var ev = new SJF();
                    Model.PlanEvent(ev, 1.0);
                    Model.Tracer.AnyTrace("");
                }
                /* End of program */
                else
                {
                    PrintShortTrace();
                    Model.isFinish = true;
                }
            }

            void PrintShortTrace()
            {
                StreamWriter sw;
                if (Environment.OSVersion.Platform.ToString() == "Win32NT")
                {
                    sw = new StreamWriter(@"D:\Langs\C#\SPOlab1\trace2.txt");

                }
                else
                {
                    sw = new StreamWriter(@"/Users/andreymakarov/Downloads/SPOlab1/Example-SIM/bin/Debug/input.txt");
                }

                sw.WriteLine("FIFO");
                sw.WriteLine("");
                sw.Flush();

                for (int i = 0; i < Model.fifoTrace.Count; i++)
                {
                    sw.WriteLine(Model.fifoTrace[i]);
                    sw.Flush();
                }

                sw.WriteLine("");
                sw.WriteLine("SJF");
                sw.WriteLine("");
                sw.Flush();

                for (int i = 0; i < Model.sjfTrace.Count; i++)
                {
                    sw.WriteLine(Model.sjfTrace[i]);
                    sw.Flush();
                }

                sw.WriteLine("");
                sw.WriteLine("Статистика");
                sw.WriteLine("");
                sw.Flush();

                sw.WriteLine("Время ожидания FIFO: " + (double)Model.fifoWaitTime / Model.maxNCP);
                sw.Flush();
                sw.WriteLine("Время выполнения FIFO: " + (double)Model.fifoExecTime / Model.maxNCP);
                sw.Flush();
                sw.Close();
            }
        }
    }
}
