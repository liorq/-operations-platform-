# Operations Platform

פלטפורמה להפעלת פעולות על שני פרמטרים (שדה א', שדה ב'). השרת ב-.NET 10 (C#), בסיס נתונים SQL Server Express (EF Core Code First), ממשק משתמש ב-Angular.

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

לפיתוח עם רענון חי: `npx ng serve` בתיקיית `frontend` (פורט 4200, מפנה את `/api` אל `http://localhost:5177`).
מחרוזת חיבור אחרת: משנים ב-`appsettings.json` או במשתנה סביבה `ConnectionStrings__Default`.

## ה-API

| Method | נתיב | תיאור |
|---|---|---|
| GET | `/api/operations` | רשימת כל הפעולות הפעילות |
| POST | `/api/calculate` | `{ "a": "3", "b": "4", "operation": "hypotenuse" }` מחזיר תוצאה, 3 ביצועים אחרונים מאותו סוג וכמות הביצועים מתחילת החודש |
| POST / PUT / DELETE | `/api/operations` | הוספה, שינוי והסרה של פעולה, בלי פיתוח נוסף (דורש `X-Admin-Key`) |

"תחילת החודש" היא חצות של ה-1 בחודש **בשעון ישראל** (`Asia/Jerusalem`), בכל שרת. השעון נקבע במחלקה אחת, `IsraelTimeProvider`.

## איך מוסיפים פעולה בלי לפתח

דרך ה-API (`POST /api/operations`) מגדירים פעולה חדשה. שלושה סוגים:

- **Expression**: נוסחה על `a` ו-`b`, למשל `a * b + 10`. מחושבת עם NCalc, מפרש לביטויים מתמטיים בלבד, בלי הרצת קוד.
- **Text**: פונקציית טקסט מובנית (`concat`, `reverse`, `levenshtein`, `anagram`).
- **Http**: קריאה ל-API חיצוני לפי תבנית. כך ממומשת תחזית מזג אוויר לפי קו רוחב ואורך:
  `{"url":"https://api.open-meteo.com/v1/forecast?latitude={a}&longitude={b}&current=temperature_2m","path":"current.temperature_2m"}`

סוג חדש לגמרי: מחלקה אחת שמממשת `IOperationExecutor` ושורת רישום ב-`CoreServiceExtensions.cs`.

## מבנה

ארבע שכבות: `Web → Core → Domain`, ו-`Common` שכולם משתמשים בו. ה-Core מכיל את השירותים, ה-validators, מנועי הפעולות (`Executors/`) והגישה לנתונים (EF Core). הממשקים של ה-repositories ב-`Abstractions/`. כל שירות בתיקייה משלו, עם הממשק שלו. הממשק ב-`frontend/`, וה-build שלו נכתב לתוך `wwwroot` של ה-Web.

יצירת migration חדשה (מתוך `backend/`): `dotnet ef migrations add <Name> -p OperationsPlatform.Core -s OperationsPlatform.Web -o Migrations`

## מה מובנה ב-ASP.NET ומה כתוב ידנית

| נושא | פתרון |
|---|---|
| טיפול בשגיאות | `IExceptionHandler` + `UseExceptionHandler` (מחלקה אחת: `GlobalExceptionHandler`). אין `try/catch` בקוד חוץ מ-`InputValidationMiddleware` |
| לוג בקשות ותשובות | `AddHttpLogging` / `UseHttpLogging`: ערך אחד לכל בקשה, עם גוף הבקשה והתשובה, סטטוס וזמן |
| ולידציה | FluentValidation, נקראת מהשירותים. כישלון הופך ל-400 עם `errors` לפי שדה |
| מפתח ניהול | `AdminKeyMiddleware`, מחלקה אחת |
| רשימת תווים מותרים | `InputValidationMiddleware` (regex אחד) |

## לוגים (בתיקייה `logs/`, מתגלגלים יומית)

ההגדרה כולה בקבוצת `Serilog` ב-`appsettings.json`: יעדים, רמות והסינון של כל קובץ.

- `errors-YYYYMMDD.log`: שגיאות שרת (500 ו-502: exceptions לא צפויים וכשלי שירות חיצוני, עם stack trace). טעויות של משתמש (400, 404, 409) לא נרשמות כאן.
- `http-YYYYMMDD.log`: כל request ו-response (גוף, סטטוס וזמן), ובנוסף שורת `[WRN]` לכל בקשה שנחסמה בגלל תו אסור.

כל שורה נושאת את מזהה הבקשה (`RequestId`), כך ששורה ב-`errors-.log` מתחברת לבקשה ב-`http-.log`.

## היסטוריה

כל חישוב שהגיע לפעולה נשמר בטבלת `OperationLogs` (קלט, תוצאה, משך, זמן, והעמודה `Succeeded`), גם כשנכשל (קלט לא תקין לפעולה, חילוק באפס, שירות חיצוני שנפל). "3 האחרונות" והספירה החודשית סופרים רק חישובים שהצליחו. שגיאת ולידציה או פעולה שלא קיימת לא נשמרות, כי אין עדיין פעולה לשייך אליה. האינדקס `(OperationId, CreatedAtUtc)` משרת גם את "3 האחרונות" וגם את הספירה החודשית.

## אבטחה והגבלות

- שינוי פעולות (`POST/PUT/DELETE` תחת `/api/operations`) וגם `GET /api/operations?includeInactive=true` דורשים כותרת `X-Admin-Key` שזהה ל-`Admin:Key`. **אין מפתח ברירת מחדל**: ב-`appsettings.json` הוא ריק, ואז כל שינוי נדחה. בפיתוח מגדירים אותו ב-user-secrets (לא נשמר ב-git): `dotnet user-secrets set "Admin:Key" "<מפתח>" -p backend/OperationsPlatform.Web`. בהרצה אמיתית מגדירים אותו במשתנה סביבה `Admin__Key`. חישוב וקריאה פתוחים.
- פעולות Http יכולות לפנות רק לדומיינים ב-`AllowedHttpHosts` (ברירת מחדל: `api.open-meteo.com`), רק ב-https וללא redirects. זה מונע מהשרת לפנות לכתובות פנימיות.
- גוף בקשה גדול מ-64KB מחזיר 413. המגבלה היא של Kestrel (`MaxRequestBodySize`), והבקשה לא מגיעה לקוד שלנו.

## בדיקת קלט

`InputValidationMiddleware` בודק את גוף כל POST/PUT ל-`/api` לפני ה-controller. כל מחרוזת ב-JSON (שמות שדות וערכים, בכל עומק) חייבת לעבור **regex אחד** (קבוע בתוך ה-middleware): אותיות מכל שפה (כולל עברית), ספרות, פיסוק, רווח, טאב ושורה חדשה, והסימנים `+ = ^ | ~ $ °`. כל מה שלא ברשימה נחסם, ובכלל זה `<` ו-`>` (ולכן אי אפשר לכתוב תג HTML), תווי בקרה, עקיפת כיוון כתיבה ואימוג'י.

מי ששולח תו אסור מקבל **400 פשוט, בלי הסבר**. השדה נרשם ב-`http-.log`:
`Rejected POST /api/calculate: field 'body.a' has characters that are not allowed`

את כללי השדות (אורך, פורמט) בודק FluentValidation בתוך השירותים. ההודעות שלו מיועדות לקורא ולכן חוזרות יחד, לפי שדה: `{"errors":{"key":["..."],"name":["..."]}}`.

**מגבלות ידועות (בכוונה):**
- נוסחה לא יכולה להשתמש ב-`<` או `>`. במקום זה: `Max`, `Min`, `Sign`, `Abs`.
- הטקסט `javascript:` עובר, כי הוא לא מזיק עד שמישהו שם אותו בקישור, והפלטפורמה לא עושה את זה.
- הבדיקות העסקיות (מפתח כפול, נוסחה שלא עובדת, דומיין לא מורשה) נשארות בשירותים, כי הן צריכות מסד נתונים או את מנוע הפעולה.
- אין הגבלת קצב, ולכן אפשר לנסות לנחש את `X-Admin-Key`. המפתח חייב להיות ארוך ואקראי (32 תווים ומעלה), ואין לחשוף את השרת לאינטרנט בלי שכבת הגנה לפניו.
- SQL injection לא נסרק כאן: כל שאילתה עוברת דרך EF Core עם פרמטרים, וזו ההגנה האמיתית.

## צילומי מסך

| חישוב והיסטוריה | מזג אוויר | שגיאה |
|---|---|---|
| ![חישוב](docs/2-result-and-history.png) | ![מזג אוויר](docs/3-weather.png) | ![שגיאה](docs/4-error.png) |

וידאו קצר של השימוש: [docs/demo.webm](docs/demo.webm).

## שימוש ב-AI

התרגיל התיר שימוש ב-AI. הקוד נכתב בעזרת Claude (Anthropic) ונבדק, והמגישים אחראים להבין ולהסביר כל חלק בו.
