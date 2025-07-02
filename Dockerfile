# مرحلة البناء
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# انسخ كل الملفات
COPY . ./

# استخدم اسم المشروع داخل علامات اقتباس ومجلد صريح
RUN dotnet restore "./project graduation.csproj"
RUN dotnet publish "./project graduation.csproj" -c Release -o /app/out

# مرحلة التشغيل
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# شغّل التطبيق (لو اسم الـ DLL فيه مسافة لازم نستخدم bash مباشرة)
ENTRYPOINT [ "sh", "-c", "dotnet 'project graduation.dll'" ]