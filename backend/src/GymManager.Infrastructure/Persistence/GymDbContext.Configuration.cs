using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GymManager.Infrastructure.Persistence;

/// <summary>
/// Ручная донастройка модели.
///
/// Файл отдельный от GymDbContext.cs, который генерируется скаффолдингом и
/// перезаписывается при каждом --force. Скаффолдер помечает контекст как
/// partial и оставляет точку расширения OnModelCreatingPartial именно
/// для этого. Namespace обязан совпадать со сгенерированным файлом,
/// иначе получатся два разных класса и ошибка CS0759.
/// </summary>
public partial class GymDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // updated_at заполняется триггером set_updated_at в БД. Из схемы
        // скаффолдер этого вывести не может, поэтому говорим EF явно:
        // значение генерирует хранилище, слать его не нужно. Иначе EF
        // отправил бы своё, триггер его перебил, и объект в памяти
        // разошёлся бы с базой.
        //
        // Цикл вместо явного перечисления сущностей: настройка переживёт
        // добавление новой таблицы с updated_at и триггером.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // У Visit и VTicket такого свойства нет — пропускаем.
            if (entityType.FindProperty("UpdatedAt") is null)
                continue;

            modelBuilder.Entity(entityType.ClrType)
                .Property("UpdatedAt")
                .ValueGeneratedOnAddOrUpdate()
                // Страховка: по умолчанию EF бросает исключение при попытке
                // изменить store-generated свойство отслеживаемой сущности.
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
