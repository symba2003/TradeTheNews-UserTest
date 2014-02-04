// Проект: Eshva.Threading
// Имя файла: TaskPriority.cs
// GUID файла: 7FB6C708-8428-478D-BF16-BECF41084621
// Автор: Mike Eshva (mike@eshva.ru)
// Дата создания: 04.06.2012

namespace Eshva.Threading.Framework
{
    /// <summary>
    /// Приоритет задачи.
    /// </summary>
    public enum TaskPriority
    {
        /// <summary>
        /// Высокий приоритет.
        /// </summary>
        High = 0,

        /// <summary>
        /// Обычный приоритет.
        /// </summary>
        Normal,

        /// <summary>
        /// Низкий приоритет.
        /// </summary>
        Low
    }
}