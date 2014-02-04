// Проект: Eshva.Threading
// Имя файла: Task.cs
// GUID файла: 292467E7-4816-4407-BB9B-3309D13C8614
// Автор: Mike Eshva (mike@eshva.ru)
// Дата создания: 04.06.2012

using System;
using System.Threading;


namespace Eshva.Threading.Framework
{
    /// <summary>
    /// Задача для выполнения в <see cref="FixedThreadPool"/>.
    /// </summary>
    public class Task
    {
        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр задачи для выполнения в <see cref="FixedThreadPool1"/>
        /// делегатом тела задачи.
        /// </summary>
        /// <param name="aTaskBody">
        /// Делегат тела задачи.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Делегат тела задачи не задан.
        /// </exception>
        public Task(Action aTaskBody)
        {
            if (aTaskBody == null)
            {
                throw new ArgumentNullException("aTaskBody", "Делегат тела задачи не задан.");
            }

            TaskBody = aTaskBody;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Начинает выполнение задачи.
        /// </summary>
        public void Execute()
        {
            TaskBody();
        }

        #endregion

        #region Private properties

        /// <summary>
        /// Получает/устанавливает делегат тела задачи.
        /// </summary>
        private Action TaskBody { get; set; }

        #endregion
    }

    /// <summary>
    /// Задача для выполнения в <see cref="FixedThreadPool1"/>.
    /// </summary>
    public class Task_INCORRECT
    {
        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр задачи для выполнения в <see cref="FixedThreadPool1"/>
        /// делегатом тела задачи.
        /// </summary>
        /// <param name="aTaskBody">
        /// Делегат тела задачи.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Делегат тела задачи не задан.
        /// </exception>
        public Task_INCORRECT(Action aTaskBody)
        {
            if (aTaskBody == null)
            {
                throw new ArgumentNullException("aTaskBody", "Делегат тела задачи не задан.");
            }

            TaskBody = aTaskBody;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Получает/устанавливает делегат тела задачи.
        /// </summary>
        public Action TaskBody { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Событие, сообщающее о завершении выполнения задачи.
        /// </summary>
        public event EventHandler Finished;

        #endregion

        #region Public methods

        /// <summary>
        /// Начинает выполнение задачи.
        /// </summary>
        public void Execute()
        {
            Thread lTaskThread =
                new Thread(
                    () =>
                    {
                        // Выполнить задачу.
                        TaskBody();
                        // Уведомить об её окончании.
                        EventHandler lFinished = Finished;
                        if (lFinished != null)
                        {
                            lFinished(this, EventArgs.Empty);
                        }
                    }) { Name = "Task thread." };
            lTaskThread.Start();
        }

        #endregion
    }
}