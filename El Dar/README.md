# El-Dar 🏠

نظام إدارة Desktop مخصوص لدار مسنين في القاهرة — بيسجل بيانات المقيمين (Guests) والموظفين (Employees)، وبيعمل جرد مالي شهري وسنوي تلقائي، وبيشتغل Offline على قاعدة بيانات محلية.

ده Demo عام للمشروع. النسخة الأصلية شغالة فعليًا في دار مسنين بالقاهرة، وبتزامن الداتا مع السحابة وبتتحدث تلقائيًا. في الـ Demo ده المزامنة والتحديث التلقائي وإرسال الإيميلات مقفولين، والداتا اللي فيه كلها وهمية.
---



📸 لقطات من السيستم
<img width="1919" height="1038" alt="image" src="https://github.com/user-attachments/assets/82183bf9-1c60-4607-8924-41fe09810bd7" />

<img width="1918" height="1040" alt="image" src="https://github.com/user-attachments/assets/67c1330d-4ca3-413f-8060-8f78541c6954" />

<img width="668" height="607" alt="image" src="https://github.com/user-attachments/assets/ba75ef2c-a49a-476f-b1cf-99bd3599fb00" />

<img width="1919" height="1031" alt="image" src="https://github.com/user-attachments/assets/8a272f9f-79cc-4679-bc37-41b698c033d8" />

<img width="1919" height="1034" alt="image" src="https://github.com/user-attachments/assets/c758495f-c2bf-440e-b995-39f6b11d6c42" />

<img width="1919" height="1037" alt="image" src="https://github.com/user-attachments/assets/004231b2-a4bb-4c2e-b27e-b1bdcf52ccc4" />

<img width="1919" height="1038" alt="image" src="https://github.com/user-attachments/assets/2154ad4e-3711-4ee1-b74c-f46c993d935f" />

<img width="1919" height="1038" alt="image" src="https://github.com/user-attachments/assets/2dd8e525-3b2c-4840-bd2e-a87068c18727" />

<img width="1919" height="1042" alt="image" src="https://github.com/user-attachments/assets/3699b323-60d2-47ab-9901-c898443ee026" />

<img width="1798" height="1038" alt="image" src="https://github.com/user-attachments/assets/b4be19b6-c60a-4893-8585-834f93766737" />

<img width="1919" height="1030" alt="image" src="https://github.com/user-attachments/assets/d329af50-383f-4a3f-aedc-f141c217d76e" />



⬇️ تجربة سريعة (Windows)
روح على صفحة Releases ونزّل ملف الـ installer (.exe).
شغّله. لو Windows طلّع تحذير "Windows protected your PC" (عشان الملف مش موقّع) دوس More info ثم Run anyway.
افتح El-Dar Demo من سطح المكتب وسجّل دخول بالبيانات دي:
UserName    Test	
Password	123456789

مفيش حاجة بتتسطب غير البرنامج نفسه، ومش محتاج إنترنت. للـ Windows بس، ولو على Mac أو Linux شغّله من الكود (تحت).





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
cd "El Dar Demo"
dotnet restore
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
