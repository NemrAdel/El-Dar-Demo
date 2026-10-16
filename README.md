# El-Dar 🏠

نظام إدارة Desktop مخصوص لدار مسنين في القاهرة — بيسجل بيانات المقيمين (Guests) والموظفين (Employees)، وبيعمل جرد مالي شهري وسنوي تلقائي، وبيشتغل Offline مع مزامنة (Sync) للداتا مع السحابة أول ما يبقى فيه اتصال إنترنت.

---

## ✨ الفيچرز الأساسية

- **إدارة المقيمين والموظفين**: سجلات كاملة مع بحث، فلترة، وPagination عبر ~11 صفحة.
- **Soft-delete & Sync tracking**: كل سجل بيتتبع عن طريق `IsSynced` / `IsDeleted` بدل المسح النهائي، عشان الداتا تفضل قابلة للمزامنة والاسترجاع.
- **جرد مالي شهري وسنوي**: بيسحب كل الحركات المالية من اليوم الحالي رجوعًا لأول يوم في الشهر أو السنة، ويعرضها في جداول توضح الفلوس اللي دخلت فعليًا مقابل الفلوس اللي لسه مستحقة، مع الإجمالي.
- **Offline-first مع Auto-Sync**: التطبيق بيشتغل بالكامل من غير إنترنت على قاعدة بيانات محلية (SQLite)، وبيزامن نفسه تلقائيًا مع السيرفر أول ما يلاقي اتصال.
- **Authentication**: تسجيل دخول مع إعادة تعيين كلمة المرور عبر Tokens مؤقتة بالإيميل.
- **Auto-updates**: التطبيق (Electron) بيحدّث نفسه تلقائيًا عن طريق `electron-updater` من ريبو GitHub خاص.

---

## 🛠️ Tech Stack

| الطبقة | التقنية |
|---|---|
| Backend / Web Framework | ASP.NET Core MVC |
| ORM | Entity Framework Core |
| قاعدة البيانات المحلية | SQLite |
| قاعدة البيانات السحابية | Supabase (PostgreSQL) |
| قاعدة بيانات الإنتاج | SQL Server (مستضافة على MonsterASP.NET) |
| Desktop Shell | Electron + electron-updater |

---

## 🏗️ الـ Architecture

المشروع مبني بنظام 3 طبقات (3-Layer Architecture) عشان يفصل المسؤوليات:

```
El Dar.sln
├── ElDarDal/          (Data Access Layer)
│   ├── Context/        — DbContext
│   ├── Migrations/
│   ├── Models/          — Entities
│   ├── Repositories/    — Repository Pattern
│   └── UnitOfWork/      — Unit of Work Pattern
│
├── ElDarBLL/          (Business Logic Layer)
│   ├── BusinessServices/
│   ├── Helpers/
│   ├── Mapping/          — AutoMapper Profiles
│   └── ViewModel/
│
└── El Dar/ (ElDarPI)   (Presentation + Desktop Shell)
    ├── Controllers/
    ├── Views/
    ├── wwwroot/
    ├── DatabaseSyncWorker.cs   — Background sync service
    ├── ElDar.db                — Local SQLite (dev)
    ├── Program.cs
    └── Main.js / index.html / package.json  — Electron wrapper
```

---

## 🔄 التحدي الأكبر: مزامنة الداتا بين 3 قواعد بيانات

أكبر تحدي فني في المشروع كان بناء **`DatabaseSyncWorker`** — خدمة Background بتزامن الداتا بين قاعدة بيانات محلية (SQLite) وقاعدتين سحابيتين (Supabase وSQL Server) بشكل ثنائي الاتجاه (Push/Pull)، مع الحفاظ على تناسق البيانات.

أهم المشاكل اللي اتحلت أثناء بناء النظام ده:

- **فروق التوقيت (Timezone)**: Supabase Postgrest بيرجّع `DateTime.Kind = Unspecified`، فكان لازم استخدام `DateTime.SpecifyKind(..., Utc)` بدل `.ToUniversalTime()` عشان نتجنب حسابات وقت غلط.
- **Race condition في الـ Self-pull**: السيستم كان أحيانًا "بيسحب" نفس الداتا اللي هو نفسه بعتها لتوه — اتحل بتتبع الـ IDs المبعوتة في HashSet (`justPushedIds`) وفلترتها من نتائج الـ Pull.
- **أخطاء صامتة**: try/catch واحد مشترك بين كل عمليات المزامنة كان بيبلّع الأخطاء ويخليها تفوت من غير ما حد يلاحظ — اتحل بإدارة كل Entity بمنطق sync منفصل وTry/catch مستقل.
- **تعارض الـ Seed Data**: كل بيئة (VS محلي، Electron AppData، Production) كانت بتعمل Database Seed مستقل بأول تشغيل، فكان بيتولّد داتا عشوائية متضاربة بين البيئات المختلفة.

---

## 🚀 التشغيل محليًا

```bash
git clone <repo-url>
cd "El Dar"
dotnet restore
dotnet ef database update --project ElDarDal
dotnet run --project "El Dar"
```

للنسخة Desktop (Electron):

```bash
cd "El Dar"
npm install
npm run start
```

---

## 📌 ملاحظة

المشروع ما زال قيد التطوير المستمر — في تحسينات جارية على منطق الـ Sync خصوصًا في التعامل مع تعارضات الـ Seed Data بين البيئات المختلفة.
