using Microsoft.EntityFrameworkCore;
using Victorina.Domain.Entities;

namespace Victorina.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedTestDataAsync(VictorinaDbContext context)
    {
        // Проверяем, есть ли уже данные
        if (await context.Categories.AnyAsync())
        {
            return; // Данные уже есть
        }

        // Категории
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "География", Emoji = "🌍", Description = "Вопросы о странах, городах и природе" },
            new() { Id = 2, Name = "История", Emoji = "📜", Description = "Исторические события и личности" },
            new() { Id = 3, Name = "Наука", Emoji = "🔬", Description = "Физика, химия, биология" },
            new() { Id = 4, Name = "Спорт", Emoji = "⚽", Description = "Спортивные события и рекорды" },
            new() { Id = 5, Name = "Кино", Emoji = "🎬", Description = "Фильмы, актёры, режиссёры" },
            new() { Id = 6, Name = "Музыка", Emoji = "🎵", Description = "Музыканты и песни" },
            new() { Id = 7, Name = "IT", Emoji = "💻", Description = "Компьютеры и технологии" },
            new() { Id = 8, Name = "Литература", Emoji = "📚", Description = "Книги и писатели" }
        };

        // Вопросы
        var questions = new List<Question>
        {
            // География (CategoryId = 1)
            new() { CategoryId = 1, Text = "Столица Франции?", CorrectAnswer = "Париж", WrongAnswer1 = "Лондон", WrongAnswer2 = "Берлин", WrongAnswer3 = "Мадрид" },
            new() { CategoryId = 1, Text = "Какая самая длинная река в мире?", CorrectAnswer = "Нил", WrongAnswer1 = "Амазонка", WrongAnswer2 = "Миссисипи", WrongAnswer3 = "Янцзы" },
            new() { CategoryId = 1, Text = "В какой стране находится Мачу-Пикчу?", CorrectAnswer = "Перу", WrongAnswer1 = "Мексика", WrongAnswer2 = "Бразилия", WrongAnswer3 = "Чили" },
            new() { CategoryId = 1, Text = "Столица Австралии?", CorrectAnswer = "Канберра", WrongAnswer1 = "Сидней", WrongAnswer2 = "Мельбурн", WrongAnswer3 = "Брисбен" },
            new() { CategoryId = 1, Text = "Какой океан самый большой?", CorrectAnswer = "Тихий", WrongAnswer1 = "Атлантический", WrongAnswer2 = "Индийский", WrongAnswer3 = "Северный Ледовитый" },
            new() { CategoryId = 1, Text = "Столица Японии?", CorrectAnswer = "Токио", WrongAnswer1 = "Киото", WrongAnswer2 = "Осака", WrongAnswer3 = "Хиросима" },
            new() { CategoryId = 1, Text = "Какая страна имеет форму сапога?", CorrectAnswer = "Италия", WrongAnswer1 = "Греция", WrongAnswer2 = "Испания", WrongAnswer3 = "Португалия" },
            new() { CategoryId = 1, Text = "Где находится пустыня Сахара?", CorrectAnswer = "Африка", WrongAnswer1 = "Азия", WrongAnswer2 = "Австралия", WrongAnswer3 = "Южная Америка" },
            new() { CategoryId = 1, Text = "Столица Канады?", CorrectAnswer = "Оттава", WrongAnswer1 = "Торонто", WrongAnswer2 = "Монреаль", WrongAnswer3 = "Ванкувер" },
            new() { CategoryId = 1, Text = "Какая самая высокая гора в мире?", CorrectAnswer = "Эверест", WrongAnswer1 = "К2", WrongAnswer2 = "Килиманджаро", WrongAnswer3 = "Монблан" },

            // История (CategoryId = 2)
            new() { CategoryId = 2, Text = "В каком году началась Вторая мировая война?", CorrectAnswer = "1939", WrongAnswer1 = "1941", WrongAnswer2 = "1938", WrongAnswer3 = "1940" },
            new() { CategoryId = 2, Text = "Кто был первым президентом США?", CorrectAnswer = "Джордж Вашингтон", WrongAnswer1 = "Авраам Линкольн", WrongAnswer2 = "Томас Джефферсон", WrongAnswer3 = "Бенджамин Франклин" },
            new() { CategoryId = 2, Text = "В каком году пал Берлинская стена?", CorrectAnswer = "1989", WrongAnswer1 = "1991", WrongAnswer2 = "1987", WrongAnswer3 = "1990" },
            new() { CategoryId = 2, Text = "Кто открыл Америку?", CorrectAnswer = "Христофор Колумб", WrongAnswer1 = "Америго Веспуччи", WrongAnswer2 = "Васко да Гама", WrongAnswer3 = "Фернан Магеллан" },
            new() { CategoryId = 2, Text = "В каком году человек впервые полетел в космос?", CorrectAnswer = "1961", WrongAnswer1 = "1957", WrongAnswer2 = "1963", WrongAnswer3 = "1969" },
            new() { CategoryId = 2, Text = "Кто написал 'Войну и мир'?", CorrectAnswer = "Лев Толстой", WrongAnswer1 = "Фёдор Достоевский", WrongAnswer2 = "Антон Чехов", WrongAnswer3 = "Иван Тургенев" },
            new() { CategoryId = 2, Text = "Когда произошла Французская революция?", CorrectAnswer = "1789", WrongAnswer1 = "1776", WrongAnswer2 = "1799", WrongAnswer3 = "1812" },
            new() { CategoryId = 2, Text = "Кто был последним русским царём?", CorrectAnswer = "Николай II", WrongAnswer1 = "Александр III", WrongAnswer2 = "Николай I", WrongAnswer3 = "Александр II" },
            new() { CategoryId = 2, Text = "В каком году закончилась Первая мировая война?", CorrectAnswer = "1918", WrongAnswer1 = "1917", WrongAnswer2 = "1919", WrongAnswer3 = "1916" },
            new() { CategoryId = 2, Text = "Кто построил пирамиды в Египте?", CorrectAnswer = "Древние египтяне", WrongAnswer1 = "Римляне", WrongAnswer2 = "Греки", WrongAnswer3 = "Персы" },

            // Наука (CategoryId = 3)
            new() { CategoryId = 3, Text = "Какой химический символ у золота?", CorrectAnswer = "Au", WrongAnswer1 = "Ag", WrongAnswer2 = "Fe", WrongAnswer3 = "Go" },
            new() { CategoryId = 3, Text = "Сколько планет в Солнечной системе?", CorrectAnswer = "8", WrongAnswer1 = "9", WrongAnswer2 = "7", WrongAnswer3 = "10" },
            new() { CategoryId = 3, Text = "Какая самая маленькая частица атома?", CorrectAnswer = "Кварк", WrongAnswer1 = "Электрон", WrongAnswer2 = "Протон", WrongAnswer3 = "Нейтрон" },
            new() { CategoryId = 3, Text = "Кто разработал теорию относительности?", CorrectAnswer = "Альберт Эйнштейн", WrongAnswer1 = "Исаак Ньютон", WrongAnswer2 = "Никола Тесла", WrongAnswer3 = "Стивен Хокинг" },
            new() { CategoryId = 3, Text = "Какой газ мы вдыхаем больше всего?", CorrectAnswer = "Азот", WrongAnswer1 = "Кислород", WrongAnswer2 = "Углекислый газ", WrongAnswer3 = "Водород" },
            new() { CategoryId = 3, Text = "Сколько костей в теле взрослого человека?", CorrectAnswer = "206", WrongAnswer1 = "208", WrongAnswer2 = "200", WrongAnswer3 = "212" },
            new() { CategoryId = 3, Text = "Какая планета ближе всего к Солнцу?", CorrectAnswer = "Меркурий", WrongAnswer1 = "Венера", WrongAnswer2 = "Марс", WrongAnswer3 = "Земля" },
            new() { CategoryId = 3, Text = "Что измеряется в джоулях?", CorrectAnswer = "Энергия", WrongAnswer1 = "Сила", WrongAnswer2 = "Мощность", WrongAnswer3 = "Давление" },
            new() { CategoryId = 3, Text = "Какой элемент обозначается H?", CorrectAnswer = "Водород", WrongAnswer1 = "Гелий", WrongAnswer2 = "Кислород", WrongAnswer3 = "Азот" },
            new() { CategoryId = 3, Text = "Скорость света приблизительно равна?", CorrectAnswer = "300 000 км/с", WrongAnswer1 = "150 000 км/с", WrongAnswer2 = "500 000 км/с", WrongAnswer3 = "1 000 000 км/с" },

            // Спорт (CategoryId = 4)
            new() { CategoryId = 4, Text = "Сколько игроков в футбольной команде на поле?", CorrectAnswer = "11", WrongAnswer1 = "10", WrongAnswer2 = "12", WrongAnswer3 = "9" },
            new() { CategoryId = 4, Text = "В какой стране изобрели баскетбол?", CorrectAnswer = "США", WrongAnswer1 = "Канада", WrongAnswer2 = "Англия", WrongAnswer3 = "Испания" },
            new() { CategoryId = 4, Text = "Кто выиграл больше всего Чемпионатов мира по футболу?", CorrectAnswer = "Бразилия", WrongAnswer1 = "Германия", WrongAnswer2 = "Италия", WrongAnswer3 = "Аргентина" },
            new() { CategoryId = 4, Text = "Сколько сетов нужно выиграть в теннисе (мужчины, Большой шлем)?", CorrectAnswer = "3", WrongAnswer1 = "2", WrongAnswer2 = "4", WrongAnswer3 = "5" },
            new() { CategoryId = 4, Text = "Какой вид спорта называют 'королём спорта'?", CorrectAnswer = "Лёгкая атлетика", WrongAnswer1 = "Футбол", WrongAnswer2 = "Бокс", WrongAnswer3 = "Плавание" },
            new() { CategoryId = 4, Text = "Где прошли первые современные Олимпийские игры?", CorrectAnswer = "Афины", WrongAnswer1 = "Париж", WrongAnswer2 = "Лондон", WrongAnswer3 = "Рим" },
            new() { CategoryId = 4, Text = "Сколько очков за тачдаун в американском футболе?", CorrectAnswer = "6", WrongAnswer1 = "7", WrongAnswer2 = "3", WrongAnswer3 = "5" },
            new() { CategoryId = 4, Text = "Какая страна принимала ЧМ по футболу 2018?", CorrectAnswer = "Россия", WrongAnswer1 = "Бразилия", WrongAnswer2 = "Германия", WrongAnswer3 = "Катар" },
            new() { CategoryId = 4, Text = "Сколько периодов в хоккейном матче?", CorrectAnswer = "3", WrongAnswer1 = "2", WrongAnswer2 = "4", WrongAnswer3 = "5" },
            new() { CategoryId = 4, Text = "Кто является рекордсменом по голам в истории футбола?", CorrectAnswer = "Криштиану Роналду", WrongAnswer1 = "Лионель Месси", WrongAnswer2 = "Пеле", WrongAnswer3 = "Герд Мюллер" },

            // Кино (CategoryId = 5)
            new() { CategoryId = 5, Text = "Кто режиссёр фильма 'Титаник'?", CorrectAnswer = "Джеймс Кэмерон", WrongAnswer1 = "Стивен Спилберг", WrongAnswer2 = "Кристофер Нолан", WrongAnswer3 = "Мартин Скорсезе" },
            new() { CategoryId = 5, Text = "Какой фильм получил Оскар за лучший фильм в 2020?", CorrectAnswer = "Паразиты", WrongAnswer1 = "1917", WrongAnswer2 = "Джокер", WrongAnswer3 = "Однажды в Голливуде" },
            new() { CategoryId = 5, Text = "Кто играет Железного человека в MCU?", CorrectAnswer = "Роберт Дауни мл.", WrongAnswer1 = "Крис Эванс", WrongAnswer2 = "Крис Хемсворт", WrongAnswer3 = "Марк Руффало" },
            new() { CategoryId = 5, Text = "В каком году вышел первый 'Звёздные войны'?", CorrectAnswer = "1977", WrongAnswer1 = "1980", WrongAnswer2 = "1975", WrongAnswer3 = "1983" },
            new() { CategoryId = 5, Text = "Кто сыграл Джокера в 'Тёмном рыцаре'?", CorrectAnswer = "Хит Леджер", WrongAnswer1 = "Джаред Лето", WrongAnswer2 = "Хоакин Феникс", WrongAnswer3 = "Джек Николсон" },
            new() { CategoryId = 5, Text = "Какой фильм является самым кассовым в истории?", CorrectAnswer = "Аватар", WrongAnswer1 = "Мстители: Финал", WrongAnswer2 = "Титаник", WrongAnswer3 = "Звёздные войны" },
            new() { CategoryId = 5, Text = "Кто режиссёр трилогии 'Властелин колец'?", CorrectAnswer = "Питер Джексон", WrongAnswer1 = "Ридли Скотт", WrongAnswer2 = "Гильермо дель Торо", WrongAnswer3 = "Дэвид Финчер" },
            new() { CategoryId = 5, Text = "Как зовут главного героя 'Матрицы'?", CorrectAnswer = "Нео", WrongAnswer1 = "Морфеус", WrongAnswer2 = "Тринити", WrongAnswer3 = "Смит" },
            new() { CategoryId = 5, Text = "Кто озвучивает Шрека в оригинале?", CorrectAnswer = "Майк Майерс", WrongAnswer1 = "Эдди Мёрфи", WrongAnswer2 = "Камерон Диаз", WrongAnswer3 = "Антонио Бандерас" },
            new() { CategoryId = 5, Text = "Сколько фильмов о Гарри Поттере?", CorrectAnswer = "8", WrongAnswer1 = "7", WrongAnswer2 = "9", WrongAnswer3 = "6" },

            // Музыка (CategoryId = 6)
            new() { CategoryId = 6, Text = "Кто является 'Королём поп-музыки'?", CorrectAnswer = "Майкл Джексон", WrongAnswer1 = "Элвис Пресли", WrongAnswer2 = "Принс", WrongAnswer3 = "Фредди Меркьюри" },
            new() { CategoryId = 6, Text = "Из какого города группа The Beatles?", CorrectAnswer = "Ливерпуль", WrongAnswer1 = "Лондон", WrongAnswer2 = "Манчестер", WrongAnswer3 = "Бирмингем" },
            new() { CategoryId = 6, Text = "Кто написал 'Лунную сонату'?", CorrectAnswer = "Бетховен", WrongAnswer1 = "Моцарт", WrongAnswer2 = "Бах", WrongAnswer3 = "Шопен" },
            new() { CategoryId = 6, Text = "Какой инструмент у Джими Хендрикса?", CorrectAnswer = "Гитара", WrongAnswer1 = "Барабаны", WrongAnswer2 = "Бас-гитара", WrongAnswer3 = "Клавишные" },
            new() { CategoryId = 6, Text = "Сколько струн у стандартной гитары?", CorrectAnswer = "6", WrongAnswer1 = "4", WrongAnswer2 = "5", WrongAnswer3 = "7" },
            new() { CategoryId = 6, Text = "Кто исполняет песню 'Bohemian Rhapsody'?", CorrectAnswer = "Queen", WrongAnswer1 = "Led Zeppelin", WrongAnswer2 = "Pink Floyd", WrongAnswer3 = "The Rolling Stones" },
            new() { CategoryId = 6, Text = "Какая нота идёт после 'до'?", CorrectAnswer = "Ре", WrongAnswer1 = "Ми", WrongAnswer2 = "Фа", WrongAnswer3 = "Соль" },
            new() { CategoryId = 6, Text = "Кто является солистом группы U2?", CorrectAnswer = "Боно", WrongAnswer1 = "Эдж", WrongAnswer2 = "Адам Клейтон", WrongAnswer3 = "Ларри Маллен" },
            new() { CategoryId = 6, Text = "Сколько симфоний написал Бетховен?", CorrectAnswer = "9", WrongAnswer1 = "5", WrongAnswer2 = "7", WrongAnswer3 = "12" },
            new() { CategoryId = 6, Text = "Кто известен как 'Королева соула'?", CorrectAnswer = "Арета Франклин", WrongAnswer1 = "Уитни Хьюстон", WrongAnswer2 = "Тина Тёрнер", WrongAnswer3 = "Дайана Росс" },

            // IT (CategoryId = 7)
            new() { CategoryId = 7, Text = "Кто основал Microsoft?", CorrectAnswer = "Билл Гейтс", WrongAnswer1 = "Стив Джобс", WrongAnswer2 = "Марк Цукерберг", WrongAnswer3 = "Илон Маск" },
            new() { CategoryId = 7, Text = "Что означает HTML?", CorrectAnswer = "HyperText Markup Language", WrongAnswer1 = "High Tech Modern Language", WrongAnswer2 = "Home Tool Markup Language", WrongAnswer3 = "Hyperlink Text Mode Language" },
            new() { CategoryId = 7, Text = "В каком году был создан первый iPhone?", CorrectAnswer = "2007", WrongAnswer1 = "2005", WrongAnswer2 = "2008", WrongAnswer3 = "2010" },
            new() { CategoryId = 7, Text = "Какой язык программирования создал Гвидо ван Россум?", CorrectAnswer = "Python", WrongAnswer1 = "Java", WrongAnswer2 = "Ruby", WrongAnswer3 = "PHP" },
            new() { CategoryId = 7, Text = "Что такое RAM?", CorrectAnswer = "Оперативная память", WrongAnswer1 = "Жёсткий диск", WrongAnswer2 = "Процессор", WrongAnswer3 = "Видеокарта" },
            new() { CategoryId = 7, Text = "Кто создал Linux?", CorrectAnswer = "Линус Торвальдс", WrongAnswer1 = "Ричард Столлман", WrongAnswer2 = "Деннис Ритчи", WrongAnswer3 = "Кен Томпсон" },
            new() { CategoryId = 7, Text = "Что означает CPU?", CorrectAnswer = "Central Processing Unit", WrongAnswer1 = "Computer Personal Unit", WrongAnswer2 = "Central Program Utility", WrongAnswer3 = "Core Processing Unit" },
            new() { CategoryId = 7, Text = "Какая компания создала Android?", CorrectAnswer = "Google", WrongAnswer1 = "Apple", WrongAnswer2 = "Samsung", WrongAnswer3 = "Microsoft" },
            new() { CategoryId = 7, Text = "Сколько бит в одном байте?", CorrectAnswer = "8", WrongAnswer1 = "4", WrongAnswer2 = "16", WrongAnswer3 = "2" },
            new() { CategoryId = 7, Text = "Что такое SQL?", CorrectAnswer = "Язык запросов к базам данных", WrongAnswer1 = "Язык программирования", WrongAnswer2 = "Операционная система", WrongAnswer3 = "Протокол передачи данных" },

            // Литература (CategoryId = 8)
            new() { CategoryId = 8, Text = "Кто написал 'Гамлета'?", CorrectAnswer = "Уильям Шекспир", WrongAnswer1 = "Чарльз Диккенс", WrongAnswer2 = "Оскар Уайльд", WrongAnswer3 = "Джейн Остин" },
            new() { CategoryId = 8, Text = "Кто автор 'Гарри Поттера'?", CorrectAnswer = "Джоан Роулинг", WrongAnswer1 = "Стивен Кинг", WrongAnswer2 = "Джордж Мартин", WrongAnswer3 = "Толкин" },
            new() { CategoryId = 8, Text = "Какое произведение написал Достоевский?", CorrectAnswer = "Преступление и наказание", WrongAnswer1 = "Война и мир", WrongAnswer2 = "Анна Каренина", WrongAnswer3 = "Мёртвые души" },
            new() { CategoryId = 8, Text = "Кто написал '1984'?", CorrectAnswer = "Джордж Оруэлл", WrongAnswer1 = "Олдос Хаксли", WrongAnswer2 = "Рэй Брэдбери", WrongAnswer3 = "Айзек Азимов" },
            new() { CategoryId = 8, Text = "Автор 'Мастера и Маргариты'?", CorrectAnswer = "Михаил Булгаков", WrongAnswer1 = "Борис Пастернак", WrongAnswer2 = "Максим Горький", WrongAnswer3 = "Иван Бунин" },
            new() { CategoryId = 8, Text = "Кто написал 'Дон Кихота'?", CorrectAnswer = "Мигель де Сервантес", WrongAnswer1 = "Габриэль Маркес", WrongAnswer2 = "Пабло Неруда", WrongAnswer3 = "Хорхе Борхес" },
            new() { CategoryId = 8, Text = "Какой роман написал Хемингуэй?", CorrectAnswer = "Старик и море", WrongAnswer1 = "Моби Дик", WrongAnswer2 = "Великий Гэтсби", WrongAnswer3 = "Над пропастью во ржи" },
            new() { CategoryId = 8, Text = "Кто автор 'Властелина колец'?", CorrectAnswer = "Джон Толкин", WrongAnswer1 = "Клайв Льюис", WrongAnswer2 = "Урсула Ле Гуин", WrongAnswer3 = "Терри Пратчетт" },
            new() { CategoryId = 8, Text = "Кто написал 'Евгения Онегина'?", CorrectAnswer = "Александр Пушкин", WrongAnswer1 = "Михаил Лермонтов", WrongAnswer2 = "Николай Гоголь", WrongAnswer3 = "Фёдор Тютчев" },
            new() { CategoryId = 8, Text = "Автор 'Маленького принца'?", CorrectAnswer = "Антуан де Сент-Экзюпери", WrongAnswer1 = "Жюль Верн", WrongAnswer2 = "Виктор Гюго", WrongAnswer3 = "Александр Дюма" }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        await context.Questions.AddRangeAsync(questions);
        await context.SaveChangesAsync();
    }

    public static async Task<bool> ClearAndReseedAsync(VictorinaDbContext context)
    {
        // Удаляем все вопросы и категории
        context.Questions.RemoveRange(context.Questions);
        context.Categories.RemoveRange(context.Categories);
        await context.SaveChangesAsync();

        // Перезаполняем
        await SeedTestDataAsync(context);
        return true;
    }
}
