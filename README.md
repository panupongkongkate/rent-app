# Comic Rental System

ระบบเช่าหนังสือการ์ตูนที่พัฒนาด้วย ASP.NET Core Web API และ Frontend ด้วย HTML/CSS/JavaScript พร้อม Tauri สำหรับ Desktop Application

## เทคโนโลยีที่ใช้

### Backend
- **ASP.NET Core 9.0** - Web API
- **Entity Framework Core** - ORM
- **SQLite** - Database
- **JWT Authentication** - การยืนยันตัวตน
- **BCrypt** - การเข้ารหัสรหัสผ่าน
- **Manual Object Mapping** - การ map object แบบ manual
- **ClosedXML** - Excel export

### Frontend
- **HTML5/CSS3/JavaScript** - Web Frontend
- **Tauri** - Desktop Application
- **Live Server** - Development server

## ข้อกำหนดของระบบ

### Software ที่ต้องติดตั้ง
- **Node.js** (v18 หรือสูงกว่า)
- **.NET 9.0 SDK**
- **Python 3.x** (สำหรับ seeding database)
- **Git**

### สำหรับ Tauri (Desktop App)
- **Rust** (latest stable)
- **System dependencies** (ตามระบบปฏิบัติการ)

## การติดตั้ง

### 1. Clone Repository
```bash
git clone <repository-url>
cd rent-app
```

### 2. ติดตั้ง Dependencies

#### Frontend Dependencies
```bash
npm install
```

#### Backend Dependencies
```bash
cd ComicRental
dotnet restore
```

### 3. ตั้งค่า Database

#### สร้าง Database และ Migration
```bash
cd ComicRental
dotnet ef database update
```

#### เพิ่มข้อมูลตัวอย่าง (Optional)
```bash
cd ..
python3 seed_database.py
```

### 4. ตั้งค่า Configuration

#### Backend Configuration
แก้ไขไฟล์ `ComicRental/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=comic_rental.db"
  },
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "ComicRental",
    "Audience": "ComicRental"
  }
}
```

## การรัน Application

### Development Mode

#### 1. รัน Backend API (Terminal 1)
```bash
npm run api
# หรือ
cd ComicRental && dotnet run
```
API จะทำงานที่: `http://localhost:5081`

#### 2. รัน Frontend 

**สำหรับ Web Browser (Terminal 2):**
```bash
npm run dev
```
Frontend จะทำงานที่: `http://localhost:3000`

**สำหรับ Desktop App (Terminal 3):**
```bash
# ต้องรัน npm run dev ก่อน!
npm run tauri dev
```

#### ลำดับการรัน Development แบบเต็ม:
1. **Terminal 1**: `npm run api` (Backend API)
2. **Terminal 2**: `npm run dev` (Frontend Web Server) 
3. **Terminal 3**: `npm run tauri dev` (Desktop App)

หมายเหตุ: Tauri จะเชื่อมต่อไปที่ `http://localhost:3000` ดังนั้นต้องรัน `npm run dev` ก่อนเสมอ

### Production Mode

#### Build Backend
```bash
cd ComicRental
dotnet publish -c Release -o publish
```

#### Build Tauri Desktop App
```bash
npm run tauri build
```

## โครงสร้างโปรเจค

```
rent-app/
├── ComicRental/           # Backend ASP.NET Core
│   ├── Controllers/       # API Controllers
│   ├── Models/           # Data Models & DTOs
│   ├── Data/             # Database Context
│   ├── Migrations/       # EF Migrations
│   └── wwwroot/         # Static files
├── admin/               # Admin Interface
├── staff/               # Staff Interface
├── js/                  # Frontend JavaScript
├── src-tauri/           # Tauri Desktop App
├── HashGenerator/       # Password hash utility
├── PasswordFix/         # Password fix utility
└── UpdateDB/           # Database update utility

```

## การใช้งาน

### ผู้ใช้ระบบ
1. **Admin** - จัดการระบบทั้งหมด
2. **Staff** - จัดการการเช่าและลูกค้า
3. **Customer** - ค้นหาและเช่าหนังสือ

### หน้าจอหลัก
- **Dashboard** - ภาพรวมของระบบ
- **Books Management** - จัดการหนังสือ
- **Customers Management** - จัดการลูกค้า
- **Rentals Management** - จัดการการเช่า
- **Reports** - รายงานต่างๆ
- **Settings** - ตั้งค่าระบบ

## API Documentation

เมื่อรัน Backend แล้ว สามารถเข้าถึง Swagger UI ได้ที่:
```
http://localhost:5081/swagger
```

## Database Schema

### หลักๆ Tables
- **Books** - ข้อมูลหนังสือ
- **Customers** - ข้อมูลลูกค้า
- **Employees** - ข้อมูลพนักงาน
- **Rentals** - ข้อมูลการเช่า
- **Categories** - หมวดหมู่หนังสือ
- **Fines** - ค่าปรับ
- **Settings** - ตั้งค่าระบบ

## Utilities

### Password Management
```bash
# Generate password hash
cd HashGenerator && dotnet run

# Fix passwords in database
cd PasswordFix && dotnet run
```

### Database Management
```bash
# Update database schema
cd UpdateDB && dotnet run
```

## การ Deploy

### 1. Web Deployment

#### Build Backend for Production
```bash
cd ComicRental
dotnet publish -c Release -o publish
```

#### IIS Deployment (Windows Server)

1. **ติดตั้ง Prerequisites**:
   - IIS พร้อม ASP.NET Core Module
   - .NET 9.0 Hosting Bundle
   ```powershell
   # Download และติดตั้งจาก Microsoft
   # https://dotnet.microsoft.com/download/dotnet/9.0
   ```

2. **เตรียม Application Pool**:
   - สร้าง Application Pool ใหม่
   - ตั้งค่า .NET CLR Version เป็น "No Managed Code"
   - ตั้งค่า Process Model Identity ตามความเหมาะสม

3. **Deploy Backend API**:
   - Copy ไฟล์ทั้งหมดจาก `ComicRental/publish/` ไป `C:\inetpub\wwwroot\comic-rental-api\`
   - สร้าง Website ใหม่ใน IIS Manager สำหรับ API
   - ชี้ไปที่ folder ที่ copy ไว้
   - ตั้งค่า Application Pool ที่สร้างไว้

5. **Deploy Frontend**:
   - Copy ไฟล์ HTML/CSS/JS ทั้งหมด ไป `C:\inetpub\wwwroot\comic-rental-web\`
   - รวมไฟล์: `index.html`, `search.html`, `admin/`, `staff/`, `js/`
   - สร้าง Website แยกต่างหากใน IIS Manager สำหรับ Frontend
   - แก้ไข API URL ใน `js/api.js` ให้ชี้ไปยัง API site
   ```javascript
   // ตัวอย่างใน js/api.js
   const API_BASE_URL = 'http://your-server/api';
   ```

### 2. Desktop Deployment

#### Build Desktop App
```bash
npm run tauri build
```

#### Output Files:
- **Windows**: `src-tauri/target/release/bundle/msi/`
- **macOS**: `src-tauri/target/release/bundle/dmg/`
- **Linux**: `src-tauri/target/release/bundle/deb/` หรือ `appimage/`

#### Distribution:
1. อัปโหลดไฟล์ installer ไปยัง release page
2. ผู้ใช้ดาวน์โหลดและติดตั้ง
3. Desktop app จะเชื่อมต่อไปยัง production API server

### 3. Database Deployment

#### Production Database Setup:
```bash
# สำหรับ production ควรใช้ database server จริง
# แทน SQLite file

# PostgreSQL example
export ConnectionStrings__DefaultConnection="Host=localhost;Database=comic_rental;Username=user;Password=pass"

# หรือ SQL Server
export ConnectionStrings__DefaultConnection="Server=localhost;Database=ComicRental;Trusted_Connection=true;"
```

#### Migration in Production:
```bash
cd ComicRental
dotnet ef database update --environment Production
```

### 4. Security Considerations

#### Production Checklist:
- เปลี่ยน JWT secret key
- ตั้งค่า HTTPS
- เปิดใช้งาน CORS อย่างเหมาะสม
- ตั้งค่า rate limiting
- เปิดใช้งาน logging
- สำรองข้อมูล database
- ตั้งค่า firewall

## การแก้ไขปัญหา

### ปัญหาที่พบบ่อย
1. **Database locked** - ปิด connection ที่ค้างอยู่
2. **CORS Error** - ตรวจสอบการตั้งค่า CORS ใน backend
3. **JWT Token expired** - ล็อคอินใหม่

### Logs
- Backend logs: Console output
- Frontend logs: Browser Developer Tools

## การมีส่วนร่วมในการพัฒนา

1. Fork repository
2. สร้าง feature branch
3. Commit changes
4. Push to branch
5. Create Pull Request

## License

This project is licensed under the MIT License.

## ผู้พัฒนา

- Backend: ASP.NET Core Web API
- Frontend: HTML/CSS/JavaScript
- Desktop: Tauri Framework