<div align="center">

# Operations Platform

**פלטפורמה להפעלת פעולות על שני פרמטרים (שדה א' ושדה ב')**

`.NET 10` · `C#` · `SQL Server` · `EF Core` · `Angular`

[הדגמה](#הדגמה) · [הרצה](#הרצה) · [API](#ה-api) · [הוספת פעולה](#איך-מוסיפים-פעולה-בלי-לפתח) · [לוגים](#לוגים) · [אבטחה](#אבטחה-והגבלות)

</div>

<div dir="rtl" align="right">

## הדגמה

[![חישוב והיסטוריה](docs/2-result-and-history.png)](docs/demo.webm)

- **סרטון איטי של כל 10 הפעולות** (לחיצה על התמונה פותחת את הקובץ המקומי): [`docs/demo.webm`](docs/demo.webm)
- **אותו סרטון כדף אינטרנט:** [Operations Platform Demo](https://claude.ai/artifact/7nCWbtnpCtzCuZgx2kuv7p). הדף פרטי, ולכן נפתח רק למי ששיתפו איתו אותו.

| חישוב והיסטוריה | מזג אוויר | שגיאה |
|:---:|:---:|:---:|
| ![חישוב](docs/2-result-and-history.png) | ![מזג אוויר](docs/3-weather.png) | ![שגיאה](docs/4-error.png) |

## מה יש כאן

| דרישה | מימוש |
|---|---|
| פעולות על שדה א' ושדה ב' | 10 פעולות: חשבון, מחרוזות, ותחזית מזג אוויר לפי קו רוחב ואורך |
| הוספה, שינוי והסרה בלי פיתוח | `POST/PUT/DELETE /api/operations`: נוסחה, פונקציית טקסט או קריאה ל-API חיצוני |
| API וממשק משתמש | Controllers ב-.NET, ממשק Angular |
| היסטוריה במסד נתונים | כל חישוב נשמר ב-`OperationLogs`, כולל כאלה שנכשלו |
| 3 ריצות אחרונות וספירה מתחילת החודש | לפי שעון ישראל, נשלפות בשאילתה אחת עם אינדקס |
| לוג שגיאות ולוג כל הבקשות והתשובות | Serilog, מוגדר כולו ב-`appsettings.json` |

## הרצה

דרישות: .NET SDK 10, Node 22, SQL Server Express (instance בשם `SQLEXPRESS`).

```bash
cd frontend
npm install
npm run build        # בונה את הממשק לתוך backend/OperationsPlatform.Web/wwwroot

cd ../backend/OperationsPlatform.Web
dotnet run
```

בהפעלה הראשונה השרת יוצר את בסיס הנתונים `OperationsPlatformDb` לבד מתוך ה-migrations (Code First) ומכניס את הפעולות ההתחלתיות.
הפורט מודפס בקונסול. הממשק בשורש `/`, התיעוד של ה-API ב-`/swagger` (במצב Development).

- לפיתוח עם רענון חי: `npx ng serve` בתיקיית `frontend` (פורט 4200, מפנה את `/api` אל `http://localhost:5177`).
- מחרוזת חיבור אחרת: משנים ב-`appsettings.json` או במשתנה סביבה `ConnectionStrings__Default`.

## ה-API

| Method | נתיב | תיאור |
|---|---|---|
| `GET` | `/api/operations` | רשימת כל הפעולות הפעילות |
| `POST` | `/api/calculate` | `{ "a": "3", "b": "4", "operation": "hypotenuse" }` מחזיר תוצאה, 3 ביצועים אחרונים מאותו סוג וכמות הביצועים מתחילת החודש |
| `POST` `PUT` `DELETE` | `/api/operations` | הוספה, שינוי והסרה של פעולה, בלי פיתוח נוסף (דורש `X-Admin-Key`) |

| סטטוס | מתי |
|:---:|---|
| `400` | קלט לא תקין, נוסחה שגויה, תו אסור |
| `401` | חסר מפתח ניהול |
| `404` / `409` | פעולה לא נמצאה / מפתח פעולה כפול |
| `413` | גוף בקשה גדול מ-64KB |
| `502` | שירות חיצוני נכשל |
| `500` | תקלה לא צפויה, נרשמת ב-`errors-.log` |

"תחילת החודש" היא חצות של ה-1 בחודש **בשעון ישראל** (`Asia/Jerusalem`), בכל שרת. השעון נקבע במחלקה אחת, `IsraelTimeProvider`.

## איך מוסיפים פעולה בלי לפתח

דרך ה-API (`POST /api/operations`) מגדירים פעולה חדשה. שלושה סוגים:

| סוג | מה זה | דוגמה |
|---|---|---|
| **Expression** | נוסחה על `a` ו-`b`, מחושבת עם NCalc: מפרש לביטויים מתמטיים בלבד, בלי הרצת קוד | `a * b + 10` |
| **Text** | פונקציית טקסט מובנית | `concat`, `reverse`, `levenshtein`, `anagram` |
| **Http** | קריאה ל-API חיצוני לפי תבנית | תחזית מזג אוויר, למטה |

```json
{"url":"https://api.open-meteo.com/v1/forecast?latitude={a}&longitude={b}&current=temperature_2m","path":"current.temperature_2m"}
```

סוג חדש לגמרי: מחלקה אחת שמממשת `IOperationExecutor` ושורת רישום ב-`CoreServiceExtensions.cs`.

## מבנה

ארבע שכבות: `Web → Core → Domain`, ו-`Common` שכולם משתמשים בו.

| שכבה | תפקיד |
|---|---|
| `Web` | controllers, middlewares, טיפול בשגיאות, הגדרות Kestrel ולוגים |
| `Core` | שירותים (כל אחד בתיקייה משלו עם הממשק שלו), validators, מנועי הפעולות ב-`Executors/`, גישה לנתונים ב-EF Core, והממשקים של ה-repositories ב-`Abstractions/` |
| `Domain` | הישויות וה-enums |
| `Common` | קבועים וחריגות משותפות |
| `frontend` | ממשק Angular. ה-build שלו נכתב לתוך `wwwroot` של ה-Web |

יצירת migration חדשה (מתוך `backend/`):

```bash
dotnet ef migrations add <Name> -p OperationsPlatform.Core -s OperationsPlatform.Web -o Migrations
```

## מה מובנה ב-ASP.NET ומה כתוב ידנית

| נושא | פתרון |
|---|---|
| טיפול בשגיאות | `IExceptionHandler` + `UseExceptionHandler` (מחלקה אחת: `GlobalExceptionHandler`). אין `try/catch` בקוד חוץ מ-`InputValidationMiddleware`; ב-`CalculationService` יש `try/finally` ששומר את הריצה גם כשהיא נכשלת |
| לוג בקשות ותשובות | `AddHttpLogging` / `UseHttpLogging`: ערך אחד לכל בקשה, עם גוף הבקשה והתשובה, סטטוס וזמן |
| ולידציה | FluentValidation, נקראת מהשירותים. כישלון הופך ל-400 עם `errors` לפי שדה |
| מפתח ניהול | `AdminKeyMiddleware`, מחלקה אחת |
| רשימת תווים מותרים | `InputValidationMiddleware` (regex אחד) |

## לוגים

בתיקייה `logs/`, מתגלגלים יומית. ההגדרה כולה בקבוצת `Serilog` ב-`appsettings.json`: יעדים, רמות והסינון של כל קובץ.

| קובץ | תוכן |
|---|---|
| `errors-YYYYMMDD.log` | שגיאות שרת (500 ו-502: exceptions לא צפויים וכשלי שירות חיצוני, עם stack trace). טעויות של משתמש (400, 404, 409) לא נרשמות כאן |
| `http-YYYYMMDD.log` | כל request ו-response (גוף, סטטוס וזמן), ובנוסף שורת `[WRN]` לכל בקשה שנחסמה בגלל תו אסור |

כל שורה נושאת את מזהה הבקשה (`RequestId`), כך ששורה ב-`errors-.log` מתחברת לבקשה ב-`http-.log`.

## היסטוריה

כל חישוב שהגיע לפעולה נשמר בטבלת `OperationLogs` (קלט, תוצאה, משך, זמן, והעמודה `Succeeded`), גם כשנכשל (קלט לא תקין לפעולה, חילוק באפס, שירות חיצוני שנפל).

- "3 האחרונות" והספירה החודשית סופרים רק חישובים שהצליחו.
- שגיאת ולידציה או פעולה שלא קיימת לא נשמרות, כי אין עדיין פעולה לשייך אליה.
- האינדקס `(OperationId, CreatedAtUtc)` משרת גם את "3 האחרונות" וגם את הספירה החודשית.

## אבטחה והגבלות

- שינוי פעולות (`POST/PUT/DELETE` תחת `/api/operations`) וגם `GET /api/operations?includeInactive=true` דורשים כותרת `X-Admin-Key` שזהה ל-`Admin:Key`. **אין מפתח ברירת מחדל**: ב-`appsettings.json` הוא ריק, ואז כל שינוי נדחה. חישוב וקריאה פתוחים.
  - בפיתוח מגדירים אותו ב-user-secrets (לא נשמר ב-git):
    ```bash
    dotnet user-secrets set "Admin:Key" "<מפתח>" -p backend/OperationsPlatform.Web
    ```
  - בהרצה אמיתית מגדירים אותו במשתנה סביבה `Admin__Key`.
- פעולות Http יכולות לפנות רק לדומיינים ב-`AllowedHttpHosts` (ברירת מחדל: `api.open-meteo.com`), רק ב-https וללא redirects. זה מונע מהשרת לפנות לכתובות פנימיות.
- גוף בקשה גדול מ-64KB מחזיר 413. המגבלה היא של Kestrel (`MaxRequestBodySize`), והבקשה לא מגיעה לקוד שלנו.

## בדיקת קלט

`InputValidationMiddleware` בודק את גוף כל POST/PUT ל-`/api` לפני ה-controller. כל מחרוזת ב-JSON (שמות שדות וערכים, בכל עומק) חייבת לעבור **regex אחד** (קבוע בתוך ה-middleware): אותיות מכל שפה (כולל עברית), ספרות, פיסוק, רווח, טאב ושורה חדשה, והסימנים `+ = ^ | ~ $ °`. כל מה שלא ברשימה נחסם, ובכלל זה `<` ו-`>` (ולכן אי אפשר לכתוב תג HTML), תווי בקרה, עקיפת כיוון כתיבה ואימוג'י.

מי ששולח תו אסור מקבל **400 פשוט, בלי הסבר**. השדה נרשם ב-`http-.log`:

```
Rejected POST /api/calculate: field 'body.a' has characters that are not allowed
```

את כללי השדות (אורך, פורמט) בודק FluentValidation בתוך השירותים. ההודעות שלו מיועדות לקורא ולכן חוזרות יחד, לפי שדה: `{"errors":{"key":["..."],"name":["..."]}}`.

**מגבלות ידועות (בכוונה):**

- נוסחה לא יכולה להשתמש ב-`<` או `>`. במקום זה: `Max`, `Min`, `Sign`, `Abs`.
- הטקסט `javascript:` עובר, כי הוא לא מזיק עד שמישהו שם אותו בקישור, והפלטפורמה לא עושה את זה.
- הבדיקות העסקיות (מפתח כפול, נוסחה שלא עובדת, דומיין לא מורשה) נשארות בשירותים, כי הן צריכות מסד נתונים או את מנוע הפעולה.
- אין הגבלת קצב, ולכן אפשר לנסות לנחש את `X-Admin-Key`. המפתח חייב להיות ארוך ואקראי (32 תווים ומעלה), ואין לחשוף את השרת לאינטרנט בלי שכבת הגנה לפניו.
- SQL injection לא נסרק כאן: כל שאילתה עוברת דרך EF Core עם פרמטרים, וזו ההגנה האמיתית.

## שימוש ב-AI

התרגיל התיר שימוש ב-AI. הקוד נכתב בעזרת Claude (Anthropic) ונבדק, והמגישים אחראים להבין ולהסביר כל חלק בו.

</div>
