using System;
using System.Threading;
using Eshva.Threading.Framework;


namespace Eshva.Threading
{
    internal class Program
    {
        #region Private methods

        private static void Main()
        {
            // this is workaround for Windows 7 NF4.5, см. ниже Console.ReadKey()
            Console.WriteLine("app started");
           
            Timer lHighPriorityTimer =
                 new Timer(
                     aState => mThreadPool.Execute(
                         new Task(
                             () =>
                             {
                                 Random lRandom = new Random();
                                 Thread.Sleep(lRandom.Next(50, 500));
                             }),
                         TaskPriority.High),
                     null,
                     0,
                     50);
            // Console.WriteLine("high priority initialized");
            Timer lNormalPriorityTimer =
                new Timer(
                    aState => mThreadPool.Execute(
                        new Task(
                            () =>
                            {
                                Random lRandom = new Random();
                                Thread.Sleep(lRandom.Next(50, 500));
                            }),
                        TaskPriority.Normal),
                    null,
                    0,
                    150);
            //Console.WriteLine("normal priority initialized");
            Timer lLowPriorityTimer =
                new Timer(
                    aState => mThreadPool.Execute(
                        new Task(
                            () =>
                            {
                                Random lRandom = new Random();
                                Thread.Sleep(lRandom.Next(50, 500));
                            }),
                        TaskPriority.Low),
                    null,
                    0,
                    150);
            //.WriteLine("low priority initialized");
            Console.ReadKey();
            // Пока не нажата клавиша - пул не останавливается. Но при этом в консоль ничего не выводится. Непонятно - почему?
            // А как только нажали к-л клавишу - идет вывод информации о потоках, и затем появляется "Запрошена остановка пула."
            // затем продолжаются оставшиеся потоки и выполнение завершается
            // Непонятно: почему сразу в консоль ничего не выводится?

            /*
            Console.WriteLine("app started"); было добавлено, так как без этого приложение "подвисало",
            * и записи в консоли "появлялись" только после нажатия какой-либо клавиши (то есть после того, как отработает Console.ReadKey()
            * 
            * http://stackoverflow.com/questions/15143931/strange-behaviour-of-console-readkey-with-multithreading
            * This is a race condition. Here is what's happening when the first Console.WriteLine is not there:
            * Task is created, but not run
            * Console.ReadKey executes, takes a lock on Console.InternalSyncObject, and blocks waiting for input
            * The Task's Console.WriteLine calls Console.Out, which calls Console.InitializeStdOutError for first-time initialization
            * to set up the console streams Console.InitializeStdOutError attempts to lock on Console.InternalSyncObject, but Console.ReadKey
            * already has it, so it blocks. The user presses a key and Console.ReadKey returns, releasing the lock
            * The call to Console.WriteLine is unblocked and finishes executing
            * The process exits, because there is nothing in Main after the ReadKey call
            * The remaining code in the Task does not get a chance to run
            * The reason it behaves differently when the Console.WriteLine is left in there is because the call to Console.InitializeStdOutError
            * is not happening in parallel with Console.ReadKey.
            * 
            * So the short answer is: yes you are abusing Console. You could either initialize the console yourself (by dereferencing Console.Out),
            * or you would wait on an event after starting the Task, but before ReadKey, and then have the Task signal the event after calling
            * Console.WriteLine the first time.
            * 
            * 
            * 
            * 
            * It is confirmed internal bug in .NET 4.5. It has been reported for example here:
            * https://connect.microsoft.com/VisualStudio/feedback/details/778650/undocumented-locking-behaviour-in-system-console
            * This worked in .NET 3.5 and .NET 4.
            * More info: http://blogs.microsoft.co.il/blogs/dorony/archive/2012/09/12/console-readkey-net-4-5-changes-may-deadlock-your-system.aspx
            * 
            */
            WriteLine("Запрошена остановка пула.");
            lHighPriorityTimer.Dispose();
            lNormalPriorityTimer.Dispose();
            lLowPriorityTimer.Dispose();
            mThreadPool.Stop();
            WriteLine("Выполнение пула остановлено. Нажмите любую кнопку.");
            Console.ReadKey();
        }

        private static void WriteLine(string aMessage= null, ConsoleColor aColor = ConsoleColor.Gray)
        {
            if (aMessage == null)
            {
                Console.ForegroundColor = aColor;
                Console.WriteLine();
            }

            Console.ForegroundColor = aColor;
            Console.WriteLine("{0:HH:mm:ss}: {1}", DateTime.Now, aMessage);
        }

        #endregion

        #region Private data

        /// <summary>
        /// Для тестирования правильности выборки задач по приоритетам.
        /// </summary>
        private static readonly FixedThreadPool mThreadPool =
            new FixedThreadPool(5, null, new PriorityConsoleLog());

        #endregion

/*
        /// <summary>
        /// Для общего тестирования.
        /// </summary>
        private static readonly FixedThreadPool mThreadPool = 
            new FixedThreadPool(5, new ConsoleLog());
*/

        #region Nested type: ConsoleLog

        public class ConsoleLog : ILog
        {
            #region ILog Members

            /// <summary>
            /// Выводит в лог сообщение <paramref name="aMessage"/>.
            /// </summary>
            /// <param name="aMessage">
            /// Сообщение.
            /// </param>
            /// <param name="aColor">
            /// Цвет сообщения в логе.
            /// </param>
            public void WriteMessage(string aMessage, ConsoleColor aColor = ConsoleColor.White)
            {
                Console.ForegroundColor = aColor;
                Console.WriteLine("{0:HH:mm:ss}: {1}", DateTime.Now, aMessage);
            }

            #endregion
        }

        #endregion

        #region Nested type: PriorityConsoleLog

        public class PriorityConsoleLog : ILog
        {
            #region ILog Members

            /// <summary>
            /// Выводит в лог сообщение <paramref name="aMessage"/>.
            /// </summary>
            /// <param name="aMessage">
            /// Сообщение.
            /// </param>
            /// <param name="aColor">
            /// Цвет сообщения в логе.
            /// </param>
            public void WriteMessage(string aMessage, ConsoleColor aColor = ConsoleColor.White)
            {
                Console.ForegroundColor = aColor;
                Console.Write(aMessage);
            }

            #endregion
        }

        #endregion
    }
}