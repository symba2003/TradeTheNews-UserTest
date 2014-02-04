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

            Console.ReadKey();
            
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