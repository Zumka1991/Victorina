using Microsoft.EntityFrameworkCore;
using Victorina.Domain.Entities;

namespace Victorina.Infrastructure.Data;

public static class SeedData
{
    // Языки: ru, hi, pt, fa, de, uz
    private static readonly string[] Languages = { "ru", "hi", "pt", "fa", "de", "uz" };

    public static async Task SeedTestDataAsync(VictorinaDbContext context)
    {
        // Проверяем, есть ли уже данные
        if (await context.Categories.AnyAsync())
        {
            return; // Данные уже есть
        }

        await SeedCategoriesAndQuestionsAsync(context);
    }

    private static async Task SeedCategoriesAndQuestionsAsync(VictorinaDbContext context)
    {
        // TranslationGroupId для связывания категорий на разных языках
        var geoGroupId = Guid.NewGuid();
        var historyGroupId = Guid.NewGuid();
        var scienceGroupId = Guid.NewGuid();

        // Категории на всех языках (без явных Id - PostgreSQL генерирует автоматически)
        var categories = new List<Category>
        {
            // География (Общие)
            new() { Name = "География", Emoji = "🌍", Description = "Вопросы о странах, городах и природе", LanguageCode = "ru", TranslationGroupId = geoGroupId, CategoryGroup = "general" },
            new() { Name = "भूगोल", Emoji = "🌍", Description = "देशों, शहरों और प्रकृति के बारे में प्रश्न", LanguageCode = "hi", TranslationGroupId = geoGroupId, CategoryGroup = "general" },
            new() { Name = "Geografia", Emoji = "🌍", Description = "Perguntas sobre países, cidades e natureza", LanguageCode = "pt", TranslationGroupId = geoGroupId, CategoryGroup = "general" },
            new() { Name = "جغرافیا", Emoji = "🌍", Description = "سوالات درباره کشورها، شهرها و طبیعت", LanguageCode = "fa", TranslationGroupId = geoGroupId, CategoryGroup = "general" },
            new() { Name = "Geographie", Emoji = "🌍", Description = "Fragen über Länder, Städte und Natur", LanguageCode = "de", TranslationGroupId = geoGroupId, CategoryGroup = "general" },
            new() { Name = "Geografiya", Emoji = "🌍", Description = "Mamlakatlar, shaharlar va tabiat haqida savollar", LanguageCode = "uz", TranslationGroupId = geoGroupId, CategoryGroup = "general" },

            // История (Популярные)
            new() { Name = "История", Emoji = "📜", Description = "Исторические события и личности", LanguageCode = "ru", TranslationGroupId = historyGroupId, CategoryGroup = "popular" },
            new() { Name = "इतिहास", Emoji = "📜", Description = "ऐतिहासिक घटनाएं और व्यक्तित्व", LanguageCode = "hi", TranslationGroupId = historyGroupId, CategoryGroup = "popular" },
            new() { Name = "História", Emoji = "📜", Description = "Eventos históricos e personalidades", LanguageCode = "pt", TranslationGroupId = historyGroupId, CategoryGroup = "popular" },
            new() { Name = "تاریخ", Emoji = "📜", Description = "رویدادها و شخصیت‌های تاریخی", LanguageCode = "fa", TranslationGroupId = historyGroupId, CategoryGroup = "popular" },
            new() { Name = "Geschichte", Emoji = "📜", Description = "Historische Ereignisse und Persönlichkeiten", LanguageCode = "de", TranslationGroupId = historyGroupId, CategoryGroup = "popular" },
            new() { Name = "Tarix", Emoji = "📜", Description = "Tarixiy voqealar va shaxslar", LanguageCode = "uz", TranslationGroupId = historyGroupId, CategoryGroup = "popular" },

            // Наука (Специальные)
            new() { Name = "Наука", Emoji = "🔬", Description = "Физика, химия, биология", LanguageCode = "ru", TranslationGroupId = scienceGroupId, CategoryGroup = "special" },
            new() { Name = "विज्ञान", Emoji = "🔬", Description = "भौतिकी, रसायन विज्ञान, जीव विज्ञान", LanguageCode = "hi", TranslationGroupId = scienceGroupId, CategoryGroup = "special" },
            new() { Name = "Ciência", Emoji = "🔬", Description = "Física, química, biologia", LanguageCode = "pt", TranslationGroupId = scienceGroupId, CategoryGroup = "special" },
            new() { Name = "علم", Emoji = "🔬", Description = "فیزیک، شیمی، زیست‌شناسی", LanguageCode = "fa", TranslationGroupId = scienceGroupId, CategoryGroup = "special" },
            new() { Name = "Wissenschaft", Emoji = "🔬", Description = "Physik, Chemie, Biologie", LanguageCode = "de", TranslationGroupId = scienceGroupId, CategoryGroup = "special" },
            new() { Name = "Fan", Emoji = "🔬", Description = "Fizika, kimyo, biologiya", LanguageCode = "uz", TranslationGroupId = scienceGroupId, CategoryGroup = "special" },
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // Создаём вопросы с переводами
        var questions = new List<Question>();

        // Получаем реальные ID категорий по TranslationGroupId и языку
        var savedCategories = await context.Categories.ToListAsync();
        var geoCategoryIds = savedCategories
            .Where(c => c.TranslationGroupId == geoGroupId)
            .ToDictionary(c => c.LanguageCode, c => c.Id);
        var historyCategoryIds = savedCategories
            .Where(c => c.TranslationGroupId == historyGroupId)
            .ToDictionary(c => c.LanguageCode, c => c.Id);
        var scienceCategoryIds = savedCategories
            .Where(c => c.TranslationGroupId == scienceGroupId)
            .ToDictionary(c => c.LanguageCode, c => c.Id);

        // ===== ГЕОГРАФИЯ (18 вопросов x 6 языков = 108 вопросов) =====

        // Вопрос 1: Столица Франции
        var q1 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q1, CategoryId = geoCategoryIds["ru"], LanguageCode = "ru", Text = "Столица Франции?", CorrectAnswer = "Париж", WrongAnswer1 = "Лондон", WrongAnswer2 = "Берлин", WrongAnswer3 = "Мадрид" },
            new Question { TranslationGroupId = q1, CategoryId = geoCategoryIds["hi"], LanguageCode = "hi", Text = "फ्रांस की राजधानी क्या है?", CorrectAnswer = "पेरिस", WrongAnswer1 = "लंदन", WrongAnswer2 = "बर्लिन", WrongAnswer3 = "मैड्रिड" },
            new Question { TranslationGroupId = q1, CategoryId = geoCategoryIds["pt"], LanguageCode = "pt", Text = "Qual é a capital da França?", CorrectAnswer = "Paris", WrongAnswer1 = "Londres", WrongAnswer2 = "Berlim", WrongAnswer3 = "Madri" },
            new Question { TranslationGroupId = q1, CategoryId = geoCategoryIds["fa"], LanguageCode = "fa", Text = "پایتخت فرانسه کجاست؟", CorrectAnswer = "پاریس", WrongAnswer1 = "لندن", WrongAnswer2 = "برلین", WrongAnswer3 = "مادرید" },
            new Question { TranslationGroupId = q1, CategoryId = geoCategoryIds["de"], LanguageCode = "de", Text = "Was ist die Hauptstadt von Frankreich?", CorrectAnswer = "Paris", WrongAnswer1 = "London", WrongAnswer2 = "Berlin", WrongAnswer3 = "Madrid" },
            new Question { TranslationGroupId = q1, CategoryId = geoCategoryIds["uz"], LanguageCode = "uz", Text = "Fransiyaning poytaxti qaysi?", CorrectAnswer = "Parij", WrongAnswer1 = "London", WrongAnswer2 = "Berlin", WrongAnswer3 = "Madrid" },
        });

        // Вопрос 2: Самая длинная река
        var q2 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q2, CategoryId = geoCategoryIds["ru"], LanguageCode = "ru", Text = "Какая самая длинная река в мире?", CorrectAnswer = "Нил", WrongAnswer1 = "Амазонка", WrongAnswer2 = "Миссисипи", WrongAnswer3 = "Янцзы" },
            new Question { TranslationGroupId = q2, CategoryId = geoCategoryIds["hi"], LanguageCode = "hi", Text = "दुनिया की सबसे लंबी नदी कौन सी है?", CorrectAnswer = "नील", WrongAnswer1 = "अमेज़न", WrongAnswer2 = "मिसिसिपी", WrongAnswer3 = "यांग्त्ज़ी" },
            new Question { TranslationGroupId = q2, CategoryId = geoCategoryIds["pt"], LanguageCode = "pt", Text = "Qual é o rio mais longo do mundo?", CorrectAnswer = "Nilo", WrongAnswer1 = "Amazonas", WrongAnswer2 = "Mississippi", WrongAnswer3 = "Yangtzé" },
            new Question { TranslationGroupId = q2, CategoryId = geoCategoryIds["fa"], LanguageCode = "fa", Text = "طولانی‌ترین رودخانه جهان کدام است؟", CorrectAnswer = "نیل", WrongAnswer1 = "آمازون", WrongAnswer2 = "می‌سی‌سی‌پی", WrongAnswer3 = "یانگ‌تسه" },
            new Question { TranslationGroupId = q2, CategoryId = geoCategoryIds["de"], LanguageCode = "de", Text = "Welcher ist der längste Fluss der Welt?", CorrectAnswer = "Nil", WrongAnswer1 = "Amazonas", WrongAnswer2 = "Mississippi", WrongAnswer3 = "Jangtse" },
            new Question { TranslationGroupId = q2, CategoryId = geoCategoryIds["uz"], LanguageCode = "uz", Text = "Dunyodagi eng uzun daryo qaysi?", CorrectAnswer = "Nil", WrongAnswer1 = "Amazonka", WrongAnswer2 = "Missisipi", WrongAnswer3 = "Yanszi" },
        });

        // Вопрос 3: Столица Японии
        var q3 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q3, CategoryId = geoCategoryIds["ru"], LanguageCode = "ru", Text = "Столица Японии?", CorrectAnswer = "Токио", WrongAnswer1 = "Киото", WrongAnswer2 = "Осака", WrongAnswer3 = "Хиросима" },
            new Question { TranslationGroupId = q3, CategoryId = geoCategoryIds["hi"], LanguageCode = "hi", Text = "जापान की राजधानी क्या है?", CorrectAnswer = "टोक्यो", WrongAnswer1 = "क्योटो", WrongAnswer2 = "ओसाका", WrongAnswer3 = "हिरोशिमा" },
            new Question { TranslationGroupId = q3, CategoryId = geoCategoryIds["pt"], LanguageCode = "pt", Text = "Qual é a capital do Japão?", CorrectAnswer = "Tóquio", WrongAnswer1 = "Quioto", WrongAnswer2 = "Osaka", WrongAnswer3 = "Hiroshima" },
            new Question { TranslationGroupId = q3, CategoryId = geoCategoryIds["fa"], LanguageCode = "fa", Text = "پایتخت ژاپن کجاست؟", CorrectAnswer = "توکیو", WrongAnswer1 = "کیوتو", WrongAnswer2 = "اوساکا", WrongAnswer3 = "هیروشیما" },
            new Question { TranslationGroupId = q3, CategoryId = geoCategoryIds["de"], LanguageCode = "de", Text = "Was ist die Hauptstadt von Japan?", CorrectAnswer = "Tokio", WrongAnswer1 = "Kyoto", WrongAnswer2 = "Osaka", WrongAnswer3 = "Hiroshima" },
            new Question { TranslationGroupId = q3, CategoryId = geoCategoryIds["uz"], LanguageCode = "uz", Text = "Yaponiyaning poytaxti qaysi?", CorrectAnswer = "Tokio", WrongAnswer1 = "Kioto", WrongAnswer2 = "Osaka", WrongAnswer3 = "Xirosima" },
        });

        // Вопрос 4: Самый большой океан
        var q4 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q4, CategoryId = geoCategoryIds["ru"], LanguageCode = "ru", Text = "Какой океан самый большой?", CorrectAnswer = "Тихий", WrongAnswer1 = "Атлантический", WrongAnswer2 = "Индийский", WrongAnswer3 = "Северный Ледовитый" },
            new Question { TranslationGroupId = q4, CategoryId = geoCategoryIds["hi"], LanguageCode = "hi", Text = "सबसे बड़ा महासागर कौन सा है?", CorrectAnswer = "प्रशांत", WrongAnswer1 = "अटलांटिक", WrongAnswer2 = "हिंद", WrongAnswer3 = "आर्कटिक" },
            new Question { TranslationGroupId = q4, CategoryId = geoCategoryIds["pt"], LanguageCode = "pt", Text = "Qual é o maior oceano?", CorrectAnswer = "Pacífico", WrongAnswer1 = "Atlântico", WrongAnswer2 = "Índico", WrongAnswer3 = "Ártico" },
            new Question { TranslationGroupId = q4, CategoryId = geoCategoryIds["fa"], LanguageCode = "fa", Text = "بزرگترین اقیانوس کدام است؟", CorrectAnswer = "آرام", WrongAnswer1 = "اطلس", WrongAnswer2 = "هند", WrongAnswer3 = "منجمد شمالی" },
            new Question { TranslationGroupId = q4, CategoryId = geoCategoryIds["de"], LanguageCode = "de", Text = "Welcher ist der größte Ozean?", CorrectAnswer = "Pazifik", WrongAnswer1 = "Atlantik", WrongAnswer2 = "Indischer Ozean", WrongAnswer3 = "Arktischer Ozean" },
            new Question { TranslationGroupId = q4, CategoryId = geoCategoryIds["uz"], LanguageCode = "uz", Text = "Eng katta okean qaysi?", CorrectAnswer = "Tinch", WrongAnswer1 = "Atlantika", WrongAnswer2 = "Hind", WrongAnswer3 = "Shimoliy Muz" },
        });

        // Вопрос 5: Самая высокая гора
        var q5 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q5, CategoryId = geoCategoryIds["ru"], LanguageCode = "ru", Text = "Какая самая высокая гора в мире?", CorrectAnswer = "Эверест", WrongAnswer1 = "К2", WrongAnswer2 = "Килиманджаро", WrongAnswer3 = "Монблан" },
            new Question { TranslationGroupId = q5, CategoryId = geoCategoryIds["hi"], LanguageCode = "hi", Text = "दुनिया का सबसे ऊंचा पर्वत कौन सा है?", CorrectAnswer = "एवरेस्ट", WrongAnswer1 = "K2", WrongAnswer2 = "किलिमंजारो", WrongAnswer3 = "मोंट ब्लांक" },
            new Question { TranslationGroupId = q5, CategoryId = geoCategoryIds["pt"], LanguageCode = "pt", Text = "Qual é a montanha mais alta do mundo?", CorrectAnswer = "Everest", WrongAnswer1 = "K2", WrongAnswer2 = "Kilimanjaro", WrongAnswer3 = "Mont Blanc" },
            new Question { TranslationGroupId = q5, CategoryId = geoCategoryIds["fa"], LanguageCode = "fa", Text = "بلندترین کوه جهان کدام است؟", CorrectAnswer = "اورست", WrongAnswer1 = "کی‌۲", WrongAnswer2 = "کلیمانجارو", WrongAnswer3 = "مون‌بلان" },
            new Question { TranslationGroupId = q5, CategoryId = geoCategoryIds["de"], LanguageCode = "de", Text = "Welcher ist der höchste Berg der Welt?", CorrectAnswer = "Everest", WrongAnswer1 = "K2", WrongAnswer2 = "Kilimandscharo", WrongAnswer3 = "Mont Blanc" },
            new Question { TranslationGroupId = q5, CategoryId = geoCategoryIds["uz"], LanguageCode = "uz", Text = "Dunyodagi eng baland tog' qaysi?", CorrectAnswer = "Everest", WrongAnswer1 = "K2", WrongAnswer2 = "Kilimanjaro", WrongAnswer3 = "Monblan" },
        });

        // Вопрос 6: Страна в форме сапога
        var q6 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q6, CategoryId = geoCategoryIds["ru"], LanguageCode = "ru", Text = "Какая страна имеет форму сапога?", CorrectAnswer = "Италия", WrongAnswer1 = "Греция", WrongAnswer2 = "Испания", WrongAnswer3 = "Португалия" },
            new Question { TranslationGroupId = q6, CategoryId = geoCategoryIds["hi"], LanguageCode = "hi", Text = "कौन सा देश जूते के आकार का है?", CorrectAnswer = "इटली", WrongAnswer1 = "ग्रीस", WrongAnswer2 = "स्पेन", WrongAnswer3 = "पुर्तगाल" },
            new Question { TranslationGroupId = q6, CategoryId = geoCategoryIds["pt"], LanguageCode = "pt", Text = "Qual país tem forma de bota?", CorrectAnswer = "Itália", WrongAnswer1 = "Grécia", WrongAnswer2 = "Espanha", WrongAnswer3 = "Portugal" },
            new Question { TranslationGroupId = q6, CategoryId = geoCategoryIds["fa"], LanguageCode = "fa", Text = "کدام کشور شکل چکمه دارد؟", CorrectAnswer = "ایتالیا", WrongAnswer1 = "یونان", WrongAnswer2 = "اسپانیا", WrongAnswer3 = "پرتغال" },
            new Question { TranslationGroupId = q6, CategoryId = geoCategoryIds["de"], LanguageCode = "de", Text = "Welches Land hat die Form eines Stiefels?", CorrectAnswer = "Italien", WrongAnswer1 = "Griechenland", WrongAnswer2 = "Spanien", WrongAnswer3 = "Portugal" },
            new Question { TranslationGroupId = q6, CategoryId = geoCategoryIds["uz"], LanguageCode = "uz", Text = "Qaysi davlat etik shaklida?", CorrectAnswer = "Italiya", WrongAnswer1 = "Gretsiya", WrongAnswer2 = "Ispaniya", WrongAnswer3 = "Portugaliya" },
        });

        // ===== ИСТОРИЯ (6 вопросов x 6 языков = 36 вопросов) =====

        // Вопрос 7: Начало Второй мировой
        var q7 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q7, CategoryId = historyCategoryIds["ru"], LanguageCode = "ru", Text = "В каком году началась Вторая мировая война?", CorrectAnswer = "1939", WrongAnswer1 = "1941", WrongAnswer2 = "1938", WrongAnswer3 = "1940" },
            new Question { TranslationGroupId = q7, CategoryId = historyCategoryIds["hi"], LanguageCode = "hi", Text = "द्वितीय विश्व युद्ध कब शुरू हुआ?", CorrectAnswer = "1939", WrongAnswer1 = "1941", WrongAnswer2 = "1938", WrongAnswer3 = "1940" },
            new Question { TranslationGroupId = q7, CategoryId = historyCategoryIds["pt"], LanguageCode = "pt", Text = "Em que ano começou a Segunda Guerra Mundial?", CorrectAnswer = "1939", WrongAnswer1 = "1941", WrongAnswer2 = "1938", WrongAnswer3 = "1940" },
            new Question { TranslationGroupId = q7, CategoryId = historyCategoryIds["fa"], LanguageCode = "fa", Text = "جنگ جهانی دوم در چه سالی آغاز شد؟", CorrectAnswer = "۱۹۳۹", WrongAnswer1 = "۱۹۴۱", WrongAnswer2 = "۱۹۳۸", WrongAnswer3 = "۱۹۴۰" },
            new Question { TranslationGroupId = q7, CategoryId = historyCategoryIds["de"], LanguageCode = "de", Text = "In welchem Jahr begann der Zweite Weltkrieg?", CorrectAnswer = "1939", WrongAnswer1 = "1941", WrongAnswer2 = "1938", WrongAnswer3 = "1940" },
            new Question { TranslationGroupId = q7, CategoryId = historyCategoryIds["uz"], LanguageCode = "uz", Text = "Ikkinchi jahon urushi qachon boshlangan?", CorrectAnswer = "1939", WrongAnswer1 = "1941", WrongAnswer2 = "1938", WrongAnswer3 = "1940" },
        });

        // Вопрос 8: Первый президент США
        var q8 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q8, CategoryId = historyCategoryIds["ru"], LanguageCode = "ru", Text = "Кто был первым президентом США?", CorrectAnswer = "Джордж Вашингтон", WrongAnswer1 = "Авраам Линкольн", WrongAnswer2 = "Томас Джефферсон", WrongAnswer3 = "Бенджамин Франклин" },
            new Question { TranslationGroupId = q8, CategoryId = historyCategoryIds["hi"], LanguageCode = "hi", Text = "अमेरिका के पहले राष्ट्रपति कौन थे?", CorrectAnswer = "जॉर्ज वाशिंगटन", WrongAnswer1 = "अब्राहम लिंकन", WrongAnswer2 = "थॉमस जेफरसन", WrongAnswer3 = "बेंजामिन फ्रैंकलिन" },
            new Question { TranslationGroupId = q8, CategoryId = historyCategoryIds["pt"], LanguageCode = "pt", Text = "Quem foi o primeiro presidente dos EUA?", CorrectAnswer = "George Washington", WrongAnswer1 = "Abraham Lincoln", WrongAnswer2 = "Thomas Jefferson", WrongAnswer3 = "Benjamin Franklin" },
            new Question { TranslationGroupId = q8, CategoryId = historyCategoryIds["fa"], LanguageCode = "fa", Text = "اولین رئیس جمهور آمریکا چه کسی بود؟", CorrectAnswer = "جرج واشنگتن", WrongAnswer1 = "آبراهام لینکلن", WrongAnswer2 = "توماس جفرسون", WrongAnswer3 = "بنجامین فرانکلین" },
            new Question { TranslationGroupId = q8, CategoryId = historyCategoryIds["de"], LanguageCode = "de", Text = "Wer war der erste Präsident der USA?", CorrectAnswer = "George Washington", WrongAnswer1 = "Abraham Lincoln", WrongAnswer2 = "Thomas Jefferson", WrongAnswer3 = "Benjamin Franklin" },
            new Question { TranslationGroupId = q8, CategoryId = historyCategoryIds["uz"], LanguageCode = "uz", Text = "AQShning birinchi prezidenti kim edi?", CorrectAnswer = "Jorj Vashington", WrongAnswer1 = "Avraam Linkoln", WrongAnswer2 = "Tomas Jefferson", WrongAnswer3 = "Benjamin Franklin" },
        });

        // Вопрос 9: Падение Берлинской стены
        var q9 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q9, CategoryId = historyCategoryIds["ru"], LanguageCode = "ru", Text = "В каком году пала Берлинская стена?", CorrectAnswer = "1989", WrongAnswer1 = "1991", WrongAnswer2 = "1987", WrongAnswer3 = "1990" },
            new Question { TranslationGroupId = q9, CategoryId = historyCategoryIds["hi"], LanguageCode = "hi", Text = "बर्लिन की दीवार कब गिरी?", CorrectAnswer = "1989", WrongAnswer1 = "1991", WrongAnswer2 = "1987", WrongAnswer3 = "1990" },
            new Question { TranslationGroupId = q9, CategoryId = historyCategoryIds["pt"], LanguageCode = "pt", Text = "Em que ano caiu o Muro de Berlim?", CorrectAnswer = "1989", WrongAnswer1 = "1991", WrongAnswer2 = "1987", WrongAnswer3 = "1990" },
            new Question { TranslationGroupId = q9, CategoryId = historyCategoryIds["fa"], LanguageCode = "fa", Text = "دیوار برلین در چه سالی فروریخت؟", CorrectAnswer = "۱۹۸۹", WrongAnswer1 = "۱۹۹۱", WrongAnswer2 = "۱۹۸۷", WrongAnswer3 = "۱۹۹۰" },
            new Question { TranslationGroupId = q9, CategoryId = historyCategoryIds["de"], LanguageCode = "de", Text = "In welchem Jahr fiel die Berliner Mauer?", CorrectAnswer = "1989", WrongAnswer1 = "1991", WrongAnswer2 = "1987", WrongAnswer3 = "1990" },
            new Question { TranslationGroupId = q9, CategoryId = historyCategoryIds["uz"], LanguageCode = "uz", Text = "Berlin devori qachon qulab tushgan?", CorrectAnswer = "1989", WrongAnswer1 = "1991", WrongAnswer2 = "1987", WrongAnswer3 = "1990" },
        });

        // Вопрос 10: Первый полёт в космос
        var q10 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q10, CategoryId = historyCategoryIds["ru"], LanguageCode = "ru", Text = "В каком году человек впервые полетел в космос?", CorrectAnswer = "1961", WrongAnswer1 = "1957", WrongAnswer2 = "1963", WrongAnswer3 = "1969" },
            new Question { TranslationGroupId = q10, CategoryId = historyCategoryIds["hi"], LanguageCode = "hi", Text = "इंसान पहली बार अंतरिक्ष में कब गया?", CorrectAnswer = "1961", WrongAnswer1 = "1957", WrongAnswer2 = "1963", WrongAnswer3 = "1969" },
            new Question { TranslationGroupId = q10, CategoryId = historyCategoryIds["pt"], LanguageCode = "pt", Text = "Em que ano o homem foi ao espaço pela primeira vez?", CorrectAnswer = "1961", WrongAnswer1 = "1957", WrongAnswer2 = "1963", WrongAnswer3 = "1969" },
            new Question { TranslationGroupId = q10, CategoryId = historyCategoryIds["fa"], LanguageCode = "fa", Text = "انسان اولین بار در چه سالی به فضا رفت؟", CorrectAnswer = "۱۹۶۱", WrongAnswer1 = "۱۹۵۷", WrongAnswer2 = "۱۹۶۳", WrongAnswer3 = "۱۹۶۹" },
            new Question { TranslationGroupId = q10, CategoryId = historyCategoryIds["de"], LanguageCode = "de", Text = "In welchem Jahr flog der erste Mensch ins All?", CorrectAnswer = "1961", WrongAnswer1 = "1957", WrongAnswer2 = "1963", WrongAnswer3 = "1969" },
            new Question { TranslationGroupId = q10, CategoryId = historyCategoryIds["uz"], LanguageCode = "uz", Text = "Inson birinchi marta kosmosga qachon uchgan?", CorrectAnswer = "1961", WrongAnswer1 = "1957", WrongAnswer2 = "1963", WrongAnswer3 = "1969" },
        });

        // Вопрос 11: Открытие Америки
        var q11 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q11, CategoryId = historyCategoryIds["ru"], LanguageCode = "ru", Text = "Кто открыл Америку?", CorrectAnswer = "Христофор Колумб", WrongAnswer1 = "Америго Веспуччи", WrongAnswer2 = "Васко да Гама", WrongAnswer3 = "Фернан Магеллан" },
            new Question { TranslationGroupId = q11, CategoryId = historyCategoryIds["hi"], LanguageCode = "hi", Text = "अमेरिका की खोज किसने की?", CorrectAnswer = "क्रिस्टोफर कोलंबस", WrongAnswer1 = "अमेरिगो वेस्पुची", WrongAnswer2 = "वास्को डी गामा", WrongAnswer3 = "फर्डिनेंड मैगलन" },
            new Question { TranslationGroupId = q11, CategoryId = historyCategoryIds["pt"], LanguageCode = "pt", Text = "Quem descobriu a América?", CorrectAnswer = "Cristóvão Colombo", WrongAnswer1 = "Américo Vespúcio", WrongAnswer2 = "Vasco da Gama", WrongAnswer3 = "Fernão de Magalhães" },
            new Question { TranslationGroupId = q11, CategoryId = historyCategoryIds["fa"], LanguageCode = "fa", Text = "چه کسی آمریکا را کشف کرد؟", CorrectAnswer = "کریستف کلمب", WrongAnswer1 = "آمریگو وسپوچی", WrongAnswer2 = "واسکو دا گاما", WrongAnswer3 = "فرناندو ماژلان" },
            new Question { TranslationGroupId = q11, CategoryId = historyCategoryIds["de"], LanguageCode = "de", Text = "Wer entdeckte Amerika?", CorrectAnswer = "Christoph Kolumbus", WrongAnswer1 = "Amerigo Vespucci", WrongAnswer2 = "Vasco da Gama", WrongAnswer3 = "Ferdinand Magellan" },
            new Question { TranslationGroupId = q11, CategoryId = historyCategoryIds["uz"], LanguageCode = "uz", Text = "Amerikani kim kashf etgan?", CorrectAnswer = "Xristofor Kolumb", WrongAnswer1 = "Amerigo Vespuchchi", WrongAnswer2 = "Vasko da Gama", WrongAnswer3 = "Fernan Magellan" },
        });

        // Вопрос 12: Французская революция
        var q12 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q12, CategoryId = historyCategoryIds["ru"], LanguageCode = "ru", Text = "Когда произошла Французская революция?", CorrectAnswer = "1789", WrongAnswer1 = "1776", WrongAnswer2 = "1799", WrongAnswer3 = "1812" },
            new Question { TranslationGroupId = q12, CategoryId = historyCategoryIds["hi"], LanguageCode = "hi", Text = "फ्रांसीसी क्रांति कब हुई?", CorrectAnswer = "1789", WrongAnswer1 = "1776", WrongAnswer2 = "1799", WrongAnswer3 = "1812" },
            new Question { TranslationGroupId = q12, CategoryId = historyCategoryIds["pt"], LanguageCode = "pt", Text = "Quando ocorreu a Revolução Francesa?", CorrectAnswer = "1789", WrongAnswer1 = "1776", WrongAnswer2 = "1799", WrongAnswer3 = "1812" },
            new Question { TranslationGroupId = q12, CategoryId = historyCategoryIds["fa"], LanguageCode = "fa", Text = "انقلاب فرانسه چه زمانی رخ داد؟", CorrectAnswer = "۱۷۸۹", WrongAnswer1 = "۱۷۷۶", WrongAnswer2 = "۱۷۹۹", WrongAnswer3 = "۱۸۱۲" },
            new Question { TranslationGroupId = q12, CategoryId = historyCategoryIds["de"], LanguageCode = "de", Text = "Wann fand die Französische Revolution statt?", CorrectAnswer = "1789", WrongAnswer1 = "1776", WrongAnswer2 = "1799", WrongAnswer3 = "1812" },
            new Question { TranslationGroupId = q12, CategoryId = historyCategoryIds["uz"], LanguageCode = "uz", Text = "Fransuz inqilobi qachon bo'lgan?", CorrectAnswer = "1789", WrongAnswer1 = "1776", WrongAnswer2 = "1799", WrongAnswer3 = "1812" },
        });

        // ===== НАУКА (6 вопросов x 6 языков = 36 вопросов) =====

        // Вопрос 13: Символ золота
        var q13 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q13, CategoryId = scienceCategoryIds["ru"], LanguageCode = "ru", Text = "Какой химический символ у золота?", CorrectAnswer = "Au", WrongAnswer1 = "Ag", WrongAnswer2 = "Fe", WrongAnswer3 = "Go" },
            new Question { TranslationGroupId = q13, CategoryId = scienceCategoryIds["hi"], LanguageCode = "hi", Text = "सोने का रासायनिक प्रतीक क्या है?", CorrectAnswer = "Au", WrongAnswer1 = "Ag", WrongAnswer2 = "Fe", WrongAnswer3 = "Go" },
            new Question { TranslationGroupId = q13, CategoryId = scienceCategoryIds["pt"], LanguageCode = "pt", Text = "Qual é o símbolo químico do ouro?", CorrectAnswer = "Au", WrongAnswer1 = "Ag", WrongAnswer2 = "Fe", WrongAnswer3 = "Go" },
            new Question { TranslationGroupId = q13, CategoryId = scienceCategoryIds["fa"], LanguageCode = "fa", Text = "نماد شیمیایی طلا چیست؟", CorrectAnswer = "Au", WrongAnswer1 = "Ag", WrongAnswer2 = "Fe", WrongAnswer3 = "Go" },
            new Question { TranslationGroupId = q13, CategoryId = scienceCategoryIds["de"], LanguageCode = "de", Text = "Was ist das chemische Symbol für Gold?", CorrectAnswer = "Au", WrongAnswer1 = "Ag", WrongAnswer2 = "Fe", WrongAnswer3 = "Go" },
            new Question { TranslationGroupId = q13, CategoryId = scienceCategoryIds["uz"], LanguageCode = "uz", Text = "Oltinning kimyoviy belgisi qanday?", CorrectAnswer = "Au", WrongAnswer1 = "Ag", WrongAnswer2 = "Fe", WrongAnswer3 = "Go" },
        });

        // Вопрос 14: Количество планет
        var q14 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q14, CategoryId = scienceCategoryIds["ru"], LanguageCode = "ru", Text = "Сколько планет в Солнечной системе?", CorrectAnswer = "8", WrongAnswer1 = "9", WrongAnswer2 = "7", WrongAnswer3 = "10" },
            new Question { TranslationGroupId = q14, CategoryId = scienceCategoryIds["hi"], LanguageCode = "hi", Text = "सौरमंडल में कितने ग्रह हैं?", CorrectAnswer = "8", WrongAnswer1 = "9", WrongAnswer2 = "7", WrongAnswer3 = "10" },
            new Question { TranslationGroupId = q14, CategoryId = scienceCategoryIds["pt"], LanguageCode = "pt", Text = "Quantos planetas existem no Sistema Solar?", CorrectAnswer = "8", WrongAnswer1 = "9", WrongAnswer2 = "7", WrongAnswer3 = "10" },
            new Question { TranslationGroupId = q14, CategoryId = scienceCategoryIds["fa"], LanguageCode = "fa", Text = "چند سیاره در منظومه شمسی وجود دارد؟", CorrectAnswer = "۸", WrongAnswer1 = "۹", WrongAnswer2 = "۷", WrongAnswer3 = "۱۰" },
            new Question { TranslationGroupId = q14, CategoryId = scienceCategoryIds["de"], LanguageCode = "de", Text = "Wie viele Planeten gibt es im Sonnensystem?", CorrectAnswer = "8", WrongAnswer1 = "9", WrongAnswer2 = "7", WrongAnswer3 = "10" },
            new Question { TranslationGroupId = q14, CategoryId = scienceCategoryIds["uz"], LanguageCode = "uz", Text = "Quyosh tizimida nechta sayyora bor?", CorrectAnswer = "8", WrongAnswer1 = "9", WrongAnswer2 = "7", WrongAnswer3 = "10" },
        });

        // Вопрос 15: Теория относительности
        var q15 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q15, CategoryId = scienceCategoryIds["ru"], LanguageCode = "ru", Text = "Кто разработал теорию относительности?", CorrectAnswer = "Альберт Эйнштейн", WrongAnswer1 = "Исаак Ньютон", WrongAnswer2 = "Никола Тесла", WrongAnswer3 = "Стивен Хокинг" },
            new Question { TranslationGroupId = q15, CategoryId = scienceCategoryIds["hi"], LanguageCode = "hi", Text = "सापेक्षता का सिद्धांत किसने विकसित किया?", CorrectAnswer = "अल्बर्ट आइंस्टीन", WrongAnswer1 = "आइजैक न्यूटन", WrongAnswer2 = "निकोला टेस्ला", WrongAnswer3 = "स्टीफन हॉकिंग" },
            new Question { TranslationGroupId = q15, CategoryId = scienceCategoryIds["pt"], LanguageCode = "pt", Text = "Quem desenvolveu a teoria da relatividade?", CorrectAnswer = "Albert Einstein", WrongAnswer1 = "Isaac Newton", WrongAnswer2 = "Nikola Tesla", WrongAnswer3 = "Stephen Hawking" },
            new Question { TranslationGroupId = q15, CategoryId = scienceCategoryIds["fa"], LanguageCode = "fa", Text = "چه کسی نظریه نسبیت را توسعه داد؟", CorrectAnswer = "آلبرت اینشتین", WrongAnswer1 = "آیزاک نیوتن", WrongAnswer2 = "نیکولا تسلا", WrongAnswer3 = "استیون هاوکینگ" },
            new Question { TranslationGroupId = q15, CategoryId = scienceCategoryIds["de"], LanguageCode = "de", Text = "Wer entwickelte die Relativitätstheorie?", CorrectAnswer = "Albert Einstein", WrongAnswer1 = "Isaac Newton", WrongAnswer2 = "Nikola Tesla", WrongAnswer3 = "Stephen Hawking" },
            new Question { TranslationGroupId = q15, CategoryId = scienceCategoryIds["uz"], LanguageCode = "uz", Text = "Nisbiylik nazariyasini kim ishlab chiqqan?", CorrectAnswer = "Albert Eynshteyn", WrongAnswer1 = "Isaak Nyuton", WrongAnswer2 = "Nikola Tesla", WrongAnswer3 = "Stiven Xoking" },
        });

        // Вопрос 16: Какой газ мы вдыхаем больше всего
        var q16 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q16, CategoryId = scienceCategoryIds["ru"], LanguageCode = "ru", Text = "Какой газ мы вдыхаем больше всего?", CorrectAnswer = "Азот", WrongAnswer1 = "Кислород", WrongAnswer2 = "Углекислый газ", WrongAnswer3 = "Водород" },
            new Question { TranslationGroupId = q16, CategoryId = scienceCategoryIds["hi"], LanguageCode = "hi", Text = "हम कौन सी गैस सबसे ज्यादा सांस लेते हैं?", CorrectAnswer = "नाइट्रोजन", WrongAnswer1 = "ऑक्सीजन", WrongAnswer2 = "कार्बन डाइऑक्साइड", WrongAnswer3 = "हाइड्रोजन" },
            new Question { TranslationGroupId = q16, CategoryId = scienceCategoryIds["pt"], LanguageCode = "pt", Text = "Qual gás respiramos em maior quantidade?", CorrectAnswer = "Nitrogênio", WrongAnswer1 = "Oxigênio", WrongAnswer2 = "Dióxido de carbono", WrongAnswer3 = "Hidrogênio" },
            new Question { TranslationGroupId = q16, CategoryId = scienceCategoryIds["fa"], LanguageCode = "fa", Text = "کدام گاز را بیشتر تنفس می‌کنیم؟", CorrectAnswer = "نیتروژن", WrongAnswer1 = "اکسیژن", WrongAnswer2 = "دی‌اکسید کربن", WrongAnswer3 = "هیدروژن" },
            new Question { TranslationGroupId = q16, CategoryId = scienceCategoryIds["de"], LanguageCode = "de", Text = "Welches Gas atmen wir am meisten ein?", CorrectAnswer = "Stickstoff", WrongAnswer1 = "Sauerstoff", WrongAnswer2 = "Kohlendioxid", WrongAnswer3 = "Wasserstoff" },
            new Question { TranslationGroupId = q16, CategoryId = scienceCategoryIds["uz"], LanguageCode = "uz", Text = "Qaysi gazni eng ko'p nafas olamiz?", CorrectAnswer = "Azot", WrongAnswer1 = "Kislorod", WrongAnswer2 = "Karbonat angidrid", WrongAnswer3 = "Vodorod" },
        });

        // Вопрос 17: Ближайшая планета к Солнцу
        var q17 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q17, CategoryId = scienceCategoryIds["ru"], LanguageCode = "ru", Text = "Какая планета ближе всего к Солнцу?", CorrectAnswer = "Меркурий", WrongAnswer1 = "Венера", WrongAnswer2 = "Марс", WrongAnswer3 = "Земля" },
            new Question { TranslationGroupId = q17, CategoryId = scienceCategoryIds["hi"], LanguageCode = "hi", Text = "सूर्य के सबसे नजदीक कौन सा ग्रह है?", CorrectAnswer = "बुध", WrongAnswer1 = "शुक्र", WrongAnswer2 = "मंगल", WrongAnswer3 = "पृथ्वी" },
            new Question { TranslationGroupId = q17, CategoryId = scienceCategoryIds["pt"], LanguageCode = "pt", Text = "Qual planeta está mais perto do Sol?", CorrectAnswer = "Mercúrio", WrongAnswer1 = "Vênus", WrongAnswer2 = "Marte", WrongAnswer3 = "Terra" },
            new Question { TranslationGroupId = q17, CategoryId = scienceCategoryIds["fa"], LanguageCode = "fa", Text = "کدام سیاره به خورشید نزدیک‌تر است؟", CorrectAnswer = "عطارد", WrongAnswer1 = "زهره", WrongAnswer2 = "مریخ", WrongAnswer3 = "زمین" },
            new Question { TranslationGroupId = q17, CategoryId = scienceCategoryIds["de"], LanguageCode = "de", Text = "Welcher Planet ist der Sonne am nächsten?", CorrectAnswer = "Merkur", WrongAnswer1 = "Venus", WrongAnswer2 = "Mars", WrongAnswer3 = "Erde" },
            new Question { TranslationGroupId = q17, CategoryId = scienceCategoryIds["uz"], LanguageCode = "uz", Text = "Quyoshga eng yaqin sayyora qaysi?", CorrectAnswer = "Merkuriy", WrongAnswer1 = "Venera", WrongAnswer2 = "Mars", WrongAnswer3 = "Yer" },
        });

        // Вопрос 18: Скорость света
        var q18 = Guid.NewGuid();
        questions.AddRange(new[]
        {
            new Question { TranslationGroupId = q18, CategoryId = scienceCategoryIds["ru"], LanguageCode = "ru", Text = "Скорость света приблизительно равна?", CorrectAnswer = "300 000 км/с", WrongAnswer1 = "150 000 км/с", WrongAnswer2 = "500 000 км/с", WrongAnswer3 = "1 000 000 км/с" },
            new Question { TranslationGroupId = q18, CategoryId = scienceCategoryIds["hi"], LanguageCode = "hi", Text = "प्रकाश की गति लगभग कितनी है?", CorrectAnswer = "300,000 km/s", WrongAnswer1 = "150,000 km/s", WrongAnswer2 = "500,000 km/s", WrongAnswer3 = "1,000,000 km/s" },
            new Question { TranslationGroupId = q18, CategoryId = scienceCategoryIds["pt"], LanguageCode = "pt", Text = "A velocidade da luz é aproximadamente?", CorrectAnswer = "300.000 km/s", WrongAnswer1 = "150.000 km/s", WrongAnswer2 = "500.000 km/s", WrongAnswer3 = "1.000.000 km/s" },
            new Question { TranslationGroupId = q18, CategoryId = scienceCategoryIds["fa"], LanguageCode = "fa", Text = "سرعت نور تقریباً چقدر است؟", CorrectAnswer = "۳۰۰٬۰۰۰ کیلومتر بر ثانیه", WrongAnswer1 = "۱۵۰٬۰۰۰ کیلومتر بر ثانیه", WrongAnswer2 = "۵۰۰٬۰۰۰ کیلومتر بر ثانیه", WrongAnswer3 = "۱٬۰۰۰٬۰۰۰ کیلومتر بر ثانیه" },
            new Question { TranslationGroupId = q18, CategoryId = scienceCategoryIds["de"], LanguageCode = "de", Text = "Wie hoch ist die Lichtgeschwindigkeit ungefähr?", CorrectAnswer = "300.000 km/s", WrongAnswer1 = "150.000 km/s", WrongAnswer2 = "500.000 km/s", WrongAnswer3 = "1.000.000 km/s" },
            new Question { TranslationGroupId = q18, CategoryId = scienceCategoryIds["uz"], LanguageCode = "uz", Text = "Yorug'lik tezligi taxminan qancha?", CorrectAnswer = "300 000 km/s", WrongAnswer1 = "150 000 km/s", WrongAnswer2 = "500 000 km/s", WrongAnswer3 = "1 000 000 km/s" },
        });

        await context.Questions.AddRangeAsync(questions);
        await context.SaveChangesAsync();
    }

    public static async Task SeedTestDataAsync(VictorinaDbContext context, bool forceReseed)
    {
        if (forceReseed)
        {
            await ClearAndReseedAsync(context);
        }
        else
        {
            await SeedTestDataAsync(context);
        }
    }

    public static async Task<bool> ClearAndReseedAsync(VictorinaDbContext context)
    {
        // Удаляем связанные данные в правильном порядке (из-за FK)
        // 1. Удаляем GameQuestions (ссылаются на Questions)
        context.GameQuestions.RemoveRange(context.GameQuestions);
        await context.SaveChangesAsync();

        // 2. Удаляем вопросы и категории
        context.Questions.RemoveRange(context.Questions);
        context.Categories.RemoveRange(context.Categories);
        await context.SaveChangesAsync();

        // Сбрасываем последовательности ID для PostgreSQL
        await context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"Categories_Id_seq\" RESTART WITH 1;");
        await context.Database.ExecuteSqlRawAsync("ALTER SEQUENCE \"Questions_Id_seq\" RESTART WITH 1;");

        // Перезаполняем
        await SeedCategoriesAndQuestionsAsync(context);
        return true;
    }
}
