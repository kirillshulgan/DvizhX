namespace DvizhX.Domain.Enums
{
    public enum EventStatus
    {
        /// <summary> Событие создается, но еще не опубликовано для участников. </summary>
        Draft = 0,

        /// <summary> Активное событие. Участники могут вступать и работать с задачами. </summary>
        Active = 1,

        /// <summary> Событие завершено. Задачи заморожены, открыта загрузка фото/видео. </summary>
        Completed = 2,

        /// <summary> Событие отменено создателем. </summary>
        Cancelled = 3,

        /// <summary> Событие перенесено в архив (read-only). </summary>
        Archived = 4
    }
}
