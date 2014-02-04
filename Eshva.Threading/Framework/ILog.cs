// Проект: Eshva.Threading
// Имя файла: ILog.cs
// GUID файла: 4845F74D-7E51-4FBB-B892-9FA2472A3288
// Автор: Mike Eshva (mike@eshva.ru)
// Дата создания: 04.06.2012

using System;


namespace Eshva.Threading.Framework
{
    /// <summary>
    /// Контракт сервиса логгирования.
    /// </summary>
    public interface ILog
    {
        #region Public methods

        /// <summary>
        /// Выводит в лог сообщение <paramref name="aMessage"/>.
        /// </summary>
        /// <param name="aMessage">
        /// Сообщение.
        /// </param>
        /// <param name="aColor">
        /// Цвет сообщения в логе.
        /// </param>
        void WriteMessage(string aMessage, ConsoleColor aColor = ConsoleColor.White);

        #endregion
    }
}