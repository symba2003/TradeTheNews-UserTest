﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Eshva.Threading.Framework
{
    /// <summary>
    /// Пул потоков, выполняющий одновременно не более определённого количество задач с учётом их
    /// приоритетов.
    /// </summary>
    /// <remarks>
    /// Тестовый проект для компании Связной.
    /// </remarks>
    public sealed class FixedThreadPool_INCORRECT
    {
        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр пул потоков максимальным количеством одновременно
        /// выполняемых задач.
        /// </summary>
        /// <param name="aConcurrentTaskNumber">
        /// Максимальное количество одновременно выполняемых задач.
        /// </param>
        /// <param name="aLog">
        /// Сервис логгирования.
        /// </param>
        /// <param name="aPriorityLog">
        /// Лог для вывода приоритета отобранной для выполнения задачи.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Неправильно заданное максимальное количество одновременно выполняемых задач.
        /// </exception>
        public FixedThreadPool_INCORRECT
            (int aConcurrentTaskNumber, ILog aLog = null, ILog aPriorityLog = null)
        {
            if (aConcurrentTaskNumber <= 1)
            {
                throw new ArgumentOutOfRangeException(
                    "aConcurrentTaskNumber",
                    "Количество одновременно выполняемых задач должно быть больше единицы.");
            }

            Log = aLog;
            PriorityLog = aPriorityLog;

            mConcurrentTaskNumber = aConcurrentTaskNumber;

            LogMessage("Создан поток планировщика.");
            Thread lTaskSchedulerThread =
                new Thread(TaskSchedulerLogic) { Name = "Поток планировщика." };
            lTaskSchedulerThread.Start();
            LogMessage("Поток планировщика запущен.");
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Ставит задачу <paramref name="aTask"/> в очередь на выполнение с приоритетом 
        /// <paramref name="aTaskPriority"/>.
        /// </summary>
        /// <param name="aTask">
        /// Задача для постановки в очередь на выполнения..
        /// </param>
        /// <param name="aTaskPriority">
        /// Приоритет задачи.
        /// </param>
        /// <returns>
        /// <see langword="true"/> - задача поставлена в очередь на выполнение. 
        /// <see langword="false"/> - задача не была поставлена в очередь на выполнение, так как
        /// работа пула потока была остановлена.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Задача для постановки в очередь на выполнения не задана.
        /// </exception>
        public bool Execute(Task_INCORRECT aTask, TaskPriority aTaskPriority)
        {
            if (aTask == null)
            {
                throw new ArgumentNullException(
                    "aTask", "Задача для постановки в очередь на выполнения не задана.");
            }

            LogMessage("Получена новая задача для выполнения.");
            lock (mIsStoppedLock)
            {
                if (IsStopped)
                {
                    // Запрошена остановка.
                    LogPriority(aTaskPriority, ConsoleColor.DarkGray);
                    // Отклонять новые задачи.
                    return false;
                }
            }

            // Добавить задачу в очередь.
            EnqueueTask(aTask, aTaskPriority);
            LogMessage("Задача добавлена в очередь задач.", ConsoleColor.DarkYellow);

            return true;
        }

        /// <summary>
        /// Останавливает добавлении задач в очередь пула потоков, очередь не очищается. Возвращает
        /// выполнение только после окончания всех имеющихся задач в очереди.
        /// </summary>
        /// <remarks>
        /// После вызова этого метода дальнейшее добавление задач в очередь на выполнение не
        /// возможно и метод <see cref="Execute"/> будет возвращать <see langword="false"/>.
        /// Имеющиеся, на момент выполнения этого метода, задачи в очереди будут выполнены.
        /// </remarks>
        public void Stop()
        {
            // Выставить признак окончания работы пула.
            lock (mIsStoppedLock)
            {
                IsStopped = true;
                LogMessage("Запрошена остановка пула.");
            }

            // Дождаться окончания выполнения всех задач, оставшихся в очереди.
            LogMessage(
                "Начато ожидание завершения выполнения всех оставшихся задач в очереди.",
                ConsoleColor.DarkRed);
            lock (mTaskSchedulerLock)
            {
                // Сигнализировать об изменении в условии блокировки по mTaskSchedulerLock.
                Monitor.Pulse(mTaskSchedulerLock);
            }

            mPoolStoppedGate.WaitOne();

            LogMessage("Дождались окончания выполнения всех задач в очереди.", ConsoleColor.DarkRed);
        }

        #endregion

        #region Private properties

        /// <summary>
        /// Получает/устанавливает признак того, остановлена ли работа пула.
        /// </summary>
        /// <value>
        /// <see langword="true"/> - работа пула остановлена, дальнейшее добавление задач в очередь
        /// не возможно. <see langword="false"/> - работа пула продолжается.
        /// </value>
        /// <remarks>
        /// Объект синхронизации доступа <see cref="mIsStoppedLock"/>.
        /// </remarks>
        private bool IsStopped { get; set; }

        /// <summary>
        /// Получает/устанавливает сервис логгирования приоритета задачи.
        /// </summary>
        private ILog PriorityLog { get; set; }

        /// <summary>
        /// Получает/устанавливает сервис логгирования сообщений.
        /// </summary>
        private ILog Log { get; set; }

        #endregion

        #region Private methods

        /// <summary>
        /// Логика работы планировщика выполнения задач. Выполняется в отдельном потоке.
        /// </summary>
        private void TaskSchedulerLogic()
        {
            lock (mTaskSchedulerLock)
            {
                while (true)
                {
                    // Отпускаем монитор и ждём сигнала, поданного через mTaskSchedulerLock.
                    Monitor.Wait(mTaskSchedulerLock);
                    lock (mQueuedTasksLock)
                    {
                        lock (mIsStoppedLock)
                        {
                            lock (mRunningTasksLock)
                            {
                                if (IsStopped &&
                                    !mRunningTasks.Any() &&
                                    !mQueuedTasks.Any())
                                {
                                    LogMessage(
                                        "Запрошена остановка пула и больше нет задач в очереди на выполнение.");
                                    LogMessage("Планировщик - Выход из потока планировщика.");
                                    // Сигнализировать об окончании выполнения последней задачи.
                                    mPoolStoppedGate.Set();
                                    return;
                                }
                            }
                        }
                    }

                    lock (mQueuedTasksLock)
                    {
                        if (!mQueuedTasks.Any())
                        {
                            // Очередь задач пуста.
                            // Продолжить ожидание.
                            continue;
                        }
                    }

                    lock (mRunningTasksLock)
                    {
                        if (mRunningTasks.Count >= mConcurrentTaskNumber)
                        {
                            // Список выполняемых задач полон.
                            // Продолжить ожидание.
                            continue;
                        }
                    }

                    LogMessage(
                        "Дождались появления задачи в очереди задач.",
                        ConsoleColor.DarkRed);
                    // Дождаться и получить следующую задачу для выполнения.
                    TaskListEntry lTask = DequeueTask();
                    LogMessage("Планировщик - Получена новая задача для выполнения.");
                    // Подписаться на событие завершения выполнения задачи.
                    lTask.Task.Finished += OnTaskFinished;
                    // Добавить задачу в список выполняемых задач.
                    lock (mRunningTasksLock)
                    {
                        mRunningTasks.Add(lTask);
                    }
                    if (lTask.TaskPriority ==
                        TaskPriority.High)
                    {
                        // Запущена задача с высоким приоритетом.
                        // Увеличить значение счётчика запущенных задач с высоким приоритетом на единицу.
                        Interlocked.Increment(ref mQueuedHighPriorityTaskCounter);
                    }
                    else if (lTask.TaskPriority ==
                             TaskPriority.Normal)
                    {
                        // Запущена задача с обычным приоритетом.
                        // Уменьшить значение счётчика запущенных задач с высоким приоритетом на HighPriorityTaskFactor.
                        Interlocked.Add(
                            ref mQueuedHighPriorityTaskCounter, -HighPriorityTaskFactor);
                    }

                    // Запустить задачу на выполнение.
                    lTask.Task.Execute();
                    LogMessage(
                        string.Format(
                            "Планировщик - Запущена задача с приоритетом {0}.",
                            lTask.TaskPriority),
                        ConsoleColor.DarkYellow);

                    lock (mRunningTasksLock)
                    {
                        LogMessage(
                            string.Format(
                                "В списке выполняющихся задач {0} задач.",
                                mRunningTasks.Count));
                    }
                }
            }
        }

        /// <summary>
        /// Обрабатывает событие завершения задачи.
        /// </summary>
        /// <param name="aSender">
        /// Задача-источник события.
        /// </param>
        /// <param name="aEventArgs">
        /// Параметры события.
        /// </param>
        private void OnTaskFinished(object aSender, EventArgs aEventArgs)
        {
            Task_INCORRECT lSender = aSender as Task_INCORRECT;
            Debug.Assert(
                lSender != null, "В параметре aSender задача должна передавать ссылку на себя.");
            // Отписаться от события завершения, чтобы задача могла быть убрана сборщиком мусора.
            lSender.Finished -= OnTaskFinished;
            // Удалить задачу из списка выполняющихся.
            lock (mRunningTasksLock)
            {
                // Найти и удалить закончившуюся задачу из списка выполняющихся задач.
                TaskListEntry lEntry = mRunningTasks.First(aEntry => aEntry.Task == lSender);
                mRunningTasks.Remove(lEntry);
                LogMessage(
                    string.Format("Задача с приоритетом {0} завершена.", lEntry.TaskPriority),
                    ConsoleColor.Red);
            }

            lock (mTaskSchedulerLock)
            {
                // Сигнализировать об изменении в условии блокировки по mTaskSchedulerLock.
                Monitor.Pulse(mTaskSchedulerLock);
            }
        }

        /// <summary>
        /// Добавляет задачу <paramref name="aTask"/> в очередь задач на выполнение.
        /// </summary>
        /// <param name="aTask">
        /// Задача, добавляемая в очередь задач на выполнение.
        /// </param>
        /// <param name="aTaskPriority">
        /// Приоритет задачи.
        /// </param>
        private void EnqueueTask(Task_INCORRECT aTask, TaskPriority aTaskPriority)
        {
            TaskListEntry lEntry = new TaskListEntry(aTask, aTaskPriority);
            LogPriority(aTaskPriority, ConsoleColor.Green);
            lock (mQueuedTasksLock)
            {
                // Добавить задачу в очередь задач на выполнение.
                mQueuedTasks.Add(lEntry);
                LogMessage(
                    string.Format(
                        "В очередь добавлена задача с приоритетом {0}", lEntry.TaskPriority),
                    ConsoleColor.Green);
                // Поднять барьер доступа к очереди задач.
                LogMessage("Поднять барьер доступа к очереди задач.", ConsoleColor.DarkRed);
            }
            lock (mTaskSchedulerLock)
            {
                // Сигнализировать об изменении в условии блокировки по mTaskSchedulerLock.
                Monitor.Pulse(mTaskSchedulerLock);
            }
        }

        /// <summary>
        /// Ожидает появления в очереди задач хотя бы одной задачи. Дождавшись изымает следующую
        /// задачу из очереди, учитывая правила приоритетов.
        /// </summary>
        /// <returns>
        /// Задача, изъятая из очереди задач на выполнение.
        /// </returns>
        private TaskListEntry DequeueTask()
        {
            TaskListEntry lNextTask;
            lock (mQueuedTasksLock)
            {
                lNextTask = FindNextTaskUsingPriorityRules();
                LogPriority(lNextTask.TaskPriority, ConsoleColor.Red);
                LogMessage(
                    string.Format(
                        "Получена задача из очереди задач с приоритетом {0}.",
                        lNextTask.TaskPriority),
                    ConsoleColor.DarkRed);
                mQueuedTasks.Remove(lNextTask);
            }
            lock (mTaskSchedulerLock)
            {
                // Сигнализировать об изменении в условии блокировки по mTaskSchedulerLock.
                Monitor.Pulse(mTaskSchedulerLock);
            }

            return lNextTask;
        }

        /// <summary>
        /// Находит следующую задачу с учётом правил приоритезации.
        /// </summary>
        /// <returns>
        /// Найденная следующая задача для выполнения.
        /// </returns>
        /// <remarks>
        /// ЗАМЕЧАНИЯ ПО ПРАВИЛАМ ПРИОРИТЕЗАЦИИ: Полученные мной правила выбора следующей задачи
        /// определены в задании не полностью. В частности, не было сказано что делать, если в
        /// очереди задач есть только задачи с обычным приоритетом. Также не определено, что нужно
        /// делать, если было выполнено больше, чем три задачи с высоким приоритетом. Поэтому я
        /// додумал эти правила, исходя из предположения, что лучше пусть выполняется хоть что-то,
        /// чем ждать появления задач с каким-то определённым приоритетом.
        /// </remarks>
        private TaskListEntry FindNextTaskUsingPriorityRules()
        {
            TaskListEntry lNextTask;
            lock (mQueuedTasksLock)
            {
                Debug.Assert(
                    mQueuedTasks.Count > 0,
                    "Метод FindNextTaskUsingPriorityRules не должен вызываться, если очередь задач пуста.");
                // По умолчанию будет выполняться задача с высоким приоритетом.
                TaskPriority lNextTaskPriority = TaskPriority.High;
                // Проверить возможность выполнения задачи с более низкими приоритетами.
                if (mQueuedTasks.All(aEntry => aEntry.TaskPriority == TaskPriority.Low))
                {
                    // В очереди задач все задачи с низким приоритетом.
                    // Следующая задача будет с низким приоритетом.
                    lNextTaskPriority = TaskPriority.Low;
                }
                else
                {
                    // Условие для задач с низким приоритетом не выполняется.
                    if (mQueuedTasks.Any(
                        aEntry => aEntry.TaskPriority == TaskPriority.Normal) &&
                        (mQueuedTasks.All(
                            aEntry => aEntry.TaskPriority != TaskPriority.High) ||
                         Interlocked.CompareExchange(ref mQueuedHighPriorityTaskCounter, 0, 0) >= //COMMENT: судя по описанию метода CompareExchange - совпадение 2 и 3 аргументов - не имеет смысла (если mQueuedHighPriorityTaskCounter = 3аргумент, то mQueuedHighPriorityTaskCounter присваивается значение второго аргумента
                         HighPriorityTaskFactor))
                    {
                        // В списке задач на выполнение есть задачи с обычным приоритетом и
                        // выполнено достаточное количество задач с высоким приоритетом или
                        // в очереди задач все задачи имеют приоритет ниже высокого.
                        // Следующая задача будет с обычным приоритетом.
                        lNextTaskPriority = TaskPriority.Normal;
                    }
                }

                lNextTask = mQueuedTasks.First(
                    aEntry => aEntry.TaskPriority == lNextTaskPriority);
            }

            return lNextTask;
        }

        /// <summary>
        /// Выводит сообщение в лог.
        /// </summary>
        /// <param name="aMessage">
        /// Сообщение.
        /// </param>
        /// <param name="aColor">
        /// Цвет сообщения.
        /// </param>
        private void LogMessage(string aMessage, ConsoleColor aColor = ConsoleColor.Yellow)
        {
            if (Log == null)
            {
                return;
            }

            Log.WriteMessage(aMessage, aColor);
        }

        /// <summary>
        /// Выводит приоритет задачи в лог.
        /// </summary>
        /// <param name="aTaskPriority">
        /// Приоритет задачи.
        /// </param>
        /// <param name="aColor">
        /// Цвет сообщения.
        /// </param>
        private void LogPriority(TaskPriority aTaskPriority, ConsoleColor aColor)
        {
            if (PriorityLog == null)
            {
                return;
            }

            string lPriority = aTaskPriority == TaskPriority.High
                                   ? "H"
                                   : aTaskPriority == TaskPriority.Normal ? "N" : "L";
            PriorityLog.WriteMessage(lPriority, aColor);
        }

        #endregion

        #region Private data

        /// <summary>
        /// Количество задач с высоким приоритетом, которое должно быть поставлено в очередь до
        /// того, как можно будет поставить задачу с обычным приоритетом, если в очереди имеются
        /// задачи с высоким приоритетом.
        /// </summary>
        private const int HighPriorityTaskFactor = 3;

        /// <summary>
        /// Максимальное количество одновременно выполняемых задач. Установка значения возможна
        /// только в конструкторе.
        /// </summary>
        private readonly int mConcurrentTaskNumber;

        /// <summary>
        /// Объект синхронизации доступа к свойству <see cref="IsStopped"/>.
        /// </summary>
        private readonly object mIsStoppedLock = new object();

        /// <summary>
        /// Барьер остановки работы пула. Поднимается, когда запрошена остановка и все задачи
        /// завершили своё выполнение.
        /// </summary>
        private readonly ManualResetEvent mPoolStoppedGate = new ManualResetEvent(false);

        /// <summary>
        /// Список задач, поставленных в очередь на выполнение.
        /// </summary>
        /// <remarks>
        /// Объект синхронизации доступа <see cref="mQueuedTasksLock"/>.
        /// </remarks>
        private readonly IList<TaskListEntry> mQueuedTasks = new List<TaskListEntry>();

        /// <summary>
        /// Объект синхронизации доступа к списку задач <see cref="mQueuedTasks"/>.
        /// </summary>
        private readonly object mQueuedTasksLock = new object();

        /// <summary>
        /// Список выполняющихся в настоящий момент задач.
        /// </summary>
        /// <remarks>
        /// Объект синхронизации доступа <see cref="mRunningTasksLock"/>.
        /// </remarks>
        private readonly IList<TaskListEntry> mRunningTasks = new List<TaskListEntry>();

        /// <summary>
        /// Объект синхронизации доступа к <see cref="mRunningTasks"/>.
        /// </summary>
        private readonly object mRunningTasksLock = new object();

        /// <summary>
        /// Объект синхронизации, используемый для блокировки/запуска потока планировщика.
        /// </summary>
        private readonly object mTaskSchedulerLock = new object();

        /// <summary>
        /// Счётчик задач с высоким приоритетом, запущенных на выполнение. Каждая запущенная задача
        /// с высоким приоритетом увеличивает это значение на единицу, каждая запущенная задача с
        /// обычным приоритетом уменьшает это значение на <see cref="HighPriorityTaskFactor"/>.
        /// </summary>
        /// <remarks>
        /// Синхронизация доступа должна выполняться по средствам использования методов класса 
        /// <see cref="Interlocked"/>.
        /// </remarks>
        private int mQueuedHighPriorityTaskCounter;

        #endregion

        #region Nested type: TaskListEntry

        /// <summary>
        /// Элемент списка задач. 
        /// </summary>
        /// <remarks>
        /// Объекты после создания не изменяемы.
        /// </remarks>
        private struct TaskListEntry
        {
            #region Constructors

            /// <summary>
            /// Инициализирует новый экземпляр элемента списка задач задачей и её приоритетом.
            /// </summary>
            /// <param name="aTask">
            /// Задача.
            /// </param>
            /// <param name="aTaskPriority">
            /// Приоритет задачи.
            /// </param>
            public TaskListEntry(Task_INCORRECT aTask, TaskPriority aTaskPriority)
            {
                mTask = aTask;
                mTaskPriority = aTaskPriority;
            }

            #endregion

            #region Public properties

            /// <summary>
            /// Задача.
            /// </summary>
            public Task_INCORRECT Task
            {
                get { return mTask; }
            }

            /// <summary>
            /// Приоритет задачи.
            /// </summary>
            public TaskPriority TaskPriority
            {
                get { return mTaskPriority; }
            }

            #endregion

            #region Private data

            private readonly Task_INCORRECT mTask;
            private readonly TaskPriority mTaskPriority;

            #endregion
        }

        #endregion
    }

}
